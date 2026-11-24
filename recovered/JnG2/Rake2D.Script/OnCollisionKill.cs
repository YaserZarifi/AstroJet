using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionKill : BehaviorScript
{
	private bool _doAfter;

	private float _minVelocity;

	private TargetClass _targetClass;

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
		if (HasParam("target_class"))
		{
			string[] array = ParamStringArray("target_class");
			if (array != null)
			{
				_targetClass = DataUtils.GetTargetClassByStringArray(array);
			}
			else
			{
				_targetClass = TargetClass.None;
			}
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
		if (MyObject.Velocity.Length() >= _minVelocity && (_targetClass == TargetClass.None || collisionObject.TargetClass == TargetClass.None || (_targetClass & collisionObject.TargetClass) != 0))
		{
			MyObject.Kill();
		}
	}
}
