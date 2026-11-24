namespace Rake2D.Script;

public abstract class AimToPosition : BehaviorScript
{
	protected float _startAngle = -999f;

	protected float _aimSpeed;

	protected float _angleDelta;

	protected float _northAngle;

	protected IntRange _aimAngleRange;

	protected bool _fullCircle;

	protected bool _aimCounterClockwise;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		InitValues();
	}

	public override void Init()
	{
		base.Init();
		_startAngle = ParamInt("start_angle", -999);
		_northAngle = ParamInt("north");
		_fullCircle = ParamBool("full_circle");
		if (!_fullCircle)
		{
			_aimAngleRange = ParamIntRange("aim_arc", -90, 90);
			if (_aimAngleRange.From > _aimAngleRange.To)
			{
				_aimAngleRange.SwapFromTo();
				_aimCounterClockwise = true;
			}
		}
		_aimSpeed = Param("aim_speed", 180f);
	}

	private void InitValues()
	{
		if (_startAngle != -999f)
		{
			SetAimAngle(_startAngle);
		}
	}

	private float GetAimAngle()
	{
		float num;
		if (!MyObject.IsMainPart)
		{
			num = ((MyObject.SpineConnector == null) ? Utils.RadToDeg(MyObject.Rotation) : MyObject.SpineConnector.GetAngle());
			num = ((!(MyObject.GetMainPart().Scale.X < 0f)) ? (num + Utils.RadToDeg(MyObject.ParentObject.WorldRotation)) : (num - Utils.RadToDeg(MyObject.ParentObject.WorldRotation)));
		}
		else
		{
			num = Utils.RadToDeg(MyObject.Rotation);
		}
		num %= 360f;
		if (num >= 180f)
		{
			num -= 360f;
		}
		else if (num <= -180f)
		{
			num += 360f;
		}
		return num;
	}

	private void SetAimAngle(float angle)
	{
		angle -= _northAngle;
		if (MyObject.IsMainPart)
		{
			MyObject.SetRotation(Utils.DegToRad(angle));
		}
		else if (MyObject.SpineConnector != null)
		{
			if (MyObject.GetMainPart().Scale.X < 0f)
			{
				MyObject.SpineConnector.SetAngle(angle + Utils.RadToDeg(MyObject.ParentObject.WorldRotation), bWorld: false);
			}
			else
			{
				MyObject.SpineConnector.SetAngle(angle - Utils.RadToDeg(MyObject.ParentObject.WorldRotation), bWorld: false);
			}
		}
		else if (MyObject.GetMainPart().Scale.X < 0f)
		{
			MyObject.SetRotation(Utils.DegToRad(angle) + MyObject.ParentObject.WorldRotation);
		}
		else
		{
			MyObject.SetRotation(Utils.DegToRad(angle) - MyObject.ParentObject.WorldRotation);
		}
	}

	protected abstract float GetAngleToTarget();

	protected virtual void OnUpdate(float elapsedTime)
	{
		if (_aimSpeed <= 0f || !MyObject.GetMainPart().IsTriggerFree)
		{
			return;
		}
		float currentAngle = GetAimAngle() + _northAngle;
		float num = GetAngleToTarget();
		float num2 = (MyObject.IsMainPart ? 0f : Utils.RadToDeg(MyObject.GetMainPart().WorldRotation));
		IntRange aimAngleRange = _aimAngleRange;
		if (MyObject.GetMainPart().WorldScale.X < 0f)
		{
			num *= -1f;
			aimAngleRange -= num2;
		}
		else
		{
			aimAngleRange += num2;
		}
		bool ccw = false;
		if (_fullCircle)
		{
			if ((num <= currentAngle && currentAngle - num < 180f) || (num > currentAngle && currentAngle - num < -180f))
			{
				ccw = true;
			}
		}
		else
		{
			int midValue = aimAngleRange.GetMidValue();
			float num3 = num - (float)midValue;
			float num4 = currentAngle - (float)midValue;
			if (num3 > 180f)
			{
				num3 -= 360f;
			}
			else if (num3 < -180f)
			{
				num3 += 360f;
			}
			if (num4 > 180f)
			{
				num4 -= 360f;
			}
			else if (num4 < -180f)
			{
				num4 += 360f;
			}
			IntRange intRange = aimAngleRange - midValue;
			if (_aimCounterClockwise)
			{
				if (num3 > (float)intRange.From && num3 <= 0f)
				{
					num = aimAngleRange.From;
					ccw = false;
				}
				else if (num3 < (float)intRange.To && num3 >= 0f)
				{
					num = aimAngleRange.To;
					ccw = true;
				}
				else if ((num3 <= num4 && num4 - num3 < 180f) || (num3 > num4 && num4 - num3 < -180f))
				{
					ccw = true;
				}
			}
			else
			{
				if (num3 < (float)intRange.From)
				{
					num = aimAngleRange.From;
					num3 = intRange.From;
				}
				else if (num3 > (float)intRange.To)
				{
					num = aimAngleRange.To;
					num3 = intRange.To;
				}
				ccw = !(num4 <= num3);
			}
		}
		if (num > 180f)
		{
			num -= 360f;
		}
		else if (num < -180f)
		{
			num += 360f;
		}
		Utils.ChangeAngleToTarget(ref currentAngle, num, _aimSpeed * elapsedTime, ccw);
		SetAimAngle(currentAngle);
	}
}
