using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionQuake : BehaviorQuakeBase
{
	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		Quake();
	}
}
