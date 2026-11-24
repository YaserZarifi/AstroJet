namespace Rake2D.Script;

public class OnKillAnim : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	public void OnKill()
	{
		int num = ParamInt("track");
		if (num == 0)
		{
			num = MyObject.PartData.SpineTrack;
		}
		if (ParamInt("reset") != 0)
		{
			MyObject.ResetSpineAnim();
		}
		string text = ParamRandomString("name");
		if (string.IsNullOrEmpty(text))
		{
			text = ((!MyObject.IsMainPart) ? (MyObject.Id + "/wreck") : "root/wreck");
		}
		MyObject.ResetSpineTrackOnDestroy = false;
		MyObject.SetSpineAnimation(num, text, bLooped: true);
		if (ParamInt("lock") != 0)
		{
			MyObject.LockSpineAnim(isLocked: true);
		}
	}
}
