namespace Rake2D.Script;

public class HeroEquipmentNanoMachines : ItemBehaviorScript
{
	private float _hpRecoverySpeed;

	private GameObject _effectObject;

	private float _healTimer;

	public override void StartInSlot()
	{
		_hpRecoverySpeed = Param("hp_recovery");
		if (_effectObject != null)
		{
			_effectObject.Destroy();
			_effectObject = null;
		}
		_healTimer = 0f;
	}

	public override void Update(float elapsedTime)
	{
		if (_effectObject != null && _effectObject.IsDestroyed)
		{
			_effectObject = null;
		}
		Utils.ChangeValueToTarget(ref _healTimer, 0f, elapsedTime);
		if ((!MyHero.KeyDown(GameInput.KeyTypeEnum.FireGuns) && MyHero.ShipHPPercent < 1f) || (MyHero.IsJetPackActive && MyHero.JetPackObject.CurrentHP < MyHero.JetPackObject.MaxHP))
		{
			if (_healTimer <= 0f)
			{
				_healTimer = 0.5f;
				if (MyHero.IsZeldorPanzerEnabled)
				{
					MyHero.AddHP(_hpRecoverySpeed / 20f, 1);
				}
				else
				{
					MyHero.AddHP(_hpRecoverySpeed / 2f, 1);
				}
			}
			if (_effectObject == null)
			{
				_effectObject = MyObject.CreateChildByPartData("hero_nanomachines", createPhysBody: true);
			}
		}
		else if (_effectObject != null)
		{
			_effectObject.Destroy();
			_effectObject = null;
		}
	}
}
