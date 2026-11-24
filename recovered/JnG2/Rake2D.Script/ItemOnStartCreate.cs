using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ItemOnStartCreate : ItemBehaviorCreateBase
{
	public override void StartInSlot()
	{
		base.StartInSlot();
		CreateObject(Vector2.Zero);
	}
}
