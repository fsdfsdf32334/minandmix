using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace LOVESIX.UI;

/// <summary>
/// Advanced Statistics Dashboard Component
/// </summary>
public class StatsDashboard : Panel
{
    private Label _lblTotalUptime;
    private Label _lblMostUsedCheat;
    private Label _lblPingDashboard;
    private Label _lblPanicCount;
    private FlowLayoutPanel _flowCheatStats;
    
    public StatsDashboard()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        this.BackColor = Color.FromArgb(20, 28, 42);
        this.Padding = new Padding(12);
        
        var titleLabel = new Label
        {
            Text = "📊 Advanced Statistics",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 200, 255),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        
        _lblTotalUptime = new Label
        {
            Text = "⏱️ Total Uptime: 00:00:00",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(230, 235, 245),
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 4)
        };
        
        _lblMostUsedCheat = new Label
        {
            Text = "🥇 Most Used: None",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(230, 235, 245),
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 4)
        };
        
        _lblPingDashboard = new Label
        {
            Text = "⚡ Ping Sessions: 0",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(230, 235, 245),
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 4)
        };
        
        _lblPanicCount = new Label
        {
            Text = "🚨 Panic Events: 0",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(255, 100, 100),
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 12)
        };
        
        _flowCheatStats = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            BackColor = this.BackColor
        };
        
        this.Controls.Add(titleLabel);
        this.Controls.Add(_lblTotalUptime);
        this.Controls.Add(_lblMostUsedCheat);
        this.Controls.Add(_lblPingDashboard);
        this.Controls.Add(_lblPanicCount);
        this.Controls.Add(_flowCheatStats);
    }
    
    public void UpdateStats(Core.StatsEngine stats)
    {
        var currentStats = stats.GetCurrentStats();
        _lblTotalUptime.Text = $"⏱️ Total Uptime: {currentStats.TotalRuntime:hh\\:mm\\:ss}";
        _lblMostUsedCheat.Text = currentStats.MostUsedCheats.Any()
            ? $"🥇 Most Used: {currentStats.MostUsedCheats.First().Key}"
            : "🥇 Most Used: None";
        _lblPingDashboard.Text = $"⚡ Ping Sessions: {currentStats.PingSessionsCount}";
        _lblPanicCount.Text = $"🚨 Panic Events: {currentStats.TotalPanicEvents}";
    }
}

/// <summary>
/// Quick Action Panel with common shortcuts
/// </summary>
public class QuickActionPanel : Panel
{
    public event Action? OnPingToggle;
    public event Action? OnPanicPress;
    public event Action? OnBackupCreate;
    public event Action? OnThemeSwitch;
    
    public QuickActionPanel()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        this.BackColor = Color.FromArgb(20, 28, 42);
        this.Height = 60;
        this.Padding = new Padding(8);
        
        var flow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            Dock = DockStyle.Fill,
            WrapContents = false,
            BackColor = this.BackColor
        };
        
        var btnPing = CreateButton("⚡ Ping", Theme.Accent);
        btnPing.Click += (s, e) => OnPingToggle?.Invoke();
        
        var btnPanic = CreateButton("🚨 Panic", Color.FromArgb(255, 100, 100));
        btnPanic.Click += (s, e) => OnPanicPress?.Invoke();
        
        var btnBackup = CreateButton("💾 Backup", Theme.Accent2);
        btnBackup.Click += (s, e) => OnBackupCreate?.Invoke();
        
        var btnTheme = CreateButton("🌙 Theme", Theme.Muted);
        btnTheme.Click += (s, e) => OnThemeSwitch?.Invoke();
        
        flow.Controls.Add(btnPing);
        flow.Controls.Add(btnPanic);
        flow.Controls.Add(btnBackup);
        flow.Controls.Add(btnTheme);
        
        this.Controls.Add(flow);
    }
    
    private Button CreateButton(string text, Color bgColor)
    {
        return new Button
        {
            Text = text,
            Size = new Size(100, 40),
            BackColor = bgColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(4)
        };
    }
}

/// <summary>
/// Notification Toast System
/// </summary>
public class ToastNotification : Form
{
    public enum ToastLevel { Info, Success, Warning, Error }
    
    private Label _lblMessage;
    private Timer _dismissTimer;
    
    public ToastNotification(string message, ToastLevel level = ToastLevel.Info, int durationMs = 3000)
    {
        InitializeComponent();
        SetLevel(level);
        _lblMessage.Text = message;
        
        _dismissTimer = new Timer { Interval = durationMs };
        _dismissTimer.Tick += (s, e) => { _dismissTimer.Stop(); this.Close(); };
    }
    
    private void InitializeComponent()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.FromArgb(20, 28, 42);
        this.Size = new Size(300, 60);
        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - 320, 
                                   Screen.PrimaryScreen.WorkingArea.Bottom - 80);
        this.TopMost = true;
        this.ShowInTaskbar = false;
        
        _lblMessage = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(230, 235, 245),
            Font = new Font("Segoe UI", 10),
            Padding = new Padding(12),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = false
        };
        
        this.Controls.Add(_lblMessage);
    }
    
    private void SetLevel(ToastLevel level)
    {
        switch (level)
        {
            case ToastLevel.Success:
                this.BackColor = Color.FromArgb(30, 60, 40);
                break;
            case ToastLevel.Warning:
                this.BackColor = Color.FromArgb(60, 50, 20);
                break;
            case ToastLevel.Error:
                this.BackColor = Color.FromArgb(60, 20, 20);
                break;
        }
    }
    
    public void Show()
    {
        base.Show();
        _dismissTimer.Start();
    }
}

/// <summary>
/// Performance Indicator Widget
/// </summary>
public class PerformanceIndicator : UserControl
{
    private Label _lblCpu;
    private Label _lblRam;
    private ProgressBar _pbCpu;
    private ProgressBar _pbRam;
    
    public PerformanceIndicator()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        this.BackColor = Color.FromArgb(20, 28, 42);
        this.Size = new Size(200, 80);
        
        _lblCpu = new Label { Text = "CPU: 0%", Location = new Point(5, 5), Font = new Font("Segoe UI", 9) };
        _pbCpu = new ProgressBar { Location = new Point(5, 20), Size = new Size(190, 15) };
        
        _lblRam = new Label { Text = "RAM: 0 MB", Location = new Point(5, 40), Font = new Font("Segoe UI", 9) };
        _pbRam = new ProgressBar { Location = new Point(5, 55), Size = new Size(190, 15) };
        
        this.Controls.Add(_lblCpu);
        this.Controls.Add(_pbCpu);
        this.Controls.Add(_lblRam);
        this.Controls.Add(_pbRam);
    }
    
    public void UpdateMetrics(Core.PerformanceStats stats)
    {
        _lblCpu.Text = $"CPU: {stats.CpuUsagePercent:F1}%";
        _pbCpu.Value = Math.Min(100, (int)stats.CpuUsagePercent);
        
        _lblRam.Text = $"RAM: {stats.MemoryUsageMb} MB";
        _pbRam.Value = Math.Min(100, (int)(stats.MemoryUsageMb / 100));
    }
}
