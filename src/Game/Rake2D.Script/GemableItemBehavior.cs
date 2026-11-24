using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public abstract class GemableItemBehavior : ItemBehaviorScript
{
	public float DamageFactor = 1f;

	public float EqualizerFactor = 1f;

	public float EqualizerCount;

	public float HeatFactor = 1f;

	public float FireRateFactor = 1f;

	public float LifeTimeFactor = 1f;

	public float StreamAngleRandom;

	public int ChanceToBurn;

	public int ChanceToAcid;

	public int ChanceToLightning;

	public int ChanceToWave;

	public int BonusChanceToBurn;

	public int BonusChanceToAcid;

	public int BonusChanceToLightning;

	public int BonusChanceToWave;

	public float ChanceFactor = 1f;

	public int DynamoCount;

	public float DynamoDamageFromTotalDPS;

	public float BonusCriterionFactor;

	public float BonusAnglerManglerFactor;

	public bool IsEnabledAutorack;

	public bool IsEnabledMouseGun;

	public bool IsEnabledBondShot;

	public PhysClass ExtraHitTo;

	public List<BehaviorData> ShotBehaviorDataList;

	protected PartData _dynamoCheckerData;

	protected PartData _criterionData;

	protected PartData _dynamoLightningData;

	protected PartData _anglerManglerHitData;

	public float GetDamageFactor()
	{
		float num = DamageFactor;
		if (EqualizerCount > 0f && EqualizerFactor > 1f)
		{
			num *= EqualizerFactor * EqualizerCount;
		}
		return num;
	}

	public int GetFinalChanceToBurn()
	{
		return ChanceToBurn + BonusChanceToBurn;
	}

	public int GetFinalChanceToAcid()
	{
		return ChanceToAcid + BonusChanceToAcid;
	}

	public int GetFinalChanceToLightning()
	{
		return ChanceToLightning + BonusChanceToLightning;
	}

	public int GetFinalChanceToWave()
	{
		return ChanceToWave + BonusChanceToWave;
	}

	public virtual float GetDPS()
	{
		return 0f;
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		ExtraHitTo = PhysClass.Undefined;
		DamageFactor = 1f;
		EqualizerFactor = 1f;
		EqualizerCount = 0f;
		HeatFactor = 1f;
		FireRateFactor = 1f;
		LifeTimeFactor = 1f;
		ChanceToBurn = ParamInt("chance_to_burn");
		ChanceToAcid = ParamInt("chance_to_acid");
		ChanceToLightning = ParamInt("chance_to_lightning");
		ChanceToWave = ParamInt("chance_to_wave");
		BonusChanceToBurn = 0;
		BonusChanceToAcid = 0;
		BonusChanceToLightning = 0;
		BonusChanceToWave = 0;
		DynamoCount = 0;
		BonusCriterionFactor = 0f;
		BonusAnglerManglerFactor = 0f;
		StreamAngleRandom = 0f;
		ShotBehaviorDataList = null;
	}

	public override void Init()
	{
		base.Init();
		_dynamoCheckerData = GameCore.GetPartData("gem_dynamo_checker");
		_dynamoLightningData = GameCore.GetPartData("gem_dynamo_lightning_strike");
		_criterionData = GameCore.GetPartData("gem_criterion_hit");
		_anglerManglerHitData = GameCore.GetPartData("gem_angler_mangler_hit");
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		MyItem.OnHitCollision += OnHitCollision;
	}

	public virtual void OnHitCollision(GameObject shotObject, GameObject targetObject, Vector2 contactPoint, Vector2 normal, float damage)
	{
		float num = (float)GetFinalChanceToBurn() * ChanceFactor;
		if (num > 0f)
		{
			FxEffect.CreateFXPyroHit(targetObject, contactPoint);
			if ((float)Utils.GetRandom(0, 99) < num)
			{
				FxEffect.StartBurning(targetObject, MyHero);
			}
		}
		num = (float)GetFinalChanceToAcid() * ChanceFactor;
		if (num > 0f)
		{
			FxEffect.CreateFXAcidHit(targetObject, contactPoint);
			if ((float)Utils.GetRandom(0, 99) < num)
			{
				FxEffect.StartAcid(targetObject, MyHero);
			}
		}
		if (DynamoCount > 0 && (targetObject.TargetClass & TargetClass.AllEnemy) != 0)
		{
			GameObject mainPart = targetObject.GetMainPart();
			int num2 = 0;
			if (mainPart.ChildObjectListFront != null)
			{
				foreach (GameObject item in mainPart.ChildObjectListFront)
				{
					if (item.PartData == _dynamoCheckerData && item.IsAlive)
					{
						num2++;
					}
				}
			}
			for (int i = num2; i < DynamoCount; i++)
			{
				GameObject gameObject = MyLayer.CreateObjectByPartData(_dynamoCheckerData, contactPoint, Vector2.One, 0f, 1, bCreateRigidBody: false);
				gameObject.Damage = MyHero.GetTotalDPS() * DynamoDamageFromTotalDPS;
				gameObject.Init();
				gameObject.LevelInit();
				gameObject.Activate();
				mainPart.AddChildRelative(gameObject, gameObject.WorldPosition.X - mainPart.WorldPosition.X, gameObject.WorldPosition.Y - mainPart.WorldPosition.Y);
			}
		}
		if (BonusCriterionFactor > 1f && ((targetObject.HitPart != null && targetObject.HitPart.HPPercent == 1f) || (targetObject.HitPart == null && targetObject.HPPercent == 1f)) && damage > 0f)
		{
			GameObject gameObject2 = MyLayer.CreateObjectByPartData(_criterionData, contactPoint, Vector2.One, 0f, 1, bCreateRigidBody: false);
			targetObject.Hit(damage * (BonusCriterionFactor - 1f), shotObject.DamageType, shotObject);
			if (targetObject.CurrentHP > damage)
			{
				targetObject.AddChildRelative(gameObject2, gameObject2.WorldPosition.X - targetObject.WorldPosition.X, gameObject2.WorldPosition.Y - targetObject.WorldPosition.Y);
			}
		}
		if (BonusAnglerManglerFactor != 0f)
		{
			float num3 = Utils.RadToDeg(Utils.GetRotationTo(MyHero.MyObject.WorldPosition, contactPoint));
			num3 = ((!(num3 >= 0f) || !(num3 <= 90f)) ? (num3 + 90f) : (num3 - 90f));
			float num4 = Math.Abs(num3 % 90f) / 90f;
			targetObject.MyLayer.CreateObjectByPartData(_anglerManglerHitData, contactPoint, new Vector2(num4), Utils.GetRotationTo(contactPoint, MyHero.MyObject.WorldPosition), 1, bCreateRigidBody: false);
			targetObject.Hit(damage * num4 * BonusAnglerManglerFactor, shotObject.DamageType, shotObject);
		}
	}

	public override bool TryGetParamSummary(string name, out float value, out float finalValue)
	{
		if (Utils.Equals(name, "chance_to_burn"))
		{
			if (ChanceToBurn != 0 || BonusChanceToBurn != 0)
			{
				value = ChanceToBurn;
				finalValue = GetFinalChanceToBurn();
				return true;
			}
			value = 0f;
			finalValue = 0f;
			return false;
		}
		if (Utils.Equals(name, "chance_to_acid"))
		{
			if (ChanceToAcid != 0 || BonusChanceToAcid != 0)
			{
				value = ChanceToAcid;
				finalValue = GetFinalChanceToAcid();
				return true;
			}
			value = 0f;
			finalValue = 0f;
			return false;
		}
		if (Utils.Equals(name, "chance_to_lightning"))
		{
			if (ChanceToLightning != 0 || BonusChanceToLightning != 0)
			{
				value = ChanceToLightning;
				finalValue = GetFinalChanceToLightning();
				return true;
			}
			value = 0f;
			finalValue = 0f;
			return false;
		}
		if (Utils.Equals(name, "chance_to_wave"))
		{
			if (ChanceToWave != 0 || BonusChanceToWave != 0)
			{
				value = ChanceToWave;
				finalValue = GetFinalChanceToWave();
				return true;
			}
			value = 0f;
			finalValue = 0f;
			return false;
		}
		return base.TryGetParamSummary(name, out value, out finalValue);
	}
}
