using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Steamworks;

namespace Rake2D.JnG;

public static class SaveGame
{
	public static readonly int SAVE_GAME_VERSION = 2;

	internal static void WriteLine(string text, StreamWriter file)
	{
		if (!string.IsNullOrWhiteSpace(text))
		{
			if (Options.EncryptFiles)
			{
				file.WriteLine(StringCipher.Encrypt(text));
			}
			else
			{
				file.WriteLine(text);
			}
		}
		else if (!Options.EncryptFiles)
		{
			file.WriteLine();
		}
	}

	private static void SaveItem(HeroItem item, int index, StreamWriter file)
	{
		if (item == null)
		{
			return;
		}
		if (index >= 0)
		{
			WriteLine($"<item.{index}={item.Data.Id}>", file);
		}
		else
		{
			WriteLine($"<item={item.Data.Id}>", file);
		}
		if (item.Level > 0)
		{
			WriteLine($" lvl={item.Level}", file);
		}
		if (item.Loadout == null)
		{
			return;
		}
		WriteLine($" lout_number={item.Loadout.Number}", file);
		foreach (KeyValuePair<HeroSlotEnum, LoadoutEntry> item2 in item.Loadout.LoadoutDictionary)
		{
			WriteLine($" lout.{item2.Key}={item2.Value.ItemData.Id}, {item2.Value.ItemLevel}", file);
		}
	}

	private static void SaveHero(Hero hero, StreamWriter file)
	{
		WriteLine(null, file);
		WriteLine($"[hero={hero.PlayerNumber}]", file);
		WriteLine($"\tname={hero.PlayerName}", file);
		WriteLine($"\tface={hero.FaceNumber}", file);
		WriteLine($"\tdeath_count={hero.StatDeathCount}", file);
	}

	private static void SaveShip(HeroShip ship, StreamWriter file)
	{
		WriteLine(null, file);
		ship.PartMap?.SaveToFile(file);
		foreach (HeroSlot value in ship.Slots.Values)
		{
			WriteLine(null, file);
			WriteLine($"[slot={value.SlotType}]", file);
			WriteLine($"\tangle={Utils.RadToDeg(value.Rotation)}", file);
			WriteLine($"\toffset={(int)value.Offset.X}, {(int)value.Offset.Y}", file);
			if (value.Item != null)
			{
				SaveItem(value.Item, -1, file);
			}
		}
	}

	public static void SaveDefaultSavegame(Hero[] heroArray, int number)
	{
		try
		{
			using MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			if (Options.EncryptFiles)
			{
				streamWriter.WriteLine("JnGeNc");
			}
			WriteLine($"ver={GameJnG.SaveGameVersion}", streamWriter);
			WriteLine("rank=0", streamWriter);
			WriteLine($"game_mode={(int)GameJnG.GameMode}", streamWriter);
			WriteLine($"difficulty={GameJnG.Difficulty}", streamWriter);
			WriteLine("score=0", streamWriter);
			WriteLine($"cash={GetDefaultCash(GameJnG.GameMode)}", streamWriter);
			foreach (Hero obj in heroArray)
			{
				SaveHero(obj, streamWriter);
				SaveShip(obj.MyShip, streamWriter);
			}
			WriteLine(null, streamWriter);
			WriteLine("[cargo=0]", streamWriter);
			HeroItemData heroItemData = GameCore.GetHeroItemData("other/welcome");
			if (heroItemData != null)
			{
				SaveItem(new HeroItem(heroItemData), 0, streamWriter);
			}
			else
			{
				GameCore.LogErr("Default hero cargo item not found");
			}
			WriteLine(null, streamWriter);
			WriteLine("[shop=0]", streamWriter);
			streamWriter.Flush();
			if (Options.UseSteam && GameCore.SteamManager.IsInitialized)
			{
				SaveToCloud(memoryStream, DataUtils.GetSaveGameFileName(number));
			}
			SaveToFile(memoryStream, DataUtils.GetSaveGamePath(number));
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to save game. Ex: {0}", ex);
		}
	}

	public static void Save(int number)
	{
		MemoryStream memoryStream = SaveToMemoryStream();
		SaveToFile(memoryStream, DataUtils.GetSaveGamePath(number));
		if (Options.UseSteam && GameCore.SteamManager.IsInitialized)
		{
			SaveToCloud(memoryStream, DataUtils.GetSaveGameFileName(number));
		}
		memoryStream.Close();
	}

	private static bool SaveToFile(MemoryStream ms, string fileName)
	{
		try
		{
			using FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
			ms.WriteTo(fileStream);
			fileStream.Close();
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to save game to file {0}. Ex: {1}", fileName, ex);
		}
		return true;
	}

	private static bool SaveToCloud(MemoryStream ms, string fileName)
	{
		try
		{
			return SteamRemoteStorage.FileWrite(fileName, ms.ToArray(), (int)ms.Length);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to save game '{0}' to steam cloud. Ex: {1}", fileName, ex);
		}
		return false;
	}

	private static MemoryStream SaveToMemoryStream()
	{
		MemoryStream memoryStream = new MemoryStream();
		try
		{
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			if (streamWriter == null)
			{
				GameCore.LogErr($"Unable write savegame to memory");
				return null;
			}
			if (Options.EncryptFiles)
			{
				streamWriter.WriteLine("JnGeNc");
			}
			WriteLine($"ver={GameJnG.SaveGameVersion}", streamWriter);
			if (GameJnG.Mia)
			{
				WriteLine("mia=1", streamWriter);
			}
			if (GameJnG.IsCheater)
			{
				WriteLine("cheater=1", streamWriter);
			}
			WriteLine($"stage={GameJnG.StageNumber}", streamWriter);
			WriteLine($"game_mode={(int)GameJnG.GameMode}", streamWriter);
			WriteLine($"difficulty={GameJnG.Difficulty}", streamWriter);
			WriteLine($"score={TotalStats.Score}", streamWriter);
			WriteLine($"play_time={GameJnG.PlayTime}", streamWriter);
			WriteLine($"cash={GameJnG.Cash}", streamWriter);
			int num = 0;
			string text = "";
			foreach (GameAchievement value3 in TotalStats.GameAchievementDictionary.Values)
			{
				if (value3.IsAchieved)
				{
					if (num > 0)
					{
						text += ",";
					}
					text += value3.IdStr;
					num++;
				}
			}
			if (num > 0)
			{
				WriteLine($"ach={text}", streamWriter);
			}
			string text2 = "";
			if (TotalStats.MedalArray != null)
			{
				for (int i = 0; i < TotalStats.MedalArray.Length; i++)
				{
					text2 = ((i != 0) ? (text2 + $",{TotalStats.MedalArray[i]}") : (text2 + TotalStats.MedalArray[i]));
				}
				WriteLine($"medals={text2}", streamWriter);
			}
			if (GameJnG.LevelStatsDictionary != null)
			{
				string text3 = "";
				foreach (KeyValuePair<string, LevelStats> item in GameJnG.LevelStatsDictionary)
				{
					LevelStats value = item.Value;
					WriteLine($"stats.{item.Key}={value.FinishCountInCampaign},{value.TotalFinishCount},{value.TopScore},{value.HighestFinishDifficulty}", streamWriter);
					if (value.IsVisible)
					{
						if (text3.Length > 0)
						{
							text3 += ",";
						}
						text3 += item.Key;
					}
				}
				if (GameJnG.LevelStatsDictionary != null && GameJnG.LevelStatsDictionary.Count > 0)
				{
					WriteLine("", streamWriter);
					foreach (KeyValuePair<string, LevelStats> item2 in GameJnG.LevelStatsDictionary)
					{
						LevelStats value2 = item2.Value;
						if (value2.FindDictionary == null || value2.FindDictionary.Count <= 0)
						{
							continue;
						}
						string text4 = "";
						foreach (KeyValuePair<string, HashSet<int>> item3 in value2.FindDictionary)
						{
							if (item3.Value == null || item3.Value.Count <= 0)
							{
								continue;
							}
							int num2 = 0;
							foreach (int item4 in item3.Value.OrderBy((int n) => n))
							{
								if (num2 > 0)
								{
									text4 += ",";
								}
								text4 += item4;
								num2++;
							}
							WriteLine($"find_{item3.Key}.{item2.Key}={text4}", streamWriter);
						}
					}
				}
				if (text3.Length > 0)
				{
					WriteLine("", streamWriter);
					WriteLine($"visible={text3}", streamWriter);
				}
			}
			WriteLine("", streamWriter);
			WriteLine($"stat_air_kills={TotalStats.KillCountAirEnemy}", streamWriter);
			WriteLine($"stat_ground_kills={TotalStats.KillCountGroundEnemy}", streamWriter);
			WriteLine($"stat_boss_kills={TotalStats.KillCountBoss}", streamWriter);
			WriteLine($"stat_man_kills={TotalStats.KillCountMan}", streamWriter);
			WriteLine($"stat_level_finishes={TotalStats.LevelWinCount}", streamWriter);
			WriteLine($"stat_dmg_causing={Utils.FormatDouble2(TotalStats.DamageCaused)}", streamWriter);
			WriteLine($"stat_dmg_absorbed={Utils.FormatFloat2(TotalStats.HeroDamageAbsorbed)}", streamWriter);
			WriteLine($"stat_shots_fired={TotalStats.FiredShots}", streamWriter);
			WriteLine($"stat_cargo_unlock={TotalStats.CargoUnlockCount}", streamWriter);
			WriteLine($"stat_bonuses={TotalStats.BonusTakenCount}", streamWriter);
			WriteLine($"atarix_autohack={GameJnG.AtarixAutoHackChance}", streamWriter);
			if (GameJnG.BountyVillainSet.Count > 0)
			{
				string text5 = "villain=";
				int num3 = 0;
				foreach (string item5 in GameJnG.BountyVillainSet)
				{
					if (num3 > 0)
					{
						text5 += ",";
					}
					text5 += item5;
					num3++;
				}
				WriteLine("", streamWriter);
				WriteLine(text5, streamWriter);
			}
			foreach (Hero hero in GameJnG.HeroList)
			{
				SaveHero(hero, streamWriter);
				SaveShip(hero.MyShip, streamWriter);
				if (hero.CurrentLoadoutItem != null)
				{
					WriteLine($"[loadout={hero.PlayerNumber}]", streamWriter);
					SaveItem(hero.CurrentLoadoutItem, -1, streamWriter);
				}
			}
			WriteLine(null, streamWriter);
			WriteLine("[cargo=0]", streamWriter);
			foreach (KeyValuePair<int, HeroItem> item6 in GameJnG.Cargo.Items)
			{
				SaveItem(item6.Value, item6.Key, streamWriter);
			}
			WriteLine(null, streamWriter);
			WriteLine("[shop=0]", streamWriter);
			foreach (HeroItem item7 in GameJnG.ShopOffer.ItemList)
			{
				SaveItem(item7, -1, streamWriter);
			}
			streamWriter.Flush();
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to save game. Ex: {0}", ex);
		}
		return memoryStream;
	}

	private static void Clear()
	{
		GameJnG.Cargo.Clear();
		foreach (Hero hero in GameJnG.HeroList)
		{
			hero.DestroySmallShip();
		}
		GameJnG.HeroList.Clear();
		GameJnG.ShopOffer.Clear();
		GameJnG.Cash = 0;
		GameJnG.PlayTime = 0f;
		GameJnG.StageNumber = 0;
		GameJnG.AtarixAutoHackChance = 0;
		GameJnG.Mia = false;
		GameJnG.GameMode = GameModeEnum.Standard;
		GameJnG.LevelStatsDictionary?.Clear();
		GameJnG.BountyVillainSet.Clear();
	}

	public static SaveGameInfo LoadSaveGameInfo(int number)
	{
		IniFile iniFile = new IniFile();
		HeroShip heroShip = null;
		Hero hero = null;
		string text = $"save_{number}.jsg";
		if (Options.UseSteam && GameCore.SteamManager.IsInitialized)
		{
			string pchFile = text;
			if (!SteamRemoteStorage.FileExists(pchFile))
			{
				return null;
			}
			int fileSize = SteamRemoteStorage.GetFileSize(pchFile);
			byte[] array = new byte[fileSize];
			SteamRemoteStorage.FileRead(pchFile, array, fileSize);
			if (!iniFile.OpenFromStream(new MemoryStream(array)))
			{
				return null;
			}
		}
		else
		{
			string pchFile = Path.Combine(DataUtils.GetSaveDirectory(), text);
			if (!File.Exists(pchFile) || !iniFile.OpenFromFile(pchFile))
			{
				return null;
			}
		}
		SaveGameInfo saveGameInfo = new SaveGameInfo();
		while (iniFile.NextLine())
		{
			if (iniFile.KeyEquals("score"))
			{
				saveGameInfo.Score = iniFile.GetIntValue();
			}
			else if (iniFile.KeyEquals("mia"))
			{
				saveGameInfo.Mia = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("game_mode"))
			{
				int intValue = iniFile.GetIntValue();
				if (Enum.IsDefined(typeof(GameModeEnum), intValue))
				{
					saveGameInfo.GameMode = (GameModeEnum)intValue;
				}
				else
				{
					saveGameInfo.GameMode = GameModeEnum.Standard;
				}
			}
			else if (iniFile.KeyEquals("difficulty"))
			{
				int intValue2 = iniFile.GetIntValue();
				if (intValue2 < 1)
				{
					saveGameInfo.GameMode = GameModeEnum.Casual;
					saveGameInfo.Difficulty = 1;
				}
				else
				{
					saveGameInfo.Difficulty = intValue2;
				}
			}
			else if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("hero"))
			{
				if (iniFile.GetIntValue() == 0)
				{
					saveGameInfo.Hero = new Hero();
					hero = saveGameInfo.Hero;
				}
				else
				{
					saveGameInfo.Hero2 = new Hero();
					hero = saveGameInfo.Hero2;
				}
				heroShip = new HeroShip(iniFile.Value);
				hero.SetShip(heroShip);
			}
			else if (hero != null)
			{
				if (iniFile.KeyEquals("name"))
				{
					hero.PlayerName = iniFile.Value;
				}
				else if (iniFile.KeyEquals("face"))
				{
					hero.FaceNumber = iniFile.GetIntValue();
				}
				else if (iniFile.ValueType == IniFileValueType.Normal && iniFile.KeyEquals("ship"))
				{
					hero.ShipId = iniFile.Value;
				}
			}
			if (heroShip == null)
			{
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("part_map"))
			{
				heroShip.PartMap.SkinId = iniFile.Value;
			}
			else if (heroShip.PartMap != null)
			{
				if (iniFile.KeyEquals("m"))
				{
					heroShip.PartMap.AddItem(iniFile.Id, iniFile.Value);
				}
				else if (iniFile.KeyEquals("ox"))
				{
					heroShip.PartMap.PartOffsetXPercentages[iniFile.Id] = iniFile.GetFloatValue() / 100f;
				}
			}
		}
		iniFile.Close();
		for (int i = 0; i < 2; i++)
		{
			hero = ((i == 0) ? saveGameInfo.Hero : saveGameInfo.Hero2);
			if (hero != null && hero.MyShip == null)
			{
				GameCore.LogErr("Undefined ship for [hero = {0}] - create default ship", hero.PlayerNumber);
				hero.CreateDefaultShip();
			}
		}
		return saveGameInfo;
	}

	public static bool Load(int saveGameNumber, bool bFromCloud)
	{
		bool flag = false;
		if ((Options.UseSteam & bFromCloud) && GameCore.SteamManager.IsInitialized)
		{
			try
			{
				string saveGameFileName = DataUtils.GetSaveGameFileName(saveGameNumber);
				if (SteamRemoteStorage.FileExists(saveGameFileName))
				{
					int fileSize = SteamRemoteStorage.GetFileSize(saveGameFileName);
					byte[] array = new byte[fileSize];
					SteamRemoteStorage.FileRead(saveGameFileName, array, fileSize);
					flag = Load(new MemoryStream(array));
				}
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Unable to load save game number {0} from Steam. Ex: '{1}'", saveGameNumber, ex.ToString());
			}
		}
		else
		{
			try
			{
				flag = Load(File.OpenRead(DataUtils.GetSaveGamePath(saveGameNumber)));
			}
			catch (Exception ex2)
			{
				GameCore.LogErr("Unable to load save game number {0}. Ex: '{1}'", saveGameNumber, ex2.ToString());
			}
		}
		Options.SaveGameNumber = saveGameNumber;
		if (flag)
		{
			GameJnG.SaveGameNumber = Options.SaveGameNumber;
		}
		else
		{
			GameJnG.SaveGameNumber = 0;
		}
		return flag;
	}

	private static bool Load(Stream stream)
	{
		IniFile iniFile = new IniFile();
		Hero hero = null;
		HeroSlot heroSlot = null;
		HeroShip heroShip = null;
		HeroItem heroItem = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		GameJnG.LevelStatsDictionary = new Dictionary<string, LevelStats>();
		GameJnG.SetCheater(isCheater: false);
		GameJnG.SaveGameVersion = 0;
		Campaign.HideAllSecretLevels();
		TotalStats.Clear();
		if (!iniFile.OpenFromStream(stream))
		{
			return false;
		}
		Clear();
		while (iniFile.NextLine())
		{
			if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("hero"))
			{
				int intValue = iniFile.GetIntValue();
				if (intValue >= 0 && intValue <= 1)
				{
					hero = GameJnG.CreateHero(intValue);
					heroShip = new HeroShip($"ship_{intValue}");
					hero.SetShip(heroShip);
					heroSlot = null;
					flag = false;
					flag2 = false;
					flag3 = false;
					heroItem = null;
					continue;
				}
				hero = null;
				iniFile.LogErr("Wrong player number {0}", intValue);
				return false;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("shop"))
			{
				hero = null;
				heroSlot = null;
				heroShip = null;
				flag = true;
				flag2 = false;
				flag3 = false;
				heroItem = null;
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("cargo"))
			{
				hero = null;
				heroSlot = null;
				heroShip = null;
				flag = false;
				flag2 = false;
				flag3 = true;
				heroItem = null;
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("loadout"))
			{
				heroSlot = null;
				heroShip = null;
				flag = false;
				flag2 = true;
				flag3 = false;
				heroItem = null;
				continue;
			}
			if (heroShip != null && iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("slot"))
			{
				if (!Enum.TryParse<HeroSlotEnum>(iniFile.Value, out var result))
				{
					iniFile.LogErr("Wrong slot type '{0}'", iniFile.Value);
				}
				else
				{
					heroSlot = heroShip.GetSlot(result);
				}
				flag = false;
				heroItem = null;
				continue;
			}
			if (heroShip != null && iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("part_map"))
			{
				heroShip.PartMap.SkinId = iniFile.Value;
				continue;
			}
			string[] resultArray;
			if (heroShip != null && heroShip.PartMap != null)
			{
				if (iniFile.KeyEquals("m"))
				{
					heroShip.PartMap.AddItem(iniFile.Id, iniFile.Value);
					continue;
				}
				if (iniFile.KeyEquals("ox"))
				{
					heroShip.PartMap.PartOffsetXPercentages[iniFile.Id] = iniFile.GetFloatValue() / 100f;
					continue;
				}
			}
			else
			{
				if (iniFile.KeyEquals("stage"))
				{
					GameJnG.StageNumber = iniFile.GetIntValue();
					continue;
				}
				if (iniFile.KeyEquals("ver"))
				{
					GameJnG.SaveGameVersion = iniFile.GetIntValue();
					continue;
				}
				if (iniFile.KeyEquals("mia"))
				{
					GameJnG.Mia = iniFile.GetBoolValue();
					continue;
				}
				if (iniFile.KeyEquals("score"))
				{
					TotalStats.Score = iniFile.GetIntValue();
				}
				else
				{
					if (iniFile.KeyEquals("cash"))
					{
						GameJnG.Cash = iniFile.GetIntValue();
						continue;
					}
					if (iniFile.KeyEquals("villain") && iniFile.TryGetStringArray(1, 9999, out resultArray))
					{
						string[] array = resultArray;
						foreach (string item in array)
						{
							GameJnG.BountyVillainSet.Add(item);
						}
						continue;
					}
					if (iniFile.KeyEquals("play_time"))
					{
						GameJnG.PlayTime = iniFile.GetFloatValue();
					}
					else
					{
						if (iniFile.KeyEquals("cheater"))
						{
							GameJnG.SetCheater(iniFile.GetBoolValue());
							continue;
						}
						if (iniFile.KeyEquals("game_mode"))
						{
							GameJnG.SetGameModeByNumber(iniFile.GetIntValue());
							continue;
						}
						if (iniFile.KeyEquals("difficulty"))
						{
							int intValue2 = iniFile.GetIntValue();
							if (intValue2 < 1)
							{
								GameJnG.GameMode = GameModeEnum.Casual;
								GameJnG.SetDifficulty(1);
							}
							else
							{
								GameJnG.SetDifficulty(intValue2);
							}
							continue;
						}
						if (iniFile.KeyEquals("atarix_autohack"))
						{
							GameJnG.AtarixAutoHackChance = Math.Min(iniFile.GetIntValue(), 100);
							continue;
						}
						if (!GameCore.SteamManager.IsInitialized && iniFile.KeyEquals("ach") && iniFile.TryGetStringArray(0, 999, out resultArray))
						{
							string[] array = resultArray;
							for (int i = 0; i < array.Length; i++)
							{
								GameAchievementEnum achievementIdByString = GameJnG.GetAchievementIdByString(array[i]);
								if (achievementIdByString != GameAchievementEnum.UNKNOWN && TotalStats.GameAchievementDictionary.TryGetValue(achievementIdByString, out var value))
								{
									value.IsAchieved = true;
								}
							}
							continue;
						}
						if (iniFile.KeyEquals("medals") && iniFile.TryGetIntArray(TotalStats.MedalArray.Length, out var resultArray2))
						{
							resultArray2.CopyTo(TotalStats.MedalArray, 0);
							continue;
						}
						if (iniFile.KeyEquals("stats") && iniFile.TryGetIntArray(4, out resultArray2))
						{
							string id = iniFile.Id;
							LevelStats levelStats = new LevelStats(iniFile.Id);
							levelStats.FinishCountInCampaign = resultArray2[0];
							levelStats.TotalFinishCount = resultArray2[1];
							levelStats.TopScore = resultArray2[2];
							levelStats.HighestFinishDifficulty = resultArray2[3];
							GameJnG.LevelStatsDictionary[id] = levelStats;
							continue;
						}
						if (iniFile.KeyEquals("visible"))
						{
							string[] array = iniFile.GetStringArray();
							foreach (string text in array)
							{
								if (!GameJnG.LevelStatsDictionary.TryGetValue(text, out var value2))
								{
									value2 = new LevelStats(text);
									GameJnG.LevelStatsDictionary[text] = value2;
								}
								value2.IsVisible = true;
							}
						}
						else if (iniFile.Key.StartsWith("find_") && iniFile.TryGetIntArray(1, 999, out resultArray2))
						{
							string key = iniFile.Key.Substring(5);
							string id2 = iniFile.Id;
							if (!GameJnG.LevelStatsDictionary.TryGetValue(id2, out var value3))
							{
								value3 = new LevelStats(id2);
								GameJnG.LevelStatsDictionary[id2] = value3;
							}
							if (value3.FindDictionary == null)
							{
								value3.FindDictionary = new Dictionary<string, HashSet<int>>();
							}
							value3.FindDictionary.Add(key, new HashSet<int>(resultArray2));
						}
						else
						{
							if (iniFile.KeyEquals("stat_air_kills"))
							{
								TotalStats.KillCountAirEnemy = iniFile.GetIntValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_ground_kills"))
							{
								TotalStats.KillCountGroundEnemy = iniFile.GetIntValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_boss_kills"))
							{
								TotalStats.KillCountBoss = iniFile.GetIntValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_man_kills"))
							{
								TotalStats.KillCountMan = iniFile.GetIntValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_bonuses"))
							{
								TotalStats.BonusTakenCount = iniFile.GetIntValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_level_finishes"))
							{
								TotalStats.LevelWinCount = iniFile.GetIntValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_cargo_unlock"))
							{
								TotalStats.CargoUnlockCount = iniFile.GetIntValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_shots_fired"))
							{
								TotalStats.FiredShots = iniFile.GetIntValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_dmg_causing"))
							{
								TotalStats.DamageCaused = iniFile.GetDoubleValue();
								continue;
							}
							if (iniFile.KeyEquals("stat_dmg_absorbed"))
							{
								TotalStats.HeroDamageAbsorbed = iniFile.GetFloatValue();
								continue;
							}
						}
					}
				}
			}
			if (heroSlot != null)
			{
				if (iniFile.KeyEquals("angle"))
				{
					heroSlot.Rotation = Utils.DegToRad(iniFile.GetIntValue());
					continue;
				}
				if (iniFile.KeyEquals("offset"))
				{
					heroSlot.Offset = iniFile.GetVector2();
					continue;
				}
			}
			else if (hero != null)
			{
				if (iniFile.KeyEquals("name"))
				{
					hero.PlayerName = iniFile.Value;
					continue;
				}
				if (iniFile.KeyEquals("face"))
				{
					hero.FaceNumber = iniFile.GetIntValue();
					continue;
				}
				if (iniFile.KeyEquals("death_count"))
				{
					hero.StatDeathCount = iniFile.GetIntValue();
					continue;
				}
				if (iniFile.KeyEquals("ship") && iniFile.ValueType == IniFileValueType.Normal)
				{
					hero.ShipId = iniFile.Value;
					continue;
				}
			}
			if (iniFile.ValueType == IniFileValueType.SubSection && iniFile.KeyEquals("item"))
			{
				HeroItemData heroItemData = GameCore.GetHeroItemData(iniFile.Value);
				if (heroItemData != null)
				{
					heroItem = new HeroItem(heroItemData);
					if (flag3)
					{
						GameJnG.Cargo.SetItem(iniFile.GetIdIntValue(), heroItem);
					}
					else if (flag)
					{
						GameJnG.ShopOffer.AddItem(heroItem);
					}
					else if (flag2)
					{
						hero.CurrentLoadoutItem = heroItem;
					}
					else if (heroSlot != null)
					{
						heroSlot.SetItem(heroItem, bReconfigurateShip: false);
					}
					else
					{
						iniFile.LogErr("Item without place");
					}
				}
				else
				{
					iniFile.LogErr("Undefined item '{0}'", iniFile.Value);
				}
				heroSlot = null;
			}
			else
			{
				if (heroItem == null)
				{
					continue;
				}
				if (iniFile.KeyEquals("lvl"))
				{
					heroItem.Level = iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("lout_number"))
				{
					heroItem.Loadout = new Loadout(iniFile.GetIntValue());
				}
				else
				{
					if (!iniFile.KeyEquals("lout") || !iniFile.TryGetStringArray(2, out resultArray) || heroItem.Loadout == null)
					{
						continue;
					}
					HeroItemData heroItemData2 = GameCore.GetHeroItemData(resultArray[0]);
					if (heroItemData2 != null)
					{
						LoadoutEntry loadoutEntry = new LoadoutEntry();
						if (Enum.TryParse<HeroSlotEnum>(iniFile.Id, out loadoutEntry.Slot))
						{
							if (int.TryParse(resultArray[1], out loadoutEntry.ItemLevel))
							{
								loadoutEntry.ItemData = heroItemData2;
								heroItem.Loadout.LoadoutDictionary.Add(loadoutEntry.Slot, loadoutEntry);
							}
							else
							{
								iniFile.LogErr("Wrong loadout level '{0}'.", resultArray[1]);
							}
						}
						else
						{
							iniFile.LogErr("Wrong loadout slot '{0}'.", iniFile.Id);
						}
					}
					else
					{
						iniFile.LogErr("Undefined loadout item '{0}'.", resultArray[0]);
					}
				}
			}
		}
		iniFile.Close();
		for (int num = GameJnG.HeroList.Count - 1; num >= 0; num--)
		{
			Hero hero2 = GameJnG.HeroList[num];
			if (hero2.MyShip == null)
			{
				GameCore.Log("Undefined ship for [hero = {0}] - create default ship", hero2.PlayerNumber);
				hero2.CreateDefaultShip();
			}
		}
		GameJnG.RecalculateItemParams(null);
		GameJnG.IsFirstLevelTry = GameJnG.GetIsFirstLevelTry();
		if (GameJnG.ShopMenu != null)
		{
			GameJnG.ShopMenu.ScreenMission.SelectUnfinishedStageLevel(GameJnG.StageNumber);
		}
		TotalStats.Recalculate();
		return true;
	}

	public static void Delete(int profileNumber)
	{
		string saveGameFileName;
		if (Options.UseSteam && GameCore.SteamManager.IsInitialized)
		{
			saveGameFileName = DataUtils.GetSaveGameFileName(profileNumber);
			if (SteamRemoteStorage.FileExists(saveGameFileName))
			{
				SteamRemoteStorage.FileDelete(saveGameFileName);
			}
		}
		saveGameFileName = DataUtils.GetSaveGamePath(profileNumber);
		if (File.Exists(saveGameFileName))
		{
			File.Delete(saveGameFileName);
		}
	}

	public static int GetDefaultCash(GameModeEnum gameMode)
	{
		if (gameMode == GameModeEnum.Casual)
		{
			return 5000;
		}
		return 1000;
	}

	public static void CreateDefaultValues()
	{
		Clear();
		GameJnG.Cash = GetDefaultCash(GameJnG.GameMode);
		GameJnG.SetDifficulty(1);
		Campaign.HideAllSecretLevels();
		Hero hero = GameJnG.CreateHero(0);
		hero.CreateDefaultShip();
		hero.PlayerName = TextBundle.GetText("default_player_name_1");
		HeroItemData heroItemData = GameCore.GetHeroItemData("other/welcome");
		if (heroItemData != null)
		{
			GameJnG.Cargo.SetItem(0, new HeroItem(heroItemData));
		}
	}
}
