using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using LOVESIX.Core;
using LOVESIX.UI;

namespace LOVESIX;

/// <summary>
/// Application Bootstrap - Initializes all systems on startup
/// </summary>
public static class ApplicationBootstrap
{
    public static void Initialize()
    {
        try
        {
            Logger.Info("=== MIXANDMIN v3.0 Bootstrap Starting ===");
            
            // 1. Initialize configuration
            Logger.Info("Step 1: Loading configuration...");
            ConfigurationManager.LoadConfig();
            
            // 2. Initialize logging
            Logger.Info("Step 2: Logger initialized");
            
            // 3. Initialize theme manager
            Logger.Info("Step 3: Initializing Theme Manager...");
            ThemeManager.CurrentMode = ThemeManager.ThemeMode.Dark;
            
            // 4. Perform health check
            Logger.Info("Step 4: Performing health check...");
            var healthStatus = HealthCheck.PerformHealthCheck();
            if (!healthStatus.IsHealthy)
            {
                foreach (var issue in healthStatus.Issues)
                {
                    Logger.Warning($"Health issue: {issue}");
                }
            }
            
            // 5. Verify all dependencies
            Logger.Info("Step 5: Verifying dependencies...");
            VerifyDependencies();
            
            Logger.Info("=== Bootstrap Complete ===");
        }
        catch (Exception ex)
        {
            CrashReporter.HandleException(ex, "Application Bootstrap");
            MessageBox.Show($"Bootstrap Error: {ex.Message}", "MIXANDMIN Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            throw;
        }
    }
    
    private static void VerifyDependencies()
    {
        // Check for required DLLs
        var requiredDlls = new[] { "WinDivert.dll", "WinDivert64.sys" };
        foreach (var dll in requiredDlls)
        {
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dll);
            if (File.Exists(dllPath))
            {
                Logger.Info($"✓ Dependency verified: {dll}");
            }
            else
            {
                Logger.Warning($"✗ Missing dependency: {dll}");
            }
        }
    }
    
    public static void Shutdown()
    {
        try
        {
            Logger.Info("=== MIXANDMIN Shutdown ===");
            ConfigurationManager.SaveConfig();
            Logger.Info("Configuration saved");
            Logger.Info("=== Shutdown Complete ===");
        }
        catch (Exception ex)
        {
            CrashReporter.HandleException(ex, "Application Shutdown");
        }
    }
}
