namespace Rake2D.Script;

public class AngleByVelocityX : BehaviorScript
{
	private float _maxAngle;

	private float _velocityFactor;

	private bool _compensateScroll;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		_maxAngle = Param("max_angle");
		_velocityFactor = Param("factor", 100f) / 100f;
		_compensateScroll = ParamBool("compensate_scroll");
	}

	private void Update(float elapsedTime)
	{
		GameObject mainPart = MyObject.GetMainPart();
		float num = (MyObject.Velocity.X - (_compensateScroll ? MyLayer.ScrollSpeed.X : 0f)) * Utils.Cos(mainPart.WorldRotation) - MyObject.Velocity.Y * Utils.Sin(mainPart.WorldRotation);
		MyObject.SetRotation(Utils.DegToRad(num * _velocityFactor));
	}
}
