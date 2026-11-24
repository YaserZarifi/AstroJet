using Rake2D.JnG;

namespace Rake2D.Script;

public class OnSquadronFinishLevel : OnSquadronBehavior
{
	protected FinishLevelModeEnum _mode;

	protected float _fadeDelay;

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		_mode = ((!ParamBool("failed")) ? FinishLevelModeEnum.WinAndFlyOut : FinishLevelModeEnum.Failed);
		_fadeDelay = Param("fade_delay", (_mode == FinishLevelModeEnum.Failed) ? 750 : 2000) / 1000f;
	}

	protected override void DoAction()
	{
		GameJnG.SetFinishLevel(MyLevel, _mode, _fadeDelay);
		MyObject.Destroy();
	}
}
