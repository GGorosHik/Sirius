using System.Collections;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages;

internal class HandlerAutoRun
{
	public static void Read(Clients client, object[] objects)
	{
		switch ((string)objects[1])
		{
		case "Connect":
		{
			FormAutoRun form2 = (FormAutoRun)Application.OpenForms["AutoRun:" + (string)objects[2]];
			if (form2 == null)
			{
				client.Disconnect();
				break;
			}
			form2.Invoke((MethodInvoker)delegate
			{
				form2.Text = "AutoRun [" + (string)objects[2] + "]";
				form2.client = client;
				form2.materialLabel1.Enabled = true;
				form2.dataGridView2.Enabled = true;
				form2.materialLabel1.Text = "Succues Connect";
			});
			client.Tag = form2;
			client.Hwid = (string)objects[2];
			break;
		}
		case "Error":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormAutoRun form5 = (FormAutoRun)client.Tag;
			form5.Invoke((MethodInvoker)delegate
			{
				form5.materialLabel1.Text = "Error: " + (string)objects[2];
			});
			break;
		}
		case "List":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormAutoRun form3 = (FormAutoRun)client.Tag;
			form3.Invoke((MethodInvoker)delegate
			{
				form3.dataGridView2.Rows.Clear();
				form3.materialLabel1.Text = "Succues auto runs";
				int num = 2;
				while (num < objects.Length)
				{
					DataGridViewRow dataGridViewRow2 = new DataGridViewRow
					{
						Cells = 
						{
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num++]
							}
						}
					};
					form3.dataGridView2.Rows.Add(dataGridViewRow2);
				}
			});
			break;
		}
		case "Set":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormAutoRun form4 = (FormAutoRun)client.Tag;
			form4.Invoke((MethodInvoker)delegate
			{
				form4.materialLabel1.Text = "Succues Set auto run";
				DataGridViewRow dataGridViewRow3 = new DataGridViewRow
				{
					Cells = 
					{
						(DataGridViewCell)new DataGridViewTextBoxCell
						{
							Value = (string)objects[2]
						},
						(DataGridViewCell)new DataGridViewTextBoxCell
						{
							Value = (string)objects[3]
						},
						(DataGridViewCell)new DataGridViewTextBoxCell
						{
							Value = (string)objects[4]
						}
					}
				};
				form4.dataGridView2.Rows.Add(dataGridViewRow3);
			});
			break;
		}
		case "Remove":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormAutoRun form = (FormAutoRun)client.Tag;
			form.Invoke((MethodInvoker)delegate
			{
				form.materialLabel1.Text = "Succues Remove auto run";
				foreach (DataGridViewRow item in (IEnumerable)form.dataGridView2.Rows)
				{
					if ((string)item.Cells[0].Value + ";" + (string)item.Cells[1].Value == (string)objects[2])
					{
						form.dataGridView2.Rows.Remove(item);
					}
				}
			});
			break;
		}
		}
	}
}
