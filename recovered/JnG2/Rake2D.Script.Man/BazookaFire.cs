namespace Rake2D.Script.Man;

public class BazookaFire : BehaviorManFireBase
{
	public override void Init()
	{
		base.Init();
		_showHideAtlas = GameCore.GetSpriteAtlas("man_bazooka_out", logWhenNotFound: true);
		_shotData = GameCore.GetPartData("man_bazooka_missile");
		_hideWeaponProbability = 60;
		_jetpackId = "man_jetpack_bazooka";
	}
}
