using System.Collections.Generic;

namespace Rake2D.Script;

public class OnSpineEventSendLinkToPart : OnSpineEventBase
{
	private string _linkOut;

	private int _linkValue;

	private List<GameObject> _linkedPartList;

	public override void Init()
	{
		base.Init();
		_linkOut = ParamString("link_out");
		_linkValue = ParamInt("link_value");
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

	protected override void DoAction()
	{
		MyObject.SendLinkToAll(MyObject, _linkedPartList, _linkOut, _linkValue, bApplyToChildren: true);
	}
}
