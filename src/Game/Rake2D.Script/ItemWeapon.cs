using System;
using System.Collections.Generic;
using Rake2D.JnG;

namespace Rake2D.Script;

public class ItemWeapon : GemableItemBehavior
{
	public delegate void OnShotStartHandler(GameObject go, ShotStream stream);

	public List<ShotStream> StreamList;

	public int ShellType;

	public bool IsFiring;

	public bool IsFireTimerPaused;

	public float FireTimer;

	public bool ManualFire;

	public int DisplayStremaCount;

	public ItemDamageMode DamageMode;

	protected List<ShotStreamData> _streamDataList;

	public virtual event OnShotStartHandler OnShotStart;

	public override void Init()
	{
		base.Init();
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		StreamList = new List<ShotStream>();
		_streamDataList?.ForEach((ShotStreamData data) =>
		{
			AddStream(data);
		});
		IsEnabledAutorack = false;
		IsEnabledMouseGun = false;
		IsEnabledBondShot = false;
		if (GameCore.IsShopMenuEnabled)
		{
			return;
		}
		foreach (ShotStream stream in StreamList)
		{
			if (FireTimer < stream.Data.ShotDelay / FireRateFactor)
			{
				FireTimer = stream.Data.ShotDelay / FireRateFactor;
			}
		}
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyItem.OnActivated += OnActivated;
		MyHero.MyObject.OnDestroy += OnHeroShipDestroy;
		MyHero.OnApplyLoadout += OnApplyLoadout;
	}

	private void OnHeroShipDestroy()
	{
		if (StreamList == null)
		{
			return;
		}
		foreach (ShotStream stream in StreamList)
		{
			stream.Destroy();
		}
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		OnShotStart = null;
		ShellType = ParamInt("shell_type");
		DamageMode = (ItemDamageMode)ParamInt("damage_mode");
		DisplayStremaCount = ParamInt("display_stream_count", -1);
		FireTimer = 0f;
		_streamDataList = new List<ShotStreamData>();
		ManualFire = false;
		if (MySlot != null && MySlot.SlotType == HeroSlotEnum.drone)
		{
			if (InitStreamParams($"drone_{MyItem.Level}", 100) == 0)
			{
				InitStreamParams($"{MyItem.Level}", 1);
			}
			foreach (ShotStreamData streamData in _streamDataList)
			{
				streamData.Heat = 0f;
			}
		}
		else
		{
			InitStreamParams($"{MyItem.Level}", 100);
		}
		OnApplyLoadout();
	}

	private void OnApplyLoadout()
	{
		FireTimer = 0f;
		if (StreamList == null)
		{
			return;
		}
		foreach (ShotStream stream in StreamList)
		{
			stream.LastFireNumber = 0L;
		}
	}

	public void OnReRegisterEvents(GameObject go)
	{
		go.OnCollision += MyItem.DoOnCollision;
	}

	private int InitStreamParams(string suffixPrefix, int maxCount)
	{
		int i;
		for (i = 0; i <= maxCount; i++)
		{
			string arg = $"{suffixPrefix}_{i}";
			string text = ParamString($"shot_id_{arg}");
			if (text == null)
			{
				break;
			}
			ShotStreamData shotStreamData;
			if ((shotStreamData = AddStreamData()) == null)
			{
				continue;
			}
			shotStreamData.ShotData = GameCore.GetPartData(text);
			text = ParamString($"gun_fire_id_{arg}");
			if (text != null)
			{
				if (Utils.Equals(text, "none"))
				{
					shotStreamData.GunFireData = null;
				}
				else
				{
					shotStreamData.GunFireData = GameCore.GetPartData(text);
				}
			}
			shotStreamData.IsSoundLooped = ParamBool($"sound_loop_{arg}");
			shotStreamData.IsSingleShot = ParamBool($"single_shot_{arg}");
			shotStreamData.ArcRange = ParamRange($"arc_{arg}") * ((float)Math.PI / 180f);
			shotStreamData.ArcSpeed = Param($"arc_speed_{arg}");
			shotStreamData.SpreadMode = ParamInt($"spread_{arg}");
			shotStreamData.ShotSpeedRange = ParamRange($"shot_speed_{arg}", 200f);
			shotStreamData.ShotRotationSpeedRange = ParamRange($"shot_rotation_speed_{arg}") * ((float)Math.PI / 180f);
			shotStreamData.ShotDamage = Param($"shot_damage_{arg}", 1f);
			shotStreamData.ShotCount = ParamInt($"shot_count_{arg}", 1);
			shotStreamData.ShotDelay = Param($"delay_{arg}", 1000f) / 1000f;
			shotStreamData.ShotRotationRange = ParamRange($"shot_angle_{arg}") * ((float)Math.PI / 180f);
			shotStreamData.IsShotAngleAbsolute = ParamBool($"shot_angle_absolute_{arg}");
			shotStreamData.StartDelay = Param($"start_delay_{arg}") / 1000f;
			shotStreamData.OffsetRange = ParamVector2Range($"offset_{arg}");
			shotStreamData.Heat = Param($"heat_{arg}");
			shotStreamData.ShotScaleRange = ParamRange($"scale_{arg}", 100f) / 100f;
			shotStreamData.ShotLifeTimeRange = ParamRange($"shot_life_time_{arg}", -1000f) / 1000f;
			shotStreamData.IsLifeTimeUsedToDPS = ParamBool($"shot_life_time_dps_{arg}");
			shotStreamData.GunFireScale = ParamVector2($"gun_fire_scale_{arg}", 100f, 100f) / 100f;
			shotStreamData.GunFireoOffset = ParamVector2($"gun_fire_offset_{arg}");
			shotStreamData.IsGunFireLooped = ParamBool($"gun_fire_looped_{arg}");
			shotStreamData.IsSingleGunFire = ParamBool($"single_gun_fire_{arg}");
			shotStreamData.GunFireUseStreamRotation = ParamBool($"gun_fire_stream_angle_{arg}");
			shotStreamData.ZOrder = ParamInt($"zorder_{arg}");
			shotStreamData.ResetFire = ParamBool($"reset_fire_{arg}");
			shotStreamData.SoundId = ParamString($"sound_{arg}");
			if (Utils.Equals(shotStreamData.SoundId, "none"))
			{
				shotStreamData.SoundId = null;
			}
			shotStreamData.SoundStopId = ParamString($"sound_stop_{arg}");
			shotStreamData.ShotAsChild = ParamBool($"child_shot_{arg}");
			if (shotStreamData.ShotLifeTimeRange.From < 0f && shotStreamData.ShotData != null)
			{
				shotStreamData.ShotLifeTimeRange = new FloatRange(shotStreamData.ShotData.LifeTime);
			}
			float[] array = ParamArray($"path_{arg}");
			if (array != null)
			{
				shotStreamData.SetPathControlPoints(array);
			}
			shotStreamData.PathFlipMode = ParamInt($"path_flip_{arg}");
		}
		return i;
	}

	public void DoOnShotStart(GameObject shotObject, ShotStream stream)
	{
		OnShotStart?.Invoke(shotObject, stream);
		MyHero.DoOnShotStart(shotObject);
	}

	public override bool TryGetParamSummary(string name, out float value, out float finalValue)
	{
		if (_streamDataList != null && Utils.Equals(name, "stream_count"))
		{
			if (DisplayStremaCount >= 0)
			{
				value = (finalValue = DisplayStremaCount);
			}
			else
			{
				value = 0f;
				foreach (ShotStreamData streamData in _streamDataList)
				{
					value += streamData.ShotCount;
				}
				finalValue = value;
			}
			return true;
		}
		if (Utils.Equals(name, "dps"))
		{
			value = GetDPS();
			finalValue = GetFinalDPS();
			return true;
		}
		if (Utils.Equals(name, "heat_per_sec"))
		{
			value = GetHeatPerSec();
			finalValue = GetFinalHeatPerSec();
			return true;
		}
		if (Utils.Equals(name, "fire_rate"))
		{
			value = GetFireRate();
			finalValue = GetFinalFireRate();
			return true;
		}
		return base.TryGetParamSummary(name, out value, out finalValue);
	}

	public override float GetDPS()
	{
		float num = 0f;
		if (_streamDataList != null)
		{
			foreach (ShotStreamData streamData in _streamDataList)
			{
				float num2 = streamData.ShotDamage * (float)streamData.ShotCount / streamData.ShotDelay;
				if (streamData.IsLifeTimeUsedToDPS)
				{
					num2 *= streamData.ShotLifeTimeRange.GetAverage() * (float)streamData.ShotCount;
				}
				num += num2;
			}
		}
		return num;
	}

	public float GetFinalDPS()
	{
		float num = 0f;
		if (_streamDataList != null)
		{
			foreach (ShotStreamData streamData in _streamDataList)
			{
				float num2 = streamData.ShotDamage * GetDamageFactor() * (float)streamData.ShotCount / (streamData.ShotDelay / FireRateFactor);
				if (streamData.IsLifeTimeUsedToDPS)
				{
					num2 *= streamData.ShotLifeTimeRange.GetAverage() * (float)streamData.ShotCount * LifeTimeFactor;
				}
				num += num2;
			}
		}
		return num;
	}

	public float GetHeatPerSec()
	{
		float hps = 0f;
		_streamDataList?.ForEach((ShotStreamData data) =>
		{
			hps += data.Heat * (float)data.ShotCount / data.ShotDelay;
		});
		return hps;
	}

	public float GetFinalHeatPerSec()
	{
		float num = 0f;
		if (StreamList != null)
		{
			foreach (ShotStream stream in StreamList)
			{
				num += stream.GetHeatPerSec();
			}
			return num;
		}
		return GetHeatPerSec();
	}

	public float GetFireRate()
	{
		float fireRate = 0f;
		_streamDataList?.ForEach((ShotStreamData data) =>
		{
			fireRate += data.FireRate;
		});
		return fireRate;
	}

	public float GetFinalFireRate()
	{
		return GetFireRate() * FireRateFactor;
	}

	public ShotStreamData AddStreamData()
	{
		ShotStreamData shotStreamData = new ShotStreamData();
		_streamDataList.Add(shotStreamData);
		return shotStreamData;
	}

	public void AddStreamData(ShotStreamData streamData)
	{
		_streamDataList.Add(streamData);
	}

	public ShotStream AddStream(ShotStreamData data)
	{
		ShotStream shotStream = new ShotStream(this, data);
		StreamList.Add(shotStream);
		return shotStream;
	}

	private void OnActivated()
	{
		ManualFire = true;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (!IsFireTimerPaused)
		{
			FireTimer += elapsedTime;
		}
		if (IsEnabledMouseGun && !GameCore.IsShopMenuEnabled && MyHero.MouseGun != null)
		{
			UpdateMouseGun(elapsedTime);
		}
		else if (IsEnabledAutorack)
		{
			UpdateAutotrack(elapsedTime);
		}
		if (MyItem.IsButtonDownAndReady || ManualFire)
		{
			IsFiring = true;
		}
		else
		{
			IsFiring = false;
		}
		if (StreamList != null)
		{
			bool flag = false;
			if (IsFiring)
			{
				IsFireTimerPaused = false;
			}
			foreach (ShotStream stream in StreamList)
			{
				if (!IsFiring && stream.IsShotReady())
				{
					IsFireTimerPaused = true;
				}
				if (stream.Update(elapsedTime))
				{
					flag = true;
				}
			}
			if (flag && ShellType == 1)
			{
				if (MyHero.IsJetPackActive && MySlot.SlotType == HeroSlotEnum.front_gun)
				{
					MyHero.IsJetPackShellReady = true;
				}
				else
				{
					MyHero.IsShellReady = true;
				}
			}
		}
		ManualFire = false;
	}

	private void UpdateMouseGun(float elapsedTime)
	{
		float num;
		if (MyHero.MouseGun.IsActive)
		{
			num = ((MySlot.SlotType != HeroSlotEnum.front_gun || !MyHero.IsJetPackActive) ? Utils.GetRotationTo(MyObject.WorldPosition, MyHero.MouseGun.WorldPosition - MyLayer.ScrollSpeed * elapsedTime, MySlot.WorldRotation) : Utils.GetRotationTo(MyObject.WorldPosition, MyHero.MouseGun.WorldPosition - MyLayer.ScrollSpeed * elapsedTime, MyHero.JetPackObject.WorldRotation));
		}
		else
		{
			num = 0f;
		}
		float currentValue = ((MyObject.SpineConnector == null) ? MyObject.Rotation : Utils.DegToRad(MyObject.SpineConnector.GetWorldAngle()));
		if (currentValue - num < -(float)Math.PI)
		{
			currentValue += (float)Math.PI * 2f;
		}
		else if (currentValue - num > (float)Math.PI)
		{
			currentValue -= (float)Math.PI * 2f;
		}
		Utils.ChangeValueToTarget(ref currentValue, num, 5f * elapsedTime);
		if (MyObject.SpineConnector != null)
		{
			MyObject.SpineConnector.SetAngle(Utils.RadToDeg(currentValue - MyObject.ParentObject.WorldRotation), bWorld: false);
		}
		else
		{
			MyObject.SetRotation(currentValue);
		}
	}

	private void UpdateAutotrack(float elapsedTime)
	{
		TargetClass targetClass = TargetClass.AllEnemy;
		if ((ExtraHitTo & PhysClass.MissileEnemy) != 0)
		{
			targetClass |= TargetClass.MissileEnemy;
		}
		GameObject gameObject = MyObject.FindNearestTarget(targetClass, 1200f, 0f, null, bOnlyHittable: true);
		if (gameObject == null)
		{
			gameObject = MyObject.FindNearestTarget(TargetClass.Man, 1200f, 0f, null, bOnlyHittable: true);
		}
		float num = ((gameObject == null) ? 0f : Utils.GetRotationTo(MyObject.WorldPosition, gameObject.GetTargetWorldPosition(), MySlot.WorldRotation));
		float currentValue = ((MyObject.SpineConnector == null) ? MyObject.Rotation : Utils.DegToRad(MyObject.SpineConnector.GetWorldAngle() - 90f));
		if (currentValue - num < -(float)Math.PI)
		{
			currentValue += (float)Math.PI * 2f;
		}
		else if (currentValue - num > (float)Math.PI)
		{
			currentValue -= (float)Math.PI * 2f;
		}
		Utils.ChangeValueToTarget(ref currentValue, num, 5f * elapsedTime);
		if (MyObject.SpineConnector != null)
		{
			MyObject.SpineConnector.SetAngle(Utils.RadToDeg(currentValue - MyObject.ParentObject.WorldRotation) + 90f, bWorld: false);
		}
		else
		{
			MyObject.SetRotation(currentValue);
		}
	}
}
