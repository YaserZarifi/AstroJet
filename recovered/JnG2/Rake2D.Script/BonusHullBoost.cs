using Rake2D.JnG;

namespace Rake2D.Script;

public class BonusHullBoost : BehaviorScript
{
	private Hero _myHero;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnDestroy += OnDestroy;
	}

	public override void Init()
	{
		base.Init();
		_myHero = MyObject.GetMainPart().MyHero;
	}

	private void OnUpdate(float elapsedTime)
	{
		if (_myHero != null)
		{
			_myHero.HullBoostHPFactor = 0.5f;
		}
	}

	private void OnDestroy()
	{
		if (_myHero != null)
		{
			_myHero.HullBoostHPFactor = 1f;
		}
	}
}
