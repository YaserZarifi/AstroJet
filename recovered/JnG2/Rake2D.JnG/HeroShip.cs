using System.Collections.Generic;
using Rake2D.Script;

namespace Rake2D.JnG;

public class HeroShip
{
	public Hero MyHero;

	public string Id;

	public Dictionary<HeroSlotEnum, HeroSlot> Slots;

	public PartMap PartMap;

	public HeroShip(string id)
	{
		Id = id;
		Slots = new Dictionary<HeroSlotEnum, HeroSlot>();
		for (HeroSlotEnum heroSlotEnum = HeroSlotEnum.front_gun; heroSlotEnum < HeroSlotEnum.MaxSlotCount; heroSlotEnum++)
		{
			Slots.Add(heroSlotEnum, new HeroSlot(this, heroSlotEnum));
		}
		PartMap = new PartMap();
	}

	public HeroSlot GetSlot(HeroSlotEnum type)
	{
		return Slots[type];
	}

	public float GetModifyiedParamValue(string name, float baseValue, HeroItem myItem)
	{
		ParamModifier modifier = default;
		foreach (HeroSlot value in Slots.Values)
		{
			if (value.Item != null && value.Item != myItem)
			{
				value.Item.UpdateParamModifier(ref modifier, name, myItem);
			}
		}
		return modifier.GetValue(baseValue);
	}
}
