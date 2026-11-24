using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroFieldBurn : ItemBehaviorScript
{
	public override void RegisterEvents()
	{
		MyHero.OnFieldCollision += OnCollision;
	}

	private void OnCollision(GameObject go, Vector2 contactPoint, Vector2 normal)
	{
		FxEffect.StartBurning(go, MyHero);
	}
}
