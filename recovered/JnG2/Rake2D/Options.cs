using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.Sound;

namespace Rake2D;

public static class Options
{
	public const float FONT_SCALE_BIG = 1.4f;

	public const float FONT_SCALE_SMALL = 1.1f;

	public static float MusicVolume;

	public static float SoundVolume;

	public static int SoundPanningLevel = 2;

	public static int LevelOfDetails = 3;

	public static int QuakeMode = 2;

	public static int ScreenResolutionWidth;

	public static int ScreenResolutionHeight;

	public static int MaxFPS = 200;

	public static bool UseDistortionShader;

	public static bool FullScreen = true;

	public static bool VSync;

	public static bool HwFullScreen;

	public static bool WarningTexts = true;

	public static GamePadDeadZone GamePadDeadZoneMode;

	public static GameInput.PlayerControllerEnum[] PlayerController = new GameInput.PlayerControllerEnum[2];

	public static int[] GamePadNumber = new int[2] { -1, -1 };

	public static int MouseSensitivity = 50;

	public static int StartCameraZoom;

	public static int StartInEditor;

	public static bool MasterOfTheUniverse;

	public static int DebugInfoMode;

	public static int SoundDebugInfoMode;

	public static bool ShowLightmaps;

	public static bool DebugPath;

	public static bool DebugNoParticles;

	public static bool DebugPause;

	public static int DebugQuickInfoMode;

	public static string StartLevelName;

	public static Vector2 StartLevelPosition;

	public static int SaveGameNumber = 0;

	public static bool EncryptFiles = true;

	public static bool SkipExpiredObjects = true;

	public static Color BgrColor = new Color(32, 32, 32);

	public static bool IsLightsOn = true;

	public static bool IsShadowsOn = true;

	public static bool ShowDebugPhysics = false;

	public static bool UseSteam = true;

	public static bool UseDefaultFactors;

	public static bool ShowHeroColisoid;

	public static float FontScale = 1.4f;

	public static HUDModeEnum HUDMode = HUDModeEnum.Both;

	public static int ScissorsMode = 1;

	public static string SShotDir;

	public static int SShotNumber;

	public static float GamePadRepeatDelay = 0.4f;

	public static float HitEffectFactor = 1f;

	public static bool IsNewVersionInstalled;

	public static bool ScreenShotModeEnabled;

	public static bool WasWelcomeScreenReaded;

	public static bool IsMiniHUDVisible
	{
		get
		{
			if (HUDMode != HUDModeEnum.MiniOnly)
			{
				return HUDMode == HUDModeEnum.Both;
			}
			return true;
		}
	}

	public static bool IsBigHUDVisible
	{
		get
		{
			if (HUDMode != HUDModeEnum.BigOnly)
			{
				return HUDMode == HUDModeEnum.Both;
			}
			return true;
		}
	}

	public static void SetDefaultsAudioVideo()
	{
		MusicVolume = SoundUtils.GetExponentialVolume(80f);
		SoundVolume = SoundUtils.GetExponentialVolume(50f);
		SoundPanningLevel = 2;
	}

	public static void SetDefaultsGame()
	{
		WarningTexts = true;
		HUDMode = HUDModeEnum.Both;
		ShowHeroColisoid = false;
		LevelOfDetails = 3;
		QuakeMode = 2;
		MaxFPS = 200;
		VSync = false;
	}

	public static void SetDefaults()
	{
		SetDefaultsAudioVideo();
		SetDefaultsGame();
		ScreenResolutionWidth = -1;
		ScreenResolutionHeight = -1;
		SkipExpiredObjects = true;
		UseDistortionShader = true;
		FullScreen = true;
		HwFullScreen = false;
		StartCameraZoom = 100;
		FontScale = 1.1f;
		GamePadDeadZoneMode = GamePadDeadZone.IndependentAxes;
		GameInput.SetDefaultKeyCodes();
		GameInput.SetDefaultGamePadButtonCodes();
		GameInput.SetDefaultMouseButtonCodes();
		PlayerController[0] = GameInput.PlayerControllerEnum.Gamepad;
		PlayerController[1] = GameInput.PlayerControllerEnum.Gamepad;
	}

	public static void SetDefaultControllers()
	{
		GamePadNumber[0] = GameInput.GetFirstConnectedGamePad();
		GamePadNumber[1] = GameInput.GetFirstConnectedGamePad(new int[1] { GamePadNumber[0] });
		if (GamePadNumber[0] >= 0)
		{
			PlayerController[0] = GameInput.PlayerControllerEnum.Gamepad;
		}
		else
		{
			PlayerController[0] = GameInput.PlayerControllerEnum.Keyboard;
		}
		if (GamePadNumber.Length > 1 && GamePadNumber[1] >= 0)
		{
			PlayerController[1] = GameInput.PlayerControllerEnum.Gamepad;
		}
		else
		{
			PlayerController[1] = GameInput.PlayerControllerEnum.Keyboard;
		}
	}

	public static void Load(string fileName)
	{
		IniFile iniFile = new IniFile();
		if (!iniFile.OpenFromFile(fileName))
		{
			GameCore.Log("Using default option values.");
			return;
		}
		while (iniFile.NextLine())
		{
			if (iniFile.KeyEquals("full_screen"))
			{
				FullScreen = iniFile.GetBoolValue(defaultValue: true);
			}
			else if (iniFile.KeyEquals("hw_full_screen"))
			{
				HwFullScreen = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("vsync"))
			{
				VSync = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("warning_texts"))
			{
				WarningTexts = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("hit_effect"))
			{
				HitEffectFactor = (float)iniFile.GetIntValue() / 100f;
			}
			else if (iniFile.KeyEquals("max_fps"))
			{
				MaxFPS = iniFile.GetIntValue();
			}
			else if (iniFile.KeyEquals("lod"))
			{
				LevelOfDetails = iniFile.GetIntValue();
			}
			else if (iniFile.KeyEquals("quake"))
			{
				QuakeMode = iniFile.GetIntValue();
			}
			else if (iniFile.KeyEquals("hud_mode"))
			{
				HUDMode = (HUDModeEnum)Utils.Clamp(iniFile.GetIntValue(), 0, 3);
			}
			else if (iniFile.KeyEquals("hero_colisoid"))
			{
				ShowHeroColisoid = iniFile.GetBoolValue();
			}
			else if (iniFile.KeyEquals("font_scale"))
			{
				FontScale = iniFile.GetFloatValue(1.4f);
			}
			else
			{
				if (iniFile.KeyEquals("controller") && iniFile.TryGetIntArray(2, out var resultArray))
				{
					for (int i = 0; i < resultArray.Length; i++)
					{
						PlayerController[i] = (GameInput.PlayerControllerEnum)resultArray[i];
					}
					continue;
				}
				if (iniFile.KeyEquals("gamepad") && iniFile.TryGetIntArray(2, out resultArray))
				{
					for (int j = 0; j < resultArray.Length; j++)
					{
						GamePadNumber[j] = resultArray[j];
					}
					continue;
				}
				if (iniFile.KeyEquals("mouse_sensitivity"))
				{
					MouseSensitivity = iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("gp_repeat_delay"))
				{
					GamePadRepeatDelay = (float)iniFile.GetIntValue() / 1000f;
				}
				else if (iniFile.KeyEquals("sound_volume"))
				{
					SoundVolume = SoundUtils.GetExponentialVolume(Utils.Clamp(iniFile.GetFloatValue(100f), 0f, 100f));
				}
				else if (iniFile.KeyEquals("music_volume"))
				{
					MusicVolume = SoundUtils.GetExponentialVolume(Utils.Clamp(iniFile.GetFloatValue(100f), 0f, 100f));
				}
				else if (iniFile.KeyEquals("sound_panning"))
				{
					SoundPanningLevel = Utils.Clamp(iniFile.GetIntValue(), 0, 2);
				}
				else if (iniFile.KeyEquals("version"))
				{
					if (!Utils.Equals(iniFile.Value, GameCore.APP_VERSION))
					{
						WasWelcomeScreenReaded = false;
						IsNewVersionInstalled = true;
					}
					else
					{
						WasWelcomeScreenReaded = true;
						IsNewVersionInstalled = false;
					}
				}
				else if (iniFile.KeyEquals("screen_width"))
				{
					ScreenResolutionWidth = iniFile.GetIntValue(-1);
				}
				else if (iniFile.KeyEquals("screen_height"))
				{
					ScreenResolutionHeight = iniFile.GetIntValue(-1);
				}
				else if (iniFile.KeyEquals("Start_camera_zoom"))
				{
					StartCameraZoom = iniFile.GetIntValue(100);
				}
				else if (iniFile.KeyEquals("start_in_editor"))
				{
					StartInEditor = iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("steam"))
				{
					UseSteam = iniFile.GetBoolValue();
				}
				else if (iniFile.KeyEquals("start_level"))
				{
					StartLevelName = iniFile.Value;
				}
				else if (iniFile.KeyEquals("start_pos"))
				{
					StartLevelPosition = iniFile.GetVector2();
				}
				else if (iniFile.KeyEquals("save_number"))
				{
					SaveGameNumber = iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("sshot_dir"))
				{
					SShotDir = iniFile.Value;
				}
				else if (iniFile.KeyEquals("sshot_number"))
				{
					SShotNumber = iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("encrypt_files"))
				{
					EncryptFiles = iniFile.GetBoolValue();
				}
				else if (iniFile.KeyEquals("debug_info"))
				{
					DebugInfoMode = iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("console"))
				{
					GameCore.Console.AddBatchCommand(iniFile.Value);
				}
			}
			GameInput.ParseIniFileLine(iniFile);
		}
		iniFile.Close();
		GameCore.Log("Options loaded from '{0}'.", fileName);
	}

	public static void ToggleHUDMode()
	{
		switch (HUDMode)
		{
		case HUDModeEnum.None:
			HUDMode = HUDModeEnum.Both;
			break;
		case HUDModeEnum.Both:
			HUDMode = HUDModeEnum.BigOnly;
			break;
		case HUDModeEnum.BigOnly:
			HUDMode = HUDModeEnum.MiniOnly;
			break;
		case HUDModeEnum.MiniOnly:
			HUDMode = HUDModeEnum.Both;
			break;
		}
	}

	public static void Save(string fileName)
	{
		using StreamWriter streamWriter = new StreamWriter(fileName);
		if (streamWriter == null)
		{
			GameCore.LogErr($"Unable save options to file '{fileName}'");
			return;
		}
		streamWriter.WriteLine($"version = {GameCore.APP_VERSION}");
		streamWriter.WriteLine();
		streamWriter.WriteLine("//--- Graphics ---");
		streamWriter.WriteLine($"screen_width = {ScreenResolutionWidth}");
		streamWriter.WriteLine($"screen_height = {ScreenResolutionHeight}");
		streamWriter.WriteLine($"full_screen = {(FullScreen ? 1 : 0)}");
		streamWriter.WriteLine($"hw_full_screen = {(HwFullScreen ? 1 : 0)}");
		streamWriter.WriteLine($"max_fps = {MaxFPS}");
		streamWriter.WriteLine($"lod = {LevelOfDetails}");
		streamWriter.WriteLine($"quake = {QuakeMode}");
		streamWriter.WriteLine($"vsync = {(VSync ? 1 : 0)}");
		streamWriter.WriteLine($"warning_texts = {(WarningTexts ? 1 : 0)}");
		streamWriter.WriteLine($"hit_effect = {(int)(HitEffectFactor * 100f)}");
		streamWriter.WriteLine();
		streamWriter.WriteLine("//--- Sound ---");
		streamWriter.WriteLine($"sound_volume = {SoundUtils.GetNormalVolume(SoundVolume)}");
		streamWriter.WriteLine($"music_volume = {SoundUtils.GetNormalVolume(MusicVolume)}");
		streamWriter.WriteLine($"sound_panning = {SoundPanningLevel}");
		streamWriter.WriteLine();
		GameInput.SaveToFile(streamWriter);
		streamWriter.WriteLine();
		streamWriter.WriteLine("//--- Controls ---");
		streamWriter.Write("controller = ");
		for (int i = 0; i < 2; i++)
		{
			if (i > 0)
			{
				streamWriter.Write(",");
			}
			streamWriter.Write($"{(int)PlayerController[i]}");
		}
		streamWriter.WriteLine();
		streamWriter.Write("gamepad = ");
		for (int j = 0; j < 2; j++)
		{
			if (j > 0)
			{
				streamWriter.Write(",");
			}
			streamWriter.Write($"{GamePadNumber[j]}");
		}
		streamWriter.WriteLine();
		streamWriter.WriteLine($"mouse_sensitivity = {MouseSensitivity}");
		streamWriter.WriteLine($"gp_repeat_delay = {(int)(GamePadRepeatDelay * 1000f)}");
		streamWriter.WriteLine();
		streamWriter.WriteLine("//--- Misc ---");
		streamWriter.WriteLine($"hud_mode = {(int)HUDMode}");
		streamWriter.WriteLine($"font_scale = {Utils.FormatFloat2(FontScale)}");
		streamWriter.WriteLine($"hero_colisoid = {(ShowHeroColisoid ? 1 : 0)}");
		if (SaveGameNumber >= 0)
		{
			streamWriter.WriteLine($"save_number = {SaveGameNumber}");
		}
		streamWriter.WriteLine($"sshot_dir = {SShotDir}");
		streamWriter.WriteLine($"sshot_number = {SShotNumber}");
		streamWriter.Close();
	}
}
