namespace Rake2D.Script;

public class AimToTarget : AimToPosition
{
	private float _maxRadius;

	private TargetClass _primaryTargetClass;

	private TargetClass _secondaryTargetClass;

	private float _findTargetTimer;

	private float _lastAngle;

	private float _findDelay;

	private GameObject _lastTarget;

	public override void Init()
	{
		base.Init();
		_maxRadius = Param("max_radius", 1000f);
		_primaryTargetClass = DataUtils.GetTargetClassByStringArray(ParamStringArray("target_class"));
		_findDelay = Param("find_delay", 500f) / 1000f;
		if (HasParam("secondary_target_class"))
		{
			_secondaryTargetClass = DataUtils.GetTargetClassByStringArray(ParamStringArray("secondary_target_class"));
		}
		else
		{
			_secondaryTargetClass = TargetClass.None;
		}
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		_lastTarget = null;
		_findTargetTimer = 0f;
	}

	protected override void OnUpdate(float elapsedTime)
	{
		base.OnUpdate(elapsedTime);
		Utils.ChangeValueToTarget(ref _findTargetTimer, 0f, elapsedTime);
	}

	protected override float GetAngleToTarget()
	{
		if (_lastTarget == null || !_lastTarget.IsAlive)
		{
			_findTargetTimer = 0f;
		}
		if (_findTargetTimer <= 0f)
		{
			_lastTarget = MyObject.FindNearestTarget(_primaryTargetClass, _maxRadius, 0f, null, bOnlyHittable: true);
			if (_lastTarget == null && _secondaryTargetClass != TargetClass.None)
			{
				_lastTarget = MyObject.FindNearestTarget(_secondaryTargetClass, _maxRadius, 0f, null, bOnlyHittable: true);
			}
			if (_lastTarget != null)
			{
				_findTargetTimer = _findDelay;
			}
		}
		if (_lastTarget != null)
		{
			_lastAngle = Utils.RadToDeg(MyObject.GetRotationToTarget(_lastTarget, 90f));
		}
		else if (_findTargetTimer <= 0f)
		{
			_lastAngle = 90f;
		}
		return _lastAngle;
	}
}
