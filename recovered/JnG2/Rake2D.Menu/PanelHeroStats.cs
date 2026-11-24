using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class PanelHeroStats : ShopMenuPanel
{
	public const int MAX_MEDAL_INDEX = 29;

	public int HighlitedMedalIndex;

	public int SelectedMedalIndex;

	private TextField _heroNameTextField;

	private SwitchButton _buttonDifficulty;

	private SwitchButton _buttonLastLevel;

	private SpriteAtlas _atlasMedal;

	private SpriteAtlas _atlasInsignia;

	public SpriteAtlas AtlasBadge;

	private bool _swappingFace;

	private FaceTV _faceTV;

	public bool IsFaceTVSelected
	{
		get
		{
			return _faceTV.IsSelected;
		}
		set
		{
			_faceTV.IsSelected = value;
		}
	}

	public bool IsHeroNameEdited => _heroNameTextField.IsEdited;

	private int _faceNumber
	{
		get
		{
			return _faceTV.FaceNumber;
		}
		set
		{
			_faceTV.FaceNumber = value;
		}
	}

	public bool IsAnyItemHighlited
	{
		get
		{
			if (HighlitedMedalIndex < 0 && !IsFaceTVSelected && !_heroNameTextField.IsSelected)
			{
				return _heroNameTextField.IsEdited;
			}
			return true;
		}
	}

	private bool IsMedalSelectedAndReady
	{
		get
		{
			if (!IsFaceTVSelected && !_heroNameTextField.IsSelected && !_heroNameTextField.IsEdited && SelectedMedalIndex >= 0 && SelectedMedalIndex <= 29 && _shopMenu.PanelShipParams.CursorSliderIndex < 0 && _shopMenu.BigShipSlotSet.CursorSlot == null && _shopMenu.PanelTopButtons.SelectedButtonIndex < 0 && _shopMenu.PanelFaceSelector.State == ShopMenuState.Hidden)
			{
				return !_shopMenu.ButtonTestRange.IsSelected;
			}
			return false;
		}
	}

	public PanelHeroStats(ShopMenu shopMenu)
		: base(shopMenu)
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
		Position = new Vector2(-400f, 0f);
		AtlasBadge = GameCore.GetSpriteAtlas("profile_badge", logWhenNotFound: true);
		_atlasFrame = GameCore.GetSpriteAtlas("shop_panel_cargo", logWhenNotFound: true);
		_atlasBackground = GameCore.GetSpriteAtlas("shop_panel_cargo_bgr", logWhenNotFound: true);
		_atlasMedal = GameCore.GetSpriteAtlas("profile_medal", logWhenNotFound: true);
		_atlasInsignia = GameCore.GetSpriteAtlas("profile_rank_insignia", logWhenNotFound: true);
		SelectedMedalIndex = -1;
		HighlitedMedalIndex = -1;
		_faceTV = new FaceTV(_shopMenu);
		_faceTV.ParentTransformation = this;
		_heroNameTextField = new TextField(_shopMenu);
		_heroNameTextField.ParentTransformation = this;
		_heroNameTextField.Position = new Vector2(154f, 385f);
		_buttonDifficulty = new SwitchButton(_shopMenu);
		_buttonDifficulty.Type = ControlsButtonTypeEnum.Big;
		_buttonDifficulty.TextAlign = TextAlign.Left;
		_buttonDifficulty.TextScale = _heroNameTextField.TextScale;
		_buttonDifficulty.IsClickable = false;
		_buttonDifficulty.IsSelectable = false;
		_buttonDifficulty.OnClick += OnDifficultyButtonClick;
		_buttonDifficulty.LabelOffset = new Vector2(-_buttonDifficulty.WIDTH / 2 + 2, -48f);
		_buttonDifficulty.ParentTransformation = this;
		_buttonDifficulty.Position = new Vector2(154f, _heroNameTextField.Position.Y + 75f);
		_buttonLastLevel = new SwitchButton(_shopMenu);
		_buttonLastLevel.Type = ControlsButtonTypeEnum.Big;
		_buttonLastLevel.TextAlign = TextAlign.Left;
		_buttonLastLevel.TextScale = _heroNameTextField.TextScale;
		_buttonLastLevel.IsClickable = false;
		_buttonLastLevel.IsSelectable = false;
		_buttonLastLevel.OnClick += OnDifficultyButtonClick;
		_buttonLastLevel.LabelOffset = new Vector2(-_buttonLastLevel.WIDTH / 2 + 2, -48f);
		_buttonLastLevel.ParentTransformation = this;
		_buttonLastLevel.Position = new Vector2(154f, _buttonDifficulty.Position.Y + 75f);
	}

	public void SetTexts()
	{
		_buttonDifficulty.Label = $"{TextBundle.GetDifficultyLabel(GameJnG.GameMode)}";
		_buttonLastLevel.Label = string.Format("{0}", TextBundle.GetText("hero_stat_stage"));
	}

	public void OnSwapHero()
	{
		_faceTV.HideNoise();
		_faceNumber = _shopMenu.MyHero.FaceNumber;
		_swappingFace = false;
	}

	private void OnDifficultyButtonClick(SwitchButton button)
	{
	}

	public void SwapHeroFace(bool bAutoSwap)
	{
		_faceTV.ShowNoise();
		_swappingFace = bAutoSwap;
	}

	public override void Show(float showDelay)
	{
		base.Show(showDelay);
		TargetPosition = new Vector2(0f, 0f);
		_heroNameTextField.Unselect();
		_faceNumber = _shopMenu.MyHero.FaceNumber;
		SetTexts();
		TotalStats.Recalculate();
	}

	public override void Hide()
	{
		base.Hide();
		TargetPosition = new Vector2(-400f, 0f);
		SelectMedal(-1);
	}

	private void ParseInput()
	{
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRightWithControl) && IsAnyItemHighlited)
		{
			_shopMenu.CancelCursorSelection();
			_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.front_gun);
		}
		else if (IsFaceTVSelected && _shopMenu.KeyPressed(ShopMenuKeys.Select))
		{
			if (_shopMenu.PanelFaceSelector.State == ShopMenuState.Visible || _shopMenu.PanelFaceSelector.State == ShopMenuState.Showing)
			{
				_shopMenu.PanelFaceSelector.Hide();
			}
			else
			{
				_shopMenu.PanelFaceSelector.StartPos = new Vector2(465f, 210f);
				_shopMenu.PanelFaceSelector.EndPos = new Vector2(960f, 210f);
				_shopMenu.PanelFaceSelector.Show(_shopMenu.MyHero.FaceNumber, 0f);
				_shopMenu.PanelFaceSelector.CursorFaceIndex = _shopMenu.MyHero.FaceNumber;
			}
		}
		if (!_heroNameTextField.IsEdited && (HasFocus || SelectedMedalIndex >= 0) && _shopMenu.PanelTopButtons.SelectedButtonIndex < 0)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
			{
				if (IsFaceTVSelected || _heroNameTextField.IsSelected)
				{
					CancelCursorSelection();
					_shopMenu.SelectTopButton(0);
				}
				else if ((HighlitedMedalIndex - 5) % 6 == 0)
				{
					HighliteMedal(-1);
					_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.rear_gun);
				}
				else if (HighlitedMedalIndex >= 0 && HighlitedMedalIndex < 29)
				{
					HighliteMedal(HighlitedMedalIndex + 1);
				}
				else if (HighlitedMedalIndex == -1 && SelectedMedalIndex >= 0 && SelectedMedalIndex < 28 && IsMedalSelectedAndReady)
				{
					HighliteMedal(SelectedMedalIndex + 1);
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				if (HighlitedMedalIndex % 6 != 0 && HighlitedMedalIndex > 0)
				{
					HighliteMedal(HighlitedMedalIndex - 1);
				}
				else if (HighlitedMedalIndex == -1 && SelectedMedalIndex > 1 && IsMedalSelectedAndReady)
				{
					HighliteMedal(SelectedMedalIndex - 1);
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
			{
				if (_heroNameTextField.IsSelected)
				{
					CancelCursorSelection();
					SelectFaceTV();
				}
				else if (HighlitedMedalIndex >= 0 && HighlitedMedalIndex < 6)
				{
					if (_shopMenu.CurrentScreen != ShopScreenEnum.Mia)
					{
						CancelCursorSelection();
						_heroNameTextField.Select();
					}
				}
				else if (HighlitedMedalIndex >= 6)
				{
					HighliteMedal(HighlitedMedalIndex - 6);
				}
				else if (SelectedMedalIndex >= 6 && IsMedalSelectedAndReady)
				{
					HighliteMedal(SelectedMedalIndex - 6);
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
			{
				if (IsFaceTVSelected)
				{
					CancelCursorSelection();
					_heroNameTextField.Select();
				}
				else if (_heroNameTextField.IsSelected)
				{
					CancelCursorSelection();
					HighliteMedal(0);
				}
				else if (HighlitedMedalIndex >= 0 && HighlitedMedalIndex <= 23)
				{
					HighliteMedal(HighlitedMedalIndex + 6);
				}
				else if (HighlitedMedalIndex < 0 && IsMedalSelectedAndReady)
				{
					HighliteMedal(SelectedMedalIndex + 6);
				}
			}
		}
		if (_shopMenu.PanelShipParams.CursorSliderIndex < 0 && _shopMenu.PanelShipParams.ActivatedSliderIndex < 0 && (_shopMenu.KeyPressed(ShopMenuKeys.CancelSelection) || _shopMenu.KeyPressed(ShopMenuKeys.CancelAll, bOnlyOnce: false)))
		{
			if (SelectedMedalIndex >= 0)
			{
				GameCore.PlaySound("menu_item_tv_hide");
			}
			_shopMenu.CancelSelection();
		}
	}

	private void UpdateFaceTV(float elapsedTime)
	{
		_faceTV.ResetPrecalculations();
		_faceTV.Position = new Vector2(Position.X + 158f, Position.Y + 225f);
		_faceTV.GameMode = GameJnG.GameMode;
		if (!_shopMenu.IsAnyPopupMenuVisible && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && _shopMenu.CurrentScreen != ShopScreenEnum.Mia)
		{
			if (_shopMenu.Cursor.IsInRect(_faceTV.WorldPosition.X - 65f, _faceTV.WorldPosition.Y - 90f, 130, 180))
			{
				SelectFaceTV();
			}
			else
			{
				IsFaceTVSelected = false;
			}
		}
		_faceTV.Update(elapsedTime);
		if (_swappingFace && _faceTV.IsNoiseVisible)
		{
			OnSwapHero();
		}
	}

	private void UpdateMedals(float elapsedTime)
	{
		bool flag = false;
		if (!_shopMenu.IsAnyPopupMenuVisible && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			for (int i = 0; i < 30; i++)
			{
				Vector2 medalPosition = GetMedalPosition(i);
				if (_shopMenu.Cursor.IsInRect(medalPosition.X - 20f, medalPosition.Y - 10f, 40, 20))
				{
					HighliteMedal(i);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				HighlitedMedalIndex = -1;
			}
		}
		if (HighlitedMedalIndex >= 0 && _shopMenu.KeyPressed(ShopMenuKeys.Select))
		{
			SelectMedal(HighlitedMedalIndex);
		}
	}

	public void SelectFaceTV(bool bPlaySound = true)
	{
		if (!IsFaceTVSelected)
		{
			IsFaceTVSelected = true;
			if (bPlaySound)
			{
				GameCore.PlaySound("menu_button_select");
			}
		}
		_shopMenu.ChangeFocus(this);
	}

	public void SelectMedal(int index)
	{
		if (index >= 0)
		{
			_shopMenu.SetSelectedItem(null);
			_shopMenu.ChangeFocus(this);
		}
		SelectedMedalIndex = index;
		_shopMenu.PanelInfo.SetPreviewMedal(index);
	}

	public void HighliteMedal(int index)
	{
		if (HighlitedMedalIndex != index)
		{
			HighlitedMedalIndex = index;
			if (index >= 0)
			{
				GameCore.PlaySound("menu_item_select");
			}
		}
		if (index >= 0)
		{
			_shopMenu.ChangeFocus(this);
		}
	}

	private Vector2 GetMedalPosition(int index)
	{
		int num = index / 6;
		int num2 = index - num * 6;
		return new Vector2(Position.X + 52f + (float)(num2 * 40), _buttonLastLevel.WorldPosition.Y + 44f + (float)(num * 19));
	}

	public override void Update(float elapsedTime)
	{
		if (State != ShopMenuState.Hidden)
		{
			base.Update(elapsedTime);
			UpdateFaceTV(elapsedTime);
			_heroNameTextField.IsInputEnabled = _shopMenu.CurrentScreen != ShopScreenEnum.Mia;
			_heroNameTextField.Text = _shopMenu.MyHero.PlayerName;
			_heroNameTextField.Update(elapsedTime);
			_heroNameTextField.Label = TextBundle.GetText($"hero_rank_{_shopMenu.MyHero.GetRank()}");
			_buttonDifficulty.Update(elapsedTime);
			_buttonDifficulty.Text = $"\\1{GameJnG.Difficulty} - {TextBundle.GetDifficultyName(GameJnG.Difficulty)}";
			_buttonLastLevel.Update(elapsedTime);
			StageData stageData = GameJnG.Campaign.GetStageData(GameJnG.StageNumber);
			string arg = ((stageData == null) ? "XYZ" : TextBundle.GetText($"level_name_{stageData.LevelDataList[0].TextId ?? stageData.LevelDataList[0].Id}"));
			_buttonLastLevel.Text = $"\\1{GameJnG.StageNumber} - {arg}";
			_shopMenu.MyHero.PlayerName = _heroNameTextField.Text;
			UpdateMedals(elapsedTime);
			if (!_shopMenu.IsAnyPopupMenuVisible)
			{
				ParseInput();
			}
			if (IsFaceTVSelected || HighlitedMedalIndex >= 0)
			{
				_shopMenu.CursorType = CursorTypeEnum.Select;
			}
		}
	}

	public override void CancelCursorSelection()
	{
		base.CancelCursorSelection();
		IsFaceTVSelected = false;
		_heroNameTextField.Unselect();
		HighlitedMedalIndex = -1;
	}

	public void CancelSelection()
	{
		SelectMedal(-1);
	}

	private void DrawMedals()
	{
		Transformation transformation = new Transformation();
		for (int i = 0; i < 30; i++)
		{
			transformation.Position = GetMedalPosition(i);
			_atlasMedal.DrawFrame(_shopMenu.SpriteBatchLight, (TotalStats.MedalArray != null && TotalStats.MedalArray[i] > 0) ? i : (i + 30), Color.White, transformation);
			if (i == SelectedMedalIndex)
			{
				_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchLight, 6, Color.White, transformation, BlendModeEnum.Additive);
			}
			if (i == HighlitedMedalIndex)
			{
				_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchLight, 9, Color.White, transformation, BlendModeEnum.Additive);
			}
		}
	}

	private void DrawWheels()
	{
		Transformation transformation = new Transformation(this);
		transformation.Position.X += 332f;
		transformation.Position.Y += 333f;
		_shopMenu.AtlasWheel.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, transformation, BlendModeEnum.Normal);
	}

	private void DrawStats()
	{
		Vector2 vector = new Vector2(Position.X + 35f, Position.Y + 672f);
		float x = vector.X + 140f;
		string text = TextBundle.GetText("hero_stat_score") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = ((TotalStats.Score <= 999999) ? Utils.FormatMoney(TotalStats.Score) : Utils.FormatBigNumber(TotalStats.Score, keepSmallDecimal: false));
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_play_time") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = TimeSpan.FromSeconds(GameJnG.PlayTime).ToString("hh':'mm':'ss");
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_death_count") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = Utils.FormatMoney(_shopMenu.MyHero.StatDeathCount);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_levels_finished") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = Utils.FormatMoney(TotalStats.LevelWinCount);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_enemy_kill_count") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = Utils.FormatMoney(TotalStats.KillCountGroundEnemy + TotalStats.KillCountAirEnemy);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_boss_kill_count") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = Utils.FormatMoney(TotalStats.KillCountBoss);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_man_kill_count") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = Utils.FormatMoney(TotalStats.KillCountMan);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_dmg_causing") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = ((!(TotalStats.DamageCaused > 999999.0)) ? Utils.FormatMoney((long)TotalStats.DamageCaused) : Utils.FormatBigNumber(TotalStats.DamageCaused, keepSmallDecimal: false));
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_dmg_absorbed") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = ((!(TotalStats.HeroDamageAbsorbed > 999999f)) ? Utils.FormatMoney((long)TotalStats.HeroDamageAbsorbed) : Utils.FormatBigNumber(TotalStats.HeroDamageAbsorbed, keepSmallDecimal: false));
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_fired_shots") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = ((TotalStats.FiredShots <= 999999) ? Utils.FormatMoney(TotalStats.FiredShots) : Utils.FormatBigNumber(TotalStats.FiredShots, keepSmallDecimal: false));
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_bonus_count") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = $"{Utils.FormatMoney(TotalStats.BonusTakenCount)}";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_find_secret_count") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = $"{Utils.FormatMoney(TotalStats.FindSecretCount)} / {Utils.FormatMoney(TotalStats.MaxFindSecretCount)}";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
		text = TextBundle.GetText("hero_stat_find_hitchhiker_count") + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 130);
		text = $"{Utils.FormatMoney(TotalStats.FindHitchhikerCount)} / {Utils.FormatMoney(TotalStats.MaxFindHitchhikerCount)}";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, x, vector.Y, ShopMenu.FONT_SCALE_LABEL, ShopMenu.FONT_COLOR_PARAM_VALUE, TextAlign.Left, 180);
		vector.Y += 30f;
	}

	public void DrawRankInsignia()
	{
		Transformation transformation = Transformation.CreateByPosition(_faceTV.Position);
		transformation.Position.X -= 112f;
		transformation.Position.Y += 10f;
		int rank = _shopMenu.MyHero.GetRank();
		_atlasInsignia.DrawFrame(_shopMenu.SpriteBatchLight, rank, Color.White, transformation);
		transformation.Position.X += 218f;
		_atlasInsignia.DrawFrame(_shopMenu.SpriteBatchLight, rank, Color.White, transformation);
	}

	public override void Draw()
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		_shopMenu.SpriteBatchBegin(bLight: true);
		_atlasBackground.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, this);
		_heroNameTextField.Draw();
		_buttonDifficulty.Draw();
		_buttonLastLevel.Draw();
		DrawMedals();
		DrawStats();
		DrawWheels();
		_spriteRenderer.SetAtlas(_atlasFrame);
		for (int i = 0; i <= 4; i++)
		{
			if (i == 4)
			{
				_spriteRenderer.Frame = 5;
			}
			else
			{
				_spriteRenderer.Frame = i;
			}
			_spriteRenderer.Draw(this, _shopMenu.SpriteBatchLight, BlendModeEnum.Normal);
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
		_faceTV.Draw();
		_shopMenu.SpriteBatchBegin(bLight: true);
		DrawRankInsignia();
		if (GameJnG.Mia)
		{
			GameCore.FontNoBorder.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("hero_profile_mia"), Position.X + 270f, Position.Y + 352f - 8.5f * Options.FontScale, ShopMenu.FONT_SCALE_LABEL, new Color(255, 0, 0, 255), TextAlign.Right, 130);
		}
		Transformation transformation = new Transformation(this);
		transformation.Position += new Vector2(158f, 111f);
		_shopMenu.AtlasPanelLabel.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, transformation);
		GameCore.FontNoBorder.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("shop_panel_label_pilot"), transformation.Position.X, transformation.Position.Y - 8.5f * Options.FontScale, ShopMenu.FONT_SCALE_LABEL_SMALL, new Color(45, 30, 20, 240), TextAlign.Center, 130);
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
