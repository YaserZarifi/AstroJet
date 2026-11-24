using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateRemovePath : BehaviorScript
{
	public override void RegisterEvents()
	{
		Vector2 velocity = MyObject.Velocity;
		MyObject.MyPath = null;
		MyObject.IsMovable = true;
		MyObject.SetVelocity(velocity);
	}
}
