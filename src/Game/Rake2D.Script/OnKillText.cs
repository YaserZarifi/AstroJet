namespace Rake2D.Script;

public class OnKillText : TextBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += SetText;
	}
}
