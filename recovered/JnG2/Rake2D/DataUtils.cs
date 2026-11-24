using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;

namespace Rake2D;

public static class DataUtils
{
	public static BlendState BlendMode2State(BlendModeEnum mode)
	{
		if (mode != BlendModeEnum.Additive)
		{
			return BlendState.AlphaBlend;
		}
		return BlendState.Additive;
	}

	public static ColorMode GetColorModeByString(string str)
	{
		if (str == null)
		{
			return ColorMode.Local;
		}
		if (Utils.Equals(str, "parent"))
		{
			return ColorMode.ParentRGBA;
		}
		if (Utils.Equals(str, "parent_alpha"))
		{
			return ColorMode.ParentAlpha;
		}
		if (Utils.Equals(str, "main_alpha"))
		{
			return ColorMode.MainAlpha;
		}
		if (Utils.Equals(str, "parent_rgb"))
		{
			return ColorMode.ParentRGB;
		}
		if (Utils.Equals(str, "parent_rgba2"))
		{
			return ColorMode.ParentRGBA2;
		}
		if (Utils.Equals(str, "local"))
		{
			return ColorMode.Local;
		}
		return ColorMode.Unknown;
	}

	public static ItemTypeEnum GetItemTypeByString(string str)
	{
		if (Utils.Equals(str, "cannon"))
		{
			return ItemTypeEnum.Cannon;
		}
		if (Utils.Equals(str, "missile"))
		{
			return ItemTypeEnum.Missile;
		}
		if (Utils.Equals(str, "blaster"))
		{
			return ItemTypeEnum.Blaster;
		}
		if (Utils.Equals(str, "beam"))
		{
			return ItemTypeEnum.Beam;
		}
		if (Utils.Equals(str, "bomb"))
		{
			return ItemTypeEnum.Bomb;
		}
		if (Utils.Equals(str, "device"))
		{
			return ItemTypeEnum.Device;
		}
		if (Utils.Equals(str, "mana_device"))
		{
			return ItemTypeEnum.Mana_device;
		}
		if (Utils.Equals(str, "equipment"))
		{
			return ItemTypeEnum.Equipment;
		}
		if (Utils.Equals(str, "gunbot"))
		{
			return ItemTypeEnum.Gunbot;
		}
		if (Utils.Equals(str, "rammer"))
		{
			return ItemTypeEnum.Rammer;
		}
		if (Utils.Equals(str, "satelite"))
		{
			return ItemTypeEnum.Satelite;
		}
		if (Utils.Equals(str, "shield"))
		{
			return ItemTypeEnum.Shield;
		}
		if (Utils.Equals(str, "armor"))
		{
			return ItemTypeEnum.Armor;
		}
		if (Utils.Equals(str, "gem"))
		{
			return ItemTypeEnum.Gem;
		}
		if (Utils.Equals(str, "medal"))
		{
			return ItemTypeEnum.Medal;
		}
		if (Utils.Equals(str, "loadout"))
		{
			return ItemTypeEnum.Loadout;
		}
		return ItemTypeEnum.Common;
	}

	public static DamageTypeEnum GetDamageTypeByString(string str)
	{
		if (Utils.Equals(str, "none"))
		{
			return DamageTypeEnum.None;
		}
		if (Utils.Equals(str, "acid"))
		{
			return DamageTypeEnum.Acid;
		}
		if (Utils.Equals(str, "bullet"))
		{
			return DamageTypeEnum.Bullet;
		}
		if (Utils.Equals(str, "cold"))
		{
			return DamageTypeEnum.Cold;
		}
		if (Utils.Equals(str, "collision"))
		{
			return DamageTypeEnum.Collision;
		}
		if (Utils.Equals(str, "electric"))
		{
			return DamageTypeEnum.Electric;
		}
		if (Utils.Equals(str, "energy"))
		{
			return DamageTypeEnum.Energy;
		}
		if (Utils.Equals(str, "explosion"))
		{
			return DamageTypeEnum.Explosion;
		}
		if (Utils.Equals(str, "fire"))
		{
			return DamageTypeEnum.Fire;
		}
		return DamageTypeEnum.Unknown;
	}

	public static EnvironmentEnum GetEnvironmentByString(string str)
	{
		if (Utils.Equals(str, "air"))
		{
			return EnvironmentEnum.Air;
		}
		if (Utils.Equals(str, "water"))
		{
			return EnvironmentEnum.Water;
		}
		if (Utils.Equals(str, "all"))
		{
			return EnvironmentEnum.All;
		}
		return EnvironmentEnum.None;
	}

	public static PhysClass GetPhysClassByString(string str)
	{
		if (Utils.Equals(str, "none"))
		{
			return PhysClass.None;
		}
		if (Utils.Equals(str, "enemy_air"))
		{
			return PhysClass.EnemyAir;
		}
		if (Utils.Equals(str, "enemy_ground"))
		{
			return PhysClass.EnemyGround;
		}
		if (Utils.Equals(str, "enemy_back"))
		{
			return PhysClass.EnemyBack;
		}
		if (Utils.Equals(str, "ground"))
		{
			return PhysClass.Ground;
		}
		if (Utils.Equals(str, "platform"))
		{
			return PhysClass.Platform;
		}
		if (Utils.Equals(str, "element_hero"))
		{
			return PhysClass.ElementHero;
		}
		if (Utils.Equals(str, "water"))
		{
			return PhysClass.Water;
		}
		if (Utils.Equals(str, "shot_hero"))
		{
			return PhysClass.ShotHero;
		}
		if (Utils.Equals(str, "bomb_hero"))
		{
			return PhysClass.BombHero;
		}
		if (Utils.Equals(str, "ghost"))
		{
			return PhysClass.Ghost;
		}
		if (Utils.Equals(str, "ball"))
		{
			return PhysClass.Ball;
		}
		if (Utils.Equals(str, "shot_enemy"))
		{
			return PhysClass.ShotEnemy;
		}
		if (Utils.Equals(str, "missile_enemy"))
		{
			return PhysClass.MissileEnemy;
		}
		if (Utils.Equals(str, "ufo"))
		{
			return PhysClass.Ufo;
		}
		if (Utils.Equals(str, "man"))
		{
			return PhysClass.Man;
		}
		if (Utils.Equals(str, "hero"))
		{
			return PhysClass.Hero;
		}
		if (Utils.Equals(str, "hero_sensor"))
		{
			return PhysClass.HeroSensor;
		}
		if (Utils.Equals(str, "drone"))
		{
			return PhysClass.Drone;
		}
		if (Utils.Equals(str, "jet_pack"))
		{
			return PhysClass.JetPack;
		}
		if (Utils.Equals(str, "bonus"))
		{
			return PhysClass.Bonus;
		}
		if (Utils.Equals(str, "building"))
		{
			return PhysClass.Building;
		}
		if (Utils.Equals(str, "ruins"))
		{
			return PhysClass.Ruins;
		}
		if (Utils.Equals(str, "misc_enemy"))
		{
			return PhysClass.MiscEnemy;
		}
		if (Utils.Equals(str, "misc_hero"))
		{
			return PhysClass.MiscHero;
		}
		if (Utils.Equals(str, "wreck"))
		{
			return PhysClass.Wreck;
		}
		if (Utils.Equals(str, "xeno_sensor"))
		{
			return PhysClass.XenoSensor;
		}
		if (Utils.Equals(str, "decoration"))
		{
			return PhysClass.Decoration;
		}
		if (Utils.Equals(str, "decoration_large"))
		{
			return PhysClass.DecorationLarge;
		}
		if (Utils.Equals(str, "particle"))
		{
			return PhysClass.Particle;
		}
		return PhysClass.Undefined;
	}

	public static TargetClass GetTargetClassByString(string str)
	{
		return GetTargetClassByStringArray(str.Split(','));
	}

	public static TargetClass GetTargetClassByStringArray(string[] classIdArray)
	{
		TargetClass targetClass = TargetClass.None;
		if (classIdArray != null)
		{
			foreach (string value in classIdArray)
			{
				if (Utils.Equals(value, "none"))
				{
					targetClass = TargetClass.None;
				}
				else if (Utils.Equals(value, "enemy_air"))
				{
					targetClass |= TargetClass.EnemyAir;
				}
				else if (Utils.Equals(value, "enemy_ground"))
				{
					targetClass |= TargetClass.EnemyGround;
				}
				else if (Utils.Equals(value, "all_enemy"))
				{
					targetClass |= TargetClass.AllEnemy;
				}
				else if (Utils.Equals(value, "boss"))
				{
					targetClass |= TargetClass.Boss;
				}
				else if (Utils.Equals(value, "man"))
				{
					targetClass |= TargetClass.Man;
				}
				else if (Utils.Equals(value, "shot_enemy"))
				{
					targetClass |= TargetClass.ShotEnemy;
				}
				else if (Utils.Equals(value, "missile_enemy"))
				{
					targetClass |= TargetClass.MissileEnemy;
				}
				else if (Utils.Equals(value, "shot_hero"))
				{
					targetClass |= TargetClass.ShotHero;
				}
				else if (Utils.Equals(value, "missile_hero"))
				{
					targetClass |= TargetClass.MissileHero;
				}
				else if (Utils.Equals(value, "building"))
				{
					targetClass |= TargetClass.Building;
				}
				else if (Utils.Equals(value, "bonus"))
				{
					targetClass |= TargetClass.Bonus;
				}
				else if (Utils.Equals(value, "secret"))
				{
					targetClass |= TargetClass.Secret;
				}
				else if (Utils.Equals(value, "other"))
				{
					targetClass |= TargetClass.Other;
				}
				else if (Utils.Equals(value, "ground"))
				{
					targetClass |= TargetClass.Ground;
				}
				else if (Utils.Equals(value, "hero"))
				{
					targetClass |= TargetClass.Hero;
				}
				else if (Utils.Equals(value, "drone"))
				{
					targetClass |= TargetClass.Drone;
				}
			}
		}
		return targetClass;
	}

	public static TargetClass GetTargetClassByPhysClass(PhysClass physClass)
	{
		if ((physClass & PhysClass.EnemyAir) != 0)
		{
			return TargetClass.EnemyAir;
		}
		if ((physClass & PhysClass.EnemyGround) != 0)
		{
			return TargetClass.EnemyGround;
		}
		if ((physClass & PhysClass.ShotEnemy) != 0)
		{
			return TargetClass.ShotEnemy;
		}
		if ((physClass & PhysClass.MissileEnemy) != 0)
		{
			return TargetClass.MissileEnemy;
		}
		if ((physClass & PhysClass.ShotHero) != 0 || (physClass & PhysClass.BombHero) != 0)
		{
			return TargetClass.ShotHero;
		}
		if ((physClass & PhysClass.Man) != 0)
		{
			return TargetClass.Man;
		}
		if ((physClass & PhysClass.Hero) != 0)
		{
			return TargetClass.Hero;
		}
		if ((physClass & PhysClass.Bonus) != 0)
		{
			return TargetClass.Bonus;
		}
		if ((physClass & PhysClass.Decoration) != 0)
		{
			return TargetClass.Other;
		}
		if ((physClass & PhysClass.Building) != 0 || (physClass & PhysClass.Platform) != 0)
		{
			return TargetClass.Building;
		}
		if ((physClass & PhysClass.Ground) != 0)
		{
			return TargetClass.Ground;
		}
		if ((physClass & PhysClass.Undefined) != 0)
		{
			return TargetClass.Undefined;
		}
		return TargetClass.None;
	}

	public static PhysClass GetPhysClassByTargetClass(TargetClass targetClass)
	{
		PhysClass physClass = PhysClass.None;
		if ((targetClass & TargetClass.EnemyAir) != 0)
		{
			physClass |= PhysClass.EnemyAir;
		}
		if ((targetClass & TargetClass.EnemyGround) != 0)
		{
			physClass |= PhysClass.EnemyGround;
		}
		if ((targetClass & TargetClass.ShotEnemy) != 0 || (targetClass & TargetClass.MissileEnemy) != 0)
		{
			physClass |= PhysClass.ShotEnemy;
		}
		if ((targetClass & TargetClass.ShotHero) != 0 || (targetClass & TargetClass.MissileHero) != 0)
		{
			physClass |= PhysClass.ShotHero;
		}
		if ((targetClass & TargetClass.Man) != 0)
		{
			physClass |= PhysClass.Man;
		}
		if ((targetClass & TargetClass.Building) != 0)
		{
			physClass |= PhysClass.Building;
		}
		if ((targetClass & TargetClass.Boss) != 0)
		{
			physClass |= PhysClass.EnemyGround;
		}
		if ((targetClass & TargetClass.Ground) != 0)
		{
			physClass |= PhysClass.Ground;
		}
		if ((targetClass & TargetClass.Other) != 0)
		{
			physClass |= PhysClass.Decoration;
		}
		if ((targetClass & TargetClass.Hero) != 0)
		{
			physClass |= PhysClass.Hero;
		}
		if ((targetClass & TargetClass.Drone) != 0)
		{
			physClass |= PhysClass.Drone;
		}
		return physClass;
	}

	public static bool IsEnemyTargetClass(TargetClass targetClass)
	{
		if ((targetClass & TargetClass.EnemyAir) == 0 && (targetClass & TargetClass.EnemyGround) == 0)
		{
			return (targetClass & TargetClass.Boss) != 0;
		}
		return true;
	}

	public static string GetPathFromValue(string value, string directory)
	{
		if (value.StartsWith(".\\") || value.StartsWith("./"))
		{
			try
			{
				return Path.Combine(directory, value.Substring(2));
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Wrong filename format: {0}, {1}", value, ex);
				return value;
			}
		}
		return value;
	}

	public static string GetAppDataDirectory()
	{
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "jng2");
		try
		{
			Directory.CreateDirectory(text);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to create save game directory. Ex: {0}", ex);
		}
		return text;
	}

	public static string GetGameIniPath()
	{
		return Path.Combine(GetAppDataDirectory(), "game.ini");
	}

	public static string GetEditorIniPath()
	{
		return Path.Combine(GetAppDataDirectory(), "editor.ini");
	}

	public static string GetSaveDirectory()
	{
		string text = Path.Combine(GetAppDataDirectory(), "save");
		try
		{
			Directory.CreateDirectory(text);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to create save game directory. Ex: {0}", ex);
		}
		return text;
	}

	public static string GetSaveGamePath(string fileName)
	{
		return Path.Combine(GetSaveDirectory(), fileName);
	}

	public static string GetSaveGamePath(int number)
	{
		return GetSaveGamePath(GetSaveGameFileName(number));
	}

	public static string GetSaveGameFileName(int number)
	{
		return $"save_{number}.jsg";
	}

	public static List<SaveGameInfo> GetSaveGameInfoList()
	{
		List<SaveGameInfo> list = new List<SaveGameInfo>();
		if (Options.UseSteam && !GameCore.SteamManager.IsInitialized)
		{
			GameCore.LogErr("Unable to get save game info. Steam is not initialized.");
		}
		for (int i = 1; i <= 5; i++)
		{
			SaveGameInfo saveGameInfo;
			if ((saveGameInfo = SaveGame.LoadSaveGameInfo(i)) == null)
			{
				saveGameInfo = new SaveGameInfo();
			}
			saveGameInfo.Number = i;
			list.Add(saveGameInfo);
		}
		return list;
	}
}
