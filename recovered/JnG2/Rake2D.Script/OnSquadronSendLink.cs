namespace Rake2D.Script;

public class OnSquadronSendLink : OnSquadronBehavior
{
	protected string _linkOut;

	public override void Init()
	{
		base.Init();
		_linkOut = ParamString("link_out");
		_ignoreFirst = ParamBool("send_to_first");
	}

	protected override void DoAction()
	{
		if (_ignoreFirst)
		{
			if (!MyObject.LinkedObjectList.IsNullOrEmpty())
			{
				MyObject.LinkedObjectList[0].DoOnLink(MyObject, _linkOut, 0, bApplyChildren: true);
			}
		}
		else
		{
			MyObject.SendLinkToAll(MyObject, _linkOut, 0);
		}
		MyObject.Destroy();
	}
}
