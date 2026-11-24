namespace Rake2D.Script;

public class BehaviorKeyValue
{
	public bool IsEvaluated;

	public bool IsTargetOnTimeLine;

	public bool IsTargetValueFounded;

	public string EventName;

	public float StartValue;

	public float StartTime;

	public float TargetValue;

	public float TargetTime;

	public BehaviorKeyValue(string eventName)
	{
		EventName = eventName;
	}

	public void InitTargetValueByObject(GameObject go)
	{
		BehaviorEvent behaviorEvent = null;
		BehaviorTimeLine currentTimeLine = go.GetMainPart().CurrentTimeLine;
		if (currentTimeLine != null)
		{
			behaviorEvent = currentTimeLine.GetNextEvent(0f, EventName, go);
		}
		if (behaviorEvent == null)
		{
			behaviorEvent = go.GetNextPathEvent(0f, EventName);
			IsTargetOnTimeLine = false;
		}
		else
		{
			IsTargetOnTimeLine = true;
		}
		if (behaviorEvent != null)
		{
			TargetValue = behaviorEvent.ValueInt;
			TargetTime = behaviorEvent.EventNode.Time;
			IsTargetValueFounded = true;
		}
		else
		{
			TargetValue = StartValue;
			IsTargetValueFounded = false;
		}
	}

	public void UpdateByNextEvent(BehaviorEvent currEvent, BehaviorTimeLine sourceTimeLine)
	{
		BehaviorEvent behaviorEvent = null;
		if (sourceTimeLine != null)
		{
			behaviorEvent = sourceTimeLine.GetNextEvent(currEvent.Time, EventName, currEvent.MyObject);
			if (behaviorEvent == null)
			{
				behaviorEvent = currEvent.MyObject.GetNextPathEvent(currEvent.MyObject.PathTime, EventName);
				IsTargetOnTimeLine = false;
				if (behaviorEvent != null)
				{
					StartTime = currEvent.MyObject.PathTime;
					TryGetValueAtTime(currEvent.MyObject, ref StartValue);
				}
			}
			else
			{
				IsTargetOnTimeLine = true;
			}
		}
		else
		{
			BehaviorTimeLine currentTimeLine = currEvent.MyObject.GetMainPart().CurrentTimeLine;
			if (currentTimeLine != null)
			{
				behaviorEvent = currentTimeLine.GetNextEvent(currEvent.MyObject.GetMainPart().TimeLineTime, EventName, currEvent.MyObject);
			}
			if (behaviorEvent == null)
			{
				behaviorEvent = currEvent.MyObject.GetNextPathEvent(currEvent.Time, EventName);
				IsTargetOnTimeLine = false;
			}
			else
			{
				IsTargetOnTimeLine = true;
				StartTime = currEvent.MyObject.TimeLineTime;
				TryGetValueAtTime(currEvent.MyObject, ref StartValue);
			}
		}
		if (behaviorEvent != null)
		{
			TargetValue = behaviorEvent.ValueInt;
			TargetTime = behaviorEvent.EventNode.Time;
			IsTargetValueFounded = true;
		}
		else
		{
			TargetValue = StartValue;
			IsTargetValueFounded = false;
		}
	}

	public bool OnEvent(BehaviorEvent behaviorEvent, BehaviorTimeLine sourceTimeLine)
	{
		if (Utils.Equals(behaviorEvent.Name, EventName))
		{
			if (sourceTimeLine == null && behaviorEvent.MyObject.CurrentTimeLine != null && IsTargetOnTimeLine && behaviorEvent.MyObject.CurrentTimeLine != behaviorEvent.EventNode.TimeLine)
			{
				if (Options.DebugPath)
				{
					GameCore.LogErr("Path event '{0}' on time {1} is ignored due same next time event is present", behaviorEvent.Name, Utils.FormatFloat2(behaviorEvent.Time));
				}
				behaviorEvent.EventNode.ErrorCount++;
				return false;
			}
			StartTime = behaviorEvent.Time;
			StartValue = behaviorEvent.ValueInt;
			TargetTime = behaviorEvent.Time;
			UpdateByNextEvent(behaviorEvent, sourceTimeLine);
			IsEvaluated = true;
			return true;
		}
		return false;
	}

	public bool TryGetValueAtTime(GameObject go, ref float result)
	{
		if (!IsTargetValueFounded)
		{
			return false;
		}
		float num = (IsTargetOnTimeLine ? go.TimeLineTime : go.PathTime);
		if (num >= TargetTime || StartValue == TargetValue)
		{
			result = TargetValue;
			return true;
		}
		float num2 = TargetTime - StartTime;
		float num3 = num - StartTime;
		float num4 = TargetValue - StartValue;
		result = StartValue + num4 * (num3 / num2);
		return true;
	}
}
