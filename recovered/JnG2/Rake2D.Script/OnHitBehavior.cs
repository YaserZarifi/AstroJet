namespace Rake2D.Script;

public abstract class OnHitBehavior : BehaviorScript
{
	protected float _hpPercent;

	public override void RegisterEvents()
	{
		if (!IsDone)
		{
			MyObject.OnHit += OnHit;
		}
	}

	public override void Init()
	{
		_hpPercent = Param("hp_percent", -1f) / 100f;
	}

	private void OnHit(float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		if (_hpPercent < 0f)
		{
			DoAction(hittingObject);
		}
		else if (MyObject.HPPercent <= _hpPercent)
		{
			IsDone = true;
			MyObject.OnHit -= OnHit;
			DoAction(hittingObject);
		}
	}

	protected abstract void DoAction(GameObject hittingObject);
}
