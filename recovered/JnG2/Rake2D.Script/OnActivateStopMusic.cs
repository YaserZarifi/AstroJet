namespace Rake2D.Script;

public class OnActivateStopMusic : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnActivate += OnActivate;
	}

	public void OnActivate()
	{
		GameCore.SoundSystem.MusicVolumeFactor = 0f;
		MyObject.OnActivate -= OnActivate;
		IsDone = true;
	}
}
