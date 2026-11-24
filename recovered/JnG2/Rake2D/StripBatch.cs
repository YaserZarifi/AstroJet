using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class StripBatch : BaseBatch
{
	private List<StripShape> _shapeList;

	public StripBatch(GraphicsDevice graphics)
		: base(graphics)
	{
		_shapeList = new List<StripShape>();
	}

	public void DrawRect(ref Rectangle rect, Color color)
	{
		DrawRect(new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Bottom), color);
	}

	public void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color color)
	{
		DrawStripShape(new Vector2[4]
		{
			new Vector2(topLeft.X, bottomRight.Y),
			topLeft,
			bottomRight,
			new Vector2(bottomRight.X, topLeft.Y)
		}, color);
	}

	public void DrawStripShape(Vector2[] vertices, Color color)
	{
		if (_vertexCount > 512 - vertices.Length)
		{
			Flush();
		}
		StripShape item = default;
		item.VertexOffset = _vertexCount;
		item.TriangleCount = vertices.Length - 2;
		_shapeList.Add(item);
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector2 vector = vertices[i];
			_vertexArray[_vertexCount++] = new VertexPositionColor(new Vector3(vector.X, vector.Y, 0f), color);
		}
	}

	protected override void DrawPrimitives()
	{
		foreach (StripShape shape in _shapeList)
		{
			GameCore.GraphicsDeviceStatic.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertexArray, shape.VertexOffset, shape.TriangleCount);
		}
		_shapeList.Clear();
	}
}
