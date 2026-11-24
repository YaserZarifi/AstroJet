using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.JnG;

public class LoadingScreen
{
	public MissionScreenStateEnum State;

	private GameCore _gameCore;

	private SpriteBatch _spriteBatch;

	private SpriteRenderer _spriteRenderer;

	private SpriteAtlas _defaultAtlas;

	private LevelData _levelData;

	private float _fadePercentage;

	private float _targetFadePercentage;

	private float _startSoundVolumeFactor;

	private float _startMusicVolumeFactor;

	private bool _fadeToShop;

	private bool _fadeToMainMenu;

	private bool _ignoreFrame;

	public bool IsFadeToShop => _fadeToShop;

	public LoadingScreen(GameCore gameCore)
	{
		_gameCore = gameCore;
		_defaultAtlas = GameCore.GetSpriteAtlas("loading_screen_00");
		_spriteBatch = new SpriteBatch(_gameCore.GraphicsDevice);
		_spriteRenderer = GameCore.Allocator.GetSpriteRenderer(null);
	}

	public void Show(LevelData levelData, bool fadeToShop)
	{
		_levelData = levelData;
		GameJnG.IsTestRangeEnabled = false;
		State = MissionScreenStateEnum.Showing;
		_fadePercentage = 0f;
		_targetFadePercentage = 1f;
		_fadeToMainMenu = false;
		_fadeToShop = fadeToShop;
		if (levelData == null || levelData.LoadingAtlasId == null)
		{
			_spriteRenderer.SetAtlas(_defaultAtlas);
		}
		else
		{
			_spriteRenderer.SetAtlas(GameCore.GetSpriteAtlas(levelData.LoadingAtlasId));
			if (_spriteRenderer.Atlas == null)
			{
				_spriteRenderer.SetAtlas(_defaultAtlas);
			}
		}
		_startSoundVolumeFactor = GameCore.SoundSystem.SoundVolumeFactor;
		_startMusicVolumeFactor = GameCore.SoundSystem.MusicVolumeFactor;
	}

	public void ShowToMainMenu()
	{
		Show(null, fadeToShop: false);
		_fadeToMainMenu = true;
	}

	public void Hide()
	{
		if (State != MissionScreenStateEnum.Hidding && State != MissionScreenStateEnum.Hidden)
		{
			_targetFadePercentage = 0f;
			State = MissionScreenStateEnum.Hidding;
			GameCore.SoundSystem.SoundVolumeFactor = 1f;
			GameCore.SoundSystem.MusicVolumeFactor = 1f;
		}
	}

	public void Update(float elapsedTime)
	{
		if (_ignoreFrame)
		{
			_ignoreFrame = false;
			return;
		}
		if (State == MissionScreenStateEnum.Showing)
		{
			GameCore.SoundSystem.SoundVolumeFactor = 1f - _startSoundVolumeFactor * _fadePercentage;
			GameCore.SoundSystem.MusicVolumeFactor = 1f - _startMusicVolumeFactor * _fadePercentage;
		}
		if (!Utils.ChangeValueToTarget(ref _fadePercentage, _targetFadePercentage, 2f * elapsedTime))
		{
			return;
		}
		switch (State)
		{
		case MissionScreenStateEnum.Hidding:
			State = MissionScreenStateEnum.Hidden;
			if (_fadeToMainMenu)
			{
				GameCore.GameLevel.ClearAll();
				GameJnG.MainMenu.Show();
			}
			else if (_fadeToShop)
			{
				GameJnG.ShopMenu.Show();
			}
			break;
		case MissionScreenStateEnum.Showing:
			State = MissionScreenStateEnum.Visible;
			break;
		case MissionScreenStateEnum.Visible:
			GameCore.SoundSystem.StopAll();
			_gameCore.SetTimeFactor(1f);
			if (_levelData != null && !GameCore.LoadGameLevel(_levelData, bAllowLowerStage: false))
			{
				_fadeToShop = true;
			}
			if (!_fadeToMainMenu && (_levelData == null || _fadeToShop))
			{
				GameCore.LoadGameLevel(GameJnG.Campaign.GetShopLevelFileName(GameJnG.StageNumber), null);
				GameCore.Editor.SetLevel(GameCore.GameLevel);
				_fadeToShop = true;
			}
			_ignoreFrame = true;
			Hide();
			break;
		}
	}

	public void Draw()
	{
		_gameCore.GraphicsDevice.SetRenderTarget(GameCore.ScreenRenderTarget);
		Color color = new Color(255, 255, 255, (int)(255f * _fadePercentage));
		_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
		Transformation transform = Transformation.CreateByPosition(960f, 540f);
		_spriteRenderer.Frame = 0;
		_spriteRenderer.Color = color;
		_spriteRenderer.Draw(transform, _spriteBatch, BlendModeEnum.Normal);
		_spriteBatch.End();
	}
}
