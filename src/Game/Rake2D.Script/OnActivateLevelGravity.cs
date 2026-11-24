using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnActivateLevelGravity : BehaviorScript
{
	private Vector2 _gravity;

	public override void Init()
	{
		base.Init();
		_gravity = ParamVector2("gravity");
	}

	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			MyObject.OnActivate += OnActivate;
		}
	}

	private void OnActivate()
	{
		MyObject.OnActivate -= OnActivate;
		MyLevel.PhysWorld.Gravity = _gravity;
	}
}
