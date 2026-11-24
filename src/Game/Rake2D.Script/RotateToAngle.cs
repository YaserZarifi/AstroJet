namespace Rake2D.Script;

public class RotateToAngle : ExitStateBehavior
{
	protected float _targetRotation;

	protected FloatRange _targetAngleRange;

	protected float _aimRotation;

	protected float _speed;

	protected FloatRange _speedRange;

	protected FloatRange _beginDelayRange;

	protected float _delayTimer;

	protected bool _isRelative;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
		_delayTimer = _beginDelayRange.GetRandom();
		_targetRotation = Utils.DegToRad(_targetAngleRange.GetRandom());
		if (_isRelative)
		{
			_targetRotation += MyObject.Rotation;
		}
		_speed = Utils.DegToRad(_speedRange.GetRandom());
	}

	public override void Init()
	{
		base.Init();
		_targetAngleRange = ParamRange("angle");
		_isRelative = ParamBool("relative");
		_speedRange = ParamRange("aim_speed", 180f);
		_beginDelayRange = ParamRange("begin_delay") / 1000f;
	}

	public void OnUpdate(float elapsedTime)
	{
		if (_delayTimer > 0f)
		{
			_delayTimer -= elapsedTime;
		}
		if (_delayTimer <= 0f)
		{
			_aimRotation = MyObject.Rotation;
			if (Utils.ChangeValueToTarget(ref _aimRotation, _targetRotation, _speed * elapsedTime))
			{
				MyObject.OnUpdate -= OnUpdate;
				MyObject.OnEditorUpdate -= OnUpdate;
				StartExit();
			}
			MyObject.SetRotation(_aimRotation);
		}
	}
}
