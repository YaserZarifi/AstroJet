using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroFieldSensor : BehaviorScript
{
	protected GameObject _heroMainObject;

	public override void Init()
	{
		base.Init();
		_heroMainObject = MyObject.GetMainPart();
	}

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
		MyObject.OnSeparation += OnSeparation;
	}

	private void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		_heroMainObject.MyHero.DoOnFieldCollision(collisionObject, contactPoint, normal);
	}

	private void OnSeparation(GameObject go)
	{
		_heroMainObject.MyHero.DoOnFieldSeparation(go);
	}
}
