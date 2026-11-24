namespace Rake2D.Script;

public class OnStateMusicPlay : BehaviorScript
{
	protected float _startTime;

	public override void RegisterEvents()
	{
		if (!GameCore.IsEditorEnabled && GameCore.SoundSystem.LoadMusic(ParamString("music")))
		{
			if (_startTime > 0f)
			{
				GameCore.SoundSystem.PlayMusicFromPositionInSec(isLooped: true, _startTime / 1000f);
			}
			else
			{
				GameCore.SoundSystem.PlayMusic(isLooped: true);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		_startTime = Param("start_time");
	}
}
