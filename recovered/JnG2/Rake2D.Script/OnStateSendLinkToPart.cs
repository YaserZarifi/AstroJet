namespace Rake2D.Script;

public class OnStateSendLinkToPart : SendLinkToPart
{
	public override void RegisterEvents()
	{
		base.RegisterEvents();
		if (DoInEditor)
		{
			DoAction();
		}
	}
}
