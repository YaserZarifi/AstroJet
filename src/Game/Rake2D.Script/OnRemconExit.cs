namespace Rake2D.Script;

internal class OnRemconExit : ExitStateBehavior
{
	private bool _cancelRemconable;

	public override void RegisterEvents()
	{
		MyObject.OnUpdateEnd += OnUpdateEnd;
	}

	public override void Init()
	{
		base.Init();
		MyObject.GetMainPart().IsRemconable = true;
		_cancelRemconable = ParamBool("cancel");
	}

	private void OnUpdateEnd(float elapsedTime)
	{
		if (MyObject.RemconedByHero != null)
		{
			MyObject.OnUpdateEnd -= OnUpdateEnd;
			if (_cancelRemconable)
			{
				MyObject.GetMainPart().IsRemconable = false;
			}
			StartExit();
		}
	}
}
