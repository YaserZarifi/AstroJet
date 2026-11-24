namespace Rake2D.Script;

internal class OnTriggerExit : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnTrigger += OnTrigger;
	}

	private void OnTrigger()
	{
		MyObject.OnTrigger -= OnTrigger;
		StartExit();
	}
}
