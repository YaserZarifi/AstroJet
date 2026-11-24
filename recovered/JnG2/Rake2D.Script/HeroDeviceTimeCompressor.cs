using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroDeviceTimeCompressor : ItemBehaviorScript
{
	private float _timer;

	private float _duration;

	private PartData _effectData;

	private GameObject _effectObject;

	private bool _wasEndSoundPlayed;

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	public override void Init()
	{
		base.Init();
		_effectData = GameCore.GetPartData("hero_time_compressor");
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_duration = Param("duration", 2f) / 4f;
	}

	private void OnActivated()
	{
		GameCore.TargetTimeFactor = 0.3f;
		GameCore.TimeFactorChangeSpeed = 1.75f;
		GameJnG.IsTimeCompressorEnabled = true;
		_wasEndSoundPlayed = false;
		_timer = _duration;
		GameObject gameObject = MyHero.MyObject.GetChildByIdDeep("jet_fx") ?? MyHero.MyObject;
		_effectObject = gameObject.CreateChildByPartData(_effectData, createPhysBody: false);
		_effectObject.LifeTime = 0.25f;
		MyHero.UseMana(100f);
		GameCore.PlaySound("device_time_compressor");
	}

	public override void Update(float elapsedTime)
	{
		if (GameJnG.IsTimeCompressorEnabled)
		{
			if (_effectObject != null)
			{
				_effectObject.LifeTime = 0.25f;
			}
			if (!_wasEndSoundPlayed && _timer < 0.1f)
			{
				_wasEndSoundPlayed = true;
				GameCore.PlaySound("device_time_compressor");
			}
			if (Utils.ChangeValueToTarget(ref _timer, 0f, elapsedTime))
			{
				GameCore.TargetTimeFactor = 1f;
				GameCore.TimeFactorChangeSpeed = 0.75f;
				GameJnG.IsTimeCompressorEnabled = false;
				_effectObject?.Destroy();
				_effectObject = null;
			}
		}
	}
}
