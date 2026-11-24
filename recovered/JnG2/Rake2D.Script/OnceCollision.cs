using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnceCollision : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		MyObject.IgnoreCollisionWith(collisionObject);
	}
}
