namespace Rake2D.Script;

public class AimToHeroX : BehaviorScript
{
	protected float _startAngle = -999f;

	protected float _aimAngle;

	protected float _aimSpeed;

	protected IntRange _aimAngleRange;

	protected bool _fullCircle;

	protected bool _aimCounterClockwise;

	protected bool _useSpineAngle;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		InitValues();
	}

	public override void Init()
	{
		base.Init();
		_startAngle = ParamInt("start_angle", -999);
		_useSpineAngle = ParamBool("use_spine", defaultValue: true);
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
		if (_startAngle == -999f)
		{
			if (_useSpineAngle && MyObject.SpineConnector != null)
			{
				_aimAngle = MyObject.SpineConnector.GetWorldAngle();
			}
			else
			{
				_aimAngle = Utils.RadToDeg(MyObject.Rotation);
			}
			if (!MyObject.IsMainPart)
			{
				_aimAngle += Utils.RadToDeg(MyObject.GetMainPart().WorldRotation);
			}
		}
		else
		{
			_aimAngle = _startAngle;
			SetRotation();
		}
	}

	private void SetRotation()
	{
		if (_useSpineAngle && !MyObject.IsMainPart && MyObject.SpineConnector != null)
		{
			if (MyObject.ParentObject != null)
			{
				MyObject.SpineConnector.SetAngle(_aimAngle - Utils.RadToDeg(MyObject.ParentObject.WorldRotation), bWorld: false);
			}
			else
			{
				MyObject.SpineConnector.SetAngle(_aimAngle, bWorld: false);
			}
		}
		else if (MyObject.IsMainPart)
		{
			MyObject.SetRotation(Utils.DegToRad(_aimAngle));
		}
		else
		{
			MyObject.SetRotation(Utils.DegToRad(_aimAngle) - MyObject.GetMainPart().WorldRotation);
		}
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (_aimSpeed <= 0f || !MyObject.GetMainPart().IsTriggerFree)
		{
			return;
		}
		float num = Utils.RadToDeg(MyObject.GetRotationToNearestHero());
		if (!MyObject.IsMainPart)
		{
			Utils.RadToDeg(MyObject.GetMainPart().WorldRotation);
		}
		float num2 = Utils.RadToDeg(MyObject.ParentObject.WorldRotation);
		IntRange intRange = _aimAngleRange + num2;
		bool ccw = false;
		if (_fullCircle)
		{
			if ((num <= _aimAngle && _aimAngle - num < 180f) || (num > _aimAngle && _aimAngle - num < -180f))
			{
				ccw = true;
			}
		}
		else
		{
			int midValue = intRange.GetMidValue();
			float num3 = num - (float)midValue;
			float num4 = _aimAngle - (float)midValue;
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
			IntRange intRange2 = intRange - midValue;
			if (_aimCounterClockwise)
			{
				if (num3 > (float)intRange2.From && num3 <= 0f)
				{
					num = intRange.From;
					num3 = intRange2.From;
					ccw = false;
				}
				else if (num3 < (float)intRange2.To && num3 >= 0f)
				{
					num = intRange.To;
					num3 = intRange2.To;
					ccw = true;
				}
				else if ((num3 <= num4 && num4 - num3 < 180f) || (num3 > num4 && num4 - num3 < -180f))
				{
					ccw = true;
				}
			}
			else
			{
				if (num3 < (float)intRange2.From)
				{
					num = intRange.From;
					num3 = intRange2.From;
				}
				else if (num3 > (float)intRange2.To)
				{
					num = intRange.To;
					num3 = intRange2.To;
				}
				ccw = !(num4 < num3);
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
		Utils.ChangeAngleToTarget(ref _aimAngle, num, _aimSpeed * elapsedTime, ccw);
		Log("AA2: {0}, parent angle: {1}", num, num2);
		SetRotation();
	}
}
