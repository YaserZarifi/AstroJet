using Microsoft.Xna.Framework;

namespace Rake2D.JnG;

public class HeroSlot : Transformation
{
	public HeroShip MyShip;

	public Vector2 Offset;

	public HeroSlotEnum SlotType;

	public float DefaultRotation;

	public float ActivateTimer = 999f;

	public Vector2 DefaultOffset;

	public GameInput.KeyTypeEnum FireKey = GameInput.KeyTypeEnum.None;

	public GameObject ParentObject;

	public bool IsDestroyed;

	public Hero MyHero => MyShip.MyHero;

	public HeroItem Item { get; private set; }

	public GameObject MyObject { get; private set; }

	public bool IsButtonDown
	{
		get
		{
			if (FireKey != GameInput.KeyTypeEnum.None)
			{
				return MyHero.KeyDown(FireKey);
			}
			return false;
		}
	}

	public bool IsButtonPressed
	{
		get
		{
			if (FireKey != GameInput.KeyTypeEnum.None)
			{
				return MyHero.KeyPressed(FireKey);
			}
			return false;
		}
	}

	public bool IsGemType
	{
		get
		{
			if (SlotType != HeroSlotEnum.gem_front && SlotType != HeroSlotEnum.gem_bottom && SlotType != HeroSlotEnum.gem_rear)
			{
				return SlotType == HeroSlotEnum.gem_top;
			}
			return true;
		}
	}

	public bool IsDeviceType
	{
		get
		{
			if (SlotType != HeroSlotEnum.primary_device)
			{
				return SlotType == HeroSlotEnum.secondary_device;
			}
			return true;
		}
	}

	public bool IsWeaponType
	{
		get
		{
			if (SlotType != HeroSlotEnum.front_gun && SlotType != HeroSlotEnum.top_gun && SlotType != HeroSlotEnum.bottom_gun)
			{
				return SlotType == HeroSlotEnum.rear_gun;
			}
			return true;
		}
	}

	public bool IsEquipType
	{
		get
		{
			if (SlotType >= HeroSlotEnum.equipment_1)
			{
				return SlotType <= HeroSlotEnum.equipment_6;
			}
			return false;
		}
	}

	public HeroSlot(HeroShip ship, HeroSlotEnum slotType)
	{
		MyShip = ship;
		SlotType = slotType;
		switch (SlotType)
		{
		case HeroSlotEnum.top_gun:
			DefaultRotation = Utils.DegToRad(90f);
			DefaultOffset = new Vector2(-10f, -20f);
			FireKey = GameInput.KeyTypeEnum.FireGuns;
			break;
		case HeroSlotEnum.front_gun:
			DefaultRotation = Utils.DegToRad(90f);
			FireKey = GameInput.KeyTypeEnum.FireGuns;
			break;
		case HeroSlotEnum.bottom_gun:
			DefaultRotation = Utils.DegToRad(90f);
			DefaultOffset = new Vector2(-10f, 20f);
			FireKey = GameInput.KeyTypeEnum.FireGuns;
			break;
		case HeroSlotEnum.bomb:
			DefaultRotation = Utils.DegToRad(0f);
			FireKey = GameInput.KeyTypeEnum.FireBombs;
			break;
		case HeroSlotEnum.missile:
			DefaultRotation = Utils.DegToRad(90f);
			FireKey = GameInput.KeyTypeEnum.FireGuns;
			break;
		case HeroSlotEnum.rear_gun:
			DefaultRotation = Utils.DegToRad(-90f);
			FireKey = GameInput.KeyTypeEnum.FireGuns;
			break;
		case HeroSlotEnum.primary_device:
			FireKey = GameInput.KeyTypeEnum.PrimaryDevice;
			break;
		case HeroSlotEnum.secondary_device:
			FireKey = GameInput.KeyTypeEnum.SecondaryDevice;
			break;
		}
		SetItem(null, bReconfigurateShip: false);
	}

	public void Init()
	{
		Rotation = DefaultRotation;
		Offset = DefaultOffset;
		ParentTransformation = ParentObject ?? MyHero.MyObject;
		ActivateTimer = 999f;
	}

	public void Update(float elapsedTime)
	{
		if (ParentObject != null && ParentObject.IsDestroyed)
		{
			Destroy();
			return;
		}
		ResetPrecalculations();
		if (Item != null && Item.IsEnabled)
		{
			if ((MyHero.Mana >= MyHero.MaxMana || !Item.IsTypeManaDevice) && (IsButtonPressed || (IsButtonDown && Item.IsTypeManaDevice)) && Item.IsReady && ActivateTimer == 999f)
			{
				if (MyHero.IsConnectedDevicesEnabled && Item.IsTypeManaDevice)
				{
					HeroSlot heroSlot = this;
					HeroSlot heroSlot2 = ((SlotType != HeroSlotEnum.primary_device) ? MyHero.GetSlot(HeroSlotEnum.primary_device) : MyHero.GetSlot(HeroSlotEnum.secondary_device));
					if (heroSlot2.Item != null && heroSlot2.Item.Data.HasPriority)
					{
						HeroSlot heroSlot3 = heroSlot;
						heroSlot = heroSlot2;
						heroSlot2 = heroSlot3;
					}
					if (heroSlot.Item != null && heroSlot.Item.IsTypeManaDevice && heroSlot.ActivateTimer == 999f)
					{
						heroSlot.Item.Activate();
						heroSlot.ActivateTimer = 999f;
					}
					if (heroSlot2.Item != null && heroSlot2.Item.IsTypeManaDevice && heroSlot2.ActivateTimer == 999f)
					{
						heroSlot2.ActivateTimer = 0.25f;
					}
				}
				else
				{
					Item.Activate();
					ActivateTimer = 999f;
				}
			}
			if (ActivateTimer < 999f)
			{
				ActivateTimer -= elapsedTime;
				if (ActivateTimer <= 0f)
				{
					ActivateTimer = 999f;
					Item.Activate();
				}
			}
			Item.Update(elapsedTime);
		}
		MyObject.Update(elapsedTime);
	}

	public void Draw()
	{
		if (Item != null && !Item.IsBlockedBySameItem)
		{
			MyObject.Draw(GameCore.SpriteBatch);
		}
	}

	public void SetItem(HeroItem item, bool bReconfigurateShip = true)
	{
		Item = item;
		if (Item != null)
		{
			Item.CargoIndex = -1;
			Item.SetSlot(this);
		}
		if (bReconfigurateShip && MyHero != null)
		{
			MyHero.RebuildShipConfiguration();
		}
	}

	public void DestroyMyObject()
	{
		if (MyObject != null)
		{
			MyObject.Destroy();
			MyObject = null;
		}
	}

	public void Destroy()
	{
		DestroyMyObject();
		IsDestroyed = true;
	}

	public void Start()
	{
		DestroyMyObject();
		MyObject = new GameObject();
		MyObject.ParentTransformation = this;
		MyObject.ActiveMode = ActiveModeEnum.AlwaysActive;
		MyObject.Activate();
		MyObject.SetLayer((ParentObject != null) ? ParentObject.MyLayer : MyHero.MyObject.MyLayer);
		if (Item != null && !Item.IsBlockedBySameItem && !GameCore.IsEditorEnabled)
		{
			Item.StartInSlot();
		}
	}

	public void StartEnd()
	{
		if (Item != null && !Item.IsBlockedBySameItem && !GameCore.IsEditorEnabled)
		{
			Item.StartInSlotEnd();
		}
	}

	public bool IsCompatible(HeroItem item)
	{
		if (item == null)
		{
			return false;
		}
		switch (SlotType)
		{
		case HeroSlotEnum.front_gun:
		case HeroSlotEnum.top_gun:
		case HeroSlotEnum.bottom_gun:
		case HeroSlotEnum.rear_gun:
			if (item.IsTypeWeapon)
			{
				return true;
			}
			break;
		case HeroSlotEnum.gem_front:
		case HeroSlotEnum.gem_top:
		case HeroSlotEnum.gem_bottom:
		case HeroSlotEnum.gem_rear:
			if (item.IsTypeGem)
			{
				return true;
			}
			break;
		case HeroSlotEnum.bomb:
			if (item.IsTypeBomb)
			{
				return true;
			}
			break;
		case HeroSlotEnum.missile:
			if (item.IsTypeMissile)
			{
				return true;
			}
			break;
		case HeroSlotEnum.primary_device:
		case HeroSlotEnum.secondary_device:
			if (item.IsTypeDevice)
			{
				return true;
			}
			break;
		case HeroSlotEnum.equipment_1:
		case HeroSlotEnum.equipment_2:
		case HeroSlotEnum.equipment_3:
		case HeroSlotEnum.equipment_4:
		case HeroSlotEnum.equipment_5:
		case HeroSlotEnum.equipment_6:
			if (item.IsTypeEquipment || item.IsTypeArmor || item.IsTypeShield)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public override string ToString()
	{
		return SlotType.ToString();
	}
}
