using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Menu;

public class NewPlayerDialog : PopupDialog
{
	public string HeroName;

	private Button _buttonOK;

	private Button _buttonBack;

	private PanelFaceSelector _faceSelector;

	private TextField _playerNameTextField;

	private bool _swappingFace;

	private NewGameWizard _wizard;

	private MainMenu _mainMenu;

	private int _swapToFace;

	private FaceTV _faceTV;

	public int FaceNumber
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

	public bool IsKeyInputFree => !_playerNameTextField.IsEdited;

	public string PlayerName => _playerNameTextField.Text;

	public NewPlayerDialog(ShopMenu shopMenu, MainMenu mainMenu, NewGameWizard wizard)
		: base(shopMenu)
	{
		_wizard = wizard;
		_mainMenu = mainMenu;
		Init();
	}

	public override void Init()
	{
		base.Init();
		_atlasBgr = _shopMenu.AtlasYesNoDialog;
		_buttonOK = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonOK.Position = new Vector2(_buttonOK.Width / 2, 110f);
		_buttonOK.ParentTransformation = this;
		_buttonOK.OnClick += _wizard.OnOKButtonPressed;
		_buttonBack = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonBack.Position = new Vector2(-_buttonBack.Width / 2, 110f);
		_buttonBack.ParentTransformation = this;
		_buttonBack.OnClick += _wizard.OnCancelButtonPressed;
		_playerNameTextField = new TextField(_shopMenu);
		_playerNameTextField.ParentTransformation = this;
		_playerNameTextField.ColorMode = ColorMode.ParentAlpha;
		_playerNameTextField.Position = new Vector2(70f, -65f);
		_faceSelector = new PanelFaceSelector(_shopMenu);
		_faceSelector.OnFaceChanged += OnFaceChanged;
		_faceSelector.OnHide += OnFaceSelectorHide;
		_faceTV = new FaceTV(_shopMenu);
		_faceTV.ParentTransformation = this;
		_faceTV.CheckCheater = false;
	}

	private void OnFaceChanged(int faceNumber)
	{
		SwapHeroFace(faceNumber);
	}

	private void OnFaceSelectorHide(PanelFaceSelector faceSelector)
	{
		SelectFaceTV(bPlaySound: false);
	}

	public void SetTexts()
	{
		if (_wizard.PlayerCount == 1 || _wizard.State == NewGameWizard.WizardStateEnum.NewPlayerDialog_2)
		{
			_buttonOK.Caption = TextBundle.GetText("button_new_profile_create");
		}
		else
		{
			_buttonOK.Caption = TextBundle.GetText("button_next");
		}
		_buttonBack.Caption = TextBundle.GetText("button_back");
		if (_wizard.PlayerCount > 1)
		{
			_playerNameTextField.Label = string.Format("{0} {1}:", TextBundle.GetText("hero_stat_player_name"), (_wizard.State == NewGameWizard.WizardStateEnum.NewPlayerDialog_1) ? 1 : 2);
		}
		else
		{
			_playerNameTextField.Label = string.Format("{0}:", TextBundle.GetText("hero_stat_player_name"));
		}
	}

	public override void Show()
	{
		base.Show();
		SetTexts();
		_playerNameTextField.Unselect();
		_buttonBack.SetUnselected();
		if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
		{
			_buttonOK.Select();
		}
		else
		{
			_buttonOK.SetUnselected();
		}
		Position.X = 960f;
		Position.Y = 540f;
		GameCore.PlaySound("menu_yesno_show");
	}

	private void ParseInput()
	{
		if (!GameCore.Console.IsActive && GameInput.SystemKeyPressed(Keys.Escape))
		{
			_buttonBack.Click();
			return;
		}
		if (IsFaceTVSelected && _shopMenu.KeyPressed(ShopMenuKeys.Select))
		{
			if (_faceSelector.State == ShopMenuState.Visible || _faceSelector.State == ShopMenuState.Showing)
			{
				_faceSelector.Hide();
			}
			else
			{
				_faceSelector.StartPos = Position + new Vector2(150f, -55f);
				_faceSelector.EndPos = Position + new Vector2(330f, -55f);
				_faceSelector.Show(FaceNumber, 0f);
				IsFaceTVSelected = false;
			}
		}
		if (_playerNameTextField.IsEdited)
		{
			return;
		}
		if (!IsFaceTVSelected && !_buttonOK.IsSelected && !_buttonBack.IsSelected && !_playerNameTextField.IsSelected && (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.KeyPressed(ShopMenuKeys.MoveDown) || _shopMenu.KeyPressed(ShopMenuKeys.MoveLeft) || _shopMenu.KeyPressed(ShopMenuKeys.MoveRight)))
		{
			_buttonOK.Select();
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			if (_buttonOK.IsSelected || _buttonBack.IsSelected)
			{
				_buttonOK.Unselect();
				_buttonBack.Unselect();
				_playerNameTextField.Select();
			}
			else if (_playerNameTextField.IsSelected)
			{
				_playerNameTextField.Unselect();
				_buttonOK.Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			if (IsFaceTVSelected)
			{
				IsFaceTVSelected = false;
				_buttonOK.Select();
			}
			else if (_buttonOK.IsSelected || _buttonBack.IsSelected)
			{
				_buttonOK.Unselect();
				_buttonBack.Unselect();
				_playerNameTextField.Select();
			}
			else if (_playerNameTextField.IsSelected)
			{
				_playerNameTextField.Unselect();
				_buttonOK.Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
		{
			if (IsFaceTVSelected)
			{
				_playerNameTextField.Select();
				IsFaceTVSelected = false;
			}
			else if (_playerNameTextField.IsSelected)
			{
				_playerNameTextField.Unselect();
				IsFaceTVSelected = true;
			}
			else if (_buttonOK.IsSelected)
			{
				_buttonOK.Unselect();
				_buttonBack.Select();
			}
			else if (_buttonBack.IsSelected)
			{
				_buttonOK.Select();
				_buttonBack.Unselect();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
		{
			if (_playerNameTextField.IsSelected)
			{
				_playerNameTextField.Unselect();
				SelectFaceTV();
			}
			else if (IsFaceTVSelected)
			{
				_playerNameTextField.Select();
				IsFaceTVSelected = false;
			}
			else if (_buttonBack.IsSelected)
			{
				_buttonBack.Unselect();
				_buttonOK.Select();
			}
			else if (_buttonOK.IsSelected)
			{
				_buttonOK.Unselect();
				_buttonBack.Select();
			}
		}
	}

	public void SwapHeroFace(int faceNumber)
	{
		_faceTV.ShowNoise();
		_swappingFace = true;
		_swapToFace = faceNumber;
	}

	public void OnSwapHero()
	{
		_faceTV.HideNoise();
		FaceNumber = _swapToFace;
		_swappingFace = false;
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
	}

	private void UpdateFaceTV(float elapsedTime)
	{
		_faceTV.Position = new Vector2(-161f, -50f);
		_faceTV.GameMode = _wizard.GameMode;
		if (!_shopMenu.IsAnyPopupMenuVisible && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			if (!_playerNameTextField.IsEdited && _shopMenu.Cursor.IsInRect(_faceTV.WorldPosition.X - 65f, _faceTV.WorldPosition.Y - 90f, 130, 180))
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

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (_playerNameTextField.IsEdited)
		{
			_buttonOK.IsInputEnabled = false;
			_buttonBack.IsInputEnabled = false;
			_playerNameTextField.Update(elapsedTime);
		}
		else if (_faceSelector.State == ShopMenuState.Hidden)
		{
			ParseInput();
			_buttonOK.IsInputEnabled = true;
			_buttonBack.IsInputEnabled = true;
			_playerNameTextField.Text = HeroName;
			_playerNameTextField.Update(elapsedTime);
		}
		else
		{
			_buttonOK.IsInputEnabled = false;
			_buttonBack.IsInputEnabled = false;
		}
		_buttonOK.Update(elapsedTime);
		_buttonBack.Update(elapsedTime);
		HeroName = _playerNameTextField.Text;
		_faceSelector.Update(elapsedTime);
		UpdateFaceTV(elapsedTime);
	}

	public override void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		base.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
		_faceTV.Color = new Color(255, 255, 255, (int)Color.A);
		_faceTV.Draw();
		_shopMenu.SpriteBatchBegin(bLight: true);
		_playerNameTextField.Draw();
		_buttonOK.Alpha = Color.A;
		_buttonBack.Alpha = _buttonOK.Alpha;
		_buttonOK.Draw();
		_buttonBack.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
		_faceSelector.Draw();
	}
}
