namespace Rake2D.Script;

public class OnLinkHit : OnLinkBehavior
{
	protected override void DoAction(GameObject senderObject, string linkIn, int linkValue)
	{
		if (MyObject.LinkedObjectList == null)
		{
			return;
		}
		foreach (GameObject linkedObject in MyObject.LinkedObjectList)
		{
			linkedObject.Hit(linkValue, DamageTypeEnum.Unknown, senderObject);
		}
	}
}
