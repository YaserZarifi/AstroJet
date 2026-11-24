using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateInvokeOnCollision : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		MyObject.DoBeforeCollision(null);
		MyObject.InvokeOnCollision(null, MyObject.WorldPosition, new Vector2(0f, -1f));
		StartExit();
	}
}
