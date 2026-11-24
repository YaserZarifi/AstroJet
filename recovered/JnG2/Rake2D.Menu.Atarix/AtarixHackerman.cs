using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rake2D.Font.BMFont;
using Rake2D.JnG;

namespace Rake2D.Menu.Atarix;

public class AtarixHackerman : AtarixContent
{
	protected enum HackermanStateEnum
	{
		Menu,
		ManualHacking,
		AutoHacking
	}

	private BMFont _fontDigital;

	private SpriteAtlas _atlas;

	private HackermanStateEnum _hackermanState;

	private int[] _digitArray;

	private float _digitTimer;

	private float _waitTimer;

	private float _hackTimer;

	private const float _maxHackTimer = 30f;

	private int _attemptCount;

	private int _guessedCount;

	public AtarixHackerman(Atarix atarix, ShopMenu shopMenu)
		: base(atarix, shopMenu)
	{
		_atlas = GameCore.GetSpriteAtlas("atarix_hackerman");
		_fontDigital = GameCore.GetBMFont("digital_fixed_font");
		_menuItemList = new List<string>();
		for (int i = 0; i < 2; i++)
		{
			_menuItemList.Add("---");
		}
		_digitArray = new int[3];
	}

	public override void RegisterEvents()
	{
		_atarix.OnUpButtonClick += OnUpButtonClick;
		_atarix.OnDownButtonClick += OnDownButtonClick;
		_atarix.OnActionButtonClick += OnActionButtonClick;
	}

	private void OnUpButtonClick()
	{
		if (_hackermanState == HackermanStateEnum.Menu)
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

	private void OnDownButtonClick()
	{
		if (_hackermanState == HackermanStateEnum.Menu)
		{
			if (_selectedMenuItemIndex < _menuItemList.Count - 1)
			{
				_selectedMenuItemIndex++;
			}
			else
			{
				_selectedMenuItemIndex = 0;
			}
			GameCore.PlaySound("atarix_menu");
		}
	}

	private void SetDigitTimer()
	{
		_digitTimer = Math.Max(0.1f, 0.75f - (float)_attemptCount * 0.08f);
	}

	private void OnActionButtonClick()
	{
		if (_hackermanState == HackermanStateEnum.Menu)
		{
			for (int i = 0; i < _digitArray.Length; i++)
			{
				_digitArray[i] = Utils.GetRandom(0, 9);
			}
			SetDigitTimer();
			_waitTimer = 0f;
			_hackTimer = 30f;
			_guessedCount = 0;
			_attemptCount = 0;
			if (_selectedMenuItemIndex == 1)
			{
				_hackermanState = HackermanStateEnum.AutoHacking;
			}
			else
			{
				_hackermanState = HackermanStateEnum.ManualHacking;
			}
		}
		else if (_hackermanState == HackermanStateEnum.ManualHacking)
		{
			_waitTimer = 0.5f;
			int num = 0;
			for (int j = 0; j < _digitArray.Length; j++)
			{
				int num2 = 0;
				for (int k = j + 1; k < _digitArray.Length; k++)
				{
					if (_digitArray[j] == _digitArray[k])
					{
						_digitArray[k] += 10;
						num2++;
					}
				}
				if (num2 > 0)
				{
					_digitArray[j] += 10;
					num++;
				}
			}
			if (num <= 0)
			{
				_hackTimer -= 5f;
			}
			else if (num > 1)
			{
				_guessedCount += 2;
			}
			else
			{
				_guessedCount++;
			}
		}
		_attemptCount++;
	}

	public override void SetTexts()
	{
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			_menuItemList[i] = TextBundle.GetText($"atarix_hack_menu_item_{i}");
		}
	}

	public override void Show()
	{
		State = AtarixContentStateEnum.Visible;
		_hackermanState = HackermanStateEnum.Menu;
		for (int i = 0; i < _digitArray.Length; i++)
		{
			_digitArray[i] = 0;
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (_hackermanState == HackermanStateEnum.Menu)
		{
			return;
		}
		if (_waitTimer > 0f)
		{
			if (Utils.ChangeValueToTarget(ref _waitTimer, 0f, elapsedTime))
			{
				for (int i = 0; i < _digitArray.Length; i++)
				{
					_digitArray[i] = Utils.GetRandom(0, 9);
				}
				SetDigitTimer();
			}
			return;
		}
		_hackTimer -= elapsedTime;
		if (_hackTimer <= 0f)
		{
			_hackTimer = 0f;
		}
		if (Utils.ChangeValueToTarget(ref _digitTimer, 0f, elapsedTime))
		{
			SetDigitTimer();
			for (int j = 0; j < _digitArray.Length; j++)
			{
				_digitArray[j] = Utils.GetRandom(0, 9);
			}
		}
	}

	private void DrawMenu()
	{
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			Transformation transformation = new Transformation(_atarix);
			transformation.Position.Y = _atarix.Position.Y + 50f + (float)(i * 30);
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

	private float GetHackTimePercentage()
	{
		return _hackTimer / 30f;
	}

	private void DrawDigits()
	{
		for (int i = 0; i < _digitArray.Length; i++)
		{
			float x = _atarix.Position.X - 50f + (float)(i * 40);
			float num = _atarix.Position.Y - 50f;
			string text;
			Color value;
			Vector2 scale;
			if (_digitArray[i] >= 10)
			{
				num -= 2f;
				text = $"{_digitArray[i] - 10}";
				value = new Color(10, 255, 0);
				scale = new Vector2(1.1f);
			}
			else
			{
				text = $"{_digitArray[i]}";
				if (_waitTimer > 0f)
				{
					num += 3f;
					value = new Color(255, 20, 0);
					scale = new Vector2(0.9f);
				}
				else
				{
					value = new Color(255, 200, 0);
					scale = new Vector2(1f);
				}
			}
			_fontDigital.Write(_shopMenu.SpriteBatchNoLight, "8", x, num, scale, new Color(70, 0, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			_fontDigital.Write(_shopMenu.SpriteBatchNoLight, text, x, num, scale, value, TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
		}
	}

	public override void Draw()
	{
		base.Draw();
		Transformation transformation = new Transformation(_atarix);
		transformation.Position.Y -= 160f;
		_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, _atarix.Color, transformation, BlendModeEnum.Normal);
		if (_hackermanState == HackermanStateEnum.Menu)
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_hackerman_connected"), _atarix.Position.X, _atarix.Position.Y - 100f, Atarix.FONT_SCALE_DEFAULT, new Color(200, 20, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			DrawMenu();
		}
		else
		{
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_hackerman_hacking"), _atarix.Position.X, _atarix.Position.Y - 100f, Atarix.FONT_SCALE_DEFAULT, new Color(200, 20, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_hackerman_timer"), _atarix.Position.X, _atarix.Position.Y + 50f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 220, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			transformation.Position.Y = _atarix.Position.Y + 90f;
			transformation.Scale = new Vector2(GetHackTimePercentage(), 1f);
			Color color = ((!(GetHackTimePercentage() < 0.3f)) ? new Color(0, 255, 0, (int)_atarix.Color.A) : new Color(255, 0, 0, (int)_atarix.Color.A));
			_atarix.Atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, transformation, BlendModeEnum.Normal);
			transformation = Transformation.CreateByPosition(_atarix.Position);
			transformation.Position.X -= 25f;
			transformation.Position.Y += 25f;
			for (int i = 0; i < 3; i++)
			{
				color = ((i >= _guessedCount) ? new Color(255, 0, 0) : new Color(0, 255, 0));
				_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, transformation, BlendModeEnum.Normal);
				transformation.Position.X += 25f;
			}
		}
		DrawDigits();
		_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_hackerman_bottom_text"), _atarix.Position.X, _atarix.Position.Y + 125f, Atarix.FONT_SCALE_DEFAULT, new Color(200, 20, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
	}
}
