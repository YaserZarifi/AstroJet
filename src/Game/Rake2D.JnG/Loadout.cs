using System.Collections.Generic;

namespace Rake2D.JnG;

public class Loadout
{
	public int Number;

	public Dictionary<HeroSlotEnum, LoadoutEntry> LoadoutDictionary;

	public Loadout(int number)
	{
		Number = number;
		LoadoutDictionary = new Dictionary<HeroSlotEnum, LoadoutEntry>();
	}

	public void AddItem(HeroItem item)
	{
		if (item.Place == ItemPlaceEnum.InHeroSlot)
		{
			LoadoutEntry loadoutEntry = new LoadoutEntry();
			loadoutEntry.ItemData = item.Data;
			loadoutEntry.ItemLevel = item.Level;
			loadoutEntry.Slot = item.MySlot.SlotType;
			LoadoutDictionary[loadoutEntry.Slot] = loadoutEntry;
		}
	}

	public LoadoutEntry GetEntryForSlot(HeroSlot slot)
	{
		LoadoutEntry value = null;
		LoadoutDictionary.TryGetValue(slot.SlotType, out value);
		return value;
	}
}
