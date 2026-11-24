using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateCreate : CreateObjectBehavior
{
	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			CreateObjectList(null, Vector2.Zero);
		}
	}
}
