using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class WelcomeScreen : Transformation
{
	public enum StateEnum
	{
		Hidden,
		Showing,
		Visible,
		Hiding
	}

	public StateEnum State;

	private ShopMenu _shopMenu;

	private SpriteRenderer _bgrRenderer;

	private float _showPercentage;

	private Button _buttonContinue;

	private bool _isInitialized;

	public WelcomeScreen(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		State = StateEnum.Hidden;
		_bgrRenderer = GameCore.Allocator.GetSpriteRenderer(GameCore.GetSpriteAtlas("welcome_bgr"));
		_buttonContinue = new Button(_shopMenu, ShopButtonType.Big);
		_buttonContinue.Position = new Vector2(960f, 940f);
		_buttonContinue.OnClick += OnButtonContinueClick;
	}

	private void OnButtonContinueClick(Button btn)
	{
		Hide();
	}

	public void SetTexts()
	{
		_buttonContinue.Caption = TextBundle.GetText("button_continue");
	}

	public void Show()
	{
		SetTexts();
		State = StateEnum.Showing;
		_showPercentage = 0f;
		_isInitialized = false;
	}

	public void Hide()
	{
		if (State == StateEnum.Showing || State == StateEnum.Visible)
		{
			State = StateEnum.Hiding;
		}
	}

	public void Update(float elapsedTime)
	{
		switch (State)
		{
		case StateEnum.Showing:
			if (Utils.ChangeValueToTarget(ref _showPercentage, 1f, 5f * elapsedTime))
			{
				State = StateEnum.Visible;
			}
			break;
		case StateEnum.Hiding:
			if (Utils.ChangeValueToTarget(ref _showPercentage, 0f, 5f * elapsedTime))
			{
				State = StateEnum.Hidden;
				Options.WasWelcomeScreenReaded = true;
			}
			break;
		}
		if (!_isInitialized)
		{
			_buttonContinue.SetSelected();
			_shopMenu.CurrentController = GameInput.ControllerTypeEnum.Keyboard;
			_isInitialized = true;
		}
		else
		{
			_buttonContinue.Update(elapsedTime);
			if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveLeft) || _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveDown) || _shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveRight))
			{
				_buttonContinue.Select();
			}
		}
		if ((State == StateEnum.Showing || State == StateEnum.Visible) && _shopMenu.KeyPressed(ShopMenuKeys.CancelAll))
		{
			Hide();
		}
		_buttonContinue.Alpha = (byte)(255f * _showPercentage);
	}

	public void Draw()
	{
		_bgrRenderer.Color = new Color(Color.White, _showPercentage);
		_bgrRenderer.Draw(this, _shopMenu.SpriteBatchNoLight, BlendModeEnum.Normal);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("welcome_text"), 480f, 250f, new Vector2(0.8f), null, TextAlign.Left, 1000, _showPercentage);
		_buttonContinue.Draw(_shopMenu.SpriteBatchNoLight);
	}
}
