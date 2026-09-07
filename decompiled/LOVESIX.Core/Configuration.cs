using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LOVESIX.Core;

/// <summary>
/// Application Configuration Manager
/// </summary>
public static class ConfigurationManager
{
    private static string _configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MIXANDMIN",
        "config.json"
    );
    
    public class AppConfig
    {
        public string Theme { get; set; } = "Dark";
        public bool AutoStartPing { get; set; } = false;
        public bool EnableAdvancedFeatures { get; set; } = true;
        public bool EnableNetworkDiagnostics { get; set; } = true;
        public bool EnablePerformanceMonitoring { get; set; } = true;
        public bool EnableMacroRecording { get; set; } = true;
        public int StatisticsUpdateIntervalMs { get; set; } = 1000;
        public List<string> EnabledModules { get; set; } = new()
        {
            "ThemeManager",
            "Logger",
            "PerformanceMonitor",
            "NetworkDiagnostics",
            "StatsEngine",
            "MacroEngine",
            "BackupManager"
        };
    }
    
    private static AppConfig _currentConfig;
    
    public static AppConfig GetConfig()
    {
        if (_currentConfig == null)
        {
            LoadConfig();
        }
        return _currentConfig;
    }
    
    public static void LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                string json = File.ReadAllText(_configPath);
                _currentConfig = JsonSerializer.Deserialize<AppConfig>(json);
            }
            else
            {
                _currentConfig = new AppConfig();
                SaveConfig();
            }
            Logger.Info("Configuration loaded successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load configuration: {ex.Message}", ex);
            _currentConfig = new AppConfig();
        }
    }
    
    public static void SaveConfig()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_currentConfig, options);
            File.WriteAllText(_configPath, json);
            Logger.Info("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to save configuration: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Application Health Check System
/// </summary>
public static class HealthCheck
{
    public class HealthStatus
    {
        public bool IsHealthy { get; set; }
        public List<string> Issues { get; set; } = new();
        public DateTime CheckedAt { get; set; }
    }
    
    public static HealthStatus PerformHealthCheck()
    {
        var status = new HealthStatus { CheckedAt = DateTime.Now, IsHealthy = true };
        
        try
        {
            // Check memory
            var proc = System.Diagnostics.Process.GetCurrentProcess();
            long memoryMb = proc.WorkingSet64 / (1024 * 1024);
            if (memoryMb > 500)
            {
                status.Issues.Add($"High memory usage: {memoryMb} MB");
            }
            
            // Check thread count
            if (proc.Threads.Count > 50)
            {
                status.Issues.Add($"High thread count: {proc.Threads.Count}");
            }
            
            // Check config file
            if (!File.Exists(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MIXANDMIN", "config.json")))
            {
                status.Issues.Add("Configuration file not found");
            }
            
            status.IsHealthy = status.Issues.Count == 0;
            Logger.Info($"Health check completed: {(status.IsHealthy ? "Healthy" : "Issues found")}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Health check failed: {ex.Message}", ex);
            status.IsHealthy = false;
            status.Issues.Add($"Check failed: {ex.Message}");
        }
        
        return status;
    }
}

/// <summary>
/// Crash Reporter and Error Handler
/// </summary>
public static class CrashReporter
{
    public static void HandleException(Exception ex, string context = "")
    {
        try
        {
            Logger.Critical($"Exception in {context}", ex);
            
            string crashLogPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MIXANDMIN",
                "crashes",
                $"crash_{DateTime.Now:yyyy-MM-dd_HHmmss}.log"
            );
            
            Directory.CreateDirectory(Path.GetDirectoryName(crashLogPath));
            
            var crashInfo = new
            {
                Timestamp = DateTime.Now,
                Context = context,
                ExceptionType = ex.GetType().FullName,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException?.ToString()
            };
            
            string json = JsonSerializer.Serialize(crashInfo, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(crashLogPath, json);
        }
        catch { /* Prevent infinite exception loops */ }
    }
}
