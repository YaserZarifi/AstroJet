using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.JnG;

public static class FxEffect
{
	private static PartData _fxDataBurn;

	private static PartData _fxDataAcid;

	private static PartData _fxDataWave;

	private static PartData _fxDataPyroHit;

	private static PartData _fxDataAcidHit;

	private static PartData _fxDataElectricHit;

	private static PartData _fxDataWaveHit;

	private static PartData _drunkenEffectData;

	public static void InitData()
	{
		_fxDataBurn = GameCore.GetPartData("fx_burn");
		_fxDataAcid = GameCore.GetPartData("fx_acid");
		_fxDataWave = GameCore.GetPartData("fx_wave");
		_fxDataPyroHit = GameCore.GetPartData("fx_pyro_hit");
		_fxDataAcidHit = GameCore.GetPartData("fx_acid_hit");
		_fxDataElectricHit = GameCore.GetPartData("fx_electric_hit");
		_fxDataWaveHit = GameCore.GetPartData("fx_wave_hit");
		_drunkenEffectData = GameCore.GetPartData("drunken_effect");
	}

	public static void CreateFXPyroHit(GameObject targetObject, Vector2 contactPoint)
	{
		GameObject gameObject = targetObject.CreateChildByPartData(_fxDataPyroHit, createPhysBody: true);
		if (gameObject != null)
		{
			gameObject.SetScale(1f / targetObject.WorldScale.X, 1f / targetObject.WorldScale.Y);
			gameObject.SetPosition(targetObject.GetRelativePosition(contactPoint) + Utils.GetRandomVector2(-4f, -4f, 4f, 4f));
		}
	}

	public static void CreateFXAcidHit(GameObject targetObject, Vector2 contactPoint)
	{
		GameObject gameObject = targetObject.CreateChildByPartData(_fxDataAcidHit, createPhysBody: true);
		if (gameObject != null)
		{
			gameObject.SetScale(1f / targetObject.WorldScale.X, 1f / targetObject.WorldScale.Y);
			gameObject.SetPosition(targetObject.GetRelativePosition(contactPoint) + Utils.GetRandomVector2(-4f, -4f, 4f, 4f));
		}
	}

	public static void CreateFXElectricHit(GameObject targetObject, Vector2 contactPoint)
	{
		GameObject gameObject = targetObject.CreateChildByPartData(_fxDataElectricHit, createPhysBody: true);
		if (gameObject != null)
		{
			gameObject.SetScale(1f / targetObject.WorldScale.X, 1f / targetObject.WorldScale.Y);
			gameObject.SetPosition(targetObject.GetRelativePosition(contactPoint) + Utils.GetRandomVector2(-4f, -4f, 4f, 4f));
		}
	}

	public static void CreateFXWaveHit(GameObject targetObject, Vector2 contactPoint)
	{
		GameObject gameObject = targetObject.CreateChildByPartData(_fxDataWaveHit, createPhysBody: true);
		if (gameObject != null)
		{
			gameObject.SetScale(1f / targetObject.WorldScale.X, 1f / targetObject.WorldScale.Y);
			gameObject.SetPosition(targetObject.GetRelativePosition(contactPoint) + Utils.GetRandomVector2(-4f, -4f, 4f, 4f));
		}
	}

	public static void StartBurning(GameObject targetObject, Hero hero)
	{
		GameObject gameObject = null;
		if (_fxDataBurn != null && targetObject.IsAlive && !targetObject.IsImmortal && targetObject.TargetClass != TargetClass.None && targetObject.TargetClass != TargetClass.Undefined && targetObject.GetChildByPartDataFlat(_fxDataBurn) == null)
		{
			gameObject = targetObject.CreateChildByPartData(_fxDataBurn, createPhysBody: true);
		}
		if (gameObject != null)
		{
			gameObject.MyHero = hero;
			gameObject.Damage = hero.GetBurnDPS() * FactorManager.DamageFactor;
		}
	}

	public static void StartAcid(GameObject targetObject, Hero hero)
	{
		GameObject gameObject = null;
		if (_fxDataAcid != null && targetObject.IsAlive && !targetObject.IsImmortal && targetObject.TargetClass != TargetClass.None && targetObject.TargetClass != TargetClass.Undefined && targetObject.GetChildByPartDataFlat(_fxDataAcid) == null)
		{
			gameObject = targetObject.CreateChildByPartData(_fxDataAcid, createPhysBody: true);
		}
		if (gameObject != null && hero != null)
		{
			gameObject.MyHero = hero;
			gameObject.Damage = hero.GetAcidDPS() * FactorManager.DamageFactor;
		}
	}

	public static void StartDrunkenEffect(GameLevel level)
	{
		if (_drunkenEffectData == null)
		{
			return;
		}
		List<GameObject> gameObjectList = level.MainLayer.GameObjectList;
		int count = gameObjectList.Count;
		for (int i = 0; i < count; i++)
		{
			if (gameObjectList[i].PartData == _drunkenEffectData)
			{
				return;
			}
		}
		level.MainLayer.CreateObjectByPartData(_drunkenEffectData, Vector2.Zero, Vector2.One, 0f, 0, bCreateRigidBody: false);
	}
}
