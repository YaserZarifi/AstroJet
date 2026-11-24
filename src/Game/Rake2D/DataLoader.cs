using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;
using Rake2D.JnG;

namespace Rake2D;

public static class DataLoader
{
	public static Color EditorColor;

	public static int EditorSet;

	public static int EditorSymbol;

	public static Texture2D Texture;

	public static string TextureFileName;

	public static bool CachedTexture;

	public static SpriteBatch _spriteBatch;

	public static Texture2D LoadTexture(string fileName)
	{
		Texture2D texture2D = null;
		ContentArchive ca;
		using (Stream stream = GameContent.GetFileStream(fileName, out ca))
		{
			if (stream == null)
			{
				return null;
			}
			try
			{
				if (ca.Type == ContentArchiveTypeEnum.ZipFile)
				{
					using Stream stream2 = new MemoryStream();
					stream.CopyTo(stream2);
					stream2.Position = 0L;
					texture2D = Texture2D.FromStream(GameCore.GraphicsDeviceStatic, stream2);
				}
				else
				{
					texture2D = Texture2D.FromStream(GameCore.GraphicsDeviceStatic, stream);
				}
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Unable to load texture from file '{0}'. Ex: {1}", fileName, ex.ToString());
				return null;
			}
		}
		GraphicsDevice graphicsDevice = GameCore.GraphicsDeviceManager.GraphicsDevice;
		RenderTarget2D renderTarget2D = new RenderTarget2D(graphicsDevice, texture2D.Width, texture2D.Height);
		graphicsDevice.SetRenderTarget(renderTarget2D);
		graphicsDevice.Clear(Color.Black);
		BlendState blendState = new BlendState
		{
			ColorWriteChannels = (ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue),
			AlphaDestinationBlend = Blend.Zero,
			ColorDestinationBlend = Blend.Zero,
			AlphaSourceBlend = Blend.SourceAlpha,
			ColorSourceBlend = Blend.SourceAlpha
		};
		if (_spriteBatch == null)
		{
			_spriteBatch = new SpriteBatch(graphicsDevice);
		}
		_spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
		_spriteBatch.Draw(texture2D, texture2D.Bounds, Color.White);
		_spriteBatch.End();
		BlendState blendState2 = new BlendState
		{
			ColorWriteChannels = ColorWriteChannels.Alpha,
			AlphaDestinationBlend = Blend.Zero,
			ColorDestinationBlend = Blend.Zero,
			AlphaSourceBlend = Blend.One,
			ColorSourceBlend = Blend.One
		};
		_spriteBatch.Begin(SpriteSortMode.Immediate, blendState2);
		_spriteBatch.Draw(texture2D, texture2D.Bounds, Color.White);
		_spriteBatch.End();
		graphicsDevice.SetRenderTarget(GameCore.ScreenRenderTarget);
		return renderTarget2D;
	}

	private static byte ApplyAlpha(byte color, byte alpha)
	{
		float num = (float)(int)color / 255f;
		float num2 = (float)(int)alpha / 255f;
		int num3 = (int)(255f * num * num2);
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num3 > 255)
		{
			num3 = 255;
		}
		return (byte)num3;
	}

	public static void PremultiplyAlpha(Texture2D texture)
	{
		Color[] array = new Color[texture.Width * texture.Height];
		texture.GetData(array);
		for (int i = 0; i < array.Length; i++)
		{
			byte a = array[i].A;
			array[i].R = ApplyAlpha(array[i].R, a);
			array[i].G = ApplyAlpha(array[i].G, a);
			array[i].B = ApplyAlpha(array[i].B, a);
		}
		texture.SetData(array);
	}

	public static bool CheckParams(IniFile iniFile, GameContent content)
	{
		if (CheckEditorParams(iniFile, content) || CheckOtherParams(iniFile, content))
		{
			return true;
		}
		return false;
	}

	public static bool CheckEditorParams(IniFile iniFile, GameContent content)
	{
		if (iniFile.KeyEquals("editor_color") && iniFile.TryGetIntArray(3, out var resultArray))
		{
			EditorColor = new Color(resultArray[0], resultArray[1], resultArray[2]);
			return true;
		}
		if (iniFile.KeyEquals("editor_set"))
		{
			EditorSet = iniFile.GetIntValue();
			return true;
		}
		if (iniFile.KeyEquals("editor_symbol"))
		{
			EditorSymbol = iniFile.GetIntValue();
			return true;
		}
		return false;
	}

	private static bool CheckOtherParams(IniFile iniFile, GameContent content)
	{
		if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("texture"))
		{
			try
			{
				TextureFileName = iniFile.GetPathFromValue();
				Texture = content.LoadTexture(iniFile.Id ?? TextureFileName, TextureFileName);
			}
			catch (IOException ex)
			{
				Texture = null;
				TextureFileName = null;
				iniFile.LogErr("Unable to load texture '{0}'", TextureFileName);
				GameCore.LogErr("IO Exception: {0}", ex);
			}
			CachedTexture = false;
			return true;
		}
		if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("cached_texture"))
		{
			TextureFileName = iniFile.GetPathFromValue();
			Texture = null;
			CachedTexture = true;
			return true;
		}
		return false;
	}

	public static bool LoadHeroPartChoiceFile(string fileName)
	{
		using (IniFile iniFile = IniFile.OpenFromContent(fileName))
		{
			if (iniFile == null)
			{
				return false;
			}
			HeroPartChoice heroPartChoice = null;
			HeroPartChoiceItem heroPartChoiceItem = null;
			while (iniFile.NextLine())
			{
				if (iniFile.KeyEquals("skin_id"))
				{
					string[] stringArray = iniFile.GetStringArray();
					foreach (string item in stringArray)
					{
						GameJnG.HeroCamouflageIdList.Add(item);
					}
				}
				else if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("part_choice"))
				{
					heroPartChoice = new HeroPartChoice(iniFile.Value);
					GameJnG.HeroPartChoices.Add(heroPartChoice.Id, heroPartChoice);
				}
				else if (heroPartChoice != null && iniFile.ValueType == IniFileValueType.SubSection && iniFile.KeyEquals("choice_item"))
				{
					heroPartChoiceItem = new HeroPartChoiceItem(iniFile.GetStringArray(), iniFile.GetIdIntValue());
					heroPartChoice.AddItem(heroPartChoiceItem);
				}
				else if (heroPartChoiceItem != null && iniFile.KeyEquals("id_index"))
				{
					heroPartChoiceItem.AddChoiceIdIndex(iniFile.Id, iniFile.GetIntValue());
				}
				else
				{
					iniFile.LogErr("Unknown command");
				}
			}
			GameCore.Log($"Loaded {GameJnG.HeroPartChoices.Count} hero part choices.");
		}
		return true;
	}

	public static void ReloadAtlasList(List<SpriteAtlas> atlasList)
	{
		foreach (SpriteAtlas atlas in atlasList)
		{
			_ = atlas;
		}
	}

	public static void LoadDataFile(string fileName, GameContent content, DataType dataType = DataType.All, string idOnly = null)
	{
		using IniFile iniFile = IniFile.OpenFromContent(fileName);
		if (iniFile == null || (dataType == DataType.All && !content.AddDataFile(fileName)))
		{
			return;
		}
		EditorColor = Color.White;
		EditorSet = 0;
		EditorSymbol = 0;
		CachedTexture = false;
		Texture = null;
		TextureFileName = null;
		bool flag = true;
		while (!flag || iniFile.NextLine())
		{
			flag = true;
			if (CheckParams(iniFile, content))
			{
				continue;
			}
			bool flag2 = iniFile.ValueType == IniFileValueType.MainSection && (idOnly == null || Utils.Equals(idOnly, iniFile.Value));
			if (idOnly == null && iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("data"))
			{
				LoadDataFile(iniFile.GetPathFromValue(), content);
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Part) && iniFile.KeyEquals("main"))
			{
				PartData partData = content.CreatePartData(iniFile.Value);
				partData.EditorColor = EditorColor;
				partData.EditorSet = EditorSet;
				partData.EditorGizmo = EditorSymbol;
				partData.LoadFromIniFile(iniFile, content);
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Emitter) && iniFile.KeyEquals("emitter"))
			{
				content.CreateParticleEmitterData(iniFile.Value).LoadFromIniFile(iniFile, content);
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Emitter) && iniFile.KeyEquals("emt"))
			{
				content.CreateParticleEmitterData(iniFile.Value).LoadFromIniFileEmt(iniFile, content);
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Item) && iniFile.KeyEquals("item"))
			{
				content.CreateHeroItemData(iniFile.Value).LoadFromIniFile(iniFile, content);
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Spine) && iniFile.KeyEquals("spine"))
			{
				content.CreateSpineData(iniFile.Value).LoadFromIniFile(iniFile, content);
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Sound) && iniFile.KeyEquals("sound"))
			{
				content.CreateSoundData(iniFile.Id).LoadFromIniFile(iniFile, content);
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Spine) && iniFile.KeyEquals("font"))
			{
				BMFont bMFont = content.CreateBMFont(iniFile.Value);
				if (bMFont != null)
				{
					bMFont.LoadFromIniFile(iniFile, content);
				}
				else
				{
					iniFile.LogErr("Unable to load 'font.{0}'", iniFile.Id);
				}
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Atlas) && iniFile.KeyEquals("atlas"))
			{
				if (TextureFileName == null)
				{
					iniFile.LogErr("Undefined texture for atlas '{0}'", iniFile.Value);
					continue;
				}
				SpriteAtlas spriteAtlas = content.CreateSpriteAtlas(iniFile.Value, Texture, TextureFileName);
				spriteAtlas.CachedTexture = CachedTexture;
				SpriteAtlas.LoadFromIniFile(spriteAtlas, iniFile, content);
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if (flag2 && (dataType == DataType.All || dataType == DataType.Path) && iniFile.KeyEquals("path"))
			{
				content.CreateBezierPath(iniFile.Value).LoadFromIniFile(iniFile, content);
				if (iniFile.EOF)
				{
					break;
				}
				flag = false;
			}
			else if ((dataType == DataType.All) & flag2)
			{
				if (!iniFile.KeyEquals("end"))
				{
					iniFile.LogErr("Wrong [section]");
				}
				continue;
			}
			if (!string.IsNullOrEmpty(idOnly) && !flag)
			{
				break;
			}
		}
	}
}
