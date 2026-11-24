using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnLinkCreate : CreateObjectBehavior
{
	private string _eventName;

	public override void RegisterEvents()
	{
		MyObject.OnLink += OnLink;
	}

	public override void Init()
	{
		base.Init();
		_eventName = ParamString("link_in");
	}

	public void OnLink(GameObject senderObject, string eventName, int value)
	{
		if (_eventName == null || Utils.Equals(eventName, _eventName))
		{
			CreateObjectList(null, Vector2.Zero);
		}
	}
}
