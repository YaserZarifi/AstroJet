namespace Rake2D.Script;

public class PathSendLink : BehaviorScript
{
	public static string[] PathEvents = new string[1] { "link_number" };

	private string[] _linkOutArray;

	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		_linkOutArray = ParamStringArray("link_out");
	}

	public override void DoEvent(BehaviorEvent behEvent, BehaviorTimeLine sourceTimeLine)
	{
		if (GameCore.IsEditorEnabled || behEvent.EventNode.LinkedObjectList == null || _linkOutArray == null || behEvent.ValueInt < 0 || behEvent.ValueInt >= _linkOutArray.Length)
		{
			return;
		}
		foreach (GameObject linkedObject in behEvent.EventNode.LinkedObjectList)
		{
			linkedObject.DoOnLink(MyObject, _linkOutArray[behEvent.ValueInt], 1, bApplyChildren: false);
		}
	}
}
