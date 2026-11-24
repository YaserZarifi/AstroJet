using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class PopupMenuFinishCampaign : PopupDialog
{
	private Button _buttonContinue;

	private Button _buttonStay;

	private string _yesNoQuestionText;

	public PopupMenuFinishCampaign(ShopMenu shopMenu)
		: base(shopMenu)
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
		_atlasBgr = _shopMenu.AtlasYesNoDialog;
		_buttonContinue = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonContinue.OnClick += OnContinueButtonClick;
		_buttonContinue.HasPressEffectEnabled = true;
		_buttonContinue.ParentTransformation = this;
		_buttonContinue.Position = new Vector2(-_buttonContinue.Width / 2, 110f);
		_buttonStay = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonStay.OnClick += OnStayButtonClick;
		_buttonStay.HasPressEffectEnabled = true;
		_buttonStay.ParentTransformation = this;
		_buttonStay.Position = new Vector2(_buttonStay.Width / 2, 110f);
	}

	private void OnStayButtonClick(Button button)
	{
		SetState(ShopMenuState.Hidden);
		GameJnG.LoadingScreen.Show(null, fadeToShop: true);
	}

	private void OnContinueButtonClick(Button button)
	{
		GameJnG.FinishCampaign();
		SetState(ShopMenuState.Hidden);
		GameJnG.LoadingScreen.Show(null, fadeToShop: true);
	}

	public void SetTexts()
	{
		_yesNoQuestionText = TextBundle.GetText("yes_no_finish_campaign");
		_buttonStay.Caption = TextBundle.GetText("button_stay");
		_buttonContinue.Caption = TextBundle.GetText("button_continue");
	}

	public override void Show()
	{
		base.Show();
		SetTexts();
		_shopMenu.Cursor.UpdatePosition();
		_shopMenu.Cursor.IsPositionChanged = false;
		_shopMenu.ShopInput.CancelKeyPressed();
		Position.X = 960f;
		Position.Y = 540f;
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			_buttonContinue.SetUnselected();
		}
		else
		{
			_buttonContinue.SetSelected();
		}
		GameCore.SoundSystem.PauseMusic();
		GameCore.PlaySound("menu_yesno_show");
	}

	private void ParseInput()
	{
		if (GameInput.MousePressed(GameInput.MouseButton.Right) || (!GameCore.Console.IsActive && _shopMenu.KeyPressed(ShopMenuKeys.CancelAll)))
		{
			Hide();
			if (!GameCore.IsPaused)
			{
				GameCore.SoundSystem.ResumeMusic();
			}
		}
		else if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveLeft))
		{
			if (_buttonStay.IsSelected)
			{
				_buttonStay.Unselect();
				_buttonContinue.Select();
			}
			else if (_buttonContinue.IsSelected)
			{
				_buttonContinue.Unselect();
				_buttonStay.Select();
			}
			else
			{
				_buttonContinue.Select();
			}
		}
		else if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveRight))
		{
			if (_buttonStay.IsSelected)
			{
				_buttonStay.Unselect();
				_buttonContinue.Select();
			}
			else if (_buttonContinue.IsSelected)
			{
				_buttonContinue.Unselect();
				_buttonStay.Select();
			}
			else
			{
				_buttonContinue.Select();
			}
		}
		else if (!_buttonStay.IsSelected && !_buttonContinue.IsSelected && (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveDown)))
		{
			_buttonContinue.Select();
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		ParseInput();
		_buttonStay.Color = Color;
		_buttonStay.Update(elapsedTime);
		_buttonContinue.Color = Color;
		_buttonContinue.Update(elapsedTime);
	}

	public override void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		base.Draw();
		if (_yesNoQuestionText != null)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, _yesNoQuestionText, WorldPosition.X, WorldPosition.Y - 80f, new Vector2(0.8f), null, TextAlign.Center, 500, (float)(int)Color.A / 255f, 40f);
		}
		_buttonStay.Draw();
		_buttonContinue.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
