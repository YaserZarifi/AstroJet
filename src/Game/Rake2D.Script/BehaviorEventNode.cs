using System.Collections.Generic;

namespace Rake2D.Script;

public class BehaviorEventNode
{
	public float Time;

	public bool IsDestroyd;

	public int ErrorCount;

	public List<BehaviorEvent> EventList;

	public BehaviorTimeLine TimeLine;

	public bool IsOnTimeLine;

	public List<GameObject> LinkedObjectList;

	public List<long> LinkedPkList;

	public IntRange DifficultyRange;

	public GameObject MyObject { get; private set; }

	public BehaviorEventNode(float time)
	{
		Time = time;
		EventList = new List<BehaviorEvent>();
		DifficultyRange = new IntRange(0, 999);
	}

	public void SetObject(GameObject go)
	{
		MyObject = go;
		EventList.ForEach((BehaviorEvent e) =>
		{
			e.SetObject(go);
		});
	}

	public BehaviorEvent AddEvent(string childId, string behaviorId, string name, int value)
	{
		EventList.RemoveAll((BehaviorEvent e) => Utils.Equals(e.ChildId, childId) && Utils.Equals(e.BehaviorId, behaviorId) && Utils.Equals(e.Name, name));
		BehaviorEvent behaviorEvent = new BehaviorEvent(childId, behaviorId, name, value);
		behaviorEvent.EventNode = this;
		EventList.Add(behaviorEvent);
		return behaviorEvent;
	}

	public void AddLinkedbject(GameObject go)
	{
		if (LinkedObjectList == null)
		{
			LinkedObjectList = GameCore.Allocator.GetGameObjectList();
		}
		if (!LinkedObjectList.Contains(go))
		{
			LinkedObjectList.Add(go);
		}
	}

	public void RemoveLinkedbject(GameObject go)
	{
		if (LinkedObjectList != null)
		{
			LinkedObjectList.Remove(go);
			if (LinkedObjectList.Count == 0)
			{
				LinkedObjectList = GameCore.Allocator.DeleteAliasingGameObjectList(LinkedObjectList);
			}
		}
	}

	public void BuildLinkedList(Dictionary<long, GameObject> pkDictionary)
	{
		if (LinkedPkList == null)
		{
			return;
		}
		foreach (long linkedPk in LinkedPkList)
		{
			GameObject value = null;
			pkDictionary.TryGetValue(linkedPk, out value);
			if (value != null)
			{
				AddLinkedbject(value);
			}
		}
	}

	public void Execute(BehaviorTimeLine sourceTimeLine)
	{
		if (TimeLine != null)
		{
			MyObject.SetCurrentTimeLine(TimeLine);
		}
		foreach (BehaviorEvent @event in EventList)
		{
			if (@event.MyObject != null && @event.MyObject.IsUpdatable && @event.Behavior != null && @event.Behavior.IsForState(@event.MyObject.CurrentState))
			{
				@event.Behavior.DoEvent(@event, sourceTimeLine);
			}
		}
	}
}
