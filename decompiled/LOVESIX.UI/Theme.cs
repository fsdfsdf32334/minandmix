using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LOVESIX.UI;

public static class Theme
{
	// Deep Space Dark Backgrounds
	public static readonly Color Back = Color.FromArgb(12, 13, 18);
	public static readonly Color Panel = Color.FromArgb(20, 22, 30);
	public static readonly Color Panel2 = Color.FromArgb(16, 17, 24);
	public static readonly Color Border = Color.FromArgb(38, 42, 56);
	public static readonly Color CardHeader = Color.FromArgb(25, 28, 38);

	// Text
	public static readonly Color Text = Color.FromArgb(240, 244, 255);
	public static readonly Color Muted = Color.FromArgb(140, 148, 170);

	// Neon Cyan / Mint Accents
	public static readonly Color Accent = Color.FromArgb(0, 212, 170);
	public static readonly Color Accent2 = Color.FromArgb(0, 245, 200);
	public static readonly Color AccentDark = Color.FromArgb(0, 150, 120);

	// Status Colors
	public static readonly Color On = Color.FromArgb(0, 255, 178);
	public static readonly Color Danger = Color.FromArgb(255, 75, 106);
	public static readonly Color Warning = Color.FromArgb(255, 180, 84);

	// Input Fields
	public static readonly Color Field = Color.FromArgb(26, 29, 40);
	public static readonly Color FieldBorder = Color.FromArgb(48, 54, 72);

	// Navigation
	public static readonly Color NavActive = Color.FromArgb(18, 35, 42);

	// Fonts
	public static readonly Font Font = new Font("Segoe UI", 9.5f);
	public static readonly Font TitleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
	public static readonly Font GroupFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
	public static readonly Font SmallFont = new Font("Segoe UI", 8.5f);

	public static Button MakeButton(string text, int width, int height, bool accent = false)
	{
		Button button = new Button();
		button.Text = text;
		button.Width = width;
		button.Height = height;
		button.FlatStyle = FlatStyle.Flat;
		button.BackColor = (accent ? Accent : Panel);
		button.ForeColor = (accent ? Color.FromArgb(10, 12, 16) : Text);
		button.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
		button.Cursor = Cursors.Hand;
		button.Margin = new Padding(0, 0, 8, 0);
		button.FlatAppearance.BorderColor = (accent ? Accent : Border);
		button.FlatAppearance.MouseOverBackColor = (accent ? Accent2 : Color.FromArgb(32, 36, 48));
		button.FlatAppearance.BorderSize = ((!accent) ? 1 : 0);
		return button;
	}

	public static Button MakeNavButton(string text)
	{
		Button button = new Button();
		button.Text = text;
		button.Height = 44;
		button.FlatStyle = FlatStyle.Flat;
		button.BackColor = Color.Transparent;
		button.ForeColor = Muted;
		button.TextAlign = ContentAlignment.MiddleLeft;
		button.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
		button.Cursor = Cursors.Hand;
		button.Margin = new Padding(0, 0, 0, 4);
		button.Padding = new Padding(22, 0, 0, 0);
		button.FlatAppearance.BorderSize = 0;
		button.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 32, 44);
		return button;
	}

	public static GraphicsPath RoundRect(Rectangle r, int radius)
	{
		int num = radius * 2;
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddArc(r.X, r.Y, num, num, 180f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Y, num, num, 270f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(r.X, r.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}
}
