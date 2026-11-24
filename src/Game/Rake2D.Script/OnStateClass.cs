namespace Rake2D.Script;

public class OnStateClass : BehaviorScript
{
	private string _targetClassId;

	private string _physClassId;

	private bool _bApplyToChildren;

	public override void Init()
	{
		base.Init();
		_targetClassId = ParamString("target_class");
		_physClassId = ParamString("phys_class");
		_bApplyToChildren = ParamBool("apply_to_children", defaultValue: true);
	}

	public override void RegisterEvents()
	{
		TargetClass targetClass = TargetClass.None;
		PhysClass physClass = PhysClass.None;
		if (_targetClassId != null)
		{
			targetClass = DataUtils.GetTargetClassByString(_targetClassId);
			if (_physClassId == null)
			{
				physClass = DataUtils.GetPhysClassByTargetClass(targetClass);
			}
		}
		if (_physClassId != null)
		{
			physClass = DataUtils.GetPhysClassByString(_physClassId);
			if (_targetClassId == null)
			{
				targetClass = DataUtils.GetTargetClassByPhysClass(physClass);
			}
		}
		MyObject.SetHitAndTargetClass(physClass, targetClass, _bApplyToChildren);
		if (MyObject.RigidBody != null)
		{
			MyObject.RigidBody.IsStatic = ParamBool("static", MyObject.RigidBody.IsStatic);
		}
	}
}
