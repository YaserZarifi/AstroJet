using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnCollisionSendLink : BehaviorScript
{
	private string _linkEvent;

	private int _linkValue;

	private int _repeatCount;

	public override void RegisterEvents()
	{
		MyObject.OnCollision += OnCollision;
	}

	public override void Init()
	{
		base.Init();
		_linkEvent = ParamString("link_out");
		_linkValue = ParamInt("link_value");
		_repeatCount = ParamInt("repeat", 1);
	}

	public void OnCollision(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (_repeatCount > 0)
		{
			myObject.SendLinkToAll(MyObject, _linkEvent, _linkValue);
			_repeatCount--;
		}
		if (_repeatCount <= 0)
		{
			myObject.OnCollision -= OnCollision;
			if (ParamBool("destroy"))
			{
				myObject.Destroy();
			}
		}
	}
}
