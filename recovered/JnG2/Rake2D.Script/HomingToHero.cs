namespace Rake2D.Script;

public class HomingToHero : HomingToTarget
{
	private int _jetPackMode;

	public override void Init()
	{
		base.Init();
		_jetPackMode = ParamInt("jet_pack_mode");
	}

	protected override GameObject GetTargetObject()
	{
		return MyObject.FindNearestHeroObject(_jetPackMode);
	}
}
