using System;
using Rake2D.JnG;

namespace Rake2D.Script;

public class ItemBehaviorScript : BehaviorScript
{
	public override GameObject MyObject
	{
		get
		{
			if (MySlot == null)
			{
				return null;
			}
			if (MySlot.SlotType == HeroSlotEnum.front_gun && MyHero.IsJetPackActive)
			{
				return MyHero.JetPackSlotPart;
			}
			return MySlot.MyObject;
		}
	}

	public GameObject MyMuzzle
	{
		get
		{
			if (MySlot == null)
			{
				return null;
			}
			if (MySlot.SlotType == HeroSlotEnum.front_gun && MyHero.IsJetPackActive)
			{
				return MyHero.JetPackMuzzlePart;
			}
			return MyObject;
		}
	}

	public HeroItem MyItem { get; internal set; }

	public Hero MyHero
	{
		get
		{
			if (MyItem != null && MyItem.MySlot != null)
			{
				return MyItem.MySlot.MyHero;
			}
			return null;
		}
	}

	public HeroShip MyShip
	{
		get
		{
			if (MyItem != null)
			{
				return MyItem.MyShip;
			}
			return null;
		}
	}

	public HeroSlot MySlot
	{
		get
		{
			if (MyItem != null && MyItem.MySlot != null)
			{
				return MyItem.MySlot;
			}
			return null;
		}
	}

	public override float Param(string name, float defaultValue = 0f)
	{
		if (BehaviorData != null)
		{
			return BehaviorData.GetFloatOrItemData(name, MyItem.Level, defaultValue);
		}
		return defaultValue;
	}

	public override int ParamInt(string name, int defaultValue = 0)
	{
		if (BehaviorData != null)
		{
			return (int)BehaviorData.GetFloatOrItemData(name, MyItem.Level, defaultValue);
		}
		return defaultValue;
	}

	public virtual void StartInSlot()
	{
	}

	public virtual void RecalculateParams()
	{
	}

	public virtual void Update(float elapsedTime)
	{
	}

	public virtual void ApplyToItem(HeroItem item)
	{
	}

	public virtual bool UpdateParamModifier(ref ParamModifier modifier, string paramName, HeroItem item)
	{
		return false;
	}

	public virtual bool TryGetParamSummary(string name, out float result, out float finalResult)
	{
		result = 0f;
		finalResult = 0f;
		return false;
	}

	public static ItemBehaviorScript CreateFromBehaviorData(HeroItem item, BehaviorData data)
	{
		if (data.MyType == null)
		{
			GameCore.LogErr($"Unable instantiate script class '{data.TypeName}' - script type is null.");
			return null;
		}
		ItemBehaviorScript itemBehaviorScript = (ItemBehaviorScript)Activator.CreateInstance(data.MyType);
		if (itemBehaviorScript == null)
		{
			GameCore.LogErr($"Unable instantiate behavior script class '{data.TypeName}'");
		}
		else
		{
			itemBehaviorScript.BehaviorData = data;
			itemBehaviorScript.MyItem = item;
		}
		return itemBehaviorScript;
	}

	public virtual void RemoveFromSlot()
	{
	}

	public override void RegisterEvents()
	{
	}
}
