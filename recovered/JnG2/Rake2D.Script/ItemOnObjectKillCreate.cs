namespace Rake2D.Script;

public class ItemOnObjectKillCreate : ItemBehaviorCreateBase
{
	private TargetClass _targetClass;

	private bool _applyDemolisher;

	public override void RegisterEvents()
	{
		MyLevel.OnObjectKill += OnObjectKill;
	}

	public override void Init()
	{
		base.Init();
		_targetClass = DataUtils.GetTargetClassByStringArray(ParamStringArray("target_class"));
		_applyDemolisher = ParamBool("apply_demolisher");
	}

	private void OnObjectKill(GameObject killedObject)
	{
		if (killedObject.IsMainPart && ((killedObject.TargetClass & _targetClass) != 0 || (_applyDemolisher && MyHero.IsDemolisherEnabled && (killedObject.PartData.TargetClass & TargetClass.Building) != 0)))
		{
			GameObject go = CreateObject(killedObject.WorldPosition, killedObject);
			killedObject.IgnoreCollisionWith(go);
		}
	}
}
