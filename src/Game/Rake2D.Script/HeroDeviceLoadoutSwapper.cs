using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroDeviceLoadoutSwapper : ItemBehaviorScript
{
	private HeroItem _myLoadoutItem;

	public override void StartInSlot()
	{
		base.StartInSlot();
		MyHero.IsLoadoutSwapperEnabled = true;
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyItem.OnActivated += OnActivated;
		_myLoadoutItem = GameJnG.Cargo.GetFirstLoadoutItem(MyHero);
	}

	private void OnActivated()
	{
		if (_myLoadoutItem != null && MyHero.CurrentLoadoutItem != null && MyHero.CurrentLoadoutItem.Loadout != null && (MyHero.MyObject.Rotation == 0f || MyHero.MyObject.Rotation == Utils.DegToRad(-180f)))
		{
			HeroItem currentLoadoutItem = MyHero.CurrentLoadoutItem;
			int cargoIndex = _myLoadoutItem.CargoIndex;
			GameJnG.Cargo.RemoveItem(_myLoadoutItem);
			MyHero.CurrentLoadoutItem = _myLoadoutItem;
			_myLoadoutItem = currentLoadoutItem;
			GameJnG.Cargo.SetItem(cargoIndex, _myLoadoutItem);
			MyHero.ApplyLoadout(MyHero.CurrentLoadoutItem.Loadout, MySlot);
			MyHero.MyObject.PlaySound("menu_item_place");
		}
	}
}
