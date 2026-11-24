using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionDoLink : BehaviorScript
{
	private string _linkEvent;

	private int _linkValue;

	public bool _checkLinkedOnly;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		base.Init();
		_linkEvent = ParamString("link_out");
		_linkValue = ParamInt("link_value");
		_checkLinkedOnly = !MyObject.LinkedObjectList.IsNullOrEmpty();
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (!_checkLinkedOnly || (myObject.LinkedObjectList != null && myObject.LinkedObjectList.Contains(collisionObject)))
		{
			collisionObject.DoOnLink(MyObject, _linkEvent, _linkValue, bApplyChildren: true);
			if (ParamBool("destroy"))
			{
				myObject.Destroy();
			}
		}
	}
}
