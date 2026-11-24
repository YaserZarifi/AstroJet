using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ScaleByHero : BehaviorScript
{
	private Vector2 _originalScale;

	public override void Init()
	{
		base.Init();
		_originalScale = MyObject.Scale;
	}

	public override void RegisterEvents()
	{
		if (MyObject.MyHero != null)
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.MyHero != null)
		{
			MyObject.SetScale(_originalScale * MyObject.MyHero.MyObject.WorldScale);
		}
	}
}
