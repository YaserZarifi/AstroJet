using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionTakeItem : TakeItemBehavior
{
	public override void RegisterEvents()
	{
		if (_itemData != null)
		{
			MyObject.OnCollision += OnCollision;
		}
	}

	protected void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (collisionObject.MyHero != null)
		{
			TakeItem();
		}
	}
}
