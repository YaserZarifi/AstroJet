using System;
using System.Collections.Generic;

namespace Rake2D.Script;

public abstract class HeroDroneDeployer : GemableItemBehavior
{
	protected int _maxCount = 1;

	protected float _deployTimer;

	protected float _collisionDPS;

	protected List<GameObject> _droneList;

	public HeroDroneDeployer()
	{
		_droneList = GameCore.Allocator.GetGameObjectList();
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_collisionDPS = Param("collision_dps");
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		_deployTimer = 0.1f;
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		_droneList.Clear();
	}

	protected virtual GameObject Deploy()
	{
		return null;
	}

	protected void SendLinkToAllDrones(string linkName, int linkValue)
	{
		foreach (GameObject drone in _droneList)
		{
			try
			{
				drone.DoOnLink(MyObject, linkName, linkValue, bApplyChildren: false);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("DroneDeployer: OnLink script exception: {0} in object '{1}'", ex, drone.Id);
			}
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		for (int num = _droneList.Count - 1; num >= 0; num--)
		{
			if (_droneList[num].IsDestroyed)
			{
				_droneList.RemoveAt(num);
			}
		}
		if (GameCore.IsMainMenuEnabled || !(MyHero.MyObject.Position.X + (float)MyHero.MyObject.VisibilityRect.Left >= MyHero.MyObject.MyCamera.Position.X - 1f))
		{
			return;
		}
		if (_droneList.Count < _maxCount && _deployTimer <= 0f)
		{
			_deployTimer = 1f;
		}
		else
		{
			if (!Utils.ChangeValueToTarget(ref _deployTimer, 0f, elapsedTime))
			{
				return;
			}
			while (_droneList.Count < _maxCount)
			{
				GameObject gameObject = Deploy();
				if (gameObject != null)
				{
					gameObject.ParentObject = null;
					gameObject.Damage = GetFinalCollisionDPS();
					gameObject.Activate();
					OnObjectReregisterEvents(gameObject);
					gameObject.OnReRegisterEvents += OnObjectReregisterEvents;
					_droneList.Add(gameObject);
					MyHero.RegisterDrone(gameObject);
					continue;
				}
				break;
			}
		}
	}

	protected void OnObjectReregisterEvents(GameObject go)
	{
	}

	public virtual float GetFinalCollisionDPS()
	{
		if (MyHero != null)
		{
			return _collisionDPS * MyHero.DroneCollisionDamageFactor;
		}
		return _collisionDPS;
	}

	public override bool TryGetParamSummary(string name, out float value, out float finalValue)
	{
		if (Utils.Equals(name, "collision_dps"))
		{
			value = _collisionDPS * (float)_maxCount;
			finalValue = GetFinalCollisionDPS() * (float)_maxCount;
			return true;
		}
		return base.TryGetParamSummary(name, out value, out finalValue);
	}

	protected abstract void SetOffset(BehaviorDataDrone droneData);
}
