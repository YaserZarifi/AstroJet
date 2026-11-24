namespace Rake2D.Script;

public class HomingToAllEnemy : HomingToEnemy
{
	protected override GameObject GetTargetObject()
	{
		GameObject gameObject = base.GetTargetObject();
		if (gameObject == null)
		{
			gameObject = MyObject.FindNearestTarget((TargetClass)192, _range, 0f, null, bOnlyHittable: true);
		}
		return gameObject;
	}
}
