using System;
using System.Windows.Forms;
using LOVESIX.Core;
using LOVESIX.UI;

namespace LOVESIX.UI;

/// <summary>
/// Advanced Statistics Tab - Comprehensive stats and analytics
/// </summary>
public class AdvancedStatsTab : Panel
{
    private Label _lblTitle;
    private StatsDashboard _statsDashboard;
    private Label _lblNetworkStatus;
    private Label _lblConnectionQuality;
    private FlowLayoutPanel _flowNetworkDetails;
    private Button _btnRunDiagnostics;
    private Label _lblDiagStatus;
    private StatsEngine _statsEngine;
    
    public AdvancedStatsTab(StatsEngine statsEngine)
    {
        _statsEngine = statsEngine;
        InitializeComponent();
        LoadInitialStats();
    }
    
    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Theme.Back;
        this.Padding = new Padding(10);
        this.AutoScroll = true;
        
        var mainFlow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Width = this.Width - 20,
            BackColor = Theme.Back
        };
        
        // Title
        _lblTitle = new Label
        {
            Text = "📊 Advanced Statistics & Analytics",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        
        // Stats Dashboard
        _statsDashboard = new StatsDashboard();
        _statsDashboard.Margin = new Padding(0, 0, 0, 12);
        
        // Network Status Card
        var networkCard = CreateNetworkCard();
        networkCard.Margin = new Padding(0, 0, 0, 12);
        
        mainFlow.Controls.Add(_lblTitle);
        mainFlow.Controls.Add(_statsDashboard);
        mainFlow.Controls.Add(networkCard);
        
        this.Controls.Add(mainFlow);
    }
    
    private Panel CreateNetworkCard()
    {
        var card = new Panel
        {
            Width = 500,
            BackColor = Theme.Panel,
            Height = 150
        };
        
        var cardTitle = new Label
        {
            Text = "🌐 Network Diagnostics",
            Location = new Point(12, 10),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true
        };
        
        _lblConnectionQuality = new Label
        {
            Text = "Connection Quality: Testing...",
            Location = new Point(12, 40),
            ForeColor = Theme.Text,
            AutoSize = true
        };
        
        _lblNetworkStatus = new Label
        {
            Text = "Status: Idle",
            Location = new Point(12, 65),
            ForeColor = Theme.Muted,
            AutoSize = true
        };
        
        _btnRunDiagnostics = new Button
        {
            Text = "🔍 Run Diagnostics",
            Location = new Point(12, 100),
            Size = new Size(150, 35),
            BackColor = Theme.Accent,
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        
        _lblDiagStatus = new Label
        {
            Text = "",
            Location = new Point(170, 110),
            ForeColor = Theme.Muted,
            AutoSize = true,
            Font = new Font("Segoe UI", 8)
        };
        
        card.Controls.Add(cardTitle);
        card.Controls.Add(_lblConnectionQuality);
        card.Controls.Add(_lblNetworkStatus);
        card.Controls.Add(_btnRunDiagnostics);
        card.Controls.Add(_lblDiagStatus);
        
        return card;
    }
    
    private void LoadInitialStats()
    {
        if (_statsEngine != null)
        {
            _statsDashboard.UpdateStats(_statsEngine);
        }
    }
}

/// <summary>
/// Performance Monitoring Tab
/// </summary>
public class PerformanceTab : Panel
{
    private PerformanceIndicator _perfIndicator;
    private Label _lblTitle;
    private Label _lblThreadCount;
    private ProgressBar _pbThreads;
    private Label _lblUpdateStatus;
    private Button _btnCheckUpdates;
    private PerformanceMonitor _perfMonitor;
    
    public PerformanceTab(PerformanceMonitor perfMonitor)
    {
        _perfMonitor = perfMonitor;
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Theme.Back;
        this.Padding = new Padding(10);
        this.AutoScroll = true;
        
        _lblTitle = new Label
        {
            Text = "⚡ Performance Monitor",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12),
            Dock = DockStyle.Top
        };
        
        _perfIndicator = new PerformanceIndicator();
        _perfIndicator.Margin = new Padding(0, 0, 0, 12);
        _perfIndicator.Dock = DockStyle.Top;
        
        var threadPanel = new Panel
        {
            BackColor = Theme.Panel,
            Height = 80,
            Width = 500,
            Margin = new Padding(0, 0, 0, 12),
            Dock = DockStyle.Top
        };
        
        _lblThreadCount = new Label
        {
            Text = "Threads: 0",
            Location = new Point(12, 10),
            ForeColor = Theme.Text,
            AutoSize = true
        };
        
        _pbThreads = new ProgressBar
        {
            Location = new Point(12, 35),
            Size = new Size(476, 20),
            Maximum = 100
        };
        
        threadPanel.Controls.Add(_lblThreadCount);
        threadPanel.Controls.Add(_pbThreads);
        
        var updatePanel = new Panel
        {
            BackColor = Theme.Panel,
            Height = 80,
            Width = 500,
            Dock = DockStyle.Top
        };
        
        _lblUpdateStatus = new Label
        {
            Text = "Version: 3.0.0 | Status: Up to date",
            Location = new Point(12, 12),
            ForeColor = Theme.Text,
            AutoSize = true
        };
        
        _btnCheckUpdates = new Button
        {
            Text = "Check Updates",
            Location = new Point(12, 35),
            Size = new Size(120, 30),
            BackColor = Theme.Accent,
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat
        };
        
        updatePanel.Controls.Add(_lblUpdateStatus);
        updatePanel.Controls.Add(_btnCheckUpdates);
        
        this.Controls.Add(updatePanel);
        this.Controls.Add(threadPanel);
        this.Controls.Add(_perfIndicator);
        this.Controls.Add(_lblTitle);
    }
}

/// <summary>
/// Advanced Tools Tab - Macros, Backups, Profiles
/// </summary>
public class AdvancedToolsTab : Panel
{
    private TabControl _toolTabs;
    private MacroEngine _macroEngine;
    private BackupManager _backupManager;
    private HotkeyProfileManager _hotkeyManager;
    
    public AdvancedToolsTab(MacroEngine macroEngine, BackupManager backupManager, HotkeyProfileManager hotkeyManager)
    {
        _macroEngine = macroEngine;
        _backupManager = backupManager;
        _hotkeyManager = hotkeyManager;
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Theme.Back;
        this.Padding = new Padding(10);
        
        _toolTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Panel,
            ForeColor = Theme.Text
        };
        
        // Macro Tab
        var macroTab = new TabPage("🎬 Macro Manager")
        {
            BackColor = Theme.Back
        };
        macroTab.Controls.Add(CreateMacroPanel());
        _toolTabs.TabPages.Add(macroTab);
        
        // Backup Tab
        var backupTab = new TabPage("💾 Backup Manager")
        {
            BackColor = Theme.Back
        };
        backupTab.Controls.Add(CreateBackupPanel());
        _toolTabs.TabPages.Add(backupTab);
        
        // Profile Tab
        var profileTab = new TabPage("⌨️ Hotkey Profiles")
        {
            BackColor = Theme.Back
        };
        profileTab.Controls.Add(CreateProfilePanel());
        _toolTabs.TabPages.Add(profileTab);
        
        this.Controls.Add(_toolTabs);
    }
    
    private Panel CreateMacroPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Back, Padding = new Padding(10) };
        
        var label = new Label
        {
            Text = "🎬 Macro Recorder",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true
        };
        
        var btnRecord = new Button
        {
            Text = "● Record",
            BackColor = Theme.Danger,
            ForeColor = System.Drawing.Color.White,
            Size = new Size(100, 35),
            Location = new Point(10, 40),
            FlatStyle = FlatStyle.Flat
        };
        
        var btnStop = new Button
        {
            Text = "⏹ Stop",
            BackColor = Theme.Muted,
            ForeColor = System.Drawing.Color.White,
            Size = new Size(100, 35),
            Location = new Point(120, 40),
            FlatStyle = FlatStyle.Flat
        };
        
        var listMacros = new ListBox
        {
            Location = new Point(10, 90),
            Size = new Size(480, 150),
            BackColor = Theme.Panel,
            ForeColor = Theme.Text
        };
        
        panel.Controls.Add(label);
        panel.Controls.Add(btnRecord);
        panel.Controls.Add(btnStop);
        panel.Controls.Add(listMacros);
        
        return panel;
    }
    
    private Panel CreateBackupPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Back, Padding = new Padding(10) };
        
        var label = new Label
        {
            Text = "💾 Backup & Recovery",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true
        };
        
        var btnCreate = new Button
        {
            Text = "💾 Create Backup",
            BackColor = Theme.Accent,
            ForeColor = System.Drawing.Color.Black,
            Size = new Size(150, 35),
            Location = new Point(10, 40),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        
        var listBackups = new ListBox
        {
            Location = new Point(10, 90),
            Size = new Size(480, 200),
            BackColor = Theme.Panel,
            ForeColor = Theme.Text
        };
        
        panel.Controls.Add(label);
        panel.Controls.Add(btnCreate);
        panel.Controls.Add(listBackups);
        
        return panel;
    }
    
    private Panel CreateProfilePanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Back, Padding = new Padding(10) };
        
        var label = new Label
        {
            Text = "⌨️ Hotkey Profiles",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Theme.Accent,
            AutoSize = true
        };
        
        var listProfiles = new ListBox
        {
            Location = new Point(10, 40),
            Size = new Size(480, 150),
            BackColor = Theme.Panel,
            ForeColor = Theme.Text
        };
        
        var btnNew = new Button
        {
            Text = "➕ New Profile",
            BackColor = Theme.On,
            ForeColor = System.Drawing.Color.Black,
            Size = new Size(120, 30),
            Location = new Point(10, 200),
            FlatStyle = FlatStyle.Flat
        };
        
        panel.Controls.Add(label);
        panel.Controls.Add(listProfiles);
        panel.Controls.Add(btnNew);
        
        return panel;
    }
}
