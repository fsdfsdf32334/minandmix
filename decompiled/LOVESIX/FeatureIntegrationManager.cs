using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LOVESIX.Core;
using LOVESIX.UI;

namespace LOVESIX;

/// <summary>
/// Integration Manager - Bridges all advanced features with MainForm
/// </summary>
public class FeatureIntegrationManager
{
    private MainForm _mainForm;
    private NetworkDiagnostics _networkDiags;
    private UpdateManager _updateManager;
    private StatsEngine _statsEngine;
    private HotkeyProfileManager _hotkeyManager;
    private MacroEngine _macroEngine;
    private BackupManager _backupManager;
    private PerformanceMonitor _perfMonitor;
    private CheatHistory _cheatHistory;
    private System.Windows.Forms.Timer _integrationTimer;
    
    public FeatureIntegrationManager(MainForm mainForm)
    {
        _mainForm = mainForm;
        InitializeFeatures();
        Logger.Info("Feature Integration Manager initialized");
    }
    
    private void InitializeFeatures()
    {
        _networkDiags = new NetworkDiagnostics();
        _updateManager = new UpdateManager();
        _statsEngine = new StatsEngine();
        _hotkeyManager = new HotkeyProfileManager();
        _macroEngine = new MacroEngine();
        _backupManager = new BackupManager();
        _perfMonitor = new PerformanceMonitor();
        _cheatHistory = new CheatHistory();
        
        // Wire up events
        _networkDiags.MetricsUpdated += OnNetworkMetricsUpdated;
        _updateManager.UpdateCheckCompleted += OnUpdateCheckCompleted;
        _macroEngine.MacroExecuted += OnMacroExecuted;
        _cheatHistory.HistoryChanged += OnCheatHistoryChanged;
        
        // Start monitoring
        StartMonitoring();
        
        Logger.Info("All features initialized and wired");
    }
    
    private void StartMonitoring()
    {
        _integrationTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _integrationTimer.Tick += OnMonitoringTick;
        _integrationTimer.Start();
    }
    
    private void OnMonitoringTick(object sender, EventArgs e)
    {
        // Monitor performance
        // Update UI with stats
    }
    
    public NetworkDiagnostics GetNetworkDiagnostics() => _networkDiags;
    public UpdateManager GetUpdateManager() => _updateManager;
    public StatsEngine GetStatsEngine() => _statsEngine;
    public HotkeyProfileManager GetHotkeyManager() => _hotkeyManager;
    public MacroEngine GetMacroEngine() => _macroEngine;
    public BackupManager GetBackupManager() => _backupManager;
    public PerformanceMonitor GetPerformanceMonitor() => _perfMonitor;
    public CheatHistory GetCheatHistory() => _cheatHistory;
    
    private void OnNetworkMetricsUpdated(NetworkDiagnostics.NetworkMetrics metrics)
    {
        Logger.Debug($"Network: {metrics.ConnectionQuality} | RTT: {metrics.AvgRttMs}ms | Loss: {metrics.PacketLossPercent}%");
    }
    
    private void OnUpdateCheckCompleted(UpdateManager.VersionInfo versionInfo)
    {
        Logger.Info($"Update check: {versionInfo.CurrentVersion} | Available: {versionInfo.UpdateAvailable}");
    }
    
    private void OnMacroExecuted(string macroName)
    {
        Logger.Info($"Macro executed: {macroName}");
    }
    
    private void OnCheatHistoryChanged()
    {
        Logger.Debug("Cheat history updated");
    }
    
    public void Cleanup()
    {
        _integrationTimer?.Stop();
        Logger.Info("Feature Integration Manager cleaned up");
    }
}
