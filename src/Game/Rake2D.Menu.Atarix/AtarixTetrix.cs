using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;
using Rake2D.Sound;

namespace Rake2D.Menu.Atarix;

public class AtarixTetrix : AtarixContent
{
	public enum TetrixStateEnum
	{
		Menu,
		Game,
		GamePaused,
		GameOver,
		ChangingLevel,
		ComboEffect
	}

	public int CurrentLevelNumber;

	private const int SQUARE_W = 14;

	private const int SQUARE_H = 14;

	public static int PIT_HEIGHT = 31;

	public static int PIT_WIDTH = 12;

	public int TopScore;

	public List<LeaderboardEntry> LeaderBoardEntryList;

	public LeaderboardEntry LeaderBoardEntryMyUser;

	private TetrixStateEnum _tetrixState;

	private SpriteAtlas _atlas;

	private Vector2 _pitTopLeft;

	private TetrixBrick _currentBrick;

	private TetrixBrick _previewBrick;

	private int _brickPosX;

	private int _brickPosY;

	private float _fallTimer;

	private float _fallDelay;

	private float _brickPauseTimer;

	private Vector2 _pitQuakeOffset;

	private Vector2 _pitQuakeTimer;

	private List<TetrixBrick.TetrixBrickTypeEnum> _brickOffer;

	private float _comboKeyTimer;

	private int _score;

	private bool _wasActionKeyReleased;

	private float _repeatLeftTimer;

	private float _repeatRightTimer;

	private static Color[] _colorArray;

	private int[,] _pit;

	private Color[,] _effectPit;

	private float[] _rowBlinkTimerArray;

	private bool[] _rowToRemoveArray;

	private List<int> _comboKeyList;

	private Dictionary<int, AtarixLevel> _levelDictionary;

	private int _changingLevelPhase;

	private int _changingLevelRow;

	private float _changingLevelTimer;

	private TetrixBrick.TetrixBrickTypeEnum _comboEffectBrickType;

	private float _comboEffectTimer;

	private bool _isComboDone;

	private bool _wasComboUsed;

	private int _comboEffectBeginRow;

	private int _comboEffectEndRow;

	private int _comboEffectBeginCol;

	private int _comboEffectRemoveRow;

	private int _comboEffectTargetEndRow;

	private int _comboEffectMode;

	private int _comboEffectReplaceBrick;

	private BlendModeEnum _comboEffectBlendMode;

	private int _levelBrickCount;

	private ISound2D _fallSound;

	private float _fallSoundTimer;

	public AtarixTetrix(Atarix atarix, ShopMenu shopMenu)
		: base(atarix, shopMenu)
	{
		_atlas = GameCore.GetSpriteAtlas("atarix_mortal_tetrix");
		_menuItemList = new List<string>();
		for (int i = 0; i < 2; i++)
		{
			_menuItemList.Add("---");
		}
		_pit = new int[PIT_HEIGHT, PIT_WIDTH];
		_effectPit = new Color[PIT_HEIGHT, PIT_WIDTH];
		_menuOffsetY = 130;
		_colorArray = new Color[16]
		{
			new Color(255, 255, 0),
			new Color(0, 255, 0),
			new Color(0, 255, 255),
			new Color(82, 94, 254),
			new Color(255, 0, 255),
			new Color(255, 0, 0),
			new Color(255, 255, 255),
			new Color(86, 86, 86),
			new Color(192, 87, 0),
			new Color(0, 163, 0),
			new Color(0, 186, 255),
			new Color(0, 0, 255),
			new Color(182, 24, 173),
			new Color(184, 17, 0),
			new Color(170, 170, 170),
			new Color(40, 40, 40)
		};
		_comboKeyList = new List<int>();
		LoadLevels();
	}

	public void LoadLevels()
	{
		AtarixLevel atarixLevel = null;
		int num = 0;
		_levelDictionary = new Dictionary<int, AtarixLevel>();
		using IniFile iniFile = IniFile.OpenFromContent("ui/tetrix_level.txt");
		if (iniFile == null)
		{
			return;
		}
		while (iniFile.NextLine())
		{
			if (iniFile.ValueType == IniFileValueType.MainSection)
			{
				if (iniFile.KeyEquals("atarix_level"))
				{
					int intValue = iniFile.GetIntValue(-1);
					if (intValue < 0)
					{
						iniFile.LogErr($"Wrong atarix level number [atarix_level = {iniFile.Value}]");
						continue;
					}
					if (_levelDictionary.ContainsKey(intValue))
					{
						iniFile.LogErr($"[atarix_level = {intValue}] already defined - replace previous values");
					}
					atarixLevel = new AtarixLevel();
					_levelDictionary[intValue] = atarixLevel;
				}
				else
				{
					atarixLevel = null;
				}
			}
			else if (atarixLevel != null && iniFile.KeyEquals("row"))
			{
				num = ((iniFile.Id != null) ? iniFile.GetIdIntValue(-1) : (num + 1));
				if (num < 0 || num > 30)
				{
					iniFile.LogErr($"Wrong atarix row number ({num}')");
				}
				else
				{
					atarixLevel.SetRowByStr(num, iniFile.Value);
				}
			}
		}
	}

	private void BuildBrickOfferByLevel()
	{
		_brickOffer = new List<TetrixBrick.TetrixBrickTypeEnum>();
		_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.raiden);
		_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.sub_zero);
		_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.scorpion);
		_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.sonya);
		_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.johny_cage);
		_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.liu_kang);
		_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.kano);
		if (CurrentLevelNumber > 0)
		{
			_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.sheeva);
		}
		if (CurrentLevelNumber > 1)
		{
			_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.baraka);
		}
		if (CurrentLevelNumber > 2)
		{
			_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.kung_lao);
		}
		if (CurrentLevelNumber > 3)
		{
			_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.mileena);
		}
		if (CurrentLevelNumber > 4)
		{
			_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.sindel);
		}
		if (CurrentLevelNumber > 5)
		{
			_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.drax);
		}
		if (CurrentLevelNumber > 6)
		{
			_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.cyrax);
		}
		if (CurrentLevelNumber > 7)
		{
			_brickOffer.Add(TetrixBrick.TetrixBrickTypeEnum.sektor);
		}
	}

	public void StartLevel(int levelNumber)
	{
		int num = levelNumber % _levelDictionary.Count;
		if (!_levelDictionary.ContainsKey(num))
		{
			GameCore.LogErr($"Atarix level {num} is not defined");
			return;
		}
		AtarixLevel atarixLevel = _levelDictionary[num];
		int num2 = _pit.GetUpperBound(0) + 1;
		int num3 = _pit.GetUpperBound(1) + 1;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num3; j++)
			{
				_pit[i, j] = atarixLevel.GetBrick(i, j);
			}
		}
		CurrentLevelNumber = levelNumber;
		BuildBrickOfferByLevel();
		_fallDelay = GetFallDelay();
		_fallTimer = _fallDelay;
		_levelBrickCount = 0;
		_rowBlinkTimerArray = new float[_pit.GetLength(0)];
		_rowToRemoveArray = new bool[_pit.GetLength(0)];
		_previewBrick = null;
		StartNewBrick();
		_tetrixState = TetrixStateEnum.Game;
		GameCore.PlaySound("tetrix_start_level");
	}

	public override void RegisterEvents()
	{
		_atarix.OnUpButtonClick += OnUpButtonClick;
		_atarix.OnDownButtonClick += OnDownButtonClick;
		_atarix.OnLeftButtonClick += OnLeftButtonClick;
		_atarix.OnRightButtonClick += OnRightButtonClick;
		_atarix.OnActionButtonClick += OnActionButtonClick;
		_atarix.OnOffButtonClick += OnOffButtonClick;
	}

	private float GetFallDelay()
	{
		return Math.Max(0.033f, 0.6f - (float)CurrentLevelNumber * 0.03f);
	}

	private void OnUpButtonClick()
	{
		if (_tetrixState == TetrixStateEnum.Menu)
		{
			if (_selectedMenuItemIndex > 0)
			{
				_selectedMenuItemIndex--;
			}
			else
			{
				_selectedMenuItemIndex = _menuItemList.Count - 1;
			}
			GameCore.PlaySound("atarix_menu");
		}
		else
		{
			TryRotateBrick(ccw: false);
			PressComboKey(0);
		}
	}

	private void OnDownButtonClick()
	{
		if (_tetrixState == TetrixStateEnum.Menu)
		{
			if (_selectedMenuItemIndex < _menuItemList.Count - 1)
			{
				_selectedMenuItemIndex++;
			}
			else
			{
				_selectedMenuItemIndex = 0;
			}
			GameCore.PlaySound("atarix_menu");
		}
		else
		{
			TryRotateBrick(ccw: true);
			PressComboKey(2);
		}
	}

	private void OnLeftButtonClick()
	{
		if (_tetrixState == TetrixStateEnum.Game)
		{
			_repeatLeftTimer = 0.15f;
			TryMoveBrick(-1, 0);
			PressComboKey(3);
		}
	}

	private void OnRightButtonClick()
	{
		if (_tetrixState == TetrixStateEnum.Game)
		{
			_repeatRightTimer = 0.15f;
			TryMoveBrick(1, 0);
			PressComboKey(1);
		}
	}

	protected override void OnMenuItemClick(int menuItem)
	{
		base.OnMenuItemClick(menuItem);
		if (menuItem == 0)
		{
			SetState(TetrixStateEnum.Game);
		}
		else
		{
			_atarix.SetContent(_atarix.ContentMainMenu);
		}
	}

	private void OnActionButtonClick()
	{
		if (_tetrixState == TetrixStateEnum.Menu)
		{
			OnMenuItemClick(_selectedMenuItemIndex);
		}
		else if (_tetrixState == TetrixStateEnum.GameOver)
		{
			SetState(TetrixStateEnum.Menu);
		}
	}

	private void OnOffButtonClick()
	{
		if (_tetrixState == TetrixStateEnum.Menu)
		{
			_atarix.SetContent(_atarix.ContentMainMenu);
		}
		else
		{
			SetState(TetrixStateEnum.Menu);
		}
	}

	public override void SetTexts()
	{
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			_menuItemList[i] = TextBundle.GetText($"tetrix_menu_item_{i}");
		}
	}

	private void SetState(TetrixStateEnum tetrixState)
	{
		_tetrixState = tetrixState;
		switch (tetrixState)
		{
		case TetrixStateEnum.Game:
			_score = 0;
			StartLevel(0);
			break;
		case TetrixStateEnum.GameOver:
			GameCore.PlaySound("tetrix_game_over");
			break;
		case TetrixStateEnum.ChangingLevel:
			_changingLevelPhase = 1;
			_changingLevelRow = 0;
			break;
		case TetrixStateEnum.Menu:
			GameCore.PlaySound("tetrix_menu");
			if (TopScore <= _score)
			{
				TopScore = _score;
				GameCore.SteamManager.UploadTetrixLeaderboards(TopScore, CurrentLevelNumber);
			}
			GameCore.SteamManager.DownloadTetrixLeaderboard();
			break;
		case TetrixStateEnum.GamePaused:
			break;
		}
	}

	public override void Show()
	{
		base.Show();
		_selectedMenuItemIndex = 0;
		SetState(TetrixStateEnum.Menu);
	}

	private void TryRotateBrick(bool ccw)
	{
		TetrixBrick tetrixBrick = new TetrixBrick();
		if (ccw)
		{
			tetrixBrick.ShapeArray = Utils.RotateMatrixCounterClockwise(_currentBrick.ShapeArray);
		}
		else
		{
			tetrixBrick.ShapeArray = Utils.RotateMatrixClockwise(_currentBrick.ShapeArray);
		}
		tetrixBrick.GetPivot(out var pivotX, out var pivotY, out var pivotType);
		tetrixBrick.PivotType = pivotType;
		int num = _brickPosX + _currentBrick.PivotX - pivotX;
		int num2 = _brickPosY + _currentBrick.PivotY - pivotY;
		switch (tetrixBrick.PivotType)
		{
		case 3:
			if (ccw)
			{
				switch (_currentBrick.Rotation)
				{
				case 0:
					num2++;
					break;
				case 1:
					num--;
					break;
				case 2:
					num2--;
					break;
				case 3:
					num++;
					break;
				}
			}
			else
			{
				switch (_currentBrick.Rotation)
				{
				case 0:
					num++;
					break;
				case 1:
					num2++;
					break;
				case 2:
					num--;
					break;
				case 3:
					num2--;
					break;
				}
			}
			break;
		case 4:
			if (ccw)
			{
				switch (_currentBrick.Rotation)
				{
				case 0:
					num++;
					break;
				case 2:
					num2++;
					break;
				case 3:
					num--;
					break;
				}
			}
			else
			{
				switch (_currentBrick.Rotation)
				{
				case 2:
					num++;
					break;
				case 3:
					num--;
					num2++;
					break;
				}
			}
			break;
		case 5:
			if (ccw)
			{
				switch (_currentBrick.Rotation)
				{
				case 2:
					num--;
					break;
				case 3:
					num++;
					break;
				}
			}
			else
			{
				switch (_currentBrick.Rotation)
				{
				case 1:
					num++;
					break;
				case 2:
					num--;
					break;
				}
			}
			break;
		case 6:
			if (ccw)
			{
				switch (_currentBrick.Rotation)
				{
				case 0:
					num--;
					break;
				case 1:
					num--;
					break;
				case 2:
					num++;
					break;
				case 3:
					num++;
					break;
				}
			}
			else
			{
				switch (_currentBrick.Rotation)
				{
				case 0:
					num++;
					break;
				case 1:
					num--;
					break;
				case 2:
					num--;
					break;
				case 3:
					num++;
					break;
				}
			}
			break;
		}
		num = Math.Min(Math.Max(0, num), _pit.GetLength(1) - tetrixBrick.ShapeArray.GetLength(1));
		num2 = Math.Min(Math.Max(0, num2), _pit.GetLength(0) - tetrixBrick.ShapeArray.GetLength(0));
		if (CheckBrickAtPosition(tetrixBrick, num, num2))
		{
			_currentBrick.PivotX = pivotX;
			_currentBrick.PivotY = pivotY;
			_currentBrick.ShapeArray = tetrixBrick.ShapeArray;
			_brickPosX = num;
			_brickPosY = num2;
			_currentBrick.Rotation += ((!ccw) ? 1 : (-1));
			if (_currentBrick.Rotation < 0)
			{
				_currentBrick.Rotation = 3;
			}
			else if (_currentBrick.Rotation > 3)
			{
				_currentBrick.Rotation = 0;
			}
			GameCore.PlaySound("tetrix_rotate_0");
		}
	}

	public void TryMoveBrick(int deltaX, int deltaY)
	{
		if (CheckBrickAtPosition(_currentBrick, _brickPosX + deltaX, _brickPosY + deltaY))
		{
			_brickPosX += deltaX;
			_brickPosY += deltaY;
			GameCore.PlaySound("tetrix_move_0");
		}
	}

	private void RemovePitRow(int rowNumber)
	{
		int num = 0;
		for (int i = 0; i < _pit.GetLength(1); i++)
		{
			if (_pit[rowNumber, i] >= 0)
			{
				num++;
			}
		}
		AddScore(num);
		for (int num2 = rowNumber; num2 >= 0; num2--)
		{
			if (num2 == 0)
			{
				_rowBlinkTimerArray[num2] = 0f;
				_rowToRemoveArray[num2] = false;
			}
			else
			{
				_rowBlinkTimerArray[num2] = _rowBlinkTimerArray[num2 - 1];
				_rowToRemoveArray[num2] = _rowToRemoveArray[num2 - 1];
			}
			for (int j = 0; j < _pit.GetLength(1); j++)
			{
				if (num2 == 0)
				{
					_pit[num2, j] = -1;
				}
				else
				{
					_pit[num2, j] = _pit[num2 - 1, j];
				}
			}
		}
	}

	private void RemoveSquare(int row, int col)
	{
		if (_pit[row, col] >= 0)
		{
			_pit[row, col] = -1;
			AddScore(1);
		}
	}

	private void AddScore(int deltaScore)
	{
		_score += deltaScore;
		if (_score >= 10000)
		{
			GameJnG.UnlockAchievement(GameAchievementEnum.ACH_TETRIX_SCORE);
		}
	}

	private void CheckRows(float elapsedTime)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _pit.GetLength(0); i++)
		{
			bool flag = true;
			for (int j = 0; j < _pit.GetLength(1); j++)
			{
				if (_pit[i, j] < 0)
				{
					flag = false;
				}
				if (_pit[i, j] >= 0 && _pit[i, j] < 99)
				{
					num2++;
				}
			}
			if (flag && !_rowToRemoveArray[i])
			{
				_rowBlinkTimerArray[i] = 0.5f;
				_rowToRemoveArray[i] = true;
				num++;
			}
		}
		if (num > 0)
		{
			GameCore.PlaySound("tetrix_row");
		}
		if (num2 <= 0)
		{
			SetState(TetrixStateEnum.ChangingLevel);
		}
		if (num > 1)
		{
			AddScore(num * num * 8);
		}
		for (int num3 = _pit.GetLength(0) - 1; num3 >= 0; num3--)
		{
			if (_rowToRemoveArray[num3] && Utils.ChangeValueToTarget(ref _rowBlinkTimerArray[num3], 0f, elapsedTime))
			{
				RemovePitRow(num3);
			}
		}
	}

	private void StopFallSound()
	{
		if (_fallSound != null)
		{
			_fallSound.Stop();
			_fallSound = null;
		}
	}

	private void ParseInput(float elapsedTime)
	{
		if (_atarix.ButtonAction.IsPressed)
		{
			if (_wasActionKeyReleased)
			{
				_fallDelay = 0.025f;
				if (_fallTimer > _fallDelay)
				{
					_fallTimer = _fallDelay;
				}
				if (_fallSound == null)
				{
					_fallSound = GameCore.PlaySound("tetrix_fall");
					_fallSoundTimer = 0.2f;
				}
			}
		}
		else
		{
			Utils.ChangeValueToTarget(ref _fallSoundTimer, 0f, elapsedTime);
			if (_fallSoundTimer == 0f)
			{
				StopFallSound();
			}
			_wasActionKeyReleased = true;
			_fallDelay = GetFallDelay();
		}
		if (_atarix.ButtonLeft.IsPressed && Utils.ChangeValueToTarget(ref _repeatLeftTimer, 0f, elapsedTime))
		{
			_repeatLeftTimer = 0.05f;
			TryMoveBrick(-1, 0);
		}
		if (_atarix.ButtonRight.IsPressed && Utils.ChangeValueToTarget(ref _repeatRightTimer, 0f, elapsedTime))
		{
			_repeatRightTimer = 0.05f;
			TryMoveBrick(1, 0);
		}
	}

	private void PlaceBrick()
	{
		for (int i = 0; i < _currentBrick.ShapeArray.GetLength(0); i++)
		{
			for (int j = 0; j < _currentBrick.ShapeArray.GetLength(1); j++)
			{
				if (_currentBrick.ShapeArray[i, j] > 0)
				{
					_pit[_brickPosY + i, _brickPosX + j] = 99;
				}
			}
		}
		GameCore.PlaySound("tetrix_place_0");
		StartNewBrick();
	}

	public void UpdateGamePit(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (!Utils.ChangeValueToTarget(ref _pitQuakeTimer.X, 0f, elapsedTime))
		{
			_pitQuakeOffset.X = Utils.Sin(_pitQuakeTimer.X * 100f) * 20f * _pitQuakeTimer.X;
		}
		if (!Utils.ChangeValueToTarget(ref _pitQuakeTimer.Y, 0f, elapsedTime))
		{
			_pitQuakeOffset.Y = Utils.Sin(_pitQuakeTimer.Y * 100f) * 20f * _pitQuakeTimer.Y;
		}
		_pitTopLeft = _atarix.Position + new Vector2(-178f + _pitQuakeOffset.X, -236f + _pitQuakeOffset.Y);
		if (Utils.ChangeValueToTarget(ref _brickPauseTimer, 0f, elapsedTime))
		{
			ParseInput(elapsedTime);
			if (_currentBrick != null && Utils.ChangeValueToTarget(ref _fallTimer, 0f, elapsedTime))
			{
				_fallTimer = _fallDelay;
				if (CheckBrickAtPosition(_currentBrick, _brickPosX, _brickPosY + 1))
				{
					_brickPosY++;
				}
				else
				{
					PlaceBrick();
				}
			}
		}
		CheckRows(elapsedTime);
	}

	private int GetPitHeight()
	{
		return _pit.GetLength(0);
	}

	private int GetPitWidth()
	{
		return _pit.GetLength(1);
	}

	private void SetBrickPause(float timer)
	{
		if (_brickPauseTimer < timer)
		{
			_brickPauseTimer = timer;
		}
	}

	private bool PressComboKey(int key)
	{
		_comboKeyList.Add(key);
		_comboKeyTimer = 2f;
		return CheckCombo();
	}

	private bool CheckCombo()
	{
		if (_currentBrick.ComboKeyArray != null || _wasComboUsed)
		{
			bool flag = true;
			for (int i = 0; i < _comboKeyList.Count && i < _currentBrick.ComboKeyArray.Length; i++)
			{
				if (_comboKeyList[i] != _currentBrick.ComboKeyArray[i])
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				int num = _comboKeyList[_comboKeyList.Count - 1];
				_comboKeyList.Clear();
				if (num == _currentBrick.ComboKeyArray[0])
				{
					_comboKeyList.Add(num);
				}
			}
			else if (_comboKeyList.Count >= _currentBrick.ComboKeyArray.Length)
			{
				PrepareCombo(_currentBrick.Type);
				return true;
			}
		}
		else
		{
			_comboKeyList.Clear();
		}
		return false;
	}

	private void PrepareCombo(TetrixBrick.TetrixBrickTypeEnum comboEffectBrickType)
	{
		if (_wasComboUsed)
		{
			return;
		}
		GameCore.PlaySound("tetrix_combo_0");
		_wasComboUsed = true;
		AddScore(10);
		for (int i = 0; i < _effectPit.GetLength(0); i++)
		{
			for (int j = 0; j < _effectPit.GetLength(1); j++)
			{
				_effectPit[i, j] = Color.Transparent;
			}
		}
		_comboEffectBrickType = comboEffectBrickType;
		_tetrixState = TetrixStateEnum.ComboEffect;
		_comboEffectTimer = 0.018f;
		_comboEffectBlendMode = BlendModeEnum.Normal;
		_comboEffectTargetEndRow = PIT_HEIGHT - 1;
		_comboEffectBeginCol = 0;
		_comboEffectRemoveRow = -1;
		switch (_comboEffectBrickType)
		{
		case TetrixBrick.TetrixBrickTypeEnum.baraka:
		{
			int l = Math.Min(_pit.GetLength(1) - 1, _brickPosX + 1);
			_comboEffectBeginRow = Math.Min(_brickPosY + 1, _pit.GetLength(0) - 1);
			_comboEffectEndRow = _comboEffectBeginRow;
			for (int num2 = _brickPosY + 1; num2 < _pit.GetLength(0); num2++)
			{
				if (_pit[num2, l] >= 0)
				{
					for (int num3 = 0; num3 < _effectPit.GetLength(1); num3++)
					{
						_comboEffectRemoveRow = ((num2 > 0) ? (num2 - 1) : 0);
						_effectPit[_comboEffectRemoveRow, num3] = new Color(255, 0, 0);
					}
					break;
				}
				_effectPit[num2, l] = new Color(255, 0, 0);
			}
			_comboEffectMode = 1;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.sub_zero:
		{
			int l = Math.Min(_pit.GetLength(1) - 3, _brickPosX);
			_comboEffectBeginRow = Math.Min(_brickPosY, _pit.GetLength(0) - 1);
			_comboEffectEndRow = _comboEffectBeginRow;
			for (int num6 = l; num6 < l + 3; num6++)
			{
				bool flag2 = false;
				for (int num7 = _brickPosY + 1; num7 < _pit.GetLength(0); num7++)
				{
					if (_pit[num7, num6] >= 0)
					{
						flag2 = true;
					}
					if (flag2)
					{
						_effectPit[num7, num6] = Color.Cyan;
					}
					else
					{
						_effectPit[num7, num6] = new Color(0, 100, 200);
					}
				}
			}
			_comboEffectMode = 1;
			_comboEffectReplaceBrick = 2;
			_comboEffectBlendMode = BlendModeEnum.Additive;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.drax:
		{
			_comboEffectBeginRow = _brickPosY;
			_comboEffectEndRow = _comboEffectBeginRow + 1;
			for (int m = _comboEffectBeginRow; m <= _comboEffectEndRow; m++)
			{
				for (int l = 0; l < _effectPit.GetLength(1); l++)
				{
					_effectPit[m, l] = new Color(0, 255, 255);
				}
			}
			_comboEffectTimer = 0.5f;
			_comboEffectBlendMode = BlendModeEnum.Additive;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.johny_cage:
		{
			_comboEffectBeginCol = _brickPosX;
			_comboEffectBeginCol = Utils.Clamp(_comboEffectBeginCol, 0, _pit.GetLength(1) - 3);
			_comboEffectBeginRow = Math.Min(_brickPosY + 1, _pit.GetLength(0) - 1);
			_comboEffectEndRow = _comboEffectBeginRow;
			for (int num8 = _comboEffectBeginCol; num8 < _comboEffectBeginCol + 3; num8++)
			{
				for (int num9 = _brickPosY + 1; num9 < _pit.GetLength(0); num9++)
				{
					if (_pit[num9, num8] >= 0)
					{
						_comboEffectTargetEndRow = num9;
						break;
					}
					_effectPit[num9, num8] = new Color(100, 100, 100);
				}
			}
			_comboEffectMode = 1;
			_comboEffectBlendMode = BlendModeEnum.Normal;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.liu_kang:
		{
			int l = ((_currentBrick.Rotation != 1 && _currentBrick.Rotation != 3) ? (_brickPosX + 1) : _brickPosX);
			_comboEffectBeginRow = _brickPosY + 1;
			_comboEffectEndRow = _comboEffectBeginRow;
			for (int num4 = _brickPosY + 1; num4 < _pit.GetLength(0); num4++)
			{
				if (_pit[num4, l] >= 0)
				{
					_comboEffectTargetEndRow = num4 - 1;
					break;
				}
				_effectPit[num4, l] = new Color(255, 0, 0);
			}
			_comboEffectMode = 10;
			_comboEffectBlendMode = BlendModeEnum.Additive;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.sektor:
		{
			for (int num11 = 0; num11 < 7; num11++)
			{
				int num12 = _brickPosY - 2 + num11;
				int num13;
				switch (num11)
				{
				case 0:
				case 6:
					num13 = 1;
					break;
				case 1:
				case 5:
					num13 = 2;
					break;
				default:
					num13 = 3;
					break;
				}
				if (num12 < 0 || num12 >= PIT_HEIGHT)
				{
					continue;
				}
				for (int l = _brickPosX - num13; l <= _brickPosX + num13; l++)
				{
					if (l >= 0 && l < PIT_WIDTH)
					{
						_effectPit[num12, l] = new Color(255, 255, 0);
					}
				}
			}
			_comboEffectBeginRow = _brickPosY - 2;
			_comboEffectEndRow = _brickPosY + 4;
			_comboEffectTimer = 0.5f;
			_comboEffectBlendMode = BlendModeEnum.Additive;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.mileena:
		{
			int val = _brickPosX + 3;
			val = Math.Min(val, PIT_WIDTH);
			_comboEffectBeginRow = Math.Min(_brickPosY, _pit.GetLength(0) - 1);
			_comboEffectEndRow = _comboEffectBeginRow;
			for (int num10 = _comboEffectBeginRow; num10 < _pit.GetLength(0); num10++)
			{
				for (int l = _brickPosX; l < val; l++)
				{
					_effectPit[num10, l] = new Color(255, 255, 255);
				}
			}
			_comboEffectMode = 1;
			_comboEffectBlendMode = BlendModeEnum.Additive;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.kano:
		{
			int l = _brickPosX;
			_comboEffectBeginRow = Math.Min(_brickPosY, _pit.GetLength(0) - 1);
			_comboEffectEndRow = _comboEffectBeginRow;
			for (int n = _comboEffectBeginRow; n < PIT_HEIGHT; n++)
			{
				if (n < PIT_HEIGHT - 2)
				{
					_effectPit[n, l] = new Color(160, 50, 0);
				}
				else
				{
					_effectPit[n, l] = new Color(192, 87, 0);
				}
				_effectPit[n, l + 1] = _effectPit[n, l];
			}
			_comboEffectMode = 1;
			_comboEffectBlendMode = BlendModeEnum.Normal;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.kung_lao:
		{
			int l = _brickPosX;
			if (_currentBrick.Rotation == 1 || _currentBrick.Rotation == 2)
			{
				l++;
			}
			_comboEffectBeginRow = _brickPosY + 1;
			_comboEffectEndRow = _comboEffectBeginRow;
			_comboEffectTargetEndRow = PIT_HEIGHT - 1;
			for (int num5 = _brickPosY + 1; num5 < _pit.GetLength(0); num5++)
			{
				if (_pit[num5, l] >= 0)
				{
					_comboEffectTargetEndRow = num5 - 1;
					break;
				}
				_effectPit[num5, l] = new Color(0, 186, 255);
			}
			_comboEffectMode = 10;
			_comboEffectReplaceBrick = 99;
			_comboEffectBlendMode = BlendModeEnum.Normal;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.sonya:
		{
			_comboEffectBeginRow = 0;
			_comboEffectEndRow = 0;
			_comboEffectTargetEndRow = PIT_HEIGHT - 1;
			for (int l = 0; l < _effectPit.GetLength(1); l++)
			{
				for (int num = 0; num < _pit.GetLength(0); num++)
				{
					if (_pit[num, l] >= 0)
					{
						_effectPit[num, l] = Color.White;
						if (num < _comboEffectEndRow)
						{
							_comboEffectTargetEndRow = num;
						}
						break;
					}
					_effectPit[num, l] = new Color(255, 0, 0);
				}
			}
			_comboEffectMode = 10;
			_comboEffectBlendMode = BlendModeEnum.Additive;
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.cyrax:
		{
			bool flag = false;
			_comboEffectBeginRow = PIT_HEIGHT - 1;
			for (int k = 0; k < PIT_HEIGHT; k++)
			{
				for (int l = 0; l < PIT_WIDTH; l++)
				{
					if (k >= PIT_HEIGHT - 1 || _pit[k + 1, l] >= 0)
					{
						flag = true;
						if (_comboEffectBeginRow > k)
						{
							_comboEffectBeginRow = k;
							_comboEffectEndRow = _comboEffectBeginRow;
						}
					}
					if (flag)
					{
						_effectPit[k, l] = new Color(0, 255, 0);
					}
				}
			}
			_comboEffectMode = 1;
			_comboEffectReplaceBrick = 1;
			break;
		}
		}
		_comboEffectBeginRow = Utils.Clamp(_comboEffectBeginRow, 0, _pit.GetLength(0) - 1);
		_comboEffectEndRow = Utils.Clamp(_comboEffectEndRow, 0, _pit.GetLength(0) - 1);
		_comboEffectTargetEndRow = Utils.Clamp(_comboEffectTargetEndRow, 0, _pit.GetLength(0) - 1);
	}

	private void UpdateComboEffect(float elapsedTime)
	{
		TetrixBrick.TetrixBrickTypeEnum comboEffectBrickType = _comboEffectBrickType;
		if (comboEffectBrickType == TetrixBrick.TetrixBrickTypeEnum.drax)
		{
			if (Utils.ChangeValueToTarget(ref _comboEffectTimer, 0f, elapsedTime))
			{
				DoCombo();
				_tetrixState = TetrixStateEnum.Game;
			}
		}
		else if (_comboEffectMode == 1)
		{
			if (Utils.ChangeValueToTarget(ref _comboEffectTimer, 0f, elapsedTime))
			{
				_comboEffectTimer = 0.018f;
				_comboEffectEndRow++;
				if (_comboEffectEndRow >= _comboEffectTargetEndRow)
				{
					_comboEffectMode = 2;
					DoCombo();
				}
			}
		}
		else if (_comboEffectMode == 2)
		{
			if (Utils.ChangeValueToTarget(ref _comboEffectTimer, 0f, elapsedTime))
			{
				_comboEffectBeginRow++;
				if (_comboEffectBeginRow > _comboEffectTargetEndRow)
				{
					_comboEffectMode = 0;
					_tetrixState = TetrixStateEnum.Game;
				}
			}
		}
		else if (!_isComboDone)
		{
			if (_comboEffectMode == 10)
			{
				if (Utils.ChangeValueToTarget(ref _comboEffectTimer, 0f, elapsedTime))
				{
					if (_comboEffectEndRow == _comboEffectTargetEndRow)
					{
						_comboEffectTimer = 0.25f;
					}
					else
					{
						_comboEffectTimer = 0.018f;
					}
					_comboEffectBeginRow++;
					_comboEffectEndRow = _comboEffectBeginRow;
					if (_comboEffectEndRow > _comboEffectTargetEndRow)
					{
						DoCombo();
						_tetrixState = TetrixStateEnum.Game;
					}
				}
			}
			else if (Utils.ChangeValueToTarget(ref _comboEffectTimer, 0f, elapsedTime))
			{
				DoCombo();
				_tetrixState = TetrixStateEnum.Game;
			}
		}
		else
		{
			_tetrixState = TetrixStateEnum.Game;
		}
		_comboEffectBeginRow = Utils.Clamp(_comboEffectBeginRow, 0, _effectPit.GetLength(0));
		_comboEffectEndRow = Utils.Clamp(_comboEffectEndRow, 0, _effectPit.GetLength(0));
	}

	private void FlipPit(int fromRow)
	{
		int[,] array = new int[PIT_HEIGHT, PIT_WIDTH];
		int num = PIT_HEIGHT - 1;
		for (int i = 0; i < PIT_HEIGHT; i++)
		{
			for (int j = 0; j < PIT_WIDTH; j++)
			{
				if (i < fromRow)
				{
					array[i, j] = _pit[i, j];
				}
				else
				{
					array[num, j] = _pit[i, j];
				}
			}
			if (i >= fromRow)
			{
				num--;
			}
		}
		_pit = array;
	}

	private void DoCombo()
	{
		_isComboDone = true;
		switch (_currentBrick.Type)
		{
		case TetrixBrick.TetrixBrickTypeEnum.sindel:
		{
			for (int num15 = 0; num15 < GetPitHeight(); num15++)
			{
				for (int num16 = 0; num16 < GetPitWidth(); num16++)
				{
					if (_pit[num15, num16] >= 0)
					{
						continue;
					}
					for (int num17 = num16 + 1; num17 < GetPitWidth(); num17++)
					{
						if (_pit[num15, num17] >= 0)
						{
							_pit[num15, num16] = _pit[num15, num17];
							_pit[num15, num17] = -1;
							break;
						}
					}
				}
			}
			_pitQuakeTimer.X = 0.5f;
			GameCore.PlaySound("tetrix_combo_1");
			SetBrickPause(_pitQuakeTimer.X);
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.goro:
		{
			for (int num5 = 0; num5 < GetPitWidth(); num5++)
			{
				for (int num6 = GetPitHeight() - 1; num6 >= 0; num6--)
				{
					if (_pit[num6, num5] < 0)
					{
						for (int num7 = num6 - 1; num7 >= 0; num7--)
						{
							if (_pit[num7, num5] >= 0)
							{
								_pit[num6, num5] = _pit[num7, num5];
								_pit[num7, num5] = -1;
								break;
							}
						}
					}
				}
			}
			_pitQuakeTimer.Y = 0.5f;
			GameCore.PlaySound("tetrix_combo_1");
			SetBrickPause(_pitQuakeTimer.Y);
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.scorpion:
		{
			bool flag = false;
			for (int num20 = 0; num20 < PIT_HEIGHT; num20++)
			{
				if (flag)
				{
					break;
				}
				for (int num21 = 0; num21 < PIT_WIDTH; num21++)
				{
					if (_pit[num20, num21] >= 0)
					{
						FlipPit(num20);
						flag = true;
						break;
					}
				}
			}
			_pitQuakeTimer.Y = 0.5f;
			GameCore.PlaySound("tetrix_combo_1");
			SetBrickPause(_pitQuakeTimer.Y);
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.sheeva:
		{
			for (int j = 0; j < GetPitHeight(); j++)
			{
				for (int k = 0; k < GetPitWidth() / 2; k++)
				{
					if (_pit[j, k] >= 0)
					{
						continue;
					}
					for (int l = k + 1; l < GetPitWidth() / 2; l++)
					{
						if (_pit[j, l] >= 0)
						{
							_pit[j, k] = _pit[j, l];
							_pit[j, l] = -1;
							break;
						}
					}
				}
				for (int num3 = GetPitWidth() - 1; num3 >= GetPitWidth() / 2; num3--)
				{
					if (_pit[j, num3] < 0)
					{
						for (int num4 = num3 - 1; num4 >= GetPitWidth() / 2; num4--)
						{
							if (_pit[j, num4] >= 0)
							{
								_pit[j, num3] = _pit[j, num4];
								_pit[j, num4] = -1;
								break;
							}
						}
					}
				}
			}
			_pitQuakeTimer.Y = 0.5f;
			GameCore.PlaySound("tetrix_combo_1");
			SetBrickPause(_pitQuakeTimer.Y);
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.raiden:
			_brickPosY = GetPitHeight() - _currentBrick.ShapeArray.GetLength(0);
			PlaceBrick();
			break;
		case TetrixBrick.TetrixBrickTypeEnum.johny_cage:
		{
			for (int num12 = _comboEffectBeginCol; num12 < _comboEffectBeginCol + 3; num12++)
			{
				for (int num13 = PIT_HEIGHT - 1; num13 > _brickPosY + _currentBrick.PivotY; num13--)
				{
					if (_pit[num13, num12] < 0)
					{
						for (int num14 = num13 - 1; num14 >= 0; num14--)
						{
							if (_pit[num14, num12] >= 0)
							{
								_pit[num13, num12] = _pit[num14, num12];
								_pit[num14, num12] = -1;
								break;
							}
						}
					}
				}
			}
			GameCore.PlaySound("tetrix_combo_1");
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.kung_lao:
		{
			bool flag2 = false;
			int num24 = GetPitHeight() - 1;
			while (num24 >= 0 && !flag2)
			{
				for (int num25 = GetPitWidth() - 1; num25 >= 0; num25--)
				{
					if (_effectPit[num24, num25] != Color.Transparent)
					{
						_pit[num24, num25] = _comboEffectReplaceBrick;
						flag2 = true;
						break;
					}
				}
				num24--;
			}
			GameCore.PlaySound("tetrix_combo_1");
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.baraka:
			GameCore.PlaySound("tetrix_combo_1");
			break;
		case TetrixBrick.TetrixBrickTypeEnum.sonya:
		case TetrixBrick.TetrixBrickTypeEnum.sektor:
		{
			for (int num8 = GetPitHeight() - 1; num8 >= 0; num8--)
			{
				for (int num9 = GetPitWidth() - 1; num9 >= 0; num9--)
				{
					if (_effectPit[num8, num9] != Color.Transparent)
					{
						RemoveSquare(num8, num9);
					}
				}
			}
			if (_currentBrick.Type == TetrixBrick.TetrixBrickTypeEnum.sektor)
			{
				StartNewBrick();
			}
			else
			{
				GameCore.PlaySound("tetrix_combo_1");
			}
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.kano:
		{
			for (int num = GetPitHeight() - 1; num >= 0; num--)
			{
				for (int num2 = GetPitWidth() - 1; num2 >= 0; num2--)
				{
					if (_effectPit[num, num2] != Color.Transparent)
					{
						if (num >= PIT_HEIGHT - 2)
						{
							_pit[num, num2] = 8;
						}
						else
						{
							RemoveSquare(num, num2);
						}
					}
				}
			}
			StartNewBrick();
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.mileena:
		{
			for (int num22 = GetPitHeight() - 1; num22 >= 0; num22--)
			{
				for (int num23 = GetPitWidth() - 1; num23 >= 0; num23--)
				{
					if (_effectPit[num22, num23] != Color.Transparent && _pit[num22, num23] == 99)
					{
						RemoveSquare(num22, num23);
					}
				}
			}
			GameCore.PlaySound("tetrix_combo_1");
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.liu_kang:
		{
			for (int num18 = GetPitWidth() - 1; num18 >= 0; num18--)
			{
				if (_effectPit[_comboEffectTargetEndRow, num18] != Color.Transparent)
				{
					for (int num19 = _comboEffectTargetEndRow + 1; num19 < GetPitHeight() && num19 < PIT_HEIGHT; num19++)
					{
						RemoveSquare(num19, num18);
					}
					break;
				}
			}
			GameCore.PlaySound("tetrix_combo_1");
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.sub_zero:
		{
			for (int num10 = GetPitHeight() - 1; num10 >= 0; num10--)
			{
				for (int num11 = GetPitWidth() - 1; num11 >= 0; num11--)
				{
					if (_effectPit[num10, num11] == Color.Cyan && _pit[num10, num11] == -1)
					{
						_pit[num10, num11] = _comboEffectReplaceBrick;
					}
				}
			}
			GameCore.PlaySound("tetrix_combo_1");
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.cyrax:
		{
			for (int m = 0; m < PIT_HEIGHT; m++)
			{
				for (int n = 0; n < PIT_WIDTH; n++)
				{
					if (_pit[m, n] < 0 && (m == PIT_HEIGHT - 1 || _pit[m + 1, n] >= 0))
					{
						_pit[m, n] = _comboEffectReplaceBrick;
					}
				}
			}
			GameCore.PlaySound("tetrix_combo_1");
			break;
		}
		case TetrixBrick.TetrixBrickTypeEnum.drax:
		{
			for (int i = 0; i < GetPitHeight(); i++)
			{
				if (_effectPit[i, 0] != Color.Transparent)
				{
					RemovePitRow(i);
				}
			}
			GameCore.PlaySound("tetrix_combo_1");
			break;
		}
		}
		if (_comboEffectRemoveRow >= 0)
		{
			RemovePitRow(_comboEffectRemoveRow);
		}
		_comboKeyList.Clear();
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (GameInput.SystemKeyPressed(Keys.P))
		{
			if (_tetrixState == TetrixStateEnum.Game)
			{
				_tetrixState = TetrixStateEnum.GamePaused;
			}
			else if (_tetrixState == TetrixStateEnum.GamePaused)
			{
				_tetrixState = TetrixStateEnum.Game;
			}
		}
		switch (_tetrixState)
		{
		case TetrixStateEnum.Menu:
			UpdateMouseInMenu();
			break;
		case TetrixStateEnum.Game:
			UpdateGamePit(elapsedTime);
			if (Utils.ChangeValueToTarget(ref _comboKeyTimer, 0f, elapsedTime))
			{
				_comboKeyList.Clear();
			}
			break;
		case TetrixStateEnum.ChangingLevel:
			if (!Utils.ChangeValueToTarget(ref _changingLevelTimer, 0f, elapsedTime))
			{
				break;
			}
			_changingLevelTimer = 0.02f;
			if (_changingLevelPhase == 1)
			{
				_changingLevelRow++;
				if (_changingLevelRow >= _pit.GetUpperBound(0))
				{
					_changingLevelRow = _pit.GetUpperBound(0);
					_changingLevelPhase = 2;
					StartLevel(CurrentLevelNumber + 1);
					AddScore(1000);
				}
			}
			else if (_changingLevelPhase == 2)
			{
				_changingLevelRow--;
				if (_changingLevelRow <= 0)
				{
					_changingLevelRow = 0;
					_changingLevelPhase = 0;
					_tetrixState = TetrixStateEnum.Game;
				}
			}
			break;
		case TetrixStateEnum.ComboEffect:
			UpdateComboEffect(elapsedTime);
			break;
		case TetrixStateEnum.GamePaused:
		case TetrixStateEnum.GameOver:
			break;
		}
	}

	private void DrawLeaderBoard()
	{
		Transformation transformation = new Transformation(_atarix);
		_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("tetrix_leaderboard"), transformation.Position.X, transformation.Position.Y - 120f, Atarix.FONT_SCALE_DEFAULT, new Color(220, 220, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
		if (LeaderBoardEntryList != null)
		{
			transformation.Position.X -= 130f;
			transformation.Position.Y -= 84f;
			bool flag = false;
			for (int i = 0; i < 7; i++)
			{
				if (i < LeaderBoardEntryList.Count)
				{
					LeaderboardEntry leaderboardEntry = LeaderBoardEntryList[i];
					Color value;
					if (leaderboardEntry.IsMyUser)
					{
						value = new Color(255, 255, 255);
						flag = true;
					}
					else
					{
						value = new Color(0, 200, 0);
					}
					_font.Write(_shopMenu.SpriteBatchNoLight, $"{leaderboardEntry.Rank}.", transformation.Position.X, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Right, 700, _shopMenu.ShowHidePercentage);
					_font.Write(_shopMenu.SpriteBatchNoLight, $"{leaderboardEntry.UserName}", transformation.Position.X + 20f, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Left, 190, _shopMenu.ShowHidePercentage);
					_font.Write(_shopMenu.SpriteBatchNoLight, $"{leaderboardEntry.Score}", transformation.Position.X + 230f, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Left, 700, _shopMenu.ShowHidePercentage);
				}
				else
				{
					Color value = new Color(100, 120, 100);
					_font.Write(_shopMenu.SpriteBatchNoLight, $"{i + 1}.", transformation.Position.X, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Right, 700, _shopMenu.ShowHidePercentage);
					_font.Write(_shopMenu.SpriteBatchNoLight, "---", transformation.Position.X + 20f, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Left, 190, _shopMenu.ShowHidePercentage);
					_font.Write(_shopMenu.SpriteBatchNoLight, "---", transformation.Position.X + 230f, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Left, 700, _shopMenu.ShowHidePercentage);
				}
				transformation.Position.Y += 22f;
			}
			if (!flag && LeaderBoardEntryMyUser != null)
			{
				transformation.Position.Y -= 6f;
				Color value = new Color(255, 255, 255);
				LeaderboardEntry leaderboardEntry = LeaderBoardEntryMyUser;
				_font.Write(_shopMenu.SpriteBatchNoLight, "...", transformation.Position.X, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, new Color(0, 100, 0), TextAlign.Right, 700, _shopMenu.ShowHidePercentage);
				transformation.Position.Y += 25f;
				_font.Write(_shopMenu.SpriteBatchNoLight, $"{leaderboardEntry.Rank}.", transformation.Position.X, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Right, 700, _shopMenu.ShowHidePercentage);
				_font.Write(_shopMenu.SpriteBatchNoLight, $"{leaderboardEntry.UserName}", transformation.Position.X + 20f, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Left, 190, _shopMenu.ShowHidePercentage);
				_font.Write(_shopMenu.SpriteBatchNoLight, $"{leaderboardEntry.Score}", transformation.Position.X + 230f, transformation.Position.Y, Atarix.FONT_SCALE_DEFAULT, value, TextAlign.Left, 700, _shopMenu.ShowHidePercentage);
			}
		}
		else
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("tetrix_downloading_leaderboad"), transformation.Position.X, transformation.Position.Y - 50f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 180, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
		}
	}

	private void DrawMenu()
	{
		Transformation transformation = new Transformation(_atarix);
		transformation.Position.Y -= 170f;
		_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 3, new Color(1f, 1f, 1f, _atarix.HideAlphaFactor), transformation, BlendModeEnum.Normal);
		DrawLeaderBoard();
		transformation = new Transformation(_atarix);
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			transformation.Position = GetMenuItemPosition(i);
			transformation.Scale = new Vector2(1.5f, 1.6f);
			string text = ((i != 1) ? _menuItemList[i] : _menuItemList[i].Replace("|hack_chance|", $"{GameJnG.AtarixAutoHackChance}"));
			if (i == _selectedMenuItemIndex)
			{
				_atarix.Atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, new Color(0, 200, 0, (int)_atarix.Color.A), transformation, BlendModeEnum.Normal);
				_font.Write(_shopMenu.SpriteBatchNoLight, text, transformation.Position.X, transformation.Position.Y - 9f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 0, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			}
			else
			{
				_font.Write(_shopMenu.SpriteBatchNoLight, text, transformation.Position.X, transformation.Position.Y - 9f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 180, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			}
		}
	}

	private void DrawGameInfoPanel()
	{
		float num = _atarix.Position.X + 107f;
		_font.Write(_shopMenu.SpriteBatchNoLight, string.Format("{0}:", TextBundle.GetText("tetrix_next")), num - 70f, _atarix.Position.Y, Atarix.FONT_SCALE_DEFAULT, new Color(255, 200, 0), TextAlign.Left, 0, _atarix.HideAlphaFactor);
		if (_previewBrick != null)
		{
			DrawBrick(_previewBrick, _atarix.Position + new Vector2(130 - _previewBrick.PivotX * 14, -(_previewBrick.PivotY * 14)), inPit: false);
		}
		_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText($"tetrix_brick_name_{_currentBrick.Type}"), num, _atarix.Position.Y - 110f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 255, 255), TextAlign.Center, 0, _atarix.HideAlphaFactor);
		_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText($"tetrix_brick_combo_{_currentBrick.Type}"), num, _atarix.Position.Y - 87f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 255, 0), TextAlign.Center, 0, _atarix.HideAlphaFactor);
		if (_currentBrick.ComboKeyArray != null && !_wasComboUsed)
		{
			Transformation transformation = new Transformation();
			transformation.Position.X = num - (float)(_currentBrick.ComboKeyArray.Length / 2 * 15);
			transformation.Position.Y = _atarix.Position.Y - 55f;
			for (int i = 0; i < _currentBrick.ComboKeyArray.Length; i++)
			{
				Color color = ((_comboKeyList.Count <= i || _currentBrick.ComboKeyArray[i] != _comboKeyList[i]) ? new Color(255, 255, 255, (int)_atarix.Alpha) : new Color(255, 200, 0, (int)_atarix.Alpha));
				_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 6 + _currentBrick.ComboKeyArray[i], color, transformation, BlendModeEnum.Normal);
				transformation.Position.X += 18f;
			}
		}
		else
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, string.Format("{0}", TextBundle.GetText("tetrix_combo_used")), num, _atarix.Position.Y - 63f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 0, 0), TextAlign.Center, 0, _atarix.HideAlphaFactor);
		}
		_font.Write(_shopMenu.SpriteBatchNoLight, string.Format("{0}", TextBundle.GetText("tetrix_level")), num, _atarix.Position.Y + 59f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 200, 0), TextAlign.Center, 0, _atarix.HideAlphaFactor);
		_font.Write(_shopMenu.SpriteBatchNoLight, $"{CurrentLevelNumber + 1}", num, _atarix.Position.Y + 79f, new Vector2(0.8f), new Color(255, 255, 255), TextAlign.Center, 0, _atarix.HideAlphaFactor);
		_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("tetrix_score"), num, _atarix.Position.Y + 133f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 200, 0), TextAlign.Center, 0, _atarix.HideAlphaFactor);
		_font.Write(_shopMenu.SpriteBatchNoLight, Utils.FormatMoney(_score), num, _atarix.Position.Y + 153f, new Vector2(0.8f), new Color(255, 255, 255), TextAlign.Center, 0, _atarix.HideAlphaFactor);
	}

	public static Color GetSquareColor(int colorNumber)
	{
		if (colorNumber >= 0 && colorNumber < _colorArray.Length)
		{
			return _colorArray[colorNumber];
		}
		return new Color(255, 0, 0);
	}

	private void DrawBrick(TetrixBrick brick, Vector2 position, bool inPit)
	{
		Transformation transformation = new Transformation(_atarix);
		transformation.Position = position;
		Color squareColor = GetSquareColor(brick.SquareColorNumber);
		if (inPit && _tetrixState == TetrixStateEnum.GamePaused)
		{
			squareColor.A = 100;
		}
		for (int i = 0; i < brick.ShapeArray.GetLength(0); i++)
		{
			for (int j = 0; j < brick.ShapeArray.GetLength(1); j++)
			{
				int num = brick.ShapeArray[i, j];
				if (num > 0 && num < 16)
				{
					_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 5, squareColor, transformation, BlendModeEnum.Normal);
				}
				transformation.Position.X += 14f;
			}
			transformation.Position.X = position.X;
			transformation.Position.Y += 14f;
		}
	}

	private bool CheckBrickAtPosition(TetrixBrick brick, int posX, int posY)
	{
		for (int i = 0; i < brick.ShapeArray.GetLength(0); i++)
		{
			if (i + posY < 0 || i + posY >= _pit.GetLength(0))
			{
				return false;
			}
			for (int j = 0; j < brick.ShapeArray.GetLength(1); j++)
			{
				if (j + posX < 0 || j + posX >= _pit.GetLength(1) || (_pit[i + posY, j + posX] >= 0 && brick.ShapeArray[i, j] > 0))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void CreatePreviewBrick()
	{
		_previewBrick = new TetrixBrick();
		if (Utils.GetRandom(30) == 0 && CurrentLevelNumber > 2)
		{
			_previewBrick.SetType(TetrixBrick.TetrixBrickTypeEnum.goro);
		}
		else
		{
			_previewBrick.SetType(Utils.GetRandomItem(_brickOffer));
		}
	}

	private void StartNewBrick()
	{
		if (_tetrixState != TetrixStateEnum.GameOver)
		{
			StopFallSound();
			if (_previewBrick == null)
			{
				CreatePreviewBrick();
			}
			_currentBrick = _previewBrick;
			CreatePreviewBrick();
			_brickPosX = 4;
			_brickPosY = 0;
			_fallDelay = GetFallDelay();
			_fallTimer = _fallDelay;
			_brickPauseTimer = 0f;
			_levelBrickCount++;
			_isComboDone = false;
			_wasComboUsed = false;
			_pitQuakeTimer = Vector2.Zero;
			_pitQuakeOffset = Vector2.Zero;
			_comboKeyList.Clear();
			_wasActionKeyReleased = false;
			if (!CheckBrickAtPosition(_currentBrick, _brickPosX, _brickPosY))
			{
				SetState(TetrixStateEnum.GameOver);
				PlaceBrick();
			}
		}
	}

	private void DrawComboEffectPit()
	{
		Transformation transformation = Transformation.CreateByPosition(_pitTopLeft);
		for (int i = _comboEffectBeginRow; i <= _comboEffectEndRow && i < _effectPit.GetLength(0); i++)
		{
			transformation.Position.Y = _pitTopLeft.Y + (float)(i * 14);
			for (int j = 0; j < _effectPit.GetLength(1); j++)
			{
				transformation.Position.X = _pitTopLeft.X + (float)(j * 14);
				_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 5, _effectPit[i, j], transformation, _comboEffectBlendMode);
			}
		}
		if (_comboEffectMode != 10)
		{
			return;
		}
		for (int k = 0; k <= _comboEffectBeginRow; k++)
		{
			transformation.Position.Y = _pitTopLeft.Y + (float)(k * 14);
			for (int l = 0; l < _effectPit.GetLength(1); l++)
			{
				if (_effectPit[k, l] == Color.White)
				{
					transformation.Position.X = _pitTopLeft.X + (float)(l * 14);
					_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 5, _effectPit[k, l], transformation, _comboEffectBlendMode);
				}
			}
		}
	}

	private void DrawGamePit()
	{
		Transformation transformation = Transformation.CreateByPosition(_pitTopLeft);
		for (int i = 0; i < _pit.GetLength(0); i++)
		{
			for (int j = 0; j < _pit.GetLength(1); j++)
			{
				if (_pit[i, j] >= 0)
				{
					if (_pit[i, j] == 99)
					{
						Color white = Color.White;
						if (_tetrixState == TetrixStateEnum.GameOver || _tetrixState == TetrixStateEnum.GamePaused)
						{
							white.A = 100;
						}
						_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 10, white, transformation, BlendModeEnum.Normal);
					}
					else
					{
						Color white = GetSquareColor(_pit[i, j]);
						if (_tetrixState == TetrixStateEnum.GameOver || _tetrixState == TetrixStateEnum.GamePaused)
						{
							white.A = 100;
						}
						_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 5, white, transformation, BlendModeEnum.Normal);
					}
					if (_rowBlinkTimerArray[i] > 0f)
					{
						_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 5, new Color(1f, 1f, 1f, (float)(int)_atarix.Color.A / 255f * Utils.Sin(_rowBlinkTimerArray[i] * 50f)), transformation, BlendModeEnum.Additive);
					}
				}
				transformation.Position.X += 14f;
			}
			transformation.Position.X = _pitTopLeft.X;
			transformation.Position.Y += 14f;
		}
	}

	private void DrawGameFrame()
	{
		Color color = _atarix.Color;
		Transformation transformation = new Transformation(_atarix);
		transformation.Position.X = _atarix.Position.X + 107f;
		transformation.Position.Y = _atarix.Position.Y - 13f;
		_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, transformation, BlendModeEnum.Normal);
		transformation.Position.X = _atarix.Position.X - 210f;
		transformation.Position.Y = _atarix.Position.Y - 13f;
		_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, transformation, BlendModeEnum.Normal);
		transformation.Position.X = _atarix.Position.X - 93f;
		transformation.Position.Y = _atarix.Position.Y + 205f;
		_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 2, color, transformation, BlendModeEnum.Normal);
	}

	private void DrawChangingLevelEffect()
	{
		if (_changingLevelPhase != 1 && _changingLevelPhase != 2)
		{
			return;
		}
		Transformation transformation = Transformation.CreateByPosition(_pitTopLeft);
		for (int i = 0; i <= _changingLevelRow; i++)
		{
			transformation.Position.Y = _pitTopLeft.Y + (float)(i * 14);
			for (int num = _pit.GetUpperBound(1); num >= 0; num--)
			{
				transformation.Position.X = _pitTopLeft.X + (float)(num * 14);
				_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 5, new Color(1f, 1f, 1f, 1f), transformation, BlendModeEnum.Normal);
			}
		}
	}

	public override void Draw()
	{
		if (_tetrixState == TetrixStateEnum.Menu)
		{
			DrawMenu();
			return;
		}
		DrawGameInfoPanel();
		DrawGamePit();
		DrawBrick(_currentBrick, _pitTopLeft + new Vector2(_brickPosX * 14, _brickPosY * 14), inPit: true);
		switch (_tetrixState)
		{
		case TetrixStateEnum.ChangingLevel:
			DrawChangingLevelEffect();
			break;
		case TetrixStateEnum.ComboEffect:
			DrawComboEffectPit();
			break;
		case TetrixStateEnum.GameOver:
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("tetrix_game_over"), _pitTopLeft.X + 86f, _pitTopLeft.Y + 201f, new Vector2(0.8f), new Color(0, 0, 0), TextAlign.Center, 500, _shopMenu.ShowHidePercentage);
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("tetrix_game_over"), _pitTopLeft.X + 84f, _pitTopLeft.Y + 199f, new Vector2(0.8f), new Color(255, 255, 255), TextAlign.Center, 500, _shopMenu.ShowHidePercentage);
			break;
		case TetrixStateEnum.GamePaused:
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("tetrix_game_paused"), _pitTopLeft.X + 86f, _pitTopLeft.Y + 201f, new Vector2(0.8f), new Color(0, 0, 0), TextAlign.Center, 500, _shopMenu.ShowHidePercentage);
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("tetrix_game_paused"), _pitTopLeft.X + 84f, _pitTopLeft.Y + 199f, new Vector2(0.8f), new Color(255, 255, 255), TextAlign.Center, 500, _shopMenu.ShowHidePercentage);
			break;
		}
		DrawGameFrame();
	}

	public void LoadLevels(string fileName)
	{
		using (IniFile.OpenFromContent(fileName))
		{
		}
	}
}
