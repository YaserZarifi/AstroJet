using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class SpriteAtlas
{
	public GameContent MyContent;

	public Texture2D Texture;

	public string TextureFileName;

	public string FileName;

	public Dictionary<string, int[]> AnimFrames;

	public string Id;

	public BlendState BlendState;

	public bool CachedTexture;

	public bool IsForEmitter;

	private List<SpriteAtlasFrame> _frameList;

	public int FrameCount
	{
		get
		{
			if (_frameList != null)
			{
				return _frameList.Count;
			}
			return 0;
		}
	}

	public SpriteAtlas(string id, Texture2D texture)
	{
		Id = id;
		Texture = texture;
		_frameList = new List<SpriteAtlasFrame>();
		BlendState = BlendState.AlphaBlend;
	}

	public SpriteAtlasFrame GetFrame(string id)
	{
		foreach (SpriteAtlasFrame frame in _frameList)
		{
			if (frame.Id != null && frame.Id.Equals(id))
			{
				return frame;
			}
		}
		return null;
	}

	public void AddAnim(string animId, int[] frameArray)
	{
		if (AnimFrames == null)
		{
			AnimFrames = new Dictionary<string, int[]>();
		}
		if (AnimFrames.ContainsKey(animId))
		{
			AnimFrames.Remove(animId);
			GameCore.LogErr("Anim '{0}' already added in Atlas '{1}'", animId, Id);
		}
		AnimFrames.Add(animId, frameArray);
	}

	public int GetFrameNumber(string id)
	{
		for (int i = 0; i < _frameList.Count; i++)
		{
			if (_frameList[i].Id != null && _frameList[i].Id.Equals(id))
			{
				return i;
			}
		}
		return 0;
	}

	public Rectangle GetFrameRectangle(int frameNumber)
	{
		if (frameNumber >= _frameList.Count || frameNumber < 0)
		{
			return Rectangle.Empty;
		}
		return _frameList[frameNumber].Rectangle;
	}

	public void GetFrameUV(int frameNumber, out Vector2 topLeft, out Vector2 bottomRight)
	{
		if (Texture != null)
		{
			Rectangle frameRectangle = GetFrameRectangle(frameNumber);
			topLeft = new Vector2((float)frameRectangle.Left / (float)Texture.Width, (float)frameRectangle.Top / (float)Texture.Height);
			bottomRight = new Vector2((float)frameRectangle.Right / (float)Texture.Width, (float)frameRectangle.Bottom / (float)Texture.Height);
		}
		else
		{
			topLeft = Vector2.Zero;
			bottomRight = Vector2.Zero;
		}
	}

	public Vector2 GetFramePivot(int frameNumber)
	{
		if (frameNumber >= _frameList.Count || frameNumber < 0)
		{
			return Vector2.Zero;
		}
		return _frameList[frameNumber].Pivot;
	}

	public SpriteAtlasFrame AddFrame(int x, int y, int width, int height, Align pivotAlign = Align.Center)
	{
		SpriteAtlasFrame spriteAtlasFrame = new SpriteAtlasFrame(x, y, width, height);
		spriteAtlasFrame.PivotAlign(pivotAlign);
		_frameList.Add(spriteAtlasFrame);
		return spriteAtlasFrame;
	}

	public SpriteAtlasFrame AddFrame(string id, int x, int y, int width, int height, int pivotX, int pivotY)
	{
		SpriteAtlasFrame spriteAtlasFrame = AddFrame(x, y, width, height, pivotX, pivotY);
		spriteAtlasFrame.Id = id;
		return spriteAtlasFrame;
	}

	public SpriteAtlasFrame AddFrame(int x, int y, int width, int height, int pivotX, int pivotY)
	{
		SpriteAtlasFrame spriteAtlasFrame = new SpriteAtlasFrame(x, y, width, height);
		spriteAtlasFrame.Pivot = new Vector2(pivotX, pivotY);
		_frameList.Add(spriteAtlasFrame);
		return spriteAtlasFrame;
	}

	public void GridFrames(int countX, int countY, int startX, int startY, int frameW, int frameH, Align pivotAlign = Align.Center)
	{
		for (int i = 0; i < countY; i++)
		{
			for (int j = 0; j < countX; j++)
			{
				AddFrame(startX + j * frameW, startY + i * frameH, frameW, frameH, pivotAlign);
			}
		}
	}

	public void GridFrames(int countX, int countY, int startX, int startY, int frameW, int frameH, int pivotX, int pivotY)
	{
		for (int i = 0; i < countY; i++)
		{
			for (int j = 0; j < countX; j++)
			{
				AddFrame(startX + j * frameW, startY + i * frameH, frameW, frameH, pivotX, pivotY);
			}
		}
	}

	public void GridFrames(int countX, int countY, Align pivotAlign = Align.Center)
	{
		if (Texture == null)
		{
			return;
		}
		int num = Texture.Width / countX;
		int num2 = Texture.Height / countY;
		for (int i = 0; i < countY; i++)
		{
			for (int j = 0; j < countX; j++)
			{
				AddFrame(j * num, i * num2, num, num2, pivotAlign);
			}
		}
	}

	public SpriteAtlasFrame OneFrame(Align pivotAlign = Align.Center)
	{
		if (Texture != null)
		{
			return AddFrame(0, 0, Texture.Width, Texture.Height, pivotAlign);
		}
		return null;
	}

	public int[] GetAnimFrames(string animId)
	{
		int[] value = null;
		AnimFrames.TryGetValue(animId, out value);
		return value;
	}

	public BlendModeEnum GetCurentBlendMode(SpriteBatch spriteBatch, BlendModeEnum blendMode)
	{
		if (blendMode == BlendModeEnum.Default && BlendState == BlendState.Additive)
		{
			return BlendModeEnum.Additive;
		}
		return blendMode;
	}

	public void DrawFrame(SpriteBatch spriteBatch, int frameNumber, Color color, Matrix screenTransform, Texture2D texture, float lightFactor = 1f, BlendModeEnum blendMode = BlendModeEnum.Default)
	{
		if (frameNumber >= 0 && frameNumber < _frameList.Count)
		{
			if ((spriteBatch.GraphicsDevice.BlendState == BlendState.AlphaBlend && blendMode == BlendModeEnum.Additive) || (blendMode == BlendModeEnum.Default && BlendState == BlendState.Additive))
			{
				color *= (float)(int)color.A / 255f;
				color.A = 0;
			}
			else
			{
				color = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);
			}
			Utils.DecomposeMatrix(ref screenTransform, out var position, out var rotation, out var scale);
			spriteBatch.Draw(texture, position, _frameList[frameNumber].Rectangle, color, rotation, _frameList[frameNumber].Pivot, scale, SpriteEffects.None, 0f);
		}
	}

	public void DrawFrame(SpriteBatch spriteBatch, int frameNumber, Color color, Transformation transform, BlendModeEnum blendMode = BlendModeEnum.Default)
	{
		DrawFrame(spriteBatch, frameNumber, color, transform, Texture, blendMode);
	}

	public void DrawFrame(SpriteBatch spriteBatch, int frameNumber, Color color, Transformation transform, Texture2D texture, BlendModeEnum blendMode = BlendModeEnum.Default)
	{
		if (frameNumber >= 0 && frameNumber < _frameList.Count)
		{
			blendMode = GetCurentBlendMode(spriteBatch, blendMode);
			color = Utils.GetColorByBlendMode(color, blendMode);
			if (blendMode == BlendModeEnum.Additive || color.A > 0)
			{
				spriteBatch.Draw(texture ?? GameCore.DefaultTexture, transform.Position, _frameList[frameNumber].Rectangle, color, transform.Rotation, _frameList[frameNumber].Pivot, transform.Scale, SpriteEffects.None, 0f);
			}
		}
	}

	public void DrawFrameFlipable(SpriteBatch spriteBatch, int frameNumber, Color color, Transformation transform, Texture2D texture, BlendModeEnum blendMode = BlendModeEnum.Default, SpriteEffects spriteEffects = SpriteEffects.None)
	{
		if (frameNumber < 0 || frameNumber >= _frameList.Count || texture == null)
		{
			return;
		}
		color = Utils.GetColorByBlendMode(color, blendMode);
		if ((spriteEffects & SpriteEffects.FlipHorizontally) != 0)
		{
			if ((spriteEffects & SpriteEffects.FlipVertically) != 0)
			{
				spriteBatch.Draw(texture, transform.Position, _frameList[frameNumber].Rectangle, color, transform.Rotation, new Vector2((float)_frameList[frameNumber].Rectangle.Width - _frameList[frameNumber].Pivot.X, (float)_frameList[frameNumber].Rectangle.Height - _frameList[frameNumber].Pivot.Y), transform.Scale, spriteEffects, 0f);
			}
			else
			{
				spriteBatch.Draw(texture, transform.Position, _frameList[frameNumber].Rectangle, color, transform.Rotation, new Vector2((float)_frameList[frameNumber].Rectangle.Width - _frameList[frameNumber].Pivot.X, _frameList[frameNumber].Pivot.Y), transform.Scale, spriteEffects, 0f);
			}
		}
		else if ((spriteEffects & SpriteEffects.FlipVertically) != 0)
		{
			spriteBatch.Draw(texture, transform.Position, _frameList[frameNumber].Rectangle, color, transform.Rotation, new Vector2(_frameList[frameNumber].Pivot.X, (float)_frameList[frameNumber].Rectangle.Height - _frameList[frameNumber].Pivot.Y), transform.Scale, spriteEffects, 0f);
		}
		else
		{
			spriteBatch.Draw(texture, transform.Position, _frameList[frameNumber].Rectangle, color, transform.Rotation, _frameList[frameNumber].Pivot, transform.Scale, spriteEffects, 0f);
		}
	}

	public void DrawTexture(ref SpriteBatch spriteBatch, float x, float y, Color color)
	{
		spriteBatch.Draw(Texture, new Vector2(x, y), color);
	}

	internal static void LoadFromIniFile(SpriteAtlas atlas, IniFile iniFile, GameContent content)
	{
		atlas.FileName = iniFile.FileName;
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
			if (iniFile.KeyEquals("blend"))
			{
				if (iniFile.GetIntValue() == 1)
				{
					atlas.BlendState = BlendState.Additive;
				}
				else
				{
					atlas.BlendState = BlendState.AlphaBlend;
				}
			}
			else if (iniFile.KeyEquals("frame"))
			{
				int[] intArray = iniFile.GetIntArray();
				if (intArray.Length == 1)
				{
					if (intArray[0] <= 1)
					{
						atlas.OneFrame();
					}
					else
					{
						atlas.GridFrames(intArray[0], 1);
					}
				}
				else if (intArray.Length == 4)
				{
					atlas.AddFrame(intArray[0], intArray[1], intArray[2], intArray[3]);
				}
				else if (intArray.Length == 6)
				{
					atlas.AddFrame(intArray[0], intArray[1], intArray[2], intArray[3], intArray[4], intArray[5]);
				}
				else
				{
					iniFile.LogErr("Wrong param count ({0} <> 1, 4 or 6) in key '{1}'", intArray.Length, iniFile.Key);
				}
			}
			else if (iniFile.KeyEquals("grid"))
			{
				int[] intArray2 = iniFile.GetIntArray();
				if (intArray2.Length == 2)
				{
					atlas.GridFrames(intArray2[0], intArray2[1]);
					continue;
				}
				if (intArray2.Length == 6)
				{
					atlas.GridFrames(intArray2[0], intArray2[1], intArray2[2], intArray2[3], intArray2[4], intArray2[5]);
					continue;
				}
				if (intArray2.Length == 8)
				{
					atlas.GridFrames(intArray2[0], intArray2[1], intArray2[2], intArray2[3], intArray2[4], intArray2[5], intArray2[6], intArray2[7]);
					continue;
				}
				iniFile.LogErr("Wrong param count ({0} <> 2 or 6 or 8) in key '{1}'", intArray2.Length, iniFile.Key);
			}
			else if (iniFile.KeyEquals("for_emitter"))
			{
				atlas.IsForEmitter = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("anim"))
			{
				int[] intArray3 = iniFile.GetIntArray();
				if (iniFile.Id == null)
				{
					iniFile.LogErr("Unefined Id");
				}
				else
				{
					atlas.AddAnim(iniFile.Id, intArray3);
				}
			}
			else
			{
				if (!iniFile.KeyEquals("anim_all"))
				{
					continue;
				}
				if (iniFile.Id == null)
				{
					iniFile.LogErr("Unefined Id");
					continue;
				}
				int[] array = new int[atlas.FrameCount];
				for (int i = iniFile.GetIntValue(); i < atlas.FrameCount; i++)
				{
					array[i] = i;
				}
				atlas.AddAnim(iniFile.Id, array);
			}
		}
	}

	public override string ToString()
	{
		return Id;
	}
}
