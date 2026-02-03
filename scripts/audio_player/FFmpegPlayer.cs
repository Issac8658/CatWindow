using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

// literally main script of audioplayer

namespace FFmpeg
{
	#region FFmpeg Player
	[GlobalClass]
	public partial class FFmpegPlayer : Node // named so because otherwise Godot would not see it
	{
		[Signal] public delegate void PlayedEventHandler();
		[Signal] public delegate void StoppedEventHandler();

		public const int SAMPLE_RATE = FFmpeg.SAMPLE_RATE;
		public const int CHANNELS = FFmpeg.CHANNELS;
		public const int FRAMES = FFmpeg.FRAMES;
		public const int SCALE = 4;
		public const float BUFFER_LENGTH = 1.0f;

		private AudioStreamGeneratorPlayback _playback;
		private Process _ffmpeg;
		private CancellationTokenSource _cts;
		private AudioStreamPlayer _player;

		private Queue<byte[]> _pcmQueue = new();
		private object _lock = new();

		private bool _playing = false;
		private string _currentFile = null;
		private double _bufferedSeconds = 0;
		private double _startOffset = 0; // in seconds
		private ulong _totalPlaybackFrames = 0; // in frames

		private FFprobeResult _metadata;
		public FFprobeResult Metadata
		{
			get => _metadata;
		}


		public bool Playing { get => _playing; }
		public string CurrentFile { get => _currentFile; }
		public double PlaybackPosition 
		{ 	get
			{
				if(Metadata != null)
				{
					double PlayingTime = (double)(_totalPlaybackFrames - SAMPLE_RATE * SCALE) / SAMPLE_RATE / SCALE / 2.0 + _startOffset;
					double Duration = double.Parse(Metadata.Format.Duration.ToString().Replace('.', ','));
					return PlayingTime - Mathf.Floor(PlayingTime / Duration) * Duration;
				}
				else
					return 0;
			}
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
				BufferLength = BUFFER_LENGTH
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

			int floatCount = data.Length / SCALE;
			float[] samples = new float[floatCount];
			Buffer.BlockCopy(data, 0, samples, 0, data.Length);

			PushSamples(_playback, samples);
			_bufferedSeconds = _playback.GetFramesAvailable() / SAMPLE_RATE;
			_totalPlaybackFrames++;
		}

		#region Public Methods
		public void Play(string pathOrUrl, double StartOffset = 0.0)
		{
			GD.Print("Play called");
			Stop();

			string input = pathOrUrl.StartsWith("user://") // res:// not supported because of ffmpeg, need to extract audio before use
				? ProjectSettings.GlobalizePath(pathOrUrl)
				: pathOrUrl;

			_cts = new CancellationTokenSource();
			_player.Play();
			_playback = (AudioStreamGeneratorPlayback)_player.GetStreamPlayback();
			_currentFile = input;
			_startOffset = StartOffset;

			Godot.Collections.Array<string> additionalArgs = [];
			
			additionalArgs += ["-ss", StartOffset.ToString().Replace(',', '.')];

			_ffmpeg = StartFFmpeg(input, FFmpeg.IsUrl(pathOrUrl), [.. additionalArgs]);

			//GD.Print("Metadating");
			_metadata = FFprobe.FFprobe.GetMetadata(input);
			//GD.Print("Metadated");
			_ = Task.Run(() => ReadPcmLoop(_cts.Token));
			_playing = true;
			EmitSignal("Played");
			GD.Print("Playing");
		}

		public void Stop()
		{
			EmitSignal("Stopped");
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
			_totalPlaybackFrames = 0;
			_startOffset = 0;
			_currentFile = null;
			_metadata = null;
		}

		public void Seek(double Time) // In seconds
		{
			if (Playing)
			{
				string file = _currentFile;
				Stop();
				Play(file, Time);
			}
		}
		public void Restart(double StartOffset = 0)
		{
			// stoping
			GD.Print("Restarting");
			_cts?.Cancel();
			_cts = null;

			try
			{
				if (_ffmpeg != null && !_ffmpeg.HasExited)
					_ffmpeg.Kill();
			}
			catch { }

			_ffmpeg = null;
			
			// starting
			_cts = new CancellationTokenSource();

			_ffmpeg = StartFFmpeg(_currentFile, FFmpeg.IsUrl(_currentFile), []);

			//GD.Print("Metadating");
			//_metadata = FFprobe.FFprobe.GetMetadata(_currentFile);
			//GD.Print("Metadated");
			_ = Task.Run(() => ReadPcmLoop(_cts.Token));
			_playing = true;
		}

		#endregion
		#region Decoding
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
			byte[] buffer = new byte[FRAMES * CHANNELS * SCALE];

			try
			{
				bool canContinue = true;
				while (!token.IsCancellationRequested)
				{
					int read = await stdout.ReadAsync(buffer, token);
					if (!canContinue) break;
					if (read == 0 && canContinue)
					{
						canContinue = false;
						await Task.Delay(10, token);
						continue;
					}
					else canContinue = true;


					byte[] chunk = new byte[read];
					Buffer.BlockCopy(buffer, 0, chunk, 0, read);
					_totalPlaybackFrames += (ulong)read;

					lock (_lock)
						_pcmQueue.Enqueue(chunk);
				}
				if (Loop) Restart();
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
		#endregion
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
				return ":3"; // why do null when I can do ":3"?
			}
			
			public static FFprobeResult GetMetadata(string filePath)
			{
				string RawMetadata = GetRawMetadata(filePath);
				GD.Print(RawMetadata);
				//GD.Print("hmmm, need to deserialize?");
				if (RawMetadata == ":3")
				{
					//GD.Print("No.");
					return null;
				}
				//GD.Print("Yes! Deserialized!");
				return JsonSerializer.Deserialize<FFprobeResult>(RawMetadata);
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
	#region FFprobe Metadata
	public class FFprobeResult()
	{
		[JsonPropertyName("format")]
		public FFprobeFormat Format { get; set; }
	}
	public class FFprobeFormat()
	{
		[JsonPropertyName("filename")]
		public string Filename { get; set; }
		[JsonPropertyName("nb_streams")]
		public float NB_Streams { get; set; }
		[JsonPropertyName("nb_programs")]
		public float NB_Programs { get; set; }
		[JsonPropertyName("nb_stream_groups")]
		public float NB_StreamGroups { get; set; }
		[JsonPropertyName("format_name")]
		public string FormatName { get; set; }
		[JsonPropertyName("format_long_name")]
		public string FormatLongName { get; set; }
		/// <summary>
		/// Can be null
		/// </summary>
		[JsonPropertyName("start_time")]
		public string StartTime { get; set; }
		[JsonPropertyName("duration")]
		public string Duration { get; set; }
		[JsonPropertyName("size")]
		public string Size { get; set; }
		[JsonPropertyName("bit_rate")]
		public string BitRate { get; set; }
		[JsonPropertyName("probe_score")]
		public float ProbeScore { get; set; }

		[JsonPropertyName("tags")]
		public FFprobeTags Tags { get; set; }
	}
	public class FFprobeTags
	{
		[JsonPropertyName("title")]
		public string Title { get; set; }
		[JsonPropertyName("artist")]
		public string Artist { get; set; }
		[JsonPropertyName("composer")]
		public string Composer { get; set; }
		[JsonPropertyName("album")]
		public string Album { get; set; }
		[JsonPropertyName("genre")]
		public string Genre { get; set; }
		[JsonPropertyName("track")]
		public string Track { get; set; }
		[JsonPropertyName("date")]
		public string Date { get; set; }
		[JsonPropertyName("comment")]
		public string Comment { get; set; }
		[JsonPropertyName("encoded_by")]
		public string EncodedBy { get; set; }
		[JsonPropertyName("disk")]
		public string Disk { get; set; }
		[JsonPropertyName("TBPM")]
		public string BPM { get; set; }
	}
	#endregion
	#endregion
}
