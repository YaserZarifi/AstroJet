using System.Collections.Generic;

namespace Rake2D.Script;

public class OnStateKillPart : BehaviorScript
{
	protected bool _bExitStateToAll;

	protected List<GameObject> _exitPartListAll;

	public override void Init()
	{
		base.Init();
		_exitPartListAll = ExitStateBehavior.GetExitPartList(ParamStringArray("exit_all"), MyObject, out _bExitStateToAll, ParamInt("root_mode", 1));
	}

	public override void RegisterEvents()
	{
		Kill();
	}

	public void Kill()
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
