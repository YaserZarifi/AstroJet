using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroFactor : ItemBehaviorScript
{
	public override void StartInSlot()
	{
		base.StartInSlot();
		MyHero.ShipMaxHPFactor *= 1f + Param("hp_factor") / 100f;
		MyHero.CoolingFactor *= 1f + Param("cooling_factor") / 100f;
		MyHero.HeatFactor *= 1f + Param("heat_factor") / 100f;
		MyHero.CollisionDamageFactor *= 1f + Param("collision_damage_factor") / 100f;
		MyHero.HeatToDamageFactor *= 1f + Param("heat_to_damage_factor") / 100f;
		MyHero.ShellGrenadierFactor = Param("shell_grenadier_factor", MyHero.ShellGrenadierFactor) / 100f;
		MyHero.SuperchargedBombProbability = ParamInt("supercharged_bomb_probability", MyHero.SuperchargedBombProbability);
		MyHero.SuperchargedBombTotalDPSFactor = Param("supercharged_bomb_total_dps", MyHero.SuperchargedBombTotalDPSFactor) / 100f;
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyItem.OnStartInSlotEnd += OnStartInSlotEnd;
	}

	private void OnStartInSlotEnd()
	{
		HeroItem weaponItemForGem = MyItem.GetWeaponItemForGem();
		if (weaponItemForGem != null)
		{
			if (!weaponItemForGem.TryGetBaseParamSummary("dps", out var value, out var finalValue) && !weaponItemForGem.TryGetBaseParamSummary("collision_dps", out value, out finalValue))
			{
				finalValue = 0f;
			}
			MyHero.BurnDPS += Param("burn_dps_from_weapon_dps") / 100f * finalValue;
			MyHero.AcidDPS += Param("acid_dps_from_weapon_dps") / 100f * finalValue;
		}
	}
}
