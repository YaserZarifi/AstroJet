using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class ScreenLeaderboard
{
	public ScreenLeaderBoardStateEnum State;

	public List<LeaderboardEntry> EntryList;

	public LeaderboardEntry MyEntry;

	private GameModeEnum _gameMode;

	private int _playerCount;

	private bool _fadeAll;

	private ShopMenu _shopMenu;

	private MainMenu _mainMenu;

	private float _alphaFactor;

	private SpriteAtlas _atlasFace;

	private Vector2 TablePosition;

	private Button _buttonStandard1;

	private Button _buttonStandard2;

	private Button _buttonHardcore1;

	private Button _buttonHardcore2;

	private Button _buttonExit;

	private List<Button> _topButtonList;

	public ScreenLeaderboard(ShopMenu shopMenu, MainMenu mainMenu)
	{
		_shopMenu = shopMenu;
		_mainMenu = mainMenu;
		_topButtonList = new List<Button>();
		_buttonStandard1 = new Button(_shopMenu, ShopButtonType.Tab);
		_buttonStandard1.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonStandard1.OnClick += OnStandard1ButtonClick;
		_buttonStandard1.Position = new Vector2(680f, 120f);
		_topButtonList.Add(_buttonStandard1);
		_buttonStandard2 = new Button(_shopMenu, ShopButtonType.Tab);
		_buttonStandard2.CaptionScale = _buttonStandard1.CaptionScale;
		_buttonStandard2.OnClick += OnStandard2ButtonClick;
		_buttonStandard2.Position = _buttonStandard1.Position + new Vector2(190f, 0f);
		_topButtonList.Add(_buttonStandard2);
		_buttonHardcore1 = new Button(_shopMenu, ShopButtonType.Tab);
		_buttonHardcore1.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonHardcore1.OnClick += OnHardcore1ButtonClick;
		_buttonHardcore1.Position = _buttonStandard2.Position + new Vector2(190f, 0f);
		_topButtonList.Add(_buttonHardcore1);
		_buttonHardcore2 = new Button(_shopMenu, ShopButtonType.Tab);
		_buttonHardcore2.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonHardcore2.OnClick += OnHardcore2ButtonClick;
		_buttonHardcore2.Position = _buttonHardcore1.Position + new Vector2(190f, 0f);
		_topButtonList.Add(_buttonHardcore2);
		_buttonExit = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonExit.CaptionScale = ShopMenu.FONT_SCALE_BUTTON_BIG;
		_buttonExit.OnClick += OnExitButtonClick;
		_buttonExit.Position = new Vector2(960f, 1035f);
	}

	private void OnExitButtonClick(Button button)
	{
		Hide();
	}

	private void OnStandard1ButtonClick(Button button)
	{
		Show(GameModeEnum.Standard, 1, bFadeAll: false);
	}

	private void OnStandard2ButtonClick(Button button)
	{
		Show(GameModeEnum.Standard, 2, bFadeAll: false);
	}

	private void OnHardcore1ButtonClick(Button button)
	{
		Show(GameModeEnum.HardCore, 1, bFadeAll: false);
	}

	private void OnHardcore2ButtonClick(Button button)
	{
		Show(GameModeEnum.HardCore, 2, bFadeAll: false);
	}

	public void SetTexts()
	{
		_buttonStandard1.Caption = TextBundle.GetText("button_leaderboard_standard_1P");
		_buttonStandard2.Caption = TextBundle.GetText("button_leaderboard_standard_2P");
		_buttonHardcore1.Caption = TextBundle.GetText("button_leaderboard_hardcore_1P");
		_buttonHardcore2.Caption = TextBundle.GetText("button_leaderboard_hardcore_2P");
		_buttonExit.Caption = TextBundle.GetText("button_back");
	}

	private void SetState(ScreenLeaderBoardStateEnum state)
	{
		State = state;
		switch (state)
		{
		case ScreenLeaderBoardStateEnum.WaitFade:
			_alphaFactor = 0f;
			break;
		case ScreenLeaderBoardStateEnum.FadeIn:
			_alphaFactor = 0f;
			GameCore.PlaySound("menu_yesno_show");
			break;
		case ScreenLeaderBoardStateEnum.Visible:
			_alphaFactor = 1f;
			break;
		case ScreenLeaderBoardStateEnum.Hidden:
			_alphaFactor = 0f;
			foreach (LeaderboardEntry entry in EntryList)
			{
				entry.Destroy();
			}
			EntryList = null;
			break;
		}
	}

	public void Show(GameModeEnum gameMode, int playerCount, bool bFadeAll)
	{
		_gameMode = gameMode;
		_playerCount = playerCount;
		_fadeAll = bFadeAll;
		if (EntryList != null)
		{
			foreach (LeaderboardEntry entry in EntryList)
			{
				entry.Destroy();
			}
		}
		EntryList = new List<LeaderboardEntry>();
		MyEntry = new LeaderboardEntry();
		if (GameCore.SteamManager.IsInitialized)
		{
			GameCore.SteamManager.DownloadTopScoreLeaderboard(EntryList, MyEntry, _gameMode, _playerCount);
			SetState(ScreenLeaderBoardStateEnum.WaitFade);
		}
		else
		{
			SetState(ScreenLeaderBoardStateEnum.FadeIn);
		}
		_atlasFace = GameCore.GetSpriteAtlas("profile_face");
		_mainMenu.SetHintText("");
		TablePosition = new Vector2(250f - 20f * Options.FontScale, 210f);
		if (bFadeAll)
		{
			UnselectAll();
			_buttonExit.Select(bPlaySound: false);
		}
		GameCore.PlaySound("menu_yesno_hide");
	}

	private void UnselectAll()
	{
		foreach (Button topButton in _topButtonList)
		{
			topButton.Unselect();
		}
		_buttonExit.Unselect();
	}

	private void SelectTopButton()
	{
		foreach (Button topButton in _topButtonList)
		{
			topButton.IsPressed = false;
		}
		if (_gameMode == GameModeEnum.Standard)
		{
			if (_playerCount > 1)
			{
				_buttonStandard2.IsPressed = true;
			}
			else
			{
				_buttonStandard1.IsPressed = true;
			}
		}
		else if (_gameMode == GameModeEnum.HardCore)
		{
			if (_playerCount > 1)
			{
				_buttonHardcore2.IsPressed = true;
			}
			else
			{
				_buttonHardcore1.IsPressed = true;
			}
		}
	}

	private void Hide()
	{
		SetState(ScreenLeaderBoardStateEnum.FadeOut);
		GameCore.PlaySound("menu_yesno_hide");
	}

	private int GetTopButtonSelectedIndex()
	{
		for (int i = 0; i < _topButtonList.Count; i++)
		{
			if (_topButtonList[i].IsSelected)
			{
				return i;
			}
		}
		return -1;
	}

	private void ParseInput()
	{
		if (_shopMenu.KeyPressed(ShopMenuKeys.CancelAll))
		{
			Hide();
			GameCore.PlaySound("menu_yesno_hide");
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			if (_buttonExit.IsSelected || GetTopButtonSelectedIndex() < 0)
			{
				UnselectAll();
				_buttonStandard1.Select();
			}
			else
			{
				UnselectAll();
				_buttonExit.Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight) && !_buttonExit.IsSelected)
		{
			int topButtonSelectedIndex = GetTopButtonSelectedIndex();
			if (topButtonSelectedIndex < _topButtonList.Count - 1)
			{
				UnselectAll();
				_topButtonList[topButtonSelectedIndex + 1].Select();
			}
			else
			{
				UnselectAll();
				_topButtonList[0].Select();
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft) && !_buttonExit.IsSelected)
		{
			int topButtonSelectedIndex2 = GetTopButtonSelectedIndex();
			if (topButtonSelectedIndex2 > 0)
			{
				UnselectAll();
				_topButtonList[topButtonSelectedIndex2 - 1].Select();
			}
			else
			{
				UnselectAll();
				_topButtonList[_topButtonList.Count - 1].Select();
			}
		}
	}

	private void UpdateEntryLine(LeaderboardEntry entry, int number, float elapsedTime)
	{
		if (entry.MyObject != null)
		{
			if (number >= 10)
			{
				entry.MyObject.SetPosition(TablePosition + new Vector2(150f, 745f) + entry.MyObject.MyCamera.Position);
			}
			else
			{
				entry.MyObject.SetPosition(TablePosition + new Vector2(150f, 15 + number * 80) + entry.MyObject.MyCamera.Position);
			}
			GameCore.UseShopLightMaps = false;
			entry.MyObject.Alpha = (byte)(_alphaFactor * 255f);
			entry.MyObject.Update(elapsedTime);
			GameCore.UseShopLightMaps = true;
		}
	}

	public void Update(float elapsedTime)
	{
		ParseInput();
		switch (State)
		{
		case ScreenLeaderBoardStateEnum.FadeIn:
			if (EntryList != null && Utils.ChangeValueToTarget(ref _alphaFactor, 1f, 4f * elapsedTime))
			{
				SetState(ScreenLeaderBoardStateEnum.Visible);
			}
			break;
		case ScreenLeaderBoardStateEnum.FadeOut:
			if (Utils.ChangeValueToTarget(ref _alphaFactor, 0f, 4f * elapsedTime))
			{
				SetState(ScreenLeaderBoardStateEnum.Hidden);
			}
			break;
		}
		if (EntryList != null)
		{
			for (int i = 0; i < EntryList.Count; i++)
			{
				UpdateEntryLine(EntryList[i], i, elapsedTime);
			}
		}
		if (MyEntry != null)
		{
			UpdateEntryLine(MyEntry, 10, elapsedTime);
		}
		Color color = ((!_fadeAll) ? Color.White : new Color(255, 255, 255, (int)(255f * _alphaFactor)));
		foreach (Button topButton in _topButtonList)
		{
			topButton.Color = color;
			topButton.Update(elapsedTime);
		}
		_buttonExit.Color = color;
		_buttonExit.Update(elapsedTime);
		SelectTopButton();
	}

	public void DoOnLeaderboardDownloaded()
	{
		foreach (LeaderboardEntry entry in EntryList)
		{
			entry.Prepare(_mainMenu.Level.MainLayer);
		}
		SetState(ScreenLeaderBoardStateEnum.FadeIn);
	}

	public void DoOnLeaderboardForMyUserDownloaded()
	{
		MyEntry.Prepare(_mainMenu.Level.MainLayer);
	}

	private void DrawEntryLine(LeaderboardEntry entry, int rowNumber)
	{
		Color color = new Color(255, 255, 255, (int)(255f * _alphaFactor));
		Vector2 vector = ((rowNumber <= 9) ? (TablePosition + new Vector2(0f, rowNumber * 80)) : (TablePosition + new Vector2(0f, 730f)));
		Vector2 scale = ShopMenu.FONT_SCALE_LABEL * Options.FontScale;
		float num = 14f - 16f * Options.FontScale;
		float num2 = 1.2f - (float)rowNumber * 0.05f;
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, $"\\0{entry.Rank}.", vector.X, vector.Y + 5f - 10f * num2, new Vector2(num2), color, TextAlign.Right, 0, _alphaFactor);
		if (entry.Player2 != null)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1}", TextBundle.GetText("hero_stat_user_name"), entry.UserName), vector.X + 340f, vector.Y - 9f - 6f * Options.FontScale + num, scale, color, TextAlign.Left, 450, _alphaFactor);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1}, {2}", TextBundle.GetText("hero_stat_player_name_p2"), entry.Player2.MyHero.PlayerName, entry.Player1.MyHero.PlayerName), vector.X + 340f, vector.Y + 9f + 6f * Options.FontScale + num, scale, color, TextAlign.Left, 450, _alphaFactor);
		}
		else
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1}", TextBundle.GetText("hero_stat_user_name"), entry.UserName), vector.X + 340f, vector.Y - 9f - 6f * Options.FontScale + num, scale, color, TextAlign.Left, 450, _alphaFactor);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1}", TextBundle.GetText("hero_stat_player_name"), entry.Player1.MyHero.PlayerName), vector.X + 340f, vector.Y + 9f + 6f * Options.FontScale + num, scale, color, TextAlign.Left, 450, _alphaFactor);
		}
		StageData stageData = GameJnG.Campaign.GetStageData(entry.StageNumber);
		string arg = ((stageData == null) ? "XYZ" : TextBundle.GetText($"level_name_{stageData.LevelDataList[0].TextId ?? stageData.LevelDataList[0].Id}"));
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1} - {2}", TextBundle.GetText("hero_stat_difficulty"), entry.Difficulty, TextBundle.GetDifficultyName(entry.Difficulty)), vector.X + 800f, vector.Y - 9f - 6f * Options.FontScale + num, scale, color, TextAlign.Left, 400, _alphaFactor);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1} - {2}", TextBundle.GetText("hero_stat_stage"), entry.StageNumber, arg), vector.X + 800f, vector.Y + 9f + 6f * Options.FontScale + num, scale, color, TextAlign.Left, 400, _alphaFactor);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}: \\3{1}", TextBundle.GetText("hero_stat_score"), Utils.FormatMoney(entry.Score)), vector.X + 1200f + 50f * Options.FontScale, vector.Y + num, scale, color, TextAlign.Left, 0, _alphaFactor);
		if (entry.Player2 != null)
		{
			Transformation transformation = Transformation.CreateByPosition(vector + new Vector2(270f, 0f));
			transformation.Scale = new Vector2(0.4f);
			_atlasFace.DrawFrame(_shopMenu.SpriteBatchNoLight, entry.Player2.MyHero.FaceNumber, color, transformation, BlendModeEnum.Additive);
			transformation = Transformation.CreateByPosition(vector + new Vector2(305f, 20f));
			transformation.Scale = new Vector2(0.4f);
			_atlasFace.DrawFrame(_shopMenu.SpriteBatchNoLight, entry.Player1.MyHero.FaceNumber, color, transformation, BlendModeEnum.Additive);
		}
		else
		{
			Transformation transformation = Transformation.CreateByPosition(vector + new Vector2(300f, 10f));
			transformation.Scale = new Vector2(0.5f);
			_atlasFace.DrawFrame(_shopMenu.SpriteBatchNoLight, entry.Player1.MyHero.FaceNumber, color, transformation, BlendModeEnum.Additive);
		}
		if (entry.MyObject != null)
		{
			GameCore.EffectLight.SetLightMaps(bUseShopMaps: false);
			GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: false);
			GameCore.EffectLight.SetLightFactor(_mainMenu.Level.MainLayer, 1f);
			GameCore.EffectLightBasic.SetLightFactor(_mainMenu.Level.MainLayer, 1f);
			GameCore.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, GameCore.RasterizerState, GameCore.GetRenderEffect(_mainMenu.Level.MainLayer), GameCore.LevelMatrix);
			entry.MyObject.Draw(GameCore.SpriteBatch);
			GameCore.SpriteBatch.End();
			GameCore.EffectLight.SetLightMaps(bUseShopMaps: true);
			GameCore.EffectLightBasic.SetLightMaps(bUseShopMaps: true);
		}
		if (entry.IsMyUser)
		{
			color = new Color(0, 255, 0, (int)(200f * _alphaFactor));
			Transformation transformation = Transformation.CreateByPosition(960f, vector.Y + 2f);
			transformation.Scale = new Vector2(7f, 0.6f);
			_mainMenu.ProfileSelector.AtlasFrame.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, transformation, BlendModeEnum.Additive);
		}
	}

	public void Draw()
	{
		_ = TablePosition;
		_shopMenu.SpriteBatchBegin(bLight: true);
		_shopMenu.SpriteBatchBegin(bLight: false);
		Color value = new Color(255, 255, 255, (int)(255f * _alphaFactor));
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}", TextBundle.GetText("leaderboards_title")), 960f, 40f, new Vector2(1f), value, TextAlign.Center, 0, _fadeAll ? _alphaFactor : 1f);
		if (EntryList.Count > 0)
		{
			for (int i = 0; i < EntryList.Count; i++)
			{
				DrawEntryLine(EntryList[i], i);
				if (i == 8 && MyEntry.Rank > 10)
				{
					DrawEntryLine(MyEntry, 10);
					break;
				}
			}
		}
		else
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("\\0{0}", TextBundle.GetText("leaderboards_no_data")), 960f, 490f, new Vector2(0.6f * Options.FontScale), value, TextAlign.Center, 0, _alphaFactor);
		}
		foreach (Button topButton in _topButtonList)
		{
			topButton.Draw();
		}
		_buttonExit.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
		_shopMenu.SpriteBatchEnd(bLight: false);
	}
}
