using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Menu.Atarix;

public class AtarixMainMenu : AtarixContent
{
	private SpriteAtlas _atlas;

	public AtarixMainMenu(Atarix atarix, ShopMenu shopMenu)
		: base(atarix, shopMenu)
	{
		_atlas = GameCore.GetSpriteAtlas("atarix_main_menu");
		_menuItemList = new List<string>();
		for (int i = 0; i < 3; i++)
		{
			_menuItemList.Add("---");
		}
		_menuOffsetY = -10;
	}

	public override void RegisterEvents()
	{
		_atarix.OnUpButtonClick += OnUpButtonClick;
		_atarix.OnDownButtonClick += OnDownButtonClick;
		_atarix.OnActionButtonClick += OnActionButtonClick;
	}

	private void OnUpButtonClick()
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

	private void OnDownButtonClick()
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

	protected override void OnMenuItemClick(int menuItem)
	{
		if (menuItem == 0)
		{
			_atarix.SetContent(_atarix.ContentTetrix);
		}
	}

	private void OnActionButtonClick()
	{
		OnMenuItemClick(_selectedMenuItemIndex);
	}

	public override void SetTexts()
	{
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			_menuItemList[i] = TextBundle.GetText($"atarix_main_menu_item_{i}");
		}
	}

	public override void Show()
	{
		State = AtarixContentStateEnum.Visible;
		GameCore.PlaySound("atarix_show");
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		UpdateMouseInMenu();
	}

	public override void Draw()
	{
		base.Draw();
		Transformation transformation = new Transformation(_atarix);
		transformation.Position.Y -= 160f;
		_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, _atarix.Color, transformation, BlendModeEnum.Normal);
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			transformation.Position = GetMenuItemPosition(i);
			transformation.Scale = new Vector2(1.5f, 1.6f);
			if (i == _selectedMenuItemIndex)
			{
				_atarix.Atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, new Color(0, 200, 0, (int)_atarix.Color.A), transformation, BlendModeEnum.Normal);
				_font.Write(_shopMenu.SpriteBatchNoLight, _menuItemList[i], transformation.Position.X, transformation.Position.Y - 9f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 0, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			}
			else
			{
				_font.Write(_shopMenu.SpriteBatchNoLight, _menuItemList[i], transformation.Position.X, transformation.Position.Y - 9f, Atarix.FONT_SCALE_DEFAULT, new Color(0, 180, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
			}
		}
		transformation.Scale = Vector2.One;
		transformation.Position.Y = _atarix.Position.Y + 135f;
		_atlas.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, _atarix.Color, transformation, BlendModeEnum.Normal);
		_font.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText("atarix_main_menu_bottom_text"), _atarix.Position.X, _atarix.Position.Y + 175f, new Vector2(0.6f, 0.55f), new Color(0, 180, 0), TextAlign.Center, 700, _shopMenu.ShowHidePercentage);
	}
}
