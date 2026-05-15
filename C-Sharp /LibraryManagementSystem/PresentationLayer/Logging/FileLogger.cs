namespace PresentationLayer.Logging;

public static class FileLogger
{
    private static string _logFilePath = string.Empty;
    private static readonly object _lock = new();

    public static void Initialize()
    {
        var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Directory.CreateDirectory(logDir);

        var fileName  = $"LibraryLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        _logFilePath  = Path.Combine(logDir, fileName);

        var header = $"""
            ╔══════════════════════════════════════════════════════════╗
            ║           COMMUNITY LIBRARY SYSTEM - SESSION LOG         ║
            ╠══════════════════════════════════════════════════════════╣
            ║  Session Started : {DateTime.Now:yyyy-MM-dd HH:mm:ss}               ║
            ║  Log File        : {fileName,-38}║
            ╚══════════════════════════════════════════════════════════╝
            """;

        WriteRaw(header);
        WriteRaw("");
    }

    public static void Log(string message)
    {
        var timestamped = $"[{DateTime.Now:HH:mm:ss}] {message}";
        WriteRaw(timestamped);
    }

    public static void LogSection(string sectionName)
    {
        WriteRaw("");
        WriteRaw($"┌─────────────────────────────────────────┐");
        WriteRaw($"│  {sectionName,-41}│");
        WriteRaw($"└─────────────────────────────────────────┘");
    }

    public static void LogSuccess(string message)
    {
        Log($"✔ SUCCESS : {message}");
    }

    public static void LogError(string message)
    {
        Log($"✘ ERROR   : {message}");
    }

    public static void LogInfo(string message)
    {
        Log($"  INFO    : {message}");
    }

    public static void LogInput(string field, string value)
    {
        Log($"  INPUT   : {field} = {value}");
    }

    public static void LogSeparator()
    {
        WriteRaw($"  {"─",-50}");
    }

    public static void LogSessionEnd()
    {
        WriteRaw("");
        WriteRaw($"╔══════════════════════════════════════════════════════════╗");
        WriteRaw($"║  Session Ended : {DateTime.Now:yyyy-MM-dd HH:mm:ss}                ║");
        WriteRaw($"╚══════════════════════════════════════════════════════════╝");
    }

    private static void WriteRaw(string message)
    {
        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logFilePath, message + Environment.NewLine);
            }
            catch
            {
                // Logging should never crash the app
            }
        }
    }

    public static string GetLogFilePath() => _logFilePath;
}