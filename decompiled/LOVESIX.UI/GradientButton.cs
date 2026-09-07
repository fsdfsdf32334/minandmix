using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LOVESIX.UI;

public class GradientButton : Control
{
	private bool _hover;

	public event EventHandler Clicked;

	public void PerformClick()
	{
		Clicked?.Invoke(this, EventArgs.Empty);
	}

	public GradientButton(string text)
	{
		Text = text;
		Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle rectangle = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
		using (GraphicsPath path = Theme.RoundRect(rectangle, 8))
		{
			Color c1 = _hover ? Theme.Accent2 : Theme.Accent;
			Color c2 = _hover ? Color.FromArgb(100, 255, 230) : Theme.Accent2;
			using (LinearGradientBrush brush = new LinearGradientBrush(rectangle, c1, c2, 45f))
			{
				graphics.FillPath(brush, path);
			}
		}
		TextRenderer.DrawText(graphics, Text, Font, rectangle, Color.FromArgb(10, 12, 16), TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		_hover = true;
		Invalidate();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		_hover = false;
		Invalidate();
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		Clicked?.Invoke(this, EventArgs.Empty);
	}
}
