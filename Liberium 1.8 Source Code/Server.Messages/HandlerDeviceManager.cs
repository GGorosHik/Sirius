using System.Collections;
using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages;

internal class HandlerDeviceManager
{
	public static void Read(Clients client, object[] objects)
	{
		switch ((string)objects[1])
		{
		case "Connect":
		{
			FormDeviceManager form3 = (FormDeviceManager)Application.OpenForms["DeviceManager:" + (string)objects[2]];
			if (form3 == null)
			{
				client.Disconnect();
				break;
			}
			form3.Invoke((MethodInvoker)delegate
			{
				form3.Text = "Device Manager [" + (string)objects[2] + "]";
				form3.client = client;
				form3.materialLabel1.Enabled = true;
				form3.dataGridView2.Enabled = true;
				form3.materialLabel1.Text = "Succues Connect";
			});
			client.Tag = form3;
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
			FormDeviceManager form2 = (FormDeviceManager)client.Tag;
			form2.Invoke((MethodInvoker)delegate
			{
				form2.materialLabel1.Text = "Error: " + (string)objects[2];
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
			FormDeviceManager form4 = (FormDeviceManager)client.Tag;
			form4.Invoke((MethodInvoker)delegate
			{
				form4.dataGridView2.Rows.Clear();
				form4.materialLabel1.Text = "Succues list";
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
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num++]
							}
						}
					};
					form4.dataGridView2.Rows.Add(dataGridViewRow2);
				}
			});
			break;
		}
		case "Status":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormDeviceManager form = (FormDeviceManager)client.Tag;
			form.Invoke((MethodInvoker)delegate
			{
				form.materialLabel1.Text = "Succues status";
				foreach (DataGridViewRow item in (IEnumerable)form.dataGridView2.Rows)
				{
					if (item.Cells[0].Value as string == (string)objects[2])
					{
						item.Cells[2].Value = (string)objects[3];
						break;
					}
				}
			});
			break;
		}
		}
	}
}
