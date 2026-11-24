namespace Rake2D.Script;

public class OnTimerHitParent : OnTimerBehavior
{
	private bool _destroyOnParentKill;

	private float _damage;

	public override void Init()
	{
		base.Init();
		_destroyOnParentKill = ParamBool("on_parent_kill_destroy");
		_damage = Param("damage", -100f);
		if (_damage < 0f)
		{
			_damage = MyObject.Damage * (0f - _damage) / 100f;
		}
	}

	protected override void DoAction()
	{
		if (MyObject.ParentObject != null)
		{
			MyObject.ParentObject.Hit(_damage * _lastDelay, MyObject.DamageType, MyObject);
			if (_destroyOnParentKill && MyObject.ParentObject.IsKilled)
			{
				MyObject.Destroy();
			}
		}
	}
}
