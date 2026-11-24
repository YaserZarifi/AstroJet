using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroWeaponSinus : ItemWeapon
{
	public override void RecalculateParams()
	{
		base.RecalculateParams();
		ShotStreamData shotStreamData;
		if ((shotStreamData = AddStreamData()) != null)
		{
			shotStreamData.SoundId = "machine_gun";
			shotStreamData.IsSoundLooped = true;
			shotStreamData.GunFireId = "hero_gun_fire_basic";
		}
	}

	public override void Update(float elapsedTime)
	{
		foreach (ShotStream stream in StreamList)
		{
			if (stream.BezierPath == null)
			{
				stream.BezierPath = new BezierPath("path", null);
				stream.BezierPath.AddCurve(MySlot.WorldPosition, recalculate: false, stream.MyObject.WorldRotation);
			}
			Vector2 value = MySlot.WorldPosition + new Vector2(1200f, 0f);
			float num = MySlot.Rotation + Utils.DegToRad((stream.Data.Number % 2 == 0) ? (-20) : 20);
			stream.BezierPath.ControlPointList[0] = MySlot.WorldPosition;
			stream.BezierPath.ControlPointList[1] = stream.BezierPath.ControlPointList[0] + Utils.GetVelocityVector2(1000f, num);
			stream.BezierPath.ControlPointList[3] = value;
			stream.BezierPath.ControlPointList[2] = stream.BezierPath.ControlPointList[3] + Utils.GetVelocityVector2(500f, num - Utils.DegToRad(180f));
			stream.BezierPath.CalculateLength();
		}
		base.Update(elapsedTime);
	}
}
