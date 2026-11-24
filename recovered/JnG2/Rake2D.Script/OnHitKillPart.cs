using System.Collections.Generic;

namespace Rake2D.Script;

public class OnHitKillPart : OnHitBehavior
{
	protected bool _bExitStateToAll;

	protected List<GameObject> _exitPartListAll;

	public override void Init()
	{
		base.Init();
		_exitPartListAll = ExitStateBehavior.GetExitPartList(ParamStringArray("exit_all"), MyObject, out _bExitStateToAll, ParamInt("main_mode", 1));
	}

	protected override void DoAction(GameObject hittingObject)
	{
		if (_bExitStateToAll)
		{
			MyObject.GetMainPart().Kill();
			return;
		}
		if (_exitPartListAll == null)
		{
			MyObject.Kill();
			return;
		}
		foreach (GameObject item in _exitPartListAll)
		{
			item.Kill();
		}
	}
}
