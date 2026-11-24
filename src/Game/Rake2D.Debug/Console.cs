using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;
using Rake2D.Menu;

namespace Rake2D.Debug;

public class Console
{
	public int MaxLineCount = 100;

	public string CommandText;

	private GameCore _gameCore;

	private Rectangle _rect;

	private TriangleBatch _triangleBatch;

	private List<ConsoleMessage> _messageList;

	private float _positionY;

	private BitmapFont _font;

	private ConsoleStateEnum _state;

	private float _cursorAlpha;

	private int _cursorPos;

	private float _keyRepeatTimer;

	private Keys _lastPressedKey;

	private List<string> _oldCommandList;

	private List<string> _batchCommandList;

	private int _oldCommandIndex;

	private SpriteBatch _spriteBatch;

	private bool _isActive;

	public bool IsActive
	{
		get
		{
			if (_isActive)
			{
				return _state != ConsoleStateEnum.Hidden;
			}
			return false;
		}
		set
		{
			_isActive = value;
		}
	}

	public Console(GameCore gameCore)
	{
		_gameCore = gameCore;
		_messageList = new List<ConsoleMessage>();
		_oldCommandList = new List<string>();
		_batchCommandList = new List<string>();
		CommandText = "";
		IsActive = true;
	}

	public void Init(BitmapFont font)
	{
		_font = font;
		_triangleBatch = new TriangleBatch(GameCore.GraphicsDeviceStatic);
		_rect = new Rectangle(0, (int)_positionY, 500, GameCore.GraphicsDeviceStatic.PresentationParameters.BackBufferHeight / 2);
		_positionY = -_rect.Height;
		_spriteBatch = new SpriteBatch(_gameCore.GraphicsDevice);
	}

	public void Toggle()
	{
		if (_state == ConsoleStateEnum.Hidden || _state == ConsoleStateEnum.Showing)
		{
			_state = ConsoleStateEnum.Showing;
			GameCore.Editor.SetFocusComponent(null);
		}
		else
		{
			_state = ConsoleStateEnum.Hiding;
		}
	}

	public void ClearAll()
	{
		_messageList.Clear();
	}

	public void UpdateComandLine(float elapsedTime)
	{
		if (!IsActive || _state != ConsoleStateEnum.Visible)
		{
			return;
		}
		Keys keys = GameInput.SystemGetPressedKey();
		if (_lastPressedKey != keys && keys != Keys.None)
		{
			_keyRepeatTimer = 0.5f;
			_lastPressedKey = keys;
		}
		if (GameInput.SystemKeyDown(_lastPressedKey))
		{
			_keyRepeatTimer -= elapsedTime;
			if (_keyRepeatTimer <= 0f)
			{
				GameInput.ClearLastKeyboadState();
				_keyRepeatTimer = 0.075f;
			}
		}
		else
		{
			_keyRepeatTimer = 0.5f;
		}
		if (!GameInput.ControlDown && GameInput.TryConvertKeyboardInput(out var keyChar) && CommandText.Length < 64)
		{
			if (_cursorPos < CommandText.Length)
			{
				CommandText = CommandText.Insert(_cursorPos, keyChar.ToString());
			}
			else
			{
				CommandText += keyChar;
			}
			_cursorPos++;
		}
		if (GameInput.SystemKeyPressed(Keys.Escape))
		{
			if (_cursorPos > 0)
			{
				CommandText = "";
				_cursorPos = 0;
			}
			else
			{
				Toggle();
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Back))
		{
			if (_cursorPos > 0)
			{
				CommandText = CommandText.Remove(_cursorPos - 1, 1);
				_cursorPos--;
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Delete))
		{
			if (_cursorPos < CommandText.Length)
			{
				CommandText = CommandText.Remove(_cursorPos, 1);
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Left))
		{
			if (GameInput.ControlDown)
			{
				bool flag = false;
				while (_cursorPos > 0)
				{
					if (!char.IsWhiteSpace(CommandText[_cursorPos - 1]))
					{
						flag = true;
						_cursorPos--;
						continue;
					}
					if (flag)
					{
						break;
					}
					_cursorPos--;
				}
			}
			else if (_cursorPos > 0)
			{
				_cursorPos--;
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Right))
		{
			if (GameInput.ControlDown)
			{
				bool flag2 = false;
				while (_cursorPos < CommandText.Length - 1)
				{
					if (char.IsWhiteSpace(CommandText[_cursorPos]))
					{
						flag2 = true;
						_cursorPos++;
						continue;
					}
					if (flag2)
					{
						break;
					}
					_cursorPos++;
				}
			}
			else if (_cursorPos < CommandText.Length)
			{
				_cursorPos++;
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Home))
		{
			_cursorPos = 0;
		}
		else if (GameInput.SystemKeyPressed(Keys.End))
		{
			_cursorPos = CommandText.Length;
		}
		else if (GameInput.SystemKeyPressed(Keys.Up))
		{
			if (_oldCommandIndex < _oldCommandList.Count - 1)
			{
				_oldCommandIndex++;
				CommandText = _oldCommandList[_oldCommandIndex];
				_cursorPos = CommandText.Length;
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Down))
		{
			if (_oldCommandIndex > 0)
			{
				_oldCommandIndex--;
				CommandText = _oldCommandList[_oldCommandIndex];
				_cursorPos = CommandText.Length;
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.Enter))
		{
			ParseCommand();
		}
		_cursorAlpha += elapsedTime * 8f;
	}

	public void ClearBatch()
	{
		_batchCommandList.Clear();
	}

	public void AddBatchCommand(string cmd)
	{
		_batchCommandList.Add(cmd);
	}

	public void DoBatch()
	{
		if (_batchCommandList.Count > 0)
		{
			AddMessage($"~ Batch of {_batchCommandList.Count} commands:", ConsoleMessageType.System);
			_batchCommandList.ForEach((string str) =>
			{
				CommandText = str;
				ParseCommand();
			});
		}
	}

	private string GetSingleCommand(List<string> commandList)
	{
		string text = "";
		for (int i = 1; i < commandList.Count; i++)
		{
			text += commandList[i];
		}
		return text;
	}

	private bool IsMasterCommand(string command, string key)
	{
		if (Options.MasterOfTheUniverse)
		{
			return Utils.Equals(command, key);
		}
		return false;
	}

	private void ParseCommand()
	{
		string[] array = CommandText.Split(' ');
		List<string> list = new List<string>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text = array2[i].Trim().ToLower();
			if (text.Length > 0 && !text.Equals("="))
			{
				list.Add(text);
			}
		}
		if (list.Count == 0)
		{
			CommandText = "";
			_cursorPos = 0;
		}
		if (list.Count > 0)
		{
			string text2 = list[0];
			AddMessage($"~ {CommandText}", ConsoleMessageType.System);
			if (_oldCommandList.Count == 0 || !_oldCommandList[0].Equals(CommandText))
			{
				_oldCommandList.Insert(0, CommandText);
				while (_oldCommandList.Count > 10)
				{
					_oldCommandList.RemoveAt(_oldCommandList.Count - 1);
				}
			}
			_oldCommandIndex = -1;
			int result;
			if (Utils.Equals(text2, "cls"))
			{
				ClearAll();
			}
			else if (Utils.Equals(text2, "vsync"))
			{
				if (list.Count > 1)
				{
					_gameCore.SetVSync(Utils.ParseBool(list[1]));
				}
				AddMessage($"- VSync = {Options.VSync}", ConsoleMessageType.Command);
			}
			else if (Utils.Equals(text2, "text_warnings"))
			{
				if (list.Count > 1)
				{
					Options.WarningTexts = Utils.ParseBool(list[1]);
				}
				AddMessage($"- Gore = {Options.WarningTexts}", ConsoleMessageType.Command);
			}
			else if (Utils.Equals(text2, "hud"))
			{
				if (list.Count > 1 && int.TryParse(list[1], out result))
				{
					if (result >= 0 && result <= 3)
					{
						Options.HUDMode = (HUDModeEnum)result;
					}
					else
					{
						Options.HUDMode = HUDModeEnum.Both;
					}
				}
				AddMessage($"- HUD mode = {Options.HUDMode}", ConsoleMessageType.Command);
			}
			else if (Utils.Equals(text2, "max_fps"))
			{
				if (list.Count > 1 && int.TryParse(list[1], out Options.MaxFPS))
				{
					_gameCore.SetMaxFPS();
				}
				AddMessage($"- max_fps = {Options.MaxFPS}", ConsoleMessageType.Command);
			}
			else if (Utils.Equals(text2, "font_scale"))
			{
				if (list.Count > 1 && int.TryParse(list[1], out result))
				{
					Options.FontScale = (float)result / 100f;
				}
				AddMessage($"- font_scale = {(int)(Options.FontScale * 100f)}", ConsoleMessageType.Command);
			}
			else if (Utils.Equals(text2, "hit_effect"))
			{
				if (list.Count > 1 && int.TryParse(list[1], out result))
				{
					Options.HitEffectFactor = (float)Utils.Clamp(result, 0, 100) / 100f;
				}
				AddMessage($"- hit_effect = {(int)(Options.HitEffectFactor * 100f)} %", ConsoleMessageType.Command);
			}
			else if (Utils.Equals(text2, "debug_info") || IsMasterCommand(text2, "di"))
			{
				if (list.Count > 1)
				{
					int.TryParse(list[1], out Options.DebugInfoMode);
				}
				AddMessage($"- debug info = {Options.DebugInfoMode}", ConsoleMessageType.Command);
			}
			else if (Utils.Equals(text2, "sound_debug_info") || IsMasterCommand(text2, "sdi"))
			{
				if (list.Count > 1)
				{
					int.TryParse(list[1], out Options.SoundDebugInfoMode);
				}
				AddMessage($"- sound debug info = {Options.SoundDebugInfoMode}", ConsoleMessageType.Command);
			}
			else if (Utils.Equals(text2, "steam"))
			{
				GameCore.SteamManager.Dump();
			}
			else if (IsMasterCommand(text2, "steam_ach") || IsMasterCommand(text2, "sach"))
			{
				if (list.Count > 1)
				{
					string text3 = list[1];
					if (Utils.Equals(text3, "all"))
					{
						int result2 = 1;
						if (list.Count < 3 || !int.TryParse(list[2], out result2) || result2 != 0)
						{
							GameJnG.UnlockAllAchievements();
						}
						else
						{
							GameJnG.LockAllAchievements();
						}
					}
					else
					{
						GameAchievementEnum achievementIdByString = GameJnG.GetAchievementIdByString(text3);
						if (achievementIdByString != GameAchievementEnum.UNKNOWN)
						{
							int result3 = 1;
							if (list.Count < 3 || !int.TryParse(list[2], out result3) || result3 != 0)
							{
								GameJnG.UnlockAchievement(achievementIdByString);
								AddMessage($"- Achievement '{achievementIdByString}' unlocked", ConsoleMessageType.Command);
							}
							else
							{
								GameJnG.LockAchievement(achievementIdByString);
								AddMessage($"- Achievement '{achievementIdByString}' locked", ConsoleMessageType.Command);
							}
						}
						else
						{
							AddMessage($"- Wrong achievement name '{text3}'", ConsoleMessageType.Error);
						}
					}
				}
				else
				{
					AddMessage("- Undefined achievement name (see 'steam' command)", ConsoleMessageType.Error);
				}
			}
			else if (IsMasterCommand(text2, "steam_stat") || IsMasterCommand(text2, "sstat"))
			{
				if (list.Count > 2)
				{
					string text4 = list[1];
					if (!int.TryParse(list[2], out var result4))
					{
						AddMessage($"- Wrong value: '{result4}'", ConsoleMessageType.Command);
					}
					else
					{
						GameCore.SteamManager.SetStat(text4, result4);
						AddMessage($"- Steam stat '{text4}' set to {result4}", ConsoleMessageType.Command);
					}
				}
				else
				{
					AddMessage("- Undefined stat name (see 'steam' command)", ConsoleMessageType.Error);
				}
			}
			else if (Utils.Equals(text2, "steam_reset_stats") || IsMasterCommand(text2, "srs"))
			{
				if (list.Count > 1)
				{
					GameCore.SteamManager.ResetAllStats(Utils.ParseBool(list[1]));
				}
				else
				{
					GameCore.SteamManager.ResetAllStats(bAchievementsToo: false);
				}
			}
			else if (Utils.Equals(text2, "i_have_the_power"))
			{
				if (list.Count > 1)
				{
					Options.MasterOfTheUniverse = Utils.ParseBool(list[1]);
				}
				if (Options.MasterOfTheUniverse)
				{
					AddMessage("- I have the POWER!", ConsoleMessageType.Cheat);
				}
				else
				{
					AddMessage("- I am not He-man.", ConsoleMessageType.Cheat);
				}
			}
			else if (Utils.Equals(text2, "cheat_no_heat") || IsMasterCommand(text2, "ch"))
			{
				if (list.Count > 1)
				{
					Cheat.NoHeat = Utils.ParseBool(list[1]);
				}
				else
				{
					Cheat.NoHeat = true;
				}
				if (Cheat.NoHeat)
				{
					GameJnG.HeroList.ForEach((Hero hero) =>
					{
						hero.Heat = 0f;
						hero.StopOverheating();
					});
					GameJnG.SetCheater();
				}
				AddMessage($"- Cheat no heat = {Cheat.NoHeat}", ConsoleMessageType.Cheat);
			}
			else if (Utils.Equals(text2, "cheat_no_damage") || IsMasterCommand(text2, "cd"))
			{
				if (list.Count > 1)
				{
					Cheat.NoDamage = Utils.ParseBool(list[1]);
				}
				else
				{
					Cheat.NoDamage = true;
				}
				if (Cheat.NoDamage)
				{
					GameJnG.HeroList.ForEach((Hero hero) =>
					{
						hero.HPPercent = 1f;
					});
					GameJnG.SetCheater();
				}
				AddMessage($"- Cheat no damage = {Cheat.NoDamage}", ConsoleMessageType.Cheat);
			}
			else if (text2.Equals("cheat_full_mana") || IsMasterCommand(text2, "cm"))
			{
				if (list.Count > 1)
				{
					Cheat.FullMana = Utils.ParseBool(list[1]);
				}
				else
				{
					Cheat.FullMana = true;
				}
				if (Cheat.FullMana)
				{
					GameJnG.HeroList.ForEach((Hero hero) =>
					{
						hero.ManaPercent = 1f;
					});
					GameJnG.SetCheater();
				}
				AddMessage("- Cheat full mana done");
			}
			else if (text2.Equals("cheat_full_shop") || IsMasterCommand(text2, "cfs"))
			{
				GameJnG.ShopOffer.Build(999999, 999999);
				GameJnG.SetCheater();
				AddMessage("- Cheat full shop activated");
			}
			else if (text2.Equals("came_saw_won") || IsMasterCommand(text2, "vvv"))
			{
				_gameCore.Pause(paused: false);
				GameJnG.SetFinishLevel(GameCore.GameLevel, FinishLevelModeEnum.WinAndFlyOut, 0f);
				GameJnG.SetCheater();
			}
			else if (text2.Equals("came_saw_failed") || IsMasterCommand(text2, "vvf"))
			{
				_gameCore.Pause(paused: false);
				GameJnG.SetFinishLevel(GameCore.GameLevel, FinishLevelModeEnum.Failed, 0f);
			}
			else if (text2.Equals("i_am_discoverer") || IsMasterCommand(text2, "iad"))
			{
				result = 1;
				if (list.Count > 1)
				{
					int.TryParse(list[1], out result);
				}
				if (result != 0)
				{
					GameJnG.SetCheater();
					GameJnG.Campaign.ShowAllSecretLevels();
					AddMessage("- All secret levels are discovered!");
				}
				else
				{
					Campaign.HideAllSecretLevels();
					AddMessage("- All secret levels are hidden.");
				}
			}
			else if (text2.Equals("be_rich_is_funny") || IsMasterCommand(text2, "cash"))
			{
				if (list.Count > 1 && int.TryParse(list[1], out result))
				{
					GameJnG.Cash = result;
					GameJnG.SetCheater();
				}
				AddMessage($"- {text2} = {GameJnG.Cash}", ConsoleMessageType.Command);
			}
			else if (text2.Equals("clear_profiles") || IsMasterCommand(text2, "cprof"))
			{
				for (int num = 1; num <= 5; num++)
				{
					SaveGame.Delete(num);
				}
				Options.SaveGameNumber = (GameJnG.SaveGameNumber = 0);
				AddMessage("- Save game profiles 1 - 5 deleted", ConsoleMessageType.Command);
			}
			else if (Options.MasterOfTheUniverse)
			{
				int[] resultArray;
				if (text2.Equals("reload") && list.Count <= 1)
				{
					AddMessage($"- Reload current level: {GameCore.GameLevel.Id}", ConsoleMessageType.Command);
					GameCore.GameLevel.Reload(null);
				}
				else if (text2.Equals("reload_items") || IsMasterCommand(text2, "ri"))
				{
					GameCore.GlobalContent.ReloadDataFiles(DataType.Item);
					foreach (Hero hero in GameJnG.HeroList)
					{
						hero.RebuildShipConfiguration();
					}
				}
				else if (text2.Equals("reload_texts") || Utils.Equals(text2, "rt"))
				{
					TextBundle.Load(TextBundle.Language);
					GameJnG.SetTexts();
					AddMessage("Text bundle reloaded", ConsoleMessageType.Command);
				}
				else if (text2.Equals("tetrix_level") || Utils.Equals(text2, "txl"))
				{
					GameJnG.ShopMenu.Atarix.ContentTetrix.LoadLevels();
					if (list.Count < 2 || !int.TryParse(list[1], out result))
					{
						result = GameJnG.ShopMenu.Atarix.ContentTetrix.CurrentLevelNumber;
					}
					GameJnG.ShopMenu.Atarix.ContentTetrix.StartLevel(result);
					GameJnG.SetCheater();
					AddMessage($"Tetrix level {result} reloaded", ConsoleMessageType.Command);
				}
				else if (text2.Equals("reload_hero") || Utils.Equals(text2, "rh"))
				{
					List<KeyValuePair<string, PartData>> list2 = GameCore.GlobalContent.PartDatas.Where((KeyValuePair<string, PartData> p) => p.Value.FileName.Contains("hero/hero.txt") || p.Value.FileName.Contains("hero/ship/")).ToList();
					HashSet<string> hashSet = new HashSet<string>();
					foreach (KeyValuePair<string, PartData> item in list2)
					{
						GameCore.GlobalContent.PartDatas.Remove(item.Key);
						hashSet.Add(item.Value.FileName);
					}
					AddMessage($"Founded {list2.Count} hero parts in {hashSet.Count} files", ConsoleMessageType.Command);
					foreach (string item2 in hashSet.OrderBy((string s) => s.ToLower()))
					{
						DataLoader.LoadDataFile(item2, GameCore.GlobalContent, DataType.Part);
						AddMessage($"File {item2} reloaded", ConsoleMessageType.Command);
					}
					foreach (PartData value in GameCore.GlobalContent.PartDatas.Values)
					{
						if (!value.IsInitialised)
						{
							value.Init();
						}
					}
					if (!GameCore.IsEditorEnabled)
					{
						foreach (Hero hero2 in GameJnG.HeroList)
						{
							hero2.CreateSmallShip(null, hero2.MyObject.WorldPosition);
						}
						if (GameCore.IsShopMenuEnabled)
						{
							GameJnG.ShopMenu.BigShip.Init(GameJnG.ShopMenu.MyHero);
						}
					}
				}
				else if (text2.Equals("screen_shot_mode") || Utils.Equals(text2, "ssm"))
				{
					if (list.Count > 1)
					{
						Options.ScreenShotModeEnabled = Utils.ParseBool(list[1]);
						if (Options.ScreenShotModeEnabled)
						{
							Options.HitEffectFactor = 0f;
						}
					}
					AddMessage(string.Format("- screen_shot_mode: {0}", Options.ScreenShotModeEnabled ? "Enabled" : "Disabled"), ConsoleMessageType.Command);
				}
				else if (text2.Equals("bgr"))
				{
					if (list.Count == 1)
					{
						AddMessage($"- bgr = {Options.BgrColor.R}, {Options.BgrColor.G}, {Options.BgrColor.B}", ConsoleMessageType.Command);
					}
					else if (list.Count >= 2 && Utils.TryGetIntArray(GetSingleCommand(list), 3, 3, out resultArray))
					{
						Options.BgrColor = new Color(resultArray[0], resultArray[1], resultArray[2]);
						AddMessage($"- bgr = {Options.BgrColor.R}, {Options.BgrColor.G}, {Options.BgrColor.B}", ConsoleMessageType.Command);
					}
					else
					{
						AddMessage("- wrong params. Usage: bgr = R,G,B", ConsoleMessageType.Error);
					}
				}
				else if (text2.Equals("scissors"))
				{
					if (list.Count > 1 && int.TryParse(list[1], out Options.ScissorsMode))
					{
						Options.ScissorsMode = Utils.Clamp(Options.ScissorsMode, 0, 2);
					}
					AddMessage($"- scissors = {Options.ScissorsMode}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("ed_scissors"))
				{
					if (list.Count > 1 && int.TryParse(list[1], out GameCore.Editor.ScissorsMode))
					{
						GameCore.Editor.ScissorsMode = Utils.Clamp(GameCore.Editor.ScissorsMode, 0, 2);
					}
					AddMessage($"- ed_scissors = {GameCore.Editor.ScissorsMode}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("goto") || text2.Equals("gt") || text2.Equals("gtk"))
				{
					if (list.Count > 1 && Utils.TryGetIntArray(GetSingleCommand(list), 1, 2, out resultArray))
					{
						Vector2 position = ((resultArray.Length <= 1) ? new Vector2(resultArray[0], GameCore.CurrentLevel.Camera.Position.Y) : new Vector2(resultArray[0], resultArray[1]));
						if (text2.Equals("gtk"))
						{
							position *= 1000f;
						}
						GameCore.CurrentLevel.Camera.Position = position;
						AddMessage($"- goto = {position.X}, {position.Y}", ConsoleMessageType.Command);
					}
					else
					{
						AddMessage("- wrong params. Usage: goto = x[, y]", ConsoleMessageType.Error);
					}
				}
				else if (text2.Equals("debug_no_particles"))
				{
					if (list.Count > 1)
					{
						Options.DebugNoParticles = Utils.ParseBool(list[1]);
					}
					AddMessage($"- {text2} = {Options.DebugNoParticles}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("debug_pause"))
				{
					if (list.Count > 1)
					{
						Options.DebugPause = Utils.ParseBool(list[1]);
					}
					AddMessage($"- {text2} = {Options.DebugPause}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("debug"))
				{
					if (list.Count > 1 && int.TryParse(list[1], out Options.DebugQuickInfoMode))
					{
						if (Options.DebugQuickInfoMode == 0)
						{
							Options.DebugPath = false;
						}
						else
						{
							Options.DebugPath = true;
						}
					}
					AddMessage($"- {text2} = {Options.DebugQuickInfoMode}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("path"))
				{
					if (GameCore.Editor.CurrentObject != null)
					{
						string text5 = "path.txt";
						GameCore.Editor.SavePathToFile(GameCore.Editor.CurrentObject, text5);
						AddMessage($"- Path saved to '{text5}'", ConsoleMessageType.Command);
					}
				}
				else if (text2.Equals("skip_expired") || text2.Equals("sex"))
				{
					if (list.Count > 1)
					{
						Options.SkipExpiredObjects = Utils.ParseBool(list[1]);
					}
					AddMessage($"- {text2} = {Options.SkipExpiredObjects}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("dump_unused"))
				{
					DebugUtils.DumpUnusedEmitters(GameCore.GameLevel);
				}
				else if (text2.Equals("dump_object"))
				{
					GameObject.DebugPrint(GameCore.Editor.CurrentObject, 0);
				}
				else if (text2.Equals("level_file"))
				{
					DataConstant.Reload();
					if (list.Count == 1)
					{
						GameCore.Editor.ReloadLevelFull();
					}
					else
					{
						AddMessage($"- Loading level from '{list[1]}'...", ConsoleMessageType.Command);
						GameCore.LoadGameLevel(list[1], null);
					}
				}
				else if (text2.Equals("level_dir"))
				{
					DataConstant.Reload();
					if (list.Count == 1)
					{
						GameCore.Editor.ReloadLevelFull();
					}
					else
					{
						string text6 = Path.Combine(GameCore.LEVEL_DIR, list[1], "level.txt");
						AddMessage($"- Loading level from '{text6}'...", ConsoleMessageType.Command);
						GameCore.LoadGameLevel(text6, null);
					}
				}
				else if (text2.Equals("use_default_factors") || IsMasterCommand(text2, "udf"))
				{
					if (list.Count > 1)
					{
						Options.UseDefaultFactors = Utils.ParseBool(list[1]);
					}
					AddMessage($"- {text2} = {Options.UseDefaultFactors}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("campaign"))
				{
					GameJnG.Campaign.DebugPrint();
				}
				else if (IsMasterCommand(text2, "mia"))
				{
					if (list.Count > 1)
					{
						GameJnG.Mia = Utils.ParseBool(list[1]);
						if (GameJnG.ShopMenu.State != ShopMenuState.Hidden)
						{
							if (GameJnG.Mia)
							{
								GameJnG.ShopMenu.SwitchScreen(ShopScreenEnum.Mia);
							}
							else
							{
								GameJnG.ShopMenu.SwitchScreen(ShopScreenEnum.Shop);
								GameJnG.SetCheater();
							}
						}
					}
					AddMessage($"- {text2} = {GameJnG.Mia}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("test_all"))
				{
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					GameCore.ErrorCount = 0;
					foreach (LevelData value2 in GameJnG.LevelDataDictionary.Values)
					{
						GameCore.LoadGameLevel(value2.FileName, null);
					}
					stopwatch.Stop();
					AddMessage($"- Error count: {GameCore.ErrorCount}", ConsoleMessageType.System);
					AddMessage($"- Time: {(float)stopwatch.ElapsedMilliseconds / 1000f} sec.", ConsoleMessageType.System);
				}
				else if (text2.Equals("clear_campaign") || IsMasterCommand(text2, "cc"))
				{
					GameJnG.LevelStatsDictionary.Clear();
					GameJnG.SetStage(0);
					AddMessage("- Campaign stats was cleared.", ConsoleMessageType.Command);
				}
				else if (text2.Equals("difficulty") || text2.Equals("diff"))
				{
					if (list.Count > 1 && int.TryParse(list[1], out result))
					{
						GameJnG.SetDifficulty(result);
						SaveGame.Save(Options.SaveGameNumber);
					}
					AddMessage($"- {text2} = {GameJnG.Difficulty}", ConsoleMessageType.Command);
				}
				else if (text2.Equals("game_mode") || IsMasterCommand(text2, "gm"))
				{
					if (list.Count > 1 && int.TryParse(list[1], out result))
					{
						GameJnG.SetGameModeByNumber(result);
						SaveGame.Save(Options.SaveGameNumber);
						GameJnG.SetTexts();
					}
					AddMessage($"- {text2} = {(int)GameJnG.GameMode} ({GameJnG.GameMode})", ConsoleMessageType.Command);
				}
				else if (text2.Equals("stage"))
				{
					if (list.Count > 1)
					{
						if (int.TryParse(list[1], out var result5))
						{
							if (result5 > 999)
							{
								GameJnG.SetDifficulty(result5 / 1000);
								GameJnG.SetStage(Utils.Clamp(result5 % 1000, 0, GameJnG.Campaign.StageDataList.Max((StageData s) => s.Number)));
								AddMessage($"- Stage is now = {GameJnG.StageNumber} and difficulty = {GameJnG.Difficulty}", ConsoleMessageType.Command);
							}
							else
							{
								GameJnG.SetStage(Utils.Clamp(result5, 0, GameJnG.Campaign.StageDataList.Max((StageData s) => s.Number)));
								AddMessage($"- Stage is now = {GameJnG.StageNumber}", ConsoleMessageType.Command);
							}
						}
						else
						{
							AddMessage($"- Wrong stage number ({list[1]}).", ConsoleMessageType.Error);
						}
					}
					else
					{
						AddMessage($"- Stage : {GameJnG.StageNumber}", ConsoleMessageType.Command);
					}
				}
				else if (text2.Equals("level") || text2.Equals("l"))
				{
					if (list.Count == 1)
					{
						GameCore.Editor.ReloadLevelFull();
					}
					else
					{
						DataConstant.Reload();
						GameJnG.Campaign = Campaign.LoadCampaign(GameJnG.Difficulty);
						AddMessage($"- Campaign for difficulty {GameJnG.Difficulty} reloaded.", ConsoleMessageType.System);
						if (int.TryParse(list[1], out var result6))
						{
							List<LevelData> orderedLevelDataList = GameJnG.Campaign.GetOrderedLevelDataList();
							if (result6 > 0 && result6 <= orderedLevelDataList.Count)
							{
								LevelData levelData = orderedLevelDataList[result6 - 1];
								AddMessage($"- Loading campaign level #{result6} from '{levelData.FileName}'...", ConsoleMessageType.Command);
								if (GameCore.IsShopMenuEnabled)
								{
									GameJnG.ShopMenu.Hide(toMainMenu: false, levelData);
								}
								else
								{
									GameCore.LoadGameLevel(levelData, bAllowLowerStage: true);
								}
							}
							else
							{
								AddMessage($"- Level number {result6} is out of range (1 - {orderedLevelDataList.Count}).", ConsoleMessageType.Error);
							}
						}
						else
						{
							LevelData levelData2 = GameJnG.Campaign.GetLevelData(list[1]);
							if (levelData2 == null)
							{
								AddMessage($"! Level '{list[1]}' not found in campaign. Try to load from file.", ConsoleMessageType.System);
								string text7 = Path.Combine(GameCore.LEVEL_DIR, list[1], "level.txt");
								if (!GameContent.FileExits(text7))
								{
									AddMessage($"Unable to load level from file '{text7}'. File not exists.", ConsoleMessageType.Error);
								}
								else
								{
									if (GameCore.IsShopMenuEnabled)
									{
										GameJnG.ShopMenu.Hide(toMainMenu: false, null);
									}
									GameCore.LoadGameLevel(text7, null);
								}
							}
							else
							{
								AddMessage($"- Loading level from '{levelData2.FileName}'...", ConsoleMessageType.Command);
								if (GameCore.IsShopMenuEnabled)
								{
									GameJnG.StageNumber = 0;
									GameJnG.ShopMenu.Hide(toMainMenu: false, levelData2);
								}
								else
								{
									GameCore.LoadGameLevel(levelData2, bAllowLowerStage: true);
								}
							}
						}
						GameJnG.IsTestRangeEnabled = false;
					}
				}
				else if (text2.Equals("load"))
				{
					int result7;
					if (list.Count == 1)
					{
						result7 = Options.SaveGameNumber;
					}
					else if (!int.TryParse(list[1], out result7))
					{
						AddMessage($"! Wrong save game number: {list[1]}", ConsoleMessageType.Error);
						result7 = 0;
					}
					if (result7 > 0)
					{
						AddMessage($"- Loading savegame: '{result7}'...", ConsoleMessageType.Command);
						SaveGame.Load(result7, bFromCloud: false);
						GameCore.CurrentLevel.SetCurrentLayer(GameCore.GameLevel.MainLayer);
						if (GameCore.IsShopMenuEnabled && GameJnG.ShopMenu.MyHero != null)
						{
							GameJnG.ShopMenu.SetHero(GameJnG.HeroList[GameJnG.ShopMenu.MyHero.PlayerNumber]);
							GameJnG.ShopMenu.HeroPreviewLevel.SpawnHeroes(GameJnG.HeroList);
						}
						else
						{
							GameCore.CurrentLevel.SpawnHeroes(GameJnG.HeroList);
						}
					}
				}
				else
				{
					AddMessage($"! Unknown command: {CommandText}", ConsoleMessageType.Error);
				}
			}
			else
			{
				AddMessage($"! Unknown command: {CommandText}", ConsoleMessageType.Error);
			}
		}
		CommandText = "";
		_cursorPos = 0;
	}

	public bool KeyPressed(Keys key)
	{
		return GameInput.SystemKeyPressed(key);
	}

	public bool KeyDown(Keys key)
	{
		return GameInput.SystemKeyDown(key);
	}

	public void Update(float elapsedTime)
	{
		if (_state == ConsoleStateEnum.Showing)
		{
			_positionY += 4000f * elapsedTime;
			if (_positionY > 0f)
			{
				_positionY = 0f;
				_state = ConsoleStateEnum.Visible;
			}
		}
		else if (_state == ConsoleStateEnum.Hiding)
		{
			_positionY -= 4000f * elapsedTime;
			if (_positionY < (float)(-_rect.Height))
			{
				_positionY = -_rect.Height;
				_state = ConsoleStateEnum.Hidden;
			}
		}
		_rect.Y = (int)_positionY;
		_rect.Width = GameCore.MainViewPort.Width;
		_rect.Height = GameCore.MainViewPort.Height / 2;
		UpdateComandLine(elapsedTime);
		if (GameInput.ControlDown && (GameInput.SystemKeyPressed(Keys.OemTilde) || GameInput.SystemKeyPressed(Keys.F11)))
		{
			if (GameInput.ShiftDown)
			{
				ClearAll();
			}
			else
			{
				IsActive = !IsActive;
			}
		}
	}

	public void DrawCommandLine()
	{
		int num = (int)(_positionY + (float)_rect.Height - 18f);
		_font.Write(_spriteBatch, $"> {CommandText}", 4f, num, new Color(255, 255, 100, IsActive ? 255 : 200));
		int textWidth = _font.GetTextWidth(CommandText.Substring(0, _cursorPos));
		if (IsActive)
		{
			_font.Write(_spriteBatch, "|", 17 + textWidth, num, new Color(255, 255, 150, (int)(255.0 * (1.0 + Math.Sin(_cursorAlpha) * 0.5))));
		}
	}

	public void Draw()
	{
		if (_state == ConsoleStateEnum.Hidden)
		{
			return;
		}
		_triangleBatch.Begin(BlendState.NonPremultiplied, Matrix.Identity);
		_triangleBatch.DrawRect(_rect, IsActive ? new Color(0, 50, 80, 200) : new Color(50, 50, 50, 100));
		_triangleBatch.End();
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
		int num = (int)(_positionY + (float)_rect.Height - 36f);
		for (int num2 = _messageList.Count - 1; num2 >= 0; num2--)
		{
			ConsoleMessage consoleMessage = _messageList[num2];
			Color color = consoleMessage.Type switch
			{
				ConsoleMessageType.Error => new Color(255, 0, 0), 
				ConsoleMessageType.System => new Color(255, 255, 0), 
				ConsoleMessageType.Command => new Color(0, 255, 0), 
				ConsoleMessageType.Cheat => new Color(255, 100, 200), 
				_ => Color.White, 
			};
			_font.Write(_spriteBatch, consoleMessage.Text, 4f, num, color);
			num -= 16;
			if (num <= -16)
			{
				break;
			}
		}
		DrawCommandLine();
		_spriteBatch.End();
	}

	public void AddMessage(string text, ConsoleMessageType type = ConsoleMessageType.Normal)
	{
		string[] array = text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text2 in array)
		{
			_messageList.Add(new ConsoleMessage
			{
				Text = text2,
				Type = type
			});
		}
		while (_messageList.Count > MaxLineCount)
		{
			_messageList.RemoveAt(0);
		}
	}
}
