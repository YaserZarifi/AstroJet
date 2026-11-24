namespace Rake2D.Script;

public class BeamNoCollison : BehaviorBeamBase
{
	public override void Init()
	{
		base.Init();
		_hitTo = PhysClass.None;
	}
}
