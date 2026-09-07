using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LOVESIX.UI;

public class ToggleSwitch : Control
{
	private bool _checked;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Checked
	{
		get
		{
			return _checked;
		}
		set
		{
			if (_checked != value)
			{
				_checked = value;
				CheckedChanged?.Invoke(this, EventArgs.Empty);
				Invalidate();
			}
		}
	}

	public event EventHandler CheckedChanged;

	public ToggleSwitch()
	{
		base.Size = new Size(48, 26);
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle r = new Rectangle(1, 1, base.Width - 2, base.Height - 2);
		using (GraphicsPath path = Theme.RoundRect(r, r.Height / 2))
		{
			using SolidBrush brush = new SolidBrush(Checked ? Theme.Accent : Theme.Border);
			graphics.FillPath(brush, path);
		}
		int num = base.Height - 8;
		int x = (Checked ? (base.Width - num - 4) : 4);
		using SolidBrush brush2 = new SolidBrush(Color.White);
		graphics.FillEllipse(brush2, x, 4, num, num);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		Checked = !Checked;
	}
}
