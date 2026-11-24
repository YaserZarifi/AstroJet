using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;

namespace Rake2D.Menu.Atarix;

public class AtarixTron : AtarixContent
{
	public enum TronStateEnum
	{
		Menu,
		Play,
		Fail,
		Win,
		AutoHacking
	}

	public static int GAME_W = 38;

	public static int GAME_H = 18;

	public TronStateEnum TronState;

	private List<TronRect> _pointList;

	private TronSnake _snake;

	internal SpriteBatch SpriteBatch;

	internal Vector2 GamePosition;

	internal SpriteAtlas Atlas;

	private HeroItemData _crateDebrisData;

	private float _hackTimer;

	private float _maxHackTime;

	private bool _autohackWin;

	public AtarixTron(Atarix atarix, ShopMenu shopMenu)
		: base(atarix, shopMenu)
	{
		_snake = new TronSnake(this);
		Atlas = GameCore.GetSpriteAtlas("atarix_tron");
		_crateDebrisData = GameCore.GetHeroItemData("other/crate_debris");
		_menuItemList = new List<string>();
		_menuOffsetY = 132;
		for (int i = 0; i < 3; i++)
		{
			_menuItemList.Add("---");
		}
	}

	public override void RegisterEvents()
	{
		_atarix.OnUpButtonClick += OnUpButtonClick;
		_atarix.OnDownButtonClick += OnDownButtonClick;
		_atarix.OnLeftButtonClick += OnLeftButtonClick;
		_atarix.OnRightButtonClick += OnRightButtonClick;
		_atarix.OnActionButtonClick += OnActionButtonClick;
		_atarix.OnOffButtonClick += OnOffButtonClick;
	}

	protected override void OnMenuItemClick(int menuItem)
	{
		base.OnMenuItemClick(menuItem);
		if (_selectedMenuItemIndex == 1)
		{
			SetState(TronStateEnum.AutoHacking);
		}
		else if (_selectedMenuItemIndex == 2)
		{
			OnOffButtonClick();
		}
		else
		{
			SetState(TronStateEnum.Play);
		}
	}

	private void OnActionButtonClick()
	{
		if (TronState == TronStateEnum.Menu)
		{
			OnMenuItemClick(_selectedMenuItemIndex);
		}
		else if (TronState == TronStateEnum.Fail || TronState == TronStateEnum.Win)
		{
			OnOffButtonClick();
		}
	}

	private void OnOffButtonClick()
	{
		if (TronState == TronStateEnum.Play)
		{
			SetState(TronStateEnum.Fail);
		}
		else if (TronState != TronStateEnum.AutoHacking)
		{
			_atarix.Hide();
		}
	}

	private void OnUpButtonClick()
	{
		if (TronState == TronStateEnum.Menu)
		{
			if (_selectedMenuItemIndex > 0)
			{
				_selectedMenuItemIndex--;
			}
			else
			{
				_selectedMenuItemIndex = _menuItemList.Count - 1;
			}
			GameCore.PlaySound("atarix_menu");
		}
	}

	private void OnRightButtonClick()
	{
	}

	private void OnDownButtonClick()
	{
		if (TronState == TronStateEnum.Menu)
		{
			if (_selectedMenuItemIndex < _menuItemList.Count - 1)
			{
				_selectedMenuItemIndex++;
			}
			else
			{
				_selectedMenuItemIndex = 0;
			}
		}
		GameCore.PlaySound("atarix_menu");
	}

	private void OnLeftButtonClick()
	{
	}

	public override void SetTexts()
	{
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			_menuItemList[i] = TextBundle.GetText($"atarix_hack_menu_item_{i}");
		}
	}

	public void RemovePoint(TronRect pointRect)
	{
		_pointList.Remove(pointRect);
		GameCore.PlaySound("tron_point");
		if (_pointList.Count == 0)
		{
			SetState(TronStateEnum.Win);
			GameJnG.AtarixAutoHackChance += 10;
			if (GameJnG.AtarixAutoHackChance > 100)
			{
				GameJnG.AtarixAutoHackChance = 100;
			}
		}
	}

	public TronRect GetRectAtPos(int x, int y)
	{
		foreach (TronRect point in _pointList)
		{
			if (x >= point.X && x <= point.X + 1 && y >= point.Y && y <= point.Y + 1)
			{
				return point;
			}
		}
		return null;
	}

	public TronRect.RectTypeEnum CheckPosition(int x, int y)
	{
		if (x < 0 || y < 0 || x >= GAME_W || y >= GAME_H)
		{
			return TronRect.RectTypeEnum.Wall;
		}
		if (_snake.IsOnPosition(x, y))
		{
			return TronRect.RectTypeEnum.Snake;
		}
		if (GetRectAtPos(x, y) != null)
		{
			return TronRect.RectTypeEnum.Point;
		}
		return TronRect.RectTypeEnum.None;
	}

	public override void Show()
	{
		base.Show();
		_snake.Build();
		SetState(TronStateEnum.Menu);
		PlacePoints();
	}

	private void PlacePoints()
	{
		_pointList = new List<TronRect>();
		for (int i = 0; i < 5; i++)
		{
			int random;
			int random2;
			do
			{
				random = Utils.GetRandom(1, GAME_W - 3);
				random2 = Utils.GetRandom(1, GAME_H - 3);
			}
			while (CheckPosition(random, random2) != TronRect.RectTypeEnum.None || (random >= GAME_W / 2 - 2 && random <= GAME_W / 2 + 1 && random2 >= GAME_H / 2 - 2 && random2 <= GAME_H / 2 + 1));
			TronRect tronRect = new TronRect(this);
			tronRect.X = random;
			tronRect.Y = random2;
			tronRect.Frame = 7;
			tronRect.Type = TronRect.RectTypeEnum.Point;
			_pointList.Add(tronRect);
		}
	}

	public void SetState(TronStateEnum state)
	{
		TronState = state;
		switch (state)
		{
		case TronStateEnum.Play:
			_maxHackTime = Math.Max(6f, 14f - (float)GameJnG.AtarixAutoHackChance / 20f);
			_hackTimer = _maxHackTime;
			GameCore.PlaySound("tron_start");
			break;
		case TronStateEnum.AutoHacking:
			_hackTimer = 0.4f;
			_autohackWin = GameJnG.AtarixAutoHackChance >= Utils.GetRandom(1, 100);
			GameCore.PlaySound("tron_start");
			break;
		case TronStateEnum.Fail:
			if (_shopMenu.PanelInfo.TVItem != null && _shopMenu.PanelInfo.TVItem.Place == ItemPlaceEnum.InCargo)
			{
				int cargoIndex = _shopMenu.PanelInfo.TVItem.CargoIndex;
				GameJnG.Cargo.RemoveItem(_shopMenu.PanelInfo.TVItem);
				HeroItem item = new HeroItem(_crateDebrisData);
				GameJnG.Cargo.SetItem(cargoIndex, item);
				_shopMenu.PanelCargo.SelectItem(item);
			}
			GameCore.PlaySound("tron_fail");
			break;
		case TronStateEnum.Win:
			if (_shopMenu.PanelInfo.TVItem != null && _shopMenu.PanelInfo.TVItem.Place == ItemPlaceEnum.InCargo)
			{
				_shopMenu.PanelCargo.SelectItem(_shopMenu.PanelInfo.TVItem.DoGetItem());
				TotalStats.CargoUnlockCount++;
				GameCore.SteamManager.SetAndStoreAll();
			}
			GameCore.PlaySound("tron_win");
			break;
		case TronStateEnum.Menu:
			if (GameJnG.AtarixAutoHackChance >= 100)
			{
				_selectedMenuItemIndex = 1;
			}
			else
			{
				_selectedMenuItemIndex = 0;
			}
			break;
		}
	}

	private void ParseGameInput()
	{
		if (GameInput.KeyDown(GameInput.KeyTypeEnum.MoveUp))
		{
			_snake.ChangeMoveDirection(0);
		}
		else if (GameInput.KeyDown(GameInput.KeyTypeEnum.MoveRight))
		{
			_snake.ChangeMoveDirection(1);
		}
		else if (GameInput.KeyDown(GameInput.KeyTypeEnum.MoveDown))
		{
			_snake.ChangeMoveDirection(2);
		}
		else if (GameInput.KeyDown(GameInput.KeyTypeEnum.MoveLeft))
		{
			_snake.ChangeMoveDirection(3);
		}
	}

	private void UpdateAutoHacking(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _hackTimer, 0f, elapsedTime))
		{
			_hackTimer = 0.4f;
			if (_pointList.Count < 3 && !_autohackWin)
			{
				SetState(TronStateEnum.Fail);
				_hackTimer = 999f;
			}
			else if (_pointList.Count > 0)
			{
				_pointList.RemoveAt(0);
			}
			if (_pointList.Count == 0)
			{
				SetState(TronStateEnum.Win);
				_hackTimer = 999f;
			}
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (TronState == TronStateEnum.Play)
		{
			ParseGameInput();
		}
		GamePosition = _atarix.Position + new Vector2(-185f, -80f);
		switch (TronState)
		{
		case TronStateEnum.Play:
			if (Utils.ChangeValueToTarget(ref _hackTimer, 0f, elapsedTime))
			{
				SetState(TronStateEnum.Fail);
			}
			_snake.Update(elapsedTime);
			break;
		case TronStateEnum.AutoHacking:
			UpdateAutoHacking(elapsedTime);
			break;
		case TronStateEnum.Menu:
			UpdateMouseInMenu();
			break;
		case TronStateEnum.Fail:
		case TronStateEnum.Win:
			break;
		}
	}

	private void DrawResultText(string id, Color color)
	{
		_shopMenu.SpriteBatchEnd(bLight: false);
		_shopMenu.TriangleBatch.Begin(BlendState.NonPremultiplied, GameCore.ViewPortMatrix);
		_shopMenu.TriangleBatch.DrawRect(GamePosition.X - 15f, GamePosition.Y - 15f, 420f, 220f, new Color(0, 0, 0, 150));
		_shopMenu.TriangleBatch.End();
		_shopMenu.SpriteBatchBegin(bLight: false);
		string text = TextBundle.GetText(id);
		Vector2 vector = new Vector2(_atarix.Position.X, _atarix.Position.Y + 122f);
		_font.Write(SpriteBatch, text, vector.X + 4f, vector.Y + 4f, new Vector2(0.8f), new Color(0, 0, 0, 150), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
		_font.Write(SpriteBatch, text, vector.X, vector.Y, new Vector2(0.8f), color, TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
	}

	private void DrawMenu()
	{
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			Transformation transformation = new Transformation(_atarix);
			transformation.Position.Y = _atarix.Position.Y + (float)_menuOffsetY + (float)(i * 30);
			transformation.Scale = new Vector2(1.5f, 1.6f);
			string text = ((i != 1) ? _menuItemList[i] : _menuItemList[i].Replace("|hack_chance|", $"{GameJnG.AtarixAutoHackChance}"));
			if (i == _selectedMenuItemIndex)
			{
				_atarix.Atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, new Color(0, 200, 0, (int)_atarix.Color.A), transformation, BlendModeEnum.Normal);
				_font.Write(_shopMenu.SpriteBatchNoLight, text, transformation.Position.X, transformation.Position.Y - 9f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 0, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			}
			else
			{
				_font.Write(_shopMenu.SpriteBatchNoLight, text, transformation.Position.X, transformation.Position.Y - 9f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 180, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			}
		}
	}

	private void DrawDiodes()
	{
		Transformation transformation = Transformation.CreateByPosition(GamePosition);
		transformation.Position.X += 96f;
		transformation.Position.Y += 250f;
		for (int i = 0; i < 5; i++)
		{
			Color color = ((i >= 5 - _pointList.Count) ? Color.Red : new Color(0, 255, 0));
			Atlas.DrawFrame(SpriteBatch, 9, color, transformation, BlendModeEnum.Additive);
			transformation.Position.X += 45f;
		}
		if (TronState == TronStateEnum.Play)
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_tron_hacking"), _atarix.Position.X, transformation.Position.Y - 40f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 255, 255), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
		}
		else if (TronState == TronStateEnum.AutoHacking)
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_tron_auto_hacking"), _atarix.Position.X, transformation.Position.Y - 40f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 150, 255), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
		}
	}

	private void DrawAutoHacking()
	{
	}

	private void DrawTimer()
	{
		float num = _hackTimer / _maxHackTime;
		Transformation transformation = new Transformation(_atarix);
		transformation.Position.Y -= 118f;
		transformation.Scale = new Vector2(num, 0.8f);
		Color color;
		if (num < 0.3f)
		{
			color = new Color(255, 0, 0, (int)_atarix.Color.A);
		}
		else
		{
			color = ((!(num < 0.5f)) ? new Color(0, 255, 0, (int)_atarix.Color.A) : new Color(255, 255, 0, (int)_atarix.Color.A));
		}
		_atarix.Atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, transformation, BlendModeEnum.Normal);
		if (num > 0f)
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_tron_time_to_fialure"), _atarix.Position.X, _atarix.Position.Y - 150f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 255, 255), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
		}
		else
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_tron_time_out"), _atarix.Position.X, _atarix.Position.Y - 135f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 0, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
		}
	}

	public override void Draw()
	{
		base.Draw();
		SpriteBatch = _shopMenu.SpriteBatchNoLight;
		Transformation transformation = Transformation.CreateByPosition(GamePosition);
		transformation.Position.X -= 15f;
		transformation.Position.Y -= 15f;
		Atlas.DrawFrame(SpriteBatch, 0, Color.White, transformation, BlendModeEnum.Normal);
		transformation.Position.X = _atarix.Position.X;
		transformation.Position.Y -= 90f;
		Atlas.DrawFrame(SpriteBatch, 8, Color.White, transformation, BlendModeEnum.Normal);
		foreach (TronRect point in _pointList)
		{
			point.Draw();
		}
		if (TronState == TronStateEnum.Menu)
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_tron_connected"), _atarix.Position.X, _atarix.Position.Y - 140f, Atarix.FONT_SCALE_DEFAULT, new Color(255, 255, 255), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			DrawMenu();
		}
		else if (TronState == TronStateEnum.AutoHacking)
		{
			DrawDiodes();
			DrawAutoHacking();
		}
		else
		{
			if (_hackTimer < 999f)
			{
				_snake.Draw();
				DrawTimer();
			}
			DrawDiodes();
		}
		if (TronState == TronStateEnum.Fail)
		{
			DrawResultText("atarix_tron_fail", new Color(255, 0, 0));
		}
		else if (TronState == TronStateEnum.Win)
		{
			DrawResultText("atarix_tron_win", new Color(0, 255, 0));
		}
	}
}
