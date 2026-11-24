namespace Rake2D.Script;

public class OnStateScore : BehaviorQuakeBase
{
	private int _score;

	public override void Init()
	{
		_score = ParamInt("score", -1);
	}

	public override void RegisterEvents()
	{
		MyObject.AddScore(_score);
	}
}
