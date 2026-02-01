using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// literally main script of audioplayer

namespace FFmpeg
{
	#region FFmpeg Player
	[GlobalClass]
	public partial class FFmpegPlayer : Node // named so because otherwise Godot would not see it
	{
		public const int SAMPLE_RATE = FFmpeg.SAMPLE_RATE;
		public const int CHANNELS = FFmpeg.CHANNELS;
		public const int FRAMES = FFmpeg.FRAMES;

		private AudioStreamGeneratorPlayback _playback;
		private Process _ffmpeg;
		private CancellationTokenSource _cts;

		private AudioStreamPlayer _player;

		private Queue<byte[]> _pcmQueue = new();
		private object _lock = new();

		private bool _playing = false;

		public bool Playing
		{
			get => _playing;
		}

		[Export]
		public bool Loop = false; // only for local files
		[Export]
		public int TargetBus = 0;

		public override void _Ready()
		{
			_player = new() { Bus = AudioServer.GetBusName(TargetBus) };
			AddChild(_player);

			AudioStreamGenerator gen = new()
			{
				MixRate = SAMPLE_RATE,
				BufferLength = 3.0f
			};

			_player.Stream = gen;
		}

		public override void _Process(double delta)
		{
			if (_playback == null || !_playback.CanPushBuffer(FRAMES))
				return;

			byte[] data = null;

			lock (_lock)
			{
				if (_pcmQueue.Count > 0)
					data = _pcmQueue.Dequeue();
			}

			if (data == null)
				return;

			int floatCount = data.Length / 4;
			float[] samples = new float[floatCount];
			Buffer.BlockCopy(data, 0, samples, 0, data.Length);

			PushSamples(_playback, samples);
		}

		public void Play(string pathOrUrl)
		{
			Stop();
			_player.Play();
			_playback = (AudioStreamGeneratorPlayback)_player.GetStreamPlayback();

			_cts = new CancellationTokenSource();

			string input = pathOrUrl.StartsWith("user://") // res:// not supported because of ffmpeg, need to extract it before use
				? ProjectSettings.GlobalizePath(pathOrUrl)
				: pathOrUrl;

			string[] additionalArgs = [];
			if (Loop)
			{
				additionalArgs = ["-stream_loop", "-1"];
			}

			_ffmpeg = StartFFmpeg(input, FFmpeg.IsUrl(pathOrUrl), additionalArgs);

			_ = Task.Run(() => ReadPcmLoop(_cts.Token));
			_playing = true;
		}

		public void Stop()
		{
			_cts?.Cancel();
			_cts = null;

			try
			{
				if (_ffmpeg != null && !_ffmpeg.HasExited)
					_ffmpeg.Kill();
			}
			catch { }

			_ffmpeg = null;

			lock (_lock)
				_pcmQueue.Clear();
			_player.Stop();
			_playing = false;
		}


		private static Process StartFFmpeg(string input, bool IsURL, string[] additionalArgs)
		{
			Godot.Collections.Array<string> ResultArgs = [];
			if (IsURL)
			{
				ResultArgs.Add("-fflags");
				ResultArgs.Add("nobuffer");
				ResultArgs.Add("-flags");
				ResultArgs.Add("low_delay");
				ResultArgs.Add("-probesize");
				ResultArgs.Add("32");
				ResultArgs.Add("-analyzeduration");
				ResultArgs.Add("0");
			}
			else
				ResultArgs.Add("-re");
			foreach (string arg in additionalArgs)
				ResultArgs.Add(arg);
			ResultArgs.Add("-i");
			ResultArgs.Add(input);
			ResultArgs.Add("-f");
			ResultArgs.Add("f32le");
			ResultArgs.Add("-ac");
			ResultArgs.Add(CHANNELS.ToString());
			ResultArgs.Add("-ar");
			ResultArgs.Add(SAMPLE_RATE.ToString());
			ResultArgs.Add("pipe:1");

			return FFmpeg.Start([.. ResultArgs]);
		}

		async Task ReadPcmLoop(CancellationToken token)
		{
			Stream stdout = _ffmpeg.StandardOutput.BaseStream;
			byte[] buffer = new byte[FRAMES * CHANNELS * 4];

			try
			{
				while (!token.IsCancellationRequested)
				{
					int read = await stdout.ReadAsync(buffer, 0, buffer.Length, token);

					if (read == 0)
					{
						await Task.Delay(10, token);
						continue;
					}

					byte[] chunk = new byte[read];
					Buffer.BlockCopy(buffer, 0, chunk, 0, read);

					lock (_lock)
						_pcmQueue.Enqueue(chunk);
				}
			}
			catch (OperationCanceledException) { }
		}


		private static void PushSamples(AudioStreamGeneratorPlayback playback, float[] samples)
		{
			for (int i = 0; i < samples.Length - 1; i += 2)
			{
				if (playback.GetFramesAvailable() > 0)
				{
					playback.PushFrame(new Vector2(samples[i], samples[i + 1]));
				}
			}
		}
	}
	#endregion

	// --- FFmpeg static classes for other scripts --------------------------------------------------------------------------

	#region FFmpeg
	public static class FFmpeg
	{
		public const int SAMPLE_RATE = 48000;
		public const int CHANNELS = 2;
		public const int FRAMES = 4096;

		public static string FFmpegPath
		{
			get
			{
				string userDir = OS.GetUserDataDir();
				string ffmpegPath = Path.Combine(userDir, "ffmpeg.exe");

				if (File.Exists(ffmpegPath))
					return ffmpegPath;

				Extract();

				return ffmpegPath;
			}
		}

		public static Process Start(string[] arguments)
		{
			ProcessStartInfo psi = new()
			{
				FileName = FFmpegPath,
				RedirectStandardOutput = true,
				RedirectStandardError = false,
				UseShellExecute = false,
				CreateNoWindow = !OS.GetCmdlineArgs().Contains("--ffmpeg_debug")
			};

			foreach (string arg in arguments)
				psi.ArgumentList.Add(arg);

			Process p = new() { StartInfo = psi };
			p.Start();
			return p;
		}

		public static void Extract()
		{
			string userDir = OS.GetUserDataDir();
			string ffmpegPath = Path.Combine(userDir, "ffmpeg.exe");
			if (!File.Exists(ffmpegPath))
			{
				GD.Print($"Extracting ffmpeg into \"{userDir}\"...");
				foreach (string binable in DirAccess.GetFilesAt("res://bin"))
				{
					using var file = Godot.FileAccess.Open(Path.Combine("res://bin", binable), Godot.FileAccess.ModeFlags.Read);
					using var outFile = File.OpenWrite(Path.Combine(userDir, binable));

					byte[] buffer = file.GetBuffer((long)file.GetLength());
					outFile.Write(buffer, 0, buffer.Length);
				}

				GD.Print("Succsess");
			}
			else
				GD.Print("FFmpeg already extracted, continuing");
		}

		public static bool IsUrl(string path)
		{
			return Uri.TryCreate(path, UriKind.Absolute, out var uri)
				   && (uri.Scheme == Uri.UriSchemeHttp
					   || uri.Scheme == Uri.UriSchemeHttps);
		}
	}

	#endregion

	#region FFprobe

	namespace FFprobe // because we need metadata
	{
		public static class FFprobe
		{
			public static string FFprobePath
			{
				get
				{
					string userDir = OS.GetUserDataDir();
					string ffprobePath = Path.Combine(userDir, "ffprobe.exe");

					if (File.Exists(ffprobePath))
						return ffprobePath;

					FFmpeg.Extract();

					return ffprobePath;
				}
			}

			public static string GetRawMetadata(string filePath) // returns as JSON
			{
				if (Godot.FileAccess.FileExists(filePath))
				{
					Godot.Collections.Array<string> ResultArgs = [];

					ResultArgs.Add("-v");
					ResultArgs.Add("error");
					ResultArgs.Add("-print_format");
					ResultArgs.Add("json");
					ResultArgs.Add("-show_format");
					//ResultArgs.Add("-show_streams");
					ResultArgs.Add(filePath);

					Process probe = Start([.. ResultArgs]);
					string result = probe.StandardOutput.ReadToEnd();
					probe.WaitForExit();

					return result;
				}
				return ":3";
			}

			public static Process Start(string[] arguments)
			{
				ProcessStartInfo psi = new()
				{
					FileName = FFprobePath,
					RedirectStandardOutput = true,
					RedirectStandardError = false,
					UseShellExecute = false,
					CreateNoWindow = !OS.GetCmdlineArgs().Contains("--ffmpeg_debug")
				};

				foreach (string arg in arguments)
					psi.ArgumentList.Add(arg);

				Process p = new() { StartInfo = psi };
				p.Start();
				return p;
			}
		}
	}
	#endregion
}
