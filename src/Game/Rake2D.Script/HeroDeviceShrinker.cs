namespace Rake2D.Script;

public class HeroDeviceShrinker : ItemBehaviorScript
{
	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	private void OnActivated()
	{
		MyHero.IsShrinkerEnabled = MyHero.MyObject.Scale.X == 1f;
		if (MyHero.IsShrinkerEnabled)
		{
			MyObject.PlaySound("device_shrinker_out");
		}
		else
		{
			MyObject.PlaySound("device_shrinker_in");
		}
	}
}
