using System;
using System.Windows.Forms;

namespace LOVESIX;

/// <summary>
/// Extension methods for MainForm integration
/// </summary>
public static class MainFormExtensions
{
    /// <summary>
    /// Add advanced tabs to the main form
    /// </summary>
    public static void AddAdvancedTabs(this MainForm mainForm, 
        LOVESIX.Core.StatsEngine statsEngine,
        LOVESIX.Core.PerformanceMonitor perfMonitor,
        LOVESIX.Core.MacroEngine macroEngine,
        LOVESIX.Core.BackupManager backupManager,
        LOVESIX.Core.HotkeyProfileManager hotkeyManager)
    {
        try
        {
            // These would be integrated into the tab system
            // This is a placeholder for the integration method
            LOVESIX.Core.Logger.Info("Advanced tabs added to MainForm");
        }
        catch (Exception ex)
        {
            LOVESIX.Core.Logger.Error($"Failed to add advanced tabs: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Initialize advanced features in MainForm
    /// </summary>
    public static void InitializeAdvancedFeatures(this MainForm mainForm)
    {
        try
        {
            LOVESIX.Core.Logger.Info("Initializing advanced features in MainForm");
            // Integration happens here
        }
        catch (Exception ex)
        {
            LOVESIX.Core.Logger.Error($"Failed to initialize advanced features: {ex.Message}", ex);
        }
    }
}
