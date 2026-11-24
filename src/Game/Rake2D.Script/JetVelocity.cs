using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class JetVelocity : ExitStateBehavior
{
	private float _maxSpeed;

	private float _acceleration;

	private float _currSpeed;

	private float _beginDelay;

	private float _jetRotation;

	private float _scrollFactor;

	private Vector2 _forceVector;

	private bool _bInitialized;

	private bool _bUseInitSpeed;

	public override void Init()
	{
		base.Init();
		_scrollFactor = Param("scroll_factor") / 100f;
		_maxSpeed = ParamRandomInt("max_speed", 200);
		_acceleration = Param("speed_up", 10000f);
		_beginDelay = ParamRandom("begin_delay", 0f) / 1000f;
		_bUseInitSpeed = ParamBool("use_init_speed");
		_jetRotation = Utils.DegToRad(Param("jet_angle"));
		_bInitialized = false;
		float num = Param("force");
		if (num != 0f)
		{
			float rotation = Utils.DegToRad(Param("force_angle"));
			_forceVector = Utils.GetVelocityVector2(num, rotation);
		}
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
		_bInitialized = false;
	}

	public void Update(float elapsedTime)
	{
		if (!_bInitialized && _bUseInitSpeed)
		{
			_currSpeed = MyObject.Velocity.Length();
			_bInitialized = true;
		}
		_beginDelay -= elapsedTime;
		if (_beginDelay <= 0f)
		{
			if (Utils.ChangeValueToTarget(ref _currSpeed, _maxSpeed, _acceleration * elapsedTime))
			{
				StartExit();
			}
			MyObject.SetVelocity(Utils.GetVelocityVector2(_currSpeed, MyObject.WorldRotation + _jetRotation) + _forceVector + MyLayer.ScrollSpeed * _scrollFactor);
		}
	}
}
