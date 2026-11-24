using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionEmit : BehaviorScript
{
	private bool _resetEmitter;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		_resetEmitter = ParamBool("reset");
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (MyObject.Emitter != null)
		{
			MyObject.Emitter.Emit(bIgnoreLoopCount: true);
			if (_resetEmitter)
			{
				MyObject.Emitter.EnableStripLine(MyObject.Emitter.Space, MyObject.Emitter.Data.BlendMode);
				MyObject.Emitter.Emit(bIgnoreLoopCount: true);
				MyObject.Emitter.ResetTimerAndLoop();
			}
		}
	}
}
