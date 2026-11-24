namespace Rake2D.Script;

public class OnLinkQuake : OnLinkBehavior
{
	protected override void DoAction(GameObject senderObject, string linkIn, int linkValue)
	{
		MyLevel.Quake = Param("quke", 100f);
	}
}
