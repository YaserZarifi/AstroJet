namespace Rake2D.Script;

public class AngleByVelocityY : BehaviorScript
{
	private float _maxAngle;

	private float _velocityFactor;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		_maxAngle = Param("max_angle");
		_velocityFactor = Param("factor", 100f) / 100f;
	}

	private void Update(float elapsedTime)
	{
		GameObject mainPart = MyObject.GetMainPart();
		float num = MyObject.Velocity.Y * Utils.Cos(mainPart.WorldRotation) - MyObject.Velocity.X * Utils.Sin(mainPart.WorldRotation);
		MyObject.SetRotation(Utils.DegToRad(num * _velocityFactor));
	}
}
