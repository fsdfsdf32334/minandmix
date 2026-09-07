using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LOVESIX.Core;
using LOVESIX.UI;

namespace LOVESIX;

public class MainForm : Form
{
	private const int BodyWidth = 470;

	private readonly List<Cheat> _cheats = CheatRegistry.Build();

	private readonly List<Cheat> _regular;

	private readonly Dictionary<Cheat, Control> _ctrl = new Dictionary<Cheat, Control>();

	private readonly Dictionary<string, Cheat> _groupActive = new Dictionary<string, Cheat>();

	private readonly HashSet<Cheat> _busy = new HashSet<Cheat>();

	private readonly SemaphoreSlim _toggleLock = new SemaphoreSlim(1, 1);

	private readonly List<(Button Btn, Panel Page, Panel Bar)> _tabs = new List<(Button, Panel, Panel)>();

	private readonly GlobalKeyboardHook _keyboardHook = new GlobalKeyboardHook();

	private readonly HashSet<Keys> _pressed = new HashSet<Keys>();

	private readonly Dictionary<Cheat, bool> _holdWanted = new Dictionary<Cheat, bool>();

	private Cheat _cheatPush;

	private Cheat _cheatSink;

	private Button _pushKeyBtn;

	private Button _sinkKeyBtn;

	private bool _capturingPush;

	private bool _capturingSink;

	// Ping Engine fields
	private readonly PingEngine _pingEngine = new PingEngine();
	private System.Windows.Forms.Timer? _pingStatsTimer;
	private Button? _btnPingHero;
	private Label? _lblPingStatus;
	private Label? _lblPingVal;
	private Label? _lblPingStats;
	private Label? _lblPingPorts;
	private TrackBar? _tbMinPing;
	private TrackBar? _tbMaxPing;
	private Label? _lblMinPingDisp;
	private Label? _lblMaxPingDisp;
	private Button? _btnPingHk;
	private bool _capturingPingHk;

	// Hotkeys for Hide Window & Panic
	private Button? _btnHideHk;
	private bool _capturingHideHk;
	private Button? _btnPanicHk;
	private bool _capturingPanicHk;

	// Custom AOB Flow
	private FlowLayoutPanel? _customAobFlow;

	// Dashboard fields
	private readonly DateTime _startTime = DateTime.Now;
	private System.Windows.Forms.Timer? _dashboardTimer;
	private Label? _lblDashAttach;
	private Label? _lblDashCheats;
	private Label? _lblDashPing;
	private Label? _lblDashUptime;
	private Label? _lblDashActiveList;

	// Automation and reports
	private System.Windows.Forms.Timer? _automationTimer;
	private DateTime? _activeSessionStartedAt;
	private DateTime? _cheatSessionStartedAt;
	private Label? _lblReportSummary;
	private Label? _lblReportTopCheats;
	private FlowLayoutPanel? _historyFlow;
	private Label? _lblAutomationStatus;

	// Preset Flow
	private FlowLayoutPanel? _presetFlow;

	// Jitter & Packet Loss controls
	private TrackBar? _tbJitter;
	private Label? _lblJitterDisp;
	private TrackBar? _tbPacketLoss;
	private Label? _lblPacketLossDisp;

	// Overlay
	private OverlayForm? _overlay;

	// Auto reattach
	private System.Windows.Forms.Timer? _reattachTimer;

	// Search box for general tab
	private TextBox? _searchBox;

	private nint _procHandle = IntPtr.Zero;

	private ComboBox _procCombo;

	private Label _statusLabel;

	private NotifyIcon _tray;

	private bool _isExiting;

	public event Action LogoutRequested;

	public MainForm()
	{
		_cheatPush = _cheats.First((Cheat c) => c.Name == "ส\u0e31\u0e48งพ\u0e38\u0e48ง");
		_cheatSink = _cheats.First((Cheat c) => c.Name == "ส\u0e31\u0e48งจม");
		_regular = _cheats.Where((Cheat c) => c.Group == null).ToList();
		InitPingEngine();
		BuildUi();
		ApplySettings();
		BuildTray();
		_keyboardHook.KeyDown += OnGlobalKeyDown;
		_keyboardHook.KeyUp += OnGlobalKeyUp;
		_keyboardHook.Install();

		// Init overlay HUD
		_overlay = new OverlayForm();
		if (AppSettings.Current.OverlayEnabled)
			_overlay.SetOverlayEnabled(true);

		// Auto re-attach watcher (every 2s)
		_reattachTimer = new System.Windows.Forms.Timer { Interval = 2000 };
		_reattachTimer.Tick += OnReattachTick;
		_reattachTimer.Start();

		// Dashboard tick timer (every 1s)
		_dashboardTimer = new System.Windows.Forms.Timer { Interval = 1000 };
		_dashboardTimer.Tick += OnDashboardTick;
		_dashboardTimer.Start();

		_automationTimer = new System.Windows.Forms.Timer { Interval = 10000 };
		_automationTimer.Tick += OnAutomationTick;
		_automationTimer.Start();
		LogUsage("Session", "เปิดโปรแกรม");
	}

	private void BuildUi()
	{
		Text = "MIXANDMIN";
		base.StartPosition = FormStartPosition.CenterScreen;
		BackColor = Theme.Back;
		ForeColor = Theme.Text;
		Font = Theme.Font;
		MinimumSize = new Size(680, 750);
		base.ClientSize = new Size(720, 820);
		base.AutoScaleMode = AutoScaleMode.Dpi;
		base.Icon = IconFactory.CreateAppIcon();
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			BackColor = Theme.Back,
			ColumnCount = 1,
			RowCount = 4
		};
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 96f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
		tableLayoutPanel.Controls.Add(BuildHeader(), 0, 0);
		tableLayoutPanel.Controls.Add(BuildAttachBar(), 0, 1);
		tableLayoutPanel.Controls.Add(BuildBody(), 0, 2);
		tableLayoutPanel.Controls.Add(BuildStatusBar(), 0, 3);
		base.Controls.Add(tableLayoutPanel);
		base.Shown += OnShown;
		base.FormClosing += OnFormClosing;
		base.FormClosed += delegate
		{
			if (_tray != null)
			{
				_tray.Visible = false;
				_tray.Dispose();
				_tray = null;
			}
			Cleanup();
		};
	}

	private Control BuildHeader()
	{
		Panel panel = new Panel
		{
			Dock = DockStyle.Fill
		};
		panel.Paint += delegate(object? s, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			Rectangle clientRectangle = panel.ClientRectangle;
			graphics.Clear(Theme.Back);
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			using (GraphicsPath path = Theme.RoundRect(new Rectangle(16, 14, 46, 46), 12))
			{
				using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(16, 14, 46, 46), Theme.Accent, Theme.Accent2, 45f);
				graphics.FillPath(brush, path);
			}
			using (Font font = new Font("Segoe UI", 12f, FontStyle.Bold))
			{
				SizeF sizeF = graphics.MeasureString("MM", font);
				using SolidBrush brush2 = new SolidBrush(Color.FromArgb(10, 12, 16));
				graphics.DrawString("MM", font, brush2, 16f + (46f - sizeF.Width) / 2f, 14f + (46f - sizeF.Height) / 2f);
			}
			using (Font font2 = new Font("Segoe UI", 16f, FontStyle.Bold))
			{
				using SolidBrush brush3 = new SolidBrush(Theme.Text);
				graphics.DrawString("MIXANDMIN", font2, brush3, 72f, 13f);
			}
			using (Font font3 = new Font("Segoe UI", 9f))
			{
				using SolidBrush brush4 = new SolidBrush(Theme.Muted);
				graphics.DrawString("FiveM / GTA5  •  Ping Controller  •  Trainer v3.0", font3, brush4, 73f, 42f);
			}
			using (SolidBrush brush5 = new SolidBrush(Theme.On))
			{
				graphics.FillEllipse(brush5, clientRectangle.Right - 80, 24, 9, 9);
			}
			using (Font font4 = new Font("Segoe UI", 9f, FontStyle.Bold))
			{
				using SolidBrush brush6 = new SolidBrush(Theme.Accent);
				graphics.DrawString("ONLINE", font4, brush6, clientRectangle.Right - 65, 20f);
			}
			using (LinearGradientBrush lineBrush = new LinearGradientBrush(new Rectangle(0, clientRectangle.Height - 2, clientRectangle.Width, 2), Theme.Accent, Color.FromArgb(30, 40, 56), 0f))
			{
				graphics.FillRectangle(lineBrush, 0, clientRectangle.Height - 2, clientRectangle.Width, 2);
			}
		};
		return panel;
	}

	private Control BuildAttachBar()
	{
		Panel obj = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Theme.Back,
			Padding = new Padding(12, 6, 12, 6)
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			BackColor = Theme.Back,
			Padding = new Padding(4, 4, 4, 4)
		};
		Label value = new Label
		{
			Text = "🎯 โปรเซส:",
			ForeColor = Theme.Accent,
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			AutoSize = true,
			Margin = new Padding(0, 6, 8, 0),
			TextAlign = ContentAlignment.MiddleLeft
		};
		_procCombo = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			Width = 280,
			Margin = new Padding(0, 2, 8, 0),
			FlatStyle = FlatStyle.Flat,
			BackColor = Theme.Field,
			ForeColor = Theme.Text,
			Font = new Font("Segoe UI", 9.5f)
		};
		_procCombo.SelectedIndexChanged += OnProcComboChanged;
		Button button = Theme.MakeButton("⟳", 38, 32);
		button.Click += delegate
		{
			RefreshProcesses();
		};
		Button button2 = Theme.MakeButton("⚡ เชื่อมต่อ", 110, 32, accent: true);
		button2.Click += delegate
		{
			Attach();
		};
		flowLayoutPanel.Controls.Add(value);
		flowLayoutPanel.Controls.Add(_procCombo);
		flowLayoutPanel.Controls.Add(button);
		flowLayoutPanel.Controls.Add(button2);
		obj.Controls.Add(flowLayoutPanel);
		return obj;
	}

	private void OnProcComboChanged(object? sender, EventArgs e)
	{
		if (_procCombo.SelectedItem is ComboItem ci && ci.Id > 0)
		{
			_pingEngine.IsAllProcesses = false;
			_pingEngine.TargetPid = ci.Id;
			_pingEngine.TargetProcessName = ci.Name;
		}
		else
		{
			_pingEngine.IsAllProcesses = true;
			_pingEngine.TargetPid = 0;
			_pingEngine.TargetProcessName = "";
		}
		_pingEngine.RefreshPorts();
	}

	private Control BuildBody()
	{
		Panel panel = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Theme.Back
		};
		Panel panel2 = new Panel
		{
			Dock = DockStyle.Left,
			Width = 180,
			BackColor = Theme.Panel2
		};
		Label value = new Label
		{
			Text = "  หมวดหมู่",
			Dock = DockStyle.Top,
			Height = 36,
			ForeColor = Theme.Accent,
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			TextAlign = ContentAlignment.MiddleLeft,
			BackColor = Theme.Panel2
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			BackColor = Theme.Panel2,
			Padding = new Padding(8, 6, 8, 6),
			AutoScroll = true
		};
		Panel panel3 = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Theme.Back
		};
		panel.Controls.Add(panel3);
		panel.Controls.Add(panel2);
		panel2.Controls.Add(flowLayoutPanel);
		panel2.Controls.Add(value);
		AddTab(flowLayoutPanel, panel3, "📊 หน้าหลัก", MakePage(MakeDashboardCard()));
		AddTab(flowLayoutPanel, panel3, "📈 รายงาน", MakePage(MakeReportCard()));
		AddTab(flowLayoutPanel, panel3, "⚙ Automation", MakePage(MakeAutomationCard()));
		AddTab(flowLayoutPanel, panel3, "⚡ ปรับแต่งปิง", MakePage(MakePingControlCard(), MakePingConfigCard(), MakePingProfileCard(), MakePingStatsCard()));
		AddTab(flowLayoutPanel, panel3, "🥊 หมัดจม", MakePage(MakePunchCard()));
		AddTab(flowLayoutPanel, panel3, "💨 ตัวไหล", MakePage(MakeBackCard(), MakeSeCard()));
		AddTab(flowLayoutPanel, panel3, "⭐ เซ็ตทั่วไป", MakePage(MakeActionsCard(), MakeGeneralCard()));
		AddTab(flowLayoutPanel, panel3, "📋 เซ็ตค่า (Preset)", MakePage(MakePresetCard()));
		AddTab(flowLayoutPanel, panel3, "➕ จัดการ AOB", MakePage(MakeCustomAobCard()));
		AddTab(flowLayoutPanel, panel3, "⌨️ ปุ่มกดค้าง", MakePage(MakeHoldCard("ปุ่มกดค้าง — สั่งพุ่ง", "กดค้าง = เปิดสั่งพุ่ง  •  ปล่อย = ปิด", _cheatPush, isPush: true), MakeHoldCard("ปุ่มกดค้าง — สั่งจม", "กดค้าง = เปิดสั่งจม  •  ปล่อย = ปิด", _cheatSink, isPush: false)));
		AddTab(flowLayoutPanel, panel3, "⚙️ ตั้งค่า", MakePage(MakeSettingsCard(), MakeAccountCard()));
		(Button Btn, Panel Page, Panel Bar) tuple = _tabs[0];
		tuple.Btn.BackColor = Theme.NavActive;
		tuple.Btn.ForeColor = Theme.Accent;
		tuple.Bar.BackColor = Theme.Accent;
		tuple.Page.Visible = true;
		return panel;
	}

	private void AddTab(FlowLayoutPanel nav, Panel content, string title, Panel page)
	{
		Button button = Theme.MakeNavButton(title);
		button.Width = nav.Width - 16;
		Panel panel = new Panel
		{
			Size = new Size(3, 26),
			Location = new Point(4, (button.Height - 26) / 2),
			BackColor = Theme.Panel2
		};
		button.Controls.Add(panel);
		button.Tag = page;
		button.Click += delegate(object? s, EventArgs e)
		{
			SelectTab((Button)s);
		};
		nav.Controls.Add(button);
		content.Controls.Add(page);
		page.Visible = false;
		_tabs.Add((button, page, panel));
	}

	private void SelectTab(Button sender)
	{
		foreach (var tab in _tabs)
		{
			Button item = tab.Btn;
			Panel item2 = tab.Page;
			Panel item3 = tab.Bar;
			bool flag = item == sender;
			item.BackColor = (flag ? Theme.NavActive : Color.Transparent);
			item.ForeColor = (flag ? Color.White : Theme.Muted);
			item3.BackColor = (flag ? Theme.Accent : Theme.Panel2);
			item2.Visible = flag;
		}
	}

	private static Panel MakePage(params Panel[] cards)
	{
		Panel panel = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Theme.Back,
			AutoScroll = true,
			Padding = new Padding(10)
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Top,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			WrapContents = false,
			FlowDirection = FlowDirection.TopDown,
			BackColor = Theme.Back,
			Padding = new Padding(2)
		};
		foreach (Panel panel2 in cards)
		{
			panel2.Margin = new Padding(0, 0, 0, 12);
			flowLayoutPanel.Controls.Add(panel2);
		}
		panel.Controls.Add(flowLayoutPanel);
		return panel;
	}

	private Panel MakeCard(string title, Panel inner)
	{
		Panel card = new Panel
		{
			Width = 500,
			BackColor = Theme.Panel
		};
		card.Resize += delegate
		{
			if (card.Region != null)
			{
				card.Region.Dispose();
			}
			card.Region = new Region(Theme.RoundRect(new Rectangle(0, 0, card.Width, card.Height), 12));
		};
		card.Region = new Region(Theme.RoundRect(new Rectangle(0, 0, card.Width, card.Height + 80), 12));
		card.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(Theme.Border);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.DrawPath(pen, Theme.RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12));
		};
		Label value = new Label
		{
			Text = title,
			Dock = DockStyle.Top,
			Height = 40,
			Font = Theme.GroupFont,
			ForeColor = Theme.Accent,
			BackColor = Theme.CardHeader,
			Padding = new Padding(14, 10, 0, 0)
		};
		inner.Location = new Point(14, 50);
		card.Controls.Add(inner);
		card.Controls.Add(value);
		card.Height = inner.Height + 64;
		return card;
	}

	private void InitPingEngine()
	{
		AppSettings s = AppSettings.Current;
		_pingEngine.MinPingMs = s.PingMin;
		_pingEngine.MaxPingMs = s.PingMax;
		_pingEngine.FixedPingMs = s.PingFixedVal;
		_pingEngine.IsFixedMode = s.PingFixedMode;
		_pingEngine.Direction = s.PingDirection;
		_pingEngine.MatchSameProcessNames = s.PingMatchSameName;

		_pingStatsTimer = new System.Windows.Forms.Timer { Interval = 150 };
		_pingStatsTimer.Tick += OnPingStatsTick;
		_pingStatsTimer.Start();
	}

	private Panel MakePingControlCard()
	{
		Panel pnl = new Panel { Width = 472, Height = 130, BackColor = Theme.Panel };

		_lblPingStatus = new Label
		{
			Text = "● สถานะ: ปิดอยู่ (IDLE)",
			Location = new Point(10, 8),
			AutoSize = true,
			Font = new Font("Segoe UI", 10f, FontStyle.Bold),
			ForeColor = Theme.Danger
		};

		_btnPingHero = new Button
		{
			Text = $"▶ เปิดใช้งานปิง (กด {AppSettings.Current.PingHotkey})",
			Location = new Point(10, 36),
			Size = new Size(310, 56),
			FlatStyle = FlatStyle.Flat,
			Font = new Font("Segoe UI", 12f, FontStyle.Bold),
			BackColor = Theme.Accent,
			ForeColor = Color.FromArgb(10, 12, 16),
			Cursor = Cursors.Hand
		};
		_btnPingHero.FlatAppearance.BorderSize = 0;
		_btnPingHero.Click += delegate { TogglePingEngine(); };

		_lblPingVal = new Label
		{
			Text = "0 ms",
			Location = new Point(330, 38),
			Size = new Size(130, 34),
			Font = new Font("Segoe UI", 18f, FontStyle.Bold),
			ForeColor = Theme.Muted,
			TextAlign = ContentAlignment.MiddleCenter
		};
		Label lblSub = new Label
		{
			Text = "ปิงจำลองขณะนี้",
			Location = new Point(330, 74),
			Size = new Size(130, 20),
			Font = Theme.SmallFont,
			ForeColor = Theme.Muted,
			TextAlign = ContentAlignment.MiddleCenter
		};

		_btnPingHk = Theme.MakeButton($"ปุ่มลัด: [{AppSettings.Current.PingHotkey}]", 180, 28);
		_btnPingHk.Location = new Point(10, 98);
		_btnPingHk.Click += delegate { StartPingHotkeyCapture(); };

		Label lblHkHint = new Label
		{
			Text = "💡 กดปุ่มลัดได้ในเกมขณะเล่นแบบเต็มจอ",
			Location = new Point(200, 102),
			AutoSize = true,
			Font = Theme.SmallFont,
			ForeColor = Theme.Muted
		};

		pnl.Controls.Add(_lblPingStatus);
		pnl.Controls.Add(_btnPingHero);
		pnl.Controls.Add(_lblPingVal);
		pnl.Controls.Add(lblSub);
		pnl.Controls.Add(_btnPingHk);
		pnl.Controls.Add(lblHkHint);

		return MakeCard("ควบคุมการปรับปิง (Ping Engine Control)", pnl);
	}

	private Panel MakePingConfigCard()
	{
		Panel pnl = new Panel { Width = 472, Height = 330, BackColor = Theme.Panel };

		int btnW = 105;
		Button b1 = Theme.MakeButton("19 - 37 ms", btnW, 30);
		b1.Location = new Point(10, 6);
		b1.Click += delegate { SetPingPreset(19, 37, false); };

		Button b2 = Theme.MakeButton("45 - 65 ms", btnW, 30);
		b2.Location = new Point(125, 6);
		b2.Click += delegate { SetPingPreset(45, 65, false); };

		Button b3 = Theme.MakeButton("80 - 120 ms", btnW, 30);
		b3.Location = new Point(240, 6);
		b3.Click += delegate { SetPingPreset(80, 120, false); };

		Button b4 = Theme.MakeButton("50 ms คงที่", btnW, 30);
		b4.Location = new Point(355, 6);
		b4.Click += delegate { SetPingPreset(50, 50, true); };

		Label lblMinT = new Label { Text = "Min Ping (ปิงต่ำสุด):", Location = new Point(10, 48), AutoSize = true, ForeColor = Theme.Muted };
		_lblMinPingDisp = new Label { Text = $"{_pingEngine.MinPingMs} ms", Location = new Point(400, 48), Size = new Size(60, 20), TextAlign = ContentAlignment.MiddleRight, ForeColor = Theme.Accent, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
		_tbMinPing = new TrackBar { Location = new Point(10, 70), Size = new Size(450, 30), Minimum = 0, Maximum = 300, Value = _pingEngine.MinPingMs, TickStyle = TickStyle.None };
		_tbMinPing.ValueChanged += delegate
		{
			if (_tbMinPing.Value > _tbMaxPing!.Value) _tbMaxPing.Value = _tbMinPing.Value;
			_pingEngine.MinPingMs = _tbMinPing.Value;
			_lblMinPingDisp.Text = $"{_tbMinPing.Value} ms";
			AppSettings.Current.PingMin = _tbMinPing.Value;
			AppSettings.Save();
		};

		Label lblMaxT = new Label { Text = "Max Ping (ปิงสูงสุด):", Location = new Point(10, 108), AutoSize = true, ForeColor = Theme.Muted };
		_lblMaxPingDisp = new Label { Text = $"{_pingEngine.MaxPingMs} ms", Location = new Point(400, 108), Size = new Size(60, 20), TextAlign = ContentAlignment.MiddleRight, ForeColor = Theme.Accent, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
		_tbMaxPing = new TrackBar { Location = new Point(10, 130), Size = new Size(450, 30), Minimum = 0, Maximum = 300, Value = _pingEngine.MaxPingMs, TickStyle = TickStyle.None };
		_tbMaxPing.ValueChanged += delegate
		{
			if (_tbMaxPing.Value < _tbMinPing!.Value) _tbMinPing.Value = _tbMaxPing.Value;
			_pingEngine.MaxPingMs = _tbMaxPing.Value;
			_lblMaxPingDisp.Text = $"{_tbMaxPing.Value} ms";
			AppSettings.Current.PingMax = _tbMaxPing.Value;
			AppSettings.Save();
		};

		// Jitter slider
		Label lblJitterT = new Label { Text = "Jitter (ความไม่สม่ำเสมอ):", Location = new Point(10, 168), AutoSize = true, ForeColor = Theme.Muted };
		_lblJitterDisp = new Label { Text = $"{AppSettings.Current.JitterMs} ms", Location = new Point(400, 168), Size = new Size(60, 20), TextAlign = ContentAlignment.MiddleRight, ForeColor = Theme.Accent, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
		_tbJitter = new TrackBar { Location = new Point(10, 188), Size = new Size(450, 30), Minimum = 0, Maximum = 100, Value = AppSettings.Current.JitterMs, TickStyle = TickStyle.None };
		_tbJitter.ValueChanged += delegate
		{
			_pingEngine.JitterMs = _tbJitter.Value;
			_lblJitterDisp!.Text = $"{_tbJitter.Value} ms";
			AppSettings.Current.JitterMs = _tbJitter.Value;
			AppSettings.Save();
		};
		_pingEngine.JitterMs = AppSettings.Current.JitterMs;

		// Packet Loss slider
		Label lblLossT = new Label { Text = "Packet Loss (ดรอปแพ็กเก็ต):", Location = new Point(10, 228), AutoSize = true, ForeColor = Theme.Muted };
		_lblPacketLossDisp = new Label { Text = $"{AppSettings.Current.PacketLossPercent}%", Location = new Point(400, 228), Size = new Size(60, 20), TextAlign = ContentAlignment.MiddleRight, ForeColor = Theme.Accent, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
		_tbPacketLoss = new TrackBar { Location = new Point(10, 248), Size = new Size(450, 30), Minimum = 0, Maximum = 30, Value = AppSettings.Current.PacketLossPercent, TickStyle = TickStyle.None };
		_tbPacketLoss.ValueChanged += delegate
		{
			_pingEngine.PacketLossPercent = _tbPacketLoss.Value;
			_lblPacketLossDisp!.Text = $"{_tbPacketLoss.Value}%";
			AppSettings.Current.PacketLossPercent = _tbPacketLoss.Value;
			AppSettings.Save();
		};
		_pingEngine.PacketLossPercent = AppSettings.Current.PacketLossPercent;

		Label lblDir = new Label { Text = "ทิศทาง:", Location = new Point(10, 290), AutoSize = true, ForeColor = Theme.Muted };
		ComboBox cmbDir = new ComboBox
		{
			Location = new Point(65, 286),
			Size = new Size(200, 24),
			DropDownStyle = ComboBoxStyle.DropDownList,
			BackColor = Theme.Field,
			ForeColor = Theme.Text,
			FlatStyle = FlatStyle.Flat
		};
		cmbDir.Items.Add("ทั้งขาเข้าและขาออก (แนะนำ)");
		cmbDir.Items.Add("ขาเข้าอย่างเดียว (Download)");
		cmbDir.Items.Add("ขาออกอย่างเดียว (Upload)");
		cmbDir.SelectedIndex = AppSettings.Current.PingDirection;
		cmbDir.SelectedIndexChanged += delegate
		{
			_pingEngine.Direction = cmbDir.SelectedIndex;
			AppSettings.Current.PingDirection = cmbDir.SelectedIndex;
			AppSettings.Save();
		};

		CheckBox chkSame = new CheckBox
		{
			Text = "หน่วงทุก Process ชื่อเดียวกัน",
			Location = new Point(280, 288),
			AutoSize = true,
			Checked = AppSettings.Current.PingMatchSameName,
			ForeColor = Theme.Muted
		};
		chkSame.CheckedChanged += delegate
		{
			_pingEngine.MatchSameProcessNames = chkSame.Checked;
			AppSettings.Current.PingMatchSameName = chkSame.Checked;
			AppSettings.Save();
		};

		pnl.Controls.Add(b1); pnl.Controls.Add(b2); pnl.Controls.Add(b3); pnl.Controls.Add(b4);
		pnl.Controls.Add(lblMinT); pnl.Controls.Add(_lblMinPingDisp); pnl.Controls.Add(_tbMinPing);
		pnl.Controls.Add(lblMaxT); pnl.Controls.Add(_lblMaxPingDisp); pnl.Controls.Add(_tbMaxPing);
		pnl.Controls.Add(lblJitterT); pnl.Controls.Add(_lblJitterDisp); pnl.Controls.Add(_tbJitter);
		pnl.Controls.Add(lblLossT); pnl.Controls.Add(_lblPacketLossDisp); pnl.Controls.Add(_tbPacketLoss);
		pnl.Controls.Add(lblDir); pnl.Controls.Add(cmbDir); pnl.Controls.Add(chkSame);

		return MakeCard("ระดับความหน่วง (Latency Presets & Range)", pnl);
	}

	private void SetPingPreset(int min, int max, bool fixedMode)
	{
		_pingEngine.IsFixedMode = fixedMode;
		_pingEngine.FixedPingMs = min;
		_tbMinPing!.Value = min;
		_tbMaxPing!.Value = max;
		_pingEngine.MinPingMs = min;
		_pingEngine.MaxPingMs = max;
		_lblMinPingDisp!.Text = $"{min} ms";
		_lblMaxPingDisp!.Text = $"{max} ms";
		AppSettings.Current.PingMin = min;
		AppSettings.Current.PingMax = max;
		AppSettings.Current.PingFixedMode = fixedMode;
		AppSettings.Current.PingFixedVal = min;
		AppSettings.Save();
		ShowMsg($"ปรับระดับปิงเป็น {min} - {max} ms แล้ว");
	}

	private Panel MakePingStatsCard()
	{
		Panel pnl = new Panel { Width = 472, Height = 75, BackColor = Theme.Panel };

		_lblPingPorts = new Label
		{
			Text = "พอร์ตตรวจจับ: ทั้งระบบ (All Ports / Global)",
			Location = new Point(10, 10),
			Size = new Size(450, 20),
			Font = new Font("Segoe UI", 9f),
			ForeColor = Theme.Accent
		};

		_lblPingStats = new Label
		{
			Text = "แพ็กเก็ตหน่วงเวลา (Lagged): 0 | ส่งผ่าน: 0 | ทิ้ง: 0",
			Location = new Point(10, 36),
			Size = new Size(450, 20),
			Font = Theme.SmallFont,
			ForeColor = Theme.Muted
		};

		pnl.Controls.Add(_lblPingPorts);
		pnl.Controls.Add(_lblPingStats);

		return MakeCard("สถานะเครือข่าย Real-Time", pnl);
	}

	private void OnPingStatsTick(object? sender, EventArgs e)
	{
		if (_pingEngine.IsRunning && _lblPingVal != null)
		{
			EngineStats stats = _pingEngine.GetStats();
			_lblPingVal.Text = $"{stats.CurrentSimulatedPing} ms";

			if (_lblPingPorts != null)
			{
				if (_pingEngine.IsAllProcesses)
				{
					_lblPingPorts.Text = "พอร์ตตรวจจับ: ทั้งระบบ (All Ports / Global)";
				}
				else
				{
					_lblPingPorts.Text = stats.ActivePortCount == 0
						? "พอร์ตตรวจจับ: กำลังดักฟังแพ็กเก็ตเกม..."
						: $"พอร์ตตรวจจับ ({stats.ActivePortCount} พอร์ต): {stats.PortsSummary}";
				}
			}

			if (_lblPingStats != null)
			{
				_lblPingStats.Text = $"หน่วงเวลา: {stats.DelayedPackets:N0} | ปกติ: {stats.PassedThroughPackets:N0} | ดรอป: {stats.DroppedPackets:N0}";
			}

			// Update overlay HUD
			string activeSummary = GetActiveCheatsSummary();
			_overlay?.UpdateStats(stats.CurrentSimulatedPing, _pingEngine.IsRunning, "ACTIVE", activeSummary);
		}
		else
		{
			string activeSummary = GetActiveCheatsSummary();
			_overlay?.UpdateStats(0, false, "IDLE", activeSummary);
		}
	}

	private string GetActiveCheatsSummary()
	{
		var onList = _cheats.Where(c => c.IsEnabled).Select(c => c.Name).ToList();
		if (onList.Count == 0) return "ฟังก์ชัน: ปิดหมด";
		if (onList.Count <= 2) return "เปิด: " + string.Join(", ", onList);
		return $"เปิด {onList.Count} ฟังก์ชัน: " + string.Join(", ", onList.Take(2)) + "...";
	}

	private void TogglePingEngine()
	{
		if (_pingEngine.IsRunning)
		{
			_pingEngine.Stop();
			if (_btnPingHero != null)
			{
				_btnPingHero.Text = $"▶ เปิดใช้งานปิง (กด {AppSettings.Current.PingHotkey})";
				_btnPingHero.BackColor = Theme.Accent;
				_btnPingHero.ForeColor = Color.FromArgb(10, 12, 16);
			}
			if (_lblPingStatus != null)
			{
				_lblPingStatus.Text = "● สถานะ: ปิดอยู่ (IDLE)";
				_lblPingStatus.ForeColor = Theme.Danger;
			}
			if (_lblPingVal != null)
			{
				_lblPingVal.Text = "0 ms";
				_lblPingVal.ForeColor = Theme.Muted;
			}
			ShowMsg("ปิดการจำลองปิงแล้ว");
			_activeSessionStartedAt = _cheats.Any(c => c.IsEnabled) ? DateTime.Now : null;
			LogUsage("Ping", "ปิดระบบจำลองปิง");
			SoundHelper.PlayToggle(false);
		}
		else
		{
			try
			{
				_pingEngine.Start();
				if (_btnPingHero != null)
				{
					_btnPingHero.Text = $"⏹ ปิดใช้งานปิง (กด {AppSettings.Current.PingHotkey})";
					_btnPingHero.BackColor = Theme.Danger;
					_btnPingHero.ForeColor = Color.White;
				}
				if (_lblPingStatus != null)
				{
					_lblPingStatus.Text = "● สถานะ: กำลังหน่วงเวลา (ACTIVE)";
					_lblPingStatus.ForeColor = Theme.On;
				}
				if (_lblPingVal != null)
				{
					_lblPingVal.ForeColor = Theme.On;
				}
				ShowMsg("เปิดการจำลองปิงสำเร็จ! (WinDivert Active)");
				_activeSessionStartedAt ??= DateTime.Now;
				LogUsage("Ping", $"เปิดระบบจำลองปิง {AppSettings.Current.PingMin}-{AppSettings.Current.PingMax} ms");
				SoundHelper.PlayToggle(true);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "ข้อผิดพลาดระบบเครือข่าย", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	private void StartPingHotkeyCapture()
	{
		_capturingPingHk = true;
		if (_btnPingHk != null)
		{
			_btnPingHk.Text = "กดปุ่มที่ต้องการ...";
			_btnPingHk.ForeColor = Theme.Danger;
		}
		ShowMsg("กดปุ่มลัดสำหรับเปิด/ปิดปิงบนคีย์บอร์ด (ESC ยกเลิก)");
	}

	private Panel MakePunchCard()
	{
		return MakeCard("🥊 หมัดจม — ระดับการจม", BuildWrapFlow(446, _cheats.Where((Cheat c) => c.Group == "หมัดจม").Select((Func<Cheat, Control>)((Cheat x) => MakeRadio(x))).ToList(), fullWidth: false));
	}

	private Panel MakeBackCard()
	{
		return MakeCard("💨 ตัวไหล — หลัง", BuildWrapFlow(446, _cheats.Where((Cheat c) => c.Group == "ตัวไหล:หลัง").Select((Func<Cheat, Control>)((Cheat x) => MakeRadio(x))).ToList(), fullWidth: false));
	}

	private Panel MakeSeCard()
	{
		return MakeCard("🌀 ตัวไหล — เซ", BuildWrapFlow(446, _cheats.Where((Cheat c) => c.Group == "ตัวไหล:เซ").Select((Func<Cheat, Control>)((Cheat x) => MakeRadio(x))).ToList(), fullWidth: false));
	}

	private Panel MakeActionsCard()
	{
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = Theme.Panel,
			Margin = new Padding(2, 4, 0, 0)
		};
		Button button = Theme.MakeButton("เป\u0e34ดท\u0e31\u0e49งหมด", 130, 34, accent: true);
		button.Click += delegate
		{
			EnableAll();
		};
		Button button2 = Theme.MakeButton("ป\u0e34ดท\u0e31\u0e49งหมด", 130, 34);
		button2.Click += delegate
		{
			DisableAll();
		};
		flowLayoutPanel.Controls.Add(button);
		flowLayoutPanel.Controls.Add(button2);
		return MakeCard("ควบค\u0e38มท\u0e31\u0e49งหมด", flowLayoutPanel);
	}

	private Panel MakeGeneralCard()
	{
		Panel outer = new Panel { Width = 472, BackColor = Theme.Panel };

		// Quick Search Box
		_searchBox = new TextBox
		{
			Location = new Point(10, 6),
			Size = new Size(452, 26),
			BackColor = Theme.Field,
			ForeColor = Theme.Text,
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 9.5f),
			PlaceholderText = "🔍 ค้นหาฟังก์ชัน..."
		};

		var cheatControls = _regular.Select(x => MakeCheck(x)).ToList<Control>();
		Panel cheatFlow = BuildWrapFlow(452, cheatControls, fullWidth: true);
		cheatFlow.Location = new Point(10, 40);

		_searchBox.TextChanged += delegate
		{
			string q = _searchBox.Text.Trim().ToLower();
			for (int i = 0; i < _regular.Count; i++)
			{
				bool visible = string.IsNullOrEmpty(q) || _regular[i].Name.ToLower().Contains(q);
				cheatControls[i].Visible = visible;
			}
		};

		outer.Controls.Add(_searchBox);
		outer.Controls.Add(cheatFlow);
		outer.Height = cheatFlow.Height + 54;

		return MakeCard("เซ็ตทั่วไป", outer);
	}

	private Panel MakeHoldCard(string title, string desc, Cheat cheat, bool isPush)
	{
		AppSettings current = AppSettings.Current;
		bool initial = (isPush ? current.HoldPush : current.HoldSink);
		ToggleRow toggleRow = new ToggleRow("เป\u0e34ดใช\u0e49ป\u0e38\u0e48มกดค\u0e49าง", desc, initial);
		toggleRow.CheckedChanged += delegate(object? x, EventArgs e)
		{
			bool flag = ((ToggleRow)x).Checked;
			if (isPush)
			{
				AppSettings.Current.HoldPush = flag;
			}
			else
			{
				AppSettings.Current.HoldSink = flag;
			}
			AppSettings.Save();
			if (flag)
			{
				WarmHoldCache(cheat);
			}
			else
			{
				DeactivateHold(cheat);
			}
		};
		Button button = Theme.MakeButton("ต\u0e31\u0e49งค\u0e48าป\u0e38\u0e48ม: " + KeyDisplay(isPush ? current.HoldPushKey : current.HoldSinkKey), 426, 34);
		button.Click += delegate
		{
			StartCapture(isPush);
		};
		if (isPush)
		{
			_pushKeyBtn = button;
		}
		else
		{
			_sinkKeyBtn = button;
		}
		Label item = new Label
		{
			Text = "กดค\u0e49าง = เป\u0e34ดฟ\u0e31งก\u0e4cช\u0e31น  •  ปล\u0e48อย = ป\u0e34ดฟ\u0e31งก\u0e4cช\u0e31น  •  กด ESC ยกเล\u0e34ก",
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont,
			TextAlign = ContentAlignment.MiddleLeft,
			BackColor = Color.Transparent
		};
		return MakeCard(title, BuildWrapFlow(446, new List<Control> { toggleRow, button, item }, fullWidth: true));
	}

	private Panel MakeSettingsCard()
	{
		AppSettings current = AppSettings.Current;
		ToggleRow toggleRow = new ToggleRow("เช\u0e37\u0e48อมต\u0e48ออ\u0e31ตโนม\u0e31ต\u0e34", "ค\u0e49นหา FiveM/GTA5 แล\u0e49วเช\u0e37\u0e48อมต\u0e48ออ\u0e31ตโนม\u0e31ต\u0e34", current.AutoAttach);
		toggleRow.CheckedChanged += delegate(object? x, EventArgs e)
		{
			AppSettings.Current.AutoAttach = ((ToggleRow)x).Checked;
			AppSettings.Save();
		};
		ToggleRow toggleRow2 = new ToggleRow("ป\u0e31กหน\u0e49าต\u0e48างบนส\u0e38ด", "ให\u0e49หน\u0e49าต\u0e48างอย\u0e39\u0e48เหน\u0e37อท\u0e38กโปรแกรม", current.AlwaysOnTop);
		toggleRow2.CheckedChanged += delegate(object? x, EventArgs e)
		{
			bool flag = ((ToggleRow)x).Checked;
			AppSettings.Current.AlwaysOnTop = flag;
			AppSettings.Save();
			base.TopMost = flag;
		};
		ToggleRow toggleRow3 = new ToggleRow("เส\u0e35ยงแจ\u0e49งเต\u0e37อน", "เล\u0e48นเส\u0e35ยงเม\u0e37\u0e48อเป\u0e34ด/ป\u0e34ดฟ\u0e31งก\u0e4cช\u0e31น", current.NotifySound);
		toggleRow3.CheckedChanged += delegate(object? x, EventArgs e)
		{
			AppSettings.Current.NotifySound = ((ToggleRow)x).Checked;
			AppSettings.Save();
		};
		ToggleRow toggleRow4 = new ToggleRow("ป\u0e34ดเข\u0e49าถาด", "กด X เพ\u0e37\u0e48อซ\u0e48อนท\u0e35\u0e48ถาดแทนการป\u0e34ดโปรแกรม", current.CloseToTray);
		toggleRow4.CheckedChanged += delegate(object? x, EventArgs e)
		{
			AppSettings.Current.CloseToTray = ((ToggleRow)x).Checked;
			AppSettings.Save();
		};
		ToggleRow toggleRow5 = new ToggleRow("🖥️ Mini Overlay HUD", "แสดงค่าปิงแบบ overlay ในเกม (ลาก-วาง ได้)", current.OverlayEnabled);
		toggleRow5.CheckedChanged += delegate(object? x, EventArgs e)
		{
			bool flag = ((ToggleRow)x).Checked;
			AppSettings.Current.OverlayEnabled = flag;
			AppSettings.Save();
			_overlay?.SetOverlayEnabled(flag);
		};

		// Hotkey setting buttons
		_btnHideHk = Theme.MakeButton($"ปุ่มซ่อน/แสดง: [{current.HideHotkey}]", 426, 32);
		_btnHideHk.Click += delegate
		{
			_capturingHideHk = true;
			_capturingPanicHk = false;
			_btnHideHk.Text = "กดปุ่มที่ต้องการ… (ESC ยกเลิก)";
			ShowMsg("กดปุ่มบนคีย์บอร์ดที่ต้องการใช้ ซ่อน/แสดง โปรแกรม (ESC ยกเลิก)");
		};

		_btnPanicHk = Theme.MakeButton($"ปุ่ม Panic (คืนค่าด่วน): [{current.PanicHotkey}]", 426, 32);
		_btnPanicHk.Click += delegate
		{
			_capturingPanicHk = true;
			_capturingHideHk = false;
			_btnPanicHk.Text = "กดปุ่มที่ต้องการ… (ESC ยกเลิก)";
			ShowMsg("กดปุ่มสำหรับ Panic (คืนค่า Memory ทั้งหมดและปิดปิงทันที)");
		};

		Label lblHkNotice = new Label
		{
			Text = "💡 ปุ่มซ่อนโปรแกรม (Boss Key) และ Panic กดได้ตลอดเวลาแม้เล่นเกมอยู่",
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont,
			AutoSize = true,
			Margin = new Padding(0, 4, 0, 4)
		};

		// Export & Import buttons
		FlowLayoutPanel exportImportRow = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.LeftToRight,
			AutoSize = true,
			Margin = new Padding(0, 6, 0, 4)
		};
		Button btnExport = Theme.MakeButton("📤 ส่งออกการตั้งค่า", 210, 32, accent: true);
		btnExport.Click += delegate
		{
			using var sfd = new SaveFileDialog
			{
				Filter = "MIXANDMIN Settings (*.mixandmin)|*.mixandmin|JSON (*.json)|*.json",
				FileName = $"MIXANDMIN_Backup_{DateTime.Now:yyyyMMdd_HHmm}.mixandmin",
				Title = "ส่งออกการตั้งค่า MIXANDMIN"
			};
			if (sfd.ShowDialog(this) == DialogResult.OK)
			{
				if (AppSettings.ExportToFile(sfd.FileName))
				{
					ShowMsg("ส่งออกการตั้งค่าสำเร็จ!");
					MessageBox.Show("ส่งออกการตั้งค่าสำเร็จ:\n" + sfd.FileName, "MIXANDMIN", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					MessageBox.Show("เกิดข้อผิดพลาดในการส่งออกไฟล์", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		};

		Button btnImport = Theme.MakeButton("📥 นำเข้าการตั้งค่า", 210, 32, accent: false);
		btnImport.Click += delegate
		{
			using var ofd = new OpenFileDialog
			{
				Filter = "MIXANDMIN Settings (*.mixandmin;*.json)|*.mixandmin;*.json",
				Title = "นำเข้าการตั้งค่า MIXANDMIN"
			};
			if (ofd.ShowDialog(this) == DialogResult.OK)
			{
				var dr = MessageBox.Show("การนำเข้าจะเขียนทับการตั้งค่าปัจจุบันทั้งหมด\nต้องการดำเนินการต่อหรือไม่?", "ยืนยันการนำเข้า", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dr == DialogResult.Yes)
				{
					if (AppSettings.ImportFromFile(ofd.FileName))
					{
						ShowMsg("นำเข้าการตั้งค่าสำเร็จ! กรุณาเปิดโปรแกรมใหม่เพื่อให้ค่าทั้งหมดมีผล");
						MessageBox.Show("นำเข้าการตั้งค่าสำเร็จ!\n\nกรุณาปิดและเปิดโปรแกรมใหม่อีกครั้งเพื่อโหลดข้อมูล Preset และ AOB ทั้งหมด", "MIXANDMIN", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					else
					{
						MessageBox.Show("ไฟล์ไม่ถูกต้องหรือเสียหาย", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		};
		exportImportRow.Controls.Add(btnExport);
		exportImportRow.Controls.Add(btnImport);

		List<Control> items = new List<Control> { toggleRow, toggleRow2, toggleRow3, toggleRow4, toggleRow5, _btnHideHk, _btnPanicHk, lblHkNotice, exportImportRow };
		return MakeCard("การตั้งค่า", BuildWrapFlow(446, items, fullWidth: true));
	}

	private Panel MakeAccountCard()
	{
		Panel panel = new Panel
		{
			Width = 446,
			Height = 96,
			BackColor = Theme.Panel
		};
		Label value = new Label
		{
			Text = "เข้าสู่ระบบด้วย: MIXANDMIN",
			ForeColor = Theme.Accent,
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			Location = new Point(6, 4),
			AutoSize = true,
			BackColor = Color.Transparent
		};
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Location = new Point(4, 34),
			AutoSize = true,
			BackColor = Theme.Panel,
			Margin = Padding.Empty
		};
		Button button = Theme.MakeButton("ล\u0e47อกเอาต\u0e4c", 130, 34);
		button.Click += delegate
		{
			Logout();
		};
		Button button2 = Theme.MakeButton("ออกจากโปรแกรม", 140, 34, accent: true);
		button2.Click += delegate
		{
			ExitApp();
		};
		flowLayoutPanel.Controls.Add(button);
		flowLayoutPanel.Controls.Add(button2);
		panel.Controls.Add(flowLayoutPanel);
		panel.Controls.Add(value);
		return MakeCard("บ\u0e31ญช\u0e35และโปรแกรม", panel);
	}

	private static Panel BuildWrapFlow(int width, List<Control> items, bool fullWidth)
	{
		Panel panel = new Panel
		{
			Width = width,
			Height = 0,
			BackColor = Theme.Panel
		};
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (Control item in items)
		{
			item.AutoSize = false;
			if ((item is CheckBox || item is RadioButton) ? true : false)
			{
				item.Height = 26;
			}
			if (fullWidth)
			{
				item.Width = width;
				item.Location = new Point(0, num2);
				num2 += item.Height + 2;
				continue;
			}
			int num4 = item.PreferredSize.Width + 18;
			if (num4 > width)
			{
				num4 = width;
			}
			if (num + num4 > width)
			{
				num = 0;
				num2 += num3 + 2;
				num3 = 0;
			}
			item.Location = new Point(num, num2);
			item.Width = num4;
			num += num4 + 6;
			num3 = Math.Max(num3, item.Height);
		}
		panel.Height = ((!fullWidth) ? (num2 + num3) : ((num2 > 0) ? (num2 - 2) : 26));
		panel.SuspendLayout();
		foreach (Control item2 in items)
		{
			panel.Controls.Add(item2);
		}
		panel.ResumeLayout();
		return panel;
	}

	private Control BuildStatusBar()
	{
		Panel obj = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Theme.Panel2
		};
		_statusLabel = new Label
		{
			Dock = DockStyle.Fill,
			ForeColor = Theme.Muted,
			BackColor = Theme.Panel2,
			Text = "พร\u0e49อมใช\u0e49งาน — เล\u0e37อกโปรเซสแล\u0e49วกด เช\u0e37\u0e48อมต\u0e48อ",
			TextAlign = ContentAlignment.MiddleLeft,
			Padding = new Padding(10, 0, 0, 0)
		};
		obj.Controls.Add(_statusLabel);
		return obj;
	}

	private void ShowMsg(string msg)
	{
		if (_statusLabel != null)
		{
			_statusLabel.Text = msg;
		}
	}

	private void BuildTray()
	{
		_tray = new NotifyIcon
		{
			Icon = IconFactory.CreateAppIcon(),
			Text = "MIXANDMIN"
		};
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		contextMenuStrip.Items.Add("แสดงหน้าต่าง", null, delegate
		{
			ShowWindow();
		});
		contextMenuStrip.Items.Add("⚡ เปิด/ปิดปิง", null, delegate
		{
			TogglePingEngine();
		});
		contextMenuStrip.Items.Add(new ToolStripSeparator());
		contextMenuStrip.Items.Add("ล็อกเอาต์", null, delegate
		{
			Logout();
		});
		contextMenuStrip.Items.Add("ออกจากโปรแกรม", null, delegate
		{
			ExitApp();
		});
		_tray.ContextMenuStrip = contextMenuStrip;
		_tray.DoubleClick += delegate
		{
			ShowWindow();
		};
		_tray.Visible = false;
	}

	private void ShowWindow()
	{
		Show();
		base.WindowState = FormWindowState.Normal;
		BringToFront();
		Activate();
		if (_tray != null)
		{
			_tray.Visible = false;
		}
	}

	private void HideToTray()
	{
		Hide();
		if (_tray == null)
		{
			return;
		}
		_tray.Visible = true;
		try
		{
			_tray.ShowBalloonTip(2500, "MIXANDMIN", "โปรแกรมยังทำงานอยู่เบื้องหลัง", ToolTipIcon.Info);
		}
		catch
		{
		}
	}

	private void ToggleVisibility()
	{
		if (Visible)
		{
			Hide();
			if (_tray != null) _tray.Visible = true;
			ShowMsg("ซ่อนโปรแกรมแล้ว (กดปุ่มลัดเพื่อแสดงอีกครั้ง)");
			SoundHelper.PlayToggle(false);
		}
		else
		{
			ShowWindow();
			ShowMsg("แสดงโปรแกรมแล้ว");
			SoundHelper.PlayToggle(true);
		}
	}

	private void EmergencyPanic()
	{
		// 1. Restore all memory immediately
		DisableAll();

		// 2. Stop ping engine if active
		if (_pingEngine.IsRunning)
		{
			TogglePingEngine();
		}

		ShowMsg("🚨 PANIC! คืนค่า Memory และปิดปิงทั้งหมดทันทีแล้ว");
		_activeSessionStartedAt = null;
		_cheatSessionStartedAt = null;
		LogUsage("Panic", "คืนค่าทั้งหมดและหยุดระบบทันที");
		SoundHelper.PlayToggle(false);
	}

	private void OnShown(object s, EventArgs e)
	{
		if (AppSettings.Current.AutoAttach)
		{
			AutoAttach();
		}
	}

	private void OnFormClosing(object s, FormClosingEventArgs e)
	{
		if (!_isExiting && AppSettings.Current.CloseToTray)
		{
			e.Cancel = true;
			HideToTray();
		}
	}

	private void ApplySettings()
	{
		base.TopMost = AppSettings.Current.AlwaysOnTop;
	}

	private void Logout()
	{
		_isExiting = true;
		if (_tray != null)
		{
			_tray.Visible = false;
			_tray.Dispose();
			_tray = null;
		}
		Cleanup();
		Close();
		LogoutRequested?.Invoke();
	}

	private void ExitApp()
	{
		_isExiting = true;
		Application.Exit();
	}

	private void OpenCheatEditor(Cheat c)
	{
		using var dlg = new CheatEditorDialog(c);
		dlg.ShowDialog(this);
		if (dlg.ValueChanged)
		{
			ShowMsg($"อัปเดตค่าของ [{c.Name}] แล้ว (AOB: {c.PatchHex})");
		}
	}

	private ContextMenuStrip CreateCheatContextMenu(Cheat c)
	{
		ContextMenuStrip cms = new ContextMenuStrip();
		var itemEdit = cms.Items.Add($"⚙️ แก้ไขค่า ({c.Name})...");
		itemEdit.Click += delegate { OpenCheatEditor(c); };

		var itemRestore = cms.Items.Add("🔄 คืนค่าเดิม");
		itemRestore.Click += delegate
		{
			c.PatchHex = c.DefaultPatchHex;
			c.Patch = CheatRegistry.H(c.DefaultPatchHex);
			if (AppSettings.Current.CheatOverrides != null)
			{
				AppSettings.Current.CheatOverrides.Remove(c.Name);
				AppSettings.Save();
			}
			if (c.IsEnabled && c.Handle != IntPtr.Zero)
			{
				foreach (var patch in c.Patches)
				{
					MemoryHelper.Write(c.Handle, patch.Address, c.Patch);
				}
			}
			ShowMsg($"คืนค่าเริ่มต้นของ [{c.Name}] แล้ว");
			SoundHelper.PlayToggle(false);
		};
		return cms;
	}

	private CheckBox MakeCheck(Cheat c)
	{
		CheckBox chk = new CheckBox
		{
			Text = c.Name,
			Tag = c,
			FlatStyle = FlatStyle.Flat,
			BackColor = Theme.Panel,
			ForeColor = Theme.Text,
			Cursor = Cursors.Hand,
			ContextMenuStrip = CreateCheatContextMenu(c)
		};
		chk.CheckedChanged += delegate
		{
			chk.ForeColor = (chk.Checked ? Theme.Accent : Theme.Text);
			if (!_busy.Contains(c))
			{
				ToggleCheatAsync(c, chk.Checked);
			}
		};
		chk.MouseUp += delegate(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				chk.ContextMenuStrip?.Show(chk, e.Location);
			}
		};
		chk.DoubleClick += delegate
		{
			OpenCheatEditor(c);
		};
		_ctrl[c] = chk;
		return chk;
	}

	private RadioButton MakeRadio(Cheat c)
	{
		RadioButton rb = new RadioButton
		{
			Text = c.Name,
			Tag = c,
			FlatStyle = FlatStyle.Flat,
			BackColor = Theme.Panel,
			ForeColor = Theme.Text,
			Cursor = Cursors.Hand,
			ContextMenuStrip = CreateCheatContextMenu(c)
		};
		rb.CheckedChanged += Radio_CheckedChanged;
		rb.CheckedChanged += delegate
		{
			rb.ForeColor = (rb.Checked ? Theme.Accent : Theme.Text);
		};
		rb.MouseUp += delegate(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				rb.ContextMenuStrip?.Show(rb, e.Location);
			}
		};
		rb.DoubleClick += delegate
		{
			OpenCheatEditor(c);
		};
		_ctrl[c] = rb;
		return rb;
	}

	private async void Radio_CheckedChanged(object sender, EventArgs e)
	{
		RadioButton radioButton = (RadioButton)sender;
		if (!radioButton.Checked)
		{
			return;
		}
		Cheat c = (Cheat)radioButton.Tag;
		if (!_busy.Contains(c))
		{
			if (_groupActive.TryGetValue(c.Group, out var value) && value != c && value.IsEnabled)
			{
				_groupActive.Remove(c.Group);
				await ToggleCheatAsync(value, enable: false);
			}
			_groupActive[c.Group] = c;
			await ToggleCheatAsync(c, enable: true);
		}
	}

	private async Task ToggleCheatAsync(Cheat c, bool enable, bool quiet = false)
	{
		bool wasEnabled = c.IsEnabled;
		if ((enable && c.IsEnabled) || (!enable && !c.IsEnabled))
		{
			return;
		}
		if (enable && _procHandle == IntPtr.Zero)
		{
			if (!quiet)
			{
				ShowMsg("กร\u0e38ณาเช\u0e37\u0e48อมต\u0e48อโปรเซสก\u0e48อน");
			}
			SetControlChecked(c, val: false);
		}
		else
		{
			if (_busy.Contains(c))
			{
				return;
			}
			_busy.Add(c);
			await _toggleLock.WaitAsync();
			try
			{
				if (enable)
				{
					nint handle = _procHandle;
					if (await Task.Run(() => c.Enable(handle)))
					{
						if (c.LockIntervalMs > 0)
						{
							StartLockTimer(c, handle);
						}
						if (!quiet)
						{
							ShowMsg(c.LastStatus);
						}
						SetControlChecked(c, val: true);
					}
					else
					{
						if (!quiet)
						{
							ShowMsg(c.LastStatus);
						}
						SetControlChecked(c, val: false);
					}
				}
				else
				{
					bool val = await Task.Run(() => c.Disable());
					if (!quiet)
					{
						ShowMsg(c.LastStatus);
					}
					SetControlChecked(c, val);
				}
				if (!quiet && AppSettings.Current.NotifySound)
				{
					SoundHelper.PlayToggle(enable);
				}
			}
			catch (Exception ex)
			{
				if (!quiet)
				{
					ShowMsg("[" + c.Name + "] ข\u0e49อผ\u0e34ดพลาด: " + ex.Message);
				}
				SetControlChecked(c, val: false);
			}
			finally
			{
				if (c.IsEnabled != wasEnabled)
				{
					_cheatSessionStartedAt = _cheats.Any(x => x.IsEnabled) ? (_cheatSessionStartedAt ?? DateTime.Now) : null;
					_activeSessionStartedAt = (_pingEngine.IsRunning || _cheats.Any(x => x.IsEnabled)) ? (_activeSessionStartedAt ?? DateTime.Now) : null;
					LogUsage("Feature", $"{c.Name} {(c.IsEnabled ? "ON" : "OFF")}");
				}
				_toggleLock.Release();
				_busy.Remove(c);
			}
		}
	}

	private void StartLockTimer(Cheat c, nint handle)
	{
		if (c.LockTimer == null)
		{
			c.LockTimer = new System.Windows.Forms.Timer
			{
				Interval = c.LockIntervalMs
			};
			c.LockTimer.Tick += delegate
			{
				foreach (PatchedAddress patch in c.Patches)
				{
					MemoryHelper.Write(handle, patch.Address, c.Patch);
				}
			};
		}
		c.LockTimer.Start();
	}

	private void SetControlChecked(Cheat c, bool val)
	{
		if (_ctrl.TryGetValue(c, out var value))
		{
			if (value is CheckBox checkBox)
			{
				checkBox.Checked = val;
			}
			else if (value is RadioButton radioButton)
			{
				radioButton.Checked = val;
			}
		}
	}

	private void EnableAll()
	{
		foreach (Cheat item in _regular.Where((Cheat x) => !x.IsEnabled))
		{
			ToggleCheatAsync(item, enable: true);
		}
		LogUsage("Action", "สั่งเปิดฟังก์ชันทั้งหมด");
	}

	private void DisableAll()
	{
		foreach (Cheat item in _cheats.Where((Cheat x) => x.IsEnabled))
		{
			ToggleCheatAsync(item, enable: false);
		}
		_cheatSessionStartedAt = null;
		_activeSessionStartedAt = _pingEngine.IsRunning ? DateTime.Now : null;
		LogUsage("Action", "สั่งปิดฟังก์ชันทั้งหมด");
	}

	private static string KeyDisplay(Keys key)
	{
		switch (key)
		{
		case Keys.None:
			return "ย\u0e31งไม\u0e48ได\u0e49ต\u0e31\u0e49ง";
		case Keys.D0:
		case Keys.D1:
		case Keys.D2:
		case Keys.D3:
		case Keys.D4:
		case Keys.D5:
		case Keys.D6:
		case Keys.D7:
		case Keys.D8:
		case Keys.D9:
			return ((char)(48 + (key - 48))).ToString();
		default:
			if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
			{
				return "Num" + (int)(key - 96);
			}
			if (key >= Keys.A && key <= Keys.Z)
			{
				return ((char)(65 + (key - 65))).ToString();
			}
			return key.ToString();
		}
	}

	private void StartCapture(bool isPush)
	{
		_capturingPush = isPush;
		_capturingSink = !isPush;
		UpdateCaptureButtons();
		ShowMsg("กดป\u0e38\u0e48มท\u0e35\u0e48ต\u0e49องการบนค\u0e35ย\u0e4cบอร\u0e4cด (กด ESC เพ\u0e37\u0e48อยกเล\u0e34ก)");
	}

	private void UpdateCaptureButtons()
	{
		if (_pushKeyBtn != null)
		{
			_pushKeyBtn.Text = (_capturingPush ? "กดปุ่มที่ต้องการ… (ESC ยกเลิก)" : ("ตั้งค่าปุ่ม: " + KeyDisplay(AppSettings.Current.HoldPushKey)));
		}
		if (_sinkKeyBtn != null)
		{
			_sinkKeyBtn.Text = (_capturingSink ? "กดปุ่มที่ต้องการ… (ESC ยกเลิก)" : ("ตั้งค่าปุ่ม: " + KeyDisplay(AppSettings.Current.HoldSinkKey)));
		}
		if (_btnPingHk != null)
		{
			_btnPingHk.Text = _capturingPingHk ? "กดปุ่มที่ต้องการ..." : $"ปุ่มลัด: [{AppSettings.Current.PingHotkey}]";
		}
		if (_btnHideHk != null)
		{
			_btnHideHk.Text = _capturingHideHk ? "กดปุ่มที่ต้องการ..." : $"ปุ่มซ่อน/แสดง: [{AppSettings.Current.HideHotkey}]";
		}
		if (_btnPanicHk != null)
		{
			_btnPanicHk.Text = _capturingPanicHk ? "กดปุ่มที่ต้องการ..." : $"ปุ่ม Panic: [{AppSettings.Current.PanicHotkey}]";
		}
	}

	private void HandleCaptureKey(Keys key)
	{
		if (_capturingHideHk)
		{
			_capturingHideHk = false;
			if (key != Keys.Escape)
			{
				AppSettings.Current.HideHotkey = key;
				AppSettings.Save();
				ShowMsg($"บันทึกปุ่มซ่อน/แสดงโปรแกรม: {key}");
			}
			else
			{
				ShowMsg("ยกเลิกการตั้งค่าปุ่มซ่อนโปรแกรม");
			}
			UpdateCaptureButtons();
			return;
		}

		if (_capturingPanicHk)
		{
			_capturingPanicHk = false;
			if (key != Keys.Escape)
			{
				AppSettings.Current.PanicHotkey = key;
				AppSettings.Save();
				ShowMsg($"บันทึกปุ่ม Panic คืนค่าด่วน: {key}");
			}
			else
			{
				ShowMsg("ยกเลิกการตั้งค่าปุ่ม Panic");
			}
			UpdateCaptureButtons();
			return;
		}

		if (_capturingPingHk)
		{
			_capturingPingHk = false;
			if (key != Keys.Escape)
			{
				AppSettings.Current.PingHotkey = key;
				AppSettings.Save();
				if (_btnPingHero != null)
				{
					_btnPingHero.Text = _pingEngine.IsRunning
						? $"⏹ ปิดใช้งานปิง (กด {key})"
						: $"▶ เปิดใช้งานปิง (กด {key})";
				}
				ShowMsg($"บันทึกปุ่มลัดปิงแล้ว: {key}");
			}
			else
			{
				ShowMsg("ยกเลิกการตั้งค่าปุ่มลัดปิง");
			}
			UpdateCaptureButtons();
			return;
		}

		bool capturingPush = _capturingPush;
		_capturingPush = false;
		_capturingSink = false;
		if (key == Keys.Escape)
		{
			UpdateCaptureButtons();
			ShowMsg("ยกเลิกการตั้งค่าปุ่มแล้ว");
			return;
		}
		if ((capturingPush ? AppSettings.Current.HoldSinkKey : AppSettings.Current.HoldPushKey) == key)
		{
			_capturingPush = capturingPush;
			_capturingSink = !capturingPush;
			UpdateCaptureButtons();
			ShowMsg("ปุ่มนี้ถูกใช้กับหมวดอื่นแล้ว — ลองปุ่มอื่น");
			return;
		}
		if (capturingPush)
		{
			AppSettings.Current.HoldPushKey = key;
		}
		else
		{
			AppSettings.Current.HoldSinkKey = key;
		}
		AppSettings.Save();
		UpdateCaptureButtons();
		WarmHoldCache(capturingPush ? _cheatPush : _cheatSink);
		ShowMsg("บันทึกปุ่มแล้ว: " + KeyDisplay(key));
	}

	private void OnGlobalKeyDown(Keys key)
	{
		if (_capturingPush || _capturingSink || _capturingPingHk || _capturingHideHk || _capturingPanicHk)
		{
			HandleCaptureKey(key);
		}
		else if (_pressed.Add(key))
		{
			// Boss Key: Toggle Hide / Show Window
			if (key == AppSettings.Current.HideHotkey && key != Keys.None)
			{
				ToggleVisibility();
				return;
			}

			// Panic Key: Restore all memory and stop ping immediately
			if (key == AppSettings.Current.PanicHotkey && key != Keys.None)
			{
				EmergencyPanic();
				return;
			}

			if (key == AppSettings.Current.PingHotkey)
			{
				TogglePingEngine();
			}
			if (AppSettings.Current.HoldPush && key == AppSettings.Current.HoldPushKey)
			{
				_holdWanted[_cheatPush] = true;
				ReconcileHoldAsync(_cheatPush);
			}
			if (AppSettings.Current.HoldSink && key == AppSettings.Current.HoldSinkKey)
			{
				_holdWanted[_cheatSink] = true;
				ReconcileHoldAsync(_cheatSink);
			}
		}
	}

	private void OnGlobalKeyUp(Keys key)
	{
		if (_pressed.Remove(key))
		{
			if (AppSettings.Current.HoldPush && key == AppSettings.Current.HoldPushKey)
			{
				_holdWanted[_cheatPush] = false;
				ReconcileHoldAsync(_cheatPush);
			}
			if (AppSettings.Current.HoldSink && key == AppSettings.Current.HoldSinkKey)
			{
				_holdWanted[_cheatSink] = false;
				ReconcileHoldAsync(_cheatSink);
			}
		}
	}

	private async Task ReconcileHoldAsync(Cheat c)
	{
		if (_busy.Contains(c))
		{
			return;
		}
		bool flag = _holdWanted.TryGetValue(c, out var value) & value;
		if (flag != c.IsEnabled)
		{
			await ToggleCheatAsync(c, flag, quiet: true);
			bool flag2 = _holdWanted.TryGetValue(c, out var value2) & value2;
			if (flag2 != c.IsEnabled && !_busy.Contains(c))
			{
				await ToggleCheatAsync(c, flag2, quiet: true);
			}
		}
	}

	private void WarmHoldCache(Cheat c)
	{
		if (_procHandle != IntPtr.Zero)
		{
			Task.Run(delegate
			{
				c.WarmCache(_procHandle);
			});
		}
	}

	private void DeactivateHold(Cheat c)
	{
		_holdWanted[c] = false;
		if (c.IsEnabled)
		{
			ToggleCheatAsync(c, enable: false, quiet: true);
		}
	}

	private void RefreshProcesses()
	{
		_procCombo.Items.Clear();
		_procCombo.Items.Add(new ComboItem(0, "🌐 ทั้งระบบ (All Processes / Global)"));

		Process[] processes;
		try
		{
			processes = Process.GetProcesses();
		}
		catch
		{
			return;
		}
		List<(int, string)> list = new List<(int, string)>();
		Process[] array = processes;
		foreach (Process process in array)
		{
			try
			{
				list.Add((process.Id, process.ProcessName));
			}
			catch
			{
			}
			try
			{
				process.Dispose();
			}
			catch
			{
			}
		}
		int num = -1;
		foreach (var (id, name) in list.OrderBy<(int, string), string>(((int id, string name) x) => x.name))
		{
			_procCombo.Items.Add(new ComboItem(id, name));
			if (num == -1 && IsCandidate(name))
			{
				num = _procCombo.Items.Count - 1;
			}
		}
		if (num >= 0)
		{
			_procCombo.SelectedIndex = num;
		}
		else
		{
			_procCombo.SelectedIndex = 0;
		}
	}

	private static bool IsCandidate(string name)
	{
		string text = name.ToLowerInvariant();
		if (!text.Contains("five"))
		{
			return text.Contains("gta");
		}
		return true;
	}

	private void AutoAttach()
	{
		RefreshProcesses();
		if (_procCombo.SelectedItem is ComboItem ci && ci.Id > 0)
		{
			Attach();
		}
	}

	private void Attach()
	{
		if (!(_procCombo.SelectedItem is ComboItem comboItem))
		{
			ShowMsg("ไม่พบโปรเซส — ลองกด ⟳ รีเฟรช");
			return;
		}
		if (comboItem.Id == 0)
		{
			_pingEngine.IsAllProcesses = true;
			_pingEngine.TargetPid = 0;
			_pingEngine.TargetProcessName = "";
			_pingEngine.RefreshPorts();
			ShowMsg("ตั้งค่าเป้าหมาย: ทั้งระบบ (Global)");
			return;
		}

		DisableAllSync();
		if (_procHandle != IntPtr.Zero)
		{
			MemoryHelper.CloseHandle(_procHandle);
			_procHandle = IntPtr.Zero;
		}
		try
		{
			_procHandle = MemoryHelper.OpenProcessSafe(comboItem.Id);
			foreach (Cheat cheat in _cheats)
			{
				cheat.ClearCache();
			}

			_pingEngine.IsAllProcesses = false;
			_pingEngine.TargetPid = comboItem.Id;
			_pingEngine.TargetProcessName = comboItem.Name;
			_pingEngine.RefreshPorts();
			LogUsage("Attach", $"{comboItem.Name} (PID {comboItem.Id})");
			if (AppSettings.Current.AutoStartPingOnAttach && !_pingEngine.IsRunning)
			{
				TogglePingEngine();
			}

			ShowMsg($"เชื่อมต่อแล้ว → {comboItem.Name} (PID {comboItem.Id})");
		}
		catch (Exception ex)
		{
			_procHandle = IntPtr.Zero;
			ShowMsg("เชื่อมต่อไม่สำเร็จ: " + ex.Message);
		}
	}

	private void DisableAllSync()
	{
		foreach (Cheat cheat in _cheats)
		{
			try
			{
				if (cheat.IsEnabled)
				{
					cheat.Disable();
				}
			}
			catch
			{
			}
		}
	}

	private void Cleanup()
	{
		_keyboardHook.Uninstall();
		_pingStatsTimer?.Stop();
		_reattachTimer?.Stop();
		_dashboardTimer?.Stop();
		_automationTimer?.Stop();
		_pingEngine.Stop();
		DisableAllSync();
		if (_procHandle != IntPtr.Zero)
		{
			MemoryHelper.CloseHandle(_procHandle);
			_procHandle = IntPtr.Zero;
		}
		if (_overlay != null && !_overlay.IsDisposed)
		{
			_overlay.Close();
			_overlay = null;
		}
	}

	// Auto re-attach: detect if the attached process died, then attempt to re-attach
	private void OnReattachTick(object? sender, EventArgs e)
	{
		if (!AppSettings.Current.AutoAttach) return;
		if (_procHandle != IntPtr.Zero) return; // already attached

		// Try auto-attach silently
		RefreshProcesses();
		if (_procCombo.SelectedItem is ComboItem ci && ci.Id > 0)
		{
			Attach();
		}
	}

	// Profile Presets card for ping tab
	private Panel MakePingProfileCard()
	{
		Panel pnl = new Panel { Width = 472, Height = 90, BackColor = Theme.Panel };

		Label lblHint = new Label
		{
			Text = "เลือกโปรไฟล์ปิงสำเร็จรูป:",
			Location = new Point(10, 8),
			AutoSize = true,
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont
		};
		pnl.Controls.Add(lblHint);

		int btnX = 10;
		int idx = 0;
		foreach (var profile in AppSettings.Current.Profiles)
		{
			var p = profile; // capture
			int capturedIdx = idx++;
			Button btn = Theme.MakeButton(p.Name, 140, 30);
			btn.Location = new Point(btnX, 32);
			btn.Click += delegate
			{
				_pingEngine.MinPingMs = p.PingMin;
				_pingEngine.MaxPingMs = p.PingMax;
				_pingEngine.JitterMs = p.JitterMs;
				_pingEngine.PacketLossPercent = p.PacketLossPercent;
				if (_tbMinPing != null) _tbMinPing.Value = Math.Max(0, Math.Min(300, p.PingMin));
				if (_tbMaxPing != null) _tbMaxPing.Value = Math.Max(0, Math.Min(300, p.PingMax));
				if (_tbJitter != null) _tbJitter.Value = Math.Max(0, Math.Min(100, p.JitterMs));
				if (_tbPacketLoss != null) _tbPacketLoss.Value = Math.Max(0, Math.Min(30, p.PacketLossPercent));
				AppSettings.Current.PingMin = p.PingMin;
				AppSettings.Current.PingMax = p.PingMax;
				AppSettings.Current.JitterMs = p.JitterMs;
				AppSettings.Current.PacketLossPercent = p.PacketLossPercent;
				AppSettings.Current.ActiveProfileIndex = capturedIdx;
				AppSettings.Save();
				ShowMsg($"โหลดโปรไฟล์: {p.Name} ({p.PingMin}-{p.PingMax} ms)");
				SoundHelper.PlayToggle(true);
			};
			pnl.Controls.Add(btn);
			btnX += 148;
		}

		// Save current as new profile
		Button btnSave = Theme.MakeButton("💾 บันทึกปัจจุบัน", 140, 30, accent: true);
		btnSave.Location = new Point(10, 58);
		btnSave.Click += delegate
		{
			var newProfile = new PingProfile
			{
				Name = $"Custom {AppSettings.Current.PingMin}-{AppSettings.Current.PingMax}",
				PingMin = AppSettings.Current.PingMin,
				PingMax = AppSettings.Current.PingMax,
				JitterMs = AppSettings.Current.JitterMs,
				PacketLossPercent = AppSettings.Current.PacketLossPercent
			};
			AppSettings.Current.Profiles.Add(newProfile);
			AppSettings.Save();
			ShowMsg($"บันทึกโปรไฟล์แล้ว: {newProfile.Name}");
		};
		pnl.Controls.Add(btnSave);

		return MakeCard("🗂️ โปรไฟล์ปิงสำเร็จรูป (Presets)", pnl);
	}

	// Custom AOB Manager Card
	private Panel MakeCustomAobCard()
	{
		Panel pnl = new Panel { Width = 472, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Theme.Panel };

		Label lblName = new Label { Text = "ชื่อฟังก์ชัน:", Location = new Point(10, 10), AutoSize = true, ForeColor = Theme.Text, Font = Theme.SmallFont };
		TextBox txtName = new TextBox { Location = new Point(10, 28), Width = 450, BackColor = Theme.Field, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "เช่น หมัดพลังสูง, วิ่งเร็วพิเศษ" };

		Label lblSearch = new Label { Text = "AOB Search (Hex ค้นหา เช่น 00 00 80 3F ?? ?? CD):", Location = new Point(10, 58), AutoSize = true, ForeColor = Theme.Text, Font = Theme.SmallFont };
		TextBox txtSearch = new TextBox { Location = new Point(10, 76), Width = 450, BackColor = Theme.Field, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "วาง Hex หรือ Pattern ที่นี่" };

		Label lblPatch = new Label { Text = "AOB Patch (Hex ที่จะแทนที่):", Location = new Point(10, 106), AutoSize = true, ForeColor = Theme.Text, Font = Theme.SmallFont };
		TextBox txtPatch = new TextBox { Location = new Point(10, 124), Width = 450, BackColor = Theme.Field, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "วาง Patch Hex ที่นี่" };

		CheckBox chkPatchAll = new CheckBox { Text = "Patch ทุกตำแหน่งที่เจอ (PatchAll)", Location = new Point(10, 154), AutoSize = true, ForeColor = Theme.Muted };

		Button btnAdd = Theme.MakeButton("➕ เพิ่มและเปิดใช้งานฟังก์ชัน", 220, 32, accent: true);
		btnAdd.Location = new Point(10, 182);

		Label lblListHeader = new Label { Text = "รายการ Custom AOB ที่บันทึกไว้:", Location = new Point(10, 224), AutoSize = true, ForeColor = Theme.Accent, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };

		_customAobFlow = new FlowLayoutPanel
		{
			Location = new Point(10, 248),
			Width = 452,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			BackColor = Theme.Panel
		};

		Action refreshCustomList = () =>
		{
			_customAobFlow.Controls.Clear();
			if (AppSettings.Current.CustomAobs.Count == 0)
			{
				Label empty = new Label { Text = "(ยังไม่มี Custom AOB ที่เพิ่มไว้)", ForeColor = Theme.Muted, AutoSize = true, Margin = new Padding(4) };
				_customAobFlow.Controls.Add(empty);
				return;
			}

			foreach (var item in AppSettings.Current.CustomAobs.ToList())
			{
				Panel row = new Panel { Width = 444, Height = 34, BackColor = Theme.Field, Margin = new Padding(0, 0, 0, 4) };
				Label title = new Label { Text = item.Name, Location = new Point(8, 8), Width = 280, ForeColor = Theme.Text, AutoEllipsis = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
				
				Button delBtn = Theme.MakeButton("ลบ", 60, 24);
				delBtn.Location = new Point(370, 4);
				delBtn.Click += delegate
				{
					AppSettings.Current.CustomAobs.Remove(item);
					AppSettings.Save();
					// Disable cheat if loaded
					var found = _cheats.FirstOrDefault(c => c.Name == item.Name);
					if (found != null)
					{
						if (found.IsEnabled) found.Disable();
						_cheats.Remove(found);
						_regular.Remove(found);
					}
					ShowMsg($"ลบฟังก์ชัน {item.Name} แล้ว");
					_customAobFlow.SuspendLayout();
					// refresh
					var refreshAction = _customAobFlow.Tag as Action;
					refreshAction?.Invoke();
					_customAobFlow.ResumeLayout();
				};

				row.Controls.Add(title);
				row.Controls.Add(delBtn);
				_customAobFlow.Controls.Add(row);
			}
		};

		_customAobFlow.Tag = refreshCustomList;
		refreshCustomList();

		btnAdd.Click += delegate
		{
			string name = txtName.Text.Trim();
			string search = txtSearch.Text.Trim();
			string patch = txtPatch.Text.Trim();
			if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(search) || string.IsNullOrEmpty(patch))
			{
				MessageBox.Show("กรุณากรอกข้อมูลให้ครบถ้วน ทั้งชื่อ, AOB Search, และ AOB Patch", "MIXANDMIN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				// Test hex parsing
				Cheat newCheat = CheatRegistry.Make(name, search, patch, null, chkPatchAll.Checked, 0);

				var newItem = new CustomAobItem
				{
					Name = name,
					SearchAob = search,
					PatchAob = patch,
					PatchAll = chkPatchAll.Checked,
					LockMs = 0
				};

				AppSettings.Current.CustomAobs.Add(newItem);
				AppSettings.Save();

				_cheats.Add(newCheat);
				_regular.Add(newCheat);

				txtName.Clear();
				txtSearch.Clear();
				txtPatch.Clear();
				chkPatchAll.Checked = false;

				refreshCustomList();
				ShowMsg($"เพิ่มฟังก์ชัน AOB สำเร็จ: {name} (เปิดใช้งานได้ในแท็บเซ็ตทั่วไป)");
				SoundHelper.PlayToggle(true);
			}
			catch (Exception ex)
			{
				MessageBox.Show("รูปแบบ Hex ไม่ถูกต้อง: " + ex.Message, "ข้อผิดพลาด AOB", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		};

		pnl.Controls.Add(lblName);
		pnl.Controls.Add(txtName);
		pnl.Controls.Add(lblSearch);
		pnl.Controls.Add(txtSearch);
		pnl.Controls.Add(lblPatch);
		pnl.Controls.Add(txtPatch);
		pnl.Controls.Add(chkPatchAll);
		pnl.Controls.Add(btnAdd);
		pnl.Controls.Add(lblListHeader);
		pnl.Controls.Add(_customAobFlow);

		return MakeCard("➕ เพิ่มฟังก์ชัน AOB เอง (Custom AOB Manager)", pnl);
	}

	// -------------------------------------------------------------
	// 📊 DASHBOARD TAB
	// -------------------------------------------------------------
	private Panel MakeReportCard()
	{
		Panel pnl = new Panel { Width = 446, AutoSize = true, BackColor = Theme.Panel };
		FlowLayoutPanel content = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			Width = 446,
			BackColor = Theme.Panel
		};

		_lblReportSummary = new Label
		{
			AutoSize = true,
			MaximumSize = new Size(420, 0),
			ForeColor = Theme.Text,
			Font = new Font("Segoe UI", 10f, FontStyle.Bold),
			Margin = new Padding(10, 8, 10, 4)
		};

		_lblReportTopCheats = new Label
		{
			AutoSize = true,
			MaximumSize = new Size(420, 0),
			ForeColor = Theme.Muted,
			Font = Theme.Font,
			Margin = new Padding(10, 0, 10, 8)
		};

		FlowLayoutPanel buttons = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.LeftToRight,
			AutoSize = true,
			Margin = new Padding(10, 4, 10, 10)
		};
		Button exportBtn = Theme.MakeButton("Export CSV", 120, 32, accent: true);
		exportBtn.Click += delegate { ExportUsageReport(); };
		Button clearBtn = Theme.MakeButton("ล้างประวัติ", 110, 32);
		clearBtn.Click += delegate
		{
			AppSettings.Current.UsageHistory.Clear();
			AppSettings.Save();
			RefreshReport();
			ShowMsg("ล้างประวัติการใช้งานแล้ว");
		};
		buttons.Controls.Add(exportBtn);
		buttons.Controls.Add(clearBtn);

		Label recentTitle = new Label
		{
			Text = "กิจกรรมล่าสุด",
			AutoSize = true,
			ForeColor = Theme.Accent,
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			Margin = new Padding(10, 4, 10, 4)
		};

		_historyFlow = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			Width = 426,
			BackColor = Theme.Field,
			Padding = new Padding(6),
			Margin = new Padding(10, 0, 10, 12)
		};

		content.Controls.Add(_lblReportSummary);
		content.Controls.Add(_lblReportTopCheats);
		content.Controls.Add(buttons);
		content.Controls.Add(recentTitle);
		content.Controls.Add(_historyFlow);
		pnl.Controls.Add(content);
		RefreshReport();
		return MakeCard("📈 รายงานและประวัติการใช้งาน", pnl);
	}

	private Panel MakeAutomationCard()
	{
		Panel pnl = new Panel { Width = 446, AutoSize = true, BackColor = Theme.Panel };
		FlowLayoutPanel content = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			Width = 446,
			BackColor = Theme.Panel
		};

		CheckBox autoPing = MakeAutomationCheck("เปิด Ping อัตโนมัติหลังเชื่อมต่อโปรเซส", AppSettings.Current.AutoStartPingOnAttach);
		autoPing.CheckedChanged += delegate
		{
			AppSettings.Current.AutoStartPingOnAttach = autoPing.Checked;
			AppSettings.Save();
			RefreshAutomationStatus();
		};

		Panel disableRow = MakeAutomationNumberRow("ปิดฟังก์ชันอัตโนมัติหลังใช้งาน", AppSettings.Current.AutoDisableCheatsAfterMinutes, delegate(int value)
		{
			AppSettings.Current.AutoDisableCheatsAfterMinutes = value;
			AppSettings.Save();
			RefreshAutomationStatus();
		});

		Panel panicRow = MakeAutomationNumberRow("Panic อัตโนมัติหลังระบบทำงานต่อเนื่อง", AppSettings.Current.AutoPanicAfterMinutes, delegate(int value)
		{
			AppSettings.Current.AutoPanicAfterMinutes = value;
			AppSettings.Save();
			RefreshAutomationStatus();
		});

		_lblAutomationStatus = new Label
		{
			AutoSize = true,
			MaximumSize = new Size(420, 0),
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont,
			Margin = new Padding(10, 8, 10, 12)
		};

		content.Controls.Add(autoPing);
		content.Controls.Add(disableRow);
		content.Controls.Add(panicRow);
		content.Controls.Add(_lblAutomationStatus);
		pnl.Controls.Add(content);
		RefreshAutomationStatus();
		return MakeCard("⚙ Automation Rules", pnl);
	}

	private CheckBox MakeAutomationCheck(string text, bool isChecked)
	{
		return new CheckBox
		{
			Text = text,
			Checked = isChecked,
			AutoSize = true,
			ForeColor = Theme.Text,
			Font = Theme.Font,
			FlatStyle = FlatStyle.Flat,
			Margin = new Padding(10, 10, 10, 4)
		};
	}

	private Panel MakeAutomationNumberRow(string title, int currentValue, Action<int> onChanged)
	{
		Panel row = new Panel { Width = 426, Height = 42, BackColor = Theme.Panel, Margin = new Padding(10, 4, 10, 4) };
		Label label = new Label { Text = title, Location = new Point(0, 10), Size = new Size(295, 22), ForeColor = Theme.Text, Font = Theme.Font };
		NumericUpDown minutes = new NumericUpDown
		{
			Location = new Point(300, 8),
			Size = new Size(70, 24),
			Minimum = 0,
			Maximum = 240,
			Value = Math.Max(0, Math.Min(240, currentValue)),
			BackColor = Theme.Field,
			ForeColor = Theme.Text
		};
		Label suffix = new Label { Text = "นาที", Location = new Point(378, 11), AutoSize = true, ForeColor = Theme.Muted, Font = Theme.SmallFont };
		minutes.ValueChanged += delegate { onChanged((int)minutes.Value); };
		row.Controls.Add(label);
		row.Controls.Add(minutes);
		row.Controls.Add(suffix);
		return row;
	}

	private void OnAutomationTick(object? sender, EventArgs e)
	{
		bool hasActiveCheats = _cheats.Any(c => c.IsEnabled);
		bool hasActiveSystem = hasActiveCheats || _pingEngine.IsRunning;

		_activeSessionStartedAt = hasActiveSystem ? (_activeSessionStartedAt ?? DateTime.Now) : null;
		_cheatSessionStartedAt = hasActiveCheats ? (_cheatSessionStartedAt ?? DateTime.Now) : null;

		int disableAfter = AppSettings.Current.AutoDisableCheatsAfterMinutes;
		if (disableAfter > 0 && _cheatSessionStartedAt.HasValue && DateTime.Now - _cheatSessionStartedAt.Value >= TimeSpan.FromMinutes(disableAfter))
		{
			LogUsage("Automation", $"ปิดฟังก์ชันอัตโนมัติหลัง {disableAfter} นาที");
			DisableAll();
			_cheatSessionStartedAt = null;
		}

		int panicAfter = AppSettings.Current.AutoPanicAfterMinutes;
		if (panicAfter > 0 && _activeSessionStartedAt.HasValue && DateTime.Now - _activeSessionStartedAt.Value >= TimeSpan.FromMinutes(panicAfter))
		{
			LogUsage("Automation", $"Panic อัตโนมัติหลัง {panicAfter} นาที");
			EmergencyPanic();
			_activeSessionStartedAt = null;
		}

		RefreshAutomationStatus();
	}

	private void RefreshAutomationStatus()
	{
		if (_lblAutomationStatus == null)
		{
			return;
		}
		List<string> lines = new List<string>();
		lines.Add(AppSettings.Current.AutoStartPingOnAttach ? "Auto Ping: เปิด" : "Auto Ping: ปิด");
		lines.Add(AppSettings.Current.AutoDisableCheatsAfterMinutes > 0 ? $"Auto Disable: {AppSettings.Current.AutoDisableCheatsAfterMinutes} นาที" : "Auto Disable: ปิด");
		lines.Add(AppSettings.Current.AutoPanicAfterMinutes > 0 ? $"Auto Panic: {AppSettings.Current.AutoPanicAfterMinutes} นาที" : "Auto Panic: ปิด");
		if (_activeSessionStartedAt.HasValue)
		{
			TimeSpan active = DateTime.Now - _activeSessionStartedAt.Value;
			lines.Add($"เวลาทำงานต่อเนื่อง: {active.Hours:D2}:{active.Minutes:D2}:{active.Seconds:D2}");
		}
		_lblAutomationStatus.Text = string.Join(Environment.NewLine, lines);
	}

	private void LogUsage(string type, string message)
	{
		try
		{
			AppSettings.Current.UsageHistory ??= new List<UsageLogEntry>();
			AppSettings.Current.UsageHistory.Add(new UsageLogEntry { Timestamp = DateTime.Now, Type = type, Message = message });
			if (AppSettings.Current.UsageHistory.Count > 300)
			{
				AppSettings.Current.UsageHistory = AppSettings.Current.UsageHistory.Skip(AppSettings.Current.UsageHistory.Count - 300).ToList();
			}
			AppSettings.Save();
			RefreshReport();
		}
		catch { }
	}

	private void RefreshReport()
	{
		if (_lblReportSummary == null || _lblReportTopCheats == null || _historyFlow == null)
		{
			return;
		}

		List<UsageLogEntry> history = AppSettings.Current.UsageHistory ?? new List<UsageLogEntry>();
		DateTime today = DateTime.Today;
		int todayCount = history.Count(x => x.Timestamp.Date == today);
		int sessionCount = history.Count(x => x.Type == "Session");
		int pingCount = history.Count(x => x.Type == "Ping");
		int featureCount = history.Count(x => x.Type == "Feature");

		_lblReportSummary.Text = $"วันนี้ {todayCount} เหตุการณ์ | Sessions {sessionCount} | Ping {pingCount} | Features {featureCount}";
		var topFeatures = history
			.Where(x => x.Type == "Feature" && x.Message.EndsWith("ON", StringComparison.OrdinalIgnoreCase))
			.GroupBy(x => x.Message.Replace(" ON", ""))
			.OrderByDescending(g => g.Count())
			.Take(5)
			.Select(g => $"{g.Key}: {g.Count()} ครั้ง")
			.ToList();
		_lblReportTopCheats.Text = topFeatures.Count > 0 ? "ใช้งานบ่อย: " + string.Join(" | ", topFeatures) : "ยังไม่มีข้อมูลฟังก์ชันที่ใช้งานบ่อย";

		_historyFlow.Controls.Clear();
		foreach (UsageLogEntry entry in history.OrderByDescending(x => x.Timestamp).Take(12))
		{
			Label row = new Label
			{
				Text = $"{entry.Timestamp:HH:mm:ss} [{entry.Type}] {entry.Message}",
				AutoSize = true,
				MaximumSize = new Size(400, 0),
				ForeColor = entry.Type == "Panic" || entry.Type == "Automation" ? Theme.Warning : Theme.Text,
				Font = Theme.SmallFont,
				Margin = new Padding(2, 2, 2, 3)
			};
			_historyFlow.Controls.Add(row);
		}
		if (_historyFlow.Controls.Count == 0)
		{
			_historyFlow.Controls.Add(new Label { Text = "ยังไม่มีประวัติการใช้งาน", AutoSize = true, ForeColor = Theme.Muted, Font = Theme.SmallFont });
		}
	}

	private void ExportUsageReport()
	{
		using SaveFileDialog dlg = new SaveFileDialog
		{
			Title = "Export Usage Report",
			Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
			FileName = $"mixandmin-report-{DateTime.Now:yyyyMMdd-HHmm}.csv"
		};
		if (dlg.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}

		StringBuilder sb = new StringBuilder();
		sb.AppendLine("timestamp,type,message");
		foreach (UsageLogEntry entry in (AppSettings.Current.UsageHistory ?? new List<UsageLogEntry>()).OrderBy(x => x.Timestamp))
		{
			sb.AppendLine($"{entry.Timestamp:O},{EscapeCsv(entry.Type)},{EscapeCsv(entry.Message)}");
		}
		System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
		ShowMsg("ส่งออกรายงาน CSV สำเร็จ");
	}

	private static string EscapeCsv(string value)
	{
		if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
		{
			return "\"" + value.Replace("\"", "\"\"") + "\"";
		}
		return value;
	}

	private Panel MakeDashboardCard()
	{
		Panel pnl = new Panel { Width = 446, AutoSize = true, BackColor = Theme.Panel };

		_lblDashAttach = new Label
		{
			Text = "🔗 สถานะเชื่อมต่อ: กำลังตรวจสอบ...",
			Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
			ForeColor = Theme.Text,
			AutoSize = true,
			Margin = new Padding(10, 8, 10, 4)
		};

		_lblDashCheats = new Label
		{
			Text = "🥊 ฟังก์ชันที่เปิด: 0 รายการ",
			Font = new Font("Segoe UI", 10f, FontStyle.Bold),
			ForeColor = Theme.Text,
			AutoSize = true,
			Margin = new Padding(10, 4, 10, 2)
		};

		_lblDashActiveList = new Label
		{
			Text = "ยังไม่มีฟังก์ชันที่เปิดใช้งาน",
			Font = Theme.SmallFont,
			ForeColor = Theme.Muted,
			AutoSize = true,
			Margin = new Padding(24, 0, 10, 6)
		};

		_lblDashPing = new Label
		{
			Text = "⚡ ปิงจำลอง: ปิดอยู่ (IDLE)",
			Font = Theme.Font,
			ForeColor = Theme.Text,
			AutoSize = true,
			Margin = new Padding(10, 4, 10, 4)
		};

		_lblDashUptime = new Label
		{
			Text = "⏱️ เวลาใช้งาน: 00:00:00",
			Font = Theme.Font,
			ForeColor = Theme.Text,
			AutoSize = true,
			Margin = new Padding(10, 4, 10, 10)
		};

		// Quick Action buttons row
		FlowLayoutPanel quickBtnRow = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.LeftToRight,
			AutoSize = true,
			Margin = new Padding(10, 6, 10, 12)
		};

		Button btnQuickPanic = Theme.MakeButton("🚨 Panic (F10)", 134, 34, accent: false);
		btnQuickPanic.BackColor = Theme.Danger;
		btnQuickPanic.ForeColor = Color.White;
		btnQuickPanic.Click += delegate { EmergencyPanic(); };

		Button btnQuickPing = Theme.MakeButton("⚡ เปิด/ปิดปิง (F6)", 134, 34, accent: true);
		btnQuickPing.Click += delegate { TogglePingEngine(); };

		Button btnQuickDisableAll = Theme.MakeButton("⏹ ปิดทุกฟังก์ชัน", 134, 34, accent: false);
		btnQuickDisableAll.Click += delegate { DisableAll(); };

		quickBtnRow.Controls.Add(btnQuickPanic);
		quickBtnRow.Controls.Add(btnQuickPing);
		quickBtnRow.Controls.Add(btnQuickDisableAll);

		FlowLayoutPanel content = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			Width = 446
		};
		content.Controls.Add(_lblDashAttach);
		content.Controls.Add(_lblDashCheats);
		content.Controls.Add(_lblDashActiveList);
		content.Controls.Add(_lblDashPing);
		content.Controls.Add(_lblDashUptime);
		content.Controls.Add(quickBtnRow);

		pnl.Controls.Add(content);
		return MakeCard("📊 แดชบอร์ดสรุปสถานะระบบ", pnl);
	}

	private void OnDashboardTick(object? sender, EventArgs e)
	{
		try
		{
			// Uptime
			TimeSpan up = DateTime.Now - _startTime;
			if (_lblDashUptime != null)
			{
				_lblDashUptime.Text = $"⏱️ เวลาใช้งาน: {up.Hours:D2}:{up.Minutes:D2}:{up.Seconds:D2}";
			}

			// Attach status
			if (_lblDashAttach != null)
			{
				if (_procHandle != IntPtr.Zero)
				{
					string procName = _procCombo.SelectedItem?.ToString() ?? "Game";
					_lblDashAttach.Text = $"🔗 สถานะเชื่อมต่อ: 🟢 เชื่อมต่อแล้ว ({procName})";
					_lblDashAttach.ForeColor = Theme.On;
				}
				else
				{
					_lblDashAttach.Text = "🔗 สถานะเชื่อมต่อ: 🔴 ยังไม่ได้เชื่อมต่อเกม";
					_lblDashAttach.ForeColor = Theme.Danger;
				}
			}

			// Cheats active
			var activeCheats = _cheats.Where(c => c.IsEnabled).ToList();
			if (_lblDashCheats != null)
			{
				_lblDashCheats.Text = $"🥊 ฟังก์ชันที่เปิด: {activeCheats.Count}/{_cheats.Count} รายการ";
				_lblDashCheats.ForeColor = activeCheats.Count > 0 ? Theme.Accent : Theme.Text;
			}
			if (_lblDashActiveList != null)
			{
				if (activeCheats.Count > 0)
				{
					_lblDashActiveList.Text = "• " + string.Join("\n• ", activeCheats.Take(8).Select(c => c.Name)) + (activeCheats.Count > 8 ? $"\n  ...และอีก {activeCheats.Count - 8} รายการ" : "");
					_lblDashActiveList.ForeColor = Theme.Accent;
				}
				else
				{
					_lblDashActiveList.Text = "ยังไม่มีฟังก์ชันที่เปิดใช้งาน";
					_lblDashActiveList.ForeColor = Theme.Muted;
				}
			}

			// Ping status
			if (_lblDashPing != null)
			{
				if (_pingEngine.IsRunning)
				{
					_lblDashPing.Text = $"⚡ ปิงจำลอง: 🟢 กำลังทำงาน ({_pingEngine.GetStats().CurrentSimulatedPing} ms) | Min:{AppSettings.Current.PingMin} Max:{AppSettings.Current.PingMax}";
					_lblDashPing.ForeColor = Theme.On;
				}
				else
				{
					_lblDashPing.Text = "⚡ ปิงจำลอง: ⚪ ปิดอยู่ (IDLE)";
					_lblDashPing.ForeColor = Theme.Muted;
				}
			}
		}
		catch { }
	}

	// -------------------------------------------------------------
	// 📋 PRESET MANAGER TAB
	// -------------------------------------------------------------
	private Panel MakePresetCard()
	{
		Panel pnl = new Panel { Width = 446, AutoSize = true, BackColor = Theme.Panel };

		// Save row
		Panel topRow = new Panel { Width = 430, Height = 40, BackColor = Color.Transparent };
		TextBox txtPresetName = new TextBox
		{
			Location = new Point(0, 5),
			Size = new Size(265, 28),
			BackColor = Theme.Field,
			ForeColor = Theme.Text,
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 9.5f),
			PlaceholderText = "ตั้งชื่อ Preset เช่น วอร์ 1, ตัวไหล..."
		};
		Button btnSavePreset = Theme.MakeButton("💾 บันทึกปัจจุบัน", 155, 30, accent: true);
		btnSavePreset.Location = new Point(275, 4);

		topRow.Controls.Add(txtPresetName);
		topRow.Controls.Add(btnSavePreset);

		Label lblNotice = new Label
		{
			Text = "💡 บันทึกฟังก์ชันที่เปิดอยู่ทั้งหมด + ค่าปิงไว้เป็น Preset เพื่อเปิดใช้ได้ในคลิกเดียว",
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont,
			AutoSize = true,
			Margin = new Padding(0, 4, 0, 8)
		};

		Label lblListHeader = new Label
		{
			Text = "📋 รายการ Preset ที่บันทึกไว้:",
			ForeColor = Theme.Accent,
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			AutoSize = true,
			Margin = new Padding(0, 8, 0, 4)
		};

		_presetFlow = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			Width = 430,
			BackColor = Theme.Field,
			Padding = new Padding(4)
		};

		Action refreshPresetList = () =>
		{
			_presetFlow.Controls.Clear();
			if (AppSettings.Current.Presets == null || AppSettings.Current.Presets.Count == 0)
			{
				Label empty = new Label
				{
					Text = "ยังไม่มี Preset ที่บันทึกไว้ (กรอกชื่อด้านบนแล้วกดบันทึก)",
					ForeColor = Theme.Muted,
					Font = Theme.SmallFont,
					AutoSize = true,
					Padding = new Padding(8)
				};
				_presetFlow.Controls.Add(empty);
				return;
			}

			foreach (var preset in AppSettings.Current.Presets.ToList())
			{
				Panel row = new Panel
				{
					Width = 416,
					Height = 44,
					BackColor = Theme.Panel,
					Margin = new Padding(2, 2, 2, 4)
				};

				Label title = new Label
				{
					Text = $"📌 {preset.Name}",
					Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
					ForeColor = Theme.Text,
					Location = new Point(8, 4),
					AutoSize = true
				};

				string pingInfo = preset.PingEnabled ? $"ปิง: {preset.PingMin}-{preset.PingMax}ms" : "ปิง: ปิด";
				Label subtitle = new Label
				{
					Text = $"{preset.EnabledCheats.Count} ฟังก์ชัน • {pingInfo}",
					Font = Theme.SmallFont,
					ForeColor = Theme.Muted,
					Location = new Point(8, 24),
					AutoSize = true
				};

				Button loadBtn = Theme.MakeButton("▶️ โหลด", 72, 28, accent: true);
				loadBtn.Location = new Point(275, 8);
				loadBtn.Click += async delegate
				{
					await LoadPresetAsync(preset);
				};

				Button delBtn = Theme.MakeButton("🗑️", 40, 28, accent: false);
				delBtn.Location = new Point(355, 8);
				delBtn.Click += delegate
				{
					AppSettings.Current.Presets.Remove(preset);
					AppSettings.Save();
					ShowMsg($"ลบ Preset [{preset.Name}] แล้ว");
					var refresh = _presetFlow.Tag as Action;
					refresh?.Invoke();
				};

				row.Controls.Add(title);
				row.Controls.Add(subtitle);
				row.Controls.Add(loadBtn);
				row.Controls.Add(delBtn);
				_presetFlow.Controls.Add(row);
			}
		};

		_presetFlow.Tag = refreshPresetList;
		refreshPresetList();

		btnSavePreset.Click += delegate
		{
			string name = txtPresetName.Text.Trim();
			if (string.IsNullOrEmpty(name))
			{
				MessageBox.Show("กรุณาตั้งชื่อ Preset ก่อน", "MIXANDMIN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			SaveCurrentPreset(name);
			txtPresetName.Clear();
			refreshPresetList();
			ShowMsg($"บันทึก Preset [{name}] สำเร็จ!");
			SoundHelper.PlayToggle(true);
		};

		FlowLayoutPanel container = new FlowLayoutPanel
		{
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoSize = true,
			Width = 446
		};
		container.Controls.Add(topRow);
		container.Controls.Add(lblNotice);
		container.Controls.Add(lblListHeader);
		container.Controls.Add(_presetFlow);

		pnl.Controls.Add(container);
		return MakeCard("📋 ระบบบันทึก/โหลด Preset", pnl);
	}

	private void SaveCurrentPreset(string name)
	{
		var preset = new CheatPreset
		{
			Name = name,
			EnabledCheats = _cheats.Where(c => c.IsEnabled).Select(c => c.Name).ToList(),
			PingEnabled = _pingEngine.IsRunning,
			PingMin = AppSettings.Current.PingMin,
			PingMax = AppSettings.Current.PingMax
		};

		if (AppSettings.Current.Presets == null)
		{
			AppSettings.Current.Presets = new List<CheatPreset>();
		}

		var existing = AppSettings.Current.Presets.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		if (existing != null)
		{
			AppSettings.Current.Presets.Remove(existing);
		}
		AppSettings.Current.Presets.Add(preset);
		AppSettings.Save();
	}

	private async Task LoadPresetAsync(CheatPreset preset)
	{
		ShowMsg($"กำลังโหลด Preset [{preset.Name}]...");

		// 1. Disable all active cheats first
		DisableAll();

		await Task.Delay(200);

		// 2. Enable matching cheats
		int count = 0;
		foreach (var cheatName in preset.EnabledCheats)
		{
			var found = _cheats.FirstOrDefault(c => c.Name == cheatName);
			if (found != null)
			{
				await ToggleCheatAsync(found, enable: true, quiet: true);
				SetControlChecked(found, true);
				count++;
			}
		}

		// 3. Ping setting
		if (preset.PingEnabled && !_pingEngine.IsRunning)
		{
			TogglePingEngine();
		}
		else if (!preset.PingEnabled && _pingEngine.IsRunning)
		{
			TogglePingEngine();
		}

		ShowMsg($"โหลด Preset [{preset.Name}] สำเร็จ! (เปิด {count} ฟังก์ชัน)");
		SoundHelper.PlayToggle(true);
	}
}
