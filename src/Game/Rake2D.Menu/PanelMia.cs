using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class PanelMia : ShopMenuPanel
{
	private SpriteAtlas _atlasTopTV;

	private Button _buttonRestart;

	private Button _buttonExit;

	public bool IsAnyItemHighlited
	{
		get
		{
			if (!_buttonExit.IsSelected)
			{
				return _buttonRestart.IsSelected;
			}
			return true;
		}
	}

	public PanelMia(ShopMenu shopMenu)
		: base(shopMenu)
	{
		_atlasTopTV = GameCore.GetSpriteAtlas("shop_price_tv", logWhenNotFound: true);
		Init();
	}

	public void SetTexts()
	{
		_buttonRestart.Caption = TextBundle.GetText("button_restart");
		_buttonExit.Caption = TextBundle.GetText("button_exit");
	}

	public void CancelSelection()
	{
	}

	public override void CancelCursorSelection()
	{
		base.CancelCursorSelection();
		_buttonRestart.Unselect();
		_buttonExit.Unselect();
	}

	public override void Init()
	{
		base.Init();
		Position = new Vector2(2320f, 0f);
		_atlasFrame = GameCore.GetSpriteAtlas("shop_panel", logWhenNotFound: true);
		_atlasBackground = GameCore.GetSpriteAtlas("shop_panel_bgr", logWhenNotFound: true);
		_buttonRestart = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonRestart.ParentTransformation = this;
		_buttonRestart.HasPressEffectEnabled = true;
		_buttonRestart.OnClick += DoOnButtonRestartClick;
		_buttonRestart.Position = new Vector2(50f, 460f);
		_buttonExit = new Button(_shopMenu, ShopButtonType.Normal);
		_buttonExit.ParentTransformation = this;
		_buttonExit.HasPressEffectEnabled = true;
		_buttonExit.OnClick += DoOnButtonExitClick;
		_buttonExit.Position = new Vector2(50f, 500f);
	}

	public void DoOnButtonRestartClick(Button button)
	{
		_shopMenu.YesNoResetHeroProfiles.Show();
	}

	public void DoOnButtonExitClick(Button button)
	{
		_shopMenu.YesNoEndCampaign.Show();
	}

	public override void Show(float showDelay)
	{
		base.Show(showDelay);
		TargetPosition = new Vector2(1920f, 0f);
	}

	public override void Hide()
	{
		base.Hide();
		TargetPosition = new Vector2(2320f, 0f);
	}

	public void SelectDefaultButton()
	{
		_shopMenu.ChangeFocus(this);
		_shopMenu.BigShipSlotSet.CancelCursorSelection();
		_shopMenu.CancelCursorSelection();
		_buttonRestart.Select();
		_buttonExit.Unselect();
	}

	private void ParseInput()
	{
		if (_buttonRestart.IsSelected)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
			{
				_buttonRestart.Unselect();
				_buttonExit.Select();
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				_buttonRestart.Unselect();
				_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.equipment_6);
			}
		}
		else if (_buttonExit.IsSelected)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp) || _shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
			{
				_buttonExit.Unselect();
				_buttonRestart.Select();
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				_buttonExit.Unselect();
				_shopMenu.BigShipSlotSet.SetCursorSlotByType(HeroSlotEnum.equipment_6);
			}
		}
	}

	public override void Update(float elapsedTime)
	{
		if (State != ShopMenuState.Hidden)
		{
			base.Update(elapsedTime);
			if (_shopMenu.IsAnyPopupMenuVisible)
			{
				_buttonRestart.IsInputEnabled = false;
				_buttonExit.IsInputEnabled = false;
			}
			else
			{
				_buttonRestart.IsInputEnabled = true;
				_buttonExit.IsInputEnabled = true;
			}
			if (!_shopMenu.IsAnyPopupMenuVisible)
			{
				ParseInput();
			}
			_buttonRestart.Position = new Vector2(-165f, 370f + 220f * Options.FontScale);
			_buttonExit.Position = new Vector2(-165f, 420f + 220f * Options.FontScale);
			_buttonRestart.Update(elapsedTime);
			_buttonExit.Update(elapsedTime);
		}
	}

	public override void Draw()
	{
		if (State != ShopMenuState.Hidden)
		{
			_shopMenu.SpriteBatchBegin(bLight: true);
			_atlasBackground.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, this);
			_spriteRenderer.SetAtlas(_atlasFrame);
			for (int i = 0; i <= 4; i++)
			{
				_spriteRenderer.Frame = i;
				_spriteRenderer.Draw(this, _shopMenu.SpriteBatchLight, BlendModeEnum.Normal);
			}
			Transformation transformation = Transformation.CreateByPosition(Position + new Vector2(-160f, 45f));
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetGameModeName(GameJnG.GameMode).ToUpper(), transformation.Position.X, transformation.Position.Y - 11f * Options.FontScale, ShopMenu.FONT_SCALE_PRICE_TV, null, TextAlign.Center);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("mission_stat_title_mia"), transformation.Position.X, transformation.Position.Y + 100f, ShopMenu.FONT_SCALE_PRICE_TV, null, TextAlign.Center);
			string arg;
			string text;
			if (GameJnG.PlayerCount > 1)
			{
				text = TextBundle.GetText("mia_panel_desc_P1");
				arg = TextBundle.GetText($"hero_rank_{GameJnG.HeroList[1].GetRank()}").ToLowerLazy();
				text = text.Replace("|player_name_2|", $"{arg} {GameJnG.HeroList[1].PlayerName}");
			}
			else
			{
				text = TextBundle.GetText("mia_panel_desc_P1");
			}
			arg = TextBundle.GetText($"hero_rank_{GameJnG.HeroList[0].GetRank()}").ToLowerLazy();
			text = text.Replace("|player_name_1|", $"{arg} {GameJnG.HeroList[0].PlayerName}");
			_shopMenu.FontDefault.WrapWrite(_shopMenu.SpriteBatchLight, text, transformation.Position.X - 120f, transformation.Position.Y + 150f, 235, ShopMenu.FONT_SCALE_LABEL, null, 1f, (int)(20f * Options.FontScale));
			_buttonRestart.Draw(_shopMenu.SpriteBatchLight);
			_buttonExit.Draw(_shopMenu.SpriteBatchLight);
			_shopMenu.SpriteBatchEnd(bLight: true);
		}
	}
}
