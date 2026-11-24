using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class NewGameDialog : PopupDialog
{
	private SpriteAtlas _atlasTV;

	private SpriteAtlas _atlasPlayerCount;

	private TVNoise _tvNoise;

	private Button _buttonNext;

	private Button _buttonBack;

	private SwitchButton _buttonPlayerCount;

	private SwitchButton _buttonGameMode;

	private NewGameWizard _wizard;

	private MainMenu _mainMenu;

	private Vector2 _tvPosition;

	public NewGameDialog(ShopMenu shopMenu, MainMenu mainMenu, NewGameWizard wizard)
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
		_atlasTV = GameCore.GetSpriteAtlas("profile_face_tv", logWhenNotFound: true);
		_atlasPlayerCount = GameCore.GetSpriteAtlas("tv_player_count", logWhenNotFound: true);
		_tvNoise = new TVNoise(_shopMenu, TVNoiseTypeEnum.PlayerFace);
		_tvNoise.ParentTransformation = this;
		_tvNoise.Position = new Vector2(-168f, -59f);
		_tvNoise.StripOffsetYRange = new FloatRange(87f, -87f);
		_tvNoise.Init();
		_tvNoise.IsAutoHiding = false;
		_tvNoise.FadeSpeed = 1500f;
		_buttonNext = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonNext.Position = new Vector2(_buttonNext.Width / 2, 110f);
		_buttonNext.ParentTransformation = this;
		_buttonNext.OnClick += _wizard.OnOKButtonPressed;
		_buttonBack = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonBack.Position = new Vector2(-_buttonBack.Width / 2, 110f);
		_buttonBack.ParentTransformation = this;
		_buttonBack.OnClick += _wizard.OnCancelButtonPressed;
		_buttonPlayerCount = new SwitchButton(_shopMenu);
		_buttonPlayerCount.IsClickable = true;
		_buttonPlayerCount.OnClick += OnPlayerCountButtonClick;
		_buttonPlayerCount.ParentTransformation = this;
		_buttonPlayerCount.Position = new Vector2(70f, -80f);
		_buttonPlayerCount.Type = ControlsButtonTypeEnum.Big;
		_buttonPlayerCount.LabelOffset = new Vector2(-120f, -46f);
		_buttonPlayerCount.TextAlign = TextAlign.Left;
		_buttonGameMode = new SwitchButton(_shopMenu);
		_buttonGameMode.IsClickable = true;
		_buttonGameMode.OnClick += OnGameModeButtonClick;
		_buttonGameMode.ParentTransformation = this;
		_buttonGameMode.Position = new Vector2(0f, 10f);
		_buttonGameMode.Type = ControlsButtonTypeEnum.Big;
		_buttonGameMode.LabelOffset = new Vector2(-120f, -46f);
		_buttonGameMode.TextAlign = TextAlign.Left;
	}

	private void OnGameModeButtonClick(SwitchButton btn)
	{
		switch (_wizard.GameMode)
		{
		case GameModeEnum.Casual:
			_wizard.GameMode = GameModeEnum.Standard;
			break;
		case GameModeEnum.Standard:
			_wizard.GameMode = GameModeEnum.HardCore;
			break;
		default:
			_wizard.GameMode = GameModeEnum.Casual;
			break;
		}
		SetGameModeButtonText();
	}

	private void OnPlayerCountButtonClick(SwitchButton btn)
	{
		_wizard.PlayerCount = ((_wizard.PlayerCount != 1) ? 1 : 2);
		SetPlayerCountButtonText();
	}

	private void SetGameModeButtonText()
	{
		_buttonGameMode.Text = TextBundle.GetGameModeText(_wizard.GameMode);
	}

	private void SetPlayerCountButtonText()
	{
		_buttonPlayerCount.Text = TextBundle.GetText($"button_player_count_{_wizard.PlayerCount}");
	}

	public void SetTexts()
	{
		_buttonNext.Caption = TextBundle.GetText("button_next");
		_buttonBack.Caption = TextBundle.GetText("button_back");
		_buttonPlayerCount.Label = TextBundle.GetText("button_player_count");
		_buttonGameMode.Label = TextBundle.GetText("button_game_mode");
		SetGameModeButtonText();
		SetPlayerCountButtonText();
	}

	public override void Show()
	{
		base.Show();
		SetTexts();
		_buttonBack.SetUnselected();
		if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
		{
			_buttonNext.Select();
		}
		else
		{
			_buttonNext.SetUnselected();
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
		}
		else
		{
			if (_buttonGameMode.IsActivated)
			{
				return;
			}
			if (!_buttonNext.IsSelected && !_buttonBack.IsSelected && !_buttonGameMode.IsSelected && !_buttonPlayerCount.IsSelected && (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.KeyPressed(ShopMenuKeys.MoveDown) || _shopMenu.KeyPressed(ShopMenuKeys.MoveLeft) || _shopMenu.KeyPressed(ShopMenuKeys.MoveRight)))
			{
				_buttonNext.Select();
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
			{
				if (_buttonNext.IsSelected || _buttonBack.IsSelected)
				{
					_buttonNext.Unselect();
					_buttonBack.Unselect();
					_buttonGameMode.Select();
				}
				else if (_buttonGameMode.IsSelected)
				{
					_buttonGameMode.Unselect();
					_buttonPlayerCount.Select();
				}
				else if (_buttonPlayerCount.IsSelected)
				{
					_buttonPlayerCount.Unselect();
					_buttonNext.Select();
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
			{
				if (_buttonNext.IsSelected || _buttonBack.IsSelected)
				{
					_buttonNext.Unselect();
					_buttonBack.Unselect();
					_buttonPlayerCount.Select();
				}
				else if (_buttonGameMode.IsSelected)
				{
					_buttonGameMode.Unselect();
					_buttonNext.Select();
				}
				else if (_buttonPlayerCount.IsSelected)
				{
					_buttonPlayerCount.Unselect();
					_buttonGameMode.Select();
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
			{
				if (_buttonNext.IsSelected)
				{
					_buttonNext.Unselect();
					_buttonBack.Select();
				}
				else if (_buttonBack.IsSelected)
				{
					_buttonNext.Select();
					_buttonBack.Unselect();
				}
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				if (_buttonBack.IsSelected)
				{
					_buttonBack.Unselect();
					_buttonNext.Select();
				}
				else if (_buttonNext.IsSelected)
				{
					_buttonNext.Unselect();
					_buttonBack.Select();
				}
			}
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		ParseInput();
		_tvPosition = new Vector2(Position.X - 161f, Position.Y - 50f);
		_tvNoise.Update(elapsedTime);
		_buttonPlayerCount.Position = new Vector2(70f, -65f);
		_buttonGameMode.Position = new Vector2(70f, 15f);
		_buttonNext.IsInputEnabled = true;
		_buttonBack.IsInputEnabled = true;
		_buttonPlayerCount.Update(elapsedTime);
		_buttonGameMode.Update(elapsedTime);
		_buttonNext.Update(elapsedTime);
		_buttonBack.Update(elapsedTime);
	}

	private void DrawPlayerCountTV()
	{
		_shopMenu.SpriteBatchEnd(bLight: true);
		Transformation transformation = Transformation.CreateByPosition(_tvPosition);
		_shopMenu.TriangleBatch.Begin(BlendState.AlphaBlend, GameCore.ViewPortMatrix);
		_shopMenu.TriangleBatch.DrawRect(transformation.Position.X - 62f, transformation.Position.Y - 87f, 125f, 176f, Color.FromNonPremultiplied(0, 0, 0, Color.A));
		_shopMenu.TriangleBatch.End();
		_shopMenu.SpriteBatchBegin(bLight: true);
		Color color = new Color(255, 255, 255, (int)Color.A);
		_atlasPlayerCount.DrawFrame(_shopMenu.SpriteBatchLight, _wizard.PlayerCount - 1, color, transformation);
		_tvNoise.Draw();
		_atlasTV.DrawFrame(_shopMenu.SpriteBatchLight, 0, color, transformation);
		_shopMenu.DrawGameModeSkull(_wizard.GameMode, transformation, color.A);
	}

	public override void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		base.Draw();
		_buttonPlayerCount.Draw();
		_buttonGameMode.Draw();
		_buttonNext.Alpha = Color.A;
		_buttonBack.Alpha = _buttonNext.Alpha;
		_buttonNext.Draw();
		_buttonBack.Draw();
		DrawPlayerCountTV();
		string text = TextBundle.GetText($"game_mode_{_wizard.GameMode.ToString().ToLowerLazy()}_desc");
		_shopMenu.FontDefault.WrapWrite(_shopMenu.SpriteBatchLight, text, 1300f, 440f, 500, ShopMenu.FONT_SCALE_LABEL, null, (float)(int)Color.A / 255f, 27);
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
