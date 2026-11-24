using Microsoft.Xna.Framework;

namespace Rake2D;

public class RayCastResult
{
	public Vector2 StartPoint;

	public Vector2 CollisionPosition;

	public Vector2 CollisionNormal;

	public GameObject TargetObject;

	public float Length => (CollisionPosition - StartPoint).Length();
}
