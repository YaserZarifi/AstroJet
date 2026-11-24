using System;
using System.Collections.Generic;

namespace Rake2D.Script;

public class HeroEquipmentCirculus : ItemBehaviorScript
{
	private List<GameObject> _ballList;

	private int _mode;

	private int _ballCount = 1;

	private float _rotation;

	private float _maxRadius;

	private float _radius;

	private float _orbitOffset;

	private float _rotationSpeed;

	private float _collisionDPS;

	private float _deployTimer;

	private PartData _ballData;

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_collisionDPS = Param("collision_dps");
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyItem.OnStartInSlotEnd += OnStartInSlotEnd;
	}

	public float GetFinalCollisionDPS()
	{
		if (MyHero != null)
		{
			return _collisionDPS * MyHero.DroneCollisionDamageFactor;
		}
		return _collisionDPS;
	}

	public override bool TryGetParamSummary(string name, out float value, out float finalValue)
	{
		if (Utils.Equals(name, "collision_dps_per_drone"))
		{
			value = _collisionDPS;
			finalValue = GetFinalCollisionDPS();
			return true;
		}
		return base.TryGetParamSummary(name, out value, out finalValue);
	}

	protected virtual void OnStartInSlotEnd()
	{
		if (!GameCore.IsMainMenuEnabled)
		{
			_orbitOffset = 0f;
			_maxRadius = Param("radius");
			_ballCount = ParamInt("ball_count", 1);
			_ballData = GameCore.GetPartData("hero_drone_circulus_ball");
			switch (MyItem.Level)
			{
			case 0:
				_rotationSpeed = 7.25f;
				break;
			case 1:
				_rotationSpeed = 7f;
				break;
			case 2:
				_rotationSpeed = 6.5f;
				break;
			default:
				_rotationSpeed = 6f;
				break;
			}
			_deployTimer = 0f;
			Deploy();
		}
	}

	private void Deploy()
	{
		GameCore.Allocator.DeleteAliasingGameObjectList(_ballList);
		_ballList = GameCore.Allocator.GetGameObjectList(_ballCount);
		MyObject.IsRotationFixed = true;
		for (int i = 0; i < _ballCount; i++)
		{
			GameObject gameObject = MyObject.CreateChildByPartData(_ballData, createPhysBody: true);
			gameObject.Damage = GetFinalCollisionDPS();
			gameObject.SetScale(0.01f);
			gameObject.Activate();
			gameObject.MyItem = MyItem;
			MyHero.RegisterDrone(gameObject);
			_ballList.Add(gameObject);
		}
		_radius = 0f;
		_mode = 0;
	}

	public override void Update(float elapsedTime)
	{
		if (elapsedTime == 0f || _ballList == null)
		{
			return;
		}
		int num = 0;
		if (_mode == 0)
		{
			if (Utils.ChangeValueToTarget(ref _radius, _maxRadius, 80f * _rotationSpeed * elapsedTime))
			{
				_mode = 1;
			}
			_rotation += _radius / _maxRadius * _rotationSpeed * elapsedTime;
			_orbitOffset = _radius / _maxRadius * 40f;
			for (int i = 0; i < _ballList.Count; i++)
			{
				if (_ballList[i] == null || _ballList[i].IsDestroyed)
				{
					_ballList[i] = null;
					num++;
				}
				else
				{
					_ballList[i].SetPosition(MyHero.DroneCenterOffset + Utils.GetVelocityVector2(_radius * MyHero.DroneRadiusFactor, _rotation + (float)Math.PI * 2f / (float)_ballList.Count * (float)i));
					_ballList[i].SetScale(_orbitOffset / 40f);
				}
			}
		}
		else if (_mode == 1)
		{
			_rotation += _rotationSpeed * elapsedTime;
			MyObject.SetPosition(Utils.RotateVector2(MyHero.DroneCenterOffset, MyHero.MyObject.WorldRotation));
			for (int j = 0; j < _ballList.Count; j++)
			{
				if (_ballList[j] == null || _ballList[j].IsDestroyed)
				{
					_ballList[j] = null;
					num++;
				}
				else
				{
					_ballList[j].OldPosition = _ballList[j].Position;
					_ballList[j].SetPosition(Utils.GetVelocityVector2(_radius * MyHero.DroneRadiusFactor, _rotation + (float)Math.PI * 2f / (float)_ballList.Count * (float)j));
				}
			}
		}
		if (num >= _ballList.Count && _deployTimer <= 0f)
		{
			_deployTimer = 1f;
		}
		if (_deployTimer > 0f && Utils.ChangeValueToTarget(ref _deployTimer, 0f, elapsedTime))
		{
			Deploy();
		}
	}
}
