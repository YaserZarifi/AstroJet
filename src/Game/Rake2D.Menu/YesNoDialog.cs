using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class YesNoDialog : PopupDialog
{
	public delegate void OnEventHandler();

	private Button _buttonYes;

	private Button _buttonNo;

	private string _textId;

	private string _yesNoQuestionText;

	public event OnEventHandler OnYesButtonPressed;

	public event OnEventHandler OnNoButtonPressed;

	public YesNoDialog(ShopMenu shopMenu, string textId)
		: base(shopMenu)
	{
		_textId = textId;
		Init();
	}

	public override void Init()
	{
		base.Init();
		_atlasBgr = _shopMenu.AtlasYesNoDialog;
		_buttonYes = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonYes.Position = new Vector2(-_buttonYes.Width / 2, 110f);
		_buttonYes.ParentTransformation = this;
		_buttonYes.OnClick += OnClickButtonYes;
		_buttonYes.HasPressEffectEnabled = true;
		_buttonNo = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonNo.Position = new Vector2(_buttonNo.Width / 2, 110f);
		_buttonNo.ParentTransformation = this;
		_buttonNo.OnClick += OnClickButtonNo;
		_buttonNo.HasPressEffectEnabled = true;
	}

	public void SetTexts()
	{
		_buttonYes.Caption = TextBundle.GetText("game_menu_button_yes");
		_buttonNo.Caption = TextBundle.GetText("game_menu_button_no");
		if (_textId != null)
		{
			_yesNoQuestionText = TextBundle.GetText(_textId);
		}
	}

	private void OnClickButtonYes(Button button)
	{
		Hide();
		OnYesButtonPressed?.Invoke();
	}

	private void OnClickButtonNo(Button button)
	{
		Hide();
		OnNoButtonPressed?.Invoke();
	}

	private void ParseInput()
	{
		if (_shopMenu.KeyPressed(ShopMenuKeys.CancelAll) || _shopMenu.KeyPressed(ShopMenuKeys.CancelSelection) || GameInput.MousePressed(GameInput.MouseButton.Right) || GameInput.SystemGPButtonPressedAll(Buttons.B))
		{
			_buttonNo.Click();
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft, bOnlyOnce: false))
		{
			if (_buttonNo.IsSelected)
			{
				_buttonNo.Unselect();
				_buttonYes.Select();
			}
			else if (_buttonYes.IsSelected)
			{
				_buttonYes.Unselect();
				_buttonNo.Select();
			}
			else
			{
				_buttonNo.Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight, bOnlyOnce: false))
		{
			if (_buttonNo.IsSelected)
			{
				_buttonNo.Unselect();
				_buttonYes.Select();
			}
			else if (_buttonYes.IsSelected)
			{
				_buttonYes.Unselect();
				_buttonNo.Select();
			}
			else
			{
				_buttonYes.Select();
			}
		}
	}

	public override void Show()
	{
		base.Show();
		SetTexts();
		_shopMenu.Cursor.UpdatePosition();
		_shopMenu.Cursor.IsPositionChanged = false;
		_shopMenu.ShopInput.CancelKeyPressed();
		_buttonYes.CancelPressEffect();
		_buttonNo.CancelPressEffect();
		Position.X = 960f;
		Position.Y = 540f;
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			_buttonYes.SetUnselected();
		}
		else
		{
			_buttonYes.SetSelected();
		}
		_buttonNo.SetUnselected();
		if (GameJnG.PopupMenuOptions.State != ShopMenuState.Hidden)
		{
			State = ShopMenuState.Visible;
		}
		GameCore.PlaySound("menu_yesno_show");
	}

	public override void Hide()
	{
		base.Hide();
		GameCore.PlaySound("menu_yesno_hide");
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (!GameCore.Console.IsActive)
		{
			ParseInput();
		}
		_buttonYes.Update(elapsedTime);
		_buttonNo.Update(elapsedTime);
	}

	public override void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		base.Draw();
		if (_yesNoQuestionText != null)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, _yesNoQuestionText, WorldPosition.X, WorldPosition.Y - 80f, ShopMenu.FONT_SCALE_PRICE_TV, null, TextAlign.Center, 500, (float)(int)Color.A / 255f, 40f);
		}
		_buttonYes.Alpha = Color.A;
		_buttonNo.Alpha = _buttonYes.Alpha;
		_buttonYes.Draw();
		_buttonNo.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
