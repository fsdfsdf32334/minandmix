using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LOVESIX.UI;

public class ToggleRow : UserControl
{
	private readonly ToggleSwitch _tgl = new ToggleSwitch();

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Checked
	{
		get
		{
			return _tgl.Checked;
		}
		set
		{
			_tgl.Checked = value;
		}
	}

	public event EventHandler CheckedChanged;

	public ToggleRow(string title, string sub, bool initial)
	{
		base.Height = 56;
		BackColor = Theme.Panel;
		Cursor = Cursors.Hand;
		Label label = new Label
		{
			Text = title,
			Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
			ForeColor = Theme.Text,
			Location = new Point(14, 7),
			AutoSize = true,
			BackColor = Color.Transparent,
			Cursor = Cursors.Hand
		};
		Label label2 = new Label
		{
			Text = sub,
			Font = Theme.SmallFont,
			ForeColor = Theme.Muted,
			Location = new Point(14, 29),
			AutoSize = true,
			BackColor = Color.Transparent,
			Cursor = Cursors.Hand
		};
		_tgl.Location = new Point(base.Width - 60, 15);
		_tgl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		_tgl.CheckedChanged += delegate
		{
			CheckedChanged?.Invoke(this, EventArgs.Empty);
		};
		label.Click += delegate
		{
			Toggle();
		};
		label2.Click += delegate
		{
			Toggle();
		};
		base.Controls.Add(label);
		base.Controls.Add(label2);
		base.Controls.Add(_tgl);
		_tgl.Checked = initial;
	}

	private void Toggle()
	{
		_tgl.Checked = !_tgl.Checked;
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		Toggle();
	}
}
