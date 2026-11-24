namespace Rake2D.Script;

public class HomingToEnemy : HomingToTarget
{
	protected float _range;

	public override void Init()
	{
		base.Init();
		_range = Param("range", 500f);
	}

	protected override GameObject GetTargetObject()
	{
		return MyObject.FindNearestTarget(TargetClass.AllEnemy, _range, 0f, null, bOnlyHittable: true);
	}
}
