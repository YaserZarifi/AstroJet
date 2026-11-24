using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class TriangleBatchTextured : BaseBatchTextured
{
	public TriangleBatchTextured(GraphicsDevice graphics)
		: base(graphics)
	{
	}

	public void DrawRect(ref Rectangle rect, Color color, Texture2D texture)
	{
		DrawRect(new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Bottom), color, texture);
	}

	public void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color color, Texture2D texture)
	{
		DrawRect(topLeft, bottomRight, color, texture, new Vector2(0f, 1f), new Vector2(1f, 0f));
	}

	public void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color color, Texture2D texture, Vector2 uvTopLeft, Vector2 uvBottomRight)
	{
		SetTexture(texture);
		if (_indexCount > 1524 || _vertexCount > 508)
		{
			Flush();
		}
		_indexArray[_indexCount++] = _vertexCount;
		_indexArray[_indexCount++] = _vertexCount + 1;
		_indexArray[_indexCount++] = _vertexCount + 2;
		_indexArray[_indexCount++] = _vertexCount;
		_indexArray[_indexCount++] = _vertexCount + 2;
		_indexArray[_indexCount++] = _vertexCount + 3;
		_vertexArray[_vertexCount++] = new VertexPositionColorTexture(new Vector3(topLeft.X, topLeft.Y, 0f), color, uvTopLeft);
		_vertexArray[_vertexCount++] = new VertexPositionColorTexture(new Vector3(bottomRight.X, topLeft.Y, 0f), color, new Vector2(uvBottomRight.X, uvTopLeft.Y));
		_vertexArray[_vertexCount++] = new VertexPositionColorTexture(new Vector3(bottomRight.X, bottomRight.Y, 0f), color, uvBottomRight);
		_vertexArray[_vertexCount++] = new VertexPositionColorTexture(new Vector3(topLeft.X, bottomRight.Y, 0f), color, new Vector2(uvTopLeft.X, uvBottomRight.Y));
	}

	public void DrawStrip(BatchVertextTextured[] vertices, int vertexCount, Texture2D texture)
	{
		if (vertexCount < 3)
		{
			return;
		}
		int num = (vertexCount - 2) * 3;
		SetTexture(texture);
		if (_indexCount > 1530 - num || _vertexCount > 512 - vertexCount)
		{
			Flush();
		}
		for (int i = 0; i < vertexCount; i++)
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
			_vertexArray[_vertexCount++] = new VertexPositionColorTexture(new Vector3(vertices[i].Position.X, vertices[i].Position.Y, 0f), vertices[i].Color, vertices[i].TextureCoordinate);
		}
	}

	public void DrawFan(BatchVertextTextured[] vertices, int vertexCount, Texture2D texture)
	{
		if (vertexCount < 3)
		{
			return;
		}
		int num = (vertexCount - 2) * 3;
		SetTexture(texture);
		if (_indexCount > 1530 - num || _vertexCount > 512 - vertexCount)
		{
			Flush();
		}
		int vertexCount2 = _vertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			if (i < vertexCount - 2)
			{
				_indexArray[_indexCount++] = vertexCount2;
				_indexArray[_indexCount++] = vertexCount2 + i + 1;
				_indexArray[_indexCount++] = vertexCount2 + i + 2;
			}
			_vertexArray[_vertexCount++] = new VertexPositionColorTexture(new Vector3(vertices[i].Position.X, vertices[i].Position.Y, 0f), vertices[i].Color, vertices[i].TextureCoordinate);
		}
	}

	protected override void DrawPrimitives()
	{
		_graphics.RasterizerState = RasterizerState.CullNone;
		_graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexArray, 0, _vertexCount, _indexArray, 0, _indexCount / 3);
	}
}
