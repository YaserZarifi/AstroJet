namespace Rake2D.Script;

public class HeroJetAnimator : BehaviorScript
{
	private GameObject _heroMainObject;

	public override void RegisterEvents()
	{
		if (_heroMainObject != null && _heroMainObject.IsHero)
		{
			MyObject.OnUpdate += Update;
		}
	}

	public override void Init()
	{
		_heroMainObject = MyObject.GetMainPart();
	}

	public void Update(float elapsedTime)
	{
		float num = Utils.Clamp(0.9f + 0.3f * Utils.RotateVector2(_heroMainObject.Velocity - _heroMainObject.MyLevel.ScrollSpeed, 0f - _heroMainObject.Rotation).X / _heroMainObject.MyHero.MaxSpeed, 0.2f, 1.2f);
		MyObject.SetScale(MyObject.CurrentState.Scale.X * num, MyObject.CurrentState.Scale.Y * num);
	}
}
