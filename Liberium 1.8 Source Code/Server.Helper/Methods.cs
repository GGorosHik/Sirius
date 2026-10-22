using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Forms;
using Toolbelt.Drawing;

namespace Server.Helper;

internal class Methods
{
	public static byte[] getIcon(string hash, object[] list)
	{
		for (int i = 1; i < list.Length; i += 2)
		{
			if ((string)list[i] == hash)
			{
				return (byte[])list[i - 1];
			}
		}
		return null;
	}

	public static string Shuffle(string str)
	{
		char[] array = str.ToCharArray();
		Random random = new Random();
		int num = array.Length;
		while (num > 1)
		{
			num--;
			int num2 = random.Next(num + 1);
			char c = array[num2];
			array[num2] = array[num];
			array[num] = c;
		}
		return new string(array);
	}

	public static string GetPublicIpAsync()
	{
		try
		{
			using WebClient webClient = new WebClient();
			return webClient.DownloadString("http://icanhazip.com").Replace("\n", "");
		}
		catch
		{
		}
		return "127.0.0.1";
	}

	public static string GetIcon(string path)
	{
		try
		{
			string text = Path.GetTempFileName() + ".ico";
			using (FileStream stream = new FileStream(text, FileMode.Create))
			{
				IconExtractor.Extract1stIconTo(path, stream);
			}
			return text;
		}
		catch
		{
		}
		return "";
	}

	public static string GetChecksum(string file)
	{
		using FileStream inputStream = File.OpenRead(file);
		return BitConverter.ToString(new SHA256Managed().ComputeHash(inputStream)).Replace("-", string.Empty);
	}

	public static void AppendLogs(string client, string message, Color color)
	{
		DataGridViewRow Item = new DataGridViewRow();
		Item.DefaultCellStyle = new DataGridViewCellStyle
		{
			Alignment = DataGridViewContentAlignment.MiddleLeft,
			ForeColor = color,
			SelectionForeColor = Color.White,
			Font = new Font("Segoe UI", 11f, FontStyle.Regular, GraphicsUnit.Pixel),
			WrapMode = DataGridViewTriState.False
		};
		Item.Cells.Add(new DataGridViewTextBoxCell
		{
			Value = client
		});
		Item.Cells.Add(new DataGridViewTextBoxCell
		{
			Value = DateTime.Now.ToString("HH:mm:ss")
		});
		Item.Cells.Add(new DataGridViewTextBoxCell
		{
			Value = message
		});
		Program.form.GridLogs.Invoke((MethodInvoker)delegate
		{
			Program.form.GridLogs.Rows.Insert(0, Item);
		});
	}

	public static string BytesToString(long byteCount)
	{
		string[] array = new string[7] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
		if (byteCount == 0L)
		{
			return "0" + array[0];
		}
		long num = Math.Abs(byteCount);
		int num2 = Convert.ToInt32(Math.Floor(Math.Log(num, 1024.0)));
		double num3 = Math.Round((double)num / Math.Pow(1024.0, num2), 1);
		return (double)Math.Sign(byteCount) * num3 + " " + array[num2];
	}

	public static Bitmap ByteArrayToBitmap(byte[] byteArray)
	{
		using MemoryStream stream = new MemoryStream(byteArray);
		return new Bitmap(stream);
	}
}
