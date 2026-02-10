using Godot;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

public partial class MutexListener : Node
{
	[Signal] public delegate void FileCaughtEventHandler(string FilePath);
	[Signal] public delegate void NewCatRequiredEventHandler();

	private static System.Threading.Mutex _mutex; // this variable is needed because otherwise Mutex doesn't work :/
	private const string MutexName = "CatWindowAudioPlayer_CatHouse";
	private const string PipeName = "CatWindowAudioPlayer_MainCat";

	public override void _Ready()
	{
		bool createdNew;
		_mutex = new System.Threading.Mutex(true, MutexName, out createdNew);
		
		_ = Task.Run(StartPipeServer);
	}

	private async Task StartPipeServer()
	{
		while (true)
		{
			using var server = new NamedPipeServerStream(
				PipeName,
				PipeDirection.In);

			await server.WaitForConnectionAsync();

			using var reader = new StreamReader(server, Encoding.UTF8);
			string path = await reader.ReadLineAsync();

			
			if (!string.IsNullOrEmpty(path))
				CallDeferred(nameof(HandleFileOpen), path);
			else
				EmitSignal("NewCatRequired");
		}
	}

	private void HandleFileOpen(string path)
	{
		EmitSignal("FileCaught", path);
	}
}
