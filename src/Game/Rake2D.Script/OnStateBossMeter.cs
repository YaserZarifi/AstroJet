using Rake2D.JnG;

namespace Rake2D.Script;

public class OnStateBossMeter : ExitStateBehavior
{
	private string _name;

	private bool _useMainPart;

	public override void RegisterEvents()
	{
		GameJnG.AddBossMeter(_useMainPart ? MyObject.GetMainPart() : MyObject, _name);
		if (ParamBool("hp_kill"))
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	public override void Init()
	{
		base.Init();
		_name = ParamString("name");
		if (_name != null)
		{
			_name = TextBundle.GetText(_name);
		}
		_useMainPart = ParamBool("main_part", defaultValue: true);
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.CurrentHP <= 0f)
		{
			MyObject.OnUpdate -= OnUpdate;
			MyObject.Kill();
		}
	}
}
