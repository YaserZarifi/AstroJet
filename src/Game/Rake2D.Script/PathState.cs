using System.Collections.Generic;

namespace Rake2D.Script;

public class PathState : BehaviorScript
{
	public static string[] PathEvents = new string[1] { "State" };

	protected bool _bExitStateToAll;

	protected List<GameObject> _exitPartListAll;

	protected bool _ignoreKilled;

	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		_exitPartListAll = ExitStateBehavior.GetExitPartList(ParamStringArray("exit_all"), MyObject, out _bExitStateToAll, ParamInt("root_mode", 1));
		_ignoreKilled = ParamBool("ignore_killed", defaultValue: true);
	}

	public override void DoEvent(BehaviorEvent behEvent, BehaviorTimeLine sourceTimeLine)
	{
		if (!Utils.Equals(behEvent.Name, "State"))
		{
			return;
		}
		if (_bExitStateToAll)
		{
			MyObject.GetMainPart().ChangePartStateToAll(behEvent.ValueString);
			return;
		}
		if (_exitPartListAll == null)
		{
			if (_ignoreKilled || MyObject.IsAlive)
			{
				MyObject.ChangePartState(behEvent.ValueString);
			}
			return;
		}
		foreach (GameObject item in _exitPartListAll)
		{
			if (_ignoreKilled || item.IsAlive)
			{
				item.ChangePartState(behEvent.ValueString);
			}
		}
	}
}
