using System.Drawing;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages;

internal class HandlerFileSearcher
{
	public static void Read(Clients client, object[] objects)
	{
		Methods.AppendLogs(client.IP, "Save Files in: Users\\" + objects[1]?.ToString() + "\\FileSearcher", Color.MediumPurple);
		DynamicFiles.Save("Users\\" + objects[1]?.ToString() + "\\FileSearcher", (object[])objects[2]);
	}
}
