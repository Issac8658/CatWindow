using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Runtime.InteropServices;

namespace CatWindowBootstrap;
class Program
{
    const string ExeFileName = "CatWindow.bin";

	private const string MutexName = "CatWindowAudioPlayer_CatHouse";
	private const string PipeName = "CatWindowAudioPlayer_MainCat";
    
    static void Main(string[] args)
    {
        Mutex _mutex = new Mutex(true, MutexName, out bool createdNew);

        Console.Write("Input args: ");
        foreach (string arg in args) Console.Write(arg + " ");
        Console.Write("\n");

        if (!createdNew)
		{
			SendToRunningInstance(args);
            Console.WriteLine("CatWindow already launched! Sending args and closing...");
			return;
		}

        Console.WriteLine("Starting " + ExeFileName);

        if (!File.Exists(Path.Combine(AppContext.BaseDirectory, ExeFileName)))
        {
            ShowError("File not found", "\"CatWindow.bin\" not found, please reinstall program");
            return;
        }
        
		ProcessStartInfo psi = new()
		{
            FileName = ExeFileName,
            UseShellExecute = false
		};

        foreach (string arg in args) psi.ArgumentList.Add(arg);

        Process Cat = new() { StartInfo = psi };

        Cat.Start();
    }

	private static void SendToRunningInstance(string[] args)
	{
		if (args.Length == 0)
			return;

		try
		{
			using var client = new NamedPipeClientStream(
				".",
				PipeName,
				PipeDirection.Out);

			client.Connect(200);

			using var writer = new StreamWriter(client, Encoding.UTF8)
			{
				AutoFlush = true
			};

			writer.WriteLine(args[0]);
		}
		catch { }
	}

    #region Error show
    public static void ShowError(string title, string message)
    {
        TryWriteLog(title, message);

        if (OperatingSystem.IsWindows())
        {
            ShowWindows(title, message);
            return;
        }

        if (OperatingSystem.IsLinux())
        {
            if (TryZenity(title, message)) return;
            if (TryKDialog(title, message)) return;
            if (TryNotifySend(title, message)) return;
        }
    }

    // Windows

    static void ShowWindows(string title, string message)
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int MessageBoxW(
            IntPtr hWnd,
            string lpText,
            string lpCaption,
            uint uType
        );

        _ = MessageBoxW(
            IntPtr.Zero,
            message,
            title,
            0x10 // MB_ICONERROR
        );
    }

    // Linux

    static bool TryZenity(string title, string message)
        => TryRun("zenity", $"--error --title=\"{title}\" --text=\"{Escape(message)}\"");

    static bool TryKDialog(string title, string message)
        => TryRun("kdialog", $"--error \"{Escape(message)}\" --title \"{title}\"");

    static bool TryNotifySend(string title, string message)
        => TryRun("notify-send", $"\"{title}\" \"{Escape(message)}\"");
    
    static bool TryRun(string file, string args)
    {
        try
        {
            Process.Start(new ProcessStartInfo {
                FileName = file,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    static void TryWriteLog(string title, string message)
    {
        try
        {
            File.WriteAllText(
                Path.Combine(AppContext.BaseDirectory, "error.log"),
                $"{title}\n{message}\n"
            );
        }
        catch { }
    }

    static string Escape(string s)
        => s.Replace("\"", "\\\"");
    
    #endregion
}
