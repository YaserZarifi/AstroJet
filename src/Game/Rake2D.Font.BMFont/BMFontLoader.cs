using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Rake2D.Font.BMFont;

public static class BMFontLoader
{
	public static BMFont LoadFontFromFile(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentNullException("fileName", "File name not specified");
		}
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException($"Cannot find file '{fileName}'", fileName);
		}
		using FileStream stream = File.OpenRead(fileName);
		using TextReader textReader = new StreamReader(stream);
		if (textReader.ReadLine().StartsWith("info "))
		{
			return LoadFontFromTextFile(fileName);
		}
		throw new InvalidDataException("Unknown file format.");
	}

	public static BMFont LoadFontFromTextFile(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentNullException("fileName");
		}
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException($"Cannot find file '{fileName}'", fileName);
		}
		BMFont bMFont = new BMFont();
		using (Stream stream = File.OpenRead(fileName))
		{
			bMFont.LoadText(stream);
		}
		QualifyResourcePaths(bMFont, Path.GetDirectoryName(fileName));
		return bMFont;
	}

	internal static bool GetNamedBool(string[] parts, string name, bool defaultValue = false)
	{
		if (int.TryParse(GetNamedString(parts, name), out var result))
		{
			return result > 0;
		}
		return defaultValue;
	}

	internal static int GetNamedInt(string[] parts, string name, int defaultValue = 0)
	{
		if (!int.TryParse(GetNamedString(parts, name), out var result))
		{
			return defaultValue;
		}
		return result;
	}

	internal static string GetNamedString(string[] parts, string name)
	{
		string result = string.Empty;
		foreach (string text in parts)
		{
			int num = text.IndexOf('=');
			if (num == -1)
			{
				continue;
			}
			string b = text.Substring(0, num);
			string text2 = text.Substring(num + 1);
			if (string.Equals(name, b, StringComparison.InvariantCultureIgnoreCase))
			{
				int length = text2.Length;
				if (length > 1 && text2[0] == '"' && text2[length - 1] == '"')
				{
					text2 = text2.Substring(1, length - 2);
				}
				result = text2;
				break;
			}
		}
		return result;
	}

	internal static Padding ParsePadding(string s)
	{
		string[] array = s.Split(',');
		return new Padding
		{
			Left = Convert.ToInt32(array[3].Trim()),
			Top = Convert.ToInt32(array[0].Trim()),
			Right = Convert.ToInt32(array[1].Trim()),
			Bottom = Convert.ToInt32(array[2].Trim())
		};
	}

	internal static Point ParsePoint(string s)
	{
		string[] array = s.Split(',');
		return new Point
		{
			X = Convert.ToInt32(array[0].Trim()),
			Y = Convert.ToInt32(array[1].Trim())
		};
	}

	internal static void QualifyResourcePaths(BMFont font, string resourcePath)
	{
		Page[] pages = font.Pages;
		for (int i = 0; i < pages.Length; i++)
		{
			Page page = pages[i];
			page.FileName = Path.Combine(resourcePath, page.FileName);
			pages[i] = page;
		}
		font.Pages = pages;
	}

	internal static string[] Split(string s, char delimiter)
	{
		if (s.IndexOf('"') != -1)
		{
			int num = -1;
			List<string> list = new List<string>();
			do
			{
				int num2 = s.IndexOf('"', num + 1);
				int num3 = s.IndexOf('"', num2 + 1);
				int num4 = s.IndexOf(delimiter, num + 1);
				if (num4 == -1)
				{
					num4 = s.Length;
				}
				int num5;
				if (num2 != -1 && num4 > num2)
				{
					num5 = ((num4 < num3) ? 1 : 0);
					if (num5 != 0)
					{
						num4 = s.IndexOf(delimiter, num3 + 1);
					}
				}
				else
				{
					num5 = 0;
				}
				list.Add(s.Substring(num + 1, num4 - num - 1));
				if (num5 != 0)
				{
					num = num4 - 1;
				}
				num = s.IndexOf(delimiter, num + 1);
			}
			while (num != -1);
			return list.ToArray();
		}
		return s.Split(new char[1] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
	}

	internal static T[] ToArray<T>(ICollection<T> values)
	{
		T[] array = new T[values.Count];
		values.CopyTo(array, 0);
		return array;
	}
}
