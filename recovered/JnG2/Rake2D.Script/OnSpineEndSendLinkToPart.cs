using System.Collections.Generic;

namespace Rake2D.Script;

public class OnSpineEndSendLinkToPart : OnSpineEndScript
{
	private string _eventName;

	private int _eventValue;

	private List<GameObject> _linkedPartList;

	public override void Init()
	{
		base.Init();
		_eventName = ParamString("link_out");
		_eventValue = ParamInt("link_value");
		string[] array = ParamStringArray("part_id");
		_linkedPartList = new List<GameObject>();
		if (array == null)
		{
			return;
		}
		string[] array2 = array;
		foreach (string childId in array2)
		{
			GameObject childByIdDeep = MyObject.GetMainPart().GetChildByIdDeep(childId);
			if (childByIdDeep != null)
			{
				_linkedPartList.Add(childByIdDeep);
			}
		}
	}

	public override void DoAction()
	{
		base.DoAction();
		MyObject.SendLinkToAll(MyObject, _linkedPartList, _eventName, _eventValue, bApplyToChildren: true);
	}
}
