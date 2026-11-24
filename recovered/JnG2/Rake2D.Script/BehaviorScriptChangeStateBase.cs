namespace Rake2D.Script;

public class BehaviorScriptChangeStateBase : BehaviorScript
{
	private GameObject _targetPart;

	public override void RegisterEvents()
	{
	}

	public void ChangeState()
	{
		if (_targetPart != null)
		{
			_targetPart.ChangePartState(ParamRandomString("state"));
		}
	}

	public override void Init()
	{
		string text = ParamString("part_id");
		if (text != null)
		{
			_targetPart = MyObject.GetMainPart().GetChildByIdDeep(text);
			if (_targetPart == null)
			{
				LogErr("Unable to find part '{0}' in object [{1}]", text, MyObject.GetMainPart().Id);
			}
		}
		else
		{
			_targetPart = MyObject;
		}
	}
}
