namespace Rake2D.Script;

public class OnLinkScrollSpeed : OnLinkBehavior
{
	private float _targetSpeed;

	private float _changeSpeed;

	private bool _useLinkValue;

	public override void Init()
	{
		base.Init();
		_targetSpeed = Param("speed", -1f);
		if (_targetSpeed == -1f)
		{
			_useLinkValue = true;
		}
		else
		{
			_useLinkValue = false;
		}
		if (_targetSpeed < 0f)
		{
			_targetSpeed = MyObject.MyLayer.MyLevel.StartScrollSpeed.X;
		}
		_changeSpeed = Param("change_speed", 100f);
	}

	protected override void DoAction(GameObject senderObject, string linkIn, int linkValue)
	{
		if (_useLinkValue)
		{
			_targetSpeed = linkValue;
			if (_targetSpeed < 0f)
			{
				_targetSpeed = MyObject.MyLayer.MyLevel.StartScrollSpeed.X;
			}
		}
		MyObject.OnUpdate += OnUpdate;
	}

	private void OnUpdate(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref MyObject.MyLayer.MyLevel.ScrollSpeed.X, _targetSpeed, _changeSpeed * elapsedTime))
		{
			MyObject.OnUpdate -= OnUpdate;
		}
	}
}
