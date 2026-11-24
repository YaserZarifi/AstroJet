using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Debug;

public static class DebugDraw
{
	private static LineBatch _lineBatch;

	private static BitmapFont _font;

	private static List<DebugPrint> _printList;

	static DebugDraw()
	{
		_lineBatch = new LineBatch(GameCore.GraphicsDeviceStatic);
		_printList = new List<DebugPrint>();
	}

	public static void Init(BitmapFont font)
	{
		_font = font;
	}

	public static void Print(int x, int y, string message, params object[] args)
	{
		DebugPrint item = default;
		item.text = string.Format(message, args);
		item.x = x;
		item.y = y;
		_printList.Add(item);
	}

	public static void DrawPrints()
	{
		GameCore.DrawBegin();
		foreach (DebugPrint print in _printList)
		{
			_font.Write(GameCore.SpriteBatch, print.text, print.x, print.y, Color.Green);
		}
		GameCore.DrawEnd();
		_printList.Clear();
	}

	public static void ShowScreenFrames()
	{
		if (GameCore.CurrentLevel.Camera.Zoom < 1f)
		{
			_lineBatch.Begin(BlendState.Additive, Matrix.Identity);
			_lineBatch.DrawRect(GameCore.CurrentLevel.Camera.ViewTopLeftOnScreen, GameCore.CurrentLevel.Camera.ViewBottomRightOnScreen, new Color(100, 100, 0));
			_lineBatch.End();
		}
		if (GameCore.CurrentLevel.Camera.Zoom < 1f || GameCore.IsEditorEnabled)
		{
			_lineBatch.Begin(BlendState.Additive, GameCore.CurrentLevel.Camera.WorldMatrixMainLayer);
			_lineBatch.DrawRect(new Vector2(0f, 0f), GameCore.CurrentLevel.Size - Vector2.One, new Color(100, 0, 0));
			_lineBatch.End();
		}
	}

	public static void ShowVisibilityRect(GameObject go)
	{
		_lineBatch.Begin(BlendState.Additive, GameCore.CurrentLevel.Camera.WorldMatrixCurrentLayer);
		Vector2 topLeft = new Vector2(go.VisibilityRect.Left, go.VisibilityRect.Top) + go.WorldPosition;
		Vector2 bottomRight = new Vector2(go.VisibilityRect.Right, go.VisibilityRect.Bottom) + go.WorldPosition;
		_lineBatch.DrawRect(topLeft, bottomRight, new Color(100, 100, 30));
		_lineBatch.End();
	}

	public static void ShowGameObjectRect(GameObject go, Rectangle rect, Color color)
	{
		_lineBatch.Begin(BlendState.Additive, Matrix.Identity);
		Vector2[] frameVerices = go.GetFrameVerices(bWorld: true, rect);
		for (int i = 0; i < frameVerices.Length; i++)
		{
			frameVerices[i] = GameCore.CurrentLevel.Camera.LayerPosToScreen(frameVerices[i], go.MyLayer);
		}
		_lineBatch.DrawPolygon(frameVerices, color);
		_lineBatch.End();
		GameCore.Allocator.DeleteRectangleVertices(frameVerices);
	}
}
