using Microsoft.Xna.Framework;

namespace Rake2D.Script.HeroShot;

public class Medusa : BehaviorScript
{
	public override void RegisterEvents()
	{
		if (MyObject.BehaviorData is HeroShotData heroShotData)
		{
			MyObject.MyPath = new BezierPath("path", null);
			MyObject.MyPath.AddCurve(heroShotData.StartObject.WorldPosition, recalculate: false, MyObject.Rotation);
			Vector2 value = MyLevel.FindNearestObject(heroShotData.StartObject.WorldPosition, (TargetClass)8376, 1000f, null, bOnlyHittable: true)?.WorldPosition ?? (heroShotData.StartObject.WorldPosition + new Vector2(Utils.GetRandom(3000, 3000, bRandonNegative: true), Utils.GetRandom(2000, 3000, bRandonNegative: true)));
			float rotation = heroShotData.StartObject.WorldRotation + Utils.DegToRad((heroShotData.Number % 2 == 0L) ? (-20) : 20);
			MyObject.MyPath.ControlPointList[0] = heroShotData.StartObject.WorldPosition;
			MyObject.MyPath.ControlPointList[1] = MyObject.MyPath.ControlPointList[0] + Utils.GetVelocityVector2(1000f, rotation);
			MyObject.MyPath.ControlPointList[3] = value;
			MyObject.MyPath.ControlPointList[2] = MyObject.MyPath.ControlPointList[3] + Utils.GetVelocityVector2(500f, rotation);
			MyObject.MyPath.CalculateLength();
			MyObject.PathSpeed = heroShotData.Speed;
		}
	}
}
