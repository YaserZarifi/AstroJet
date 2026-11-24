using Rake2D.JnG;

namespace Rake2D.Script;

public class GemMissileHit : ItemBehaviorScript
{
	public override void ApplyToItem(HeroItem item)
	{
		if (item != null && item.GetBehaviorByType(typeof(GemableItemBehavior)) is GemableItemBehavior gemableItemBehavior)
		{
			gemableItemBehavior.ExtraHitTo = PhysClass.MissileEnemy;
		}
	}
}
