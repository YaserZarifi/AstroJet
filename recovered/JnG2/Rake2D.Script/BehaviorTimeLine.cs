using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class BehaviorTimeLine
{
	public List<BehaviorEventNode> EventNodeList;

	public GameObject MyObject;

	public Vector2 ScreenPosition;

	public float CursorTime = -1f;

	public BehaviorTimeLine Clone(GameObject clonedObject, long linkedPkOffset = -1L)
	{
		BehaviorTimeLine behaviorTimeLine = new BehaviorTimeLine();
		behaviorTimeLine.MyObject = clonedObject;
		if (EventNodeList != null)
		{
			foreach (BehaviorEventNode eventNode in EventNodeList)
			{
				BehaviorEventNode behaviorEventNode = behaviorTimeLine.AddEventNode(eventNode.Time);
				foreach (BehaviorEvent @event in eventNode.EventList)
				{
					behaviorEventNode.AddEvent(@event.ChildId, @event.BehaviorId, @event.Name, @event.ValueInt).ValueString = @event.ValueString;
				}
				behaviorEventNode.SetObject(clonedObject);
				behaviorEventNode.IsOnTimeLine = true;
				behaviorEventNode.DifficultyRange = new IntRange(behaviorEventNode.DifficultyRange.From, behaviorEventNode.DifficultyRange.To);
				if (eventNode.LinkedPkList == null || linkedPkOffset < 0)
				{
					continue;
				}
				behaviorEventNode.LinkedPkList = new List<long>();
				foreach (long linkedPk in eventNode.LinkedPkList)
				{
					int num = (int)linkedPk;
					behaviorEventNode.LinkedPkList.Add(num + linkedPkOffset);
				}
			}
		}
		return behaviorTimeLine;
	}

	public void BuildLinkedList(Dictionary<long, GameObject> pkDictionary)
	{
		if (EventNodeList == null)
		{
			return;
		}
		foreach (BehaviorEventNode eventNode in EventNodeList)
		{
			eventNode.BuildLinkedList(pkDictionary);
		}
	}

	public void UnlinkDestroyedObjects()
	{
		if (EventNodeList == null)
		{
			return;
		}
		foreach (BehaviorEventNode eventNode in EventNodeList)
		{
			if (eventNode.LinkedObjectList != null)
			{
				eventNode.LinkedObjectList.RemoveAll((GameObject o) => o.IsDestroyed);
			}
		}
	}

	public float GetMaxTime()
	{
		return EventNodeList.Last().Time;
	}

	public BehaviorEventNode AddEventNode(float time)
	{
		if (EventNodeList == null)
		{
			EventNodeList = new List<BehaviorEventNode>();
		}
		BehaviorEventNode behaviorEventNode = new BehaviorEventNode(time);
		behaviorEventNode.IsOnTimeLine = true;
		EventNodeList.Add(behaviorEventNode);
		ReorderNodes();
		return behaviorEventNode;
	}

	public void RemoveEventNode(BehaviorEventNode node)
	{
		if (EventNodeList != null && EventNodeList.Contains(node))
		{
			EventNodeList.Remove(node);
			if (EventNodeList.Count <= 0)
			{
				EventNodeList = null;
			}
			else
			{
				ReorderNodes();
			}
		}
	}

	public BehaviorEvent GetNextEvent(float time, string eventName, GameObject forObject)
	{
		if (EventNodeList != null)
		{
			foreach (BehaviorEventNode item in EventNodeList.FindAll((BehaviorEventNode o) => o.Time > time && o.EventList.Count > 0))
			{
				BehaviorEvent behaviorEvent = item.EventList.Find((BehaviorEvent o) => o.MyObject == forObject && Utils.Equals(o.Name, eventName));
				if (behaviorEvent != null)
				{
					return behaviorEvent;
				}
			}
		}
		return null;
	}

	public void ReorderNodes()
	{
		EventNodeList = EventNodeList?.OrderBy((BehaviorEventNode n) => n.Time).ToList();
	}
}
