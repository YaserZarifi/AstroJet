using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.JnG;
using JnG2.Mobile;

namespace Rake2D;

public static class GameInput
{
	public enum ControllerTypeEnum
	{
		Mouse,
		GamePad,
		Keyboard
	}

	public enum MouseButton
	{
		Left,
		Right,
		Middle,
		X1,
		X2
	}

	public enum KeyTypeEnum
	{
		FireGuns = 0,
		FireBombs = 1,
		PrimaryDevice = 2,
		SecondaryDevice = 3,
		MoveUp = 4,
		MoveDown = 5,
		MoveLeft = 6,
		MoveRight = 7,
		MaxCount = 8,
		None = -1
	}

	public enum PlayerControllerEnum
	{
		Keyboard,
		Gamepad,
		Mouse
	}

	private struct KeyTypeBindings
	{
		internal Keys[] _keys;

		internal Buttons _button;

		internal MouseButton? _mouseButton;
	}

	private struct PlayerBindings
	{
		internal KeyTypeBindings[] _bindings;
	}

	public static bool IsMouseFixed;

	public static Vector2 MouseMoveDelta;

	private const int _MAX_KEY = 20;

	private const int _MAX_KEY_ALTERNATIVE = 3;

	private static Buttons[] _buttonValues;

	private static PlayerBindings[] _playerBindings;

	private static KeyboardState _keyboardState;

	private static Keys[] _keyboardKeys;

	private static KeyboardState _lastKeyboardState;

	private static MouseState _mouseState;

	private static MouseState _lastMouseState;

	private static GamePadState[] _gamePadState;

	private static GamePadState[] _lastGamePadState;

	private static GamePadCapabilities[] _gamePadCapabilities;

	private static float _mouseClickTimer;

	private static int _mouseClickCount;

	private static float _mouseUpdateTimer;

	private static GameCore _gameCore;

	public static bool ControlDown { get; private set; }

	public static bool AltDown { get; private set; }

	public static bool ShiftDown { get; private set; }

	public static bool DoubleClick { get; private set; }

	public static int MouseX => _mouseState.X;

	public static int MouseY => _mouseState.Y;

	static GameInput()
	{
		_buttonValues = (Buttons[])Enum.GetValues(typeof(Buttons));
		_playerBindings = new PlayerBindings[3];
		for (int i = 0; i < _playerBindings.Length; i++)
		{
			_playerBindings[i]._bindings = new KeyTypeBindings[8];
			for (int j = 0; j < _playerBindings[i]._bindings.Length; j++)
			{
				_playerBindings[i]._bindings[j]._keys = new Keys[0];
			}
		}
		MergePlayerBindings();
		_gamePadState = new GamePadState[2];
		_lastGamePadState = new GamePadState[2];
		_gamePadCapabilities = new GamePadCapabilities[2];
	}

	public static void SetGameCore(GameCore gameCore)
	{
		_gameCore = gameCore;
	}

	public static void Update(float elapsedTime)
	{
		_mouseState = Mouse.GetState();
		if (_gameCore != null && MobileInputBridge.Enabled)
		{
			bool gameplay = GameJnG.MainMenu != null && !GameCore.IsMainMenuEnabled &&
				!GameCore.IsShopMenuEnabled && !GameCore.IsInLevelMenuEnabled &&
				!GameJnG.IsAnyMissionScreenVisible && !GameJnG.IsAnyPopupMenuVisible;
			_mouseState = MobileInputBridge.Update(_gameCore.Window.ClientBounds.Width,
				_gameCore.Window.ClientBounds.Height, gameplay, _mouseState);
		}
		if (IsMouseFixed)
		{
			if (Utils.ChangeValueToTarget(ref _mouseUpdateTimer, 0f, elapsedTime))
			{
				_mouseUpdateTimer = 0.025f;
				Vector2 vector = new Vector2(_gameCore.Window.ClientBounds.Width / 2, _gameCore.Window.ClientBounds.Height / 2);
				MouseMoveDelta = new Vector2(_mouseState.Position.X, _mouseState.Position.Y) - vector;
				MouseMoveDelta /= new Vector2((float)_gameCore.GraphicsDevice.DisplayMode.Width / 1920f, (float)_gameCore.GraphicsDevice.DisplayMode.Height / 1080f);
				Mouse.SetPosition((int)vector.X, (int)vector.Y);
			}
		}
		else
		{
			MouseMoveDelta = Vector2.Zero;
		}
		if (!GameCore.IsApplicationActive)
		{
			return;
		}
		_keyboardState = Keyboard.GetState();
		_keyboardKeys = _keyboardState.GetPressedKeys();
		for (int i = 0; i < _gamePadState.Length; i++)
		{
			int num = Options.GamePadNumber[i];
			if (num >= 0 && num < GamePad.MaximumGamePadCount)
			{
				_gamePadState[i] = GamePad.GetState(num, Options.GamePadDeadZoneMode);
				_gamePadCapabilities[i] = GamePad.GetCapabilities(num);
			}
			else
			{
				_gamePadState[i] = default;
				_gamePadCapabilities[i] = default;
			}
		}
		_mouseClickTimer -= elapsedTime;
		if (_mouseClickTimer <= 0f)
		{
			DoubleClick = false;
			_mouseClickCount = 0;
		}
		if (MousePressed(MouseButton.Left))
		{
			_mouseClickCount++;
			if (_mouseClickCount > 1)
			{
				_mouseClickCount = 0;
				DoubleClick = true;
			}
			_mouseClickTimer = 0.3f;
		}
		AltDown = _keyboardState.IsKeyDown(Keys.LeftAlt) || _keyboardState.IsKeyDown(Keys.RightAlt);
		ControlDown = _keyboardState.IsKeyDown(Keys.LeftControl) || _keyboardState.IsKeyDown(Keys.RightControl);
		ShiftDown = _keyboardState.IsKeyDown(Keys.LeftShift) || _keyboardState.IsKeyDown(Keys.RightShift);
	}

	public static bool IsGamepadConnected(int gamePadNumber)
	{
		if (gamePadNumber >= 0 && gamePadNumber < GamePad.MaximumGamePadCount)
		{
			return GamePad.GetCapabilities(gamePadNumber).IsConnected;
		}
		return false;
	}

	public static string GetGamePadName(int gamePadNumber)
	{
		if (gamePadNumber >= 0 && gamePadNumber < GamePad.MaximumGamePadCount)
		{
			GamePadCapabilities capabilities = GamePad.GetCapabilities(gamePadNumber);
			return capabilities.DisplayName + $" {capabilities.Identifier}";
		}
		return TextBundle.GetText("options_controller_gamepad") + $" {gamePadNumber}";
	}

	public static int GetFirstConnectedGamePad(int[] ignoreNumberArray = null)
	{
		for (int i = 0; i < GamePad.MaximumGamePadCount; i++)
		{
			GamePadCapabilities capabilities = GamePad.GetCapabilities(i);
			if ((ignoreNumberArray == null || !ignoreNumberArray.Contains(i)) && capabilities.IsConnected)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool AnyGamepadButtonDown(int playerNumber)
	{
		if (playerNumber < 0 || playerNumber >= _gamePadState.Length)
		{
			return false;
		}
		Buttons[] buttonValues = _buttonValues;
		foreach (Buttons button in buttonValues)
		{
			if (_gamePadState[playerNumber].IsButtonDown(button))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyGamepadButtonPressed(int playerNumber)
	{
		if (playerNumber < 0 || playerNumber >= _gamePadState.Length)
		{
			return false;
		}
		Buttons[] buttonValues = _buttonValues;
		for (int i = 0; i < buttonValues.Length; i++)
		{
			if (SystemGPButtonPressed(buttonValues[i], playerNumber))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyGamepadButtonPressedAll()
	{
		for (int i = 0; i < 2; i++)
		{
			if (AnyGamepadButtonPressed(i))
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyMouseButtonPressed()
	{
		if (!MousePressed(MouseButton.Left) && !MousePressed(MouseButton.Right))
		{
			return MousePressed(MouseButton.Middle);
		}
		return true;
	}

	public static bool AnySystemKeyPressed()
	{
		if (_keyboardKeys != null)
		{
			return _keyboardKeys.Length != 0;
		}
		return false;
	}

	public static Keys SystemGetPressedKey(bool bIgnoreControlKeys = true)
	{
		if (_keyboardKeys == null)
		{
			return Keys.None;
		}
		for (int i = 0; i < _keyboardKeys.Length; i++)
		{
			Keys keys = _keyboardKeys[i];
			if ((!bIgnoreControlKeys || (keys != Keys.LeftControl && keys != Keys.RightControl && keys != Keys.LeftShift && keys != Keys.RightShift && keys != Keys.LeftAlt && keys != Keys.RightAlt)) && !_lastKeyboardState.IsKeyDown(keys))
			{
				return keys;
			}
		}
		return Keys.None;
	}

	public static void CancelKeyPressed()
	{
		_lastKeyboardState = _keyboardState;
	}

	public static void CancelMousePressed()
	{
		_lastMouseState = _mouseState;
	}

	public static void CancelButtonPressed()
	{
		for (int i = 0; i < _lastGamePadState.Length; i++)
		{
			_lastGamePadState[i] = _gamePadState[i];
		}
	}

	public static void ClearLastKeyboadState()
	{
		_lastKeyboardState = default;
	}

	public static void UpdateEnd()
	{
		_lastKeyboardState = _keyboardState;
		_lastMouseState = _mouseState;
		for (int i = 0; i < 2; i++)
		{
			_lastGamePadState[i] = _gamePadState[i];
		}
	}

	private static void MergePlayerBindings()
	{
		for (int i = 0; i < 8; i++)
		{
			int num = 0;
			for (int j = 0; j < 2; j++)
			{
				num += _playerBindings[j]._bindings[i]._keys.Length;
			}
			if (_playerBindings[2]._bindings[i]._keys.Length != num)
			{
				_playerBindings[2]._bindings[i]._keys = new Keys[num];
			}
			_playerBindings[2]._bindings[i]._button = (Buttons)0;
			_playerBindings[2]._bindings[i]._mouseButton = null;
			int num2 = 0;
			for (int k = 0; k < 2; k++)
			{
				for (int l = 0; l < _playerBindings[k]._bindings[i]._keys.Length; l++)
				{
					_playerBindings[2]._bindings[i]._keys[num2++] = _playerBindings[k]._bindings[i]._keys[l];
				}
				if (_playerBindings[2]._bindings[i]._button == 0)
				{
					_playerBindings[2]._bindings[i]._button = _playerBindings[k]._bindings[i]._button;
				}
				if (!_playerBindings[2]._bindings[i]._mouseButton.HasValue)
				{
					_playerBindings[2]._bindings[i]._mouseButton = _playerBindings[k]._bindings[i]._mouseButton;
				}
			}
		}
	}

	public static Keys[] GetKeysForPlayer(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber >= 2)
		{
			return null;
		}
		if (keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return null;
		}
		if (GameJnG.PlayerCount == 1)
		{
			return _playerBindings[2]._bindings[(int)keyType]._keys;
		}
		return _playerBindings[playerNumber]._bindings[(int)keyType]._keys;
	}

	public static Buttons GetButtonForPlayer(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber >= 2)
		{
			return (Buttons)0;
		}
		if (keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return (Buttons)0;
		}
		return _playerBindings[playerNumber]._bindings[(int)keyType]._button;
	}

	public static MouseButton? GetMouseButtonForPlayer(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber >= 2)
		{
			return null;
		}
		if (keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return null;
		}
		return _playerBindings[playerNumber]._bindings[(int)keyType]._mouseButton;
	}

	public static bool TryConvertKeyboardInput(out char keyChar)
	{
		switch (SystemGetPressedKey())
		{
		case Keys.A:
			if (ShiftDown)
			{
				keyChar = 'A';
			}
			else
			{
				keyChar = 'a';
			}
			return true;
		case Keys.B:
			if (ShiftDown)
			{
				keyChar = 'B';
			}
			else
			{
				keyChar = 'b';
			}
			return true;
		case Keys.C:
			if (ShiftDown)
			{
				keyChar = 'C';
			}
			else
			{
				keyChar = 'c';
			}
			return true;
		case Keys.D:
			if (ShiftDown)
			{
				keyChar = 'D';
			}
			else
			{
				keyChar = 'd';
			}
			return true;
		case Keys.E:
			if (ShiftDown)
			{
				keyChar = 'E';
			}
			else
			{
				keyChar = 'e';
			}
			return true;
		case Keys.F:
			if (ShiftDown)
			{
				keyChar = 'F';
			}
			else
			{
				keyChar = 'f';
			}
			return true;
		case Keys.G:
			if (ShiftDown)
			{
				keyChar = 'G';
			}
			else
			{
				keyChar = 'g';
			}
			return true;
		case Keys.H:
			if (ShiftDown)
			{
				keyChar = 'H';
			}
			else
			{
				keyChar = 'h';
			}
			return true;
		case Keys.I:
			if (ShiftDown)
			{
				keyChar = 'I';
			}
			else
			{
				keyChar = 'i';
			}
			return true;
		case Keys.J:
			if (ShiftDown)
			{
				keyChar = 'J';
			}
			else
			{
				keyChar = 'j';
			}
			return true;
		case Keys.K:
			if (ShiftDown)
			{
				keyChar = 'K';
			}
			else
			{
				keyChar = 'k';
			}
			return true;
		case Keys.L:
			if (ShiftDown)
			{
				keyChar = 'L';
			}
			else
			{
				keyChar = 'l';
			}
			return true;
		case Keys.M:
			if (ShiftDown)
			{
				keyChar = 'M';
			}
			else
			{
				keyChar = 'm';
			}
			return true;
		case Keys.N:
			if (ShiftDown)
			{
				keyChar = 'N';
			}
			else
			{
				keyChar = 'n';
			}
			return true;
		case Keys.O:
			if (ShiftDown)
			{
				keyChar = 'O';
			}
			else
			{
				keyChar = 'o';
			}
			return true;
		case Keys.P:
			if (ShiftDown)
			{
				keyChar = 'P';
			}
			else
			{
				keyChar = 'p';
			}
			return true;
		case Keys.Q:
			if (ShiftDown)
			{
				keyChar = 'Q';
			}
			else
			{
				keyChar = 'q';
			}
			return true;
		case Keys.R:
			if (ShiftDown)
			{
				keyChar = 'R';
			}
			else
			{
				keyChar = 'r';
			}
			return true;
		case Keys.S:
			if (ShiftDown)
			{
				keyChar = 'S';
			}
			else
			{
				keyChar = 's';
			}
			return true;
		case Keys.T:
			if (ShiftDown)
			{
				keyChar = 'T';
			}
			else
			{
				keyChar = 't';
			}
			return true;
		case Keys.U:
			if (ShiftDown)
			{
				keyChar = 'U';
			}
			else
			{
				keyChar = 'u';
			}
			return true;
		case Keys.V:
			if (ShiftDown)
			{
				keyChar = 'V';
			}
			else
			{
				keyChar = 'v';
			}
			return true;
		case Keys.W:
			if (ShiftDown)
			{
				keyChar = 'W';
			}
			else
			{
				keyChar = 'w';
			}
			return true;
		case Keys.X:
			if (ShiftDown)
			{
				keyChar = 'X';
			}
			else
			{
				keyChar = 'x';
			}
			return true;
		case Keys.Y:
			if (ShiftDown)
			{
				keyChar = 'Y';
			}
			else
			{
				keyChar = 'y';
			}
			return true;
		case Keys.Z:
			if (ShiftDown)
			{
				keyChar = 'Z';
			}
			else
			{
				keyChar = 'z';
			}
			return true;
		case Keys.D0:
			if (ShiftDown)
			{
				keyChar = ')';
			}
			else
			{
				keyChar = '0';
			}
			return true;
		case Keys.D1:
			if (ShiftDown)
			{
				keyChar = '!';
			}
			else
			{
				keyChar = '1';
			}
			return true;
		case Keys.D2:
			if (ShiftDown)
			{
				keyChar = '@';
			}
			else
			{
				keyChar = '2';
			}
			return true;
		case Keys.D3:
			if (ShiftDown)
			{
				keyChar = '#';
			}
			else
			{
				keyChar = '3';
			}
			return true;
		case Keys.D4:
			if (ShiftDown)
			{
				keyChar = '$';
			}
			else
			{
				keyChar = '4';
			}
			return true;
		case Keys.D5:
			if (ShiftDown)
			{
				keyChar = '%';
			}
			else
			{
				keyChar = '5';
			}
			return true;
		case Keys.D6:
			if (ShiftDown)
			{
				keyChar = '^';
			}
			else
			{
				keyChar = '6';
			}
			return true;
		case Keys.D7:
			if (ShiftDown)
			{
				keyChar = '&';
			}
			else
			{
				keyChar = '7';
			}
			return true;
		case Keys.D8:
			if (ShiftDown)
			{
				keyChar = '*';
			}
			else
			{
				keyChar = '8';
			}
			return true;
		case Keys.D9:
			if (ShiftDown)
			{
				keyChar = '(';
			}
			else
			{
				keyChar = '9';
			}
			return true;
		case Keys.NumPad0:
			keyChar = '0';
			return true;
		case Keys.NumPad1:
			keyChar = '1';
			return true;
		case Keys.NumPad2:
			keyChar = '2';
			return true;
		case Keys.NumPad3:
			keyChar = '3';
			return true;
		case Keys.NumPad4:
			keyChar = '4';
			return true;
		case Keys.NumPad5:
			keyChar = '5';
			return true;
		case Keys.NumPad6:
			keyChar = '6';
			return true;
		case Keys.NumPad7:
			keyChar = '7';
			return true;
		case Keys.NumPad8:
			keyChar = '8';
			return true;
		case Keys.NumPad9:
			keyChar = '9';
			return true;
		case Keys.Add:
			keyChar = '+';
			return true;
		case Keys.Subtract:
			keyChar = '-';
			return true;
		case Keys.Multiply:
			keyChar = '*';
			return true;
		case Keys.Divide:
			keyChar = '/';
			return true;
		case Keys.Decimal:
			keyChar = '.';
			return true;
		case Keys.OemTilde:
			if (ShiftDown)
			{
				keyChar = '~';
			}
			else
			{
				keyChar = '`';
			}
			return true;
		case Keys.OemSemicolon:
			if (ShiftDown)
			{
				keyChar = ':';
			}
			else
			{
				keyChar = ';';
			}
			return true;
		case Keys.OemQuotes:
			if (ShiftDown)
			{
				keyChar = '"';
			}
			else
			{
				keyChar = '\'';
			}
			return true;
		case Keys.OemQuestion:
			if (ShiftDown)
			{
				keyChar = '?';
			}
			else
			{
				keyChar = '/';
			}
			return true;
		case Keys.OemPlus:
			if (ShiftDown)
			{
				keyChar = '+';
			}
			else
			{
				keyChar = '=';
			}
			return true;
		case Keys.OemPipe:
			if (ShiftDown)
			{
				keyChar = '|';
			}
			else
			{
				keyChar = '\\';
			}
			return true;
		case Keys.OemPeriod:
			if (ShiftDown)
			{
				keyChar = '>';
			}
			else
			{
				keyChar = '.';
			}
			return true;
		case Keys.OemOpenBrackets:
			if (ShiftDown)
			{
				keyChar = '{';
			}
			else
			{
				keyChar = '[';
			}
			return true;
		case Keys.OemCloseBrackets:
			if (ShiftDown)
			{
				keyChar = '}';
			}
			else
			{
				keyChar = ']';
			}
			return true;
		case Keys.OemMinus:
			if (ShiftDown)
			{
				keyChar = '_';
			}
			else
			{
				keyChar = '-';
			}
			return true;
		case Keys.OemComma:
			if (ShiftDown)
			{
				keyChar = '<';
			}
			else
			{
				keyChar = ',';
			}
			return true;
		case Keys.Space:
			keyChar = ' ';
			return true;
		default:
			keyChar = '\0';
			return false;
		}
	}

	public static bool KeyDown(KeyTypeEnum keyType, int playerNumber = 0)
	{
		if (GameCore.IsEditorEnabled || GameCore.Console.IsActive)
		{
			return false;
		}
		if (MobileInputBridge.IsDown((int)keyType, playerNumber)) return true;
		Keys[] keysForPlayer = GetKeysForPlayer(keyType, playerNumber);
		if (keysForPlayer != null)
		{
			Keys[] array = keysForPlayer;
			for (int i = 0; i < array.Length; i++)
			{
				if (SystemKeyDown(array[i]))
				{
					return true;
				}
			}
		}
		Buttons buttonForPlayer = GetButtonForPlayer(keyType, playerNumber);
		if (buttonForPlayer != 0 && playerNumber >= 0 && SystemGPButtonDown(buttonForPlayer, playerNumber))
		{
			return true;
		}
		bool flag = false;
		if (playerNumber < GameJnG.PlayerCount && GameJnG.HeroList[playerNumber].MouseGun != null)
		{
			flag = true;
		}
		if ((Options.PlayerController[playerNumber] == PlayerControllerEnum.Mouse || GameJnG.PlayerCount < 2) | flag)
		{
			MouseButton? mouseButtonForPlayer = GetMouseButtonForPlayer(keyType, playerNumber);
			if (mouseButtonForPlayer.HasValue && MouseDown(mouseButtonForPlayer.Value))
			{
				return true;
			}
		}
		return false;
	}

	public static bool KeyPressed(KeyTypeEnum keyType, int playerNumber = 0)
	{
		if (GameCore.IsEditorEnabled || GameCore.Console.IsActive)
		{
			return false;
		}
		if (MobileInputBridge.IsPressed((int)keyType, playerNumber)) return true;
		Keys[] keysForPlayer = GetKeysForPlayer(keyType, playerNumber);
		if (keysForPlayer != null)
		{
			Keys[] array = keysForPlayer;
			for (int i = 0; i < array.Length; i++)
			{
				if (SystemKeyPressed(array[i]))
				{
					return true;
				}
			}
		}
		if (playerNumber >= 0)
		{
			Buttons buttonForPlayer = GetButtonForPlayer(keyType, playerNumber);
			if (buttonForPlayer != 0 && SystemGPButtonPressed(buttonForPlayer, playerNumber))
			{
				return true;
			}
		}
		if (Options.PlayerController[playerNumber] == PlayerControllerEnum.Mouse || GameJnG.PlayerCount < 2)
		{
			MouseButton? mouseButtonForPlayer = GetMouseButtonForPlayer(keyType, playerNumber);
			if (mouseButtonForPlayer.HasValue && MousePressed(mouseButtonForPlayer.Value))
			{
				return true;
			}
		}
		return false;
	}

	public static Vector2 GetLeftStickPos(int gpIndex)
	{
		if (gpIndex == 0 && MobileInputBridge.Movement != Vector2.Zero) return MobileInputBridge.Movement;
		if (gpIndex < 0 || _gamePadCapabilities.Length < gpIndex + 1 || !_gamePadCapabilities[gpIndex].HasLeftXThumbStick)
		{
			return Vector2.Zero;
		}
		return new Vector2(_gamePadState[gpIndex].ThumbSticks.Left.X, _gamePadState[gpIndex].ThumbSticks.Left.Y);
	}

	public static Vector2 GetRightStickPos(int gpIndex)
	{
		if (gpIndex < 0 || _gamePadCapabilities.Length < gpIndex + 1 || !_gamePadCapabilities[gpIndex].HasRightXThumbStick)
		{
			return Vector2.Zero;
		}
		return new Vector2(_gamePadState[gpIndex].ThumbSticks.Right.X, _gamePadState[gpIndex].ThumbSticks.Right.Y);
	}

	public static bool MouseDown(MouseButton button)
	{
		return button switch
		{
			MouseButton.Left => _mouseState.LeftButton == ButtonState.Pressed, 
			MouseButton.Right => _mouseState.RightButton == ButtonState.Pressed, 
			MouseButton.Middle => _mouseState.MiddleButton == ButtonState.Pressed, 
			MouseButton.X1 => _mouseState.XButton1 == ButtonState.Pressed, 
			MouseButton.X2 => _mouseState.XButton2 == ButtonState.Pressed, 
			_ => false, 
		};
	}

	public static bool MousePressed(MouseButton button)
	{
		switch (button)
		{
		case MouseButton.Left:
			if (_mouseState.LeftButton == ButtonState.Pressed)
			{
				return _lastMouseState.LeftButton == ButtonState.Released;
			}
			return false;
		case MouseButton.Right:
			if (_mouseState.RightButton == ButtonState.Pressed)
			{
				return _lastMouseState.RightButton == ButtonState.Released;
			}
			return false;
		case MouseButton.Middle:
			if (_mouseState.MiddleButton == ButtonState.Pressed)
			{
				return _lastMouseState.MiddleButton == ButtonState.Released;
			}
			return false;
		case MouseButton.X1:
			if (_mouseState.XButton1 == ButtonState.Pressed)
			{
				return _lastMouseState.XButton1 == ButtonState.Released;
			}
			return false;
		case MouseButton.X2:
			if (_mouseState.XButton2 == ButtonState.Pressed)
			{
				return _lastMouseState.XButton2 == ButtonState.Released;
			}
			return false;
		default:
			return false;
		}
	}

	public static bool MouseReleased(MouseButton button)
	{
		switch (button)
		{
		case MouseButton.Left:
			if (_mouseState.LeftButton == ButtonState.Released)
			{
				return _lastMouseState.LeftButton == ButtonState.Pressed;
			}
			return false;
		case MouseButton.Right:
			if (_mouseState.RightButton == ButtonState.Released)
			{
				return _lastMouseState.RightButton == ButtonState.Pressed;
			}
			return false;
		case MouseButton.Middle:
			if (_mouseState.MiddleButton == ButtonState.Released)
			{
				return _lastMouseState.MiddleButton == ButtonState.Pressed;
			}
			return false;
		case MouseButton.X1:
			if (_mouseState.XButton1 == ButtonState.Released)
			{
				return _lastMouseState.XButton1 == ButtonState.Pressed;
			}
			return false;
		case MouseButton.X2:
			if (_mouseState.XButton2 == ButtonState.Released)
			{
				return _lastMouseState.XButton2 == ButtonState.Pressed;
			}
			return false;
		default:
			return false;
		}
	}

	public static int GetMouseWheelDelta()
	{
		if (GameCore.IsApplicationActive)
		{
			return _mouseState.ScrollWheelValue - _lastMouseState.ScrollWheelValue;
		}
		return 0;
	}

	public static bool SystemKeyDown(Keys keyCode)
	{
		return _keyboardState.IsKeyDown(keyCode);
	}

	public static bool SystemKeyPressed(Keys keyCode)
	{
		if (keyCode == Keys.Escape && MobileInputBridge.Enabled &&
			(MobileInputBridge.Controls.Pressed & TouchAction.Pause) != 0) return true;
		if (_lastKeyboardState.IsKeyUp(keyCode))
		{
			return _keyboardState.IsKeyDown(keyCode);
		}
		return false;
	}

	public static bool SystemKeyReleased(Keys keyCode)
	{
		if (_lastKeyboardState.IsKeyDown(keyCode))
		{
			return _keyboardState.IsKeyUp(keyCode);
		}
		return false;
	}

	public static bool SystemGPButtonDown(Buttons button, int gpIndex = 0)
	{
		if (button > (Buttons)0 && gpIndex >= 0 && gpIndex < _gamePadState.Length)
		{
			return _gamePadState[gpIndex].IsButtonDown(button);
		}
		return false;
	}

	public static bool SystemGPButtonPressed(Buttons button, int gpIndex = 0)
	{
		if (button > (Buttons)0 && gpIndex >= 0 && gpIndex < _gamePadState.Length)
		{
			if (_lastGamePadState[gpIndex].IsButtonUp(button))
			{
				return _gamePadState[gpIndex].IsButtonDown(button);
			}
			return false;
		}
		return false;
	}

	public static bool SystemGPButtonPressedAll(Buttons button)
	{
		if (button <= (Buttons)0)
		{
			return false;
		}
		for (int i = 0; i < _lastGamePadState.Length; i++)
		{
			if (SystemGPButtonPressed(button, i))
			{
				return true;
			}
		}
		return false;
	}

	public static void RemoveKeyCodesDuplicity(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber >= 2 || keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return;
		}
		Keys[] checkedKeyArray = _playerBindings[playerNumber]._bindings[(int)keyType]._keys;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < _playerBindings[playerNumber]._bindings.Length; j++)
			{
				if ((j != (int)keyType || i != playerNumber) && _playerBindings[i]._bindings[j]._keys.Length != 0)
				{
					_playerBindings[i]._bindings[j]._keys = _playerBindings[i]._bindings[j]._keys.Where((Keys key) => !checkedKeyArray.Contains(key)).ToArray();
				}
			}
		}
		MergePlayerBindings();
	}

	public static void RemoveButtonCodesDuplicity(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber >= 2 || keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return;
		}
		for (int i = 0; i < _playerBindings[playerNumber]._bindings.Length; i++)
		{
			if (i != (int)keyType && _playerBindings[playerNumber]._bindings[i]._button == _playerBindings[playerNumber]._bindings[(int)keyType]._button)
			{
				_playerBindings[playerNumber]._bindings[i]._button = (Buttons)0;
			}
		}
		if (_playerBindings[playerNumber]._bindings[4]._button == 0)
		{
			_playerBindings[playerNumber]._bindings[4]._button = Buttons.DPadUp;
		}
		if (_playerBindings[playerNumber]._bindings[5]._button == 0)
		{
			_playerBindings[playerNumber]._bindings[5]._button = Buttons.DPadDown;
		}
		if (_playerBindings[playerNumber]._bindings[6]._button == 0)
		{
			_playerBindings[playerNumber]._bindings[6]._button = Buttons.DPadLeft;
		}
		if (_playerBindings[playerNumber]._bindings[7]._button == 0)
		{
			_playerBindings[playerNumber]._bindings[7]._button = Buttons.DPadRight;
		}
		MergePlayerBindings();
	}

	public static void RemoveMouseButtonCodesDuplicity(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber >= 2 || keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return;
		}
		for (int i = 0; i < _playerBindings[playerNumber]._bindings.Length; i++)
		{
			if (i != (int)keyType && _playerBindings[playerNumber]._bindings[i]._mouseButton == _playerBindings[playerNumber]._bindings[(int)keyType]._mouseButton)
			{
				_playerBindings[playerNumber]._bindings[i]._mouseButton = null;
			}
		}
		MergePlayerBindings();
	}

	public static void SetDefaultKeyCodes()
	{
		ClearKeyCodes();
		_playerBindings[0]._bindings[4]._keys = new Keys[1] { Keys.Up };
		_playerBindings[0]._bindings[5]._keys = new Keys[1] { Keys.Down };
		_playerBindings[0]._bindings[6]._keys = new Keys[1] { Keys.Left };
		_playerBindings[0]._bindings[7]._keys = new Keys[1] { Keys.Right };
		_playerBindings[0]._bindings[0]._keys = new Keys[1] { Keys.RightControl };
		_playerBindings[0]._bindings[1]._keys = new Keys[1] { Keys.RightAlt };
		_playerBindings[0]._bindings[2]._keys = new Keys[1] { Keys.OemComma };
		_playerBindings[0]._bindings[3]._keys = new Keys[1] { Keys.OemPeriod };
		_playerBindings[1]._bindings[4]._keys = new Keys[1] { Keys.W };
		_playerBindings[1]._bindings[5]._keys = new Keys[1] { Keys.S };
		_playerBindings[1]._bindings[6]._keys = new Keys[1] { Keys.A };
		_playerBindings[1]._bindings[7]._keys = new Keys[1] { Keys.D };
		_playerBindings[1]._bindings[0]._keys = new Keys[1] { Keys.LeftControl };
		_playerBindings[1]._bindings[1]._keys = new Keys[1] { Keys.LeftAlt };
		_playerBindings[1]._bindings[2]._keys = new Keys[1] { Keys.Z };
		_playerBindings[1]._bindings[3]._keys = new Keys[1] { Keys.X };
		MergePlayerBindings();
	}

	public static void SetDefaultGamePadButtonCodes()
	{
		ClearButtonCodes();
		for (int i = 0; i < 2; i++)
		{
			_playerBindings[i]._bindings[0]._button = Buttons.X;
			_playerBindings[i]._bindings[1]._button = Buttons.A;
			_playerBindings[i]._bindings[2]._button = Buttons.LeftTrigger;
			_playerBindings[i]._bindings[3]._button = Buttons.RightTrigger;
			_playerBindings[i]._bindings[6]._button = Buttons.DPadLeft;
			_playerBindings[i]._bindings[7]._button = Buttons.DPadRight;
			_playerBindings[i]._bindings[4]._button = Buttons.DPadUp;
			_playerBindings[i]._bindings[5]._button = Buttons.DPadDown;
		}
		MergePlayerBindings();
	}

	public static void SetDefaultMouseButtonCodes()
	{
		ClearMouseButtonCodes();
		for (int i = 0; i < 2; i++)
		{
			_playerBindings[i]._bindings[0]._mouseButton = MouseButton.Left;
			_playerBindings[i]._bindings[1]._mouseButton = MouseButton.Right;
			_playerBindings[i]._bindings[2]._mouseButton = MouseButton.X1;
			_playerBindings[i]._bindings[3]._mouseButton = MouseButton.X2;
			_playerBindings[i]._bindings[6]._mouseButton = null;
			_playerBindings[i]._bindings[7]._mouseButton = null;
			_playerBindings[i]._bindings[4]._mouseButton = null;
			_playerBindings[i]._bindings[5]._mouseButton = null;
		}
		MergePlayerBindings();
	}

	private static string GetKeyTypeName(KeyTypeEnum keyType)
	{
		return keyType switch
		{
			KeyTypeEnum.FireGuns => "guns", 
			KeyTypeEnum.FireBombs => "bombs", 
			KeyTypeEnum.PrimaryDevice => "primary_device", 
			KeyTypeEnum.SecondaryDevice => "secondary_device", 
			KeyTypeEnum.MoveLeft => "move_left", 
			KeyTypeEnum.MoveRight => "move_right", 
			KeyTypeEnum.MoveUp => "move_up", 
			KeyTypeEnum.MoveDown => "move_down", 
			_ => null, 
		};
	}

	private static void ClearKeyCodes()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < _playerBindings[i]._bindings.Length; j++)
			{
				_playerBindings[i]._bindings[j]._keys = new Keys[0];
			}
		}
	}

	private static void ClearButtonCodes()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < _playerBindings[i]._bindings.Length; j++)
			{
				_playerBindings[i]._bindings[j]._button = (Buttons)0;
			}
		}
	}

	private static void ClearMouseButtonCodes()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < _playerBindings[i]._bindings.Length; j++)
			{
				_playerBindings[i]._bindings[j]._mouseButton = null;
			}
		}
	}

	public static void SetKeyCode(KeyTypeEnum keyType, Keys key, int playerNumber)
	{
		if (playerNumber >= 0 && playerNumber <= 2 && keyType >= KeyTypeEnum.FireGuns && (int)keyType < _playerBindings[playerNumber]._bindings.Length)
		{
			_playerBindings[playerNumber]._bindings[(int)keyType]._keys = new Keys[1] { key };
			MergePlayerBindings();
		}
	}

	public static Keys[] GetKeyCodes(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber > 2)
		{
			return null;
		}
		if (keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return null;
		}
		return _playerBindings[playerNumber]._bindings[(int)keyType]._keys;
	}

	public static void SetGamePadButtonCode(KeyTypeEnum keyType, Buttons button, int playerNumber)
	{
		if (playerNumber >= 0 && playerNumber <= 2 && keyType >= KeyTypeEnum.FireGuns && (int)keyType < _playerBindings[playerNumber]._bindings.Length)
		{
			_playerBindings[playerNumber]._bindings[(int)keyType]._button = button;
			MergePlayerBindings();
		}
	}

	public static Buttons GetGamePadButtonCode(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber > 2)
		{
			return (Buttons)0;
		}
		if (keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return (Buttons)0;
		}
		return _playerBindings[playerNumber]._bindings[(int)keyType]._button;
	}

	public static void SetMouseButtonCode(KeyTypeEnum keyType, MouseButton button, int playerNumber)
	{
		if (playerNumber >= 0 && playerNumber <= 2 && keyType >= KeyTypeEnum.FireGuns && (int)keyType < _playerBindings[playerNumber]._bindings.Length)
		{
			_playerBindings[playerNumber]._bindings[(int)keyType]._mouseButton = button;
			MergePlayerBindings();
		}
	}

	public static MouseButton? GetMouseButtonCode(KeyTypeEnum keyType, int playerNumber)
	{
		if (playerNumber < 0 || playerNumber > 2)
		{
			return null;
		}
		if (keyType < KeyTypeEnum.FireGuns || (int)keyType >= _playerBindings[playerNumber]._bindings.Length)
		{
			return null;
		}
		return _playerBindings[playerNumber]._bindings[(int)keyType]._mouseButton;
	}

	internal static void ParseIniFileLine(IniFile iniFile)
	{
		for (int i = 0; i < 8; i++)
		{
			string keyTypeName = GetKeyTypeName((KeyTypeEnum)i);
			if (keyTypeName == null)
			{
				continue;
			}
			int num = 0;
			if (iniFile.KeyEquals("key_" + keyTypeName))
			{
				num = 1;
			}
			else if (iniFile.KeyEquals("btn_" + keyTypeName))
			{
				num = 2;
			}
			else
			{
				if (!iniFile.KeyEquals("mse_" + keyTypeName))
				{
					continue;
				}
				num = 3;
			}
			if (!int.TryParse(iniFile.Id, out var result) || result < 0 || result >= 2)
			{
				iniFile.LogErr("Wrong player number ({0}) for key '{1}'", iniFile.Id, iniFile.Key);
				break;
			}
			int intValue = iniFile.GetIntValue(-1);
			if (intValue < 0)
			{
				iniFile.LogErr("No value list for key '{0}'", iniFile.Key);
				break;
			}
			switch (num)
			{
			case 1:
			{
				Keys[] array = null;
				array = ((intValue == 0) ? new Keys[0] : new Keys[1] { (Keys)intValue });
				_playerBindings[result]._bindings[i]._keys = array;
				break;
			}
			case 2:
				_playerBindings[result]._bindings[i]._button = (Buttons)intValue;
				break;
			}
			MergePlayerBindings();
		}
	}

	internal static void SaveToFile(StreamWriter file)
	{
		for (int i = 0; i < 2; i++)
		{
			file.WriteLine($"//---- Input {i + 1} ----");
			for (int j = 0; j < 8; j++)
			{
				if (_playerBindings[i]._bindings[j]._keys != null && _playerBindings[i]._bindings[j]._keys.Length != 0)
				{
					file.WriteLine($"key_{GetKeyTypeName((KeyTypeEnum)j)}.{i} = {(int)_playerBindings[i]._bindings[j]._keys[0]}");
				}
				else
				{
					file.WriteLine($"key_{GetKeyTypeName((KeyTypeEnum)j)}.{i} = 0");
				}
				if (_playerBindings[i]._bindings[j]._button != 0)
				{
					file.WriteLine($"btn_{GetKeyTypeName((KeyTypeEnum)j)}.{i} = {(int)_playerBindings[i]._bindings[j]._button}");
				}
				if (_playerBindings[i]._bindings[j]._mouseButton.HasValue)
				{
					file.WriteLine($"mse_{GetKeyTypeName((KeyTypeEnum)j)}.{i} = {(int)_playerBindings[i]._bindings[j]._mouseButton.Value}");
				}
			}
		}
	}

	public static string GetKeyName(Keys key)
	{
		return key.ToString();
	}

	public static string GetGamePadButtonName(Buttons button)
	{
		return button.ToString();
	}

	public static string GetGamePadButtonName(KeyTypeEnum keyType, int playerNumber)
	{
		Buttons? buttons = GetGamePadButtonCode(keyType, playerNumber);
		if (buttons.HasValue)
		{
			return GetGamePadButtonName(buttons.Value);
		}
		return null;
	}

	public static string GetKeyName(KeyTypeEnum keyType, int playerNumber)
	{
		Keys[] keyCodes = GetKeyCodes(keyType, playerNumber);
		if (keyCodes.Length != 0 && keyCodes[0] > Keys.None)
		{
			return GetKeyName(keyCodes[0]);
		}
		return null;
	}
}
