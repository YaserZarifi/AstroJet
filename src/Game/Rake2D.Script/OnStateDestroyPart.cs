using System.Collections.Generic;

namespace Rake2D.Script;

public class OnStateDestroyPart : BehaviorScript
{
	private string[] _partIdArray;

	private int _rootMode;

	public override void Init()
	{
		base.Init();
		_partIdArray = ParamStringArray("exit_all");
		_rootMode = ParamInt("root_mode", 1);
	}

	public override void RegisterEvents()
	{
		Kill();
	}

	public void Kill()
	{
		List<GameObject> exitPartList = ExitStateBehavior.GetExitPartList(_partIdArray, MyObject, out var bExistStateToAll, _rootMode);
		if (bExistStateToAll)
		{
			MyObject.GetMainPart().Destroy();
		}
		else if (exitPartList == null)
		{
			MyObject.Destroy();
		}
		else
		{
			foreach (GameObject item in exitPartList)
			{
				item.Destroy();
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(exitPartList);
	}
}
