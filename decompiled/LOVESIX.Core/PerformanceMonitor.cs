using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LOVESIX.Core;

/// <summary>
/// Real-time performance monitoring for the application
/// </summary>
public class PerformanceMonitor
{
    private Process _currentProcess;
    private PerformanceCounter _cpuCounter;
    private PerformanceCounter _ramCounter;
    
    public PerformanceStats CurrentStats { get; private set; } = new();
    public event Action<PerformanceStats> StatsUpdated;
    
    public PerformanceMonitor()
    {
        try
        {
            _currentProcess = Process.GetCurrentProcess();
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
        }
        catch { /* Graceful degradation if performance counters not available */ }
    }
    
    public async Task StartMonitoring(int intervalMs = 1000)
    {
        while (true)
        {
            try
            {
                await Task.Delay(intervalMs);
                UpdateStats();
                StatsUpdated?.Invoke(CurrentStats);
            }
            catch { }
        }
    }
    
    private void UpdateStats()
    {
        if (_currentProcess == null) return;
        
        try
        {
            CurrentStats.Timestamp = DateTime.Now;
            CurrentStats.MemoryUsageMb = _currentProcess.WorkingSet64 / (1024 * 1024);
            CurrentStats.ThreadCount = _currentProcess.Threads.Count;
            
            if (_cpuCounter != null)
                CurrentStats.CpuUsagePercent = (float)_cpuCounter.NextValue();
            
            if (_ramCounter != null)
                CurrentStats.AvailableMemoryMb = (float)_ramCounter.NextValue();
        }
        catch { }
    }
}

public class PerformanceStats
{
    public DateTime Timestamp { get; set; }
    public long MemoryUsageMb { get; set; }
    public int ThreadCount { get; set; }
    public float CpuUsagePercent { get; set; }
    public float AvailableMemoryMb { get; set; }
}
