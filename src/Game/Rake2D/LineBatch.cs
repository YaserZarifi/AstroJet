using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class LineBatch : BaseBatch
{
	public LineBatch(GraphicsDevice graphics)
		: base(graphics)
	{
	}

	public void DrawLine(Vector2 from, Vector2 to, Color color)
	{
		DrawLine(from.X, from.Y, to.X, to.Y, color);
	}

	public void DrawLine(Vector2 from, Vector2 to, Color color1, Color color2)
	{
		DrawLine(from.X, from.Y, to.X, to.Y, color1, color2);
	}

	public void DrawPolygon(Vector2[] vertices, Color color)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector2 vector = vertices[i];
			Vector2 vector2 = ((i >= vertices.Length - 1) ? vertices[0] : vertices[i + 1]);
			DrawLine(vector.X, vector.Y, vector2.X, vector2.Y, color);
		}
	}

	public void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color color, RectangleFrameType frameType = RectangleFrameType.Normal)
	{
		DrawRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, color, frameType);
	}

	public void DrawRect(Vector2 topLeft, float width, float height, Color color, RectangleFrameType frameType = RectangleFrameType.Normal)
	{
		DrawRect(topLeft.X, topLeft.Y, topLeft.X + width - 1f, topLeft.Y + height - 1f, color, frameType);
	}

	public void DrawRect(float x1, float y1, float x2, float y2, Color color, RectangleFrameType frameType = RectangleFrameType.Normal)
	{
		switch (frameType)
		{
		case RectangleFrameType.Normal:
			DrawLine(x1, y1, x2, y1, color);
			DrawLine(x1, y1 + 1f, x1, y2, color);
			DrawLine(x1, y2, x2, y2, color);
			DrawLine(x2, y1, x2, y2, color);
			break;
		case RectangleFrameType.Raised:
		{
			DrawLine(x1, y1, x2 + 1f, y1, color);
			DrawLine(x1, y1 + 1f, x1, y2 + 1f, color);
			byte a = color.A;
			color *= 0.4f;
			color.A = a;
			DrawLine(x1, y2, x2, y2, color);
			DrawLine(x2, y1, x2, y2 + 1f, color);
			break;
		}
		case RectangleFrameType.Lowered:
		{
			DrawLine(x1, y2, x2 + 1f, y2, color);
			DrawLine(x2, y1, x2, y2, color);
			byte a = color.A;
			color *= 0.4f;
			color.A = a;
			DrawLine(x1, y1, x2 + 1f, y1, color);
			DrawLine(x1, y1 + 1f, x1, y2 + 1f, color);
			break;
		}
		}
	}

	public void DrawLine(float x1, float y1, float x2, float y2, Color color)
	{
		DrawLine(x1, y1, x2, y2, color, color);
	}

	public void DrawLine(float x1, float y1, float x2, float y2, Color color1, Color color2)
	{
		if (_vertexCount > 510)
		{
			Flush();
		}
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(x1, y1, 0f), color1);
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(x2, y2, 0f), color2);
	}

	protected override void DrawPrimitives()
	{
		_graphics.DrawUserPrimitives(PrimitiveType.LineList, _vertexArray, 0, _vertexCount / 2);
	}
}
