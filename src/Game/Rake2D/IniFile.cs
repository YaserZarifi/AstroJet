using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Rake2D;

public class IniFile : IDisposable
{
	public string FileName;

	public ContentArchive ContentArchive;

	public IniFileValueType ValueType;

	public string Key;

	public string Value;

	public string Id;

	public bool IsInComment;

	public bool EOF;

	public int CurrentLineNumber;

	private string[] _lines;

	public string FullName
	{
		get
		{
			if (ContentArchive == null)
			{
				return FileName;
			}
			if (ContentArchive.Type == ContentArchiveTypeEnum.ZipFile)
			{
				return $"[{ContentArchive.Name}]/{FileName}";
			}
			return $"({ContentArchive.Name})/{FileName}";
		}
	}

	public string[] LineArray => _lines;

	public static IniFile OpenFromContent(string fileName)
	{
		if (fileName == null)
		{
			GameCore.LogErr("Unable to open null filename");
			return null;
		}
		IniFile iniFile = new IniFile();
		using Stream stream = GameContent.GetFileStream(fileName, out iniFile.ContentArchive);
		iniFile.FileName = fileName;
		if (stream == null || !iniFile.OpenFromStream(stream))
		{
			GameCore.LogErr("Unable to open ini file '{0}'. Not found in any content archive.", fileName);
			return null;
		}
		return iniFile;
	}

	public bool OpenFromFile(string fileName, bool bLogError = true)
	{
		try
		{
			return OpenFromStream(File.OpenRead(fileName));
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to open ini file. Ex: '{0}'", ex.ToString());
		}
		return false;
	}

	public bool OpenFromStream(Stream stream)
	{
		try
		{
			using (StreamReader streamReader = new StreamReader(stream))
			{
				List<string> list = new List<string>();
				while (!streamReader.EndOfStream)
				{
					string item = streamReader.ReadLine();
					list.Add(item);
				}
				SetLines(list.ToArray());
			}
			return true;
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to open stream. Ex: '{0}'", ex.ToString());
		}
		return false;
	}

	public void SetLines(string[] lines, bool encoded = false)
	{
		_lines = lines;
		if (!_lines.IsNullOrEmpty() && _lines[0] == "JnGeNc")
		{
			_lines[0] = "";
			for (int num = _lines.Length - 1; num > 0; num--)
			{
				_lines[num] = StringCipher.Decrypt(_lines[num]);
			}
		}
		IsInComment = false;
	}

	public bool NextLine(bool bFindKey = true)
	{
		if (_lines == null || CurrentLineNumber >= _lines.Length)
		{
			Key = null;
			Value = null;
			EOF = true;
			return false;
		}
		string text = _lines[CurrentLineNumber];
		int num;
		while (IsInComment)
		{
			num = text.IndexOf("*/");
			if (num >= 0)
			{
				text = text.Substring(num + 2).Trim();
				IsInComment = false;
				break;
			}
			if (CurrentLineNumber >= _lines.Length - 1)
			{
				Key = null;
				Value = null;
				EOF = true;
				return false;
			}
			text = _lines[++CurrentLineNumber];
		}
		CurrentLineNumber++;
		ValueType = IniFileValueType.Normal;
		num = text.IndexOf('#');
		if (num >= 0)
		{
			text = text.Substring(0, num);
		}
		else
		{
			num = text.IndexOf("//");
			if (num >= 0)
			{
				text = text.Substring(0, num);
			}
			else
			{
				num = text.IndexOf("/*");
				if (num >= 0)
				{
					string text2 = text.Substring(0, num);
					int num2 = text.IndexOf("*/");
					string text3;
					if (num2 > num)
					{
						text3 = text.Substring(num2 + 2);
					}
					else
					{
						text3 = "";
						IsInComment = true;
					}
					text = text2 + text3;
				}
			}
		}
		num = text.IndexOf('[');
		if (num >= 0)
		{
			int num3 = text.IndexOf(']');
			if (num3 < num)
			{
				GameCore.LogErr($"Bracket order '[' ']' mismatch at line {CurrentLineNumber} in file '{FileName}'");
			}
			else if (num3 > 0)
			{
				text = text.Substring(num + 1, num3 - num - 1);
				ValueType = IniFileValueType.MainSection;
			}
			else
			{
				GameCore.LogErr($"Missing ending bracket ']' at line {CurrentLineNumber} in file '{FileName}'");
			}
		}
		else
		{
			num = text.IndexOf('<');
			if (num >= 0)
			{
				int num4 = text.IndexOf('>');
				if (num4 < num)
				{
					GameCore.LogErr($"Bracket order '<' '>' mismatch at line {CurrentLineNumber} in file '{FileName}'");
				}
				else if (num4 > 0)
				{
					text = text.Substring(num + 1, num4 - num - 1);
					ValueType = IniFileValueType.SubSection;
				}
				else
				{
					GameCore.LogErr($"Missing ending bracket '>' at line {CurrentLineNumber} in file '{FileName}'");
				}
			}
			else
			{
				num = text.IndexOf('{');
				if (num >= 0)
				{
					int num5 = text.IndexOf('}');
					if (num5 < num)
					{
						GameCore.LogErr($"Bracket order '{' '}' mismatch at line {CurrentLineNumber} in file '{FileName}'");
					}
					else if (num5 > 0)
					{
						text = text.Substring(num + 1, num5 - num - 1);
						ValueType = IniFileValueType.BlockSection;
					}
					else
					{
						GameCore.LogErr($"Missing ending bracket '}}' at line {CurrentLineNumber} in file '{FileName}'");
					}
				}
			}
		}
		if (bFindKey)
		{
			num = text.IndexOf('=');
			if (num > 0)
			{
				Key = text.Substring(0, num).Trim();
				Value = text.Substring(num + 1).Trim();
				if (Value.Length > 2 && Value[0] == '%')
				{
					if (!DataConstant.TryGetConstant(Value.Substring(1), out var result))
					{
						LogErr("Undefined constant '{0}'", Value);
					}
					else
					{
						Value = result;
					}
				}
			}
			else
			{
				Key = text.Trim();
				Value = "";
				Id = null;
			}
			num = Key.IndexOf('.');
			if (num >= 0)
			{
				Id = Key.Substring(num + 1).Trim();
				Key = Key.Substring(0, num).Trim();
			}
			else
			{
				Id = null;
			}
			if (Key.Length <= 0 && !NextLine(bFindKey))
			{
				return false;
			}
		}
		else
		{
			Value = text;
			Key = text.Trim();
		}
		return true;
	}

	public bool TryGetFloat(out float result)
	{
		if (!float.TryParse(Value, out result))
		{
			LogErr("Wrong float value ('{0}')", Value);
			return false;
		}
		return true;
	}

	public int[] GetIntArray()
	{
		int[] array = null;
		if (Value != null)
		{
			string[] array2 = Value.Split(',');
			array = new int[array2.Length];
			int num = 0;
			string[] array3 = array2;
			foreach (string s in array3)
			{
				int result = 0;
				int.TryParse(s, out result);
				array[num++] = result;
			}
		}
		else
		{
			LogErr("Missing params");
		}
		return array;
	}

	public bool TryGetIntArray(int wantedLength, out int[] resultArray)
	{
		int[] intArray = GetIntArray();
		if (intArray == null)
		{
			resultArray = null;
			return false;
		}
		if (intArray.Length == wantedLength)
		{
			resultArray = intArray;
			return true;
		}
		LogErr("Wrong param count ({0} <> {1})", intArray.Length, wantedLength);
		resultArray = null;
		return false;
	}

	public bool TryGetIntArray(int lenMin, int lenMax, out int[] resultArray)
	{
		int[] intArray = GetIntArray();
		if (intArray == null)
		{
			resultArray = null;
			return false;
		}
		if (intArray != null && intArray.Length >= lenMin && intArray.Length <= lenMax)
		{
			resultArray = intArray;
			return true;
		}
		LogErr("Wrong param count ({0} <> [{1} - {2}])", intArray.Length, lenMin, lenMax);
		resultArray = null;
		return false;
	}

	public bool TryGetFloatArray(int wantedLength, out float[] resultArray)
	{
		float[] floatArray = GetFloatArray();
		if (floatArray == null)
		{
			resultArray = null;
			return false;
		}
		if (floatArray.Length == wantedLength)
		{
			resultArray = floatArray;
			return true;
		}
		LogErr("Wrong param count ({0} <> {1})", floatArray.Length, wantedLength);
		resultArray = null;
		return false;
	}

	public bool TryGetFloatArray(int lenMin, int lenMax, out float[] resultArray)
	{
		float[] floatArray = GetFloatArray();
		if (floatArray == null)
		{
			resultArray = null;
			return false;
		}
		if (floatArray != null && floatArray.Length >= lenMin && floatArray.Length <= lenMax)
		{
			resultArray = floatArray;
			return true;
		}
		LogErr("Wrong param count ({0} <> [{1} - {2}])", floatArray.Length, lenMin, lenMax);
		resultArray = null;
		return false;
	}

	public bool TryGetStringArray(int wantedLength, out string[] resultArray)
	{
		string[] stringArray = GetStringArray();
		if (stringArray == null)
		{
			resultArray = null;
			return false;
		}
		if (stringArray.Length == wantedLength)
		{
			resultArray = stringArray;
			return true;
		}
		LogErr("Wrong param count ({0} <> {1})", stringArray.Length, wantedLength);
		resultArray = null;
		return false;
	}

	public bool TryGetStringArray(int lenMin, int lenMax, out string[] resultArray)
	{
		string[] stringArray = GetStringArray();
		if (stringArray == null)
		{
			resultArray = null;
			return false;
		}
		if (stringArray != null && stringArray.Length >= lenMin && stringArray.Length <= lenMax)
		{
			resultArray = stringArray;
			return true;
		}
		LogErr("Wrong param count ({0} <> [{1} - {2}])", stringArray.Length, lenMin, lenMax);
		resultArray = null;
		return false;
	}

	public bool TryGetStringArrayFromId(int lenMin, int lenMax, out string[] resultArray)
	{
		string[] idStringArray = GetIdStringArray();
		if (idStringArray == null)
		{
			resultArray = null;
			return false;
		}
		if (idStringArray != null && idStringArray.Length >= lenMin && idStringArray.Length <= lenMax)
		{
			resultArray = idStringArray;
			return true;
		}
		resultArray = null;
		return false;
	}

	public bool TryGetColorValue(out Color color)
	{
		if (TryGetIntArray(3, 4, out var resultArray))
		{
			if (resultArray.Length == 3)
			{
				color = new Color(resultArray[0], resultArray[1], resultArray[2], 255);
			}
			else
			{
				color = new Color(resultArray[0], resultArray[1], resultArray[2], resultArray[3]);
			}
			return true;
		}
		color = Color.White;
		return false;
	}

	public string[] GetStringArray()
	{
		string[] array = null;
		if (Value != null)
		{
			string[] array2 = Value.Split(',');
			array = new string[array2.Length];
			int num = 0;
			string[] array3 = array2;
			foreach (string text in array3)
			{
				array[num++] = text.Trim();
			}
		}
		return array;
	}

	public string[] GetIdStringArray()
	{
		string[] array = null;
		if (Id != null)
		{
			string[] array2 = Id.Split(',');
			array = new string[array2.Length];
			int num = 0;
			string[] array3 = array2;
			foreach (string text in array3)
			{
				array[num++] = text.Trim();
			}
		}
		return array;
	}

	public IntRange GetIdIntRange()
	{
		IntRange result = default;
		if (Id != null)
		{
			string[] array = Id.Split(',');
			if (array.Length == 1)
			{
				int result2 = 0;
				int.TryParse(array[0], out result2);
				result.From = (result.To = result2);
			}
			else
			{
				int num = 0;
				string[] array2 = array;
				foreach (string s in array2)
				{
					int result3 = 0;
					int.TryParse(s, out result3);
					if (num == 0)
					{
						result.From = result3;
						num++;
						continue;
					}
					result.To = result3;
					break;
				}
			}
		}
		return result;
	}

	public float[] GetFloatArray()
	{
		float[] array = null;
		if (Value != null)
		{
			string[] array2 = Value.Split(',');
			array = new float[array2.Length];
			int num = 0;
			string[] array3 = array2;
			foreach (string str in array3)
			{
				array[num++] = Utils.StringToFloat(str);
			}
		}
		return array;
	}

	public bool TryGetIntRange(ref IntRange range)
	{
		int[] intArray = GetIntArray();
		if (intArray.Length == 1)
		{
			range.From = intArray[0];
			range.To = intArray[0];
		}
		else
		{
			if (intArray.Length != 2)
			{
				LogErr("Wrong param count in range value ({0} <> 1 or 2)", intArray.Length);
				return false;
			}
			range.From = intArray[0];
			range.To = intArray[1];
		}
		return true;
	}

	public bool TryGetFloatRange(ref FloatRange range)
	{
		float[] floatArray = GetFloatArray();
		if (floatArray.Length == 1)
		{
			range.From = floatArray[0];
			range.To = floatArray[0];
		}
		else
		{
			if (floatArray.Length != 2)
			{
				LogErr("Wrong param count in range value ({0} <> 1 or 2)", floatArray.Length);
				return false;
			}
			range.From = floatArray[0];
			range.To = floatArray[1];
		}
		return true;
	}

	public int GetIntValue(int defaultValue = 0)
	{
		if (!int.TryParse(Value, out var result))
		{
			LogErr("Value '{0}' is not a number.", Value);
			return defaultValue;
		}
		return result;
	}

	public long GetLongValue(long defaultValue = 0L)
	{
		if (!long.TryParse(Value, out var result))
		{
			LogErr("Value '{0}' is not a long number.", Value);
			return defaultValue;
		}
		return result;
	}

	public float GetFloatValue(float defaultValue = 0f)
	{
		if (!float.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public double GetDoubleValue(double defaultValue = 0.0)
	{
		if (!double.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	public bool GetBoolValue(bool defaultValue = false)
	{
		if (!int.TryParse(Value, out var result))
		{
			return defaultValue;
		}
		return result != 0;
	}

	public Vector2 GetVector2()
	{
		return Utils.GetVector2(GetFloatArray());
	}

	public float GetPercentage(int defaultValue = 100)
	{
		if (!int.TryParse(Value, out var result))
		{
			LogErr("Value '{0}' is not a percentage number.", Value);
			result = defaultValue;
		}
		return (float)result / 100f;
	}

	public int GetIdIntValue(int defaultValue = 0)
	{
		if (int.TryParse(Id, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public bool KeyEquals(string value)
	{
		return Key.Equals(value, StringComparison.OrdinalIgnoreCase);
	}

	public bool IdEquals(string value)
	{
		if (Id == null)
		{
			return false;
		}
		return Id.Equals(value, StringComparison.OrdinalIgnoreCase);
	}

	public bool ValueEquals(string value)
	{
		return Value.Equals(value, StringComparison.OrdinalIgnoreCase);
	}

	public void Close()
	{
		_lines = null;
		FileName = null;
	}

	public void LogErr(string message, params object[] args)
	{
		string text = string.Format(message, args);
		GameCore.LogErr("{0}. Key '{1}' in file '{2}' at line {3}", text, Key, FullName, CurrentLineNumber);
	}

	public string GetPathFromValue()
	{
		try
		{
			return DataUtils.GetPathFromValue(Value, Path.GetDirectoryName(FileName));
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Wrong filenam format: {0}: {1}", Value, ex);
			return Value;
		}
	}

	public void SaveToFile(string fileName)
	{
		fileName = ((ContentArchive == null || ContentArchive.Type != ContentArchiveTypeEnum.Directory) ? Path.Combine(GameContent.GetFirstArchive(ContentArchiveTypeEnum.Directory)?.Name, fileName) : Path.Combine(ContentArchive.Name, fileName));
		try
		{
			new FileInfo(fileName).Directory.Create();
			using StreamWriter streamWriter = new StreamWriter(fileName);
			string[] lines = _lines;
			foreach (string value in lines)
			{
				streamWriter.WriteLine(value);
			}
		}
		catch (Exception ex)
		{
			LogErr("Ex: {0}", ex);
		}
	}

	public void ReplaceMainSection(string[] sectionNameArray, string[] ignoreSectionNameArray, string value, List<string> lineList)
	{
		int num = -1;
		int num2 = -1;
		while (NextLine())
		{
			if (ValueType == IniFileValueType.MainSection && ValueEquals(value) && sectionNameArray.Count((string sectionName) => KeyEquals(sectionName)) > 0)
			{
				num = CurrentLineNumber - 1;
			}
			else
			{
				if (num < 0 || (ValueType != IniFileValueType.MainSection && !Key.ToLowerLazy().StartsWith("editor_")))
				{
					continue;
				}
				bool flag = true;
				if (ignoreSectionNameArray != null)
				{
					foreach (string value2 in ignoreSectionNameArray)
					{
						if (KeyEquals(value2))
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					num2 = CurrentLineNumber - 1;
					break;
				}
			}
		}
		List<string> list;
		if (num >= 0)
		{
			list = new List<string>(_lines.Take(num).ToArray());
			list.AddRange(lineList);
			if (num2 >= 0)
			{
				list.AddRange(_lines.Skip(num2));
			}
		}
		else
		{
			list = _lines.ToList();
			list.AddRange(lineList);
		}
		_lines = list.ToArray();
	}

	public void Dispose()
	{
		Close();
	}
}
