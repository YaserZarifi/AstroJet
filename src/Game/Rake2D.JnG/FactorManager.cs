using System;

namespace Rake2D.JnG;

public static class FactorManager
{
	public static float HPFactorDefault;

	public static float ScoreFactorDefault;

	public static float DamageToHeroFactorDefault;

	public static float[] GameModeHPFactorArray;

	public static float HPInfinity = 1f;

	public static float ScoreInfinity = 1f;

	public static float DamageToHeroInfinity = 1f;

	public static float HPFactor { get; private set; }

	public static float DamageFactor { get; private set; }

	public static float DamageToHeroFactor { get; private set; }

	public static float ScoreFactor { get; private set; }

	internal static void CheckValues()
	{
		if (GameModeHPFactorArray == null)
		{
			GameCore.LogErr("game_mode_hp_factor not defined in '{0}'.", "game.txt");
			GameModeHPFactorArray = new float[1] { 1f };
		}
	}

	public static float GetDifficultyHPFactor(int difficulty)
	{
		if (difficulty < 0)
		{
			return 1f;
		}
		return HPFactorDefault + (float)difficulty * HPInfinity;
	}

	public static float GetGameModeHPFactor(GameModeEnum gameMode)
	{
		if (GameModeHPFactorArray.Length > (int)gameMode)
		{
			return GameModeHPFactorArray[(int)gameMode];
		}
		return 1f;
	}

	public static float GetDifficultyScoreFactor(int difficulty)
	{
		if (difficulty < 0)
		{
			return 1f;
		}
		return ScoreFactorDefault + (float)difficulty * ScoreInfinity;
	}

	public static float GetDifficultyDamageToHeroFactor(int difficulty)
	{
		if (difficulty < 0)
		{
			return 1f;
		}
		return DamageToHeroFactorDefault + (float)difficulty * DamageToHeroInfinity;
	}

	private static float GetHPFactor(LevelData levelData)
	{
		float num = 0f;
		if (GameJnG.IsTestRangeEnabled)
		{
			num = Math.Max(0f, GameJnG.Campaign.GetStageData(GameJnG.StageNumber).TestRangeHPFactor);
		}
		num = ((levelData == null || !(levelData.HPFactor >= 0f)) ? GetDifficultyHPFactor(GameJnG.Difficulty) : levelData.HPFactor);
		if (GameJnG.HeroList.Count > 1)
		{
			num *= 1.5f;
		}
		return Math.Max(0f, num);
	}

	private static float GetDamageFactor(LevelData levelData)
	{
		return GetHPFactor(levelData);
	}

	private static float GetScoreFactor(LevelData levelData)
	{
		if (GameCore.GameLevel.IgnoreTakenValues)
		{
			return 0f;
		}
		float num = ((levelData == null || !(levelData.ScoreFactor >= 0f)) ? Math.Max(0f, GetDifficultyScoreFactor(GameJnG.Difficulty)) : levelData.ScoreFactor);
		if (GameJnG.HeroList.Count > 1)
		{
			num *= 1.2f;
		}
		return num;
	}

	private static float GetDamageToHeroFactor(LevelData levelData)
	{
		if (levelData != null && levelData.DamageToHeroFactor >= 0f)
		{
			return levelData.DamageToHeroFactor;
		}
		return Math.Max(0f, GetDifficultyDamageToHeroFactor(GameJnG.Difficulty));
	}

	public static void ActualizeFactors(LevelData levelData)
	{
		if (Options.UseDefaultFactors)
		{
			HPFactor = 1f;
			DamageFactor = 1f;
			DamageToHeroFactor = 1f;
			ScoreFactor = 1f;
		}
		else
		{
			int stageNumberByLevelData = GameJnG.Campaign.GetStageNumberByLevelData(levelData);
			StageData stageData = GameJnG.Campaign.GetStageData(GameJnG.StageNumber);
			LevelData levelData2 = ((stageData == null || (levelData != null && (stageNumberByLevelData < 0 || stageNumberByLevelData >= GameJnG.StageNumber))) ? levelData : stageData.LevelDataList[0]);
			HPFactor = GetHPFactor(levelData2) * GetGameModeHPFactor(GameJnG.GameMode);
			DamageFactor = GetDamageFactor(levelData2);
			DamageToHeroFactor = GetDamageToHeroFactor(levelData2);
			ScoreFactor = GetScoreFactor(levelData);
		}
	}
}
