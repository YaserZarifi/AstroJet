using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Menu.Atarix;

public class TronRect
{
	public enum RectTypeEnum
	{
		None,
		Snake,
		Wall,
		Point
	}

	public int X;

	public int Y;

	public int Direction;

	public int Frame;

	public int ShadowSize;

	public const int RECT_W = 10;

	public const int RECT_H = 10;

	public RectTypeEnum Type;

	private AtarixTron _tron;

	public TronRect(AtarixTron tron)
	{
		_tron = tron;
	}

	public void DrawShadow()
	{
		Transformation transformation = new Transformation();
		transformation.Position = new Vector2((float)(X * 10) + _tron.GamePosition.X + (float)ShadowSize, (float)(Y * 10) + _tron.GamePosition.Y + (float)ShadowSize);
		_tron.Atlas.DrawFrame(_tron.SpriteBatch, Frame, new Color(0, 0, 0, 100), transformation, BlendModeEnum.Normal);
	}

	public void Draw()
	{
		Transformation transformation = new Transformation();
		transformation.Position = new Vector2((float)(X * 10) + _tron.GamePosition.X, (float)(Y * 10) + _tron.GamePosition.Y);
		switch (Direction)
		{
		case 1:
			transformation.Rotation = (float)Math.PI / 2f;
			break;
		case 2:
			transformation.Rotation = (float)Math.PI;
			break;
		case 3:
			transformation.Rotation = -(float)Math.PI / 2f;
			break;
		}
		_tron.Atlas.DrawFrame(_tron.SpriteBatch, Frame, Color.White, transformation, BlendModeEnum.Normal);
	}
}
