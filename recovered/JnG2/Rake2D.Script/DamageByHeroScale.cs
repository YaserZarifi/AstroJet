namespace Rake2D.Script;

public class DamageByHeroScale : BehaviorScript
{
	private float _damage;

	private bool _isInitialized;

	public override void RegisterEvents()
	{
		if (MyObject.MyHero != null)
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		if (!_isInitialized)
		{
			_damage = MyObject.Damage;
			_isInitialized = true;
		}
		if (MyObject.MyHero != null)
		{
			MyObject.Damage = _damage * MyObject.MyHero.MyObject.Scale.X;
		}
	}
}
