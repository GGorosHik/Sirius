using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms;

public class FormSpeakerBot : FormMaterial
{
	public Clients client;

	public Clients parrent;

	private IContainer components;

	private Timer timer1;

	private Panel panel1;

	public RichTextBox richTextBox1;

	private MaterialLabel materialLabel1;

	private MaterialLabel materialLabel2;

	private MaterialLabel materialLabel3;

	public TrackBar trackBar1;

	public TrackBar trackBar2;

	public RJComboBox rjComboBox1;

	public RJButton rjButton1;

	public FormSpeakerBot()
	{
		InitializeComponent();
		base.FormClosing += Closing1;
	}

	private void FormProcess_Load(object sender, EventArgs e)
	{
		MaterialSkinManager.Instance.ThemeChanged += ChangeScheme;
		ChangeScheme(this);
		timer1.Start();
	}

	private void ChangeScheme(object sender)
	{
		rjButton1.BorderColor = FormMaterial.PrimaryColor;
		rjButton1.ForeColor = FormMaterial.PrimaryColor;
		richTextBox1.ForeColor = FormMaterial.PrimaryColor;
		rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
	}

	private void Closing1(object sender, EventArgs e)
	{
		if (client != null)
		{
			client.Disconnect();
		}
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

	private void rjButton1_Click_1(object sender, EventArgs e)
	{
		if (client.itsConnect && rjComboBox1.SelectedIndex != 0)
		{
			client.Send(new object[5]
			{
				"Speak",
				trackBar2.Value,
				trackBar1.Value,
				((string)rjComboBox1.SelectedItem).Split(new string[1] { " | " }, StringSplitOptions.None)[0],
				richTextBox1.Text
			});
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
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.panel1 = new System.Windows.Forms.Panel();
		this.rjButton1 = new CustomControls.RJControls.RJButton();
		this.rjComboBox1 = new CustomControls.RJControls.RJComboBox();
		this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
		this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
		this.trackBar1 = new System.Windows.Forms.TrackBar();
		this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
		this.trackBar2 = new System.Windows.Forms.TrackBar();
		this.richTextBox1 = new System.Windows.Forms.RichTextBox();
		this.panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.trackBar2).BeginInit();
		base.SuspendLayout();
		this.timer1.Interval = 1000;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		this.panel1.Controls.Add(this.rjButton1);
		this.panel1.Controls.Add(this.rjComboBox1);
		this.panel1.Controls.Add(this.materialLabel3);
		this.panel1.Controls.Add(this.materialLabel2);
		this.panel1.Controls.Add(this.trackBar1);
		this.panel1.Controls.Add(this.materialLabel1);
		this.panel1.Controls.Add(this.trackBar2);
		this.panel1.Controls.Add(this.richTextBox1);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(3, 64);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(524, 479);
		this.panel1.TabIndex = 0;
		this.rjButton1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.rjButton1.BackColor = System.Drawing.Color.White;
		this.rjButton1.BackgroundColor = System.Drawing.Color.White;
		this.rjButton1.BorderColor = System.Drawing.Color.MediumSlateBlue;
		this.rjButton1.BorderRadius = 0;
		this.rjButton1.BorderSize = 1;
		this.rjButton1.Enabled = false;
		this.rjButton1.FlatAppearance.BorderSize = 0;
		this.rjButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton1.ForeColor = System.Drawing.Color.MediumSlateBlue;
		this.rjButton1.Location = new System.Drawing.Point(395, 376);
		this.rjButton1.Name = "rjButton1";
		this.rjButton1.Size = new System.Drawing.Size(92, 30);
		this.rjButton1.TabIndex = 36;
		this.rjButton1.Text = "Speak";
		this.rjButton1.TextColor = System.Drawing.Color.MediumSlateBlue;
		this.rjButton1.UseVisualStyleBackColor = false;
		this.rjButton1.Click += new System.EventHandler(rjButton1_Click_1);
		this.rjComboBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.rjComboBox1.BackColor = System.Drawing.Color.WhiteSmoke;
		this.rjComboBox1.BorderColor = System.Drawing.Color.MediumSlateBlue;
		this.rjComboBox1.BorderSize = 1;
		this.rjComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
		this.rjComboBox1.Enabled = false;
		this.rjComboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.rjComboBox1.ForeColor = System.Drawing.Color.DimGray;
		this.rjComboBox1.IconColor = System.Drawing.Color.MediumSlateBlue;
		this.rjComboBox1.ListBackColor = System.Drawing.Color.FromArgb(230, 228, 245);
		this.rjComboBox1.ListTextColor = System.Drawing.Color.DimGray;
		this.rjComboBox1.Location = new System.Drawing.Point(98, 429);
		this.rjComboBox1.MinimumSize = new System.Drawing.Size(200, 30);
		this.rjComboBox1.Name = "rjComboBox1";
		this.rjComboBox1.Padding = new System.Windows.Forms.Padding(1);
		this.rjComboBox1.Size = new System.Drawing.Size(250, 30);
		this.rjComboBox1.TabIndex = 35;
		this.rjComboBox1.Texts = "";
		this.materialLabel3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.materialLabel3.AutoSize = true;
		this.materialLabel3.Depth = 0;
		this.materialLabel3.Font = new System.Drawing.Font("Roboto", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		this.materialLabel3.Location = new System.Drawing.Point(7, 428);
		this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialLabel3.Name = "materialLabel3";
		this.materialLabel3.Size = new System.Drawing.Size(48, 19);
		this.materialLabel3.TabIndex = 34;
		this.materialLabel3.Text = "Voices";
		this.materialLabel2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.materialLabel2.AutoSize = true;
		this.materialLabel2.Depth = 0;
		this.materialLabel2.Font = new System.Drawing.Font("Roboto", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		this.materialLabel2.Location = new System.Drawing.Point(7, 340);
		this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialLabel2.Name = "materialLabel2";
		this.materialLabel2.Size = new System.Drawing.Size(55, 19);
		this.materialLabel2.TabIndex = 33;
		this.materialLabel2.Text = "Volume";
		this.trackBar1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.trackBar1.Enabled = false;
		this.trackBar1.Location = new System.Drawing.Point(98, 327);
		this.trackBar1.Maximum = 100;
		this.trackBar1.Name = "trackBar1";
		this.trackBar1.Size = new System.Drawing.Size(250, 45);
		this.trackBar1.TabIndex = 30;
		this.trackBar1.Value = 100;
		this.materialLabel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.materialLabel1.AutoSize = true;
		this.materialLabel1.Depth = 0;
		this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		this.materialLabel1.Location = new System.Drawing.Point(7, 387);
		this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialLabel1.Name = "materialLabel1";
		this.materialLabel1.Size = new System.Drawing.Size(85, 19);
		this.materialLabel1.TabIndex = 32;
		this.materialLabel1.Text = "Tone Speak";
		this.trackBar2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.trackBar2.Enabled = false;
		this.trackBar2.Location = new System.Drawing.Point(98, 378);
		this.trackBar2.Minimum = -10;
		this.trackBar2.Name = "trackBar2";
		this.trackBar2.Size = new System.Drawing.Size(250, 45);
		this.trackBar2.TabIndex = 31;
		this.richTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.richTextBox1.BackColor = System.Drawing.Color.White;
		this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.richTextBox1.Enabled = false;
		this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.richTextBox1.ForeColor = System.Drawing.Color.MediumSlateBlue;
		this.richTextBox1.Location = new System.Drawing.Point(0, 0);
		this.richTextBox1.Name = "richTextBox1";
		this.richTextBox1.Size = new System.Drawing.Size(521, 321);
		this.richTextBox1.TabIndex = 2;
		this.richTextBox1.Text = "Your fucking computer infected by Leberium rat";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.White;
		base.ClientSize = new System.Drawing.Size(530, 546);
		base.Controls.Add(this.panel1);
		base.Name = "FormSpeakerBot";
		this.Text = "Speaker Bot";
		base.Load += new System.EventHandler(FormProcess_Load);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.trackBar2).EndInit();
		base.ResumeLayout(false);
	}
}
