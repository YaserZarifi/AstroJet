using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateImpulse : ExitStateBehavior
{
	private FloatRange _impulseRange;

	private FloatRange _impulseRotationRange;

	private float _scaleFactor;

	public override void Init()
	{
		base.Init();
		_impulseRange = ParamRange("impulse", 100f);
		_impulseRotationRange = ParamRange("impulse_angle") * ((float)Math.PI / 180f);
		_scaleFactor = Param("scale_factor", 100f) / 100f;
	}

	public override void RegisterEvents()
	{
		Vector2 vector = Vector2.One - (Vector2.One - Utils.Abs(MyObject.Scale)) * _scaleFactor;
		if (MyObject.Scale.X < 0f)
		{
			vector.X *= -1f;
		}
		if (MyObject.Scale.Y < 0f)
		{
			vector.Y *= -1f;
		}
		MyObject.ApplyLinearImpulse(Utils.GetVelocityVector2(_impulseRange.GetRandom(), _impulseRotationRange.GetRandom()) * vector);
	}
}
