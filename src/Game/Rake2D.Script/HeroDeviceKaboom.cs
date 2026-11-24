using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroDeviceKaboom : ItemBehaviorScript
{
	private float _mana;

	private float _radius;

	private float _physForce;

	private float _totalDPSFactor;

	private float _exploTimer;

	private PartData _exploData;

	private PartData _hitEffectData;

	private TargetClass _targetClass = (TargetClass)73976;

	private List<GameObject> _droneList;

	private bool _isActivated;

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
		_isActivated = false;
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_mana = Param("mana", 100f);
		_radius = Param("radius");
		_totalDPSFactor = Param("total_dps_percent", 100f) / 100f;
		_exploData = GameCore.GetPartData("explo_item_device_kaboom");
		_hitEffectData = GameCore.GetPartData("hit_item_device_kaboom");
		_physForce = Param("force");
	}

	private void OnActivated()
	{
		if (!_isActivated)
		{
			MyObject.OnUpdate += OnUpdate;
			_droneList = GameCore.Allocator.CloneGameObjectList(MyHero.DroneList);
			MyHero.MyObject.MyLayer.CreateObjectByPartData(_exploData, MyHero.MyObject.WorldPosition, Vector2.One, 0f, MyHero.MyObject.ZOrder);
			MyHero.MyObject.MyLevel.ShockHit(MyHero.MyObject, MyHero.MyObject.WorldPosition, _radius, _targetClass, DamageTypeEnum.Explosion, MyHero.GetTotalDPS() * _totalDPSFactor, _physForce, null, onlyHitable: true, wantResults: false);
			_exploTimer = 0.1f;
			MyHero.UseMana(_mana);
			_isActivated = true;
		}
	}

	private void OnUpdate(float elepsedTime)
	{
		for (int num = _droneList.Count - 1; num >= 0; num--)
		{
			if (_droneList[num].IsDestroyed)
			{
				_droneList.RemoveAt(num);
			}
		}
		if (_droneList.Count <= 0)
		{
			MyObject.OnUpdate -= OnUpdate;
			_isActivated = false;
		}
		else if (Utils.ChangeValueToTarget(ref _exploTimer, 0f, elepsedTime))
		{
			_exploTimer = 0.05f;
			int index = 0;
			GameObject gameObject = _droneList[index];
			GameObject gameObject2 = gameObject.MyLayer.CreateObjectByPartData(_exploData, gameObject.WorldPosition, Vector2.One, 0f, gameObject.ZOrder);
			gameObject2.SetVelocity(gameObject2.MyLayer.ScrollSpeed);
			gameObject.MyLevel.ShockHit(MyHero.MyObject, gameObject.WorldPosition, _radius, _targetClass, DamageTypeEnum.Explosion, MyHero.GetTotalDPS() * _totalDPSFactor, _physForce, null, onlyHitable: true, wantResults: false);
			gameObject.Destroy();
		}
	}
}
