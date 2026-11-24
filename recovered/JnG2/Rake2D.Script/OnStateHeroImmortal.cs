using Rake2D.JnG;

namespace Rake2D.Script;

public class OnStateHeroImmortal : BehaviorScript
{
	private bool _isImmortal;

	public override void RegisterEvents()
	{
		foreach (Hero hero in GameJnG.HeroList)
		{
			if (hero.MyObject != null)
			{
				hero.MyObject.IsImmortal = _isImmortal;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		_isImmortal = ParamBool("immortal", defaultValue: true);
	}
}
