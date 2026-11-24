namespace Rake2D.Script;

public class BeamEnemy : BehaviorBeamBase
{
	public override void Init()
	{
		base.Init();
		_hitTo = (PhysClass)67113987;
	}
}
