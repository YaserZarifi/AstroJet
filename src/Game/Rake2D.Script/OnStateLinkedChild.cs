using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateLinkedChild : BehaviorScript
{
	public override void RegisterEvents()
	{
		if (MyObject.LinkedObjectList == null || !DoInEditor)
		{
			return;
		}
		foreach (GameObject linkedObject in MyObject.LinkedObjectList)
		{
			if (linkedObject.ParentObject == null)
			{
				Vector2 vector = linkedObject.Position - MyObject.Position;
				MyObject.AddChildRelative(linkedObject, vector.X, vector.Y, linkedObject.ZOrder);
			}
		}
	}
}
