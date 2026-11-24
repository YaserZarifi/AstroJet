using System;

namespace Rake2D.Script;

public class FlipYByAngle : BehaviorScript
{
	private float _north;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		_north = Utils.DegToRad(Param("north"));
	}

	private void Update(float elapsedTime)
	{
		if (MyObject.Scale.Y > 0f && MyObject.WorldRotation + _north < 0f && MyObject.WorldRotation + _north > -(float)Math.PI)
		{
			MyObject.SetScale(MyObject.Scale.X, MyObject.Scale.Y * -1f);
		}
		else if (MyObject.Scale.Y < 0f && (MyObject.WorldRotation + _north >= 0f || MyObject.WorldRotation + _north <= -(float)Math.PI))
		{
			MyObject.SetScale(MyObject.Scale.X, MyObject.Scale.Y * -1f);
		}
	}
}
