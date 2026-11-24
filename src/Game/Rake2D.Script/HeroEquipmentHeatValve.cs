using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroEquipmentHeatValve : ItemBehaviorScript
{
	private PartData _flameData;

	private float _flameTimer;

	private float _throwTimer;

	private float _throwDelay;

	private float _heatLimit;

	private float _totalDPSFactor;

	private int _throwCount;

	private bool _isThrowing;

	private List<GameObject> _flameList;

	public override void Init()
	{
		base.Init();
		_flameData = GameCore.GetPartData("heat_valve");
		_flameList = GameCore.Allocator.GetGameObjectList();
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		_isThrowing = false;
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_throwDelay = Param("delay", 3000f) / 1000f;
		_heatLimit = Param("heat_limit", 30f);
		_totalDPSFactor = Param("total_damage_percent", 100f) / 100f;
	}

	private void CreateFlame(Vector2 offset, float angle)
	{
		GameObject gameObject = MyObject.CreateChildByPartData(_flameData, createPhysBody: true);
		gameObject.SetPosition(offset);
		gameObject.SetRotation(Utils.DegToRad(angle));
		gameObject.SetScale(0.01f);
		gameObject.Damage = MyHero.GetTotalDPS() * _totalDPSFactor;
		_flameList.Add(gameObject);
	}

	public override void Update(float elapsedTime)
	{
		for (int num = _flameList.Count - 1; num >= 0; num--)
		{
			_flameList[num].SetScale(1f);
		}
		_flameList.Clear();
		if (_isThrowing)
		{
			Vector2 offset = new Vector2(25f, 0f);
			if (!Utils.ChangeValueToTarget(ref _throwTimer, 0f, elapsedTime))
			{
				return;
			}
			MyObject.PlaySound("hero_heat_valve");
			MyHero.RemoveHeat(_heatLimit / 2f);
			if (_throwCount == 0)
			{
				CreateFlame(offset, 150f);
				if (MyItem.Level > 1)
				{
					CreateFlame(offset, 30f);
				}
				if (MyItem.Level > 2)
				{
					CreateFlame(new Vector2(-50f, 0f), -90f);
				}
			}
			else
			{
				if (MyItem.Level == 1)
				{
					CreateFlame(offset, 30f);
				}
				else
				{
					CreateFlame(offset, 150f);
				}
				if (MyItem.Level > 1)
				{
					CreateFlame(offset, 30f);
				}
				if (MyItem.Level > 2)
				{
					CreateFlame(new Vector2(-50f, 0f), -90f);
				}
			}
			_throwTimer = 0.9f;
			_throwCount++;
			if (_throwCount > 1)
			{
				_isThrowing = false;
				_flameTimer = _throwDelay;
			}
		}
		else if (Utils.ChangeValueToTarget(ref _flameTimer, 0f, elapsedTime) && MyHero.Heat >= _heatLimit)
		{
			_isThrowing = true;
			_throwTimer = 0f;
			_throwCount = 0;
		}
	}
}
