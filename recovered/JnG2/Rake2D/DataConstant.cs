using System.Collections.Generic;

namespace Rake2D;

public static class DataConstant
{
	public static readonly string CONST_FILE_NAME = "const.txt";

	private static Dictionary<string, string> _constDictionary;

	private static string _fileName;

	public static void LoadFromFile(string fileName)
	{
		_fileName = null;
		_constDictionary = new Dictionary<string, string>();
		using IniFile iniFile = IniFile.OpenFromContent(fileName);
		if (iniFile == null)
		{
			return;
		}
		_fileName = fileName;
		while (iniFile.NextLine())
		{
			try
			{
				_constDictionary.Add(iniFile.Key.ToLowerLazy(), iniFile.Value);
			}
			catch
			{
				iniFile.LogErr("Duplicate key");
			}
		}
		GameCore.Log("Loaded {0} data constants from file {1}", _constDictionary.Count, fileName);
	}

	public static void Reload()
	{
		if (_fileName != null)
		{
			LoadFromFile(_fileName);
		}
	}

	public static bool TryGetConstant(string key, out string result)
	{
		return _constDictionary.TryGetValue(key, out result);
	}

	public static void ReplaceConst(ref string text)
	{
		if (text.IndexOf('%') < 0)
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in _constDictionary)
		{
			text = text.Replace("%" + item.Key, item.Value);
		}
	}
}
