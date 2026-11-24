using Microsoft.Xna.Framework;

namespace Rake2D;

public class PolygonVertex
{
	public Vector2 Position;

	public Color Color;

	public PolygonVertex(Vector2 position)
	{
		Position = position;
		Color = Color.White;
	}

	public PolygonVertex(Vector2 position, Color color)
	{
		Position = position;
		Color = color;
	}

	public void ChangePosition(Vector2 deltaPos)
	{
		Position += deltaPos;
	}
}
