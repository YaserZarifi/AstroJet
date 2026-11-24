using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public abstract class SpeedKeyValueBehavior : ExitStateBehavior
{
	private float _startSpeed;

	private float _endSpeed;

	private float _duration;

	private bool _useStartSpeed;

	private bool _useEndSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUnregisterEvents += OnUnregisterEvents;
		if (MyObject.SpeedKeyValue == null)
		{
			MyObject.SpeedKeyValue = new KeyFrameValue(MyObject);
		}
		MyObject.SpeedKeyValue.OnEndValueReached += OnSpeedReached;
	}

	private void OnUnregisterEvents()
	{
		MyObject.SpeedKeyValue.OnEndValueReached -= OnSpeedReached;
	}

	public override void Init()
	{
		base.Init();
		if (HasParam("start_speed"))
		{
			_startSpeed = Param("start_speed");
			_useStartSpeed = true;
		}
		else
		{
			_useStartSpeed = false;
		}
		if (HasParam("end_speed"))
		{
			_endSpeed = Param("end_speed");
			_duration = Param("duration") / 1000f;
			_useEndSpeed = true;
		}
		else
		{
			_useEndSpeed = false;
		}
	}

	private void OnSpeedReached()
	{
		MyObject.SpeedKeyValue.OnEndValueReached -= OnSpeedReached;
		StartExit();
	}

	protected void SetSpeedKey()
	{
		float num;
		if (MyObject.Velocity == Vector2.Zero)
		{
			if (_useStartSpeed)
			{
				num = _startSpeed;
			}
			else
			{
				num = ((MyObject.SpeedKeyValue != null) ? MyObject.SpeedKeyValue.CurrentValue : 0f);
			}
		}
		else
		{
			num = MyObject.Velocity.Length();
		}
		if (_useEndSpeed)
		{
			MyObject.SpeedKeyValue.SetAll(TimeSourceEnum.ObjectTime, num, MyObject.Timer, _endSpeed, MyObject.Timer + _duration);
			MyObject.SpeedKeyValue.IsEndTimeOnPath = false;
		}
		else if (_useStartSpeed)
		{
			MyObject.SpeedKeyValue.SetStartTimeToCurrentTime();
			MyObject.SpeedKeyValue.StartValue = num;
			MyObject.SpeedKeyValue.CurrentValue = num;
		}
	}
}
