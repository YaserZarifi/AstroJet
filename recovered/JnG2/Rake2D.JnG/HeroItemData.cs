using System.Collections.Generic;
using System.Linq;
using Rake2D.Script;

namespace Rake2D.JnG;

public class HeroItemData
{
	public const int MAX_ITEM_LEVEL = 3;

	public string Id;

	public string TextId;

	public string BlockId;

	public List<BehaviorData> BehaviorDataList;

	public Dictionary<string, HeroItemDataParam> Params;

	public string FileName;

	public SpriteAtlas IconAtlas;

	public List<int> IconFrameList;

	public List<string> TVObjectIdList;

	public List<int> PriceList;

	public int MaxLevel;

	public ItemTypeEnum ItemType = ItemTypeEnum.Common;

	public string TvAdvertAnimId = "1";

	public string TvAdvertAnimIdForeground = "1";

	public bool TvAdvertExtraPicture;

	public bool HasPriority;

	public IntRange ShopStageRange;

	public PartData ViewPartData;

	public string ShopActionButtonLabel;

	public string ShopActionButtonHint;

	public ItemShopActionEnum ShopAction;

	public string ShopActionNeedItemIdInCargo;

	public string[] ItemIdToGetArray;

	public int ItemIdToGetCount;

	public bool IsShopTypeWeapon
	{
		get
		{
			if (ItemType != ItemTypeEnum.Cannon && ItemType != ItemTypeEnum.Beam && ItemType != ItemTypeEnum.Blaster)
			{
				return ItemType == ItemTypeEnum.Rammer;
			}
			return true;
		}
	}

	public bool IsShopTypeGem => ItemType == ItemTypeEnum.Gem;

	public bool IsShopTypeMissile => ItemType == ItemTypeEnum.Missile;

	public bool IsShopTypeBomb
	{
		get
		{
			if (ItemType != ItemTypeEnum.Bomb)
			{
				return ItemType == ItemTypeEnum.Gunbot;
			}
			return true;
		}
	}

	public bool IsShopTypeDevice
	{
		get
		{
			if (ItemType != ItemTypeEnum.Device)
			{
				return ItemType == ItemTypeEnum.Mana_device;
			}
			return true;
		}
	}

	public bool IsShopTypeEquipment
	{
		get
		{
			if (ItemType != ItemTypeEnum.Equipment && ItemType != ItemTypeEnum.Shield)
			{
				return ItemType == ItemTypeEnum.Armor;
			}
			return true;
		}
	}

	public bool IsShopTypeOther
	{
		get
		{
			if (ItemType != ItemTypeEnum.Common && ItemType != ItemTypeEnum.Medal && ItemType != ItemTypeEnum.Other && ItemType != ItemTypeEnum.Loadout)
			{
				return ItemType == ItemTypeEnum.Unknown;
			}
			return true;
		}
	}

	public HeroItemData(string id)
	{
		Id = id;
		TextId = Id;
		Params = new Dictionary<string, HeroItemDataParam>();
		ShopStageRange = new IntRange(0, 99999);
		PriceList = new List<int>();
	}

	public string GetShopName()
	{
		return TextBundle.GetText($"name_{TextId}");
	}

	public void Init(GameContent content)
	{
		if (IconAtlas == null)
		{
			IconAtlas = content.GetSpriteAtlas($"item_icon_{TextId}");
			if (IconAtlas == null)
			{
				GameCore.LogErr($"Unable to find icon atlas for [item = {Id}]");
				IconAtlas = content.DefaultItemIconAtlas;
			}
		}
	}

	internal void LoadFromIniFile(IniFile iniFile, GameContent content)
	{
		FileName = iniFile.FileName;
		BehaviorData behaviorData = null;
		string text = null;
		while (iniFile.NextLine())
		{
			if (DataLoader.CheckParams(iniFile, content))
			{
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection)
			{
				break;
			}
			int[] resultArray2;
			string[] resultArray3;
			if (iniFile.Key.StartsWith("@") && iniFile.Key.Length > 1 && iniFile.TryGetFloatArray(1, 4, out var resultArray))
			{
				string text2 = iniFile.Key.Substring(1).ToLowerLazy();
				if (Params.ContainsKey(text2))
				{
					iniFile.LogErr("Param '@{0}' already defined in [item = {1}]", text2, Id);
				}
				Params[text2] = new HeroItemDataParam(resultArray);
			}
			else if (iniFile.KeyEquals("icon") && iniFile.TryGetIntArray(1, 10, out resultArray2))
			{
				IconFrameList = new List<int>(resultArray2);
				if (iniFile.Id != null)
				{
					IconAtlas = GameCore.GetSpriteAtlas(iniFile.Id);
					if (IconAtlas == null)
					{
						iniFile.LogErr("Unable to found [atlas = {0}] in content", iniFile.Id);
					}
				}
			}
			else if (iniFile.KeyEquals("tv_object") && iniFile.TryGetStringArray(1, 10, out resultArray3))
			{
				TVObjectIdList = new List<string>(resultArray3);
			}
			else if (iniFile.KeyEquals("tv_advert_anim") && iniFile.TryGetStringArray(1, 2, out resultArray3))
			{
				TvAdvertAnimId = resultArray3[0];
				if (resultArray3.Length > 1)
				{
					TvAdvertAnimIdForeground = resultArray3[1];
				}
				else
				{
					TvAdvertAnimIdForeground = resultArray3[0];
				}
			}
			else if (iniFile.KeyEquals("tv_advert_pic"))
			{
				TvAdvertExtraPicture = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("priority"))
			{
				HasPriority = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("price"))
			{
				PriceList = new List<int>(iniFile.GetIntArray());
			}
			else if (iniFile.KeyEquals("text_id"))
			{
				TextId = iniFile.Value;
			}
			else if (iniFile.KeyEquals("block_id"))
			{
				BlockId = iniFile.Value;
			}
			else if (iniFile.KeyEquals("max_level"))
			{
				MaxLevel = iniFile.GetIntValue();
			}
			else if (iniFile.KeyEquals("shop_stage"))
			{
				if (!iniFile.TryGetIntRange(ref ShopStageRange))
				{
					iniFile.LogErr("Wrong params");
				}
			}
			else if (iniFile.KeyEquals("type"))
			{
				ItemType = DataUtils.GetItemTypeByString(iniFile.Value);
				if (ItemType == ItemTypeEnum.Unknown)
				{
					iniFile.LogErr("Wrong item type");
				}
			}
			else if (iniFile.KeyEquals("button_label"))
			{
				ShopActionButtonLabel = iniFile.Value;
			}
			else if (iniFile.KeyEquals("button_hint"))
			{
				ShopActionButtonHint = iniFile.Value;
			}
			else if (iniFile.KeyEquals("button_action"))
			{
				if (iniFile.ValueEquals("show_atarix"))
				{
					ShopAction = ItemShopActionEnum.ShowAtarix;
					continue;
				}
				if (iniFile.ValueEquals("apply_loadout"))
				{
					ShopAction = ItemShopActionEnum.ApplyLoadout;
					continue;
				}
				if (iniFile.ValueEquals("hack"))
				{
					ShopAction = ItemShopActionEnum.Hack;
					continue;
				}
				if (iniFile.ValueEquals("drink_beer"))
				{
					ShopAction = ItemShopActionEnum.DrinkBeer;
					continue;
				}
				iniFile.LogErr("Unknown button_action '{0}'", iniFile.Value);
			}
			else if (iniFile.KeyEquals("button_action_need_item"))
			{
				ShopActionNeedItemIdInCargo = iniFile.Value;
			}
			else if (iniFile.KeyEquals("view_object"))
			{
				ViewPartData = GameCore.GetPartData(iniFile.Value);
				if (ViewPartData == null)
				{
					iniFile.LogErr("Unable to set view_object");
					continue;
				}
				ShopAction = ItemShopActionEnum.ViewObject;
				if (ShopActionButtonLabel == null)
				{
					ShopActionButtonLabel = "button_view";
				}
				if (ShopActionButtonHint == null)
				{
					ShopActionButtonHint = "hint_view";
				}
			}
			else if (iniFile.KeyEquals("get_item"))
			{
				ItemIdToGetArray = iniFile.GetStringArray();
				ItemIdToGetCount = iniFile.GetIdIntValue(1);
				if (ShopAction == ItemShopActionEnum.None)
				{
					ShopAction = ItemShopActionEnum.GetItem;
				}
				if (ShopActionButtonLabel == null)
				{
					ShopActionButtonLabel = "button_get_item";
				}
				if (ShopActionButtonHint == null)
				{
					ShopActionButtonHint = "hint_get_item";
				}
			}
			else if (iniFile.KeyEquals("beh"))
			{
				text = null;
				behaviorData = new BehaviorData();
				if (behaviorData.SetTypeName(iniFile.Value) != null)
				{
					if (BehaviorDataList == null)
					{
						BehaviorDataList = new List<BehaviorData>();
					}
					BehaviorDataList.Add(behaviorData);
				}
				else
				{
					iniFile.LogErr("Undefined behavior class '{0}'", iniFile.Value);
					behaviorData = null;
				}
			}
			else if (iniFile.KeyEquals("suffix") && iniFile.ValueType == IniFileValueType.SubSection)
			{
				text = iniFile.Value;
				if (iniFile.Id == null || behaviorData == null)
				{
					continue;
				}
				string[] idStringArray = iniFile.GetIdStringArray();
				foreach (string key in idStringArray)
				{
					foreach (KeyValuePair<string, BehaviorData.ParamValue> item in behaviorData.ParametersInternal.Where((KeyValuePair<string, BehaviorData.ParamValue> o) => o.Key.EndsWith(key)).ToList())
					{
						string key2 = item.Key.Substring(0, item.Key.LastIndexOf(key)) + text;
						if (item.Value._floats != null)
						{
							behaviorData.SetParam(key2, (float[])item.Value._floats.Clone());
						}
						else if (item.Value._strings != null)
						{
							behaviorData.SetParam(key2, (string[])item.Value._strings.Clone());
						}
					}
				}
			}
			else if (iniFile.Key.StartsWith("$") && iniFile.Key.Length > 1 && iniFile.Value.Length > 0)
			{
				if (behaviorData != null)
				{
					string text3 = iniFile.Key.Substring(1);
					if (text != null)
					{
						text3 += $"_{text}";
					}
					if (iniFile.Value.StartsWith("@"))
					{
						string text4 = iniFile.Value.Substring(1).ToLowerLazy();
						if (text4.Length > 0 && ((text4[0] >= '0' && text4[0] <= '9') || text4[0] == '-') && Utils.TryGetFloatArray(text4, 1, 4, out resultArray))
						{
							behaviorData.SetParam(text3, new HeroItemDataParam(resultArray));
							continue;
						}
						if (Params.ContainsKey(text4))
						{
							behaviorData.SetParam(text3, Params[text4]);
							continue;
						}
						iniFile.LogErr("Undefined item param @{0}", text4);
					}
					else if (char.IsDigit(iniFile.Value[0]) || (iniFile.Value.Length > 1 && iniFile.Value[0] == '-' && char.IsDigit(iniFile.Value[1])))
					{
						behaviorData.SetParam(text3, iniFile.GetFloatArray());
					}
					else
					{
						behaviorData.SetParam(text3, iniFile.GetStringArray());
					}
				}
				else
				{
					iniFile.LogErr($"Param '{iniFile.Key}' without defined behavior");
				}
			}
			else
			{
				iniFile.LogErr("Unknown item command");
			}
		}
	}

	public SlotTypeEnum GetSlotType()
	{
		switch (ItemType)
		{
		case ItemTypeEnum.Loadout:
			return SlotTypeEnum.Loadout;
		case ItemTypeEnum.Cannon:
		case ItemTypeEnum.Blaster:
		case ItemTypeEnum.Beam:
		case ItemTypeEnum.Rammer:
			return SlotTypeEnum.Weapon;
		case ItemTypeEnum.Missile:
			return SlotTypeEnum.Missile;
		case ItemTypeEnum.Bomb:
		case ItemTypeEnum.Gunbot:
			return SlotTypeEnum.Bomb;
		case ItemTypeEnum.Device:
		case ItemTypeEnum.Mana_device:
			return SlotTypeEnum.Device;
		case ItemTypeEnum.Equipment:
		case ItemTypeEnum.Armor:
		case ItemTypeEnum.Shield:
			return SlotTypeEnum.Equipment;
		case ItemTypeEnum.Gem:
			return SlotTypeEnum.Gem;
		default:
			return SlotTypeEnum.None;
		}
	}
}
