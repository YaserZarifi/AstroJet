namespace Rake2D.Script;

public class OnStateDynamic : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.SetDynamic(isDynamic: true, 0f);
		MyObject.OnBeforeCollision += OnBeforeCollision;
	}

	protected void OnBeforeCollision(GameObject go)
	{
		if (go.TargetClass == TargetClass.Hero)
		{
			MyObject.IgnoreCollisionWith(go);
		}
	}
}
