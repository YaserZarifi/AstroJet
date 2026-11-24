using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroGunbotXenon : HeroDroneDeployer
{
	private bool _isDetached;

	private float _damage;

	private float _fireRate;

	private float _fireTimer;

	private int _shotCount;

	public override void Init()
	{
		base.Init();
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_damage = Param("damage");
		_shotCount = ParamInt("shot_count");
		_fireRate = Param("fire_rate");
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyItem.OnActivated += OnActivated;
		MyItem.OnStartInSlotEnd += OnStartInSlotEnd;
	}

	protected virtual void OnStartInSlotEnd()
	{
		_maxCount = ((!MyHero.IsBombDuplicatorEnabled) ? 1 : 2);
	}

	private void OnActivated()
	{
		SendLinkToAllDrones(_isDetached ? "attach" : "detach", 0);
		_isDetached = !_isDetached;
	}

	protected virtual string GetObjectName()
	{
		return $"hero_gunbot_xenon_{MyItem.Level + 1}";
	}

	protected override GameObject Deploy()
	{
		if (_isDetached)
		{
			OnActivated();
		}
		GameObject gameObject = MyObject.CreateChildByPartData(GetObjectName(), createPhysBody: true);
		if (gameObject != null)
		{
			gameObject.SetPosition(MyObject.WorldPosition);
			BehaviorDataDrone behaviorDataDrone = new BehaviorDataDrone(DroneTypeEnum.Xenon)
			{
				MaxSpeed = Param("speed"),
				Acceleration = Param("acceleration") / 100f
			};
			SetOffset(behaviorDataDrone);
			gameObject.BehaviorData = behaviorDataDrone;
			gameObject.MyItem = MyItem;
			if (MyObject.WorldRotation == 0f)
			{
				gameObject.SetRotation(((behaviorDataDrone.OffsetToHero.Y > 0f) ? ((float)Math.PI / 2f) : (-(float)Math.PI / 2f)) - gameObject.WorldRotation);
			}
			else
			{
				gameObject.ChangePartStateToAll("activate");
			}
			gameObject.SetVelocity(behaviorDataDrone.OffsetToHero * 2.8f + MyLevel.ScrollSpeed);
			GameObject childByIdDeep = gameObject.GetChildByIdDeep("trigger");
			if (childByIdDeep != null)
			{
				childByIdDeep.Damage = _damage;
				childByIdDeep.MyHero = MyHero;
			}
		}
		return gameObject;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (Utils.ChangeValueToTarget(ref _fireTimer, 0f, elapsedTime) && MyHero.KeyDown(GameInput.KeyTypeEnum.FireGuns))
		{
			SendLinkToAllDrones("fire", 1);
			_fireTimer = 1f / _fireRate;
		}
	}

	protected override void SetOffset(BehaviorDataDrone droneData)
	{
		foreach (GameObject drone in _droneList)
		{
			if (drone.BehaviorData is BehaviorDataDrone { Placement: 0 })
			{
				droneData.OffsetToHero = new Vector2(-10f, -60f);
				droneData.Placement = 1;
				return;
			}
		}
		droneData.Placement = 0;
		droneData.OffsetToHero = new Vector2(-10f, 60f);
	}

	public override bool TryGetParamSummary(string name, out float value, out float finalValue)
	{
		if (Utils.Equals(name, "dps"))
		{
			value = _damage * _fireRate * (float)_shotCount;
			finalValue = value * (float)_maxCount;
		}
		else
		{
			if (!Utils.Equals(name, "fire_rate"))
			{
				return base.TryGetParamSummary(name, out value, out finalValue);
			}
			value = _fireRate * (float)_shotCount;
			finalValue = value * (float)_maxCount;
		}
		return true;
	}
}
