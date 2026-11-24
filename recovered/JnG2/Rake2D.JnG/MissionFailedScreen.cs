using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;

namespace Rake2D.JnG;

public class MissionFailedScreen
{
	private GameCore _gameCore;

	public GameLevel Level;

	private float _flashAlpha;

	private float _fadeAlpha;

	private float _textAlphaFactor;

	private float _startSoundVolumeFactor;

	private float _startMusicVolumeFactor;

	private bool _exitToShop;

	private BMFont _font;

	private SpriteBatch _spriteBatch;

	public MissionScreenStateEnum State { get; private set; }

	public MissionFailedScreen(GameCore gameCore)
	{
		_gameCore = gameCore;
		_font = GameCore.GetBMFont("shop_font");
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		State = MissionScreenStateEnum.Hidden;
		Init();
	}

	public void Init()
	{
		GameLevel gameLevel = GameCore.GameLevel;
		Level = new GameLevel();
		Level.Init();
		GameCore.GameLevel = Level;
		if (!Level.LoadFromFile("level/shop/mission_failed/level.txt"))
		{
			GameCore.LogErr("Unable to load mission failed level");
			Level = null;
		}
		GameCore.GameLevel = gameLevel;
	}

	public void SetState(MissionScreenStateEnum state)
	{
		switch (state)
		{
		case MissionScreenStateEnum.FlashToWhite:
			_flashAlpha = 0f;
			_fadeAlpha = 0f;
			GameJnG.CancelFinishLevel();
			_startSoundVolumeFactor = GameCore.SoundSystem.SoundVolumeFactor;
			_startMusicVolumeFactor = GameCore.SoundSystem.MusicVolumeFactor;
			break;
		case MissionScreenStateEnum.Showing:
			_gameCore.SetTimeFactor(1f);
			GameCore.GameLevel.Clear();
			GameCore.SoundSystem.StopAll();
			if (Level != null)
			{
				GameLevel gameLevel = GameCore.GameLevel;
				GameCore.GameLevel = Level;
				Level.Reload(null);
				Level.Camera.Position = Vector2.Zero;
				GameCore.GameLevel = gameLevel;
			}
			GameCore.SoundSystem.MusicVolumeFactor = 1f;
			GameCore.SoundSystem.SoundVolumeFactor = 1f;
			break;
		case MissionScreenStateEnum.Hidden:
			GameCore.SoundSystem.StopMusic();
			if (_exitToShop)
			{
				if (GameJnG.IsFirstLevelTry)
				{
					GameJnG.LoadingScreen.ShowToMainMenu();
				}
				else
				{
					GameJnG.LoadingScreen.Show(null, fadeToShop: true);
				}
			}
			else
			{
				GameCore.SetGCPhase(GCPhase.Loading);
				GameCore.GameLevel.Reload(null, bUseChecpoint: true);
				GameCore.SetGCPhase(GCPhase.Game);
			}
			GameCore.SoundSystem.SoundVolumeFactor = 1f;
			GameCore.SoundSystem.MusicVolumeFactor = 1f;
			break;
		}
		State = state;
	}

	public void SetHidden()
	{
		State = MissionScreenStateEnum.Hidden;
	}

	public void Update(float elapsedTime)
	{
		switch (State)
		{
		case MissionScreenStateEnum.FlashToWhite:
			if (Utils.ChangeValueToTarget(ref _flashAlpha, 255f, 400f * elapsedTime))
			{
				GameJnG.ClearLevelAchievedValues();
				SetState(MissionScreenStateEnum.Showing);
			}
			else
			{
				float num = (255f - _flashAlpha) / 255f;
				GameCore.SoundSystem.SoundVolumeFactor = _startSoundVolumeFactor * num;
				GameCore.SoundSystem.MusicVolumeFactor = _startMusicVolumeFactor * num;
			}
			break;
		case MissionScreenStateEnum.Showing:
			if (Utils.ChangeValueToTarget(ref _flashAlpha, 0f, 400f * elapsedTime))
			{
				SetState(MissionScreenStateEnum.Visible);
			}
			break;
		case MissionScreenStateEnum.Hidding:
		{
			if (Utils.ChangeValueToTarget(ref _fadeAlpha, 255f, 400f * elapsedTime))
			{
				SetState(MissionScreenStateEnum.Hidden);
				break;
			}
			float num = (255f - _fadeAlpha) / 255f;
			GameCore.SoundSystem.SoundVolumeFactor = _startSoundVolumeFactor * num;
			GameCore.SoundSystem.MusicVolumeFactor = _startMusicVolumeFactor * num;
			break;
		}
		case MissionScreenStateEnum.Visible:
			_flashAlpha = 0f;
			_fadeAlpha = 0f;
			_textAlphaFactor += elapsedTime * 5f;
			GameCore.UpdateHeroes = false;
			if (!GameCore.Console.IsActive && (GameInput.AnySystemKeyPressed() || GameInput.AnyGamepadButtonPressedAll() || GameInput.AnyMouseButtonPressed()))
			{
				_exitToShop = false;
				SetState(MissionScreenStateEnum.Hidding);
			}
			break;
		case MissionScreenStateEnum.Hidden:
			return;
		}
		if (Level != null && State > MissionScreenStateEnum.FlashToWhite)
		{
			GameLevel gameLevel = GameCore.GameLevel;
			GameCore.GameLevel = Level;
			Level.Camera.SetViewPort(GameCore.MainViewPort);
			Level.Update(elapsedTime);
			GameCore.GameLevel = gameLevel;
		}
	}

	public void Show()
	{
		SetState(MissionScreenStateEnum.FlashToWhite);
	}

	public void Hide()
	{
		SetState(MissionScreenStateEnum.Hidding);
	}

	public void Draw()
	{
		if (Level != null && State > MissionScreenStateEnum.FlashToWhite)
		{
			GameLevel gameLevel = GameCore.GameLevel;
			GameCore.GameLevel = Level;
			GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.LevelRenderTarget);
			Level.Draw();
			GameCore.GameLevel = gameLevel;
		}
		if (State > MissionScreenStateEnum.FlashToWhite)
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			float alphaFactor = 0.6f - Utils.Cos(_textAlphaFactor) * 0.4f;
			_font.Write(_spriteBatch, TextBundle.GetText("press_any_key_to_continue"), 960f, 985f, Vector2.One, new Color(255, 255, 255), TextAlign.Center, 0, alphaFactor);
			_font.Write(_spriteBatch, TextBundle.GetText("mission_failed"), 960f, 95f, new Vector2(2f), new Color(255, 0, 0), TextAlign.Center);
			_spriteBatch.End();
		}
		if (_flashAlpha > 0f)
		{
			GameCore.TriangleBatch.Begin(BlendState.Additive, GameCore.ViewPortMatrix);
			GameCore.TriangleBatch.DrawRect(new Vector2(0f, 0f), new Vector2(1920f, 1080f), new Color((int)_flashAlpha, (int)_flashAlpha, (int)_flashAlpha, (int)_flashAlpha));
			GameCore.TriangleBatch.End();
		}
		if (_fadeAlpha > 0f)
		{
			GameCore.TriangleBatch.Begin(BlendState.AlphaBlend, GameCore.ViewPortMatrix);
			GameCore.TriangleBatch.DrawRect(new Vector2(0f, 0f), new Vector2(1920f, 1080f), new Color(0, 0, 0, (int)_fadeAlpha));
			GameCore.TriangleBatch.End();
		}
	}
}
