using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rake2D.Font.BMFont;

namespace Rake2D.Menu.Atarix;

public class AtarixContent
{
	public enum AtarixContentStateEnum
	{
		Hidden,
		Hidding,
		Showing,
		Visible
	}

	public AtarixContentStateEnum State;

	protected Atarix _atarix;

	protected ShopMenu _shopMenu;

	protected BMFont _font;

	protected List<string> _menuItemList;

	protected int _selectedMenuItemIndex;

	protected int _menuOffsetY;

	public AtarixContent(Atarix atarix, ShopMenu shopMenu)
	{
		_atarix = atarix;
		_shopMenu = shopMenu;
		_font = GameCore.GetBMFont("level_font");
	}

	public virtual void Show()
	{
		_selectedMenuItemIndex = 0;
		if (State == AtarixContentStateEnum.Hidden || State == AtarixContentStateEnum.Hidding)
		{
			State = AtarixContentStateEnum.Showing;
		}
	}

	public virtual void SetTexts()
	{
	}

	public virtual void RegisterEvents()
	{
	}

	public virtual void Update(float elapsedTime)
	{
	}

	public virtual void Draw()
	{
	}

	protected Vector2 GetMenuItemPosition(int number)
	{
		return new Vector2(_atarix.Position.X, _atarix.Position.Y + (float)_menuOffsetY + (float)(number * 30));
	}

	protected virtual void OnMenuItemClick(int menuItem)
	{
	}

	protected virtual void UpdateMouseInMenu()
	{
		if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
		{
			return;
		}
		for (int i = 0; i < _menuItemList.Count; i++)
		{
			Vector2 menuItemPosition = GetMenuItemPosition(i);
			menuItemPosition.X -= 200f;
			menuItemPosition.Y -= 20f;
			if (_shopMenu.Cursor.IsInRect(menuItemPosition, 400, 40))
			{
				if (_selectedMenuItemIndex != i)
				{
					GameCore.PlaySound("atarix_menu");
					_selectedMenuItemIndex = i;
				}
				_shopMenu.CursorType = CursorTypeEnum.Select;
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					OnMenuItemClick(i);
				}
				break;
			}
		}
	}
}
