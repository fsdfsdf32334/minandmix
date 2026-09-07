using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LOVESIX.Core;

public static class UserStore
{
	public const string AccountName = "MIXANDMIN";

	private static string AccountFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIXANDMIN", "account.dat");

	public static bool HasAccount()
	{
		return File.Exists(AccountFile);
	}

	public static void CreateAccount(string password)
	{
		byte[] bytes = RandomNumberGenerator.GetBytes(16);
		byte[] inArray = Hash(password, bytes);
		string? dir = Path.GetDirectoryName(AccountFile);
		if (!string.IsNullOrEmpty(dir))
		{
			Directory.CreateDirectory(dir);
		}
		File.WriteAllText(AccountFile, Convert.ToBase64String(bytes) + "|" + Convert.ToBase64String(inArray));
	}

	public static bool Verify(string password)
	{
		try
		{
			string[] array = File.ReadAllText(AccountFile).Split('|');
			if (array.Length < 2)
			{
				return false;
			}
			byte[] salt = Convert.FromBase64String(array[0]);
			byte[] array2 = Convert.FromBase64String(array[1]);
			return CryptographicOperations.FixedTimeEquals(Hash(password, salt), array2);
		}
		catch
		{
			return false;
		}
	}

	private static byte[] Hash(string password, byte[] salt)
	{
		using SHA256 sHA = SHA256.Create();
		byte[] buffer = salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray();
		return sHA.ComputeHash(buffer);
	}
}
