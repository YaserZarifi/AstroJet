namespace Rake2D.Script;

public class HeroDeviceDroneRadius : ItemBehaviorScript
{
	private static int _mode;

	private float _targetRadiusFactor;

	public override void StartInSlot()
	{
		SetMode(_mode);
	}

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	private void SetMode(int mode)
	{
		_mode = mode;
		switch (_mode)
		{
		case 0:
			_targetRadiusFactor = 1.5f;
			break;
		case 1:
			_targetRadiusFactor = 1f;
			break;
		case 2:
			_targetRadiusFactor = 0.5f;
			break;
		case 3:
			_targetRadiusFactor = 1f;
			break;
		}
	}

	private void OnActivated()
	{
		if (MyHero.DroneRadiusChangingItem == null || MyHero.DroneRadiusChangingItem == MyItem)
		{
			switch (_mode)
			{
			case 0:
				SetMode(2);
				break;
			case 1:
				SetMode(2);
				break;
			case 2:
				SetMode(0);
				break;
			case 3:
				SetMode(0);
				break;
			}
			MyHero.DroneRadiusChangingItem = MyItem;
			MyObject.PlaySound("device_drone_radius");
		}
	}

	public override void Update(float elapsedTime)
	{
		if (MyHero.DroneRadiusChangingItem == MyItem && Utils.ChangeValueToTarget(ref MyHero.DroneRadiusFactor, _targetRadiusFactor, 3f * elapsedTime))
		{
			MyHero.DroneRadiusChangingItem = null;
		}
	}
}
