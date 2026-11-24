using Rake2D.JnG;

namespace Rake2D.Script;

public class FinishLevelBehavior : BehaviorScript
{
	protected FinishLevelModeEnum _mode;

	protected float _fadeDelay;

	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		_doInEditor = false;
		base.Init();
		switch (ParamInt("mode"))
		{
		case 1:
			_mode = FinishLevelModeEnum.WinAndFlyOut;
			break;
		case 2:
			_mode = FinishLevelModeEnum.WinToShop;
			break;
		case 3:
			_mode = FinishLevelModeEnum.WinAndBlackHole;
			break;
		default:
			_mode = FinishLevelModeEnum.Failed;
			break;
		}
		_fadeDelay = Param("fade_delay", (_mode == FinishLevelModeEnum.Failed) ? 750 : 2000) / 1000f;
	}

	protected void FinishLevel()
	{
		GameJnG.SetFinishLevel(MyLevel, _mode, _fadeDelay);
	}
}
