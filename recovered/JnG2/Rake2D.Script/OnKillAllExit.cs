using System.Collections.Generic;

namespace Rake2D.Script;

public class OnKillAllExit : ExitStateBehavior
{
	private List<GameObject> _partList;

	public override void RegisterEvents()
	{
		if (_partList == null)
		{
			return;
		}
		foreach (GameObject part in _partList)
		{
			part.OnKill -= OnPartKill;
			part.OnKill += OnPartKill;
			part.OnReRegisterEvents += OnPartReRegisterEvents;
		}
	}

	public override void Init()
	{
		base.Init();
		string[] array = ParamStringArray("part_id");
		_partList = GameCore.Allocator.GetGameObjectList();
		string[] array2 = array;
		foreach (string text in array2)
		{
			GameObject childByIdDeep = MyObject.GetMainPart().GetChildByIdDeep(text);
			if (childByIdDeep != null)
			{
				_partList.Add(childByIdDeep);
				continue;
			}
			LogErr("Child part '{0}' not found", text);
		}
	}

	private void OnPartReRegisterEvents(GameObject go)
	{
		go.OnKill += OnPartKill;
	}

	private void OnPartKill()
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
		}
	}
}
