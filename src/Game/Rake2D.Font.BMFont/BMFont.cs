using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Font.BMFont;

public class BMFont : IEnumerable<Character>, IEnumerable
{
	public const int NoMaxWidth = -1;

	public List<Texture2D> TextureList;

	private SpriteBatch _spriteBatch;

	private Color[] _colorArray;

	public int AlphaChannel { get; set; }

	public int BaseHeight { get; set; }

	public int BlueChannel { get; set; }

	public bool Bold { get; set; }

	public IDictionary<char, Character> Characters { get; set; }

	public string Charset { get; set; }

	public string FamilyName { get; set; }

	public int FontSize { get; set; }

	public int GreenChannel { get; set; }

	public bool Italic { get; set; }

	public Character this[char character]
	{
		get
		{
			Characters.TryGetValue(character, out var value);
			return value;
		}
	}

	public IDictionary<Kerning, int> Kernings { get; set; }

	public int LineHeight { get; set; }

	public int OutlineSize { get; set; }

	public bool Packed { get; set; }

	public Padding Padding { get; set; }

	public Page[] Pages { get; set; }

	public int RedChannel { get; set; }

	public bool Smoothed { get; set; }

	public Point Spacing { get; set; }

	public int StretchedHeight { get; set; }

	public int SuperSampling { get; set; }

	public Vector2 TextureSize { get; set; }

	public bool Unicode { get; set; }

	public BMFont()
	{
		_colorArray = new Color[10];
		for (int i = 0; i < _colorArray.Length; i++)
		{
			_colorArray[i] = Color.White;
		}
	}

	public void SetColor(int arrayIndex, Color color)
	{
		if (arrayIndex >= 0 && arrayIndex < _colorArray.Length)
		{
			_colorArray[arrayIndex] = color;
		}
	}

	public int GetKerning(char previous, char current)
	{
		Kerning key = new Kerning(previous, current, 0);
		if (!Kernings.TryGetValue(key, out var value))
		{
			return 0;
		}
		return value;
	}

	public void Init()
	{
		TextureList = new List<Texture2D>();
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		Page[] pages = Pages;
		for (int i = 0; i < pages.Length; i++)
		{
			Page page = pages[i];
			Texture2D texture2D = GameCore.GlobalContent.LoadTexture(page.FileName, page.FileName);
			if (texture2D != null)
			{
				TextureList.Add(texture2D);
				continue;
			}
			GameCore.LogErr("Unable to load font texture '{0}'", page.FileName);
		}
	}

	private void WriteCharacter(SpriteBatch spriteBatch, Character character, Vector2 position, Vector2 scale, Color? color)
	{
		Texture2D texture = TextureList[character.TexturePage];
		spriteBatch.Draw(texture, position, character.Bounds, color.Value, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
	}

	public float GetTextHeight(string text, float scaleY)
	{
		int num = 1;
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			if (text[i] == '\n')
			{
				num++;
			}
		}
		return (float)(num * LineHeight) * scaleY;
	}

	public int Write(SpriteBatch spriteBatch, string text, float x, float y, Vector2 scale, Color? color = null, TextAlign align = TextAlign.Left, int maxWidth = 0, float alphaFactor = 1f, float lineHeight = -1f, BlendModeEnum blendMode = BlendModeEnum.Normal)
	{
		Color color2 = color ?? _colorArray[0];
		Color currentColor = color2;
		return WriteSubstring(spriteBatch, text, x, y, scale, color2, ref currentColor, align, 0, -1, maxWidth, alphaFactor, lineHeight, blendMode);
	}

	private int WriteSubstring(SpriteBatch spriteBatch, string text, float x, float y, Vector2 scale, Color initColor, ref Color currentColor, TextAlign align = TextAlign.Left, int offset = 0, int length = -1, int maxWidth = 0, float alphaFactor = 1f, float lineHeight = -1f, BlendModeEnum blendMode = BlendModeEnum.Normal)
	{
		float num = x;
		int num2 = 1;
		switch (align)
		{
		case TextAlign.Center:
		{
			int length3 = text.IndexOf('\n');
			x -= MeasureFont(text, 0, length3).X / 2f * scale.X;
			break;
		}
		case TextAlign.Right:
		{
			int length2 = text.IndexOf('\n');
			x -= MeasureFont(text, 0, length2).X * scale.X;
			break;
		}
		}
		Color color = currentColor;
		_ = Matrix.Identity;
		char previous = ' ';
		bool flag = false;
		int num3 = text.Length;
		if (offset < 0)
		{
			offset = 0;
		}
		else if (offset > num3)
		{
			offset = num3;
		}
		if (length >= 0 && offset + length < num3)
		{
			num3 = offset + length;
		}
		for (int i = offset; i < num3; i++)
		{
			char c = text[i];
			if (flag)
			{
				color = (char.IsDigit(c) ? (currentColor = _colorArray[c - 48]) : (currentColor = initColor));
				flag = false;
				continue;
			}
			switch (c)
			{
			case '\\':
				flag = true;
				continue;
			case '\n':
				x = num;
				switch (align)
				{
				case TextAlign.Center:
				{
					int num5 = text.IndexOf('\n', i + 1);
					x -= MeasureFont(text, i + 1, (num5 >= 0) ? (num5 - i - 1) : (-1)).X / 2f * scale.X;
					break;
				}
				case TextAlign.Right:
				{
					int num4 = text.IndexOf('\n', i + 1);
					x -= MeasureFont(text, i + 1, (num4 >= 0) ? (num4 - i - 1) : (-1)).X * scale.X;
					break;
				}
				}
				y += ((lineHeight > 0f) ? lineHeight : ((float)LineHeight * scale.Y));
				num2++;
				continue;
			case '\r':
				continue;
			}
			Color value = color;
			value.A = (byte)((float)(int)color.A * alphaFactor);
			if (blendMode == BlendModeEnum.Additive)
			{
				value *= (float)(int)value.A / 255f;
				value.A = 0;
			}
			else
			{
				value = Color.FromNonPremultiplied(value.R, value.G, value.B, value.A);
			}
			Character character = this[c];
			int kerning = GetKerning(previous, c);
			float num6 = (float)(character.XAdvance + kerning) * scale.X;
			if (maxWidth <= 0 || (float)maxWidth > x + num6 - num)
			{
				WriteCharacter(spriteBatch, character, new Vector2(x + (float)(character.Offset.X + kerning) * scale.X, y + (float)character.Offset.Y * scale.Y), scale, value);
				x += num6;
			}
			previous = c;
		}
		return num2;
	}

	public int WrapWrite(SpriteBatch spriteBatch, string text, float x, float y, int lineWidth, Vector2 scale, Color? color = null, float alphaFactor = 1f, int lineHeight = -1)
	{
		int num = 0;
		int num2 = 0;
		int length = text.Length;
		bool flag = false;
		int num3 = 0;
		Color color2 = color ?? _colorArray[0];
		Color currentColor = color2;
		while (num < length)
		{
			if (flag)
			{
				if (num2 >= length)
				{
					break;
				}
				if (text[num2] == ' ')
				{
					num2++;
				}
				else
				{
					flag = false;
					num = num2;
				}
			}
			int i;
			for (i = num2; i < length && text[i] != ' ' && text[i] != '\n'; i++)
			{
			}
			float num4 = MeasureFont(text, num, i - num).X * scale.X;
			bool flag2 = false;
			if (num4 >= (float)lineWidth)
			{
				flag2 = true;
				flag = true;
			}
			else if (i >= length || text[i] == '\n')
			{
				flag2 = true;
				num2 = i;
			}
			if (flag2)
			{
				int num5 = num2 - num;
				while (num5 > 0 && text[num + num5 - 1] == ' ')
				{
					num5--;
				}
				WriteSubstring(spriteBatch, text, x, y, scale, color2, ref currentColor, TextAlign.Left, num, num5, 0, alphaFactor, lineHeight);
				num3++;
				y += ((lineHeight >= 0) ? ((float)lineHeight) : ((float)LineHeight * scale.Y));
				num = num2;
				if (num < length && text[num] == '\n')
				{
					num2++;
					num++;
				}
			}
			else
			{
				num2 = i + 1;
			}
		}
		return num3;
	}

	public virtual void Load(Stream stream, string fileName)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanSeek)
		{
			throw new ArgumentException("Stream must be seekable in order to determine file format.", "stream");
		}
		LoadText(stream);
		BMFontLoader.QualifyResourcePaths(this, Path.GetDirectoryName(fileName));
		Init();
	}

	public void LoadFromFile(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentNullException("fileName");
		}
		if (!File.Exists(fileName))
		{
			throw new FileNotFoundException($"Cannot find file '{fileName}'.", fileName);
		}
		using Stream stream = File.OpenRead(fileName);
		Load(stream, fileName);
	}

	public void LoadText(string text)
	{
		using StringReader reader = new StringReader(text);
		LoadText(reader);
	}

	public void LoadText(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using TextReader reader = new StreamReader(stream);
		LoadText(reader);
	}

	public virtual void LoadText(TextReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		IDictionary<int, Page> dictionary = new SortedDictionary<int, Page>();
		IDictionary<Kerning, int> dictionary2 = new Dictionary<Kerning, int>();
		IDictionary<char, Character> dictionary3 = new Dictionary<char, Character>();
		string text;
		do
		{
			text = reader.ReadLine();
			if (text == null)
			{
				continue;
			}
			string[] array = BMFontLoader.Split(text, ' ');
			if (array.Length == 0)
			{
				continue;
			}
			switch (array[0])
			{
			case "info":
				FamilyName = BMFontLoader.GetNamedString(array, "face");
				FontSize = BMFontLoader.GetNamedInt(array, "size");
				Bold = BMFontLoader.GetNamedBool(array, "bold");
				Italic = BMFontLoader.GetNamedBool(array, "italic");
				Charset = BMFontLoader.GetNamedString(array, "charset");
				Unicode = BMFontLoader.GetNamedBool(array, "unicode");
				StretchedHeight = BMFontLoader.GetNamedInt(array, "stretchH");
				Smoothed = BMFontLoader.GetNamedBool(array, "smooth");
				SuperSampling = BMFontLoader.GetNamedInt(array, "aa");
				Padding = BMFontLoader.ParsePadding(BMFontLoader.GetNamedString(array, "padding"));
				Spacing = BMFontLoader.ParsePoint(BMFontLoader.GetNamedString(array, "spacing"));
				OutlineSize = BMFontLoader.GetNamedInt(array, "outline");
				break;
			case "common":
				LineHeight = BMFontLoader.GetNamedInt(array, "lineHeight");
				BaseHeight = BMFontLoader.GetNamedInt(array, "base");
				TextureSize = new Vector2(BMFontLoader.GetNamedInt(array, "scaleW"), BMFontLoader.GetNamedInt(array, "scaleH"));
				Packed = BMFontLoader.GetNamedBool(array, "packed");
				AlphaChannel = BMFontLoader.GetNamedInt(array, "alphaChnl");
				RedChannel = BMFontLoader.GetNamedInt(array, "redChnl");
				GreenChannel = BMFontLoader.GetNamedInt(array, "greenChnl");
				BlueChannel = BMFontLoader.GetNamedInt(array, "blueChnl");
				break;
			case "page":
			{
				int namedInt = BMFontLoader.GetNamedInt(array, "id");
				string namedString = BMFontLoader.GetNamedString(array, "file");
				dictionary.Add(namedInt, new Page(namedInt, namedString));
				break;
			}
			case "char":
			{
				Character value = new Character
				{
					Char = (char)BMFontLoader.GetNamedInt(array, "id"),
					Bounds = new Rectangle(BMFontLoader.GetNamedInt(array, "x"), BMFontLoader.GetNamedInt(array, "y"), BMFontLoader.GetNamedInt(array, "width"), BMFontLoader.GetNamedInt(array, "height")),
					Offset = new Point(BMFontLoader.GetNamedInt(array, "xoffset"), BMFontLoader.GetNamedInt(array, "yoffset")),
					XAdvance = BMFontLoader.GetNamedInt(array, "xadvance"),
					TexturePage = BMFontLoader.GetNamedInt(array, "page"),
					Channel = BMFontLoader.GetNamedInt(array, "chnl")
				};
				dictionary3.Add(value.Char, value);
				break;
			}
			case "kerning":
			{
				Kerning key = new Kerning((char)BMFontLoader.GetNamedInt(array, "first"), (char)BMFontLoader.GetNamedInt(array, "second"), BMFontLoader.GetNamedInt(array, "amount"));
				if (!dictionary2.ContainsKey(key))
				{
					dictionary2.Add(key, key.Amount);
				}
				break;
			}
			}
		}
		while (text != null);
		Pages = BMFontLoader.ToArray(dictionary.Values);
		Characters = dictionary3;
		Kernings = dictionary2;
	}

	public Vector2 MeasureFont(string text, int offset = 0, int length = -1)
	{
		return MeasureFontMaxWidth(text, -1.0, offset, length);
	}

	public Vector2 MeasureFontMaxWidth(string text, double maxWidth, int offset = 0, int length = -1)
	{
		if (string.IsNullOrEmpty(text))
		{
			return Vector2.Zero;
		}
		char previous = ' ';
		int num = 0;
		int num2 = LineHeight;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = text.Length;
		if (offset < 0)
		{
			offset = 0;
		}
		else if (offset > num6)
		{
			offset = num6;
		}
		if (length >= 0 && offset + length < num6)
		{
			num6 = offset + length;
		}
		for (int i = offset; i < num6; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '\\':
				i++;
				continue;
			case '\n':
				num4 = Math.Max(num4, num);
				num = 0;
				num2 = LineHeight;
				num3++;
				previous = ' ';
				continue;
			case '\r':
				continue;
			}
			Character character = this[c];
			int num7 = character.XAdvance + GetKerning(previous, c);
			if (maxWidth != -1.0 && (double)(num + num7) >= maxWidth)
			{
				num4 = Math.Max(num4, num);
				num = 0;
				num2 = LineHeight;
				num3++;
				previous = ' ';
				num7 = character.XAdvance + GetKerning(previous, c);
			}
			num += num7;
			num2 = Math.Max(num2, character.Bounds.Height + character.Offset.Y);
			previous = c;
		}
		num5 = num3 * LineHeight;
		if (num > 0)
		{
			num5 += num2;
		}
		return new Vector2(Math.Max(num, num4), num5);
	}

	internal void LoadFromIniFile(IniFile iniFile, GameContent content)
	{
		while (iniFile.NextLine())
		{
			if (DataLoader.CheckParams(iniFile, content))
			{
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection)
			{
				break;
			}
			int[] resultArray;
			if (iniFile.KeyEquals("file"))
			{
				try
				{
					ContentArchive ca;
					using Stream stream = GameContent.GetFileStream(iniFile.Value, out ca);
					if (ca.Type == ContentArchiveTypeEnum.ZipFile)
					{
						using Stream stream2 = new MemoryStream();
						stream.CopyTo(stream2);
						stream2.Position = 0L;
						Load(stream2, iniFile.Value);
					}
					else
					{
						Load(stream, iniFile.Value);
					}
				}
				catch (Exception ex)
				{
					GameCore.LogErr("Unable to load BM Font from file '{0}' - Ex: {1}", iniFile.Value, ex);
				}
			}
			else if (iniFile.KeyEquals("color") && iniFile.TryGetIntArray(3, 4, out resultArray))
			{
				int idIntValue = iniFile.GetIdIntValue();
				if (idIntValue < 0 || idIntValue >= _colorArray.Length)
				{
					iniFile.LogErr("Color index out of range [0 - {0}]", _colorArray.Length - 1);
				}
				else
				{
					_colorArray[idIntValue] = new Color(resultArray[0], resultArray[1], resultArray[2], (resultArray.Length > 3) ? resultArray[3] : 255);
				}
			}
			else
			{
				iniFile.LogErr("Unknown font command");
			}
		}
	}

	public IEnumerator<Character> GetEnumerator()
	{
		foreach (KeyValuePair<char, Character> character in Characters)
		{
			yield return character.Value;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
