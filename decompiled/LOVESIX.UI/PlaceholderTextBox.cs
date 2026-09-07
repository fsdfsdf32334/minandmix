using System;
using System.ComponentModel;
using System.Drawing.Text;
using System.Windows.Forms;

namespace LOVESIX.UI;

public class PlaceholderTextBox : TextBox
{
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string Placeholder { get; set; } = "";

	public PlaceholderTextBox()
	{
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		base.OnPaintBackground(e);
		if (!string.IsNullOrEmpty(Placeholder) && Text.Length <= 0 && !Focused)
		{
			e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			TextRenderer.DrawText(e.Graphics, Placeholder, Font, base.ClientRectangle, Theme.Muted, TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
		}
	}

	protected override void OnTextChanged(EventArgs e)
	{
		base.OnTextChanged(e);
		Invalidate();
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		Invalidate();
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		Invalidate();
	}
}
