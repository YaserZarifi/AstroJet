using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnLinkClone : OnLinkBehavior
{
	protected string _initSateId;

	protected Vector2 _offset;

	protected override void DoAction(GameObject senderObject, string linkIn, int linkValue)
	{
		Vector2 newPosition = senderObject.WorldPosition + _offset;
		GameObject.CloneObject(MyObject, senderObject.MyLayer, newPosition, bMovePath: true).ChangePartState(_initSateId);
	}

	public override void Init()
	{
		base.Init();
		_initSateId = ParamString("init_state", "idle");
		_offset = ParamVector2("offset");
	}
}
