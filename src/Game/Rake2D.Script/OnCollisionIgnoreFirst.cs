using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionIgnoreFirst : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	protected void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if ((double)MyObject.Timer < 0.2)
		{
			MyObject.IgnoreCollisionWith(collisionObject);
			MyObject.IgnoreCollisionMode = 2;
		}
		MyObject.OnCollision -= OnCollision;
		IsDone = true;
	}
}
