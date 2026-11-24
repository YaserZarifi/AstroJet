using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Rake2D.JnG;

namespace Rake2D;

public static class TextBundle
{
	[CompilerGenerated]
	private static string Language__BackingField;

	private static Dictionary<string, string> _dictionary;

	private static Regex _regex;

	public static string Language
	{
		[CompilerGenerated]
		get
		{
			return Language__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Language__BackingField = value;
		}
	}

	static TextBundle()
	{
		Language__BackingField = "en";
		_regex = new Regex("\\\\u(?<Value>[a-zA-Z0-9]{4})", RegexOptions.Compiled);
		_dictionary = new Dictionary<string, string>();
	}

	public static string Decoder(string value)
	{
		return _regex.Replace(value, (Match m) => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
	}

	private static void AddText(IniFile iniFile, string key, string text)
	{
		try
		{
			DataConstant.ReplaceConst(ref text);
			_dictionary.Add(key, text);
		}
		catch (Exception ex)
		{
			iniFile.LogErr("Add text bundle key '{0}' failed: {1}", key, ex);
		}
	}

	public static void Load(string language = "en")
	{
		Language = language ?? "en";
		_dictionary.Clear();
		using IniFile iniFile = IniFile.OpenFromContent("text_" + Language.ToLowerLazy() + ".txt");
		if (iniFile == null)
		{
			return;
		}
		bool flag = true;
		bool flag2 = false;
		string key = null;
		while (iniFile.NextLine(flag))
		{
			if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("end"))
			{
				if (!flag)
				{
					flag = true;
				}
				else
				{
					iniFile.LogErr("[end] without [text = ID]");
				}
				continue;
			}
			if (flag)
			{
				if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("text"))
				{
					flag = false;
					key = iniFile.Value.ToLowerLazy();
					AddText(iniFile, key, "");
					flag2 = false;
				}
				else if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("wrap_text"))
				{
					flag = false;
					flag2 = true;
					key = iniFile.Value.ToLowerLazy();
					AddText(iniFile, key, "");
				}
				else
				{
					string value = iniFile.Value.Replace("\\n", "\n");
					AddText(iniFile, iniFile.Key.ToLowerLazy(), Decoder(value));
				}
				continue;
			}
			string text = iniFile.Value.Replace("\\n", "\n");
			if (_dictionary[key].Length > 0)
			{
				if (flag2)
				{
					_dictionary[key] += "\n";
				}
				else if (!_dictionary[key].EndsWith("\n"))
				{
					_dictionary[key] += " ";
				}
			}
			DataConstant.ReplaceConst(ref text);
			_dictionary[key] += text.Trim(' ');
		}
		GameCore.Log("Loaded {0} text keys from file '{1}'.", _dictionary.Count, iniFile.FileName);
	}

	public static bool KeyExists(string key)
	{
		return _dictionary.ContainsKey(key.ToLowerLazy());
	}

	public static string GetText(string key)
	{
		if (!_dictionary.TryGetValue(key.ToLowerLazy(), out var value))
		{
			return key;
		}
		return value;
	}

	public static char ToSpecialChar(string hexaCode)
	{
		return Convert.ToChar(Convert.ToUInt32(hexaCode, 16));
	}

	public static string GetControlText(string key, GameInput.ControllerTypeEnum controller)
	{
		string text = GetText(key);
		text = text.Replace("|key_sort|", "\\1[Ctrl+S]\\-");
		switch (controller)
		{
		case GameInput.ControllerTypeEnum.Mouse:
			text = text.Replace("|key_select|", string.Format("\\3{0}\\-", ToSpecialChar("2711")));
			text = text.Replace("|key_select_ctrl|", string.Format("\\1Ctrl+\\3{0}\\-", ToSpecialChar("2711")));
			text = text.Replace("|key_action|", string.Format("\\3{0}\\-", ToSpecialChar("2712")));
			text = text.Replace("|key_delete|", string.Format("\\3{0}\\-", ToSpecialChar("2712")));
			text = text.Replace("|key_esc|", "\\1[ESC]\\-");
			break;
		case GameInput.ControllerTypeEnum.Keyboard:
			text = text.Replace("|key_select|", "\\1[ENTER]\\-");
			text = text.Replace("|key_action|", "\\1[B]\\-");
			text = text.Replace("|key_delete|", "\\1[DELETE]\\-");
			text = text.Replace("|key_esc|", "\\1[ESC]\\-");
			break;
		case GameInput.ControllerTypeEnum.GamePad:
			text = text.Replace("|key_select|", string.Format("\\3{0}\\-", ToSpecialChar("2718")));
			text = text.Replace("|key_select_ctrl|", string.Format("\\1Ctrl+\\3{0}\\-", ToSpecialChar("2718")));
			text = text.Replace("|key_action|", string.Format("\\3{0}\\-", ToSpecialChar("2719")));
			text = text.Replace("|key_delete|", string.Format("\\3{0}\\-", ToSpecialChar("2719")));
			text = text.Replace("|key_esc|", string.Format("\\3{0}\\-", ToSpecialChar("2722")));
			break;
		}
		return text;
	}

	public static string GetGameModeName(GameModeEnum gameMode)
	{
		_dictionary.TryGetValue($"game_mode_{(int)gameMode}", out var value);
		return value;
	}

	public static string GetGameModeText(GameModeEnum gameMode)
	{
		return GetGameModeName(gameMode);
	}

	public static string GetDifficultyName(int diff)
	{
		if (!_dictionary.TryGetValue($"difficulty_{diff}", out var value))
		{
			return GetText("difficulty_last");
		}
		return value;
	}

	public static string GetDifficultyLabel(GameModeEnum gameMode)
	{
		return GetGameModeText(gameMode) + " " + GetText("hero_stat_difficulty").ToLowerLazy();
	}
}
