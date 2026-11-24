namespace Rake2D.Script;

public class HeroDeviceBounceX : HeroDeviceBlast
{
	public override void StartInSlot()
	{
		base.StartInSlot();
		if (MyItem.Level < 2)
		{
			_rotationArray = new float[4]
			{
				Utils.DegToRad(60f),
				Utils.DegToRad(120f),
				Utils.DegToRad(240f),
				Utils.DegToRad(300f)
			};
		}
		else
		{
			_rotationArray = new float[8]
			{
				Utils.DegToRad(50f),
				Utils.DegToRad(70f),
				Utils.DegToRad(110f),
				Utils.DegToRad(130f),
				Utils.DegToRad(230f),
				Utils.DegToRad(250f),
				Utils.DegToRad(290f),
				Utils.DegToRad(310f)
			};
		}
	}
}
