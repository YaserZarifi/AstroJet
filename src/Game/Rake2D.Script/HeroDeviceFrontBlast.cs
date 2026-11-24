namespace Rake2D.Script;

public class HeroDeviceFrontBlast : HeroDeviceBlast
{
	public override void StartInSlot()
	{
		base.StartInSlot();
		_rotationArray = new float[1] { Utils.DegToRad(90f) };
	}
}
