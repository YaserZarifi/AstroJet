namespace Rake2D.Script;

public class SetTargetPartBehavior : BehaviorScript
{
	private GameObject _targetPart;

	public override void Init()
	{
		base.Init();
		string text = ParamString("target_part");
		if (text != null)
		{
			if (Utils.Equals(text, "this"))
			{
				_targetPart = MyObject;
			}
			else if (Utils.Equals(text, "none"))
			{
				_targetPart = null;
			}
			else
			{
				_targetPart = MyObject.GetMainPart().GetChildByIdDeep(text);
			}
		}
	}

	public override void RegisterEvents()
	{
	}

	protected void SetTargetPart()
	{
		if (_targetPart == null || (!_targetPart.IsDestroyed && !_targetPart.IsKilled))
		{
			MyObject.GetMainPart().TargetPart = _targetPart;
		}
	}
}
