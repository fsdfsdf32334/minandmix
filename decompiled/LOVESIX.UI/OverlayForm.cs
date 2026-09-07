using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using LOVESIX.Core;

namespace LOVESIX.UI;

public class OverlayForm : Form
{
	private int _ping;
	private bool _active;
	private string _mode = "IDLE";
	private string _activeCheatsSummary = "";

	private readonly System.Windows.Forms.Timer _fadeTimer;
	private float _opacity = 0.9f;

	public OverlayForm()
	{
		FormBorderStyle = FormBorderStyle.None;
		StartPosition = FormStartPosition.Manual;
		Location = new Point(20, 20);
		Size = new Size(220, 95);
		TopMost = true;
		ShowInTaskbar = false;
		Opacity = _opacity;
		BackColor = Color.Black;
		TransparencyKey = Color.FromArgb(1, 1, 1);
		SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

		_fadeTimer = new System.Windows.Forms.Timer { Interval = 50 };
		_fadeTimer.Tick += FadeTick;

		MouseDown += OnMouseDown;
	}

	protected override CreateParams CreateParams
	{
		get
		{
			const int WS_EX_TOOLWINDOW = 0x80;
			const int WS_EX_LAYERED = 0x80000;
			const int WS_EX_TRANSPARENT = 0x20;
			var cp = base.CreateParams;
			cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_LAYERED;
			return cp;
		}
	}

	public void UpdateStats(int simulatedPing, bool active, string modeName = "", string activeCheats = "")
	{
		_ping = simulatedPing;
		_active = active;
		_mode = active ? (string.IsNullOrEmpty(modeName) ? "ACTIVE" : modeName) : "IDLE";
		_activeCheatsSummary = activeCheats;
		if (InvokeRequired)
			Invoke(new Action(Invalidate));
		else
			Invalidate();
	}

	public void SetOverlayEnabled(bool enabled)
	{
		if (InvokeRequired) { Invoke(new Action(() => SetOverlayEnabled(enabled))); return; }
		if (enabled)
		{
			Opacity = 0;
			Show();
			_opacity = 0.9f;
			_fadeTimer.Start();
		}
		else
		{
			_fadeTimer.Stop();
			Hide();
		}
	}

	private void FadeTick(object? sender, EventArgs e)
	{
		if (Opacity < _opacity)
		{
			Opacity = Math.Min(_opacity, Opacity + 0.08);
		}
		else
		{
			_fadeTimer.Stop();
		}
	}

	private Point _dragStart;
	private void OnMouseDown(object? sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_dragStart = e.Location;
			MouseMove += OnMouseMove;
			MouseUp += OnMouseUp;
		}
		else if (e.Button == MouseButtons.Right)
		{
			// Right-click to toggle hide
			Hide();
		}
	}

	private void OnMouseMove(object? sender, MouseEventArgs e)
	{
		Location = new Point(Location.X + e.X - _dragStart.X, Location.Y + e.Y - _dragStart.Y);
	}

	private void OnMouseUp(object? sender, MouseEventArgs e)
	{
		MouseMove -= OnMouseMove;
		MouseUp -= OnMouseUp;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.AntiAlias;

		// Background
		Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
		using (SolidBrush bg = new SolidBrush(Color.FromArgb(200, 10, 12, 20)))
			g.FillRectangle(bg, rect);

		// Border glow
		Color borderColor = _active ? Color.FromArgb(180, 0, 255, 140) : Color.FromArgb(80, 100, 120, 160);
		using (Pen border = new Pen(borderColor, 1.5f))
			g.DrawRectangle(border, rect);

		// Status dot
		Color dotColor = _active ? Color.FromArgb(0, 255, 140) : Color.FromArgb(180, 60, 80);
		using (SolidBrush dot = new SolidBrush(dotColor))
			g.FillEllipse(dot, 10, 10, 8, 8);

		// Mode label
		using (Font modeFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
		using (SolidBrush modeBrush = new SolidBrush(_active ? Color.FromArgb(0, 255, 140) : Color.FromArgb(140, 160, 180)))
			g.DrawString(_mode, modeFont, modeBrush, 22, 8);

		// Ping value
		string pingText = _active ? $"{_ping} ms" : "-- ms";
		Color pingColor = _active ? Color.FromArgb(0, 240, 130) : Color.FromArgb(80, 100, 120);
		using (Font pingFont = new Font("Segoe UI", 21f, FontStyle.Bold))
		using (SolidBrush pingBrush = new SolidBrush(pingColor))
		{
			SizeF sz = g.MeasureString(pingText, pingFont);
			g.DrawString(pingText, pingFont, pingBrush, (Width - sz.Width) / 2f, 22f);
		}

		// Active cheats info bar
		string featText = string.IsNullOrEmpty(_activeCheatsSummary) ? "ไม่มีฟังก์ชันเปิด" : _activeCheatsSummary;
		using (Font featFont = new Font("Segoe UI", 7f))
		using (SolidBrush featBrush = new SolidBrush(Color.FromArgb(180, 200, 220)))
		{
			g.DrawString(featText, featFont, featBrush, 10f, Height - 20f);
		}

		// MIXANDMIN watermark
		using (Font wmFont = new Font("Segoe UI", 6f))
		using (SolidBrush wmBrush = new SolidBrush(Color.FromArgb(60, 0, 200, 120)))
			g.DrawString("MIXANDMIN", wmFont, wmBrush, Width - 60f, 6f);
	}

	protected override bool ShowWithoutActivation => true;
}
