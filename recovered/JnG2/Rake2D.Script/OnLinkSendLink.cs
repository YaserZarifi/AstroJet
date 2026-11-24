namespace Rake2D.Script;

public class OnLinkSendLink : OnLinkBehavior
{
	private float _timer;

	private GameObject _senderObject;

	private bool _destroy;

	private bool _isActivated;

	private bool _useSenderObject;

	private string _linkOut;

	private string _linkIn;

	private int _linkValue;

	public override void Init()
	{
		base.Init();
		_timer = ParamRandom("link_delay", 0f);
		_destroy = ParamBool("destroy");
		_linkOut = ParamRandomString("link_out", null);
		_linkValue = ParamRandomIntRange("link_value", 1);
		_useSenderObject = ParamBool("sender_object", defaultValue: true);
	}

	protected override void DoAction(GameObject senderObject, string linkIn, int linkValue)
	{
		_linkIn = linkIn;
		if (_timer <= 0f)
		{
			_senderObject = senderObject;
			SendLink();
		}
		else if (!_isActivated)
		{
			MyObject.OnUpdate += OnUpdate;
			_timer /= 1000f;
			_senderObject = senderObject;
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		if ((_timer -= elapsedTime) <= 0f)
		{
			SendLink();
			MyObject.OnUpdate -= OnUpdate;
		}
	}

	private void SendLink()
	{
		MyObject.SendLinkToAll(_useSenderObject ? _senderObject : MyObject, _linkOut ?? _linkIn, _linkValue);
		if (_destroy)
		{
			MyObject.Destroy();
		}
		_isActivated = false;
	}
}
