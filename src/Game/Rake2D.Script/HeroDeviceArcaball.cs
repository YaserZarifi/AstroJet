namespace Rake2D.Script;

public class HeroDeviceArcaball : ItemBehaviorScript
{
	private PartData _ballData;

	private float _timer;

	private int _ballCount;

	private int _maxBallCount;

	private float _totalDamageFactor;

	public override void Init()
	{
		base.Init();
		_ballData = GameCore.GetPartData("bomb_arcaball");
	}

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
		_ballCount = _maxBallCount;
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_maxBallCount = ParamInt("ball_count", 3);
		_totalDamageFactor = Param("total_damage_percent", 100f) / 100f;
		_ballCount = _maxBallCount;
	}

	private void OnActivated()
	{
		MyHero.UseMana(100f);
		_ballCount = 0;
		_timer = 0f;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (_ballCount >= _maxBallCount || !Utils.ChangeValueToTarget(ref _timer, 0f, elapsedTime))
		{
			return;
		}
		_ballCount++;
		_timer = 0.25f;
		GameObject gameObject = MyLayer.CreateObjectByPartData(_ballData, MyHero.MyObject.WorldPosition, MyHero.MyObject.WorldScale, 0f, MyHero.MyObject.ZOrder - 1);
		gameObject.Damage = _totalDamageFactor * gameObject.Scale.X * MyHero.GetTotalDPS();
		gameObject.MyHero = MyHero;
		switch (MyItem.Level)
		{
		case 0:
			gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(135f)));
			break;
		case 1:
			if (_ballCount == 1)
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(135f)));
			}
			else
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(45f)));
			}
			break;
		case 2:
			if (_ballCount == 1)
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(135f)));
			}
			else if (_ballCount == 2)
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(45f)));
			}
			else
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(150f)));
			}
			break;
		case 3:
			if (_ballCount == 1)
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(135f)));
			}
			else if (_ballCount == 2)
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(45f)));
			}
			else if (_ballCount == 3)
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(150f)));
			}
			else
			{
				gameObject.SetVelocity(Utils.GetVelocityVector2(1600f, Utils.DegToRad(30f)));
			}
			break;
		}
	}
}
