using Rake2D.JnG;

namespace Rake2D.Script;

public class GemEnableFeature : ItemBehaviorScript
{
	public override void ApplyToItem(HeroItem item)
	{
		if (item == null || !(item.GetBehaviorByType(typeof(GemableItemBehavior)) is GemableItemBehavior gemableItemBehavior))
		{
			return;
		}
		string text = ParamString("feature");
		if (Utils.Equals(text, "autotrack"))
		{
			gemableItemBehavior.IsEnabledAutorack = true;
		}
		else if (Utils.Equals(text, "mouse_gun"))
		{
			gemableItemBehavior.IsEnabledMouseGun = true;
			if (MyHero.MouseGun == null)
			{
				MyHero.MouseGun = new MouseGun(MyHero);
			}
		}
		else
		{
			LogErr("Unknown weapon feature '{0}'", text);
		}
	}
}
