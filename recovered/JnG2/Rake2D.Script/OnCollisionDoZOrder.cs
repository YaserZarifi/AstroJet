using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionDoZOrder : BehaviorScript
{
	private int _zorder;

	public bool _checkLinkedOnly;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		base.Init();
		_zorder = ParamInt("zorder");
		_checkLinkedOnly = !MyObject.LinkedObjectList.IsNullOrEmpty();
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (!_checkLinkedOnly || (myObject.LinkedObjectList != null && myObject.LinkedObjectList.Contains(collisionObject)))
		{
			collisionObject.ZOrder = _zorder;
			if (ParamBool("destroy"))
			{
				myObject.Destroy();
			}
		}
	}
}
