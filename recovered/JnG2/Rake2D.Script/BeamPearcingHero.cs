namespace Rake2D.Script;

public class BeamPearcingHero : BehaviorBeamPearcingBase
{
	public override void Init()
	{
		base.Init();
		_hitTo = (PhysClass)43004984;
	}
}
