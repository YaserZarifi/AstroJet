using System.Collections.Specialized;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class TextureCache
{
	public int Capacity;

	private OrderedDictionary _textureDictionary;

	public TextureCache(int capacity)
	{
		Capacity = capacity;
		_textureDictionary = new OrderedDictionary();
	}

	public void Clear()
	{
		while (_textureDictionary.Count > 0)
		{
			RemoveLast();
		}
	}

	private void RemoveLast()
	{
		((Texture2D)_textureDictionary[0])?.Dispose();
		_textureDictionary.RemoveAt(0);
	}

	public Texture2D Load(string fileName)
	{
		string key = fileName.ToLowerLazy();
		if (_textureDictionary.Contains(key))
		{
			Texture2D texture2D = (Texture2D)_textureDictionary[key];
			if (texture2D != null && !texture2D.IsDisposed)
			{
				return (Texture2D)_textureDictionary[key];
			}
			_textureDictionary.Remove(key);
		}
		while (_textureDictionary.Count >= Capacity)
		{
			RemoveLast();
		}
		Texture2D texture2D2 = null;
		try
		{
			texture2D2 = DataLoader.LoadTexture(fileName);
		}
		catch
		{
			GameCore.LogErr("Unable to load texture to cache from file '{0}'", fileName);
			return null;
		}
		_textureDictionary.Add(key, texture2D2);
		return texture2D2;
	}
}
