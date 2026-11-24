namespace Rake2D.Script;

public class HeroDeviceRemcon : ItemBehaviorScript
{
	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	private void OnActivated()
	{
		MyHero.Remcon.Activate(MyItem.Level);
	}
}
