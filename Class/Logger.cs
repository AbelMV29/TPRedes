namespace TPRedes.Class;

public static class Logger
{
    private static readonly object WriteLock = new object();

    public static string LogPath { get; set; } = "./Logs";

    public static void LogInfo(string message)
    {
        Log(message, LogLevel.Info);
    }

    public static void LogWarning(string message)
    {
        Log(message, LogLevel.Warning);
    }

    public static void LogError(string message)
    {
        Log(message, LogLevel.Error);
    }

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        string prefix = level switch
        {
            LogLevel.Info => "[INFO]",
            LogLevel.Warning => "[WARNING]",
            LogLevel.Error => "[ERROR]",
            _ => "[UNKNOWN]"
        };

        DateTime now = DateTime.UtcNow.AddHours(-3);
        string todayFile = $"log_{now:yyyy-MM-dd}.txt";
        string pathFinal = Path.Combine(LogPath, todayFile);
        string logEntry = $"{now:dd-MM-yyyy HH:mm:ss} {prefix} {message}{Environment.NewLine}";

        lock (WriteLock)
        {
            if (!Directory.Exists(LogPath)) Directory.CreateDirectory(LogPath);
            if (!File.Exists(pathFinal)) File.Create(pathFinal).Close();

            File.AppendAllText(pathFinal, logEntry);
            Console.WriteLine(logEntry);
        }
    }
}

public enum LogLevel
{
    Info,
    Warning,
    Error
}
