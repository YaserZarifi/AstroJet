using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Menu;

internal class TextField : Transformation
{
	public string Text;

	public string Label;

	public bool IsSelected;

	public bool IsEdited;

	public bool IsInputEnabled = true;

	public Color TextColor;

	public int PaddingLeft = 10;

	public Vector2 LabelScale = ShopMenu.FONT_SCALE_BUTTON;

	public Vector2 TextScale = ShopMenu.FONT_SCALE_BUTTON;

	public const int WIDTH = 244;

	public const int HEIGHT = 42;

	private ShopMenu _shopMenu;

	private float _cursorAlpha;

	private float _keyRepeatTimer;

	private Keys _lastPressedKey;

	private int _cursorPos;

	private IntRange _selectionRange;

	public TextField(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		TextColor = new Color(255, 255, 20);
	}

	public void Select()
	{
		if (!IsSelected)
		{
			IsSelected = true;
			GameCore.PlaySound("menu_button_select");
		}
	}

	public void Unselect()
	{
		IsEdited = false;
		IsSelected = false;
	}

	private bool DeleteSelection()
	{
		if (_selectionRange.From < _selectionRange.To)
		{
			Text = Text.Substring(0, _selectionRange.From) + ((_selectionRange.To < Text.Length - 1) ? Text.Substring(_selectionRange.To + 1, Text.Length - _selectionRange.To - 1) : "");
			_cursorPos = Math.Min(_selectionRange.From, Text.Length);
			_selectionRange.From = _selectionRange.To;
			return true;
		}
		return false;
	}

	public void Update(float elapsedTime)
	{
		ResetPrecalculations();
		if (!_shopMenu.IsAnyPopupMenuVisible && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			if (!IsEdited && IsInputEnabled && _shopMenu.Cursor.IsInRect(WorldPosition.X - 110f, WorldPosition.Y - 21f, 244, 42))
			{
				Select();
			}
			else
			{
				IsSelected = false;
			}
		}
		if (!_shopMenu.IsAnyPopupMenuVisible)
		{
			if (IsEdited)
			{
				Keys keys = GameInput.SystemGetPressedKey();
				if (_lastPressedKey != keys && keys != Keys.None)
				{
					_keyRepeatTimer = 0.5f;
					_lastPressedKey = keys;
				}
				if (GameInput.SystemKeyDown(_lastPressedKey))
				{
					_keyRepeatTimer -= elapsedTime;
					if (_keyRepeatTimer <= 0f)
					{
						GameInput.ClearLastKeyboadState();
						_keyRepeatTimer = 0.075f;
					}
				}
				else
				{
					_keyRepeatTimer = 0.5f;
				}
				_cursorAlpha += 10f * elapsedTime;
				char keyChar;
				if ((_shopMenu.KeyPressed(ShopMenuKeys.CancelSelection) || _shopMenu.KeyPressed(ShopMenuKeys.Select) || _shopMenu.KeyPressed(ShopMenuKeys.CancelAll, bOnlyOnce: false) || GameInput.SystemKeyPressed(Keys.Enter) || GameInput.SystemKeyPressed(Keys.Escape)) && !GameCore.Console.IsActive)
				{
					IsEdited = false;
					IsSelected = true;
					GameCore.PlaySound("menu_yesno_hide");
				}
				else if (GameInput.SystemKeyPressed(Keys.Home))
				{
					_selectionRange.To = (_selectionRange.From = 0);
					_cursorPos = 0;
				}
				else if (GameInput.SystemKeyPressed(Keys.End))
				{
					_selectionRange.To = (_selectionRange.From = 0);
					_cursorPos = Text.Length;
				}
				else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
				{
					_selectionRange.To = (_selectionRange.From = 0);
					if (_cursorPos > 0)
					{
						_cursorPos--;
					}
				}
				else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
				{
					_selectionRange.To = (_selectionRange.From = 0);
					if (_cursorPos < Text.Length)
					{
						_cursorPos++;
					}
				}
				else if (GameInput.SystemKeyPressed(Keys.Back))
				{
					if (!DeleteSelection() && _cursorPos > 0)
					{
						Text = Text.Remove(_cursorPos - 1, 1);
						_cursorPos--;
					}
				}
				else if (GameInput.SystemKeyPressed(Keys.Delete))
				{
					if (!DeleteSelection() && _cursorPos < Text.Length)
					{
						Text = Text.Remove(_cursorPos, 1);
					}
				}
				else if ((_shopMenu.FontDefault.MeasureFont(Text) * ShopMenu.FONT_SCALE_ITEM_NAME).X < 200f && GameInput.TryConvertKeyboardInput(out keyChar) && Text.Length < 30)
				{
					DeleteSelection();
					if (_cursorPos < Text.Length)
					{
						Text = Text.Insert(_cursorPos, keyChar.ToString());
					}
					else
					{
						Text += keyChar;
					}
					_cursorPos++;
				}
			}
			else if (IsSelected && _shopMenu.KeyPressed(ShopMenuKeys.Select))
			{
				IsEdited = true;
				if (Text == null)
				{
					Text = "";
				}
				_selectionRange.From = 0;
				_selectionRange.To = Text.Length - 1;
				_cursorPos = Text.Length;
				GameCore.PlaySound("menu_yesno_show");
			}
		}
		if (IsEdited)
		{
			_shopMenu.CursorType = CursorTypeEnum.Selected;
		}
		else if (IsSelected)
		{
			_shopMenu.CursorType = CursorTypeEnum.Select;
		}
	}

	public void Draw()
	{
		_shopMenu.AtlasOptionsElement.DrawFrame(_shopMenu.SpriteBatchLight, 1, Color, Transformation.CreateByPosition(WorldPosition));
		_shopMenu.SpriteBatchEnd(bLight: true);
		_shopMenu.SpriteBatchBegin(bLight: true);
		_shopMenu.SpriteBatchBegin(bLight: false);
		if (IsSelected || IsEdited)
		{
			_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 7, Color, Transformation.CreateByPosition(WorldPosition), BlendModeEnum.Additive);
		}
		float num = (float)(int)Color.A / 255f;
		if (Text != null)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, Text, WorldPosition.X - 122f + (float)PaddingLeft, WorldPosition.Y - 11f * Options.FontScale, TextScale * Options.FontScale, TextColor, TextAlign.Left, 230, num);
			if (IsEdited)
			{
				Vector2 vector = _shopMenu.FontDefault.MeasureFont(Text.Substring(0, _cursorPos)) * TextScale * Options.FontScale;
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, "|", WorldPosition.X - 122f + (float)PaddingLeft + vector.X, WorldPosition.Y - 11f * Options.FontScale, TextScale * Options.FontScale, new Color(255, 255, 20, (int)(255.0 * (1.0 + Math.Sin(_cursorAlpha) * 0.5 * (double)num))));
				if (_selectionRange.To > _selectionRange.From)
				{
					_shopMenu.SpriteBatchEnd(bLight: false);
					_shopMenu.SpriteBatchBegin(bLight: false);
					int num2 = (int)(_shopMenu.FontDefault.MeasureFont(Text.Substring(0, _selectionRange.From)).X * TextScale.X * Options.FontScale);
					int num3 = (int)(_shopMenu.FontDefault.MeasureFont(Text.Substring(_selectionRange.From, Math.Min(_selectionRange.To, Text.Length - 1) + 1 - _selectionRange.From)).X * TextScale.X * Options.FontScale);
					Rectangle rect = new Rectangle((int)WorldPosition.X - 122 + PaddingLeft - 2 + num2, (int)WorldPosition.Y - 12, num3 + 3, 21);
					_shopMenu.TriangleBatch.DrawRect(rect, new Color(0, 100, 255, 70));
					_shopMenu.TriangleBatch.Flush();
				}
			}
		}
		if (Label != null)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, Label, WorldPosition.X - 120f, WorldPosition.Y - 46f, LabelScale * Options.FontScale, ShopMenu.FONT_COLOR_LABEL, TextAlign.Left, 0, num);
		}
		_shopMenu.SpriteBatchEnd(bLight: false);
	}
}
