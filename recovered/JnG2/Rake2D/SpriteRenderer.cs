using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class SpriteRenderer
{
	public SpriteAtlas Atlas;

	public Color Color;

	public Texture2D Texture;

	private int _frame;

	public SpriteAnimator Animator { get; private set; }

	public string CachedTextureFileName { get; internal set; }

	public int Frame
	{
		get
		{
			if (Animator != null)
			{
				return Animator.AtlasFrame;
			}
			return _frame;
		}
		set
		{
			_frame = value;
		}
	}

	public string FrameId
	{
		set
		{
			_frame = Atlas.GetFrameNumber(value);
		}
	}

	public Rectangle FrameRectangle
	{
		get
		{
			if (Atlas == null)
			{
				return Rectangle.Empty;
			}
			return Atlas.GetFrameRectangle(Frame);
		}
	}

	public Vector2 FramePivot
	{
		get
		{
			if (Atlas == null)
			{
				return Vector2.Zero;
			}
			return Atlas.GetFramePivot(Frame);
		}
	}

	public SpriteRenderer(SpriteAtlas atlas, string skinSuffix = null)
	{
		SetAtlas(atlas, skinSuffix);
		Color = Color.White;
	}

	public void ClearSpriteRenderer()
	{
		Atlas = null;
		if (Animator != null)
		{
			GameCore.Allocator.DeleteSpriteAnimator(Animator);
			Animator = null;
		}
		Color = Color.White;
		CachedTextureFileName = null;
		Texture = null;
		_frame = 0;
	}

	public void SetAtlas(SpriteAtlas atlas, string skinSuffix = null)
	{
		Atlas = atlas;
		CachedTextureFileName = null;
		if (Atlas != null)
		{
			if (Atlas.CachedTexture)
			{
				string text = ((skinSuffix == null) ? atlas.TextureFileName : Path.Combine(Path.GetDirectoryName(atlas.TextureFileName), $"{skinSuffix}", Path.GetFileName(atlas.TextureFileName)));
				try
				{
					Texture = GameCore.TextureCache.Load(text);
					CachedTextureFileName = text;
					if (atlas.Texture == null && Texture != null && atlas.FrameCount == 0)
					{
						atlas.AddFrame(0, 0, Texture.Width, Texture.Height);
					}
					return;
				}
				catch
				{
					Texture = GameCore.DefaultTexture;
					GameCore.LogErr("Unable to load texture instance from '{0}'", text);
					return;
				}
			}
			Texture = Atlas.Texture;
		}
		else
		{
			Texture = GameCore.DefaultTexture;
		}
	}

	public void CreateAnimator(int[] animFrames, float animDuration, bool isLooped)
	{
		if (Animator != null)
		{
			GameCore.Allocator.DeleteSpriteAnimator(Animator);
		}
		Animator = GameCore.Allocator.GetSpriteAnimator();
		Animator.SetFrames(animFrames, animDuration);
		Animator.IsLooped = isLooped;
	}

	public bool Update(float elapsedTime)
	{
		if (Animator != null)
		{
			return Animator.Update(elapsedTime);
		}
		if (CachedTextureFileName != null && (Texture == null || Texture.IsDisposed))
		{
			Texture = GameCore.TextureCache.Load(CachedTextureFileName);
			GameCore.Log("Recached texture '{0}'", CachedTextureFileName);
			if (Texture == null)
			{
				GameCore.LogErr("Unable to recache texture '{0}'", CachedTextureFileName);
				return false;
			}
		}
		return false;
	}

	public bool ChangeAnim(string id, float duration)
	{
		if (Atlas.AnimFrames == null || Atlas.AnimFrames.ContainsKey(id))
		{
			if (Animator == null)
			{
				if (Atlas.AnimFrames != null)
				{
					CreateAnimator(Atlas.AnimFrames[id], duration, isLooped: true);
				}
			}
			else
			{
				Animator.SetFrames(Atlas.AnimFrames[id], duration);
			}
			return true;
		}
		GameCore.Allocator.DeleteSpriteAnimator(Animator);
		Animator = null;
		return false;
	}

	public void Draw(Matrix matrix, SpriteBatch spriteBatch, float lightFactor = 1f, BlendModeEnum blendMode = BlendModeEnum.Default)
	{
		if (Texture == null)
		{
			return;
		}
		if (CachedTextureFileName != null && Texture.IsDisposed)
		{
			Texture = GameCore.TextureCache.Load(CachedTextureFileName);
			GameCore.Log("Recached texture '{0}'", CachedTextureFileName);
			if (Texture == null)
			{
				return;
			}
		}
		Atlas.DrawFrame(spriteBatch, Frame, Color, matrix, Texture, lightFactor, blendMode);
	}

	public void Draw(Transformation transform, SpriteBatch spriteBatch, BlendModeEnum blendMode = BlendModeEnum.Default)
	{
		if (Texture == null)
		{
			return;
		}
		if (CachedTextureFileName != null && Texture.IsDisposed)
		{
			Texture = GameCore.TextureCache.Load(CachedTextureFileName);
			GameCore.Log("Recached texture '{0}'", CachedTextureFileName);
			if (Texture == null)
			{
				return;
			}
		}
		Atlas?.DrawFrame(spriteBatch ?? GameCore.SpriteBatch, Frame, Color, transform, Texture, blendMode);
	}
}
