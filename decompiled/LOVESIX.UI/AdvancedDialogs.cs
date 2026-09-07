using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LOVESIX.UI;

/// <summary>
/// Quick Profile Switcher Dialog
/// </summary>
public class QuickProfileSwitcher : Form
{
    private ListBox _lstProfiles;
    private Button _btnLoad;
    private Button _btnNew;
    private Button _btnDelete;
    private TextBox _txtProfileName;
    
    public event Action<string> ProfileSelected;
    
    public QuickProfileSwitcher(List<string> profiles)
    {
        InitializeComponent();
        LoadProfiles(profiles);
    }
    
    private void InitializeComponent()
    {
        this.Text = "Quick Profile Switcher";
        this.Size = new Size(350, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(13, 18, 27);
        this.ForeColor = Color.FromArgb(230, 235, 245);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        
        var lblTitle = new Label
        {
            Text = "Select or Create Profile",
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(20, 28, 42)
        };
        
        _lstProfiles = new ListBox
        {
            Dock = DockStyle.Top,
            Height = 200,
            BackColor = Color.FromArgb(25, 35, 50),
            ForeColor = Color.FromArgb(230, 235, 245)
        };
        
        _txtProfileName = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.FromArgb(25, 35, 50),
            ForeColor = Color.FromArgb(230, 235, 245),
            PlaceholderText = "New profile name..."
        };
        
        var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(20, 28, 42) };
        
        _btnLoad = new Button
        {
            Text = "Load",
            Location = new Point(10, 10),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(100, 200, 255),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat
        };
        _btnLoad.Click += (s, e) => { if (_lstProfiles.SelectedItem != null) ProfileSelected?.Invoke(_lstProfiles.SelectedItem.ToString()); this.Close(); };
        
        _btnNew = new Button
        {
            Text = "New",
            Location = new Point(120, 10),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(76, 200, 120),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat
        };
        
        _btnDelete = new Button
        {
            Text = "Delete",
            Location = new Point(230, 10),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(255, 100, 100),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        
        btnPanel.Controls.Add(_btnLoad);
        btnPanel.Controls.Add(_btnNew);
        btnPanel.Controls.Add(_btnDelete);
        
        this.Controls.Add(btnPanel);
        this.Controls.Add(_txtProfileName);
        this.Controls.Add(_lstProfiles);
        this.Controls.Add(lblTitle);
    }
    
    private void LoadProfiles(List<string> profiles)
    {
        _lstProfiles.Items.Clear();
        foreach (var profile in profiles)
        {
            _lstProfiles.Items.Add(profile);
        }
    }
}

/// <summary>
/// Cheat Editor with Advanced Options
/// </summary>
public class AdvancedCheatEditor : Form
{
    private TextBox _txtName;
    private TextBox _txtSearchAob;
    private TextBox _txtPatchAob;
    private CheckBox _chkPatchAll;
    private NumericUpDown _numLockInterval;
    private Label _lblStatus;
    private Button _btnSave;
    private Button _btnTest;
    
    public event Action<string, string, string, bool, int> CheatSaved;
    
    public AdvancedCheatEditor()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        this.Text = "Advanced Cheat Editor";
        this.Size = new Size(500, 450);
        this.BackColor = Color.FromArgb(13, 18, 27);
        this.ForeColor = Color.FromArgb(230, 235, 245);
        this.StartPosition = FormStartPosition.CenterParent;
        
        int yPos = 10;
        
        // Name
        var lblName = new Label { Text = "Cheat Name:", Location = new Point(10, yPos), AutoSize = true };
        _txtName = new TextBox { Location = new Point(120, yPos), Size = new Size(360, 24), BackColor = Color.FromArgb(25, 35, 50), ForeColor = Color.FromArgb(230, 235, 245) };
        yPos += 30;
        
        // Search AOB
        var lblSearch = new Label { Text = "Search Pattern:", Location = new Point(10, yPos), AutoSize = true };
        _txtSearchAob = new TextBox { Location = new Point(120, yPos), Size = new Size(360, 24), BackColor = Color.FromArgb(25, 35, 50), ForeColor = Color.FromArgb(230, 235, 245), Multiline = true, Height = 60 };
        yPos += 70;
        
        // Patch AOB
        var lblPatch = new Label { Text = "Patch Pattern:", Location = new Point(10, yPos), AutoSize = true };
        _txtPatchAob = new TextBox { Location = new Point(120, yPos), Size = new Size(360, 24), BackColor = Color.FromArgb(25, 35, 50), ForeColor = Color.FromArgb(230, 235, 245), Multiline = true, Height = 60 };
        yPos += 70;
        
        // Options
        _chkPatchAll = new CheckBox { Text = "Patch All Occurrences", Location = new Point(10, yPos), AutoSize = true };
        yPos += 25;
        
        var lblLock = new Label { Text = "Lock Interval (ms):", Location = new Point(10, yPos), AutoSize = true };
        _numLockInterval = new NumericUpDown { Location = new Point(150, yPos), Size = new Size(80, 24), Minimum = 0, Maximum = 10000 };
        yPos += 30;
        
        // Test Button
        _btnTest = new Button { Text = "Test Pattern", Location = new Point(10, yPos), Size = new Size(100, 30), BackColor = Color.FromArgb(100, 150, 200), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        yPos += 40;
        
        // Status
        _lblStatus = new Label { Text = "Ready", Location = new Point(10, yPos), AutoSize = true, ForeColor = Color.FromArgb(100, 200, 255) };
        yPos += 30;
        
        // Save Button
        _btnSave = new Button { Text = "Save Cheat", Location = new Point(400, yPos), Size = new Size(80, 30), BackColor = Color.FromArgb(76, 200, 120), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
        _btnSave.Click += (s, e) => SaveCheat();
        
        this.Controls.Add(lblName);
        this.Controls.Add(_txtName);
        this.Controls.Add(lblSearch);
        this.Controls.Add(_txtSearchAob);
        this.Controls.Add(lblPatch);
        this.Controls.Add(_txtPatchAob);
        this.Controls.Add(_chkPatchAll);
        this.Controls.Add(lblLock);
        this.Controls.Add(_numLockInterval);
        this.Controls.Add(_btnTest);
        this.Controls.Add(_lblStatus);
        this.Controls.Add(_btnSave);
    }
    
    private void SaveCheat()
    {
        CheatSaved?.Invoke(_txtName.Text, _txtSearchAob.Text, _txtPatchAob.Text, _chkPatchAll.Checked, (int)_numLockInterval.Value);
        this.Close();
    }
}
