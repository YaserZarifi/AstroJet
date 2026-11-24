namespace Rake2D.Script;

public class AimToHero : AimToPosition
{
	protected override float GetAngleToTarget()
	{
		return Utils.RadToDeg(MyObject.GetRotationToNearestHero());
	}
}
