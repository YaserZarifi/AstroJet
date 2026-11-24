using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroDeviceDroneOffset : ItemBehaviorScript
{
	private static int _mode;

	private Vector2 _targetOffset;

	private GameObject _centerObject;

	public override void StartInSlot()
	{
		SetMode(_mode);
		if (_centerObject != null)
		{
			_centerObject.Destroy();
		}
		_centerObject = MyObject.CreateChildByPartData("hero_drone_center", createPhysBody: true);
		_centerObject.IsEnabled = false;
		MyHero.DroneCenterChangingItem = MyItem;
	}

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	private void SetMode(int mode)
	{
		if (_mode != mode)
		{
			_mode = mode;
			if (_mode == 0)
			{
				_targetOffset = new Vector2(0f, 0f);
				MyObject.PlaySound("device_drone_offset_out");
			}
			else
			{
				_targetOffset = new Vector2(500f, 0f);
				MyObject.PlaySound("device_drone_offset_in");
			}
		}
	}

	private void OnActivated()
	{
		if (MyHero.DroneCenterChangingItem == null || MyHero.DroneCenterChangingItem == MyItem)
		{
			SetMode((_mode == 0) ? 1 : 0);
			MyHero.DroneCenterChangingItem = MyItem;
		}
	}

	public override void Update(float elapsedTime)
	{
		if (GameCore.IsMainMenuEnabled || GameCore.IsShopMenuEnabled)
		{
			if (_mode != 0)
			{
				SetMode(0);
				MyHero.DroneCenterOffset = Vector2.Zero;
			}
		}
		else if (MyHero.DroneCenterChangingItem == MyItem)
		{
			if (Utils.ChangeValueToTarget(ref MyHero.DroneCenterOffset, _targetOffset, 1000f * elapsedTime))
			{
				if (_mode == 0)
				{
					_centerObject.IsEnabled = false;
				}
				MyHero.DroneCenterChangingItem = null;
			}
			else
			{
				_centerObject.IsEnabled = true;
				_centerObject.SetScale(0.5f + MyHero.DroneRadiusFactor / 2f);
				_centerObject.SetPosition(MyHero.DroneCenterOffset);
			}
		}
		else if (MyHero.DroneCenterChangingItem != null && MyHero.DroneCenterChangingItem != MyItem)
		{
			_centerObject.IsEnabled = false;
		}
	}
}
