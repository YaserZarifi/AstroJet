namespace Rake2D.Script;

public class OnPathScrollSpeed : BehaviorScript
{
	public static string[] PathEvents = new string[2] { "speed", "change_speed" };

	private float _targetSpeed;

	private float _changeSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	private void OnUpdate(float elapsedTime)
	{
		if (_changeSpeed < 0f || Utils.ChangeValueToTarget(ref MyObject.MyLayer.MyLevel.ScrollSpeed.X, _targetSpeed, _changeSpeed * elapsedTime))
		{
			MyObject.MyLayer.MyLevel.ScrollSpeed.X = _targetSpeed;
			MyObject.OnUpdate -= OnUpdate;
		}
	}

	public override void DoEvent(BehaviorEvent behEvent, BehaviorTimeLine sourceTimeLine)
	{
		if (Utils.Equals(behEvent.Name, "speed"))
		{
			_targetSpeed = behEvent.ValueInt;
		}
		else if (Utils.Equals(behEvent.Name, "change_speed"))
		{
			_changeSpeed = behEvent.ValueInt;
		}
	}
}
