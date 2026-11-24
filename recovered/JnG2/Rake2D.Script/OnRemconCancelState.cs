namespace Rake2D.Script;

internal class OnRemconCancelState : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnUpdateEnd += OnUpdateEnd;
	}

	private void OnUpdateEnd(float elapsedTime)
	{
		if (MyObject.RemconedCancelByHero != null)
		{
			MyObject.OnUpdateEnd -= OnUpdateEnd;
			StartExit();
		}
	}
}
