using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Editor;

public class Cursor : Transformation
{
	public Color TextColor;

	public string Text;

	private SpriteRenderer _spriteRenderer;

	public Editor Editor;

	public Cursor(Editor editor)
	{
		SpriteAtlas spriteAtlas = GameCore.GetSpriteAtlas("editor_cursor");
		_spriteRenderer = new SpriteRenderer(spriteAtlas);
		Editor = editor;
		Position = default;
		TextColor = Color.White;
	}

	public void SetType(CursorType type)
	{
		switch (type)
		{
		case CursorType.Arrow:
			_spriteRenderer.Frame = 0;
			break;
		case CursorType.ResizeVertical:
			_spriteRenderer.Frame = 1;
			break;
		case CursorType.Hand:
			_spriteRenderer.Frame = 2;
			break;
		}
	}

	public void Update(float elapsedTime)
	{
		Position.X = GameInput.MouseX;
		Position.Y = GameInput.MouseY;
		_spriteRenderer.Update(elapsedTime);
	}

	public void Draw()
	{
		if (Text != null)
		{
			int num = ((!(Position.Y < (float)(GameCore.GraphicsDeviceStatic.Viewport.Height - 50))) ? ((int)Position.Y - 20) : ((int)Position.Y + 30));
			int num2 = (int)Position.X + 20;
			int textWidth = Editor.Font.GetTextWidth(Text);
			if (Editor.Font.GetTextWidth(Text) + num2 > GameCore.MainViewPort.Bounds.Right)
			{
				Editor.TriangleBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
				Editor.TriangleBatch.DrawRect(GameCore.MainViewPort.Bounds.Right - textWidth - 3, num - 1, textWidth + 4, 20f, new Color(0, 0, 0, 120));
				Editor.TriangleBatch.End();
				Editor.Font.Write(Editor.SpriteBatch, Text, GameCore.MainViewPort.Bounds.Right, num, TextColor, TextAlign.Right);
			}
			else
			{
				Editor.TriangleBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
				Editor.TriangleBatch.DrawRect(num2 - 3, num - 1, textWidth + 4, 20f, new Color(0, 0, 0, 100));
				Editor.TriangleBatch.End();
				Editor.Font.Write(Editor.SpriteBatch, Text, num2, num, TextColor);
			}
		}
		_spriteRenderer.Color = Color;
		_spriteRenderer.Draw(this, Editor.SpriteBatch);
	}

	public bool IsInCircle(Vector2 pos, int radius)
	{
		return Vector2.Distance(Position, pos) <= (float)radius;
	}

	public bool IsInRect(Rectangle rect)
	{
		return IsInRect(rect.X, rect.Y, rect.Width, rect.Height);
	}

	public bool IsInRect(Vector2 position, int width, int height)
	{
		return IsInRect(position.X, position.Y, width, height);
	}

	public bool IsInRect(float x, float y, int width, int height)
	{
		if (Position.X >= x && Position.X < x + (float)width && Position.Y >= y)
		{
			return Position.Y < y + (float)height;
		}
		return false;
	}

	public bool IsInCurrentLayerRect(int x, int y, int width, int height)
	{
		Vector2 currentLayerPosition = GetCurrentLayerPosition();
		if (currentLayerPosition.X >= (float)x && currentLayerPosition.X < (float)(x + width) && currentLayerPosition.Y >= (float)y)
		{
			return currentLayerPosition.Y < (float)(y + height);
		}
		return false;
	}

	public Vector2 GetCurrentLayerPosition()
	{
		return GameCore.CurrentLevel.Camera.ScreenPosToCurrentLayer(Position);
	}

	public Vector2 GetMainLayerPosition()
	{
		return GameCore.CurrentLevel.Camera.ScreenPosToMainLayer(Position);
	}

	public Vector2 GetLayerPosition(Layer layer)
	{
		return GameCore.CurrentLevel.Camera.ScreenPosToLayer(Position, layer);
	}
}
