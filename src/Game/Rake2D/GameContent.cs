using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;
using Rake2D.JnG;
using Rake2D.Sound;

namespace Rake2D;

public class GameContent
{
	public SpriteAtlas DefaultItemIconAtlas;

	public static List<ContentArchive> ContentArchiveList;

	private Dictionary<string, BitmapFont> _fonts;

	private Dictionary<string, SpriteAtlas> _atlases;

	private Dictionary<string, Texture2D> _textures;

	private Dictionary<string, SpineData> _spineDatas;

	private Dictionary<string, BezierPath> _bezierPaths;

	private Dictionary<string, SoundData> _sounds;

	private Dictionary<string, BMFont> _bmFonts;

	private HashSet<string> _dataFiles;

	internal Dictionary<string, PartData> PartDatas { get; private set; }

	internal Dictionary<string, ParticleEmitterData> ParticleEmitterDatas { get; private set; }

	private Dictionary<string, HeroItemData> _itemDatas { get; set; }

	public GameContent()
	{
		_dataFiles = new HashSet<string>();
		PartDatas = new Dictionary<string, PartData>();
		_itemDatas = new Dictionary<string, HeroItemData>();
		_fonts = new Dictionary<string, BitmapFont>();
		_atlases = new Dictionary<string, SpriteAtlas>();
		_textures = new Dictionary<string, Texture2D>();
		_spineDatas = new Dictionary<string, SpineData>();
		ParticleEmitterDatas = new Dictionary<string, ParticleEmitterData>();
		_bezierPaths = new Dictionary<string, BezierPath>();
		_sounds = new Dictionary<string, SoundData>();
		_bmFonts = new Dictionary<string, BMFont>();
	}

	internal void Clear()
	{
		PartDatas.Clear();
		_itemDatas.Clear();
		_fonts.Clear();
		_atlases.Clear();
		foreach (Texture2D value in _textures.Values)
		{
			value.Dispose();
		}
		_textures.Clear();
		_spineDatas.Clear();
		ParticleEmitterDatas.Clear();
		foreach (SoundData value2 in _sounds.Values)
		{
			value2.FreeSample();
		}
		_sounds.Clear();
		_bmFonts.Clear();
		_dataFiles.Clear();
	}

	public static void LoadContentArchiveList()
	{
		GameCore.Log("Loading content archive list");
		ContentArchiveList = new List<ContentArchive>();
		IniFile iniFile = new IniFile();
		if (iniFile.OpenFromFile("content.txt"))
		{
			while (iniFile.NextLine())
			{
				if (iniFile.KeyEquals("dir"))
				{
					ContentArchiveList.Add(new ContentArchive(iniFile.Value, ContentArchiveTypeEnum.Directory));
				}
				else if (iniFile.KeyEquals("zip"))
				{
					GameCore.Log("Trying to open zip archive '{0}'", iniFile.Value);
					ContentArchive contentArchive = new ContentArchive(iniFile.Value, ContentArchiveTypeEnum.ZipFile);
					try
					{
						contentArchive.ZipArchive = new ZipArchive(File.OpenRead(contentArchive.Name), ZipArchiveMode.Read);
						ContentArchiveList.Add(contentArchive);
					}
					catch (Exception ex)
					{
						GameCore.LogErr("Zip error {0}", ex);
					}
				}
			}
			GameCore.Log("Loaded {0} content archive(s).", ContentArchiveList.Count);
		}
		else
		{
			GameCore.LogErr("Unable to open content.txt");
		}
		iniFile.Close();
	}

	public static ContentArchive GetFirstArchive(ContentArchiveTypeEnum type)
	{
		return ContentArchiveList?.FirstOrDefault((ContentArchive ca) => ca.Type == ContentArchiveTypeEnum.Directory);
	}

	public static bool FileExits(string fileName)
	{
		string fullPath = Path.GetFullPath(fileName);
		for (int i = 0; i < ContentArchiveList.Count; i++)
		{
			ContentArchive contentArchive = ContentArchiveList[i];
			switch (contentArchive.Type)
			{
			case ContentArchiveTypeEnum.Directory:
			{
				string path = Path.Combine(contentArchive.Name, fileName);
				try
				{
					if (File.Exists(path))
					{
						return true;
					}
				}
				catch
				{
				}
				break;
			}
			case ContentArchiveTypeEnum.ZipFile:
				try
				{
					if (contentArchive.ZipArchive != null && contentArchive.ZipArchive.Entries.FirstOrDefault((ZipArchiveEntry e) => Path.GetFullPath(e.FullName).Equals(fullPath)) != null)
					{
						return true;
					}
				}
				catch
				{
				}
				break;
			}
		}
		return false;
	}

	public static Stream GetFileStream(string fileName)
	{
		ContentArchive ca;
		return GetFileStream(fileName, out ca);
	}

	public static Stream GetFileStream(string fileName, out ContentArchive ca)
	{
		string fullPath = Path.GetFullPath(fileName);
		for (int i = 0; i < ContentArchiveList.Count; i++)
		{
			ca = ContentArchiveList[i];
			switch (ca.Type)
			{
			case ContentArchiveTypeEnum.Directory:
			{
				string path = Path.Combine(ca.Name, fileName);
				try
				{
					if (File.Exists(path))
					{
						return File.OpenRead(path);
					}
				}
				catch
				{
				}
				break;
			}
			case ContentArchiveTypeEnum.ZipFile:
				try
				{
					if (ca.ZipArchive != null)
					{
						ZipArchiveEntry zipArchiveEntry = ca.ZipArchive.Entries.FirstOrDefault((ZipArchiveEntry e) => Path.GetFullPath(e.FullName).Equals(fullPath));
						if (zipArchiveEntry != null)
						{
							return zipArchiveEntry.Open();
						}
					}
				}
				catch
				{
				}
				break;
			}
		}
		GameCore.LogErr("Unable to open file '{0}'. Not found in any content archive.", fileName);
		ca = null;
		return null;
	}

	public void ReloadAtlas(SpriteAtlas atlas)
	{
		string textureId = atlas.MyContent.GetTextureId(atlas.Texture);
		GameContent gameContent = atlas.MyContent;
		if (textureId == null && atlas.MyContent != GameCore.GlobalContent)
		{
			textureId = GameCore.GlobalContent.GetTextureId(atlas.Texture);
			gameContent = GameCore.GlobalContent;
		}
		if (textureId != null)
		{
			atlas.Texture = gameContent.LoadTexture(textureId, atlas.TextureFileName, bForceLoad: true);
		}
		DataLoader.Texture = atlas.Texture;
		DataLoader.TextureFileName = atlas.TextureFileName;
		foreach (KeyValuePair<string, SpriteAtlas> item in _atlases.Where((KeyValuePair<string, SpriteAtlas> a) => a.Value == atlas).ToList())
		{
			_atlases.Remove(item.Key);
		}
		DataLoader.LoadDataFile(atlas.FileName, this, DataType.Atlas, atlas.Id);
	}

	public void ReloadSounds()
	{
		foreach (SoundData value in _sounds.Values)
		{
			value.FreeSample();
			value.LoadSample();
		}
	}

	public void ReloadDataFiles(DataType dataType)
	{
		switch (dataType)
		{
		case DataType.Item:
			_itemDatas.Clear();
			GameCore.TextureCache.Clear();
			break;
		case DataType.Part:
			PartDatas.Clear();
			break;
		case DataType.Atlas:
			_atlases.Clear();
			break;
		case DataType.Emitter:
			ParticleEmitterDatas.Clear();
			break;
		}
		foreach (string dataFile in _dataFiles)
		{
			if (dataType != DataType.Item || dataFile.ToLower().Contains("item"))
			{
				GameCore.Log("Reloading {0} from file '{1}'", dataType.ToString(), dataFile);
				DataLoader.LoadDataFile(dataFile, this, dataType);
			}
		}
		if (dataType != DataType.Item)
		{
			return;
		}
		foreach (SpriteAtlas item in _atlases.Values.Where((SpriteAtlas a) => a.Id.StartsWith("item_icon_", StringComparison.OrdinalIgnoreCase)).ToList())
		{
			ReloadAtlas(item);
		}
		InitItemData();
		foreach (HeroItem allItem in GameJnG.GetAllItemList())
		{
			if (_itemDatas.TryGetValue(allItem.Data.Id, out var value))
			{
				allItem.Data = value;
			}
			allItem.InitData();
		}
	}

	public List<SpriteAtlas> GetAtlasList()
	{
		return _atlases.Values.ToList();
	}

	public bool AddDataFile(string filename)
	{
		if (_dataFiles.Contains(filename.ToLowerLazy()))
		{
			GameCore.LogErr($"Data file '{filename}' already exist in game content.");
			return false;
		}
		return _dataFiles.Add(filename.ToLowerLazy());
	}

	public BitmapFont AddBitmapFont(string id, string textureFileName)
	{
		BitmapFont bitmapFont = new BitmapFont(id);
		bitmapFont.LoadTexture(textureFileName);
		_fonts.Add(id, bitmapFont);
		return bitmapFont;
	}

	public SpriteAtlas CreateSpriteAtlas(string id, Texture2D texture, string textureFileName, BlendModeEnum blendType = BlendModeEnum.Normal)
	{
		SpriteAtlas spriteAtlas = GetSpriteAtlas(id);
		if (spriteAtlas == null)
		{
			spriteAtlas = new SpriteAtlas(id, texture);
			spriteAtlas.TextureFileName = textureFileName;
			spriteAtlas.BlendState = Utils.GetBlendState(blendType);
			_atlases.Add(id.ToLowerLazy(), spriteAtlas);
		}
		else
		{
			GameCore.LogErr("SpriteAtlas ID = {0} is already created!", id);
		}
		spriteAtlas.MyContent = this;
		return spriteAtlas;
	}

	public Texture2D LoadTexture(string id, string fileName, bool bForceLoad = false)
	{
		if (bForceLoad || !_textures.TryGetValue(id.ToLowerLazy(), out var value))
		{
			value = DataLoader.LoadTexture(fileName);
			if (value == null)
			{
				GameCore.LogErr("Unable to load texture from file '{0}'", fileName);
				return null;
			}
			_textures[id.ToLowerLazy()] = value;
		}
		return value;
	}

	public Texture2D GetTexture(string id)
	{
		_textures.TryGetValue(id.ToLowerLazy(), out var value);
		return value;
	}

	public string GetTextureId(Texture2D texture)
	{
		foreach (KeyValuePair<string, Texture2D> texture2 in _textures)
		{
			if (texture2.Value == texture)
			{
				return texture2.Key;
			}
		}
		return null;
	}

	public BMFont CreateBMFont(string id)
	{
		BMFont bMFont = GetBMFont(id);
		if (bMFont == null)
		{
			bMFont = new BMFont();
			_bmFonts[id.ToLowerLazy()] = bMFont;
		}
		return bMFont;
	}

	public BMFont GetBMFont(string id)
	{
		_bmFonts.TryGetValue(id.ToLowerLazy(), out var value);
		return value;
	}

	public PartData CreatePartData(string id)
	{
		PartData partData = GetPartData(id);
		if (partData != null)
		{
			GameCore.LogErr("Part data {0} was already created in file '{1}'", id, partData.FileName);
			PartDatas.Remove(id.ToLowerLazy());
		}
		partData = new PartData(id);
		partData.MyContent = this;
		PartDatas.Add(id.ToLowerLazy(), partData);
		return partData;
	}

	public PartData GetPartData(string id)
	{
		PartDatas.TryGetValue(id.ToLowerLazy(), out var value);
		return value;
	}

	public HeroItemData CreateHeroItemData(string id)
	{
		HeroItemData heroItemData = GetHeroItemData(id);
		if (heroItemData != null)
		{
			GameCore.LogErr("Hero device data {0} was already created in file '{1}'", id, heroItemData.FileName);
			_itemDatas.Remove(id.ToLowerLazy());
		}
		heroItemData = new HeroItemData(id);
		_itemDatas.Add(id.ToLowerLazy(), heroItemData);
		return heroItemData;
	}

	public HeroItemData GetHeroItemData(string id)
	{
		HeroItemData value = null;
		_itemDatas.TryGetValue(id.ToLowerLazy(), out value);
		return value;
	}

	public List<HeroItemData> GetHeroItemDataList()
	{
		return new List<HeroItemData>(_itemDatas.Values);
	}

	public void Init()
	{
		foreach (PartData value in PartDatas.Values)
		{
			value.Init();
		}
		DefaultItemIconAtlas = GameCore.GetSpriteAtlas("item_icon");
		if (DefaultItemIconAtlas == null)
		{
			GameCore.LogErr("Undfined [atlas = item_icon] - unable to setup items");
		}
		InitItemData();
		foreach (SpineData value2 in _spineDatas.Values)
		{
			if (!value2.IsInitialized)
			{
				value2.Init();
			}
		}
		ReloadSounds();
	}

	private void InitItemData()
	{
		foreach (HeroItemData value in _itemDatas.Values)
		{
			value.Init(this);
		}
	}

	public void LogInfo(string captionText)
	{
		GameCore.Log(captionText);
		GameCore.Log($"  Textures: {_textures.Count}");
		GameCore.Log($"  Atlases: {_atlases.Count}");
		GameCore.Log($"  Fonts: {_bmFonts.Count}");
		GameCore.Log($"  Parts: {PartDatas.Count}");
		GameCore.Log($"  Emitters: {ParticleEmitterDatas.Count}");
		GameCore.Log($"  Spines: {_spineDatas.Count}");
		GameCore.Log($"  Sounds: {_sounds.Count}");
		GameCore.Log($"  Items: {_itemDatas.Count}");
		GameCore.Log($"  Paths: {_bezierPaths.Count}");
	}

	public ParticleEmitterData CreateParticleEmitterData(string id)
	{
		ParticleEmitterData particleEmitterData = GetParticleEmitterData(id);
		if (particleEmitterData != null)
		{
			GameCore.LogErr("Particle emitter data '{0}' was already created in file '{1}'", id, particleEmitterData.FileName);
			ParticleEmitterDatas.Remove(id.ToLowerLazy());
		}
		particleEmitterData = new ParticleEmitterData(id);
		ParticleEmitterDatas.Add(id.ToLowerLazy(), particleEmitterData);
		return particleEmitterData;
	}

	public ParticleEmitterData GetParticleEmitterData(string id)
	{
		ParticleEmitterDatas.TryGetValue(id.ToLowerLazy(), out var value);
		return value;
	}

	public void RemoveParticleEmitterData(string id)
	{
		ParticleEmitterDatas.Remove(id.ToLowerLazy());
	}

	public SpineData CreateSpineData(string id)
	{
		SpineData spineData = GetSpineData(id);
		if (spineData != null)
		{
			GameCore.LogErr("Spine data '{0}' was already created in file '{1}'", id, spineData.FileName);
			_spineDatas.Remove(id.ToLowerLazy());
		}
		spineData = new SpineData(id);
		_spineDatas.Add(id.ToLowerLazy(), spineData);
		return spineData;
	}

	public BezierPath CreateBezierPath(string id)
	{
		BezierPath bezierPath = GetBezierPath(id);
		if (bezierPath != null)
		{
			GameCore.LogErr("Path '{0}' was already created in file '{1}'", id, bezierPath.FileName);
			_bezierPaths.Remove(id.ToLowerLazy());
		}
		bezierPath = new BezierPath(id, null);
		_bezierPaths.Add(id.ToLowerLazy(), bezierPath);
		return bezierPath;
	}

	public SpineData GetSpineData(string id)
	{
		_spineDatas.TryGetValue(id.ToLowerLazy(), out var value);
		return value;
	}

	public BezierPath GetBezierPath(string id)
	{
		_bezierPaths.TryGetValue(id.ToLowerLazy(), out var value);
		return value;
	}

	public SoundData CreateSoundData(string id)
	{
		SoundData soundData = GetSoundData(id);
		if (soundData == null)
		{
			soundData = new JnG2.Mobile.SoundDataAndroid(id);
			_sounds.Add(id.ToLowerLazy(), soundData);
		}
		return soundData;
	}

	public SoundData GetSoundData(string id)
	{
		_sounds.TryGetValue(id.ToLowerLazy(), out var value);
		return value;
	}

	public List<SoundData> GetAllSoundData()
	{
		return _sounds.Values.ToList();
	}

	public SpriteAtlas GetSpriteAtlas(string id)
	{
		_atlases.TryGetValue(id.ToLowerLazy(), out var value);
		return value;
	}
}
