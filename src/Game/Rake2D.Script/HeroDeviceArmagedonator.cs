namespace Rake2D.Script;

public class HeroDeviceArmagedonator : HeroDeviceBlast
{
	public override void StartInSlot()
	{
		base.StartInSlot();
		_rotationArray = new float[1] { Utils.DegToRad(90f) };
	}

	public override void OnActivated()
	{
		base.OnActivated();
		if (_isActive)
		{
			MyHero.AddHeat(100f);
		}
	}
}
