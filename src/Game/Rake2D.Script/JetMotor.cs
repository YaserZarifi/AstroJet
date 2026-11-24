namespace Rake2D.Script;

public class JetMotor : BehaviorScript
{
	private float _maxForce;

	private float _acceleration;

	private float _currThrust;

	private float _beginDelay;

	private float _jetRotation;

	public override void Init()
	{
		_maxForce = Param("max_force", 1000f);
		_acceleration = ParamRandom("speed_up", 10000f);
		_beginDelay = ParamRandom("begin_delay", 0f) / 1000f;
		_jetRotation = Utils.DegToRad(Param("jet_angle"));
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public void Update(float elapsedTime)
	{
		if (_beginDelay <= 0f)
		{
			Utils.ChangeValueToTarget(ref _currThrust, _maxForce, _acceleration * elapsedTime);
			if (MyObject.RigidBody == null || MyObject.RigidBody.IsStatic)
			{
				MyObject.SetVelocity(Utils.GetVelocityVector2(_currThrust, MyObject.WorldRotation + _jetRotation));
			}
			else
			{
				MyObject.ApplyForce(Utils.GetVelocityVector2(_currThrust, MyObject.WorldRotation + _jetRotation));
			}
		}
		_beginDelay -= elapsedTime;
	}
}
