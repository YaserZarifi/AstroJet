namespace Rake2D.Script;

public class HeroEquipmentPanicAttack : ItemBehaviorScript
{
	private float _factor;

	private PartData _panicData;

	private GameObject _panicObject;

	public override void Init()
	{
		base.Init();
		_factor = Param("factor") / 100f;
		_panicData = GameCore.GetPartData("panic_attack");
	}

	private void CancelPanic()
	{
		MyHero.PanicAttackFactor = 0f;
		if (_panicObject != null)
		{
			_panicObject.Destroy();
			_panicObject = null;
		}
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		CancelPanic();
	}

	public override void Update(float elapsedTime)
	{
		if (MyHero.MyObject.IsAlive && MyHero.MyObject.CurrentHP <= 30f)
		{
			MyHero.PanicAttackFactor = _factor;
			if (_panicObject == null)
			{
				_panicObject = MyHero.MyObject.CreateChildByPartData(_panicData, createPhysBody: true);
			}
		}
		else
		{
			CancelPanic();
		}
	}
}
