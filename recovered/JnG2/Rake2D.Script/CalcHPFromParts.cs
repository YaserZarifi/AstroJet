using System.Collections.Generic;

namespace Rake2D.Script;

public class CalcHPFromParts : BehaviorScript
{
	private string[] _partIdArray;

	private List<GameObject> _partList;

	private int _rootMode;

	public override void RegisterEvents()
	{
		GameObject gameObject = ((_rootMode != 0) ? MyObject.GetMainPart() : MyObject);
		if (_partList == null)
		{
			_partList = GameCore.Allocator.GetGameObjectList();
			string[] partIdArray = _partIdArray;
			foreach (string childId in partIdArray)
			{
				GameObject childByIdDeep = gameObject.GetChildByIdDeep(childId);
				if (childByIdDeep != null && childByIdDeep.IsAlive && childByIdDeep.MaxHP > 0f)
				{
					_partList.Add(childByIdDeep);
				}
			}
		}
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		_partIdArray = ParamStringArray("part_id");
		_rootMode = ParamInt("root_mode", 1);
	}

	private void OnUpdate(float elapsedTime)
	{
		float maxHP = MyObject.MaxHP;
		MyObject.MaxHP = 0f;
		MyObject.CurrentHP = 0f;
		foreach (GameObject part in _partList)
		{
			MyObject.MaxHP += part.MaxHP;
			MyObject.CurrentHP += part.CurrentHP;
		}
		if (maxHP > MyObject.MaxHP)
		{
			MyObject.MaxHP = maxHP;
		}
	}
}
