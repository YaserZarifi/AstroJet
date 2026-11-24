using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Rake2D;

public static class StringCipher
{
	private static Rfc2898DeriveBytes _pdb;

	private static Aes encryptor;

	public static void SetPassword(string password)
	{
		_pdb = new Rfc2898DeriveBytes(password, new byte[13]
		{
			73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
			100, 101, 118
		});
		encryptor = Aes.Create();
		encryptor.Key = _pdb.GetBytes(32);
		encryptor.IV = _pdb.GetBytes(16);
	}

	public static string Encrypt(string clearText)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(clearText);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
		{
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.Close();
		}
		clearText = Convert.ToBase64String(memoryStream.ToArray());
		return clearText;
	}

	public static string Decrypt(string cipherText)
	{
		cipherText = cipherText.Replace(" ", "+");
		byte[] array;
		try
		{
			array = Convert.FromBase64String(cipherText);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to decrypt text: '{0}'. {1}", cipherText, ex.ToString());
			return cipherText;
		}
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
		{
			cryptoStream.Write(array, 0, array.Length);
			cryptoStream.Close();
		}
		cipherText = Encoding.Unicode.GetString(memoryStream.ToArray());
		return cipherText;
	}
}
