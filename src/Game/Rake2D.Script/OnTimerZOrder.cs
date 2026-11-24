namespace Rake2D.Script;

public class OnTimerZOrder : OnTimerBehavior
{
	private int _zorder;

	public override void Init()
	{
		base.Init();
		_zorder = ParamInt("zorder");
	}

	protected override void DoAction()
	{
		MyObject.ZOrder = _zorder;
		base.DoAction();
	}
}
