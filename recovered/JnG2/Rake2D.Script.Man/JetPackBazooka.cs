namespace Rake2D.Script.Man;

public class JetPackBazooka : BehaviorManJetPackBase
{
	public override void Init()
	{
		base.Init();
		_shotData = GameCore.GetPartData("man_bazooka_missile");
		_shotBackForceFactor = 2f;
	}
}
