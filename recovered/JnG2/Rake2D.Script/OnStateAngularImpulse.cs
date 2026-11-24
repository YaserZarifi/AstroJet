using System;

namespace Rake2D.Script;

public class OnStateAngularImpulse : ExitStateBehavior
{
	private FloatRange _impulseRange;

	private float _scaleFactor;

	public override void Init()
	{
		base.Init();
		_impulseRange = ParamRange("impulse", 100f);
		_scaleFactor = Param("scale_factor", 100f) / 100f;
	}

	public override void RegisterEvents()
	{
		float num = 1f - (1f - Math.Abs(MyObject.Scale.X)) * _scaleFactor;
		MyObject.ApplyAngularImpulse(Utils.DegToRad(_impulseRange.GetRandom() * num));
	}
}
