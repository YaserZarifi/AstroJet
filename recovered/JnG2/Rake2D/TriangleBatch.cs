using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class TriangleBatch : BaseBatch
{
	public TriangleBatch(GraphicsDevice graphics)
		: base(graphics)
	{
	}

	public void DrawRect(float x, float y, float width, float height, Color color)
	{
		DrawRect(new Vector2(x, y), new Vector2(x + width - 1f, y + height), color);
	}

	public void DrawRect(Rectangle rect, Color color)
	{
		DrawRect(new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Bottom), color);
	}

	public void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color color)
	{
		if (_indexCount > 1018 || _vertexCount > 508)
		{
			Flush();
		}
		_indexArray[_indexCount++] = _vertexCount;
		_indexArray[_indexCount++] = _vertexCount + 1;
		_indexArray[_indexCount++] = _vertexCount + 2;
		_indexArray[_indexCount++] = _vertexCount;
		_indexArray[_indexCount++] = _vertexCount + 2;
		_indexArray[_indexCount++] = _vertexCount + 3;
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(topLeft.X, topLeft.Y, 0f), color);
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(bottomRight.X, topLeft.Y, 0f), color);
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(bottomRight.X, bottomRight.Y, 0f), color);
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(topLeft.X, bottomRight.Y, 0f), color);
	}

	public void DrawBezierStrip(BezierPath topLine, BezierPath bottomLine, Color color)
	{
		int num = 20;
		Vector2[] vertices = new Vector2[num * 2];
		for (int i = 0; i < num; i++)
		{
			float currTime = (float)topLine.CurveCount * ((float)i / (float)(num - 1));
			vertices[i * 2] = topLine.GetPositionFast(currTime);
			currTime = (float)bottomLine.CurveCount * ((float)i / (float)(num - 1));
			vertices[i * 2 + 1] = bottomLine.GetPositionFast(currTime);
		}
		DrawStrip(ref vertices, color);
	}

	public void DrawStrip(ref Vector2[] vertices, Color color)
	{
		int num = (vertices.Length - 2) * 3;
		int num2 = vertices.Length;
		if (_indexCount > 1024 - num || _vertexCount > 512 - num2)
		{
			Flush();
		}
		for (int i = 0; i < vertices.Length; i++)
		{
			if (i > 1)
			{
				if (i % 2 != 0)
				{
					_indexArray[_indexCount++] = _vertexCount - 2;
					_indexArray[_indexCount++] = _vertexCount - 1;
					_indexArray[_indexCount++] = _vertexCount;
				}
				else
				{
					_indexArray[_indexCount++] = _vertexCount - 2;
					_indexArray[_indexCount++] = _vertexCount;
					_indexArray[_indexCount++] = _vertexCount - 1;
				}
			}
			_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(vertices[i].X, vertices[i].Y, 0f), new Color(i * 4, i * 4, i * 4));
		}
	}

	public void DrawTrinangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color)
	{
		if (_indexCount > 1021)
		{
			Flush();
		}
		_indexArray[_indexCount++] = _vertexCount;
		_indexArray[_indexCount++] = _vertexCount + 1;
		_indexArray[_indexCount++] = _vertexCount + 2;
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(v1.X, v1.Y, 0f), color);
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(v2.X, v2.Y, 0f), color);
		_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(v3.X, v3.Y, 0f), color);
	}

	protected override void DrawPrimitives()
	{
		_graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexArray, 0, _vertexCount, _indexArray, 0, _indexCount / 3);
	}
}
