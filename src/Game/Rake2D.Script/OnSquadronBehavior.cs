using System.Collections.Generic;
using System.Linq;

namespace Rake2D.Script;

public abstract class OnSquadronBehavior : ExitStateBehavior
{
	protected bool _actionReady;

	private bool _checkDestroyed;

	protected bool _ignoreFirst;

	protected float _actionTimer;

	protected int _minCount;

	protected List<GameObject> _squadList;

	public override void Init()
	{
		base.Init();
		_actionTimer = Param("delay") / 1000f;
		_minCount = ParamInt("min_count");
		_checkDestroyed = ParamBool("check_destroyed");
		if (MyObject.LinkedObjectList != null)
		{
			_squadList = GameCore.Allocator.CloneGameObjectList(MyObject.LinkedObjectList);
		}
		else
		{
			_squadList = null;
		}
	}

	public override void RegisterEvents()
	{
		if (_squadList != null)
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	protected virtual void OnUpdate(float elapsedTime)
	{
		if (_squadList != null)
		{
			for (int num = Enumerable.Count(_squadList) - 1; num >= (_ignoreFirst ? 1 : 0); num--)
			{
				GameObject gameObject = _squadList[num];
				if (gameObject.IsKilled || (_checkDestroyed && gameObject.IsDestroyed))
				{
					_squadList.RemoveAt(num);
					if (Enumerable.Count(_squadList) <= _minCount + (_ignoreFirst ? 1 : 0))
					{
						_actionReady = true;
					}
				}
			}
		}
		if (_actionReady && Utils.ChangeValueToTarget(ref _actionTimer, 0f, elapsedTime))
		{
			MyObject.OnUpdate -= OnUpdate;
			DoAction();
		}
	}

	protected virtual void DoAction()
	{
	}
}
