using System.Collections.Generic;

namespace Rake2D.Script;

public class OnSpineEventKillPart : OnSpineEventBase
{
	protected bool _bExitStateToAll;

	protected override void DoAction()
	{
		if (_bExitStateToAll)
		{
			MyObject.GetMainPart().Kill();
			return;
		}
		List<GameObject> exitPartList = ExitStateBehavior.GetExitPartList(ParamStringArray("exit_all"), MyObject, out _bExitStateToAll, ParamInt("root_mode", 1));
		if (exitPartList == null)
		{
			MyObject.Kill();
			return;
		}
		foreach (GameObject item in exitPartList)
		{
			item.Kill();
		}
	}
}
