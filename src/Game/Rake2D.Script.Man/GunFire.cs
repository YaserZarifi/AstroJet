namespace Rake2D.Script.Man;

public class GunFire : BehaviorManFireBase
{
	public override void Init()
	{
		base.Init();
		_showHideAtlas = GameCore.GetSpriteAtlas("man_gun_out", logWhenNotFound: true);
		_shotData = GameCore.GetPartData("man_gun_shot");
		_hideWeaponProbability = 20;
		_jetpackId = "man_jetpack_gun";
	}
}
