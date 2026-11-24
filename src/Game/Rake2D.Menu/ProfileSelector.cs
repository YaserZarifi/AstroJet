using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class ProfileSelector
{
	public ProfileSelectorStateEnum State;

	public int EditedProfile;

	public SpriteAtlas AtlasFrame;

	private ShopMenu _shopMenu;

	private MainMenu _mainMenu;

	private List<SaveGameInfo> _saveGameInfoList;

	private float _alphaFactor;

	private float _alphaTimer;

	private Button _buttonBack;

	private int _selectedProfile;

	private YesNoDialog _yesNoDeleteProfile;

	private PartData _profilePartData;

	private SaveGameInfo _saveGameInfoToDelete;

	private const int _maxVisibleProfiles = 5;

	private const int PROFILE_WIDTH = 384;

	private const int PROFILE_HEIGHT = 250;

	public bool IsAnyPopupVisible
	{
		get
		{
			if (GameJnG.MainMenu.NewGameWizard.State == NewGameWizard.WizardStateEnum.Hidden)
			{
				return _yesNoDeleteProfile.State != ShopMenuState.Hidden;
			}
			return true;
		}
	}

	public ProfileSelector(ShopMenu shopMenu, MainMenu mainMenu)
	{
		_shopMenu = shopMenu;
		_mainMenu = mainMenu;
		_yesNoDeleteProfile = new YesNoDialog(_shopMenu, "yes_no_delete_profile");
		_yesNoDeleteProfile.OnYesButtonPressed += OnDeleteProfilePressed;
		AtlasFrame = GameCore.GetSpriteAtlas("profile_slot");
		_buttonBack = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonBack.OnClick += OnButtonBackClick;
		_profilePartData = GameCore.GetPartData("main_menu_hero_profile");
		SetTexts();
	}

	public void SetTexts()
	{
		_buttonBack.Caption = TextBundle.GetText("button_back");
	}

	private void OnDeleteProfilePressed()
	{
		if (EditedProfile >= 0 && EditedProfile < _saveGameInfoList.Count)
		{
			if (_saveGameInfoToDelete != null)
			{
				DeleteProfile(_saveGameInfoToDelete);
			}
			SaveGameInfo saveGameInfo = _saveGameInfoList[EditedProfile];
			saveGameInfo.MyObject.ChangePartState("change_to_empty");
			saveGameInfo.MyObject.OnSpineAnimCompleted += ProfileOnSpineAnimCompleted;
			_saveGameInfoToDelete = saveGameInfo;
		}
	}

	private void DeleteProfile(SaveGameInfo sgi)
	{
		SaveGame.Delete(sgi.Number);
		if (GameJnG.SaveGameNumber == sgi.Number)
		{
			if (_mainMenu.MyHeroArray != null)
			{
				Hero[] myHeroArray = _mainMenu.MyHeroArray;
				for (int i = 0; i < myHeroArray.Length; i++)
				{
					myHeroArray[i].DestroySmallShip();
				}
			}
			GameJnG.SaveGameNumber = 0;
			Options.SaveGameNumber = 0;
		}
		sgi.Clear();
	}

	private void ProfileOnSpineAnimCompleted(int trackNumber, int loopCount)
	{
		DeleteProfile(_saveGameInfoToDelete);
		_saveGameInfoToDelete.MyObject.OnSpineAnimCompleted -= ProfileOnSpineAnimCompleted;
		_saveGameInfoToDelete.Hero = null;
		_saveGameInfoToDelete = null;
	}

	private void OnButtonBackClick(Button button)
	{
		if (GameJnG.SaveGameNumber > 0)
		{
			_mainMenu.SwitchToSaveGameInfo = GetSaveGameInfo(GameJnG.SaveGameNumber - 1);
		}
		Hide();
		GameCore.PlaySound("menu_yesno_hide");
	}

	private void SetState(ProfileSelectorStateEnum state)
	{
		State = state;
	}

	public void Show()
	{
		_alphaFactor = 0f;
		_alphaTimer = 0.5f;
		SetState(ProfileSelectorStateEnum.FadeWait);
		_buttonBack.SetUnselected();
		ReloadSavegameList();
		GameCore.PlaySound("menu_yesno_show");
	}

	private void Clear()
	{
		if (_saveGameInfoList == null)
		{
			return;
		}
		foreach (SaveGameInfo saveGameInfo in _saveGameInfoList)
		{
			saveGameInfo?.Destroy();
		}
	}

	public void ReloadSavegameList()
	{
		Clear();
		_saveGameInfoList = DataUtils.GetSaveGameInfoList();
		for (int i = 0; i < _saveGameInfoList.Count; i++)
		{
			SaveGameInfo saveGameInfo = _saveGameInfoList[i];
			if (saveGameInfo != null)
			{
				saveGameInfo.MyObject = GameObject.CreateByPartDataSimple(_profilePartData, bCreateRigidBody: false);
				saveGameInfo.MyObject.SetLayer(_mainMenu.Level.MainLayer);
				if (saveGameInfo.Hero != null)
				{
					saveGameInfo.MyObject.ChangePartState("full");
					saveGameInfo.MyObject.SpineRenderer.SetTrackTime(0, (float)i * 0.4f);
				}
				else
				{
					saveGameInfo.MyObject.ChangePartState("empty");
				}
				saveGameInfo.MyObject.Activate();
				GameObject childByIdFlat = saveGameInfo.MyObject.GetChildByIdFlat("face_2");
				GameObject childByIdFlat2;
				GameObject childByIdDeep;
				if (saveGameInfo.Hero2 != null)
				{
					childByIdFlat2 = saveGameInfo.MyObject.GetChildByIdFlat("face_1");
					childByIdDeep = saveGameInfo.MyObject.GetChildByIdDeep("ship_1");
				}
				else
				{
					childByIdFlat2 = saveGameInfo.MyObject.GetChildByIdFlat("face");
					childByIdDeep = saveGameInfo.MyObject.GetChildByIdDeep("ship");
				}
				if (childByIdDeep != null && saveGameInfo.Hero != null)
				{
					saveGameInfo.Hero.CreateSmallShip(_mainMenu.Level.MainLayer, Vector2.Zero);
					saveGameInfo.Hero.DestroyRigidBody();
					childByIdDeep.AddChild(saveGameInfo.Hero.MyObject);
					childByIdFlat2.Frame = saveGameInfo.Hero.FaceNumber;
					childByIdFlat2.IsVisible = true;
				}
				if (saveGameInfo.Hero2 != null)
				{
					childByIdDeep = saveGameInfo.MyObject.GetChildByIdDeep("ship_2");
					saveGameInfo.Hero2.CreateSmallShip(_mainMenu.Level.MainLayer, Vector2.Zero);
					saveGameInfo.Hero2.DestroyRigidBody();
					childByIdDeep.AddChild(saveGameInfo.Hero2.MyObject);
					childByIdFlat.Frame = saveGameInfo.Hero2.FaceNumber;
					childByIdFlat.IsVisible = true;
				}
			}
		}
	}

	public void Hide()
	{
		if (_saveGameInfoToDelete != null)
		{
			DeleteProfile(_saveGameInfoToDelete);
			_saveGameInfoToDelete.MyObject.OnSpineAnimCompleted -= ProfileOnSpineAnimCompleted;
			_saveGameInfoToDelete = null;
		}
		SetState(ProfileSelectorStateEnum.FadeOut);
	}

	private void SelectProfile(int number)
	{
		if (_selectedProfile != number)
		{
			_selectedProfile = number;
			GameCore.PlaySound("menu_item_select");
		}
	}

	private void ParseInput()
	{
		if (_shopMenu.KeyPressed(ShopMenuKeys.CancelAll))
		{
			Hide();
			GameCore.PlaySound("menu_yesno_hide");
		}
		if (!_buttonBack.IsSelected)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown) || _shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
			{
				if (_selectedProfile < 0)
				{
					SelectProfile(2);
				}
				else
				{
					_selectedProfile = -1;
					_buttonBack.Select();
				}
			}
			if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				if (_selectedProfile <= 0)
				{
					SelectProfile(4);
				}
				else
				{
					SelectProfile(_selectedProfile - 1);
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
			{
				if (_selectedProfile < 0 || _selectedProfile >= 4)
				{
					SelectProfile(0);
				}
				else
				{
					SelectProfile(_selectedProfile + 1);
				}
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			SelectProfile(2);
			_buttonBack.Unselect();
		}
		if (_saveGameInfoToDelete != null || _selectedProfile < 0)
		{
			return;
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.Select))
		{
			SaveGameInfo saveGameInfo = GetSaveGameInfo(_selectedProfile);
			if (saveGameInfo.Hero == null)
			{
				EditedProfile = _selectedProfile;
				GameJnG.MainMenu.NewGameWizard.Show();
				return;
			}
			_mainMenu.SwitchToSaveGameInfo = saveGameInfo;
			GameJnG.SaveGameNumber = saveGameInfo.Number;
			Hide();
			GameCore.PlaySound("menu_yesno_hide");
		}
		else if (_saveGameInfoList[_selectedProfile] != null && _saveGameInfoList[_selectedProfile].Hero != null && (_shopMenu.KeyPressed(ShopMenuKeys.Action) || GameInput.SystemKeyPressed(Keys.Delete)))
		{
			EditedProfile = _selectedProfile;
			GameInput.CancelMousePressed();
			_yesNoDeleteProfile.Show();
		}
	}

	public void Update(float elapsedTime)
	{
		if (!IsAnyPopupVisible)
		{
			ParseInput();
		}
		switch (State)
		{
		case ProfileSelectorStateEnum.FadeWait:
			if (Utils.ChangeValueToTarget(ref _alphaTimer, 0f, elapsedTime))
			{
				SetState(ProfileSelectorStateEnum.FadeIn);
			}
			break;
		case ProfileSelectorStateEnum.FadeIn:
			if (Utils.ChangeValueToTarget(ref _alphaFactor, 1f, 4f * elapsedTime))
			{
				SetState(ProfileSelectorStateEnum.Visible);
			}
			break;
		case ProfileSelectorStateEnum.FadeOut:
			if (Utils.ChangeValueToTarget(ref _alphaFactor, 0f, 4f * elapsedTime))
			{
				SetState(ProfileSelectorStateEnum.Hidden);
				Clear();
				GameJnG.IsFirstLevelTry = GameJnG.GetIsFirstLevelTry();
				_mainMenu.SetTexts();
				if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
				{
					_mainMenu.SelectButton(0, bPlaySound: false);
				}
			}
			break;
		}
		bool flag = false;
		for (int i = 0; i < 5; i++)
		{
			Vector2 profilePos = GetProfilePos(i);
			SaveGameInfo saveGameInfo = GetSaveGameInfo(i);
			if (saveGameInfo != null && saveGameInfo.MyObject != null)
			{
				saveGameInfo.MyObject.SetPosition(profilePos + new Vector2(192f, 125f) + saveGameInfo.MyObject.MyCamera.Position);
				GameCore.UseShopLightMaps = false;
				saveGameInfo.MyObject.Alpha = (byte)(_alphaFactor * 255f);
				saveGameInfo.MyObject.Update(elapsedTime);
				GameCore.UseShopLightMaps = true;
			}
			if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && GameJnG.MainMenu.NewGameWizard.State == NewGameWizard.WizardStateEnum.Hidden && _yesNoDeleteProfile.State == ShopMenuState.Hidden && _shopMenu.Cursor.IsInRect(profilePos, 384, 250))
			{
				SelectProfile(i);
				flag = true;
				_shopMenu.CursorType = CursorTypeEnum.Select;
			}
		}
		if (IsAnyPopupVisible)
		{
			_selectedProfile = EditedProfile;
		}
		else if (!flag && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			_selectedProfile = -1;
		}
		_buttonBack.IsInputEnabled = !IsAnyPopupVisible;
		_buttonBack.Position = new Vector2(960f, 970f);
		_buttonBack.Alpha = (byte)(255f * _alphaFactor);
		_buttonBack.Update(elapsedTime);
		if (_buttonBack.IsSelected)
		{
			GameJnG.MainMenu.SetHintText(TextBundle.GetText("hint_main_back"));
		}
		else if (_selectedProfile >= 0 && _selectedProfile < _saveGameInfoList.Count && !IsAnyPopupVisible)
		{
			if (_saveGameInfoList[_selectedProfile].Hero != null)
			{
				GameJnG.MainMenu.SetHintText(TextBundle.GetControlText("hint_main_select_full_profile", _shopMenu.CurrentController));
			}
			else
			{
				GameJnG.MainMenu.SetHintText(TextBundle.GetControlText("hint_main_select_empty_profile", _shopMenu.CurrentController));
			}
		}
		if (_yesNoDeleteProfile.State != ShopMenuState.Hidden)
		{
			_yesNoDeleteProfile.Update(elapsedTime);
		}
	}

	private Vector2 GetProfilePos(int profileNumber)
	{
		return new Vector2(profileNumber * 384, 640f);
	}

	public SaveGameInfo GetSaveGameInfo(int profileNumber)
	{
		if (profileNumber >= 0 && profileNumber < _saveGameInfoList.Count)
		{
			return _saveGameInfoList[profileNumber];
		}
		return null;
	}

	public void Draw()
	{
		_ = _saveGameInfoList.Count;
		_shopMenu.SpriteBatchBegin(bLight: true);
		_shopMenu.SpriteBatchBegin(bLight: false);
		Vector2 fONT_SCALE_LABEL = ShopMenu.FONT_SCALE_LABEL;
		float num = 140f;
		float num2 = 133f;
		for (int i = 0; i < 5; i++)
		{
			Vector2 profilePos = GetProfilePos(i);
			SaveGameInfo saveGameInfo = GetSaveGameInfo(i);
			Transformation transformation;
			if (saveGameInfo != null)
			{
				if (saveGameInfo.MyObject != null)
				{
					GameCore.EffectLight.SetLightMaps(bUseShopMaps: false);
					GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: false);
					GameCore.EffectLight.SetLightFactor(_mainMenu.Level.MainLayer, 1f);
					GameCore.EffectLightBasic.SetLightFactor(_mainMenu.Level.MainLayer, 1f);
					GameCore.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, GameCore.RasterizerState, GameCore.GetRenderEffect(_mainMenu.Level.MainLayer), GameCore.LevelMatrix);
					saveGameInfo.MyObject.Draw(GameCore.SpriteBatch);
					GameCore.SpriteBatch.End();
					GameCore.EffectLight.SetLightMaps(bUseShopMaps: true);
					GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: true);
				}
				float num3 = 61f;
				if (saveGameInfo.Hero != null && saveGameInfo.Hero.MyObject != null)
				{
					if (saveGameInfo.Hero2 != null)
					{
						_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0} 1: \\3{1}", TextBundle.GetText("hero_stat_player_name"), saveGameInfo.Hero.PlayerName), profilePos.X + num, profilePos.Y + num2, fONT_SCALE_LABEL, null, TextAlign.Left, 284, _alphaFactor);
						_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0} 2: \\3{1}", TextBundle.GetText("hero_stat_player_name"), saveGameInfo.Hero2.PlayerName), profilePos.X + num, profilePos.Y + num2 + 30f, fONT_SCALE_LABEL, null, TextAlign.Left, 284, _alphaFactor);
					}
					else
					{
						_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1}", TextBundle.GetText("hero_stat_player_name"), saveGameInfo.Hero.PlayerName), profilePos.X + num, profilePos.Y + num2, fONT_SCALE_LABEL, null, TextAlign.Left, 284, _alphaFactor);
					}
					_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, $"\\0{TextBundle.GetGameModeText(saveGameInfo.GameMode)}: \\3{saveGameInfo.Difficulty} - {TextBundle.GetDifficultyName(saveGameInfo.Difficulty)}", profilePos.X + num, profilePos.Y + num2 + num3, fONT_SCALE_LABEL, null, TextAlign.Left, 364, _alphaFactor);
					_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1}", TextBundle.GetText("hero_stat_score"), Utils.FormatMoney(saveGameInfo.Score)), profilePos.X + num, profilePos.Y + num2 + num3 + 31f, fONT_SCALE_LABEL, null, TextAlign.Left, 364, _alphaFactor);
				}
				else
				{
					transformation = Transformation.CreateByPosition(profilePos + new Vector2(192f, 58f));
					_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\1<{0}>", TextBundle.GetText("hero_profile_new_game")), profilePos.X + num + 70f, profilePos.Y + num2 + 45f, fONT_SCALE_LABEL, null, TextAlign.Center, 364, _alphaFactor);
				}
			}
			if (saveGameInfo.Mia)
			{
				transformation = Transformation.CreateByPosition(profilePos + new Vector2(192f, 95f));
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("{0}", TextBundle.GetText("hero_profile_mia")), transformation.Position.X, transformation.Position.Y - 9.5f * Options.FontScale, fONT_SCALE_LABEL, new Color(255, 0, 0), TextAlign.Center, 0, _alphaFactor, -1f, BlendModeEnum.Additive);
				AtlasFrame.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, new Color(255f, 255f, 255f, _alphaFactor), transformation, BlendModeEnum.Additive);
			}
			transformation = Transformation.CreateByPosition(profilePos + new Vector2(192f, 181f));
			if (_mainMenu.MyHeroArray != null && GameJnG.SaveGameNumber == i + 1)
			{
				Color color = ((!saveGameInfo.Mia) ? Color.FromNonPremultiplied(80, 160, 30, (int)(255f * _alphaFactor)) : new Color(255f, 0f, 0f, _alphaFactor));
				AtlasFrame.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, transformation, BlendModeEnum.Additive);
			}
			if (_selectedProfile == i)
			{
				Color color = ((!saveGameInfo.Mia) ? new Color(255, 255, 255, (int)(255f * _alphaFactor)) : Color.FromNonPremultiplied(255, 50, 50, (int)(255f * _alphaFactor)));
				AtlasFrame.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, transformation, BlendModeEnum.Additive);
			}
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
		_shopMenu.SpriteBatchEnd(bLight: false);
		_shopMenu.SpriteBatchBegin(bLight: true);
		_buttonBack.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
		if (_yesNoDeleteProfile.State != ShopMenuState.Hidden)
		{
			_shopMenu.GameCore.DrawDarkBackground((float)(int)_yesNoDeleteProfile.Color.A / 400f);
			_yesNoDeleteProfile.Draw();
		}
	}
}
