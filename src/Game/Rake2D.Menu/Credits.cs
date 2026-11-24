using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Rake2D.Font.BMFont;

namespace Rake2D.Menu;

public class Credits
{
	public CreditsStateEnum State;

	public Layer MyLayer;

	private float _timer;

	private MainMenu _mainMenu;

	private BMFont _font;

	private int _lineNumber;

	private int _updateNumber;

	private float _alphaFactor;

	private const float SCALE_NORMAL = 0.6f;

	public Credits(MainMenu mainMenu, BMFont font)
	{
		_mainMenu = mainMenu;
		_font = font;
	}

	public void Update(float elapsedTime)
	{
		MyLayer = _mainMenu.Level.GetLayerById("credits");
		if (MyLayer != null && Utils.ChangeValueToTarget(ref _timer, 0f, elapsedTime))
		{
			string text = TextBundle.GetText($"credits_{_lineNumber}");
			float scale;
			switch (_lineNumber)
			{
			case 0:
				_timer = 0.65f;
				scale = 1f;
				break;
			case 14:
				_lineNumber = 0;
				_timer = 7f;
				return;
			default:
				scale = 0.6f;
				_timer = ((_lineNumber % 2 == 0) ? 0.5f : 1.2f);
				break;
			}
			CreditsObject creditsObject = new CreditsObject(_font, text);
			creditsObject.IsMovable = true;
			creditsObject.Position = new Vector2(960f, 1080f + 50f * Options.FontScale);
			creditsObject.DestroyOutOfView = OutOfFrame.OnTop;
			creditsObject.SetVelocity(0f, -70f);
			creditsObject.SetScale(scale);
			MyLayer.AddGameObject(creditsObject);
			_lineNumber++;
		}
		if (_updateNumber > 0 && ((!GameCore.Console.IsActive && GameInput.SystemGetPressedKey() != Keys.None) || GameInput.SystemGPButtonPressedAll(Buttons.X) || GameInput.SystemGPButtonPressedAll(Buttons.B) || GameInput.SystemGPButtonPressedAll(Buttons.Back) || GameInput.SystemGPButtonPressedAll(Buttons.Start) || GameInput.MousePressed(GameInput.MouseButton.Left) || GameInput.MousePressed(GameInput.MouseButton.Right) || GameInput.MousePressed(GameInput.MouseButton.Middle)))
		{
			Hide();
		}
		if (State == CreditsStateEnum.FadeOut && Utils.ChangeValueToTarget(ref _alphaFactor, 0f, 3f * elapsedTime))
		{
			SetState(CreditsStateEnum.Hidden);
		}
		float num = 450f;
		foreach (GameObject gameObject in MyLayer.GameObjectList)
		{
			if (gameObject.WorldPosition.Y < num)
			{
				gameObject.Destroy();
			}
			else if (gameObject.WorldPosition.Y < num + 100f)
			{
				gameObject.Alpha = (byte)Math.Max(0f, 255f * (gameObject.WorldPosition.Y - num) / 100f * _alphaFactor);
			}
			else
			{
				gameObject.Alpha = (byte)Math.Max(0f, 255f * _alphaFactor);
			}
		}
		_updateNumber++;
	}

	public void SetState(CreditsStateEnum state)
	{
		State = state;
		if (State == CreditsStateEnum.Hidden)
		{
			_alphaFactor = 0f;
			MyLayer.Clear();
		}
	}

	public void Show()
	{
		_lineNumber = 0;
		_timer = 0f;
		_updateNumber = 0;
		_alphaFactor = 1f;
		MyLayer?.Clear();
		GameInput.CancelMousePressed();
		GameInput.CancelButtonPressed();
		GameInput.CancelKeyPressed();
		GameInput.ClearLastKeyboadState();
		SetState(CreditsStateEnum.Visible);
	}

	public void Hide()
	{
		SetState(CreditsStateEnum.FadeOut);
		GameCore.PlaySound("menu_yesno_hide");
	}
}
