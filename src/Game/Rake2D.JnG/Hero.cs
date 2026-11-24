using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Menu;
using Rake2D.Script;
using Rake2D.Sound;

namespace Rake2D.JnG;

public class Hero
{
	public delegate void OnApplyLoadouthandler();

	public delegate void OnShotStartHandler(GameObject go);

	public delegate void OnCollisionHandler(GameObject go, Vector2 contactPoint, Vector2 normal);

	public delegate void OnFieldSeparationHandler(GameObject go);

	public delegate void OnHitBeginHandler(ref float damage, DamageTypeEnum damageType, GameObject hittingObject);

	public const float HP_CRITICAL_PERCENT = 0.25f;

	public int PlayerNumber;

	public string PlayerName;

	public int FaceNumber;

	public int StatDeathCount;

	public ShipParams ShipParams;

	public GameObject JetPackObject;

	public GameObject JetPackSlotPart;

	public GameObject JetPackMuzzlePart;

	public float ShipRecoveryValue;

	public string ShipId;

	public bool IsShellReady;

	public bool IsJetPackShellReady;

	public bool RecreateSmallShip;

	public SpriteBatch SpriteBatch;

	public List<GameObject> DroneList;

	public Vector2 LastPosition;

	public Remcon Remcon;

	public MouseGun MouseGun;

	public HUD Hud;

	public List<GameObject> FieldObjectList;

	public GameObject FieldSensorPart;

	private bool[] _keyDownArray;

	private bool[] _keyPressedArray;

	public float MaxSpeedFactor;

	public Vector2 SpeedRatio;

	public float ShipMaxHPFactor;

	public float MaxShield;

	public float ShieldRechargeSpeed;

	public float ShieldWaitDelay;

	public float ShieldWaitTimer;

	public float[] DamageFactorArray;

	public float DamageFactorDoubleDragon;

	public float DamageFactorHarmonicAmplifier;

	public float PanicAttackFactor;

	public int SuperchargedBombProbability;

	public float SuperchargedBombTotalDPSFactor;

	public float BurnDPS;

	public float AcidDPS;

	public float DamageLightning;

	public float DamageWave;

	public float DroneRadiusFactor;

	public float DroneCollisionDamageFactor;

	public float CollisionDamageFactor;

	public Vector2 DroneCenterOffset;

	public HeroItem DroneCenterChangingItem;

	public HeroItem DroneRadiusChangingItem;

	public HeroItem CurrentLoadoutItem;

	public Loadout CurrLoadout;

	public float Mana;

	public float MaxMana;

	public float ManaRechargeSpeed;

	public float MaxHeat;

	public float MaxBurningHeat;

	public float Heat;

	public float CoolingFactor;

	public float HeatFactor;

	public bool IsOverheated;

	public bool IsBurning;

	public bool IsBuzzsawEnabled;

	public bool IsMainGemEnabled;

	public bool IsQuazitronEnabled;

	public bool IsDemolisherEnabled;

	public bool IsElDiabloEnabled;

	public bool IsConnectedDevicesEnabled;

	public bool IsBombDuplicatorEnabled;

	public bool IsBlastProjectorEnabled;

	public bool IsCarnageTVEnabled;

	public bool IsZeldorPanzerEnabled;

	public bool IsLoadoutSwapperEnabled;

	public bool IsSetEnablerEnabled;

	public bool WasLoadoutApplyed;

	public float HeatToDamageFactor;

	public float ShellGrenadierFactor;

	public float HullBoostHPFactor;

	public bool IsShrinkerEnabled;

	public bool IsJetPackInstalled;

	public int MaxBombCount;

	public float CurrentBombCount;

	public HeroAutopilotMode AutopilotMode;

	public HeroAutopilotStateXEnum AutopilotStateX = HeroAutopilotStateXEnum.MoveRight;

	public HeroAutopilotStateYEnum AutopilotStateY = HeroAutopilotStateYEnum.Wait;

	public Vector2 AutopilotStartPos;

	private int _stuckOnViewEdge;

	private Vector2 _autopilotVelocity = new Vector2(0f);

	private float _autopilotStateTimerX;

	private float _autopilotStateTimerY;

	private float _autopilotAngle;

	private float _autoPilotAngleSpeed;

	private bool _autopilotFireWaitToZeroHeat;

	private bool _autopilotBombWaitToFullRound;

	private bool _autopilotSwap;

	private float _coolStartTimer;

	private bool _wasKillReported;

	private int _currentStateIndex;

	private float _burningStopTimer;

	private float _burningHitTimer;

	private float _burningDamage;

	private float _sideFrontOffsetX;

	private float _sideBackOffsetX;

	private GameObject _sideFrontPart;

	private GameObject _sideBackPart;

	public PartData _heatFreezeData;

	public GameObject _heatFreezeObject;

	private bool _partHpCriticalActive;

	private float _shellTimer;

	private PartData _shellData;

	private PartData _shellGrenadierData;

	private GameObject _shellOutPart;

	private static string _strHPText;

	private static readonly string[] _partStateId;

	private static BehaviorData _behSuperchargedBomb;

	public GameObject MyObject { get; internal set; }

	public float ShrinkDamageFactor => MyObject.Scale.X;

	public bool NoHeat
	{
		get
		{
			if (!Cheat.NoHeat)
			{
				return DamageFactorDoubleDragon > 1f;
			}
			return true;
		}
	}

	public bool IsJetPackActive => JetPackObject != null;

	public bool IsAlive
	{
		get
		{
			if (MyObject.IsAlive)
			{
				return !IsJetPackActive;
			}
			return false;
		}
	}

	public HeroShip MyShip { get; private set; }

	public GameObject PartOverheated { get; private set; }

	public GameObject PartBurning { get; private set; }

	public GameObject PartHpCritical { get; private set; }

	public ISound2D SoundOverheated { get; private set; }

	public float MaxSpeed => 900f * MaxSpeedFactor;

	public float Acceleration => 10000f * MaxSpeedFactor;

	public float Shield { get; private set; }

	public bool IsShieldWaiting
	{
		get
		{
			if (ShieldWaitTimer > 0f && MaxShield > 0f)
			{
				return Shield <= 0f;
			}
			return false;
		}
	}

	public bool IsShieldEnabled => MaxShield > 0f;

	public float NoHeatTimer { get; private set; }

	public bool IsDestroyed
	{
		get
		{
			if (MyObject != null)
			{
				return MyObject.IsDestroyed;
			}
			return false;
		}
	}

	public float ShipHPPercent => MyObject.HPPercent;

	public float HPPercent
	{
		get
		{
			return GetControlledObject()?.HPPercent ?? 0f;
		}
		set
		{
			if (MyObject != null)
			{
				MyObject.CurrentHP = (long)(MyObject.MaxHP * value);
			}
		}
	}

	public float HeatPercent
	{
		get
		{
			if (!(MaxHeat > 0f))
			{
				return 0f;
			}
			return Heat / MaxHeat;
		}
	}

	public float ManaPercent
	{
		get
		{
			if (!(MaxMana > 0f))
			{
				return 0f;
			}
			return Mana / MaxMana;
		}
		set
		{
			Mana = (long)(MaxMana * value);
		}
	}

	public float ShieldPercent
	{
		get
		{
			if (!(MaxShield > 0f))
			{
				return 0f;
			}
			return Shield / MaxShield;
		}
	}

	public event OnShotStartHandler OnShotStart;

	public event OnShotStartHandler OnBombStart;

	public event OnApplyLoadouthandler OnBeforeApplyLoadout;

	public event OnApplyLoadouthandler OnApplyLoadout;

	public event OnHitBeginHandler OnHitBegin;

	public event OnCollisionHandler OnFieldCollision;

	public event OnFieldSeparationHandler OnFieldSeparation;

	public void SetJetPackObject(GameObject go)
	{
		if (go != null)
		{
			JetPackObject = go;
			JetPackMuzzlePart = JetPackObject.GetChildByIdDeep("muzzle");
			JetPackSlotPart = JetPackObject.GetChildByIdDeep("arm");
			JetPackObject.MyHero = this;
			_autopilotStateTimerY = 0f;
		}
		else
		{
			JetPackObject = null;
			JetPackObject = null;
			JetPackMuzzlePart = null;
			JetPackSlotPart = null;
		}
	}

	public GameObject GetControlledObject()
	{
		if (IsJetPackActive)
		{
			return JetPackObject;
		}
		return MyObject;
	}

	public bool KeyDown(GameInput.KeyTypeEnum keyType)
	{
		return _keyDownArray[(int)keyType];
	}

	public void SetKeyDown(GameInput.KeyTypeEnum keyType, bool bIsDown)
	{
		_keyDownArray[(int)keyType] = bIsDown;
	}

	public bool KeyPressed(GameInput.KeyTypeEnum keyType)
	{
		return _keyPressedArray[(int)keyType];
	}

	public void SetKeyPressed(GameInput.KeyTypeEnum keyType, bool bIsDown)
	{
		_keyPressedArray[(int)keyType] = bIsDown;
	}

	public float GetShipMaxHP()
	{
		if (IsZeldorPanzerEnabled)
		{
			return 10f * ShipMaxHPFactor;
		}
		return 100f * ShipMaxHPFactor;
	}

	public float GetBurnDPS()
	{
		return BurnDPS;
	}

	public float GetAcidDPS()
	{
		return AcidDPS;
	}

	public float GetBonusWeaponDamageFactor()
	{
		return Math.Max(0f, 1f + HeatToDamageFactor * Heat / 100f + PanicAttackFactor);
	}

	public float GetCoolingSpeed()
	{
		return 35f * CoolingFactor;
	}

	public Hero(int playerNumber)
	{
		PlayerNumber = playerNumber;
		_keyDownArray = new bool[8];
		_keyPressedArray = new bool[8];
		DamageFactorArray = new float[9];
		Remcon = new Remcon(this);
		Hud = new HUD(this);
	}

	public Hero()
	{
		PlayerNumber = 0;
	}

	public static void SetTexts()
	{
		_strHPText = TextBundle.GetText("hud_hp_text");
	}

	static Hero()
	{
		_partStateId = new string[7] { "Up3", "Up2", "Up1", "Idle", "Down1", "Down2", "Down3" };
		_behSuperchargedBomb = new BehaviorData();
		_behSuperchargedBomb.SetTypeName("OnCollisionCreate");
		_behSuperchargedBomb.SetParam("object_id", new string[1] { "hero_supercharged_bomb_lightning" });
		_behSuperchargedBomb.SetParam("parent_angle", new float[1]);
		_behSuperchargedBomb.SetParam("normal", new float[1] { 1f });
		_behSuperchargedBomb.SetParam("repeat_delay", new float[1] { 200f });
		_behSuperchargedBomb.SetParam("on_contact", new float[1] { 1f });
	}

	public void SpawnInLevel(GameLevel level)
	{
		DamageFactorDoubleDragon = 1f;
		DamageFactorHarmonicAmplifier = 1f;
		for (int i = 0; i < DamageFactorArray.Length; i++)
		{
			DamageFactorArray[i] = 1f;
		}
		Remcon.Clear();
		Hud.Clear();
		CancelAutopilot();
		if (GameCore.IsMainMenuEnabled)
		{
			CreateSmallShip(level.MainLayer, level.GetNearestSafePosition(level.Camera.Position.X + 550f));
			StartAutopilotMainMenu();
		}
		else if (level.CutsceneMode != 0)
		{
			StartAutopilotCutScene(level.MainLayer);
		}
		else
		{
			StartAutopilotLevelIncoming(level.MainLayer);
		}
		if (PlayerNumber == 0 && !GameCore.IsEditorEnabled)
		{
			MyObject.MyCamera.Position.Y = GameJnG.HeroList[0].MyObject.WorldPosition.Y - 540f;
		}
		NoHeatTimer = 0f;
		Heat = 0f;
		Mana = MaxMana;
		SetJetPackObject(null);
		_heatFreezeData = GameCore.GetPartData("hero_heat_freeze");
	}

	private void AddJetpackHP(float deltaHP, int textMode)
	{
		JetPackObject.CurrentHP += deltaHP;
		if (JetPackObject.CurrentHP > JetPackObject.MaxHP)
		{
			JetPackObject.CurrentHP = JetPackObject.MaxHP;
		}
		else if (JetPackObject.CurrentHP < 0f)
		{
			JetPackObject.CurrentHP = 0f;
		}
		else if (textMode == 1)
		{
			AddHPTextParticle(deltaHP, JetPackObject.WorldPosition);
		}
	}

	public float ApplyHeroDamageFactors(float damage, DamageTypeEnum damageType)
	{
		if (damageType != DamageTypeEnum.Unknown)
		{
			damage *= DamageFactorArray[(int)damageType];
		}
		damage *= DamageFactorDoubleDragon;
		damage *= DamageFactorHarmonicAmplifier;
		return damage;
	}

	public void AddHP(float deltaHP, int textMode)
	{
		if (!IsDestroyed)
		{
			MyObject.CurrentHP += deltaHP;
			if (MyObject.CurrentHP > MyObject.MaxHP)
			{
				MyObject.CurrentHP = MyObject.MaxHP;
			}
			else if (MyObject.CurrentHP < 0f)
			{
				MyObject.CurrentHP = 0f;
			}
			if (textMode == 1 && deltaHP >= 0.5f)
			{
				AddHPTextParticle((int)deltaHP, MyObject.Position + Utils.GetRandomVector2(-40f, -8f, 40f, 8f));
			}
		}
		if (textMode == 2)
		{
			string text = TextBundle.GetText("hud_warning_hp");
			text = text.Replace("@hp", ((int)deltaHP).ToString());
			MyObject.MyLevel.AddWarningText(text, new Color(255, 220, 80));
		}
		if (IsJetPackActive)
		{
			AddJetpackHP(deltaHP, textMode);
		}
	}

	public void SetShipHP(float hp)
	{
		MyObject.CurrentHP = Utils.Clamp(hp, 0f, MyObject.MaxHP);
	}

	public void SetMana(float mana)
	{
		Mana = Utils.Clamp(mana, 0f, MaxMana);
	}

	public void SetShield(float shield)
	{
		Shield = Utils.Clamp(shield, 0f, MaxShield);
	}

	public void AddShield(float delta)
	{
		SetShield(Shield + delta);
	}

	public bool IsItemIdInstalled(string itemId)
	{
		return !GetSlotItemList(GameCore.GetHeroItemData(itemId)).IsNullOrEmpty();
	}

	public HeroItem GetInstalledItem(string itemId)
	{
		List<HeroItem> slotItemList = GetSlotItemList(GameCore.GetHeroItemData(itemId));
		if (!slotItemList.IsNullOrEmpty())
		{
			foreach (HeroItem item in slotItemList)
			{
				if (!item.IsBlockedBySameItem)
				{
					return item;
				}
			}
		}
		return null;
	}

	public bool IsItemSetEnabled(string itemSetId)
	{
		if (GameJnG.ItemSetDictionary.TryGetValue(itemSetId, out var value))
		{
			string[] array;
			if (IsSetEnablerEnabled)
			{
				array = value;
				foreach (string itemId in array)
				{
					if (IsItemIdInstalled(itemId))
					{
						break;
					}
				}
				return false;
			}
			array = value;
			foreach (string itemId2 in array)
			{
				if (!IsItemIdInstalled(itemId2))
				{
					return false;
				}
			}
			GameJnG.UnlockAchievement(GameAchievementEnum.ACH_FIRST_ITEM_SET);
			return true;
		}
		GameCore.LogErr("Undefined item_set.{0}", itemSetId);
		return false;
	}

	public void ActualizeItemSets()
	{
		foreach (HeroSlot value in MyShip.Slots.Values)
		{
			if (value.Item != null)
			{
				value.Item.IsMyItemSetComplete = false;
			}
		}
		foreach (string key in GameJnG.ItemSetDictionary.Keys)
		{
			List<string> list = new List<string>(GameJnG.ItemSetDictionary[key]);
			bool flag = true;
			foreach (string item in list)
			{
				bool flag2 = false;
				foreach (HeroSlot value2 in MyShip.Slots.Values)
				{
					if (value2.Item != null && Utils.Equals(value2.Item.Data.Id, item))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (HeroSlot value3 in MyShip.Slots.Values)
			{
				if (value3.Item != null && list.Contains(value3.Item.Data.Id))
				{
					value3.Item.IsMyItemSetComplete = true;
				}
			}
		}
	}

	private void ClearEvents()
	{
		OnShotStart = null;
		OnBombStart = null;
		OnHitBegin = null;
		OnFieldCollision = null;
		OnFieldSeparation = null;
		OnApplyLoadout = null;
	}

	public int GetRank()
	{
		int score = TotalStats.Score;
		if (score < 130000)
		{
			return 0;
		}
		if (score < 224000)
		{
			return 1;
		}
		if (score < 412000)
		{
			return 2;
		}
		if (score < 694000)
		{
			return 3;
		}
		if (score < 1070000)
		{
			return 4;
		}
		if (score < 1540000)
		{
			return 5;
		}
		if (score < 2104000)
		{
			return 6;
		}
		if (score < 2762000)
		{
			return 7;
		}
		if (score < 3514000)
		{
			return 8;
		}
		if (score < 4360000)
		{
			return 9;
		}
		if (score < 5300000)
		{
			return 10;
		}
		if (score < 6334000)
		{
			return 11;
		}
		if (score < 7462000)
		{
			return 12;
		}
		if (score < 8684000)
		{
			return 13;
		}
		if (score < 10000000)
		{
			return 14;
		}
		return 15;
	}

	public int GetCamouflageIndex()
	{
		if (MyShip == null)
		{
			return 0;
		}
		int num = GameJnG.HeroCamouflageIdList.IndexOf(MyShip.PartMap.SkinId);
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	public void RegisterDrone(GameObject droneObject)
	{
		droneObject.MyHero = this;
		DroneList.Add(droneObject);
		if (DroneList.Count >= 10)
		{
			GameJnG.UnlockAchievement(GameAchievementEnum.ACH_DRONE_COUNT);
		}
	}

	public void ApplySlotToSlot(HeroSlot fromSlot, HeroSlot toSlot)
	{
		fromSlot.Item?.ApplyToItem(toSlot.Item);
	}

	public void ApplyEquipSlotsToSlot(HeroSlot toSlot)
	{
		ApplySlotToSlot(GetSlot(HeroSlotEnum.equipment_1), toSlot);
		ApplySlotToSlot(GetSlot(HeroSlotEnum.equipment_2), toSlot);
		ApplySlotToSlot(GetSlot(HeroSlotEnum.equipment_3), toSlot);
		ApplySlotToSlot(GetSlot(HeroSlotEnum.equipment_4), toSlot);
		ApplySlotToSlot(GetSlot(HeroSlotEnum.equipment_5), toSlot);
		ApplySlotToSlot(GetSlot(HeroSlotEnum.equipment_6), toSlot);
	}

	public void RebuildShipConfiguration()
	{
		ParamModifier modifier = default;
		ParamModifier modifier2 = default;
		ParamModifier modifier3 = default;
		ParamModifier modifier4 = default;
		ClearEvents();
		if (DroneList != null)
		{
			foreach (GameObject drone in DroneList)
			{
				drone.Destroy();
			}
			GameCore.Allocator.DeleteAliasingGameObjectList(DroneList);
		}
		DroneList = GameCore.Allocator.GetGameObjectList();
		FieldObjectList = GameCore.Allocator.GetGameObjectList();
		ShipMaxHPFactor = 1f;
		MaxSpeedFactor = 1f;
		DroneCollisionDamageFactor = 1f;
		CollisionDamageFactor = 1f;
		CoolingFactor = 1f;
		HeatFactor = 1f;
		ShellGrenadierFactor = 0f;
		ShipRecoveryValue = 0f;
		HullBoostHPFactor = 1f;
		MaxHeat = 100f;
		MaxBurningHeat = 999f;
		MaxMana = 100f;
		DroneCenterOffset = Vector2.Zero;
		DroneCenterChangingItem = null;
		DroneRadiusFactor = 1f;
		DroneRadiusChangingItem = null;
		SpeedRatio = Vector2.One;
		MaxBombCount = 0;
		CurrentBombCount = 0f;
		AcidDPS = 100f;
		BurnDPS = 100f;
		IsOverheated = false;
		IsBurning = false;
		IsConnectedDevicesEnabled = false;
		IsBuzzsawEnabled = false;
		IsMainGemEnabled = false;
		IsBombDuplicatorEnabled = false;
		IsQuazitronEnabled = false;
		IsDemolisherEnabled = false;
		IsElDiabloEnabled = false;
		IsShrinkerEnabled = false;
		IsBlastProjectorEnabled = false;
		IsCarnageTVEnabled = false;
		IsJetPackInstalled = false;
		IsZeldorPanzerEnabled = false;
		IsSetEnablerEnabled = false;
		IsLoadoutSwapperEnabled = false;
		HeatToDamageFactor = 0f;
		PanicAttackFactor = 0f;
		SuperchargedBombProbability = 0;
		SuperchargedBombTotalDPSFactor = 0f;
		WasLoadoutApplyed = false;
		GameJnG.RecalculateItemParams(this);
		MouseGun = null;
		for (int i = 0; i < MyShip.Slots.Values.Count; i++)
		{
			HeroSlot heroSlot = MyShip.Slots[(HeroSlotEnum)i];
			if (heroSlot.Item == null)
			{
				continue;
			}
			heroSlot.Item.IsBlockedBySameItem = false;
			if (heroSlot.SlotType >= HeroSlotEnum.equipment_1 && heroSlot.SlotType <= HeroSlotEnum.equipment_6)
			{
				List<HeroItem> slotItemList = GetSlotItemList(heroSlot.Item.Data);
				if (heroSlot.Item.Data.BlockId != null)
				{
					slotItemList.AddRange(GetSlotItemList(GameCore.GetHeroItemData(heroSlot.Item.Data.BlockId)));
				}
				foreach (HeroItem item in slotItemList)
				{
					if (item.MySlot.SlotType < heroSlot.Item.MySlot.SlotType)
					{
						heroSlot.Item.IsBlockedBySameItem = true;
						break;
					}
				}
			}
			heroSlot.Item.UpdateParamModifier(ref modifier3, "hero_mana_recharge", null);
			heroSlot.Item.UpdateParamModifier(ref modifier, "hero_shield_recharge", null);
			heroSlot.Item.UpdateParamModifier(ref modifier4, "hero_shield_delay", null);
			heroSlot.Item.UpdateParamModifier(ref modifier2, "hero_shield_max", null);
		}
		for (int j = 0; j < MyShip.Slots.Values.Count; j++)
		{
			HeroSlot heroSlot2 = MyShip.Slots[(HeroSlotEnum)j];
			if (MyObject != null)
			{
				heroSlot2.Start();
			}
		}
		if (IsMainGemEnabled)
		{
			ApplySlotToSlot(GetSlot(HeroSlotEnum.gem_top), GetSlot(HeroSlotEnum.front_gun));
			ApplySlotToSlot(GetSlot(HeroSlotEnum.gem_front), GetSlot(HeroSlotEnum.front_gun));
			ApplySlotToSlot(GetSlot(HeroSlotEnum.gem_bottom), GetSlot(HeroSlotEnum.front_gun));
		}
		else
		{
			ApplySlotToSlot(GetSlot(HeroSlotEnum.gem_top), GetSlot(HeroSlotEnum.top_gun));
			ApplySlotToSlot(GetSlot(HeroSlotEnum.gem_front), GetSlot(HeroSlotEnum.front_gun));
			ApplySlotToSlot(GetSlot(HeroSlotEnum.gem_bottom), GetSlot(HeroSlotEnum.bottom_gun));
		}
		ApplySlotToSlot(GetSlot(HeroSlotEnum.gem_rear), GetSlot(HeroSlotEnum.rear_gun));
		ApplyEquipSlotsToSlot(GetSlot(HeroSlotEnum.top_gun));
		ApplyEquipSlotsToSlot(GetSlot(HeroSlotEnum.front_gun));
		ApplyEquipSlotsToSlot(GetSlot(HeroSlotEnum.bottom_gun));
		ApplyEquipSlotsToSlot(GetSlot(HeroSlotEnum.rear_gun));
		ApplyEquipSlotsToSlot(GetSlot(HeroSlotEnum.missile));
		ApplyEquipSlotsToSlot(GetSlot(HeroSlotEnum.bomb));
		MaxShield = modifier2.GetValue(0f);
		ShieldRechargeSpeed = modifier.GetValue(0f);
		ShieldWaitDelay = modifier4.GetValue(0f);
		ManaRechargeSpeed = modifier3.GetValue(0f);
		Shield = MaxShield;
		if (MyObject != null)
		{
			MyObject.MaxHP = GetShipMaxHP();
			MyObject.CurrentHP = MyObject.MaxHP;
		}
		ShieldWaitTimer = 0f;
		for (int k = 0; k < MyShip.Slots.Values.Count; k++)
		{
			HeroSlot heroSlot3 = MyShip.Slots[(HeroSlotEnum)k];
			if (MyObject != null)
			{
				heroSlot3.StartEnd();
			}
		}
		if (FieldSensorPart != null)
		{
			FieldSensorPart.Destroy();
		}
		if (OnFieldCollision != null)
		{
			FieldSensorPart = MyObject.CreateChildByPartData("hero_field_sensor", createPhysBody: true);
		}
		else
		{
			FieldSensorPart = null;
		}
		if (GameJnG.ShopMenu != null)
		{
			GameJnG.ShopMenu.IsAtarixInstalled = GameJnG.IsItemIdInCargo("other/atarix");
		}
		ActualizeItemSets();
		CreateItemSetEffects();
		if (CurrentLoadoutItem == null)
		{
			HeroItemData heroItemData = GameCore.GetHeroItemData("other/loadout");
			CurrentLoadoutItem = new HeroItem(heroItemData);
		}
		if (CurrentLoadoutItem.Loadout == null)
		{
			CurrentLoadoutItem.Loadout = SaveLoadout(GameJnG.GetFreeLoadoutNumber());
		}
		else
		{
			CurrentLoadoutItem.Loadout = SaveLoadout(CurrentLoadoutItem.Loadout.Number);
		}
	}

	private float GetOffsetXForPart(GameObject part)
	{
		if (part != null && MyShip.PartMap.PartOffsetXPercentages.TryGetValue(part.Id, out var value))
		{
			return part.PartData.TargetOffset.X + (part.PartData.TargetOffset.Y - part.PartData.TargetOffset.X) * value;
		}
		return 0f;
	}

	public void ActualizeSideOffsets()
	{
		_sideFrontPart = MyObject.GetChildByIdDeep("side_front");
		_sideFrontOffsetX = GetOffsetXForPart(_sideFrontPart);
		_sideBackPart = MyObject.GetChildByIdDeep("side_back");
		_sideBackOffsetX = GetOffsetXForPart(_sideBackPart);
	}

	public void CreateSmallShip(Layer layer, Vector2 position)
	{
		Transformation transformation = null;
		Vector2 velocity = Vector2.Zero;
		float timer = 0f;
		RecreateSmallShip = false;
		_wasKillReported = false;
		Layer layer2;
		if (MyObject != null)
		{
			transformation = MyObject;
			velocity = MyObject.Velocity;
			timer = MyObject.Timer;
			layer2 = layer ?? MyObject.MyLayer;
			DestroySmallShip();
		}
		else
		{
			layer2 = layer ?? GameCore.GameLevel.MainLayer;
		}
		_currentStateIndex = -1;
		_shellData = GameCore.GetPartData("shot_shell");
		_shellGrenadierData = GameCore.GetPartData("shot_shell_grenadier");
		try
		{
			PartData partData = GameCore.GlobalContent.GetPartData("hero");
			if (partData == null)
			{
				GameCore.LogErr("Unable to create part for hero.{0} - data id 'hero' not found in game content", PlayerNumber);
			}
			else
			{
				if (MyShip == null)
				{
					GameCore.LogErr("MyShip si null (probably save game is corupted). Create default ship.");
					CreateDefaultShip();
				}
				PartData data = MyShip.PartMap.CreateModifiedData(partData, $"Hero_{PlayerNumber}");
				MyObject = GameObject.CreateByPartData(data, null, null, position, Vector2.One, 0f, 0, null, bCreateRigidBody: true, layer2);
				if (MyObject.RigidBody == null)
				{
					GameCore.LogErr("Hero without rigid body");
				}
				ActualizeSideOffsets();
				GameObject childByIdFlat = MyObject.GetChildByIdFlat("Body");
				if (childByIdFlat != null)
				{
					GameObject childByIdFlat2 = childByIdFlat.GetChildByIdFlat("Nose");
					if (childByIdFlat2 != null)
					{
						float num = childByIdFlat2.Position.X + (float)childByIdFlat2.FrameRectangle.Width - childByIdFlat2.FramePivot.X;
						childByIdFlat.Position.X -= (childByIdFlat.FramePivot.X + num) / 2f - childByIdFlat.FramePivot.X;
					}
				}
				if (transformation != null)
				{
					MyObject.SetTransformation(transformation);
					MyObject.SetVelocity(velocity);
					MyObject.Timer = timer;
				}
				MyObject.IsHero = true;
				MyObject.MyHero = this;
			}
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to create hero main part: {0}", ex);
			MyObject = new GameObject();
			MyObject.IsHero = true;
		}
		InitSlots();
		try
		{
			InitSmallShip();
		}
		catch (Exception ex2)
		{
			GameCore.LogErr("InitSmallShip exception: {0}", ex2);
		}
		_shellOutPart = MyObject.GetChildByIdDeep("shell_out");
		if (_shellOutPart == null)
		{
			_shellOutPart = new GameObject();
			MyObject.AddChild(_shellOutPart);
			GameCore.LogErr("Undefined shell_out part for hero");
		}
		GameObject childByIdDeep = MyObject.GetChildByIdDeep("jet");
		childByIdDeep?.CreateChildByPartData("hero_jet_sensor", createPhysBody: true).SetRotation(0f - childByIdDeep.Rotation);
		MyObject.MyHero = this;
		List<GameObject> partList = MyObject.GetPartList();
		foreach (GameObject item in partList)
		{
			item.MyHero = this;
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(partList);
		MyObject.Init();
		MyObject.LevelInit();
		MyObject.Activate();
		MyObject.ActiveMode = ActiveModeEnum.AlwaysActive;
		MyObject.OnHitBegin += DoOnHitBegin;
		MyObject.OnHit += OnHit;
		MyObject.OnKill += OnKill;
		UpdateState(3);
		RebuildShipConfiguration();
	}

	private void CreateItemSetEffects()
	{
		if (MyObject != null)
		{
			GameObject childByIdDeep;
			while ((childByIdDeep = MyObject.GetChildByIdDeep("hero_reindeer")) != null && !childByIdDeep.IsDestroyed)
			{
				childByIdDeep.Destroy();
			}
			if (IsItemSetEnabled("xmas"))
			{
				(MyObject.GetChildByIdDeep("point_nose_tip") ?? MyObject.GetChildByIdDeep("nose") ?? MyObject).CreateChildByPartData("hero_reindeer", createPhysBody: true).MyHero = this;
				MaxSpeedFactor += 0.1f;
			}
			while ((childByIdDeep = MyObject.GetChildByIdDeep("kaiser_turret")) != null && !childByIdDeep.IsDestroyed)
			{
				childByIdDeep.Destroy();
			}
			if (IsItemSetEnabled("kaiser"))
			{
				MyObject.CreateChildByPartData("kaiser_turret", createPhysBody: true).MyHero = this;
			}
		}
	}

	public void InitSmallShip()
	{
		PartOverheated = MyObject.GetChildByIdDeep("overheat");
		PartOverheated.IsEnabled = false;
		PartBurning = MyObject.GetChildByIdDeep("burning");
		PartBurning.IsEnabled = false;
		PartHpCritical = MyObject.GetChildByIdDeep("hp_critical");
		MyObject.HitOnCollision = false;
	}

	public void DestroySmallShip()
	{
		if (MyObject != null)
		{
			MyObject.Destroy();
			MyObject = null;
		}
		foreach (HeroSlot value in MyShip.Slots.Values)
		{
			value.DestroyMyObject();
		}
		if (JetPackObject != null)
		{
			JetPackObject.Destroy();
		}
	}

	public List<HeroItem> GetSlotItemList(HeroItemData data)
	{
		List<HeroItem> list = new List<HeroItem>();
		if (data != null)
		{
			for (HeroSlotEnum heroSlotEnum = HeroSlotEnum.front_gun; heroSlotEnum < HeroSlotEnum.MaxSlotCount; heroSlotEnum++)
			{
				HeroItem item = MyShip.Slots[heroSlotEnum].Item;
				if (item != null && item.Data == data)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public void DoOnShotStart(GameObject go)
	{
		ProcessObject(go);
		OnShotStart?.Invoke(go);
	}

	public void DoOnFieldCollision(GameObject go, Vector2 contactPoint, Vector2 normal)
	{
		if (!FieldObjectList.Contains(go))
		{
			FieldObjectList.Add(go);
			OnFieldCollision?.Invoke(go, contactPoint, normal);
		}
	}

	public void DoOnFieldSeparation(GameObject go)
	{
		if (FieldObjectList.Remove(go))
		{
			OnFieldSeparation?.Invoke(go);
		}
	}

	public void DoOnBombStart(GameObject go)
	{
		try
		{
			ProcessObject(go);
			if (SuperchargedBombProbability > 0 && SuperchargedBombProbability >= Utils.GetRandom(1, 100))
			{
				_behSuperchargedBomb.SetParam("damage", new float[1] { GetTotalDPS() * SuperchargedBombTotalDPSFactor });
				BehaviorScript behaviorScript = go.AddBehaviorScript(_behSuperchargedBomb);
				behaviorScript.Init();
				behaviorScript.RegisterEvents();
			}
			OnBombStart?.Invoke(go);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnBombStart exception: {0}", ex);
		}
	}

	public void InitSlots()
	{
		if (MyObject == null)
		{
			GameCore.LogErr("Hero init slots failed - No MyObject");
			return;
		}
		Vector2 vector = MyObject.GetChildByIdDeep("point_front_gun")?.GetPositionRelativeToMain() ?? new Vector2(30f, 0f);
		foreach (HeroSlot value in MyShip.Slots.Values)
		{
			Vector2 vector2;
			switch (value.SlotType)
			{
			case HeroSlotEnum.front_gun:
			case HeroSlotEnum.top_gun:
			case HeroSlotEnum.bottom_gun:
				vector2 = vector;
				break;
			case HeroSlotEnum.rear_gun:
				vector2 = MyObject.GetChildByIdDeep("point_rear_gun")?.GetPositionRelativeToMain() ?? new Vector2(-60f, -15f);
				break;
			default:
				vector2 = Vector2.Zero;
				break;
			}
			value.Init();
			value.Position = vector2 + value.Offset;
		}
	}

	public HeroSlot GetSlot(HeroSlotEnum type)
	{
		return MyShip.GetSlot(type);
	}

	public Vector2 GetTargetVelocityByInput()
	{
		Vector2 vector = default;
		Vector2 leftStickPos = GameInput.GetLeftStickPos(PlayerNumber);
		bool flag = false;
		if (GameInput.KeyDown(GameInput.KeyTypeEnum.MoveLeft, PlayerNumber))
		{
			vector.X = 0f - MaxSpeed;
		}
		else if (GameInput.KeyDown(GameInput.KeyTypeEnum.MoveRight, PlayerNumber))
		{
			vector.X = MaxSpeed;
		}
		else if (Options.PlayerController[PlayerNumber] == GameInput.PlayerControllerEnum.Mouse)
		{
			float num = 50f - (float)Options.MouseSensitivity / 2.1f;
			float num2 = 0.2f - 0.2f * ((float)Options.MouseSensitivity / 100f);
			if (GameInput.MouseMoveDelta.X < 0f - num2 || GameInput.MouseMoveDelta.X > num2)
			{
				leftStickPos.X = (float)((!(GameInput.MouseMoveDelta.X < 0f)) ? 1 : (-1)) * Math.Min(Math.Abs(GameInput.MouseMoveDelta.X), num) / num;
			}
			if (GameInput.MouseMoveDelta.Y < 0f - num2 || GameInput.MouseMoveDelta.Y > num2)
			{
				leftStickPos.Y = (float)((GameInput.MouseMoveDelta.Y < 0f) ? 1 : (-1)) * Math.Min(Math.Abs(GameInput.MouseMoveDelta.Y), num) / num;
			}
		}
		if (GameInput.KeyDown(GameInput.KeyTypeEnum.MoveUp, PlayerNumber))
		{
			vector.Y = 0f - MaxSpeed;
		}
		else if (GameInput.KeyDown(GameInput.KeyTypeEnum.MoveDown, PlayerNumber))
		{
			vector.Y = MaxSpeed;
		}
		if (leftStickPos.X != 0f)
		{
			vector.X = MaxSpeed * leftStickPos.X;
			flag = true;
			if (leftStickPos.X < 0f)
			{
				_keyDownArray[6] = true;
			}
			else
			{
				_keyDownArray[7] = true;
			}
		}
		if (leftStickPos.Y != 0f)
		{
			vector.Y = MaxSpeed * (0f - leftStickPos.Y);
			flag = true;
			if (leftStickPos.Y < 0f)
			{
				_keyDownArray[4] = true;
			}
			else
			{
				_keyDownArray[5] = true;
			}
		}
		Vector2 vector2 = vector;
		if (vector != Vector2.Zero && !flag)
		{
			vector = Vector2.Normalize(vector) * Math.Max(Math.Abs(vector.X), Math.Abs(vector.Y));
		}
		vector *= new Vector2(1.3f, 1f);
		SpeedRatio = vector / vector2;
		if (float.IsNaN(SpeedRatio.X))
		{
			SpeedRatio.X = 1f;
		}
		if (float.IsNaN(SpeedRatio.Y))
		{
			SpeedRatio.Y = 1f;
		}
		return vector;
	}

	private void UpdateInput()
	{
		for (int i = 0; i < _keyDownArray.Length; i++)
		{
			_keyDownArray[i] = GameInput.KeyDown((GameInput.KeyTypeEnum)i, PlayerNumber);
			_keyPressedArray[i] = GameInput.KeyPressed((GameInput.KeyTypeEnum)i, PlayerNumber);
		}
		if (MouseGun != null && MouseGun.IsGunTriggerPressed)
		{
			_keyDownArray[0] = true;
		}
	}

	private void ParseInput(float elapsedTime)
	{
		if (MyObject == null || MyObject.RigidBody == null || MyObject.IsKilled)
		{
			return;
		}
		if (AutopilotMode != HeroAutopilotMode.None)
		{
			MyObject.IgnoreCollisionMode = 1;
		}
		else
		{
			MyObject.IgnoreCollisionMode = 0;
		}
		if (GameCore.IsEditorEnabled)
		{
			MyObject.SetVelocity(Vector2.Zero);
			return;
		}
		if (GameCore.IsShopMenuEnabled)
		{
			UpdateShopAutopilot(elapsedTime);
			return;
		}
		if (GameCore.IsMainMenuEnabled)
		{
			UpdateMainMenuAutopilot(elapsedTime);
			return;
		}
		if (MyObject.MyLevel.CutsceneMode != 0)
		{
			UpdateCutSceneAutopilot(elapsedTime);
			return;
		}
		if (AutopilotMode == HeroAutopilotMode.BlackHole)
		{
			UpdateBlackHoleAutopilot(elapsedTime);
			return;
		}
		if (AutopilotMode != HeroAutopilotMode.None)
		{
			UpdateLevelAutopilot(elapsedTime);
			if (AutopilotMode != HeroAutopilotMode.None)
			{
				return;
			}
		}
		if (GameCore.IsInLevelMenuEnabled || GameCore.IsPaused)
		{
			return;
		}
		UpdateInput();
		if (!IsJetPackActive)
		{
			if (GameJnG.IsTimeCompressorEnabled)
			{
				UpdateVelocity(GetTargetVelocityByInput() / GameCore.TimeFactor, elapsedTime / GameCore.TimeFactor);
			}
			else
			{
				UpdateVelocity(GetTargetVelocityByInput(), elapsedTime);
			}
		}
		else
		{
			_autopilotStateTimerY += elapsedTime * 4f;
			_autopilotVelocity.X = 0f;
			_autopilotVelocity.Y = Utils.Sin(_autopilotStateTimerY) * 100f;
			UpdateVelocity(_autopilotVelocity, elapsedTime);
		}
	}

	private void UpdateVelocity(Vector2 targetVelocity, float elapsedTime)
	{
		float num = Acceleration;
		if (GameJnG.IsTimeCompressorEnabled)
		{
			num /= GameCore.TimeFactor;
		}
		if (IsShrinkerEnabled)
		{
			if (MyObject.Scale.X > 0.5f)
			{
				float currentValue = MyObject.Scale.X;
				Utils.ChangeValueToTarget(ref currentValue, 0.5f, 2f * elapsedTime);
				MyObject.SetScale(currentValue);
			}
			targetVelocity *= 1.3f;
			num *= 1.5f;
		}
		else if (MyObject.Scale.X < 1f)
		{
			float currentValue2 = MyObject.Scale.X;
			Utils.ChangeValueToTarget(ref currentValue2, 1f, 2f * elapsedTime);
			MyObject.SetScale(currentValue2);
		}
		targetVelocity.X += GameCore.CurrentLevel.ScrollSpeed.X;
		Vector2 velocity = MyObject.Velocity;
		Utils.ChangeValueToTarget(ref velocity.X, targetVelocity.X, elapsedTime * ((targetVelocity.X != 0f) ? num : (num * 2f)));
		Utils.ChangeValueToTarget(ref velocity.Y, targetVelocity.Y, elapsedTime * ((targetVelocity.Y != 0f) ? num : (num * 2f)));
		MyObject.SetVelocity(velocity);
	}

	public void UpdateShopFire(float elapsedTime)
	{
		if (IsOverheated || (IsBurning && HeatPercent >= 2f) || GetTotalHeatPS() <= 0f)
		{
			_autopilotFireWaitToZeroHeat = true;
		}
		if (CurrentBombCount <= 0f)
		{
			_autopilotBombWaitToFullRound = true;
		}
		if (!_autopilotFireWaitToZeroHeat || Heat <= 0f)
		{
			_autopilotFireWaitToZeroHeat = false;
			SetKeyDown(GameInput.KeyTypeEnum.FireGuns, bIsDown: true);
		}
		if (!_autopilotBombWaitToFullRound || CurrentBombCount >= (float)MaxBombCount)
		{
			_autopilotBombWaitToFullRound = false;
			SetKeyDown(GameInput.KeyTypeEnum.FireBombs, bIsDown: true);
		}
	}

	public void StartAutopilotMainMenu()
	{
		AutopilotMode = HeroAutopilotMode.MainMenu;
		AutopilotStateX = HeroAutopilotStateXEnum.Wait;
		AutopilotStateY = HeroAutopilotStateYEnum.None;
		_autopilotStateTimerX = 0.1f;
		_autopilotAngle = 0f;
		_autoPilotAngleSpeed = 0f;
		if (MyObject == null || MyObject.IsDestroyed)
		{
			RecreateSmallShip = true;
		}
	}

	public void StartAutopilotLevelIncoming(Layer layer)
	{
		AutopilotMode = HeroAutopilotMode.LevelIncomming;
		AutopilotStateX = HeroAutopilotStateXEnum.MoveRight;
		AutopilotStateY = HeroAutopilotStateYEnum.None;
		DestroySmallShip();
		if (layer != null)
		{
			Vector2 vector;
			if (GameJnG.PlayerCount <= 1)
			{
				vector = new Vector2(-250f, 0f);
			}
			else
			{
				vector = ((PlayerNumber != 0) ? new Vector2(-300f, -30f) : new Vector2(-250f, 30f));
			}
			CreateSmallShip(layer, layer.MyLevel.GetNearestSafePosition(layer.MyLevel.Camera.Position.X) + vector);
		}
		MyObject.SetPhysClass("none");
	}

	public void StartAutopilotCutScene(Layer layer)
	{
		AutopilotMode = HeroAutopilotMode.LevelIncomming;
		AutopilotStateX = HeroAutopilotStateXEnum.MoveRight;
		AutopilotStateY = HeroAutopilotStateYEnum.None;
		DestroySmallShip();
		if (layer != null)
		{
			Vector2 vector;
			if (GameJnG.PlayerCount <= 1)
			{
				vector = new Vector2(-250f, 0f);
			}
			else
			{
				vector = ((PlayerNumber != 0) ? new Vector2(-300f, -30f) : new Vector2(-250f, 30f));
			}
			CreateSmallShip(layer, layer.MyLevel.GetNearestSafePosition(layer.MyLevel.Camera.Position.X) + vector);
		}
		MyObject.SetPhysClass("none");
	}

	public void StarAutopilotLevelFlyOut()
	{
		AutopilotMode = HeroAutopilotMode.LevelFlyOut;
		AutopilotStateX = HeroAutopilotStateXEnum.None;
		AutopilotStateY = HeroAutopilotStateYEnum.ToSafeLine;
		if (MyObject != null)
		{
			AutopilotStartPos = MyObject.WorldPosition;
			MyObject.SetPhysClass("none");
		}
	}

	public void StarAutopilotBlackHole()
	{
		AutopilotMode = HeroAutopilotMode.BlackHole;
		AutopilotStateX = HeroAutopilotStateXEnum.None;
		AutopilotStateY = HeroAutopilotStateYEnum.ToSafeLine;
		_autopilotStateTimerX = 1f;
		if (MyObject != null)
		{
			AutopilotStartPos = MyObject.WorldPosition;
			MyObject.SetPhysClass("none");
		}
	}

	public void StartAutopilotShopMenu()
	{
		AutopilotMode = HeroAutopilotMode.ShopMenuPreview;
		AutopilotStateX = HeroAutopilotStateXEnum.Wait;
		AutopilotStateY = HeroAutopilotStateYEnum.Wait;
		_autopilotStateTimerX = 1f;
		_autopilotStateTimerY = 0.5f;
	}

	public void StartAutopilotFlyOutMainMenu()
	{
		if (MyObject != null)
		{
			MyObject.SetVelocity(MyObject.MyLayer.ScrollSpeed.X, 0f);
			if (AutopilotStateX != HeroAutopilotStateXEnum.FlyOutOfGame)
			{
				AutopilotStateX = HeroAutopilotStateXEnum.FlyOutOfGame;
				AutopilotStateY = HeroAutopilotStateYEnum.Wait;
			}
		}
	}

	public void CancelAutopilot()
	{
		MyObject?.SetPhysClass("hero");
		AutopilotStateX = HeroAutopilotStateXEnum.None;
		AutopilotStateY = HeroAutopilotStateYEnum.None;
		AutopilotMode = HeroAutopilotMode.None;
	}

	public void UpdateLevelAutopilot(float elapsedTime)
	{
		Vector2 position = MyObject.Position;
		if (AutopilotMode == HeroAutopilotMode.LevelFlyOut)
		{
			if (AutopilotStateY == HeroAutopilotStateYEnum.ToSafeLine)
			{
				Vector2 nearestSafePosition = MyObject.MyLevel.GetNearestSafePosition(MyObject.WorldPosition.X);
				MyObject.SetVelocity(MyObject.MyLevel.ScrollSpeed + Utils.GetVelocityVector2(MaxSpeed * 0.75f, Utils.GetRotationTo(MyObject.WorldPosition, nearestSafePosition)));
				if ((AutopilotStartPos.Y <= nearestSafePosition.Y && MyObject.WorldPosition.Y >= nearestSafePosition.Y) || (AutopilotStartPos.Y >= nearestSafePosition.Y && MyObject.WorldPosition.Y <= nearestSafePosition.Y))
				{
					AutopilotStateY = HeroAutopilotStateYEnum.Wait;
					AutopilotStartPos = MyObject.WorldPosition;
					_autoPilotAngleSpeed = MyObject.Velocity.X;
				}
			}
			else
			{
				Utils.ChangeValueToTarget(ref _autoPilotAngleSpeed, 4000f, 2000f * elapsedTime);
				if (MyObject.WorldPosition.X > MyObject.MyCamera.Position.X + 1920f + 500f)
				{
					MyObject.SetPosition(MyObject.MyCamera.Position.X + 1920f + 500f, MyObject.WorldPosition.Y);
					MyObject.SetVelocity(0f, 0f);
				}
				else
				{
					MyObject.SetVelocity(_autoPilotAngleSpeed, 0f);
				}
			}
		}
		else
		{
			if (AutopilotMode != HeroAutopilotMode.LevelIncomming || AutopilotStateX != HeroAutopilotStateXEnum.MoveRight)
			{
				return;
			}
			float y = MyObject.MyLevel.GetNearestSafePosition(MyObject.WorldPosition.X).Y;
			Vector2 nearestSafePosition = new Vector2(MyObject.MyCamera.Position.X + 500f, y);
			if (GameJnG.PlayerCount > 1 && PlayerNumber > 0)
			{
				nearestSafePosition.X -= 50f;
			}
			float num = (nearestSafePosition - position).Length();
			float num2 = Math.Min(3000f, Math.Max(0f, num * 5f));
			MyObject.SetVelocity(MyObject.MyLevel.ScrollSpeed.X + num2, 0f);
			if (position.X >= nearestSafePosition.X || num2 <= 0f)
			{
				CancelAutopilot();
			}
			else
			{
				if (MyObject.MyLevel.CutsceneMode != 0 || GameCore.IsInLevelMenuEnabled || GameCore.IsPaused)
				{
					return;
				}
				if (position.X > MyObject.MyCamera.Position.X + 50f)
				{
					UpdateInput();
					if (KeyDown(GameInput.KeyTypeEnum.FireGuns))
					{
						_keyDownArray[0] = true;
					}
					else
					{
						_keyDownArray[0] = false;
					}
					if (KeyDown(GameInput.KeyTypeEnum.FireBombs))
					{
						_keyDownArray[1] = true;
					}
					else
					{
						_keyDownArray[1] = false;
					}
				}
				Vector2 vector = GameInput.GetLeftStickPos(PlayerNumber);
				if (vector == Vector2.Zero && Options.PlayerController[PlayerNumber] == GameInput.PlayerControllerEnum.Mouse)
				{
					vector = GameInput.MouseMoveDelta;
				}
				if (position.X > MyObject.MyCamera.Position.X + 200f && (vector.X != 0f || vector.Y != 0f || GameInput.KeyDown(GameInput.KeyTypeEnum.MoveRight) || GameInput.KeyDown(GameInput.KeyTypeEnum.MoveLeft) || GameInput.KeyDown(GameInput.KeyTypeEnum.MoveUp) || GameInput.KeyDown(GameInput.KeyTypeEnum.MoveDown) || GameInput.KeyDown(GameInput.KeyTypeEnum.PrimaryDevice) || GameInput.KeyDown(GameInput.KeyTypeEnum.SecondaryDevice)))
				{
					CancelAutopilot();
					MyObject.SetVelocity(MyObject.MyLevel.ScrollSpeed);
				}
			}
		}
	}

	private Vector2 GetTargetMainMenuPosition()
	{
		Vector2 vector = new Vector2(MyObject.MyCamera.Position.X + 960f, 580f);
		if (GameJnG.PlayerCount > 0)
		{
			if (PlayerNumber == 0)
			{
				return vector + new Vector2(30f, 20f);
			}
			return vector + new Vector2(-30f, -20f);
		}
		return vector;
	}

	public void UpdateCutSceneAutopilot(float elapsedTime)
	{
		Vector2 currentValue = MyObject.Position;
		_ = PlayerNumber;
		Vector2 vector = ((MyObject.MyLevel.CutsceneMode != 9) ? GetTargetMainMenuPosition() : new Vector2(MyObject.MyCamera.Position.X + 640f, 520f));
		switch (AutopilotStateX)
		{
		case HeroAutopilotStateXEnum.MoveRight:
		{
			float num = (vector - currentValue).Length();
			float num2 = Math.Min(2000f, Math.Max(50f, num * 3f));
			num2 += MyObject.MyLayer.ScrollSpeed.X;
			if (Utils.ChangeValueToTarget(ref currentValue, vector, num2 * elapsedTime))
			{
				AutopilotStateX = HeroAutopilotStateXEnum.OnPosition;
				_autopilotAngle = 0f;
				_autoPilotAngleSpeed = 0f;
				MyObject.SetVelocity(0f, 0f);
			}
			else
			{
				MyObject.SetVelocity(num2, 0f);
			}
			MyObject.SetPosition(currentValue);
			break;
		}
		case HeroAutopilotStateXEnum.OnPosition:
			MyObject.SetPosition(vector + new Vector2(0f, (float)(30 - PlayerNumber * 7) * Utils.Sin(_autopilotAngle)));
			Utils.ChangeValueToTarget(ref _autoPilotAngleSpeed, 2f, 2f * elapsedTime);
			_autopilotAngle += (_autoPilotAngleSpeed - (float)PlayerNumber * 0f) * elapsedTime;
			break;
		}
	}

	public void UpdateMainMenuAutopilot(float elapsedTime)
	{
		Vector2 currentValue = MyObject.Position;
		float y = 580f - (float)(PlayerNumber * 50);
		switch (AutopilotStateX)
		{
		case HeroAutopilotStateXEnum.Wait:
			if (Utils.ChangeValueToTarget(ref _autopilotStateTimerX, 0f, elapsedTime))
			{
				AutopilotStateX = HeroAutopilotStateXEnum.MoveRight;
			}
			MyObject.SetPosition(MyObject.MyCamera.Position.X - 300f, y);
			break;
		case HeroAutopilotStateXEnum.MoveRight:
		{
			Vector2 targetMainMenuPosition = GetTargetMainMenuPosition();
			float num = (targetMainMenuPosition - currentValue).Length();
			float num2 = Math.Min(2000f, Math.Max(50f, num * 3f));
			num2 += MyObject.MyLayer.ScrollSpeed.X;
			if (Utils.ChangeValueToTarget(ref currentValue, targetMainMenuPosition, num2 * elapsedTime))
			{
				AutopilotStateX = HeroAutopilotStateXEnum.OnPosition;
				_autopilotAngle = 0f;
				_autoPilotAngleSpeed = 0f;
				MyObject.SetVelocity(0f, 0f);
			}
			else
			{
				MyObject.SetVelocity(num2, 0f);
			}
			MyObject.SetPosition(currentValue);
			break;
		}
		case HeroAutopilotStateXEnum.OnPosition:
			MyObject.SetPosition(GetTargetMainMenuPosition() + new Vector2(0f, (float)(30 - PlayerNumber * 7) * Utils.Sin(_autopilotAngle)));
			Utils.ChangeValueToTarget(ref _autoPilotAngleSpeed, 2f, 2f * elapsedTime);
			_autopilotAngle += (_autoPilotAngleSpeed - (float)PlayerNumber * 0f) * elapsedTime;
			break;
		case HeroAutopilotStateXEnum.FlyOutOfGame:
			if (MyObject.WorldPosition.X > MyObject.MyCamera.Position.X + 1920f + 500f)
			{
				MyObject.SetVelocity(0f, 0f);
				AutopilotStateX = HeroAutopilotStateXEnum.OutOfGame;
			}
			else
			{
				float num2 = MyObject.Velocity.X;
				Utils.ChangeValueToTarget(ref num2, 3000f, 3000f * elapsedTime);
				MyObject.SetVelocity(num2, 0f);
			}
			break;
		case HeroAutopilotStateXEnum.OutOfGame:
			if (GameJnG.MainMenu.Credits.State != CreditsStateEnum.Visible && (GameJnG.MainMenu.ScreenLeaderboard.State == ScreenLeaderBoardStateEnum.Hidden || GameJnG.MainMenu.ScreenLeaderboard.State == ScreenLeaderBoardStateEnum.FadeOut) && (GameJnG.MainMenu.ProfileSelector.State == ProfileSelectorStateEnum.Hidden || GameJnG.MainMenu.ProfileSelector.State == ProfileSelectorStateEnum.FadeOut))
			{
				StartAutopilotMainMenu();
			}
			else
			{
				MyObject.SetPosition(MyObject.MyCamera.Position.X - 300f, y);
			}
			break;
		case HeroAutopilotStateXEnum.MoveLeft:
			break;
		}
	}

	public void UpdateShopAutopilot(float elapsedTime)
	{
		_ = MyObject.Velocity;
		float y = MyObject.MyLevel.GetNearestSafePosition(MyObject.WorldPosition.X).Y;
		if (AutopilotMode == HeroAutopilotMode.ShopMenuProfile)
		{
			Vector2 vector = new Vector2(MyObject.MyCamera.Position.X + 960f - 90f, y);
			if (this != GameJnG.ShopMenu.MyHero || (GameJnG.ShopMenu.SwapToHero != null && this != GameJnG.ShopMenu.SwapToHero))
			{
				vector = new Vector2(MyObject.MyCamera.Position.X + 960f - 200f, y - 500f);
				_autopilotSwap = true;
			}
			Vector2 currentValue = MyObject.Position;
			float num = (vector - currentValue).Length();
			float num2 = Math.Max(MaxSpeed / 1.5f, Math.Min(MaxSpeed, num * 3f));
			Utils.ChangeValueToTarget(ref currentValue, vector, num2 * elapsedTime);
			if (!_autopilotSwap)
			{
				_autopilotAngle -= elapsedTime * 3f;
				vector.Y += Utils.Sin(_autopilotAngle) * 20f + 30f;
				MyObject.SetPosition(vector);
				MyObject.SetVelocity(new Vector2(0f, Utils.Sin(_autopilotAngle) * MaxSpeed * 1.5f));
			}
			else
			{
				_autopilotAngle = 0f;
				_autopilotVelocity = Utils.GetVelocityVector2(num2, Utils.GetRotationTo(currentValue, vector));
				if (Utils.GetDistance(currentValue, vector) < 1f)
				{
					_autopilotSwap = false;
				}
			}
			_autopilotStateTimerX = 0f;
			_autopilotStateTimerY = 0f;
			AutopilotStateX = HeroAutopilotStateXEnum.Wait;
			AutopilotStateY = HeroAutopilotStateYEnum.Wait;
		}
		else
		{
			Vector2 vector2 = new Vector2(300f, 200f);
			Vector2 vector3 = new Vector2(860f, y);
			float acceleration = Acceleration;
			_autopilotAngle = 0f;
			switch (AutopilotStateX)
			{
			case HeroAutopilotStateXEnum.Wait:
				if (Utils.ChangeValueToTarget(ref _autopilotStateTimerX, 0f, elapsedTime))
				{
					_autopilotStateTimerX = Utils.GetRandom(0.2f, 1.5f);
					if (MyObject.VirtualScreenPosition.X > vector3.X + vector2.X / 2f)
					{
						AutopilotStateX = HeroAutopilotStateXEnum.MoveLeft;
					}
					else if (MyObject.VirtualScreenPosition.X < vector3.X - vector2.X / 2f)
					{
						AutopilotStateX = HeroAutopilotStateXEnum.MoveRight;
					}
					else if (Utils.GetRandom(1) == 0)
					{
						AutopilotStateX = HeroAutopilotStateXEnum.MoveRight;
					}
					else
					{
						AutopilotStateX = HeroAutopilotStateXEnum.MoveLeft;
					}
				}
				break;
			case HeroAutopilotStateXEnum.MoveRight:
				if (MyObject.VirtualScreenPosition.X >= vector3.X + vector2.X || Utils.ChangeValueToTarget(ref _autopilotStateTimerX, 0f, elapsedTime))
				{
					if (Utils.ChangeValueToTarget(ref _autopilotVelocity.X, 0f, acceleration * elapsedTime))
					{
						AutopilotStateX = HeroAutopilotStateXEnum.Wait;
						_autopilotStateTimerX = Utils.GetRandom(0.25f, 3f);
					}
				}
				else
				{
					Utils.ChangeValueToTarget(ref _autopilotVelocity.X, MaxSpeed, acceleration * elapsedTime);
				}
				break;
			case HeroAutopilotStateXEnum.MoveLeft:
				if (MyObject.VirtualScreenPosition.X < vector3.X - vector2.X || Utils.ChangeValueToTarget(ref _autopilotStateTimerX, 0f, elapsedTime))
				{
					if (Utils.ChangeValueToTarget(ref _autopilotVelocity.X, 0f, Acceleration * elapsedTime))
					{
						AutopilotStateX = HeroAutopilotStateXEnum.Wait;
						_autopilotStateTimerX = Utils.GetRandom(0.25f, 3f);
					}
				}
				else
				{
					Utils.ChangeValueToTarget(ref _autopilotVelocity.X, 0f - MaxSpeed, acceleration * elapsedTime);
				}
				break;
			}
			switch (AutopilotStateY)
			{
			case HeroAutopilotStateYEnum.Wait:
				if (Utils.ChangeValueToTarget(ref _autopilotStateTimerY, 0f, elapsedTime))
				{
					_autopilotStateTimerY = Utils.GetRandom(0.2f, 0.75f);
					if (MyObject.WorldPosition.Y > vector3.Y + vector2.Y / 2f)
					{
						AutopilotStateY = HeroAutopilotStateYEnum.MoveUp;
					}
					else if (MyObject.WorldPosition.Y < vector3.Y - vector2.Y / 2f)
					{
						AutopilotStateY = HeroAutopilotStateYEnum.MoveDown;
					}
					else if (Utils.GetRandom(1) == 0)
					{
						AutopilotStateY = HeroAutopilotStateYEnum.MoveUp;
					}
					else
					{
						AutopilotStateY = HeroAutopilotStateYEnum.MoveDown;
					}
				}
				break;
			case HeroAutopilotStateYEnum.MoveDown:
				if (MyObject.WorldPosition.Y > vector3.Y + vector2.Y || Utils.ChangeValueToTarget(ref _autopilotStateTimerY, 0f, elapsedTime))
				{
					if (Utils.ChangeValueToTarget(ref _autopilotVelocity.Y, 0f, acceleration * elapsedTime))
					{
						AutopilotStateY = HeroAutopilotStateYEnum.Wait;
						_autopilotStateTimerY = Utils.GetRandom(0.1f, 1f);
					}
				}
				else
				{
					Utils.ChangeValueToTarget(ref _autopilotVelocity.Y, MaxSpeed, acceleration * elapsedTime);
				}
				break;
			case HeroAutopilotStateYEnum.MoveUp:
				if (MyObject.WorldPosition.Y < vector3.Y - vector2.Y || Utils.ChangeValueToTarget(ref _autopilotStateTimerY, 0f, elapsedTime))
				{
					if (Utils.ChangeValueToTarget(ref _autopilotVelocity.Y, 0f, acceleration * elapsedTime))
					{
						AutopilotStateY = HeroAutopilotStateYEnum.Wait;
						_autopilotStateTimerY = Utils.GetRandom(0.1f, 1f);
					}
				}
				else
				{
					Utils.ChangeValueToTarget(ref _autopilotVelocity.Y, 0f - MaxSpeed, acceleration * elapsedTime);
				}
				break;
			}
		}
		if (AutopilotMode != HeroAutopilotMode.ShopMenuProfile || this == GameJnG.ShopMenu.MyHero)
		{
			UpdateShopFire(elapsedTime);
		}
		UpdateVelocity(_autopilotVelocity, elapsedTime);
	}

	public void UpdateBlackHoleAutopilot(float elapsedTime)
	{
		if (AutopilotStateY == HeroAutopilotStateYEnum.ToSafeLine)
		{
			Vector2 nearestSafePosition = MyObject.MyLevel.GetNearestSafePosition(MyObject.MyCamera.Position.X + 960f);
			MyObject.SetVelocity(MyObject.MyLevel.ScrollSpeed + Utils.GetVelocityVector2(MaxSpeed * 0.75f, Utils.GetRotationTo(MyObject.WorldPosition, nearestSafePosition)));
			if ((AutopilotStartPos.Y <= nearestSafePosition.Y && MyObject.WorldPosition.Y >= nearestSafePosition.Y) || (AutopilotStartPos.Y >= nearestSafePosition.Y && MyObject.WorldPosition.Y <= nearestSafePosition.Y))
			{
				AutopilotStateY = HeroAutopilotStateYEnum.Wait;
				AutopilotStartPos = MyObject.WorldPosition;
				_autopilotVelocity = MyObject.Scale;
				MyObject.MyLevel.AddWarningText(TextBundle.GetText("hud_warning_mission_completed"), new Color(255, 255, 0));
			}
			Utils.ChangeValueToTarget(ref _autopilotStateTimerX, 0.01f, 0.2f * elapsedTime);
			MyObject.SetScale(_autopilotStateTimerX * _autopilotStateTimerX);
		}
		else
		{
			Utils.ChangeValueToTarget(ref _autopilotStateTimerX, 0.01f, 0.5f * elapsedTime);
			MyObject.SetVelocity(MyObject.MyLevel.ScrollSpeed);
			MyObject.SetScale(_autopilotStateTimerX * _autopilotStateTimerX);
			UpdateState((int)(2.5f * _autopilotStateTimerX));
		}
	}

	public void AddHeat(float heat)
	{
		if (!NoHeat)
		{
			if (Heat < MaxHeat)
			{
				Heat += heat * HeatFactor;
			}
			else
			{
				Heat += heat * MaxHeat / Heat;
			}
		}
		if (!IsElDiabloEnabled && Heat >= MaxHeat)
		{
			Heat = MaxHeat;
		}
		else if (IsElDiabloEnabled && Heat >= MaxBurningHeat)
		{
			Heat = MaxBurningHeat;
			GameJnG.UnlockAchievement(GameAchievementEnum.ACH_HEAT_MAX);
		}
		UpdateHeat(0f);
	}

	public float RemoveHeat(float heat)
	{
		Heat -= heat;
		float heat2 = Heat;
		Heat = Math.Max(Heat, 0f);
		UpdateHeat(0f);
		return heat2;
	}

	public void AddMana(float value, Vector2 position, bool bTextParticle)
	{
		Mana += value;
		if (Mana >= MaxMana)
		{
			Mana = MaxMana;
		}
		if (bTextParticle)
		{
			AddManaTextParticle(value, position);
		}
	}

	public void UseMana(float value)
	{
		if (!Cheat.FullMana && !GameJnG.IsTestRangeEnabled)
		{
			Mana -= value;
		}
		if (Mana >= MaxMana)
		{
			Mana = MaxMana;
		}
		else if (Mana < 0f)
		{
			Mana = 0f;
		}
	}

	public void SetShip(HeroShip ship)
	{
		MyShip = ship;
		ship.MyHero = this;
	}

	public void CreateDefaultSlotItems()
	{
		HeroItemData heroItemData = GameCore.GetHeroItemData("weapon/start");
		if (heroItemData != null)
		{
			HeroItem item = new HeroItem(heroItemData);
			MyShip.GetSlot(HeroSlotEnum.front_gun).SetItem(item, bReconfigurateShip: false);
			item = new HeroItem(heroItemData);
			MyShip.GetSlot(HeroSlotEnum.bottom_gun).SetItem(item, bReconfigurateShip: false);
			if (GameJnG.GameMode == GameModeEnum.Casual)
			{
				item = new HeroItem(heroItemData);
				MyShip.GetSlot(HeroSlotEnum.top_gun).SetItem(item, bReconfigurateShip: false);
			}
		}
		else
		{
			GameCore.LogErr("Default hero weapon item not found");
		}
		heroItemData = GameCore.GetHeroItemData("bomb/start");
		if (heroItemData != null)
		{
			HeroItem item2 = new HeroItem(heroItemData);
			MyShip.GetSlot(HeroSlotEnum.bomb).SetItem(item2, bReconfigurateShip: false);
		}
		else
		{
			GameCore.LogErr("Default hero bomb item not found");
		}
		heroItemData = GameCore.GetHeroItemData("device/energy_barrier");
		if (heroItemData != null)
		{
			HeroItem item3 = new HeroItem(heroItemData);
			MyShip.GetSlot(HeroSlotEnum.primary_device).SetItem(item3, bReconfigurateShip: false);
		}
		else
		{
			GameCore.LogErr("Default hero device item not found");
		}
		MyShip.MyHero.RebuildShipConfiguration();
	}

	public void CreateDefaultShip()
	{
		SetShip(new HeroShip($"ship_{PlayerNumber}"));
		if (PlayerNumber == 0)
		{
			MyShip.PartMap.SkinId = "hot_rod";
			MyShip.PartMap.AddItem("body", "hero_body_1c");
			MyShip.PartMap.AddItem("body/rudder", "hero_rudder_1");
			MyShip.PartMap.AddItem("body/nose", "hero_nose_1");
			MyShip.PartMap.AddItem("body/side_front", "hero_side_front_1");
			MyShip.PartMap.AddItem("body/side_front/wing", "hero_wing_1");
			MyShip.PartMap.AddItem("body/side_back", "hero_side_back_1");
			MyShip.PartMap.AddItem("body/side_back/wing", "hero_wing_1b");
			MyShip.PartMap.AddItem("body/jet", "hero_jet_flames_yellow");
		}
		else
		{
			MyShip.PartMap.SkinId = "shark";
			MyShip.PartMap.AddItem("body", "hero_body_1c");
			MyShip.PartMap.AddItem("body/rudder", "hero_rudder_1");
			MyShip.PartMap.AddItem("body/nose", "hero_nose_1");
			MyShip.PartMap.AddItem("body/side_front", "hero_side_front_1");
			MyShip.PartMap.AddItem("body/side_front/wing", "hero_wing_1");
			MyShip.PartMap.AddItem("body/side_back", "hero_side_back_1");
			MyShip.PartMap.AddItem("body/side_back/wing", "hero_wing_1b");
			MyShip.PartMap.AddItem("body/jet", "hero_jet_flames_blue");
		}
		CreateDefaultSlotItems();
	}

	public void CheckOutOfGameView()
	{
		Vector2 position = GameCore.CurrentLevel.Camera.Position;
		if (MyObject.Position.X + (float)MyObject.VisibilityRect.Left < (float)(int)position.X)
		{
			if (_stuckOnViewEdge++ > 10 && MyObject.Position.X + (float)MyObject.VisibilityRect.Left < (float)((int)position.X - 20))
			{
				if (Cheat.NoDamage)
				{
					Vector2 nearestSafePosition = MyObject.MyLevel.GetNearestSafePosition(MyObject.WorldPosition.X);
					if (nearestSafePosition.Y + (float)MyObject.VisibilityRect.Bottom >= MyObject.MyCamera.Position.Y + 1080f)
					{
						MyObject.CurrentHP -= 25f;
						MyObject.MyCamera.Position.Y = nearestSafePosition.Y + (float)MyObject.VisibilityRect.Bottom - 1080f;
					}
					else if (nearestSafePosition.Y + (float)MyObject.VisibilityRect.Top <= MyObject.MyCamera.Position.Y)
					{
						MyObject.CurrentHP -= 25f;
						MyObject.MyCamera.Position.Y = nearestSafePosition.Y + (float)MyObject.VisibilityRect.Top;
					}
					position = GameCore.CurrentLevel.Camera.Position;
					MyObject.SetPosition(new Vector2((int)position.X - MyObject.VisibilityRect.Left, nearestSafePosition.Y));
				}
				else
				{
					MyObject.Kill();
				}
			}
			else
			{
				MyObject.SetPosition(new Vector2((int)position.X - MyObject.VisibilityRect.Left, MyObject.Position.Y));
			}
			if (MyObject.VelocityX < GameCore.CurrentLevel.ScrollSpeed.X)
			{
				MyObject.VelocityX = GameCore.CurrentLevel.ScrollSpeed.X;
			}
		}
		else
		{
			_stuckOnViewEdge = 0;
			if (MyObject.Position.X + (float)MyObject.VisibilityRect.Right > position.X + 1920f)
			{
				MyObject.SetPosition(new Vector2((float)(int)position.X + 1920f - (float)MyObject.VisibilityRect.Right, MyObject.Position.Y));
				MyObject.VelocityX = GameCore.CurrentLevel.ScrollSpeed.X;
			}
		}
		if (!IsJetPackActive)
		{
			if (MyObject.Position.Y + (float)MyObject.VisibilityRect.Top < position.Y)
			{
				MyObject.SetPosition(new Vector2(MyObject.Position.X, GameCore.CurrentLevel.Camera.Position.Y - (float)MyObject.VisibilityRect.Top));
				MyObject.VelocityY = GameCore.CurrentLevel.ScrollSpeed.Y;
			}
			else if (MyObject.Position.Y + (float)MyObject.VisibilityRect.Bottom > position.Y + 1080f)
			{
				MyObject.SetPosition(new Vector2(MyObject.Position.X, position.Y + 1080f - (float)MyObject.VisibilityRect.Bottom));
				MyObject.VelocityY = GameCore.CurrentLevel.ScrollSpeed.Y;
			}
		}
	}

	public void UpdateState(int index)
	{
		if (index != _currentStateIndex && MyObject != null)
		{
			_currentStateIndex = Utils.Clamp(index, 0, _partStateId.Length - 1);
			MyObject.ChangePartStateToAll(_partStateId[_currentStateIndex]);
			if (_sideFrontPart != null)
			{
				_sideFrontPart.Position.X = _sideFrontOffsetX;
			}
			if (_sideBackPart != null)
			{
				_sideBackPart.Position.X = _sideBackOffsetX;
			}
		}
	}

	public void DestroyRigidBody()
	{
		if (MyObject != null)
		{
			GameObject.DestroyRigidBodyWithChildren(MyObject);
		}
	}

	internal void Update(float elapsedTime)
	{
		for (int i = 0; i < _keyDownArray.Length; i++)
		{
			_keyDownArray[i] = false;
			_keyPressedArray[i] = false;
		}
		for (int num = DroneList.Count - 1; num >= 0; num--)
		{
			if (DroneList[num].IsDestroyed)
			{
				DroneList.RemoveAt(num);
			}
		}
		if (!GameCore.IsEditorEnabled && MyObject != null)
		{
			if (!IsDestroyed)
			{
				ParseInput(elapsedTime);
				_shellTimer -= elapsedTime;
				if (_shellTimer <= 0f)
				{
					_shellTimer = 0.05f;
					if (IsShellReady)
					{
						GameObject gameObject;
						if (ShellGrenadierFactor > 0f)
						{
							gameObject = MyObject.MyLayer.CreateObjectByPartData(_shellGrenadierData, _shellOutPart.WorldPosition, MyObject.Scale, _shellOutPart.WorldRotation, 1);
							gameObject.MyHero = this;
							gameObject.Damage = GetTotalDPS() * ShellGrenadierFactor;
						}
						else
						{
							gameObject = MyObject.MyLayer.CreateObjectByPartData(_shellData, _shellOutPart.WorldPosition, MyObject.Scale, _shellOutPart.WorldRotation, 1);
						}
						gameObject.SetVelocity(MyObject.MyLevel.ScrollSpeed + Utils.RotateVector2(new Vector2(Utils.GetRandom(-80, -50), Utils.GetRandom(-200, -280)), _shellOutPart.WorldRotation));
						gameObject.SetRotationSpeedInDeg(Utils.GetRandom(-280, -400));
						IsShellReady = false;
					}
				}
				if (!GameCore.IsPaused)
				{
					UpdateHeat(elapsedTime);
				}
				UpdateShield(elapsedTime);
				if (!GameCore.IsMainMenuEnabled)
				{
					UpdateMana(elapsedTime);
				}
				else
				{
					Mana = 0f;
				}
				MouseGun?.Update(elapsedTime);
				int num2 = _partStateId.Length;
				Vector2 vector = Utils.RotateVector2(MyObject.Velocity / SpeedRatio - MyObject.MyLevel.ScrollSpeed, 0f - MyObject.Rotation);
				int value = (int)((float)(num2 / 2) * vector.Y / MaxSpeed + (float)num2 / 2f);
				value = Utils.Clamp(value, 0, num2 - 1);
				if (AutopilotMode != HeroAutopilotMode.BlackHole || AutopilotStateY == HeroAutopilotStateYEnum.ToSafeLine)
				{
					UpdateState(value);
				}
				Remcon.Update(elapsedTime);
				MyObject.Update(elapsedTime);
				LastPosition = MyObject.WorldPosition;
				if (ShipHPPercent <= 0.25f)
				{
					if (!_partHpCriticalActive)
					{
						PartHpCritical.ChangePartState("on");
						_partHpCriticalActive = true;
					}
				}
				else if (_partHpCriticalActive)
				{
					PartHpCritical.ChangePartState("idle");
					_partHpCriticalActive = false;
				}
				foreach (HeroSlot value2 in MyShip.Slots.Values)
				{
					value2.Update(elapsedTime);
				}
				if (FieldSensorPart != null)
				{
					FieldSensorPart.Position = DroneCenterOffset;
					FieldSensorPart.SetScale(DroneRadiusFactor);
				}
			}
			else if (IsJetPackActive && JetPackObject.IsAlive)
			{
				UpdateAsJetPackOnly(elapsedTime);
			}
			else if (GameJnG.PlayerCount > 1)
			{
				float shipMaxHP = GetShipMaxHP();
				Utils.ChangeValueToTarget(ref ShipRecoveryValue, shipMaxHP, shipMaxHP / 20f * elapsedTime);
				UpdateInput();
				if (ShipRecoveryValue >= shipMaxHP && (KeyPressed(GameInput.KeyTypeEnum.FireGuns) || KeyPressed(GameInput.KeyTypeEnum.FireBombs) || KeyPressed(GameInput.KeyTypeEnum.PrimaryDevice) || KeyPressed(GameInput.KeyTypeEnum.SecondaryDevice)))
				{
					PartData partData = GameCore.GetPartData("hero_jet_pack_truck");
					if (partData != null && MyObject != null)
					{
						MyObject.MyLevel.LastMainLayer.CreateObjectByPartData(partData, MyObject.MyCamera.Position, Vector2.One, 0f, 999, bCreateRigidBody: false).MyHero = this;
					}
				}
			}
			if (MyObject.MyLevel.CutsceneMode == 0)
			{
				Hud.Update(elapsedTime);
			}
		}
		if (MyObject == null || MyObject.IsDestroyed)
		{
			Mana = 0f;
			Heat = 0f;
			Shield = 0f;
		}
	}

	public void ReportKill()
	{
		if (!_wasKillReported)
		{
			if (!GameCore.IsMainMenuEnabled && !GameCore.IsShopMenuEnabled && !GameJnG.IsTestRangeEnabled)
			{
				StatDeathCount++;
				GameJnG.HeroDeathCountInLevel++;
			}
			_wasKillReported = true;
		}
	}

	private void UpdateAsJetPackOnly(float elapsedTime)
	{
		PanicAttackFactor = 0f;
		UpdateInput();
		MouseGun?.Update(elapsedTime);
		if (!GameCore.IsPaused)
		{
			UpdateHeat(elapsedTime);
			float shipMaxHP = GetShipMaxHP();
			Utils.ChangeValueToTarget(ref ShipRecoveryValue, shipMaxHP, shipMaxHP / 20f * elapsedTime);
		}
		MyShip.Slots[HeroSlotEnum.front_gun].Update(elapsedTime);
	}

	private void DoOnHitBegin(ref float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (Cheat.NoDamage || GameCore.IsShopMenuEnabled)
		{
			damage = 0f;
		}
		if (hittingObject == null || (hittingObject.TargetClass & TargetClass.AllEnemy) != 0 || (hittingObject.TargetClass & TargetClass.MissileEnemy) != 0 || (hittingObject.TargetClass & TargetClass.ShotEnemy) != 0)
		{
			damage *= FactorManager.DamageToHeroFactor;
		}
		if (Shield > 0f)
		{
			float num = damage;
			damage = Math.Max(num - Shield, 0f);
			Shield -= num;
			GameJnG.AchievementManager.HeroDamageAbsorbed += damage;
			if (Shield <= 0f && ShieldWaitTimer <= 0f)
			{
				MyObject.CreateChildByPartData("hero_shield_out", createPhysBody: false);
			}
		}
		damage *= HullBoostHPFactor;
		OnHitBegin?.Invoke(ref damage, damageType, hittingObject);
		if (Shield <= 0f)
		{
			Shield = 0f;
			ShieldWaitTimer = ShieldWaitDelay;
			if (damage > 0f && IsZeldorPanzerEnabled)
			{
				damage = 1f;
			}
		}
	}

	public Particle AddManaTextParticle(float value, Vector2 position)
	{
		string text = TextBundle.GetText("bonus_mana_taken");
		text = text.Replace("@val", ((int)value).ToString());
		Color color = ((!(value >= 0f)) ? new Color(255, 0, 0) : new Color(180, 30, 255));
		return MyObject.MyLevel.AddTextParticle(text, color, position);
	}

	public Particle AddHPTextParticle(float value, Vector2 position)
	{
		Color color = ((!(value >= 0f)) ? new Color(255, 0, 0) : new Color(10, 255, 10));
		return MyObject.MyLevel.AddTextParticle(string.Format("{0} {1}{2}", _strHPText, (value > 0f) ? "+" : "-", Utils.FormatDecimal1(Math.Abs(value))), color, position);
	}

	private void OnHit(float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		GameJnG.AchievementManager.HeroDamageAbsorbed += damage;
		if (damage >= 0.5f)
		{
			AddHPTextParticle(-(int)damage, MyObject.WorldPosition + MyObject.MyLevel.HerotTextEmitterData.OffsetRange.GetRandom());
		}
	}

	private void OnKill()
	{
		if (!IsJetPackActive)
		{
			ReportKill();
		}
		ShipRecoveryValue = 0f;
		foreach (GameObject drone in DroneList)
		{
			if (!IsJetPackActive || drone.MyItem == null || drone.MyItem.MySlot == null || drone.MyItem.MySlot.SlotType != HeroSlotEnum.front_gun)
			{
				drone.Kill();
			}
		}
		if (IsJetPackInstalled)
		{
			MyObject.MyLayer.CreateObjectByPartData("explo_hero_small", MyObject.WorldPosition, MyObject.WorldScale, 0f, 1);
			MyObject.MyLevel.AddWarningText(TextBundle.GetText("hud_warning_ship_destroyed"), new Color(255, 30, 20));
			GameCore.PlaySound("hero_ship_destroyed_alarm");
		}
		else if (GameJnG.PlayerCount < 2 || (this != GameJnG.HeroList[0] && !GameJnG.HeroList[0].IsAlive) || (this != GameJnG.HeroList[1] && !GameJnG.HeroList[1].IsAlive))
		{
			MyObject.MyLayer.CreateObjectByPartData("explo_hero_final", MyObject.WorldPosition, MyObject.WorldScale, 0f, 1);
		}
		else
		{
			MyObject.MyLayer.CreateObjectByPartData("explo_hero_final_P2", MyObject.WorldPosition, MyObject.WorldScale, 0f, 1);
		}
		MyObject.Destroy();
	}

	public void AddNoHeatTimer(float deltaTime)
	{
		if (NoHeatTimer < deltaTime)
		{
			NoHeatTimer = deltaTime;
			if (_heatFreezeObject == null || !_heatFreezeObject.IsAlive)
			{
				_heatFreezeObject = MyObject.CreateChildByPartData(_heatFreezeData, createPhysBody: false);
			}
		}
	}

	public void UpdateShield(float elapsedTime)
	{
		if (!IsOverheated && !IsBurning)
		{
			if (ShieldWaitTimer > 0f)
			{
				ShieldWaitTimer -= elapsedTime;
			}
			if (ShieldWaitTimer <= 0f)
			{
				float currentValue = Shield;
				Utils.ChangeValueToTarget(ref currentValue, MaxShield, ShieldRechargeSpeed * elapsedTime);
				SetShield(currentValue);
			}
		}
	}

	public float GetCollisionDamage()
	{
		return 20f * CollisionDamageFactor;
	}

	public float GetTotalDPS()
	{
		float num = GetCollisionDamage();
		for (HeroSlotEnum heroSlotEnum = HeroSlotEnum.front_gun; heroSlotEnum < HeroSlotEnum.MaxSlotCount; heroSlotEnum++)
		{
			HeroSlot heroSlot = MyShip.Slots[heroSlotEnum];
			if (heroSlot.IsGemType || heroSlot.IsEquipType || heroSlot.IsDeviceType)
			{
				continue;
			}
			HeroItem item = heroSlot.Item;
			if (item != null)
			{
				float value = 0f;
				float finalValue = 0f;
				if (item.TryGetBaseParamSummary("dps", out value, out finalValue))
				{
					num += finalValue;
				}
				if (item.TryGetBaseParamSummary("collision_dps", out value, out finalValue))
				{
					num += finalValue;
				}
			}
		}
		if (num > 10000f)
		{
			GameJnG.UnlockAchievement(GameAchievementEnum.ACH_DPS_1);
		}
		return num;
	}

	public float GetTotalHeatPS()
	{
		float num = 0f;
		for (HeroSlotEnum heroSlotEnum = HeroSlotEnum.front_gun; heroSlotEnum < HeroSlotEnum.MaxSlotCount; heroSlotEnum++)
		{
			HeroItem item = MyShip.Slots[heroSlotEnum].Item;
			if (item != null)
			{
				float value = 0f;
				float finalValue = 0f;
				if (item.TryGetBaseParamSummary("heat_per_sec", out value, out finalValue))
				{
					num += finalValue * HeatFactor;
				}
			}
		}
		return num;
	}

	public Loadout SaveLoadout(int number)
	{
		Loadout loadout = new Loadout(number);
		foreach (HeroSlot value in MyShip.Slots.Values)
		{
			if (value.Item != null)
			{
				loadout.AddItem(value.Item);
			}
		}
		return loadout;
	}

	public void ApplyLoadout(Loadout loadout, HeroSlot skipSlot = null)
	{
		if (loadout == null)
		{
			return;
		}
		bool flag = false;
		OnBeforeApplyLoadout?.Invoke();
		foreach (HeroSlot value in MyShip.Slots.Values)
		{
			LoadoutEntry entryForSlot = loadout.GetEntryForSlot(value);
			if (value != skipSlot && value.Item != null && (entryForSlot == null || entryForSlot.ItemData != value.Item.Data || entryForSlot.ItemLevel != value.Item.Level))
			{
				int firstEmptyIndex = GameJnG.Cargo.GetFirstEmptyIndex();
				if (GameCore.IsShopMenuEnabled)
				{
					GameJnG.ShopMenu.ItemFlySet.StartToCargo(value.Item, firstEmptyIndex);
				}
				value.Item.DoOnRemoveFromSlot();
				GameJnG.Cargo.SetItem(firstEmptyIndex, value.Item);
				value.SetItem(null, bReconfigurateShip: false);
				flag = true;
			}
		}
		foreach (HeroSlot value2 in MyShip.Slots.Values)
		{
			LoadoutEntry entryForSlot2 = loadout.GetEntryForSlot(value2);
			if (value2.Item != null || entryForSlot2 == null)
			{
				continue;
			}
			HeroItem firstItem = GameJnG.Cargo.GetFirstItem(entryForSlot2.ItemData, entryForSlot2.ItemLevel);
			if (firstItem != null)
			{
				GameJnG.Cargo.RemoveItem(firstItem);
				if (GameCore.IsShopMenuEnabled)
				{
					GameJnG.ShopMenu.ItemFlySet.StartToHeroSlot(firstItem, value2.SlotType);
				}
				value2.SetItem(firstItem, bReconfigurateShip: false);
				flag = true;
			}
		}
		if (flag)
		{
			float currentHP = MyObject.CurrentHP;
			float maxHP = MyObject.MaxHP;
			RebuildShipConfiguration();
			if (maxHP > 0f)
			{
				MyObject.CurrentHP = currentHP * (MyObject.MaxHP / maxHP);
			}
			OnApplyLoadout?.Invoke();
		}
		CurrLoadout = loadout;
	}

	public void UpdateMana(float elapsedTime)
	{
		Utils.ChangeValueToTarget(ref Mana, MaxMana, ManaRechargeSpeed * elapsedTime);
	}

	public void UpdateHeat(float elapsedTime)
	{
		if (NoHeatTimer > 0f)
		{
			Heat = 0f;
			NoHeatTimer -= elapsedTime;
		}
		else if (_heatFreezeObject != null)
		{
			_heatFreezeObject.Destroy();
			_heatFreezeObject = null;
		}
		if (HeatPercent >= 1f)
		{
			if (!IsBurning && Heat >= MaxHeat * 1.2f)
			{
				StartBurning();
			}
			else if (!IsElDiabloEnabled && !IsOverheated)
			{
				StartOverheating();
			}
		}
		if (IsOverheated || IsBurning)
		{
			_coolStartTimer = 0f;
		}
		else if (KeyDown(GameInput.KeyTypeEnum.FireGuns))
		{
			_coolStartTimer = 0.25f;
		}
		else
		{
			Utils.ChangeValueToTarget(ref _coolStartTimer, 0f, elapsedTime);
		}
		if (IsBurning)
		{
			if (!KeyDown(GameInput.KeyTypeEnum.FireGuns))
			{
				Heat -= GetCoolingSpeed() * elapsedTime;
				if (Heat > MaxHeat && Utils.ChangeValueToTarget(ref _burningStopTimer, 0f, elapsedTime))
				{
					Heat = MaxHeat;
				}
			}
		}
		else if (Heat > 0f && (_coolStartTimer <= 0f || IsOverheated || NoHeat))
		{
			Heat -= GetCoolingSpeed() * elapsedTime;
		}
		if (Heat < 0f)
		{
			Heat = 0f;
		}
		if (HeatPercent < 0.7f)
		{
			if (IsOverheated)
			{
				StopOverheating();
			}
			if (IsBurning)
			{
				StopBurning();
			}
		}
		if (!IsBurning)
		{
			return;
		}
		_burningDamage = 0.4f;
		if (MyObject.CurrentHP > 1f && Utils.ChangeValueToTarget(ref _burningHitTimer, 0f, elapsedTime))
		{
			_burningHitTimer = 0.2f;
			if (MyObject.CurrentHP - 1f < _burningDamage)
			{
				MyObject.Hit(Math.Max(0f, MyObject.CurrentHP - 1f), DamageTypeEnum.Fire, MyObject);
			}
			else
			{
				MyObject.Hit(_burningDamage, DamageTypeEnum.Fire, MyObject);
			}
		}
	}

	private void StartOverheating()
	{
		if (!PartOverheated.IsEnabled)
		{
			PartOverheated.IsEnabled = true;
			if (SoundOverheated != null)
			{
				SoundOverheated.Stop();
			}
			SoundOverheated = MyObject.PlaySound("hero_overheated", 100, looped: true);
		}
		IsOverheated = true;
	}

	public void StopOverheating()
	{
		if (PartOverheated != null && PartOverheated.IsEnabled)
		{
			PartOverheated.IsEnabled = false;
		}
		if (SoundOverheated != null)
		{
			SoundOverheated.Stop();
		}
		IsOverheated = false;
	}

	private void StartBurning()
	{
		StopOverheating();
		IsBurning = true;
		_burningStopTimer = 0.5f;
		_burningHitTimer = 0f;
		if (!PartBurning.IsEnabled)
		{
			PartBurning.IsEnabled = true;
		}
	}

	private void StopBurning()
	{
		IsBurning = false;
		_burningStopTimer = 0f;
		if (PartBurning.IsEnabled)
		{
			PartBurning.IsEnabled = false;
		}
	}

	public void ProcessObject(GameObject go)
	{
		go.MyHero = this;
	}

	internal void Draw()
	{
		if (!IsDestroyed && MyObject != null)
		{
			GameCore.EffectLight.LayerBeginDraw(MyObject.MyLayer);
			GameCore.EffectLightBasic.LayerBeginDraw(MyObject.MyLayer);
			GameCore.DrawBegin(MyObject.MyCamera.WorldMatrixCurrentLayer);
			MyObject.Draw(GameCore.SpriteBatch);
			GameCore.DrawEnd();
			GameCore.DrawBegin(MyObject.MyCamera.WorldMatrixCurrentLayer);
			for (int num = MyShip.Slots.Values.Count - 1; num >= 0; num--)
			{
				MyShip.Slots[(HeroSlotEnum)num].Draw();
			}
			GameCore.DrawEnd();
		}
	}
}
