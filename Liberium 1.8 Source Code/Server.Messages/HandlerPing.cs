using Server.Connectings;

namespace Server.Messages;

internal class HandlerPing
{
	public static void Read(Clients client, object[] objects)
	{
		if (client.Tag == null)
		{
			client.Disconnect();
			return;
		}
		client.lastPing.Last();
		client.Send(new object[1] { "Pong" });
	}
}
