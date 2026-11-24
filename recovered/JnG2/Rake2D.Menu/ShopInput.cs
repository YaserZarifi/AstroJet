using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Menu;

public class ShopInput
{
	public ControllerDirectionEnum StickDirection;

	public ControllerDirectionEnum StickDirectionPressed;

	private ControllerDirectionEnum _oldStickDirecion;

	private Vector2 _deadZone;

	private float _stickRepeatTimer;

	private ShopMenu _shopMenu;

	public ShopInput(ShopMenu shopMenu)
	{
		_deadZone = new Vector2(0.5f, 0.5f);
		_shopMenu = shopMenu;
	}

	public void Update(float elapsedTime)
	{
		if (_shopMenu.Cursor.IsPositionChanged || GameInput.MouseDown(GameInput.MouseButton.Left) || GameInput.MouseDown(GameInput.MouseButton.Right))
		{
			_shopMenu.CurrentController = GameInput.ControllerTypeEnum.Mouse;
			_shopMenu.IsCursorOnSlot = false;
		}
		else if (GameInput.AnyGamepadButtonDown(0) || GameInput.AnyGamepadButtonDown(1))
		{
			_shopMenu.CurrentController = GameInput.ControllerTypeEnum.GamePad;
		}
		else if (!GameCore.Console.IsActive && GameInput.SystemGetPressedKey() != Keys.None)
		{
			_shopMenu.CurrentController = GameInput.ControllerTypeEnum.Keyboard;
		}
		StickDirection = ControllerDirectionEnum.None;
		StickDirectionPressed = ControllerDirectionEnum.None;
		for (int i = 0; i < 2; i++)
		{
			Vector2 leftStickPos = GameInput.GetLeftStickPos(i);
			Vector2 rightStickPos = GameInput.GetRightStickPos(i);
			if (leftStickPos.X <= 0f - _deadZone.X || rightStickPos.X <= 0f - _deadZone.X || GameInput.SystemGPButtonDown(Buttons.DPadLeft, i) || (!GameCore.Console.IsActive && GameInput.SystemKeyDown(Keys.Left)))
			{
				StickDirection = ControllerDirectionEnum.Left;
				break;
			}
			if (leftStickPos.X >= _deadZone.X || rightStickPos.X >= _deadZone.X || GameInput.SystemGPButtonDown(Buttons.DPadRight, i) || (!GameCore.Console.IsActive && GameInput.SystemKeyDown(Keys.Right)))
			{
				StickDirection = ControllerDirectionEnum.Right;
				break;
			}
			if (leftStickPos.Y >= _deadZone.Y || rightStickPos.Y >= _deadZone.Y || GameInput.SystemGPButtonDown(Buttons.DPadUp, i) || (!GameCore.Console.IsActive && GameInput.SystemKeyDown(Keys.Up)))
			{
				StickDirection = ControllerDirectionEnum.Up;
				break;
			}
			if (leftStickPos.Y <= 0f - _deadZone.Y || rightStickPos.Y <= 0f - _deadZone.Y || GameInput.SystemGPButtonDown(Buttons.DPadDown, i) || (!GameCore.Console.IsActive && GameInput.SystemKeyDown(Keys.Down)))
			{
				StickDirection = ControllerDirectionEnum.Down;
				break;
			}
		}
		if (StickDirection == ControllerDirectionEnum.None)
		{
			_stickRepeatTimer = Options.GamePadRepeatDelay;
		}
		if (_oldStickDirecion != StickDirection)
		{
			StickDirectionPressed = StickDirection;
			_oldStickDirecion = StickDirection;
			return;
		}
		_stickRepeatTimer -= elapsedTime;
		if (_stickRepeatTimer <= 0f)
		{
			_oldStickDirecion = ControllerDirectionEnum.None;
			if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile && _shopMenu.PanelShipParams.ActivatedSliderIndex >= 0)
			{
				_stickRepeatTimer = 0.075f;
			}
			else
			{
				_stickRepeatTimer = Options.GamePadRepeatDelay / 4f;
			}
		}
	}

	public void CancelKeyPressed()
	{
		GameInput.CancelKeyPressed();
		GameInput.CancelButtonPressed();
		StickDirectionPressed = ControllerDirectionEnum.None;
		_oldStickDirecion = StickDirection;
	}

	public bool KeyPressed(ShopMenuKeys menuKey)
	{
		switch (menuKey)
		{
		case ShopMenuKeys.Select:
			if (GameInput.ControlDown || ((!_shopMenu.IsKeyInputFree || (!GameInput.SystemKeyPressed(Keys.Enter) && !GameInput.SystemKeyPressed(Keys.A))) && !GameInput.MousePressed(GameInput.MouseButton.Left)))
			{
				if (GameInput.SystemGPButtonPressedAll(Buttons.A) && !GameInput.SystemGPButtonDown(Buttons.LeftTrigger))
				{
					return !GameInput.SystemGPButtonDown(Buttons.RightTrigger);
				}
				return false;
			}
			return true;
		case ShopMenuKeys.Action:
			if (GameInput.ControlDown || ((!_shopMenu.IsKeyInputFree || !GameInput.SystemKeyPressed(Keys.B)) && !GameInput.MousePressed(GameInput.MouseButton.Right)))
			{
				if (GameInput.SystemGPButtonPressedAll(Buttons.B) && !GameInput.SystemGPButtonDown(Buttons.LeftTrigger))
				{
					return !GameInput.SystemGPButtonDown(Buttons.RightTrigger);
				}
				return false;
			}
			return true;
		case ShopMenuKeys.ActionWithControl:
			if (!GameInput.ControlDown || ((!_shopMenu.IsKeyInputFree || (!GameInput.SystemKeyPressed(Keys.Enter) && !GameInput.SystemKeyPressed(Keys.A) && !GameInput.SystemKeyPressed(Keys.B))) && !GameInput.MousePressed(GameInput.MouseButton.Left) && !GameInput.MousePressed(GameInput.MouseButton.Right)))
			{
				if ((GameInput.SystemGPButtonPressedAll(Buttons.A) || GameInput.SystemGPButtonPressedAll(Buttons.B)) && !GameInput.SystemGPButtonDown(Buttons.LeftTrigger))
				{
					return GameInput.SystemGPButtonDown(Buttons.RightTrigger);
				}
				return false;
			}
			return true;
		case ShopMenuKeys.ActionWithShift:
			if (!GameInput.ShiftDown || GameInput.AltDown || ((!_shopMenu.IsKeyInputFree || (!GameInput.SystemKeyPressed(Keys.Enter) && !GameInput.SystemKeyPressed(Keys.A) && !GameInput.SystemKeyPressed(Keys.B))) && !GameInput.MousePressed(GameInput.MouseButton.Left) && !GameInput.MousePressed(GameInput.MouseButton.Right)))
			{
				if (GameInput.SystemGPButtonPressedAll(Buttons.A) || GameInput.SystemGPButtonPressedAll(Buttons.B))
				{
					return GameInput.SystemGPButtonDown(Buttons.LeftTrigger);
				}
				return false;
			}
			return true;
		case ShopMenuKeys.CancelSelection:
			if (!_shopMenu.IsKeyInputFree || !GameInput.SystemKeyPressed(Keys.Back))
			{
				return GameInput.SystemGPButtonPressedAll(Buttons.Back);
			}
			return true;
		case ShopMenuKeys.CancelAll:
			if (!_shopMenu.IsKeyInputFree || !GameInput.SystemKeyPressed(Keys.Escape))
			{
				return GameInput.SystemGPButtonPressedAll(Buttons.Start);
			}
			return true;
		case ShopMenuKeys.MoveUp:
			return StickDirectionPressed == ControllerDirectionEnum.Up;
		case ShopMenuKeys.MoveDown:
			return StickDirectionPressed == ControllerDirectionEnum.Down;
		case ShopMenuKeys.MoveRight:
			return StickDirectionPressed == ControllerDirectionEnum.Right;
		case ShopMenuKeys.MoveLeft:
			return StickDirectionPressed == ControllerDirectionEnum.Left;
		case ShopMenuKeys.PageUp:
			if (_shopMenu.IsKeyInputFree)
			{
				return GameInput.SystemKeyPressed(Keys.PageUp);
			}
			return false;
		case ShopMenuKeys.PageDown:
			if (_shopMenu.IsKeyInputFree)
			{
				return GameInput.SystemKeyPressed(Keys.PageDown);
			}
			return false;
		case ShopMenuKeys.Home:
			if (_shopMenu.IsKeyInputFree)
			{
				return GameInput.SystemKeyPressed(Keys.Home);
			}
			return false;
		case ShopMenuKeys.End:
			if (_shopMenu.IsKeyInputFree)
			{
				return GameInput.SystemKeyPressed(Keys.End);
			}
			return false;
		case ShopMenuKeys.MoveUpWithControl:
			if (StickDirectionPressed != ControllerDirectionEnum.Up || (!GameInput.SystemGPButtonDown(Buttons.LeftTrigger) && !GameInput.SystemGPButtonDown(Buttons.RightTrigger)))
			{
				if (_shopMenu.IsKeyInputFree && GameInput.SystemKeyPressed(Keys.Up))
				{
					if (!GameInput.ShiftDown)
					{
						return GameInput.ControlDown;
					}
					return true;
				}
				return false;
			}
			return true;
		case ShopMenuKeys.MoveDownWithControl:
			if (StickDirectionPressed != ControllerDirectionEnum.Down || (!GameInput.SystemGPButtonDown(Buttons.LeftTrigger) && !GameInput.SystemGPButtonDown(Buttons.RightTrigger)))
			{
				if (_shopMenu.IsKeyInputFree && GameInput.SystemKeyPressed(Keys.Down))
				{
					if (!GameInput.ShiftDown)
					{
						return GameInput.ControlDown;
					}
					return true;
				}
				return false;
			}
			return true;
		case ShopMenuKeys.MoveLeftWithControl:
			if (StickDirectionPressed != ControllerDirectionEnum.Left || (!GameInput.SystemGPButtonDown(Buttons.LeftTrigger) && !GameInput.SystemGPButtonDown(Buttons.RightTrigger)))
			{
				if (_shopMenu.IsKeyInputFree && GameInput.SystemKeyPressed(Keys.Left))
				{
					if (!GameInput.ShiftDown)
					{
						return GameInput.ControlDown;
					}
					return true;
				}
				return false;
			}
			return true;
		case ShopMenuKeys.MoveRightWithControl:
			if (StickDirectionPressed != ControllerDirectionEnum.Right || (!GameInput.SystemGPButtonDown(Buttons.LeftTrigger) && !GameInput.SystemGPButtonDown(Buttons.RightTrigger)))
			{
				if (_shopMenu.IsKeyInputFree && GameInput.SystemKeyPressed(Keys.Right))
				{
					if (!GameInput.ShiftDown)
					{
						return GameInput.ControlDown;
					}
					return true;
				}
				return false;
			}
			return true;
		case ShopMenuKeys.NextTab:
			if (!GameInput.SystemGPButtonPressedAll(Buttons.RightShoulder))
			{
				if (_shopMenu.IsKeyInputFree && GameInput.SystemKeyPressed(Keys.Tab) && !GameInput.ShiftDown)
				{
					return !GameInput.ControlDown;
				}
				return false;
			}
			return true;
		case ShopMenuKeys.PreviousTab:
			if (!GameInput.SystemGPButtonPressedAll(Buttons.LeftShoulder))
			{
				if (_shopMenu.IsKeyInputFree && GameInput.SystemKeyPressed(Keys.Tab) && GameInput.ShiftDown)
				{
					return !GameInput.ControlDown;
				}
				return false;
			}
			return true;
		case ShopMenuKeys.ButtonX:
			if (!_shopMenu.IsKeyInputFree || (!GameInput.SystemKeyPressed(Keys.OemMinus) && !GameInput.SystemKeyPressed(Keys.Subtract) && !GameInput.SystemKeyPressed(Keys.OemComma) && !GameInput.SystemKeyPressed(Keys.X)))
			{
				return GameInput.SystemGPButtonPressedAll(Buttons.X);
			}
			return true;
		case ShopMenuKeys.ButtonY:
			if (!_shopMenu.IsKeyInputFree || (!GameInput.SystemKeyPressed(Keys.OemPlus) && !GameInput.SystemKeyPressed(Keys.Add) && !GameInput.SystemKeyPressed(Keys.OemPeriod) && !GameInput.SystemKeyPressed(Keys.Y)))
			{
				return GameInput.SystemGPButtonPressedAll(Buttons.Y);
			}
			return true;
		default:
			return false;
		}
	}
}
