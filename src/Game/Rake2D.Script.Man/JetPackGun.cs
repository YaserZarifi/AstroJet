namespace Rake2D.Script.Man;

public class JetPackGun : BehaviorManJetPackBase
{
	public override void Init()
	{
		base.Init();
		_shotData = GameCore.GetPartData("man_gun_shot");
	}
}
