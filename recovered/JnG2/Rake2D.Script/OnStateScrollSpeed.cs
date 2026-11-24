namespace Rake2D.Script;

public class OnStateScrollSpeed : BehaviorScript
{
	private float _targetSpeed;

	private float _changeSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_targetSpeed = Param("speed", -1f);
		if (_targetSpeed < 0f)
		{
			_targetSpeed = MyObject.MyLayer.MyLevel.StartScrollSpeed.X;
		}
		_changeSpeed = Param("change_speed");
	}

	private void OnUpdate(float elapsedTime)
	{
		if (_changeSpeed <= 0f || Utils.ChangeValueToTarget(ref MyObject.MyLayer.MyLevel.ScrollSpeed.X, _targetSpeed, _changeSpeed * elapsedTime))
		{
			MyObject.MyLayer.MyLevel.ScrollSpeed.X = _targetSpeed;
			MyObject.OnUpdate -= OnUpdate;
		}
	}
}
