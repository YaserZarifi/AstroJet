using System.Collections.Generic;

namespace Rake2D.Script;

public class SendLinkToPart : BehaviorScript
{
	private string _eventName;

	private int _eventValue;

	private int _partRootMode;

	private List<GameObject> _linkedPartList;

	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		_eventName = ParamString("link_out");
		_eventValue = ParamInt("link_value");
		_partRootMode = ParamInt("part_root_mode", 1);
		string[] array = ParamStringArray("part_id");
		_linkedPartList = new List<GameObject>();
		if (array == null)
		{
			return;
		}
		string[] array2 = array;
		foreach (string text in array2)
		{
			GameObject gameObject;
			if (_partRootMode == -1)
			{
				gameObject = MyObject.GetChildByIdFlat(text);
			}
			else
			{
				gameObject = ((_partRootMode != 0) ? MyObject.GetMainPart().GetChildByIdDeep(text) : MyObject.GetChildByIdDeep(text));
			}
			if (gameObject != null)
			{
				_linkedPartList.Add(gameObject);
				continue;
			}
			LogErr("Part {0} not found", text);
		}
	}

	public void DoAction()
	{
		MyObject.SendLinkToAll(MyObject, _linkedPartList, _eventName, _eventValue, bApplyToChildren: true);
	}
}
