using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroWeaponFactor : ItemBehaviorScript
{
	public override void ApplyToItem(HeroItem item)
	{
		base.ApplyToItem(item);
		if (item.GetBehaviorByType(typeof(GemableItemBehavior)) is GemableItemBehavior gemableItemBehavior)
		{
			gemableItemBehavior.DamageFactor *= 1f + Param("damage_factor") / 100f;
			gemableItemBehavior.FireRateFactor *= 1f + Param("fire_rate_factor") / 100f;
			gemableItemBehavior.LifeTimeFactor *= 1f + Param("life_time_factor") / 100f;
			gemableItemBehavior.HeatFactor *= 1f + Param("heat_factor") / 100f;
			gemableItemBehavior.StreamAngleRandom += Utils.DegToRad(Param("stream_angle_random"));
			gemableItemBehavior.DynamoCount += ParamInt("dynamo_count");
			gemableItemBehavior.DynamoDamageFromTotalDPS = Param("dynamo_damage_from_total_dps") / 100f;
			gemableItemBehavior.BonusCriterionFactor += Param("criterion_factor") / 100f;
			gemableItemBehavior.BonusAnglerManglerFactor += Param("angler_mangler_factor") / 100f;
			gemableItemBehavior.BonusChanceToBurn += ParamInt("chance_to_burn");
			gemableItemBehavior.BonusChanceToAcid += ParamInt("chance_to_acid");
			gemableItemBehavior.BonusChanceToLightning += ParamInt("chance_to_lightning");
			gemableItemBehavior.BonusChanceToWave += ParamInt("chance_to_wave");
		}
	}
}
