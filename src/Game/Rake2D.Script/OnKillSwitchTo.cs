namespace Rake2D.Script;

public class OnKillSwitchTo : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		MyObject.SwitchToObject(ParamRandomString("object_id"), bAssumeChildren: true, ParamRandomVector2Range("offset"), (!ParamBool("destroy")) ? SwitchObjectKillModeEnum.Destroy : SwitchObjectKillModeEnum.None);
	}
}
