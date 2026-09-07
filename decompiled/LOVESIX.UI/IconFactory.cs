using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace LOVESIX.UI;

public static class IconFactory
{
	public static Icon CreateAppIcon()
	{
		using Bitmap bitmap = new Bitmap(32, 32);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, 32, 32), Theme.Accent, Theme.Accent2, 45f))
			{
				using GraphicsPath path = Theme.RoundRect(new Rectangle(1, 1, 30, 30), 8);
				graphics.FillPath(brush, path);
			}
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			using Font font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
			using SolidBrush brush2 = new SolidBrush(Color.FromArgb(10, 12, 16));
			SizeF sizeF = graphics.MeasureString("MM", font);
			graphics.DrawString("MM", font, brush2, (32f - sizeF.Width) / 2f, (32f - sizeF.Height) / 2f - 1f);
		}
		return Icon.FromHandle(bitmap.GetHicon());
	}
}
