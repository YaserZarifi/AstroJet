namespace Rake2D.Script;

public class HeroDeviceTotalCooler : ItemBehaviorScript
{
	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	private void OnActivated()
	{
		MyHero.UseMana(100f);
		MyHero.AddNoHeatTimer(Param("duration", 5f));
	}
}
