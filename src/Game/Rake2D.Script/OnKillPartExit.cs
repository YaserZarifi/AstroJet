using System.Collections.Generic;

namespace Rake2D.Script;

public class OnKillPartExit : ExitStateBehavior
{
	private List<GameObject> _killledPartList;

	public override void Init()
	{
		base.Init();
		string[] idArray = ParamStringArray("part_id");
		_killledPartList = ExitStateBehavior.GetExitPartList(idArray, MyObject, out var _, _rootMode);
	}

	public override void RegisterEvents()
	{
		if (_killledPartList != null)
		{
			foreach (GameObject killledPart in _killledPartList)
			{
				killledPart.OnKill -= OnKill;
			}
		}
		MyObject.OnUnregisterEvents += OnUnregisterEvents;
		MyObject.OnUpdateBegin += OnUpdateBegin;
	}

	private void OnUnregisterEvents()
	{
		if (_killledPartList == null)
		{
			return;
		}
		foreach (GameObject killledPart in _killledPartList)
		{
			killledPart.OnKill -= OnKill;
		}
	}

	private void OnUpdateBegin(float elapsedTime)
	{
		MyObject.OnUpdateBegin -= OnUpdateBegin;
		if (_killledPartList == null)
		{
			return;
		}
		foreach (GameObject killledPart in _killledPartList)
		{
			killledPart.OnKill += OnKill;
		}
	}

	private void OnKill()
	{
		StartExit();
	}
}
