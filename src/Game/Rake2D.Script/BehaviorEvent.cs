namespace Rake2D.Script;

public class BehaviorEvent
{
	public BehaviorEventNode EventNode;

	public GameObject MyObject;

	public BehaviorScript Behavior;

	public string Name;

	public string ChildId;

	public string BehaviorId;

	public int ValueInt;

	public string ValueString;

	public float Time => EventNode.Time;

	public BehaviorEvent(string childId, string behaviorId, string name, int value)
	{
		MyObject = null;
		ChildId = childId;
		Name = name;
		ValueInt = value;
		BehaviorId = behaviorId;
	}

	public void SetObject(GameObject go)
	{
		if (ChildId == null)
		{
			MyObject = go;
		}
		else
		{
			MyObject = go.GetChildByIdDeep(ChildId);
		}
		if (MyObject != null && BehaviorId != null)
		{
			Behavior = MyObject.GetBehaviorScriptById(BehaviorId);
		}
	}

	public override string ToString()
	{
		return Name;
	}
}
