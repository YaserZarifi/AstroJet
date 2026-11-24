using System.Collections.Generic;

namespace Rake2D.Script;

public class OnKillDestroyPart : BehaviorScript
{
	protected bool _bExitStateToAll;

	public override void Init()
	{
		base.Init();
	}

	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		List<GameObject> exitPartList = ExitStateBehavior.GetExitPartList(ParamStringArray("exit_all"), MyObject, out _bExitStateToAll, ParamInt("root_mode", 1));
		if (_bExitStateToAll)
		{
			MyObject.GetMainPart().Destroy();
			return;
		}
		if (exitPartList == null)
		{
			MyObject.Destroy();
			return;
		}
		foreach (GameObject item in exitPartList)
		{
			item.Destroy();
		}
	}
}
