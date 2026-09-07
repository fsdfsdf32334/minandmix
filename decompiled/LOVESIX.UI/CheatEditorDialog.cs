using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LOVESIX.Core;

namespace LOVESIX.UI;

public class CheatEditorDialog : Form
{
	private readonly Cheat _cheat;
	private readonly TextBox _txtPatchHex;
	private readonly TextBox _txtFloatVal;
	private readonly Label _lblOriginal;
	private readonly Label _lblDefaultPatch;
	private readonly Label _lblStatus;

	public bool ValueChanged { get; private set; }

	public CheatEditorDialog(Cheat cheat)
	{
		_cheat = cheat;

		Text = $"แก้ไขค่าฟังก์ชัน — {cheat.Name}";
		StartPosition = FormStartPosition.CenterParent;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		ClientSize = new Size(500, 390);
		BackColor = Theme.Back;
		ForeColor = Theme.Text;
		Font = Theme.Font;

		int y = 16;

		// Function Title
		Label lblTitle = new Label
		{
			Text = $"⚙️ {cheat.Name}",
			Font = Theme.TitleFont,
			ForeColor = Theme.Accent,
			Location = new Point(18, y),
			AutoSize = true
		};
		Controls.Add(lblTitle);
		y += 38;

		// Group Info
		if (!string.IsNullOrEmpty(cheat.Group))
		{
			Label lblGroup = new Label
			{
				Text = $"หมวดหมู่: {cheat.Group}",
				ForeColor = Theme.Muted,
				Font = Theme.SmallFont,
				Location = new Point(20, y),
				AutoSize = true
			};
			Controls.Add(lblGroup);
			y += 24;
		}

		// Original Search AOB
		Label lblSearchTitle = new Label
		{
			Text = "🔍 AOB Search (ค่าค้นหาเดิม):",
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont,
			Location = new Point(20, y),
			AutoSize = true
		};
		Controls.Add(lblSearchTitle);
		y += 18;

		_lblOriginal = new Label
		{
			Text = cheat.SearchHex,
			ForeColor = Color.FromArgb(170, 180, 200),
			Font = new Font("Consolas", 9f),
			Location = new Point(20, y),
			Size = new Size(460, 22),
			BackColor = Theme.Field,
			BorderStyle = BorderStyle.FixedSingle,
			TextAlign = ContentAlignment.MiddleLeft,
			Padding = new Padding(4, 0, 0, 0)
		};
		Controls.Add(_lblOriginal);
		y += 30;

		// Default Patch Hex
		Label lblDefaultTitle = new Label
		{
			Text = "📦 ค่าดั้งเดิม (Default Patch Hex):",
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont,
			Location = new Point(20, y),
			AutoSize = true
		};
		Controls.Add(lblDefaultTitle);
		y += 18;

		_lblDefaultPatch = new Label
		{
			Text = cheat.DefaultPatchHex,
			ForeColor = Theme.Muted,
			Font = new Font("Consolas", 9f),
			Location = new Point(20, y),
			Size = new Size(460, 22),
			BackColor = Theme.Field,
			BorderStyle = BorderStyle.FixedSingle,
			TextAlign = ContentAlignment.MiddleLeft,
			Padding = new Padding(4, 0, 0, 0)
		};
		Controls.Add(_lblDefaultPatch);
		y += 32;

		// Current / Custom Patch Hex
		Label lblPatchTitle = new Label
		{
			Text = "✏️ ค่าแก้ไขปัจจุบัน (Custom Patch Hex):",
			ForeColor = Theme.Accent,
			Font = new Font("Segoe UI", 9f, FontStyle.Bold),
			Location = new Point(20, y),
			AutoSize = true
		};
		Controls.Add(lblPatchTitle);
		y += 20;

		_txtPatchHex = new TextBox
		{
			Text = cheat.PatchHex,
			ForeColor = Theme.Accent2,
			BackColor = Theme.Field,
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Consolas", 10f, FontStyle.Bold),
			Location = new Point(20, y),
			Size = new Size(460, 24)
		};
		Controls.Add(_txtPatchHex);
		y += 34;

		// Float Value Helper
		Panel pnlFloat = new Panel
		{
			Location = new Point(20, y),
			Size = new Size(460, 36),
			BackColor = Theme.Panel
		};
		Label lblFloatT = new Label
		{
			Text = "🔢 แปลงทศนิยม (Float):",
			Location = new Point(6, 9),
			AutoSize = true,
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont
		};
		_txtFloatVal = new TextBox
		{
			Location = new Point(135, 6),
			Width = 100,
			BackColor = Theme.Field,
			ForeColor = Theme.Text,
			BorderStyle = BorderStyle.FixedSingle,
			Font = new Font("Segoe UI", 9f)
		};
		TryExtractFloat(cheat.PatchHex);

		Button btnConvertFloat = Theme.MakeButton("แปลงเป็น Hex", 100, 26);
		btnConvertFloat.Location = new Point(245, 5);
		btnConvertFloat.Click += delegate
		{
			if (float.TryParse(_txtFloatVal.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float fVal))
			{
				byte[] bytes = BitConverter.GetBytes(fVal);
				string hex = BitConverter.ToString(bytes).Replace("-", " ");
				string currentHex = _txtPatchHex.Text.Trim();
				string[] parts = currentHex.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length > 4)
				{
					string tail = string.Join(" ", parts, 4, parts.Length - 4);
					_txtPatchHex.Text = $"{hex} {tail}";
				}
				else
				{
					_txtPatchHex.Text = hex;
				}
			}
			else
			{
				MessageBox.Show("กรุณากรอกตัวเลขทศนิยมที่ถูกต้อง เช่น 1.5 หรือ 2.0", "MIXANDMIN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		};

		pnlFloat.Controls.Add(lblFloatT);
		pnlFloat.Controls.Add(_txtFloatVal);
		pnlFloat.Controls.Add(btnConvertFloat);
		Controls.Add(pnlFloat);
		y += 44;

		// Status / Help
		_lblStatus = new Label
		{
			Text = "💡 แนะนำ: สามารถดับเบิ้ลคลิกหรือคลิกขวาที่ฟังก์ชันเพื่อเปิดหน้านี้ได้ตลอดเวลา",
			ForeColor = Theme.Muted,
			Font = Theme.SmallFont,
			Location = new Point(20, y),
			Size = new Size(460, 20)
		};
		Controls.Add(_lblStatus);
		y += 26;

		// Action Buttons
		Button btnSave = Theme.MakeButton("💾 บันทึกค่าใหม่", 130, 34, accent: true);
		btnSave.Location = new Point(20, y);
		btnSave.Click += OnSaveClick;

		Button btnReset = Theme.MakeButton("🔄 คืนค่าดั้งเดิม", 120, 34);
		btnReset.Location = new Point(160, y);
		btnReset.Click += delegate
		{
			_txtPatchHex.Text = _cheat.DefaultPatchHex;
			TryExtractFloat(_cheat.DefaultPatchHex);
		};

		Button btnCancel = Theme.MakeButton("ยกเลิก", 90, 34);
		btnCancel.Location = new Point(390, y);
		btnCancel.Click += delegate { Close(); };

		Controls.Add(btnSave);
		Controls.Add(btnReset);
		Controls.Add(btnCancel);
	}

	private void TryExtractFloat(string hexStr)
	{
		try
		{
			string[] parts = hexStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length >= 4)
			{
				byte[] b = new byte[4];
				for (int i = 0; i < 4; i++)
					b[i] = Convert.ToByte(parts[i], 16);
				float val = BitConverter.ToSingle(b, 0);
				if (!float.IsNaN(val) && !float.IsInfinity(val) && Math.Abs(val) < 100000f)
				{
					_txtFloatVal.Text = val.ToString("0.##", CultureInfo.InvariantCulture);
				}
			}
		}
		catch { }
	}

	private void OnSaveClick(object? sender, EventArgs e)
	{
		string newHex = _txtPatchHex.Text.Trim();
		if (string.IsNullOrWhiteSpace(newHex))
		{
			MessageBox.Show("ค่า Patch Hex ต้องไม่ว่างเปล่า", "MIXANDMIN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return;
		}

		try
		{
			byte[] newBytes = CheatRegistry.H(newHex);
			_cheat.Patch = newBytes;
			_cheat.PatchHex = newHex;

			if (AppSettings.Current.CheatOverrides == null)
				AppSettings.Current.CheatOverrides = new System.Collections.Generic.Dictionary<string, string>();

			AppSettings.Current.CheatOverrides[_cheat.Name] = newHex;
			AppSettings.Save();

			if (_cheat.IsEnabled && _cheat.Handle != IntPtr.Zero)
			{
				foreach (var patch in _cheat.Patches)
				{
					MemoryHelper.Write(_cheat.Handle, patch.Address, _cheat.Patch);
				}
			}

			ValueChanged = true;
			SoundHelper.PlayToggle(true);
			MessageBox.Show($"บันทึกค่าใหม่ของ '{_cheat.Name}' สำเร็จแล้ว!\nระบบจะใช้ค่านี้ทันที", "MIXANDMIN", MessageBoxButtons.OK, MessageBoxIcon.Information);
			Close();
		}
		catch (Exception ex)
		{
			MessageBox.Show($"รูปแบบ Hex ไม่ถูกต้อง: {ex.Message}", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
