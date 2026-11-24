using Rake2D.JnG;

namespace Rake2D.Script;

public class TakeItemBehavior : BehaviorScript
{
	protected HeroItemData _itemData;

	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		string text = ParamString("item");
		if (text != null)
		{
			_itemData = GameCore.GetHeroItemData(text);
			if (_itemData == null)
			{
				LogErr("Unable to found [item = {0}]", text);
			}
		}
		else
		{
			LogErr("Undefined param '$item'");
		}
	}

	protected void TakeItem()
	{
		if (_itemData != null)
		{
			GameJnG.AchievementManager.LevelTakenItemDataList.Add(_itemData);
		}
	}
}
