using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using LOVESIX.Core;
using LOVESIX.UI;

namespace LOVESIX;

public class LoginForm : Form
{
	private readonly bool _registerMode;
	private Panel _pass;
	private Panel? _pass2;
	private TextBox _passTb;
	private TextBox? _pass2Tb;
	private Label _hint;
	private GradientButton _submit;

	public bool Succeeded { get; private set; }

	public LoginForm()
	{
		_registerMode = !UserStore.HasAccount();
		Text = "MIXANDMIN — เข้าสู่ระบบ";
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ClientSize = new Size(460, 720);
		BackColor = Theme.Back;
		ForeColor = Theme.Text;
		Font = Theme.Font;
		base.AutoScaleMode = AutoScaleMode.Dpi;
		base.Icon = IconFactory.CreateAppIcon();
		base.KeyPreview = true;
		base.KeyDown += delegate(object? s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				_submit.PerformClick();
			}
		};
		base.Controls.Add(BuildCard());
	}

	private Control BuildCard()
	{
		Panel card = new Panel
		{
			BackColor = Theme.Panel,
			Size = new Size(380, _registerMode ? 640 : 520),
			Location = new Point((base.ClientSize.Width - 380) / 2, 70)
		};
		card.Region = new Region(Theme.RoundRect(new Rectangle(0, 0, card.Width, card.Height), 16));
		card.Paint += delegate(object? s, PaintEventArgs e)
		{
			using Pen pen = new Pen(Theme.Border);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.DrawPath(pen, Theme.RoundRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 16));
		};

		Panel panel = new Panel
		{
			Size = new Size(84, 84),
			Location = new Point((card.Width - 84) / 2, 30)
		};
		panel.Region = new Region(Theme.RoundRect(new Rectangle(0, 0, 84, 84), 20));
		panel.Paint += delegate(object? s, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, 84, 84), Theme.Accent, Theme.Accent2, 45f))
			{
				graphics.FillRectangle(brush, new Rectangle(0, 0, 84, 84));
			}
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			using Font font = new Font("Segoe UI", 22f, FontStyle.Bold);
			using SolidBrush brush2 = new SolidBrush(Color.FromArgb(10, 12, 16));
			SizeF sizeF = graphics.MeasureString("MM", font);
			graphics.DrawString("MM", font, brush2, (84f - sizeF.Width) / 2f, (84f - sizeF.Height) / 2f - 1f);
		};

		Label label = new Label
		{
			Text = "MIXANDMIN",
			Font = new Font("Segoe UI", 22f, FontStyle.Bold),
			ForeColor = Theme.Text,
			AutoSize = true,
			BackColor = Color.Transparent
		};
		label.Location = new Point((card.Width - label.PreferredSize.Width) / 2, 122);

		Label label2 = new Label
		{
			Text = (_registerMode ? "ตั้งรหัสผ่านครั้งแรกเพื่อเข้าใช้งาน" : "กรอกรหัสผ่านเพื่อเข้าใช้งาน"),
			Font = Theme.SmallFont,
			ForeColor = Theme.Muted,
			AutoSize = true,
			BackColor = Color.Transparent
		};
		label2.Location = new Point((card.Width - label2.PreferredSize.Width) / 2, 164);

		_pass = MakeField(card, "รหัสผ่าน", _registerMode ? 206 : 214, "พิมพ์รหัสผ่าน", out _passTb);
		_pass2 = (_registerMode ? MakeField(card, "ยืนยันรหัสผ่าน", 278, "พิมพ์รหัสผ่านอีกครั้ง", out _pass2Tb) : null);

		_hint = new Label
		{
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			ForeColor = Theme.Danger,
			AutoSize = true,
			BackColor = Color.Transparent,
			Location = new Point(28, _registerMode ? 340 : 286)
		};

		_submit = new GradientButton(_registerMode ? "ตั้งรหัสผ่าน" : "เข้าสู่ระบบ")
		{
			Size = new Size(324, 46),
			Location = new Point(28, _registerMode ? 372 : 318)
		};
		_submit.Clicked += delegate
		{
			Submit();
		};

		Label label3 = new Label
		{
			Text = "MIXANDMIN  •  v3.0",
			Font = Theme.SmallFont,
			ForeColor = Theme.Muted,
			AutoSize = true,
			BackColor = Color.Transparent
		};
		label3.Location = new Point((card.Width - label3.PreferredSize.Width) / 2, card.Height - 40);

		card.Controls.Add(panel);
		card.Controls.Add(label);
		card.Controls.Add(label2);
		card.Controls.Add(_hint);
		card.Controls.Add(_submit);
		card.Controls.Add(label3);
		return card;
	}

	private Panel MakeField(Panel card, string labelText, int y, string placeholder, out TextBox tb)
	{
		Label value = new Label
		{
			Text = labelText,
			Font = Theme.SmallFont,
			ForeColor = Theme.Muted,
			AutoSize = true,
			Location = new Point(28, y),
			BackColor = Color.Transparent
		};
		Panel box = new Panel
		{
			Location = new Point(28, y + 24),
			Size = new Size(324, 48),
			BackColor = Theme.Field
		};
		box.Region = new Region(Theme.RoundRect(new Rectangle(0, 0, 324, 48), 10));
		Font font = new Font("Segoe UI", 11f);
		int num = TextRenderer.MeasureText("Ag", font).Height + 6;
		tb = new TextBox
		{
			BorderStyle = BorderStyle.None,
			BackColor = Theme.Field,
			ForeColor = Theme.Text,
			Font = font,
			UseSystemPasswordChar = true,
			Size = new Size(250, num),
			Location = new Point(14, (box.Height - num) / 2),
			Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
		};
		Label ph = new Label
		{
			Text = placeholder,
			Font = font,
			ForeColor = Theme.Muted,
			BackColor = Color.Transparent,
			AutoSize = false,
			Size = new Size(220, num),
			Location = new Point(tb.Left + 3, tb.Top),
			TextAlign = ContentAlignment.MiddleLeft,
			Cursor = Cursors.IBeam
		};
		TextBox txt = tb;
		ph.Click += delegate
		{
			txt.Focus();
		};
		txt.TextChanged += delegate
		{
			UpdatePh();
		};
		txt.GotFocus += delegate
		{
			UpdatePh();
		};
		txt.LostFocus += delegate
		{
			UpdatePh();
		};
		UpdatePh();
		Button eye = new Button
		{
			Text = "แสดง",
			Size = new Size(40, 30),
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.Transparent,
			ForeColor = Theme.Muted,
			Cursor = Cursors.Hand,
			Font = Theme.SmallFont
		};
		eye.FlatAppearance.BorderSize = 0;
		eye.FlatAppearance.MouseOverBackColor = Color.Transparent;
		eye.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		eye.Location = new Point(box.Width - 46, 9);
		eye.Click += delegate
		{
			txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar;
			eye.Text = (txt.UseSystemPasswordChar ? "แสดง" : "ซ่อน");
		};
		bool focused = false;
		box.Paint += delegate(object? s, PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using Pen pen = new Pen(focused ? Theme.Accent : Theme.FieldBorder);
			e.Graphics.DrawPath(pen, Theme.RoundRect(new Rectangle(1, 1, 322, 46), 10));
		};
		txt.GotFocus += delegate
		{
			focused = true;
			box.Invalidate();
		};
		txt.LostFocus += delegate
		{
			focused = false;
			box.Invalidate();
		};
		box.Controls.Add(tb);
		box.Controls.Add(ph);
		box.Controls.Add(eye);
		card.Controls.Add(value);
		card.Controls.Add(box);
		return box;
		void UpdatePh()
		{
			ph.Visible = txt.Text.Length == 0 && !txt.Focused;
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		_pass.Focus();
	}

	private void Submit()
	{
		string text = _passTb.Text;
		if (string.IsNullOrEmpty(text))
		{
			ShowErr("กรุณากรอกรหัสผ่าน");
			return;
		}
		if (_registerMode)
		{
			if (text.Length < 4)
			{
				ShowErr("รหัสผ่านต้องมีอย่างน้อย 4 ตัว");
				return;
			}
			if (_pass2 == null || text != _pass2Tb?.Text)
			{
				ShowErr("รหัสผ่านไม่ตรงกัน");
				return;
			}
			UserStore.CreateAccount(text);
		}
		else if (!UserStore.Verify(text))
		{
			ShowErr("รหัสผ่านไม่ถูกต้อง");
			return;
		}
		Succeeded = true;
		Close();
	}

	private void ShowErr(string msg)
	{
		_hint.Text = msg;
	}
}
