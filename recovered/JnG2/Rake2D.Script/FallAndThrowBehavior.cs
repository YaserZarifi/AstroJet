namespace Rake2D.Script;

public class FallAndThrowBehavior : BehaviorScript
{
	public override void RegisterEvents()
	{
	}

	protected void FallAndThrow()
	{
		if (MyObject.LinkedObjectList == null)
		{
			return;
		}
		MyObject.SendLinkToAll(MyObject, "fall", 0);
		foreach (GameObject linkedObject in MyObject.LinkedObjectList)
		{
			if (linkedObject.RigidBody != null)
			{
				linkedObject.ApplyLinearImpulse(Utils.GetVelocityVector2(linkedObject.RigidBody.Mass * 10f, MyObject.GetRotationToTarget(linkedObject)));
				linkedObject.ApplyAngularImpulse(linkedObject.RigidBody.Mass * (float)Utils.GetRandom(4, 10) * (float)((!(MyObject.GetRotationToTarget(linkedObject) < 0f)) ? 1 : (-1)));
				if (linkedObject.RotationSpeed < -6f)
				{
					linkedObject.RotationSpeed = -6f;
				}
				else if (linkedObject.RotationSpeed > 6f)
				{
					linkedObject.RotationSpeed = 6f;
				}
			}
		}
	}
}
