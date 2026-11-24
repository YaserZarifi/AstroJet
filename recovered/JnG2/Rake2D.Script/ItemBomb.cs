using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ItemBomb : ItemBehaviorScript
{
	private float _damage;

	private int _batchSize;

	private Vector2Range _startOffsetRange;

	private Vector2Range _startVelocityRange;

	private float _dropTimer;

	private float _dropDelay;

	private float _reloadTimer;

	private float _reloadDelay;

	private float _reloadBeginTimer;

	private float _reloadBeginDelay;

	private FloatRange _startRotationRange;

	private bool _hideBombDamage;

	protected PartData _bombData;

	public override void RecalculateParams()
	{
		_dropDelay = Param("drop_delay", 500f) / 1000f;
		_reloadDelay = Param("reload_delay", 250f) / 1000f;
		_reloadBeginDelay = Param("reload_begin_delay") / 1000f;
		_startRotationRange = ParamRange("start_angle", 90f, 90f) * ((float)Math.PI / 180f);
		_damage = Param("damage");
		_startOffsetRange = ParamVector2Range("start_offset");
		_startVelocityRange = ParamVector2Range("start_speed", 0f, 100f);
		_hideBombDamage = ParamBool("hide_bomb_damage");
		_dropTimer = 0f;
		_reloadTimer = _reloadDelay;
		_reloadBeginTimer = _reloadBeginDelay;
		_batchSize = (int)Param("batch_size", 1f);
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyHero.OnApplyLoadout += OnApplyLoadout;
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		_bombData = GameCore.GetPartData(ParamString("object_id"));
		MyHero.MaxBombCount = _batchSize;
		MyHero.CurrentBombCount = MyHero.MaxBombCount;
	}

	private void OnApplyLoadout()
	{
		_dropTimer = 0f;
		_reloadTimer = _reloadDelay;
		MyHero.CurrentBombCount = 0f;
	}

	public override void Update(float elapsedTime)
	{
		if (_dropTimer > 0f)
		{
			_dropTimer -= elapsedTime;
		}
		if (MyItem.IsButtonDownAndReady && _dropTimer <= 0f && MyHero.CurrentBombCount > 0f)
		{
			_dropTimer = _dropDelay;
			_reloadTimer = _reloadDelay;
			_reloadBeginTimer = _reloadBeginDelay;
			for (int i = 0; i < ((!MyHero.IsBombDuplicatorEnabled) ? 1 : 2); i++)
			{
				Vector2 pos = MyObject.WorldPosition + _startOffsetRange.GetRandom() * MyHero.MyObject.WorldScale;
				if (MyHero.IsBombDuplicatorEnabled)
				{
					pos.X += Utils.GetRandom((-0.5f + (float)i) * 20f, (-0.5f + (float)i) * 100f);
					pos.Y += Utils.GetRandom(-10, 10);
				}
				GameObject gameObject = MyObject.MyLayer.CreateObjectByPartData(_bombData, pos, MyObject.WorldScale, MyObject.WorldRotation + _startRotationRange.GetRandom(), MyHero.MyObject.ZOrder - 1);
				gameObject.Damage = (long)(_damage * MyHero.MyObject.Scale.X);
				Vector2 velocity = _startVelocityRange.GetRandom() + MyLayer.MyLevel.ScrollSpeed;
				if (MyHero.IsBombDuplicatorEnabled)
				{
					velocity.X += (-0.5f + (float)i) * 20f;
				}
				gameObject.SetVelocity(velocity);
				gameObject.Init();
				gameObject.LevelInit();
				MyHero.DoOnBombStart(gameObject);
			}
			MyHero.CurrentBombCount--;
		}
		if (_reloadBeginTimer > 0f)
		{
			_reloadBeginTimer -= elapsedTime;
		}
		if (!(_reloadBeginTimer <= 0f))
		{
			return;
		}
		if (_reloadTimer > 0f)
		{
			_reloadTimer -= elapsedTime;
		}
		if (!(_reloadTimer <= 0f) || !(MyHero.CurrentBombCount < (float)MyHero.MaxBombCount))
		{
			return;
		}
		_reloadTimer = _reloadDelay;
		MyHero.CurrentBombCount++;
		if (!GameCore.IsShopMenuEnabled)
		{
			if (!GameCore.IsShopMenuEnabled && MyHero.CurrentBombCount == (float)MyHero.MaxBombCount)
			{
				GameCore.PlaySound("hud_bomb_ready");
			}
			else
			{
				GameCore.PlaySound("hud_bomb_counter");
			}
		}
	}

	public float GetDropRate()
	{
		int batchSize = _batchSize;
		return 1f / ((float)(batchSize - 1) * _dropDelay + _reloadBeginDelay + _reloadDelay * (float)batchSize) * (float)batchSize;
	}

	public override bool TryGetParamSummary(string name, out float value, out float finalValue)
	{
		if (_hideBombDamage)
		{
			if (Utils.Equals(name, "bomb_damage"))
			{
				value = _damage;
				finalValue = value;
				return false;
			}
			if (Utils.Equals(name, "dps"))
			{
				value = _damage;
				if (MyHero != null)
				{
					finalValue = value * (float)((!MyHero.IsBombDuplicatorEnabled) ? 1 : 2);
				}
				else
				{
					finalValue = value;
				}
				return true;
			}
		}
		else
		{
			if (Utils.Equals(name, "bomb_damage"))
			{
				value = _damage;
				finalValue = value;
				return true;
			}
			if (Utils.Equals(name, "dps"))
			{
				value = GetDropRate() * _damage;
				if (MyHero != null)
				{
					finalValue = value * (float)((!MyHero.IsBombDuplicatorEnabled) ? 1 : 2);
				}
				else
				{
					finalValue = value;
				}
				return true;
			}
		}
		if (Utils.Equals(name, "reload_delay"))
		{
			value = _reloadDelay + _reloadBeginDelay;
			finalValue = value;
		}
		else
		{
			if (!Utils.Equals(name, "batch_size"))
			{
				return base.TryGetParamSummary(name, out value, out finalValue);
			}
			value = _batchSize;
			finalValue = value;
		}
		return true;
	}
}
