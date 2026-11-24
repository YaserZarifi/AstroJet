namespace Rake2D.Script;

public class PathRider : BehaviorScript
{
	public static string[] PathEvents = new string[2] { "Speed", "Angle" };

	private static string _SPEED_EVENT_NAME = PathEvents[0];

	private static string _ANGLE_EVENT_NAME = PathEvents[1];

	private const int USE_PATH_ANGLE = -999;

	private const int USE_CURRENT_ANGLE = -998;

	private bool _isStartedUp;

	public override void RegisterEvents()
	{
		MyObject.OnUnregisterEvents += OnUnregisterEvents;
		MyObject.OnTimeLineChanged += OnTimeLineChanged;
		MyObject.OnPathChanged += OnChangePath;
		if (!_isStartedUp && MyObject.MyPath != null)
		{
			Startup();
		}
	}

	private void OnUnregisterEvents()
	{
		if (MyObject.SpeedKeyValue != null)
		{
			MyObject.SpeedKeyValue.OnEndValueReached -= OnSpeedReached;
		}
		if (MyObject.AngleKeyValue != null)
		{
			MyObject.AngleKeyValue.OnEndValueReached -= OnAngleReached;
		}
	}

	public override void Init()
	{
		base.Init();
	}

	private void OnUpdate(float elapsedTime)
	{
		if (!_isStartedUp)
		{
			Startup();
			MyObject.OnUpdate -= OnUpdate;
		}
	}

	private void OnChangePath()
	{
		Startup();
		_isStartedUp = true;
	}

	private void Startup()
	{
		if (MyObject.SpeedKeyValue == null)
		{
			MyObject.SpeedKeyValue = new KeyFrameValue(MyObject);
		}
		MyObject.SpeedKeyValue.CurrentValue = MyObject.PathSpeed;
		MyObject.SpeedKeyValue.SetupByNextEvent(_SPEED_EVENT_NAME);
		if (!MyObject.SpeedKeyValue.NextValueFounded)
		{
			MyObject.SpeedKeyValue.EndValue = MyObject.SpeedKeyValue.StartValue;
		}
		HackPathZeroSpeed();
		MyObject.SpeedKeyValue.OnEndValueReached += OnSpeedReached;
		if (MyObject.AngleKeyValue == null)
		{
			MyObject.AngleKeyValue = new KeyFrameValue(MyObject);
		}
		MyObject.AngleKeyValue.CurrentValue = Utils.RadToDeg(MyObject.Rotation + MyObject.PartData.PathFront);
		MyObject.AngleKeyValue.SetupByNextEvent(_ANGLE_EVENT_NAME);
		BehaviorEvent pathEventForTime = MyObject.GetPathEventForTime(0f, _ANGLE_EVENT_NAME);
		if (!GameCore.IsEditorEnabled)
		{
			if (pathEventForTime != null)
			{
				MyObject.AngleKeyValue.CurrentValue = pathEventForTime.ValueInt;
			}
			MyObject.AngleKeyValue.StartValue = MyObject.AngleKeyValue.CurrentValue;
		}
		if (pathEventForTime != null || MyObject.AngleKeyValue.NextValueFounded)
		{
			MyObject.PathFollowMode = MyObject.PartData.PathFollowMode;
			HackPathAngle();
			MyObject.AngleKeyValue.OnEndValueReached += OnAngleReached;
		}
		else
		{
			MyObject.AngleKeyValue = null;
		}
		_isStartedUp = true;
	}

	private void OnSpeedReached()
	{
		if (MyObject.SpeedKeyValue.TimeSource == TimeSourceEnum.ObjectTime)
		{
			MyObject.SpeedKeyValue.SetupByNextEvent(_SPEED_EVENT_NAME);
			HackPathZeroSpeed();
		}
	}

	private void OnAngleReached()
	{
		if (MyObject.AngleKeyValue.TimeSource == TimeSourceEnum.ObjectTime)
		{
			MyObject.AngleKeyValue.SetupByNextEvent(_ANGLE_EVENT_NAME);
			HackPathAngle();
		}
	}

	private void HackPathZeroSpeed()
	{
		if (MyObject.SpeedKeyValue != null && MyObject.SpeedKeyValue.TimeSource == TimeSourceEnum.PathTime)
		{
			float startValue = MyObject.SpeedKeyValue.StartValue;
			if (MyObject.SpeedKeyValue.StartValue == 0f && MyObject.SpeedKeyValue.EndValue != 0f)
			{
				startValue = 20f;
			}
			if (MyObject.SpeedKeyValue.EndValue == 0f && MyObject.SpeedKeyValue.StartValue != 0f)
			{
				MyObject.SpeedKeyValue.EndValue = 20f;
			}
			MyObject.SpeedKeyValue.StartValue = startValue;
		}
	}

	private void HackPathAngle()
	{
		if (MyObject.PathFollowMode == PathFollowModeEnum.NoMove)
		{
			return;
		}
		if (MyObject.AngleKeyValue.StartValue == -999f)
		{
			MyObject.PathFollowMode = PathFollowModeEnum.Follow;
			MyObject.AngleKeyValue.StartValue = Utils.RadToDeg(MyObject.MyPath.GetDirection(MyObject.PathTime));
			MyObject.AngleKeyValue.CurrentValue = MyObject.AngleKeyValue.StartValue;
		}
		else if (MyObject.AngleKeyValue.StartValue == -998f)
		{
			MyObject.PathFollowMode = PathFollowModeEnum.FreeRotation;
			MyObject.AngleKeyValue.StartValue = Utils.RadToDeg(MyObject.Rotation);
			MyObject.AngleKeyValue.CurrentValue = MyObject.AngleKeyValue.StartValue;
		}
		else if (!GameCore.IsEditorEnabled)
		{
			MyObject.PathFollowMode = PathFollowModeEnum.FreeRotation;
		}
		if (MyObject.AngleKeyValue.EndValue == -999f)
		{
			if (MyObject.AngleKeyValue.IsEndTimeOnPath)
			{
				MyObject.AngleKeyValue.EndValue = Utils.RadToDeg(MyObject.MyPath.GetDirection(MyObject.AngleKeyValue.EndTime));
			}
			else
			{
				MyObject.AngleKeyValue.EndValue = Utils.RadToDeg(MyObject.MyPath.GetDirection(MyObject.PathTime));
			}
		}
		else if (MyObject.AngleKeyValue.EndValue == -998f)
		{
			MyObject.AngleKeyValue.EndValue = Utils.RadToDeg(MyObject.Rotation);
		}
		else if (!GameCore.IsEditorEnabled)
		{
			MyObject.PathFollowMode = PathFollowModeEnum.FreeRotation;
		}
	}

	private void OnTimeLineChanged()
	{
		if (MyObject.CurrentTimeLine != null)
		{
			if (MyObject.SpeedKeyValue != null)
			{
				MyObject.SpeedKeyValue.SetupByNextEvent(_SPEED_EVENT_NAME);
			}
			if (MyObject.AngleKeyValue != null)
			{
				MyObject.AngleKeyValue.SetupByNextEvent(_ANGLE_EVENT_NAME);
			}
		}
	}

	public override void DoEvent(BehaviorEvent pathEvent, BehaviorTimeLine sourceTimeLine)
	{
		if (MyObject.SpeedKeyValue != null && Utils.Equals(pathEvent.Name, _SPEED_EVENT_NAME))
		{
			MyObject.SpeedKeyValue.SetupByEvent(pathEvent);
			HackPathZeroSpeed();
		}
		if (MyObject.AngleKeyValue != null && Utils.Equals(pathEvent.Name, _ANGLE_EVENT_NAME))
		{
			MyObject.AngleKeyValue.SetupByEvent(pathEvent);
			HackPathAngle();
		}
	}
}
