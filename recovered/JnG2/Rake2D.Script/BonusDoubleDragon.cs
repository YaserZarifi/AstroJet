using Rake2D.JnG;

namespace Rake2D.Script;

public class BonusDoubleDragon : BehaviorScript
{
	private Hero _myHero;

	private PartData _effectEndData;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnDestroy += OnDestroy;
		MyObject.GetMainPart().DestroySameChildren(MyObject);
	}

	public override void Init()
	{
		base.Init();
		_myHero = MyObject.GetMainPart().MyHero;
		_effectEndData = GameCore.GetPartData("hero_double_dragon_end");
	}

	private void OnUpdate(float elapsedTime)
	{
		if (_myHero != null && _myHero.DamageFactorDoubleDragon < MyObject.Damage)
		{
			_myHero.DamageFactorDoubleDragon = MyObject.Damage;
		}
		MyLevel.Quake = 8f;
	}

	private void OnDestroy()
	{
		if (_myHero != null)
		{
			_myHero.DamageFactorDoubleDragon = 1f;
			_myHero.MyObject.CreateChildByPartData(_effectEndData, createPhysBody: false);
		}
	}
}
