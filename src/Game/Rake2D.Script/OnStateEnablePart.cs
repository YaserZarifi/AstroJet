using System.Collections.Generic;

namespace Rake2D.Script;

public class OnStateEnablePart : ExitStateBehavior
{
	private List<GameObject> _enableList;

	private List<GameObject> _disableList;

	public override void RegisterEvents()
	{
		if (_enableList != null)
		{
			foreach (GameObject enable in _enableList)
			{
				enable.IsEnabled = true;
			}
		}
		if (_disableList == null)
		{
			return;
		}
		foreach (GameObject disable in _disableList)
		{
			disable.IsEnabled = false;
		}
	}

	public override void Init()
	{
		base.Init();
		_enableList = ExitStateBehavior.GetExitPartList(ParamStringArray("enable_part"), MyObject, out var bExistStateToAll, 1);
		_disableList = ExitStateBehavior.GetExitPartList(ParamStringArray("disable_part"), MyObject, out bExistStateToAll, 1);
	}
}
