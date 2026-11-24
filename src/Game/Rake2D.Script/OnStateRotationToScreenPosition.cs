using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateRotationToScreenPosition : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		Vector2 vec = new Vector2(1920f, 1080f) * ParamVector2("position", 50f, 50f) / 100f;
		MyObject.SetRotation(Utils.GetRotationTo(MyObject.VirtualScreenPosition, vec));
		StartExit();
	}
}
