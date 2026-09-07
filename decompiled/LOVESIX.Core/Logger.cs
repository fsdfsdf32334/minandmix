using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace LOVESIX.Core;

/// <summary>
/// Centralized logging system with file persistence
/// </summary>
public static class Logger
{
    public enum LogLevel { Debug, Info, Warning, Error, Critical }
    
    private static string _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIXANDMIN", "logs");
    private static Queue<LogEntry> _logBuffer = new(100);
    private static LogLevel _minLevel = LogLevel.Debug;
    
    public static event Action<LogEntry> OnLog;
    
    static Logger()
    {
        try
        {
            Directory.CreateDirectory(_logPath);
        }
        catch { }
    }
    
    public static void Log(LogLevel level, string message, Exception ex = null)
    {
        if (level < _minLevel) return;
        
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            Exception = ex?.ToString()
        };
        
        _logBuffer.Enqueue(entry);
        if (_logBuffer.Count > 100)
            _logBuffer.Dequeue();
        
        OnLog?.Invoke(entry);
        
        try
        {
            string logFile = Path.Combine(_logPath, $"mixandmin_{DateTime.Now:yyyy-MM-dd}.log");
            File.AppendAllText(logFile, FormatLogEntry(entry) + Environment.NewLine, Encoding.UTF8);
        }
        catch { }
    }
    
    public static void Debug(string message) => Log(LogLevel.Debug, message);
    public static void Info(string message) => Log(LogLevel.Info, message);
    public static void Warning(string message) => Log(LogLevel.Warning, message);
    public static void Error(string message, Exception ex = null) => Log(LogLevel.Error, message, ex);
    public static void Critical(string message, Exception ex = null) => Log(LogLevel.Critical, message, ex);
    
    private static string FormatLogEntry(LogEntry entry)
    {
        var sb = new StringBuilder();
        sb.Append($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] ");
        sb.Append($"[{entry.Level}] ");
        sb.Append(entry.Message);
        if (!string.IsNullOrEmpty(entry.Exception))
            sb.Append($" | Exception: {entry.Exception}");
        return sb.ToString();
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public Logger.LogLevel Level { get; set; }
    public string Message { get; set; }
    public string Exception { get; set; }
}
