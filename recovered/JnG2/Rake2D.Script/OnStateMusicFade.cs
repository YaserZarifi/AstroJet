namespace Rake2D.Script;

public class OnStateMusicFade : ExitStateBehavior
{
	private float _targetFadeFactor;

	private float _fadeSpeed;

	public override void RegisterEvents()
	{
		if (_fadeSpeed <= 0f)
		{
			GameCore.SoundSystem.MusicVolumeFactor = _targetFadeFactor;
			StartExit();
		}
		else
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	public override void Init()
	{
		base.Init();
		_targetFadeFactor = Param("fade_to") / 100f;
		_fadeSpeed = Param("fade_speed") / 100f;
	}

	private void OnUpdate(float elapsedTime)
	{
		float currentValue = GameCore.SoundSystem.MusicVolumeFactor;
		if (Utils.ChangeValueToTarget(ref currentValue, _targetFadeFactor, _fadeSpeed * elapsedTime))
		{
			MyObject.OnUpdate -= OnUpdate;
			StartExit();
		}
		GameCore.SoundSystem.MusicVolumeFactor = currentValue;
	}
}
