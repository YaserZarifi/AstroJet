using System;
using Rake2D.Script;

namespace Rake2D;

public class KeyFrameValue
{
	public delegate void OnEventHandler();

	public TimeSourceEnum TimeSource;

	public float StartValue;

	public float EndValue;

	public float StartTime;

	public float EndTime;

	public float CurrentValue;

	public GameObject MyObject;

	public bool NextValueFounded;

	public bool IsEndTimeOnPath;

	private bool _endReached;

	public event OnEventHandler OnEndValueReached;

	public void SetAll(TimeSourceEnum timeSource, float startValue, float startTime, float endValue, float endTime)
	{
		TimeSource = timeSource;
		StartValue = startValue;
		StartTime = startTime;
		EndValue = endValue;
		EndTime = endTime;
		CurrentValue = startValue;
	}

	public KeyFrameValue(GameObject go)
	{
		MyObject = go;
		TimeSource = TimeSourceEnum.Undefind;
	}

	private float GetTimeBySource()
	{
		GameObject mainPart = MyObject.GetMainPart();
		switch (TimeSource)
		{
		case TimeSourceEnum.ObjectTime:
			return MyObject.Timer;
		case TimeSourceEnum.PathTime:
			if (mainPart.MyPath != null)
			{
				return mainPart.PathTime;
			}
			return 0f;
		case TimeSourceEnum.TimeLineTime:
			if (mainPart.CurrentTimeLine != null)
			{
				return mainPart.TimeLineTime;
			}
			return 0f;
		default:
			return 0f;
		}
	}

	public void SetStartTimeToCurrentTime()
	{
		StartTime = GetTimeBySource();
		if (EndTime < StartTime)
		{
			EndTime = StartTime;
		}
	}

	public void SetupByNextEvent(string eventName)
	{
		StartValue = CurrentValue;
		GameObject mainPart = MyObject.GetMainPart();
		BehaviorEvent behaviorEvent = MyObject.FindNextEventFromCurrentTime(eventName);
		if (behaviorEvent != null)
		{
			EndTime = behaviorEvent.Time;
			EndValue = behaviorEvent.ValueInt;
			if (behaviorEvent.EventNode.IsOnTimeLine)
			{
				StartTime = mainPart.TimeLineTime;
				TimeSource = TimeSourceEnum.TimeLineTime;
				IsEndTimeOnPath = false;
			}
			else
			{
				StartTime = mainPart.PathTime;
				TimeSource = TimeSourceEnum.PathTime;
				IsEndTimeOnPath = true;
			}
			NextValueFounded = true;
		}
		else
		{
			NextValueFounded = false;
		}
	}

	public void SetupByEvent(BehaviorEvent pathEvent)
	{
		GameObject mainPart = MyObject.GetMainPart();
		BehaviorEvent behaviorEvent = MyObject.FindNextEventFromCurrentTime(pathEvent.Name);
		BehaviorTimeLine currentTimeLine = mainPart.CurrentTimeLine;
		StartValue = pathEvent.ValueInt;
		CurrentValue = StartValue;
		if (behaviorEvent == null)
		{
			TimeSource = ((currentTimeLine != null) ? TimeSourceEnum.TimeLineTime : TimeSourceEnum.PathTime);
			StartTime = ((currentTimeLine != null) ? mainPart.TimeLineTime : mainPart.PathTime);
			EndValue = StartValue;
			EndTime = StartTime;
			IsEndTimeOnPath = false;
		}
		else if (currentTimeLine != null && pathEvent.EventNode.IsOnTimeLine)
		{
			if (behaviorEvent.EventNode.IsOnTimeLine)
			{
				TimeSource = TimeSourceEnum.TimeLineTime;
				StartTime = mainPart.TimeLineTime;
				EndTime = behaviorEvent.Time;
				EndValue = behaviorEvent.ValueInt;
				IsEndTimeOnPath = false;
			}
			else
			{
				TimeSource = TimeSourceEnum.PathTime;
				StartTime = mainPart.PathTime;
				EndTime = behaviorEvent.Time;
				EndValue = behaviorEvent.ValueInt;
				IsEndTimeOnPath = true;
			}
		}
		else if (behaviorEvent.EventNode.IsOnTimeLine)
		{
			TimeSource = TimeSourceEnum.TimeLineTime;
			StartTime = mainPart.TimeLineTime;
			EndTime = behaviorEvent.Time;
			EndValue = behaviorEvent.ValueInt;
			IsEndTimeOnPath = false;
		}
		else
		{
			TimeSource = TimeSourceEnum.PathTime;
			StartTime = mainPart.PathTime;
			EndTime = behaviorEvent.Time;
			EndValue = behaviorEvent.ValueInt;
			IsEndTimeOnPath = true;
		}
	}

	public void UpdateCurrentValue()
	{
		float timeBySource = GetTimeBySource();
		if (timeBySource >= EndTime)
		{
			CurrentValue = EndValue;
		}
		else if (EndTime <= StartTime || timeBySource <= StartTime || EndValue == StartValue)
		{
			CurrentValue = StartValue;
		}
		else
		{
			float num = (timeBySource - StartTime) / (EndTime - StartTime);
			CurrentValue = StartValue + (EndValue - StartValue) * num;
		}
		if (CurrentValue == EndValue && TimeSource != TimeSourceEnum.Undefined)
		{
			if (!_endReached)
			{
				try
				{
					OnEndValueReached?.Invoke();
				}
				catch (Exception ex)
				{
					GameCore.LogErr("Key Value end reached exception: {0}", ex);
				}
				_endReached = true;
			}
		}
		else
		{
			_endReached = false;
		}
	}
}
