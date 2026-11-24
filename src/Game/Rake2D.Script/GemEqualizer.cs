using System;
using Rake2D.JnG;

namespace Rake2D.Script;

public class GemEqualizer : ItemBehaviorScript
{
	public override void ApplyToItem(HeroItem item)
	{
		base.ApplyToItem(item);
		float num = 0.001f;
		foreach (HeroSlot value in MyHero.MyShip.Slots.Values)
		{
			if (value.IsWeaponType && value.Item != null && value.Item.GetBehaviorByType(typeof(GemableItemBehavior)) is GemableItemBehavior gemableItemBehavior)
			{
				num = Math.Max(num, gemableItemBehavior.GetDPS());
			}
		}
		if (item.GetBehaviorByType(typeof(GemableItemBehavior)) is GemableItemBehavior gemableItemBehavior2)
		{
			float dPS = gemableItemBehavior2.GetDPS();
			if (dPS < num && dPS > 0f)
			{
				gemableItemBehavior2.EqualizerFactor = num / dPS;
				gemableItemBehavior2.EqualizerCount++;
			}
		}
	}
}
