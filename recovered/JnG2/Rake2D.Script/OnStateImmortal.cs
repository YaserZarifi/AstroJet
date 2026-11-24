using System.Collections.Generic;

namespace Rake2D.Script;

public class OnStateImmortal : BehaviorScript
{
	private List<GameObject> _partList;

	private bool _immortality;

	public override void Init()
	{
		base.Init();
		_immortality = ParamBool("immortal", defaultValue: true);
		_partList = ExitStateBehavior.GetExitPartList(ParamStringArray("part"), MyObject, out var _, ParamInt("root_mode", 1));
	}

	public override void RegisterEvents()
	{
		foreach (GameObject part in _partList)
		{
			part.IsImmortal = _immortality;
		}
	}
}
