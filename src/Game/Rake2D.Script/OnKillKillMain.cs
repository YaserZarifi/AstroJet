namespace Rake2D.Script;

public class OnKillKillMain : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		GameObject mainPart = MyObject.GetMainPart();
		if (mainPart != MyObject)
		{
			mainPart.Kill();
			MyObject.OnKill -= OnKill;
		}
	}
}
