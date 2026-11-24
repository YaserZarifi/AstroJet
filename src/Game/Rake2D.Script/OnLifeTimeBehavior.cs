namespace Rake2D.Script;

public abstract class OnLifeTimeBehavior : ExitStateBehavior
{
	protected FloatRange _exitTimeRange;

	protected float _exitTime;

	public override void RegisterEvents()
	{
		if (!GameCore.IsEditorEnabled || _doInEditor)
		{
			_exitTime = _exitTimeRange.GetRandom();
		}
	}
}
