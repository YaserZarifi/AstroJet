namespace Rake2D.Script;

public class HeroDeviceFlipper : ItemBehaviorScript
{
	private int _flipDirection;

	private float _currRotation;

	private float _targetRotation;

	private float _flipEndTimer;

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
		_flipDirection = 0;
	}

	private void OnActivated()
	{
		_currRotation = MyHero.MyObject.Rotation;
		_flipEndTimer = 0.05f * GameCore.TimeFactor;
		if (_flipDirection != 0)
		{
			_flipDirection *= -1;
			if (_flipDirection < 1)
			{
				_targetRotation = ((_currRotation > Utils.DegToRad(-180f)) ? Utils.DegToRad(-180f) : Utils.DegToRad(-360f));
			}
			else
			{
				_targetRotation = ((_currRotation > Utils.DegToRad(-180f)) ? 0f : Utils.DegToRad(-180f));
			}
		}
		else
		{
			if (_currRotation > Utils.DegToRad(-180f))
			{
				_targetRotation = Utils.DegToRad(-180f);
			}
			else
			{
				_targetRotation = Utils.DegToRad(-360f);
			}
			_flipDirection = -1;
		}
	}

	public override void Update(float elapsedTime)
	{
		if (_flipDirection == 0)
		{
			return;
		}
		if (Utils.ChangeValueToTarget(ref _currRotation, _targetRotation, 7f * elapsedTime) && Utils.ChangeValueToTarget(ref _flipEndTimer, 0f, elapsedTime))
		{
			_flipDirection = 0;
			if (_currRotation <= Utils.DegToRad(-360f))
			{
				_currRotation = 0f;
			}
		}
		MyHero.MyObject.SetRotation(_currRotation);
	}
}
