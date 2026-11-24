using System.Collections.Generic;
using System.Diagnostics;
using Rake2D.JnG;

namespace Rake2D;

public class GameLoader
{
	public bool Load(string fileName)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		GameJnG.ItemSetDictionary = new Dictionary<string, string[]>();
		using (IniFile iniFile = IniFile.OpenFromContent(fileName))
		{
			if (iniFile == null)
			{
				return false;
			}
			while (iniFile.NextLine())
			{
				float[] resultArray;
				if (iniFile.KeyEquals("data"))
				{
					DataLoader.LoadDataFile(iniFile.Value.ToLowerLazy(), GameCore.GlobalContent);
				}
				else if (iniFile.KeyEquals("language"))
				{
					TextBundle.Load(iniFile.Value);
				}
				else if (iniFile.KeyEquals("less_is_better_param"))
				{
					string[] stringArray = iniFile.GetStringArray();
					for (int i = 0; i < stringArray.Length; i++)
					{
						HeroItem.AddLessIsBetterParam(stringArray[i]);
					}
				}
				else if (iniFile.KeyEquals("diff_hp_factor") && iniFile.TryGetFloatArray(2, out resultArray))
				{
					FactorManager.HPFactorDefault = resultArray[0];
					FactorManager.HPInfinity = resultArray[1];
				}
				else if (iniFile.KeyEquals("diff_score_factor") && iniFile.TryGetFloatArray(2, out resultArray))
				{
					FactorManager.ScoreFactorDefault = resultArray[0];
					FactorManager.ScoreInfinity = resultArray[1];
				}
				else if (iniFile.KeyEquals("diff_damage_to_hero_factor") && iniFile.TryGetFloatArray(2, out resultArray))
				{
					FactorManager.DamageToHeroFactorDefault = resultArray[0];
					FactorManager.DamageToHeroInfinity = resultArray[1];
				}
				else if (iniFile.KeyEquals("game_mode_hp_factor"))
				{
					FactorManager.GameModeHPFactorArray = iniFile.GetFloatArray();
				}
				else if (iniFile.KeyEquals("music_mission_completed"))
				{
					GameJnG.MusicFilenameMissionCompleted = iniFile.Value;
				}
				else if (iniFile.KeyEquals("item_set") && iniFile.Id != null)
				{
					GameJnG.ItemSetDictionary[iniFile.Id] = iniFile.GetStringArray();
				}
			}
		}
		FactorManager.CheckValues();
		GameCore.Log("Defined {0} item sets", GameJnG.ItemSetDictionary.Count);
		stopwatch.Stop();
		GameCore.Log("Game loader time: {0} sec.", (float)stopwatch.ElapsedMilliseconds / 1000f);
		return true;
	}
}
