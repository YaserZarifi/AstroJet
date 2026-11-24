using System.Collections.Generic;

namespace Rake2D.Script;

public class OnHitExit : OnHitBehavior
{
	protected bool _bExitStateToAll;

	protected List<GameObject> _exitPartListAll;

	public override void Init()
	{
		base.Init();
		_exitPartListAll = ExitStateBehavior.GetExitPartList(ParamStringArray("exit_all"), MyObject, out _bExitStateToAll, ParamInt("root_mode", 1));
	}

	protected override void DoAction(GameObject hittingObject)
	{
		string text = ParamRandomString("exit_state", null);
		if (text != null)
		{
			if (_bExitStateToAll)
			{
				MyObject.GetMainPart().ChangePartStateToAll(text);
				return;
			}
			if (_exitPartListAll != null)
			{
				foreach (GameObject item in _exitPartListAll)
				{
					item.ChangePartState(text);
				}
				return;
			}
			MyObject.ChangePartState(text);
		}
		else
		{
			LogErr("Undefined $exit_state");
		}
	}
}
