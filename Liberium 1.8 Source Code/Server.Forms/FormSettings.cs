using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using Server.Connectings;
using Server.Data;
using Server.Helper;

namespace Server.Forms;

public class FormSettings : FormMaterial
{
	public static List<Listner> listners = new List<Listner>();

	private IContainer components;

	private RJTextBox rjTextBox1;

	private RJButton rjButton1;

	private GroupBox groupBox1;

	private MaterialLabel materialLabel2;

	private Label materialLabel1;

	private MaterialLabel materialLabel3;

	private NumericUpDown numericUpDown1;

	private GroupBox groupBox2;

	private RJTextBox rjTextBox2;

	public MaterialSwitch materialSwitch1;

	public MaterialSwitch materialSwitch2;

	private GroupBox groupBox3;

	private RJTextBox rjTextBox3;

	private GroupBox groupBox4;

	private MaterialLabel materialLabel4;

	private RJComboBox rjComboBox1;

	private CheckBox checkBox2;

	// New theme controls
	private GroupBox groupBox5;
	private MaterialLabel materialLabel5;
	private RJComboBox rjComboBoxTheme;

	public FormSettings()
	{
		InitializeComponent();
	}

	private void FormSettings_Load(object sender, EventArgs e)
	{
		MaterialSkinManager.Instance.ThemeChanged += ChangeScheme;
		ChangeScheme(this);
		if (File.Exists("local\\Settings.json"))
		{
			Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json"));
			materialSwitch1.Checked = settings.WebHookNewConnect;
			materialSwitch2.Checked = settings.WebHookConnect;
			checkBox2.Checked = settings.AutoStealer;
			rjTextBox2.Texts = settings.WebHook;
			rjTextBox1.Texts = string.Join(",", (IEnumerable<string>)settings.Ports);
			rjTextBox3.Texts = settings.linkMiner;
			rjComboBox1.SelectedIndex = settings.Style;
			rjComboBoxTheme.SelectedIndex = settings.Theme;
			numericUpDown1.Value = settings.second;
		}
		if (listners.Count > 0)
		{
			rjTextBox1.Enabled = false;
			rjButton1.Text = "Stop";
			materialLabel1.Text = "Status: [Listner ports: ";
			string text = "";
			foreach (Listner listner in listners)
			{
				text = text + listner.port + ",";
			}
			text = text.Remove(text.Length - 1, 1);
			Label label = materialLabel1;
			label.Text = label.Text + text + "]";
		}
		if (Certificate.Imported)
		{
			materialLabel2.Text = "Certificate: [Imported]";
		}
		base.FormClosing += ClosingForm;
	}

	private void ChangeScheme(object sender)
	{
		numericUpDown1.ForeColor = FormMaterial.PrimaryColor;
		rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
		rjComboBoxTheme.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox3.BorderColor = FormMaterial.PrimaryColor;
		rjButton1.BackColor = FormMaterial.PrimaryColor;
	}

	private void ClosingForm(object sender, EventArgs e)
	{
		Settings settings = new Settings();
		settings.Ports = rjTextBox1.Texts.Split(',');
		settings.Start = rjButton1.Text == "Stop";
		settings.second = (int)numericUpDown1.Value;
		settings.WebHookNewConnect = materialSwitch1.Checked;
		settings.WebHookConnect = materialSwitch2.Checked;
		settings.AutoStealer = checkBox2.Checked;
		settings.WebHook = rjTextBox2.Texts;
		settings.linkMiner = rjTextBox3.Texts;
		settings.Style = rjComboBox1.SelectedIndex;
		settings.Theme = rjComboBoxTheme.SelectedIndex;
		Program.form.settings = settings;
		File.WriteAllText("local\\Settings.json", JsonConvert.SerializeObject(settings));
	}

	private void rjButton1_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(rjButton1.Text))
		{
			return;
		}
		if (materialLabel2.Text == "Certificate: [Not Exists]")
		{
			new FormCertificate().ShowDialog();
		}
		else if (rjButton1.Text == "Start")
		{
			rjTextBox1.Enabled = false;
			rjButton1.Text = "Stop";
			rjTextBox1.Texts.Split(',').ToList().ForEach(delegate(string item)
			{
				listners.Add(new Listner(Convert.ToInt32(item)));
			});
			materialLabel1.Text = "Status: [Listner ports: ";
			foreach (Listner listner in listners)
			{
				Label label = materialLabel1;
				label.Text = label.Text + listner.port + ",";
			}
			materialLabel1.Text = materialLabel1.Text.Remove(materialLabel1.Text.Length - 1, 1) + "]";
		}
		else
		{
			rjButton1.Text = "Start";
			rjTextBox1.Enabled = true;
			listners.ForEach(delegate(Listner item)
			{
				item.Stop();
			});
			listners.Clear();
			materialLabel1.Text = "Status: [offline]";
		}
	}

	private void rjComboBox1_OnSelectedIndexChanged(object sender, EventArgs e)
	{
		MaterialSkinManager instance = MaterialSkinManager.Instance;
		FormMaterial.GetColorScheme(rjComboBox1.SelectedIndex, instance);
		// Keep current theme setting
		if (rjComboBoxTheme.SelectedIndex == 1)
		{
			instance.Theme = MaterialSkinManager.Themes.DARK;
		}
		else
		{
			instance.Theme = MaterialSkinManager.Themes.LIGHT;
		}
		Refresh();
	}

	private void rjComboBoxTheme_OnSelectedIndexChanged(object sender, EventArgs e)
	{
		MaterialSkinManager instance = MaterialSkinManager.Instance;
		if (rjComboBoxTheme.SelectedIndex == 1)
		{
			instance.Theme = MaterialSkinManager.Themes.DARK;
		}
		else
		{
			instance.Theme = MaterialSkinManager.Themes.LIGHT;
		}
		Refresh();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.rjTextBox1 = new CustomControls.RJControls.RJTextBox();
		this.rjButton1 = new CustomControls.RJControls.RJButton();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.checkBox2 = new System.Windows.Forms.CheckBox();
		this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
		this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
		this.materialLabel1 = new System.Windows.Forms.Label();
		this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.materialSwitch2 = new MaterialSkin.Controls.MaterialSwitch();
		this.materialSwitch1 = new MaterialSkin.Controls.MaterialSwitch();
		this.rjTextBox2 = new CustomControls.RJControls.RJTextBox();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.rjTextBox3 = new CustomControls.RJControls.RJTextBox();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.materialLabel4 = new MaterialSkin.Controls.MaterialLabel();
		this.rjComboBox1 = new CustomControls.RJControls.RJComboBox();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.materialLabel5 = new MaterialSkin.Controls.MaterialLabel();
		this.rjComboBoxTheme = new CustomControls.RJControls.RJComboBox();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).BeginInit();
		this.groupBox2.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.groupBox5.SuspendLayout();
		base.SuspendLayout();
		this.rjTextBox1.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox1.BorderColor = System.Drawing.Color.MediumSlateBlue;
		this.rjTextBox1.BorderFocusColor = System.Drawing.Color.DarkViolet;
		this.rjTextBox1.BorderRadius = 0;
		this.rjTextBox1.BorderSize = 2;
		this.rjTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox1.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox1.Location = new System.Drawing.Point(16, 20);
		this.rjTextBox1.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox1.Multiline = false;
		this.rjTextBox1.Name = "rjTextBox1";
		this.rjTextBox1.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox1.PasswordChar = false;
		this.rjTextBox1.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox1.PlaceholderText = "Ports";
		this.rjTextBox1.Size = new System.Drawing.Size(250, 31);
		this.rjTextBox1.TabIndex = 0;
		this.rjTextBox1.Texts = "";
		this.rjTextBox1.UnderlinedStyle = false;
		this.rjButton1.BackColor = System.Drawing.Color.MediumSlateBlue;
		this.rjButton1.BackgroundColor = System.Drawing.Color.MediumSlateBlue;
		this.rjButton1.BorderColor = System.Drawing.Color.DarkViolet;
		this.rjButton1.BorderRadius = 0;
		this.rjButton1.BorderSize = 0;
		this.rjButton1.FlatAppearance.BorderSize = 0;
		this.rjButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton1.ForeColor = System.Drawing.Color.White;
		this.rjButton1.Location = new System.Drawing.Point(16, 58);
		this.rjButton1.Name = "rjButton1";
		this.rjButton1.Size = new System.Drawing.Size(103, 29);
		this.rjButton1.TabIndex = 1;
		this.rjButton1.Text = "Start";
		this.rjButton1.TextColor = System.Drawing.Color.White;
		this.rjButton1.UseVisualStyleBackColor = false;
		this.rjButton1.Click += new System.EventHandler(rjButton1_Click);
		this.groupBox1.Controls.Add(this.checkBox2);
		this.groupBox1.Controls.Add(this.materialLabel3);
		this.groupBox1.Controls.Add(this.numericUpDown1);
		this.groupBox1.Controls.Add(this.materialLabel1);
		this.groupBox1.Controls.Add(this.materialLabel2);
		this.groupBox1.Controls.Add(this.rjTextBox1);
		this.groupBox1.Controls.Add(this.rjButton1);
		this.groupBox1.Location = new System.Drawing.Point(15, 67);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(407, 200);
		this.groupBox1.TabIndex = 2;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Server";
		this.checkBox2.AutoSize = true;
		this.checkBox2.Location = new System.Drawing.Point(315, 166);
		this.checkBox2.Name = "checkBox2";
		this.checkBox2.Size = new System.Drawing.Size(84, 17);
		this.checkBox2.TabIndex = 7;
		this.checkBox2.Text = "Auto Stealer";
		this.checkBox2.UseVisualStyleBackColor = true;
		this.materialLabel3.AutoSize = true;
		this.materialLabel3.Depth = 0;
		this.materialLabel3.Font = new System.Drawing.Font("Roboto", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		this.materialLabel3.Location = new System.Drawing.Point(193, 68);
		this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialLabel3.Name = "materialLabel3";
		this.materialLabel3.Size = new System.Drawing.Size(116, 19);
		this.materialLabel3.TabIndex = 6;
		this.materialLabel3.Text = "Ping Disconnect";
		this.numericUpDown1.Location = new System.Drawing.Point(315, 67);
		this.numericUpDown1.Maximum = new decimal(new int[4] { 120, 0, 0, 0 });
		this.numericUpDown1.Minimum = new decimal(new int[4] { 15, 0, 0, 0 });
		this.numericUpDown1.Name = "numericUpDown1";
		this.numericUpDown1.Size = new System.Drawing.Size(62, 20);
		this.numericUpDown1.TabIndex = 5;
		this.numericUpDown1.Value = new decimal(new int[4] { 35, 0, 0, 0 });
		this.materialLabel1.AutoSize = true;
		this.materialLabel1.Font = new System.Drawing.Font("Cambria", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.materialLabel1.Location = new System.Drawing.Point(12, 102);
		this.materialLabel1.MaximumSize = new System.Drawing.Size(270, 0);
		this.materialLabel1.Name = "materialLabel1";
		this.materialLabel1.Size = new System.Drawing.Size(134, 22);
		this.materialLabel1.TabIndex = 4;
		this.materialLabel1.Text = "Status: [offline]";
		this.materialLabel2.AutoSize = true;
		this.materialLabel2.Depth = 0;
		this.materialLabel2.Font = new System.Drawing.Font("Roboto", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		this.materialLabel2.Location = new System.Drawing.Point(13, 166);
		this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialLabel2.Name = "materialLabel2";
		this.materialLabel2.Size = new System.Drawing.Size(160, 19);
		this.materialLabel2.TabIndex = 3;
		this.materialLabel2.Text = "Certificate: [Not Exists]";
		this.groupBox2.Controls.Add(this.materialSwitch2);
		this.groupBox2.Controls.Add(this.materialSwitch1);
		this.groupBox2.Controls.Add(this.rjTextBox2);
		this.groupBox2.Location = new System.Drawing.Point(15, 282);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(407, 109);
		this.groupBox2.TabIndex = 7;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Discord Notificator";
		this.materialSwitch2.AutoSize = true;
		this.materialSwitch2.Depth = 0;
		this.materialSwitch2.Location = new System.Drawing.Point(190, 59);
		this.materialSwitch2.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch2.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch2.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch2.Name = "materialSwitch2";
		this.materialSwitch2.Ripple = true;
		this.materialSwitch2.Size = new System.Drawing.Size(116, 37);
		this.materialSwitch2.TabIndex = 63;
		this.materialSwitch2.Text = "Connect";
		this.materialSwitch2.UseVisualStyleBackColor = true;
		this.materialSwitch1.AutoSize = true;
		this.materialSwitch1.Depth = 0;
		this.materialSwitch1.Location = new System.Drawing.Point(16, 59);
		this.materialSwitch1.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch1.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch1.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch1.Name = "materialSwitch1";
		this.materialSwitch1.Ripple = true;
		this.materialSwitch1.Size = new System.Drawing.Size(151, 37);
		this.materialSwitch1.TabIndex = 62;
		this.materialSwitch1.Text = "New Connect";
		this.materialSwitch1.UseVisualStyleBackColor = true;
		this.rjTextBox2.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox2.BorderColor = System.Drawing.Color.MediumSlateBlue;
		this.rjTextBox2.BorderFocusColor = System.Drawing.Color.DarkViolet;
		this.rjTextBox2.BorderRadius = 0;
		this.rjTextBox2.BorderSize = 2;
		this.rjTextBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox2.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox2.Location = new System.Drawing.Point(16, 20);
		this.rjTextBox2.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox2.Multiline = false;
		this.rjTextBox2.Name = "rjTextBox2";
		this.rjTextBox2.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox2.PasswordChar = false;
		this.rjTextBox2.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox2.PlaceholderText = "Webhook";
		this.rjTextBox2.Size = new System.Drawing.Size(384, 31);
		this.rjTextBox2.TabIndex = 0;
		this.rjTextBox2.Texts = "";
		this.rjTextBox2.UnderlinedStyle = false;
		this.groupBox3.Controls.Add(this.rjTextBox3);
		this.groupBox3.Location = new System.Drawing.Point(15, 397);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(407, 65);
		this.groupBox3.TabIndex = 64;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "Miner Download";
		this.rjTextBox3.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox3.BorderColor = System.Drawing.Color.MediumSlateBlue;
		this.rjTextBox3.BorderFocusColor = System.Drawing.Color.DarkViolet;
		this.rjTextBox3.BorderRadius = 0;
		this.rjTextBox3.BorderSize = 2;
		this.rjTextBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox3.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox3.Location = new System.Drawing.Point(16, 20);
		this.rjTextBox3.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox3.Multiline = false;
		this.rjTextBox3.Name = "rjTextBox3";
		this.rjTextBox3.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox3.PasswordChar = false;
		this.rjTextBox3.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox3.PlaceholderText = "url";
		this.rjTextBox3.Size = new System.Drawing.Size(384, 31);
		this.rjTextBox3.TabIndex = 0;
		this.rjTextBox3.Texts = "";
		this.rjTextBox3.UnderlinedStyle = false;
		this.groupBox4.Controls.Add(this.materialLabel4);
		this.groupBox4.Controls.Add(this.rjComboBox1);
		this.groupBox4.Location = new System.Drawing.Point(428, 67);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(407, 100);
		this.groupBox4.TabIndex = 65;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "Color Scheme";
		this.materialLabel4.AutoSize = true;
		this.materialLabel4.Depth = 0;
		this.materialLabel4.Font = new System.Drawing.Font("Roboto", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		this.materialLabel4.Location = new System.Drawing.Point(16, 20);
		this.materialLabel4.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialLabel4.Name = "materialLabel4";
		this.materialLabel4.Size = new System.Drawing.Size(45, 19);
		this.materialLabel4.TabIndex = 1;
		this.materialLabel4.Text = "Color:";
		this.rjComboBox1.BackColor = System.Drawing.Color.WhiteSmoke;
		this.rjComboBox1.BorderColor = System.Drawing.Color.MediumSlateBlue;
		this.rjComboBox1.BorderSize = 1;
		this.rjComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
		this.rjComboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.rjComboBox1.ForeColor = System.Drawing.Color.DimGray;
		this.rjComboBox1.IconColor = System.Drawing.Color.MediumSlateBlue;
		this.rjComboBox1.Items.AddRange(new object[20] { "Purple", "Blue", "Red", "Green", "Orange", "Yellow", "Deep Purple", "Teal", "Cyan", "Light Blue", "Lime", "Indigo", "Deep Orange", "Amber", "Pink", "Light Green", "Blue Grey", "Brown", "Grey", "Dark Grey" });
		this.rjComboBox1.ListBackColor = System.Drawing.Color.FromArgb(230, 228, 245);
		this.rjComboBox1.ListTextColor = System.Drawing.Color.DimGray;
		this.rjComboBox1.Location = new System.Drawing.Point(16, 42);
		this.rjComboBox1.MinimumSize = new System.Drawing.Size(200, 30);
		this.rjComboBox1.Name = "rjComboBox1";
		this.rjComboBox1.Padding = new System.Windows.Forms.Padding(1);
		this.rjComboBox1.Size = new System.Drawing.Size(200, 30);
		this.rjComboBox1.TabIndex = 0;
		this.rjComboBox1.Texts = "";
		this.rjComboBox1.OnSelectedIndexChanged += new System.EventHandler(rjComboBox1_OnSelectedIndexChanged);
		// Theme selection group
		this.groupBox5.Controls.Add(this.materialLabel5);
		this.groupBox5.Controls.Add(this.rjComboBoxTheme);
		this.groupBox5.Location = new System.Drawing.Point(428, 173);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(407, 100);
		this.groupBox5.TabIndex = 66;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "Theme";
		this.materialLabel5.AutoSize = true;
		this.materialLabel5.Depth = 0;
		this.materialLabel5.Font = new System.Drawing.Font("Roboto", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		this.materialLabel5.Location = new System.Drawing.Point(16, 20);
		this.materialLabel5.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialLabel5.Name = "materialLabel5";
		this.materialLabel5.Size = new System.Drawing.Size(52, 19);
		this.materialLabel5.TabIndex = 1;
		this.materialLabel5.Text = "Theme:";
		this.rjComboBoxTheme.BackColor = System.Drawing.Color.WhiteSmoke;
		this.rjComboBoxTheme.BorderColor = System.Drawing.Color.MediumSlateBlue;
		this.rjComboBoxTheme.BorderSize = 1;
		this.rjComboBoxTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
		this.rjComboBoxTheme.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.rjComboBoxTheme.ForeColor = System.Drawing.Color.DimGray;
		this.rjComboBoxTheme.IconColor = System.Drawing.Color.MediumSlateBlue;
		this.rjComboBoxTheme.Items.AddRange(new object[2] { "Light", "Dark" });
		this.rjComboBoxTheme.ListBackColor = System.Drawing.Color.FromArgb(230, 228, 245);
		this.rjComboBoxTheme.ListTextColor = System.Drawing.Color.DimGray;
		this.rjComboBoxTheme.Location = new System.Drawing.Point(16, 42);
		this.rjComboBoxTheme.MinimumSize = new System.Drawing.Size(200, 30);
		this.rjComboBoxTheme.Name = "rjComboBoxTheme";
		this.rjComboBoxTheme.Padding = new System.Windows.Forms.Padding(1);
		this.rjComboBoxTheme.Size = new System.Drawing.Size(200, 30);
		this.rjComboBoxTheme.TabIndex = 0;
		this.rjComboBoxTheme.Texts = "";
		this.rjComboBoxTheme.OnSelectedIndexChanged += new System.EventHandler(rjComboBoxTheme_OnSelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(847, 474);
		base.Controls.Add(this.groupBox5);
		base.Controls.Add(this.groupBox4);
		base.Controls.Add(this.groupBox3);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.MaximizeBox = false;
		base.Name = "FormSettings";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Settings";
		base.Load += new System.EventHandler(FormSettings_Load);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown1).EndInit();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		this.groupBox3.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.groupBox4.PerformLayout();
		this.groupBox5.ResumeLayout(false);
		this.groupBox5.PerformLayout();
		base.ResumeLayout(false);
	}
}

