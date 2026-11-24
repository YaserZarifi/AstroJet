namespace Rake2D.Script;

public class OnLinkKill : OnLinkBehavior
{
	protected override void DoAction(GameObject senderObject, string linkIn, int linkValue)
	{
		if (MyObject.IsUpdatable && !MyObject.IsKilled)
		{
			MyObject.KillTimer = (float)(linkValue * ParamRandomIntRange("delay")) / 1000f;
		}
	}
}
