using System;

namespace Rake2D.Script;

public abstract class HomingToTarget : BehaviorScript
{
	private float _targetVelocity;

	private float _currVelocity;

	private float _acceleration;

	private float _beginTimer;

	private float _endTimer;

	private float _aimSpeed;

	private GameObject _targetObject;

	private float _updateTargetTimer;

	private float _homingRotation;

	private bool _isInitialized;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public override void Init()
	{
		_targetVelocity = ParamRandom("speed", 100f);
		_acceleration = Param("speed_up", 10000f);
		_aimSpeed = Utils.DegToRad(ParamRandom("aim_speed", 180f));
		_beginTimer = ParamRandom("begin_delay", 0f) / 1000f;
		_endTimer = ParamRandom("end_delay", 9999999f) / 1000f;
		_homingRotation = Utils.DegToRad(Param("homing_angle"));
		_updateTargetTimer = 0f;
	}

	protected abstract GameObject GetTargetObject();

	public void Update(float elapsedTime)
	{
		if ((_endTimer -= elapsedTime) <= 0f)
		{
			MyObject.OnUpdate -= Update;
		}
		else
		{
			if (!Utils.ChangeValueToTarget(ref _beginTimer, 0f, elapsedTime))
			{
				return;
			}
			if (!_isInitialized)
			{
				_isInitialized = true;
				_currVelocity = (MyObject.Velocity - MyLevel.ScrollSpeed * MyLayer.ScrollFactor).Length();
				MyObject.RotationSpeed = 0f;
			}
			float currentValue = MyObject.Rotation;
			if (Utils.ChangeValueToTarget(ref _updateTargetTimer, 0f, elapsedTime))
			{
				_updateTargetTimer = 0.2f;
				_targetObject = GetTargetObject();
			}
			if (_targetObject != null)
			{
				float rotationToTarget = MyObject.GetRotationToTarget(_targetObject);
				if (currentValue - rotationToTarget < -(float)Math.PI)
				{
					currentValue += (float)Math.PI * 2f;
				}
				else if (currentValue - rotationToTarget > (float)Math.PI)
				{
					currentValue -= (float)Math.PI * 2f;
				}
				Utils.ChangeValueToTarget(ref currentValue, rotationToTarget, _aimSpeed * elapsedTime);
			}
			MyObject.SetRotation(currentValue);
			Utils.ChangeValueToTarget(ref _currVelocity, _targetVelocity, _acceleration * elapsedTime);
			MyObject.SetVelocity(Utils.GetVelocityVector2(_currVelocity, currentValue + _homingRotation) + MyLevel.ScrollSpeed);
		}
	}
}
