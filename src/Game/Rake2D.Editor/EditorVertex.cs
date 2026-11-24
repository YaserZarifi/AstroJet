using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class EditorVertex
{
	public Vector2 Position;

	public EditorVertex(Vector2 position)
	{
		Position = position;
	}

	public Vector2 GetWorldPosition(Matrix worldMatrix)
	{
		return Vector2.Transform(Position, worldMatrix);
	}
}
