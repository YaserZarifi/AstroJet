using System.Collections.Generic;
using System.Linq;

namespace Rake2D.Script.Boss;

public class BeerCoreRingShuffle : BehaviorScript
{
	private Dictionary<string, GameObject> _stateDisctionary;

	public override void Init()
	{
		base.Init();
		_stateDisctionary = new Dictionary<string, GameObject>();
		GameObject childByIdDeep = MyObject.GetChildByIdDeep("node1");
		if (childByIdDeep != null)
		{
			_stateDisctionary.Add("idle1", childByIdDeep);
		}
		for (int i = 2; i < 7; i++)
		{
			childByIdDeep = MyObject.GetChildByIdDeep($"node{i}");
			if (childByIdDeep != null)
			{
				_stateDisctionary.Add($"start{i}", childByIdDeep);
			}
		}
	}

	public override void RegisterEvents()
	{
		if (_stateDisctionary.Count > 0)
		{
			_stateDisctionary = _stateDisctionary.Where((KeyValuePair<string, GameObject> pair) => pair.Value.IsAlive).ToDictionary((KeyValuePair<string, GameObject> pair) => pair.Key, (KeyValuePair<string, GameObject> pair) => pair.Value);
			if (_stateDisctionary.Count > 0)
			{
				MyObject.ChangePartState(_stateDisctionary.ElementAt(Utils.GetRandom(0, _stateDisctionary.Count - 1)).Key);
			}
		}
		else
		{
			MyObject.StopSound(bLoopedOnly: true);
		}
	}
}
