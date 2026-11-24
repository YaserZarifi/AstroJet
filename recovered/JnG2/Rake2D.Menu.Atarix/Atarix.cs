using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;

namespace Rake2D.Menu.Atarix;

public class Atarix : Transformation
{
	public delegate void OnButtonHandler();

	public static readonly Vector2 FONT_SCALE_DEFAULT = new Vector2(0.6f, 0.55f);

	public AtarixStateEnum State;

	public SpriteAtlas Atlas;

	private ShopMenu _shopMenu;

	public float HideAlphaFactor;

	private List<AtarixButton> _buttonList;

	private bool _wasGateOpened;

	public AtarixContent CurrentContent { get; private set; }

	public AtarixMainMenu ContentMainMenu { get; private set; }

	public AtarixTetrix ContentTetrix { get; private set; }

	public AtarixTron ContentTron { get; private set; }

	public AtarixButton ButtonUp { get; private set; }

	public AtarixButton ButtonDown { get; private set; }

	public AtarixButton ButtonLeft { get; private set; }

	public AtarixButton ButtonRight { get; private set; }

	public AtarixButton ButtonAction { get; private set; }

	public AtarixButton ButtonQuit { get; private set; }

	public event OnButtonHandler OnLeftButtonClick;

	public event OnButtonHandler OnRightButtonClick;

	public event OnButtonHandler OnUpButtonClick;

	public event OnButtonHandler OnDownButtonClick;

	public event OnButtonHandler OnActionButtonClick;

	public event OnButtonHandler OnOffButtonClick;

	public Atarix(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		Atlas = GameCore.GetSpriteAtlas("atarix_main");
		AtarixButton.InitStatic();
		_buttonList = new List<AtarixButton>();
		ButtonUp = new AtarixButton(this, _shopMenu);
		ButtonUp.Frame = 0;
		ButtonUp.Position = new Vector2(-108f, 224f);
		ButtonUp.OnClick += OnButtonClick;
		_buttonList.Add(ButtonUp);
		ButtonDown = new AtarixButton(this, _shopMenu);
		ButtonDown.Frame = 1;
		ButtonDown.Position = new Vector2(-107f, 284f);
		ButtonDown.OnClick += OnButtonClick;
		_buttonList.Add(ButtonDown);
		ButtonLeft = new AtarixButton(this, _shopMenu);
		ButtonLeft.Frame = 2;
		ButtonLeft.Position = new Vector2(-137f, 253f);
		ButtonLeft.OnClick += OnButtonClick;
		_buttonList.Add(ButtonLeft);
		ButtonRight = new AtarixButton(this, _shopMenu);
		ButtonRight.Frame = 3;
		ButtonRight.Position = new Vector2(-77f, 253f);
		ButtonRight.OnClick += OnButtonClick;
		_buttonList.Add(ButtonRight);
		ButtonAction = new AtarixButton(this, _shopMenu);
		ButtonAction.Frame = 4;
		ButtonAction.Position = new Vector2(100f, 245f);
		ButtonAction.OnClick += OnButtonClick;
		_buttonList.Add(ButtonAction);
		ButtonQuit = new AtarixButton(this, _shopMenu);
		ButtonQuit.Frame = 5;
		ButtonQuit.Position = new Vector2(243f, 245f);
		ButtonQuit.OnClick += OnButtonClick;
		_buttonList.Add(ButtonQuit);
		ContentMainMenu = new AtarixMainMenu(this, _shopMenu);
		ContentTetrix = new AtarixTetrix(this, _shopMenu);
		ContentTron = new AtarixTron(this, _shopMenu);
	}

	public void SetTexts()
	{
		ContentMainMenu.SetTexts();
		ContentTetrix.SetTexts();
		ContentTron.SetTexts();
	}

	private void OnButtonClick(AtarixButton button)
	{
		if (button == ButtonLeft)
		{
			OnLeftButtonClick?.Invoke();
		}
		else if (button == ButtonRight)
		{
			OnRightButtonClick?.Invoke();
		}
		else if (button == ButtonUp)
		{
			OnUpButtonClick?.Invoke();
		}
		else if (button == ButtonDown)
		{
			OnDownButtonClick?.Invoke();
		}
		else if (button == ButtonAction)
		{
			OnActionButtonClick?.Invoke();
		}
		else if (button == ButtonQuit)
		{
			if (CurrentContent == ContentMainMenu)
			{
				Hide();
			}
			OnOffButtonClick?.Invoke();
		}
	}

	public void SetState(AtarixStateEnum state)
	{
		State = state;
	}

	public void SetContent(AtarixContent content)
	{
		OnUpButtonClick = null;
		OnDownButtonClick = null;
		OnLeftButtonClick = null;
		OnRightButtonClick = null;
		OnActionButtonClick = null;
		OnOffButtonClick = null;
		CurrentContent = content;
		CurrentContent.RegisterEvents();
		CurrentContent.Show();
	}

	private void Show()
	{
		if (State == AtarixStateEnum.Hidden)
		{
			_wasGateOpened = _shopMenu.ShopBackground.GateState == GateStateEnum.Opened || _shopMenu.ShopBackground.GateState == GateStateEnum.Opening;
			if (_wasGateOpened)
			{
				_shopMenu.ShopBackground.ToggleGate();
			}
			SetState(AtarixStateEnum.Showing);
		}
	}

	public void ShowToMainMenu()
	{
		SetContent(ContentMainMenu);
		Show();
	}

	public void ShowToHackMenu()
	{
		SetContent(ContentTron);
		GameCore.PlaySound("tron_show");
		Show();
	}

	public void Hide()
	{
		SetState(AtarixStateEnum.Hidding);
		if (_wasGateOpened && (_shopMenu.ShopBackground.GateState == GateStateEnum.Closed || _shopMenu.ShopBackground.GateState == GateStateEnum.Closing))
		{
			_shopMenu.ShopBackground.ToggleGate();
		}
		GameCore.PlaySound("atarix_hide");
	}

	public void UpdateButtons(float elapsedTime)
	{
		ButtonUp.Position = new Vector2(-86f, 280f);
		ButtonDown.Position = new Vector2(-85f, 340f);
		ButtonLeft.Position = new Vector2(-115f, 309f);
		ButtonRight.Position = new Vector2(-55f, 309f);
		ButtonAction.Position = new Vector2(122f, 303f);
		ButtonQuit.Position = new Vector2(265f, 303f);
		foreach (AtarixButton button in _buttonList)
		{
			button.Update(elapsedTime);
			button.IsSelected = false;
		}
		foreach (AtarixButton button2 in _buttonList)
		{
			if (button2.IsUnderCursor)
			{
				button2.IsSelected = true;
				_shopMenu.CursorType = ((button2 != ButtonQuit) ? CursorTypeEnum.Select : CursorTypeEnum.Unselect);
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					button2.Press();
				}
				break;
			}
		}
	}

	private void ParseInput()
	{
		if (!GameJnG.IsAnyPopupMenuVisible && !GameCore.Console.IsActive)
		{
			if (GameInput.SystemKeyPressed(Keys.Escape) || GameInput.SystemGPButtonPressedAll(Buttons.Back) || GameInput.SystemGPButtonPressedAll(Buttons.Start))
			{
				ButtonQuit.Press();
			}
			if (GameInput.SystemKeyPressed(Keys.Up) || GameInput.SystemGPButtonPressedAll(Buttons.DPadUp) || GameInput.SystemKeyPressed(Keys.W))
			{
				ButtonUp.Press();
			}
			if (GameInput.SystemKeyPressed(Keys.Down) || GameInput.SystemGPButtonPressedAll(Buttons.DPadDown) || GameInput.SystemKeyPressed(Keys.S))
			{
				ButtonDown.Press();
			}
			if (GameInput.SystemKeyPressed(Keys.Left) || GameInput.SystemGPButtonPressedAll(Buttons.DPadLeft) || GameInput.SystemKeyPressed(Keys.A))
			{
				ButtonLeft.Press();
			}
			if (GameInput.SystemKeyPressed(Keys.Right) || GameInput.SystemGPButtonPressedAll(Buttons.DPadRight) || GameInput.SystemKeyPressed(Keys.D))
			{
				ButtonRight.Press();
			}
			if (GameInput.SystemKeyPressed(Keys.X) || GameInput.SystemKeyPressed(Keys.Space) || GameInput.SystemKeyPressed(Keys.Enter) || GameInput.SystemGPButtonPressedAll(Buttons.A) || GameInput.SystemGPButtonPressedAll(Buttons.X))
			{
				ButtonAction.Press();
			}
			if (!GameInput.SystemKeyDown(Keys.Up) && !GameInput.SystemKeyDown(Keys.W) && !GameInput.SystemGPButtonDown(Buttons.DPadDown) && GameInput.GetLeftStickPos(0).Y <= 0f)
			{
				ButtonUp.Release();
			}
			if (!GameInput.SystemKeyDown(Keys.Down) && !GameInput.SystemKeyDown(Keys.S) && !GameInput.SystemGPButtonDown(Buttons.DPadUp) && GameInput.GetLeftStickPos(0).Y >= 0f)
			{
				ButtonDown.Release();
			}
			if (!GameInput.SystemKeyDown(Keys.Left) && !GameInput.SystemKeyDown(Keys.A) && !GameInput.SystemGPButtonDown(Buttons.DPadLeft) && GameInput.GetLeftStickPos(0).X <= 0f)
			{
				ButtonLeft.Release();
			}
			if (!GameInput.SystemKeyDown(Keys.Right) && !GameInput.SystemKeyDown(Keys.D) && !GameInput.SystemGPButtonDown(Buttons.DPadRight) && GameInput.GetLeftStickPos(0).X >= 0f)
			{
				ButtonRight.Release();
			}
			if (!GameInput.SystemKeyDown(Keys.X) && !GameInput.SystemKeyDown(Keys.Space) && !GameInput.SystemKeyDown(Keys.Enter) && !GameInput.SystemGPButtonDown(Buttons.A) && !GameInput.SystemGPButtonDown(Buttons.X))
			{
				ButtonAction.Release();
			}
			if (!GameInput.SystemKeyDown(Keys.Escape) && !GameInput.SystemGPButtonDown(Buttons.Back) && !GameInput.SystemGPButtonDown(Buttons.Start))
			{
				ButtonQuit.Release();
			}
		}
	}

	public void Update(float elapsedTime)
	{
		_shopMenu.CursorType = CursorTypeEnum.NoAction;
		ResetPrecalculations();
		switch (State)
		{
		case AtarixStateEnum.Visible:
			ParseInput();
			break;
		case AtarixStateEnum.Hidding:
			if (Utils.ChangeValueToTarget(ref HideAlphaFactor, 0f, 5f * elapsedTime))
			{
				SetState(AtarixStateEnum.Hidden);
			}
			break;
		case AtarixStateEnum.Showing:
			if (Utils.ChangeValueToTarget(ref HideAlphaFactor, 1f, 5f * elapsedTime))
			{
				SetState(AtarixStateEnum.Visible);
			}
			break;
		}
		CurrentContent.Update(elapsedTime);
		Position = new Vector2(960f - (1f - HideAlphaFactor) * 1260f, 440f);
		Alpha = (byte)(255f * HideAlphaFactor);
		UpdateButtons(elapsedTime);
	}

	public void Draw()
	{
		_shopMenu.TriangleBatch.Begin(BlendState.AlphaBlend, GameCore.ViewPortMatrix);
		_shopMenu.TriangleBatch.DrawRect(Position.X - 235f, Position.Y - 240f, 470f, 470f, Color.FromNonPremultiplied(0, 0, 0, Color.A));
		_shopMenu.TriangleBatch.End();
		_shopMenu.SpriteBatchBegin(bLight: false);
		CurrentContent.Draw();
		_shopMenu.SpriteBatchEnd(bLight: false);
		_shopMenu.SpriteBatchBegin(bLight: true);
		Atlas.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color, this, BlendModeEnum.Normal);
		foreach (AtarixButton button in _buttonList)
		{
			button.Draw();
		}
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
