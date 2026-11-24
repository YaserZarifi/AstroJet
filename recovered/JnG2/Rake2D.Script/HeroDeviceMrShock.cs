using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

internal class HeroDeviceMrShock : ManaDeviceBehaviorScript
{
	private PartData _effectData;

	private float _totalDamageFactor;

	private float _hitTimer;

	private float _damage;

	private int _phase;

	private List<Vector2> _destroyedList;

	public override void Init()
	{
		base.Init();
		_effectData = GameCore.GetPartData("hero_mr_shock");
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyItem.OnActivated += OnActivated;
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_totalDamageFactor = Param("total_damage_percent") / 100f;
	}

	private void OnActivated()
	{
		MyHero.MyObject.CreateChildByPartData(_effectData, createPhysBody: false);
		_damage = _totalDamageFactor * MyHero.ShrinkDamageFactor * MyHero.GetTotalDPS();
		_hitTimer = 0.15f;
		_phase = 0;
		MyObject.MyHero = MyHero;
		MyObject.OnUpdate += OnUpdate;
		MyHero.UseMana(_mana);
	}

	private void OnUpdate(float elepsedTime)
	{
		if (_phase == 0 && _hitTimer <= 0f)
		{
			List<GameObject> list = MyLevel.ShockHit(MyObject, MyHero.MyObject.WorldPosition, 600f, (TargetClass)8440, DamageTypeEnum.Energy, _damage, 100f, null, onlyHitable: true, wantResults: true);
			foreach (GameObject item in list)
			{
				if (item.IsDestroyed)
				{
					item.MyLayer.CreateObjectByPartData(_effectData, item.WorldPosition, new Vector2(0.5f), 0f, 0, bCreateRigidBody: false);
					if (_destroyedList == null)
					{
						_destroyedList = new List<Vector2>();
					}
					_destroyedList.Add(item.Position);
				}
			}
			GameCore.Allocator.DeleteAliasingGameObjectList(list);
			List<GameObject> physPartsInRangeInAllPhsLayers = MyLevel.GetPhysPartsInRangeInAllPhsLayers(MyHero.MyObject.WorldPosition, (TargetClass)768, 600f, null, bOlyHittable: true);
			foreach (GameObject item2 in physPartsInRangeInAllPhsLayers)
			{
				if (item2.IsAlive)
				{
					item2.DoOnCollision(MyObject, item2.WorldPosition, new Vector2(0f, -1f));
				}
			}
			GameCore.Allocator.DeleteAliasingGameObjectList(physPartsInRangeInAllPhsLayers);
			if (_destroyedList == null)
			{
				_phase = 2;
			}
			else
			{
				_phase = 1;
				_hitTimer = 0.15f;
			}
		}
		else if (_phase == 1 && _hitTimer <= 0f)
		{
			_phase = 2;
			foreach (Vector2 destroyed in _destroyedList)
			{
				MyLevel.ShockHit(MyObject, destroyed, 300f, (TargetClass)8440, DamageTypeEnum.Energy, _damage / 2f, 100f, null, onlyHitable: true, wantResults: false);
			}
		}
		_hitTimer -= elepsedTime;
	}
}
