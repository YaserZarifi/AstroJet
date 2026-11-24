using System.Collections.Generic;

namespace Rake2D.Script;

public class OnKillAllExitContinous : ExitStateBehavior
{
	private List<GameObject> _partList;

	private string[] _partIdArray;

	private int _partRootMode;

	private bool _exitDone;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		_partList = GameCore.Allocator.GetGameObjectList();
		string[] partIdArray = _partIdArray;
		foreach (string childId in partIdArray)
		{
			GameObject gameObject = ((_partRootMode != 0) ? MyObject.GetMainPart().GetChildByIdDeep(childId) : MyObject.GetChildByIdDeep(childId));
			if (gameObject != null && gameObject.IsAlive)
			{
				_partList.Add(gameObject);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		_partIdArray = ParamStringArray("part_id");
		_partRootMode = ParamInt("part_root_mode", 1);
		_exitDone = ParamBool("exit_done");
	}

	private void OnUpdate(float elapsedTime)
	{
		for (int num = _partList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = _partList[num];
			if (gameObject.IsKilled || gameObject.IsDestroyed)
			{
				_partList.RemoveAt(num);
			}
		}
		if (_partList.Count == 0)
		{
			StartExit();
			MyObject.OnUpdate -= OnUpdate;
			IsDone = _exitDone;
		}
	}
}
