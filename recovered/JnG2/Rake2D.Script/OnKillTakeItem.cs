namespace Rake2D.Script;

public class OnKillTakeItem : TakeItemBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	private void OnKill()
	{
		TakeItem();
	}
}
