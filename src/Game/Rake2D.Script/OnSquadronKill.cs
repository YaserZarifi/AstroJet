namespace Rake2D.Script;

public class OnSquadronKill : OnSquadronBehavior
{
	protected override void DoAction()
	{
		MyObject.Kill();
	}
}
