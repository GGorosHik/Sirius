using System;
using System.Text;

namespace Server.Helper;

internal class Randomizer
{
	public static string[] LegalNaming = new string[2] { "Guna", "MetroFramework" };

	public static Random random { get; private set; } = new Random();

	public static string getRandomCharacters()
	{
		return getRandomCharacters(random.Next(6, 32));
	}

	public static string getRandomCharacters(int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i <= count; i++)
		{
			int index = random.Next(0, "asdfghjklqwertyuiopmnbvcxz123456890+_)(*&^%$#@!".Length);
			stringBuilder.Append("asdfghjklqwertyuiopmnbvcxz123456790+_)(*&^%$#@!"[index]);
		}
		return stringBuilder.ToString();
	}

	public static string getRandomCharactersAscii()
	{
		return getRandomCharactersAscii(random.Next(6, 32));
	}

	public static string getRandomCharactersAscii(int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i <= count; i++)
		{
			int index = random.Next(0, "asdfghjklqwertyuiopmnbvcxz123456890".Length);
			stringBuilder.Append("asdfghjklqwertyuiopmnbvcxz123456790"[index]);
		}
		return stringBuilder.ToString();
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
}
