using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionRotate : ExitStateBehavior
{
	private int _rotateDirection;

	private float _startRotation;

	private float _targetRotation;

	private float _maxRotationRight;

	private float _maxRotationLeft;

	private float _turnSpeed;

	private float _backSpeed;

	private string[] _soundIdArray;

	public override void Init()
	{
		base.Init();
		FloatRange floatRange = ParamRange("max_angle", 45f, 45f);
		_maxRotationLeft = Utils.DegToRad(floatRange.From);
		_maxRotationRight = Utils.DegToRad(floatRange.To);
		_turnSpeed = Utils.DegToRad(ParamInt("turn_speed", 180));
		_backSpeed = Utils.DegToRad(ParamInt("back_speed", 30));
		_soundIdArray = ParamStringArray("sound_id");
	}

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
		MyObject.OnUpdate -= OnUpdate;
		_rotateDirection = 0;
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (_rotateDirection == 0)
		{
			_rotateDirection = 1;
			_startRotation = GetRotation();
			MyObject.OnUpdate += OnUpdate;
			float num = Utils.RadToDeg(Utils.GetRotationFromZero(MyObject.GetRelativePosition(contactPoint)));
			if ((num < 0f && num > -90f) || num > 90f)
			{
				_targetRotation = _startRotation + _maxRotationRight;
			}
			else
			{
				_targetRotation = _startRotation + _maxRotationLeft;
			}
			if (_soundIdArray != null)
			{
				MyObject.PlaySound(Utils.GetRandomItem(_soundIdArray));
			}
		}
	}

	public float GetRotation()
	{
		if (MyObject.IsMainPart)
		{
			return MyObject.Rotation;
		}
		if (MyObject.SpineConnector != null)
		{
			return Utils.DegToRad(MyObject.SpineConnector.GetAngle());
		}
		return MyObject.Rotation;
	}

	private void SetRotation(float rotation)
	{
		if (!MyObject.IsMainPart && MyObject.SpineConnector != null)
		{
			MyObject.SpineConnector.SetAngle(Utils.RadToDeg(rotation), bWorld: false);
		}
		else
		{
			MyObject.SetRotation(rotation);
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		float currentValue = GetRotation();
		if (_rotateDirection == 1)
		{
			if (Utils.ChangeValueToTarget(ref currentValue, _targetRotation, _turnSpeed * elapsedTime))
			{
				_rotateDirection = -1;
			}
		}
		else if (_rotateDirection == -1 && Utils.ChangeValueToTarget(ref currentValue, _startRotation, _backSpeed * elapsedTime))
		{
			_rotateDirection = 0;
			MyObject.OnUpdate -= OnUpdate;
		}
		SetRotation(currentValue);
	}
}
