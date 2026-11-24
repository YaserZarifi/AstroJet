using System;

namespace Rake2D.Script;

public class AimToAngle : ExitStateBehavior
{
	protected float _targetAngle;

	protected FloatRange _targetAngleRange;

	protected float _aimAngle;

	protected float _speed;

	protected FloatRange _speedRange;

	protected FloatRange _beginDelayRange;

	protected float _delayTimer;

	protected float _blockAngle;

	private bool _ccw;

	private bool _spineRotation;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
		_delayTimer = _beginDelayRange.GetRandom();
		_targetAngle = _targetAngleRange.GetRandom();
		_speed = _speedRange.GetRandom();
	}

	public override void Init()
	{
		base.Init();
		_spineRotation = ParamBool("spine", defaultValue: true);
		_targetAngleRange = ParamRange("angle");
		_blockAngle = Param("block_angle", 999f);
		_speedRange = ParamRange("aim_speed", 180f);
		_beginDelayRange = ParamRange("begin_delay") / 1000f;
	}

	public virtual void OnUpdate(float elapsedTime)
	{
		if (_delayTimer >= 0f)
		{
			_delayTimer -= elapsedTime;
			if (_spineRotation && MyObject.SpineConnector != null)
			{
				_aimAngle = MyObject.SpineConnector.GetAngle();
			}
			else
			{
				_aimAngle = Utils.RadToDeg(MyObject.Rotation);
			}
			_aimAngle %= 360f;
			if (_aimAngle >= 180f)
			{
				_aimAngle -= 360f;
			}
			else if (_aimAngle <= -180f)
			{
				_aimAngle += 360f;
			}
			if (_aimAngle < _targetAngle)
			{
				_ccw = false;
			}
			else
			{
				_ccw = true;
			}
			if (new FloatRange(Math.Min(_aimAngle, _targetAngle), Math.Max(_aimAngle, _targetAngle)).IsInRange(_blockAngle))
			{
				_ccw = !_ccw;
			}
		}
		if (!(_delayTimer <= 0f))
		{
			return;
		}
		if (Utils.ChangeAngleToTarget(ref _aimAngle, _targetAngle, _speed * elapsedTime, _ccw))
		{
			MyObject.OnUpdate -= OnUpdate;
			MyObject.OnEditorUpdate -= OnUpdate;
			if (_exitStateArray != null)
			{
				StartExit();
			}
		}
		if (_spineRotation && MyObject.SpineConnector != null)
		{
			MyObject.SpineConnector.SetAngle(_aimAngle, bWorld: false);
		}
		else
		{
			MyObject.SetRotation(Utils.DegToRad(_aimAngle));
		}
	}
}
