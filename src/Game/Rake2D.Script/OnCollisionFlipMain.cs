using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionFlipMain : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		float num = Utils.RadToDeg(Utils.GetRotationFromZero(normal));
		GameObject mainPart = MyObject.GetMainPart();
		if ((num < -70f && num > -110f && mainPart.Scale.X < 0f) || (num > 70f && num < 110f && mainPart.Scale.X > 0f))
		{
			mainPart.SetScale(mainPart.Scale.X * -1f, mainPart.Scale.Y);
		}
	}
}
