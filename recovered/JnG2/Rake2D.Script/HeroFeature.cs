namespace Rake2D.Script;

public class HeroFeature : ItemBehaviorScript
{
	public override void StartInSlot()
	{
		base.StartInSlot();
		if (ParamBool("device_connector"))
		{
			MyHero.IsConnectedDevicesEnabled = true;
		}
		if (ParamBool("demolisher"))
		{
			MyHero.IsDemolisherEnabled = true;
		}
		if (ParamBool("el_diablo"))
		{
			MyHero.IsElDiabloEnabled = true;
		}
		if (ParamBool("main_gem"))
		{
			MyHero.IsMainGemEnabled = true;
		}
		if (ParamBool("bomb_duplicator"))
		{
			MyHero.IsBombDuplicatorEnabled = true;
		}
		if (ParamBool("quazitron"))
		{
			MyHero.IsQuazitronEnabled = true;
		}
		if (ParamBool("carnage_tv"))
		{
			MyHero.IsCarnageTVEnabled = true;
		}
		if (ParamBool("zeldor_panzer"))
		{
			MyHero.IsZeldorPanzerEnabled = true;
		}
		if (ParamBool("set_enabler"))
		{
			MyHero.IsSetEnablerEnabled = true;
		}
	}
}
