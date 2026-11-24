using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionExit : ExitStateBehavior
{
	private bool _doAfter;

	private float _minVelocity;

	public override void RegisterEvents()
	{
		if (_doAfter)
		{
			MyObject.OnAfterCollision += OnCollision;
		}
		else
		{
			MyObject.OnCollision += OnCollision;
		}
	}

	public override void Init()
	{
		base.Init();
		_minVelocity = Param("min_speed");
		_doAfter = ParamBool("after");
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (MyObject.Velocity.Length() >= _minVelocity)
		{
			StartExit();
		}
	}
}
