using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class Combo : ShopComponent
{
	public delegate void OnEventHandler(Combo slider);

	public const int WIDTH = 260;

	public const int HEIGHT = 45;

	public List<ComboValue> ValueList;

	public string Text;

	public string Caption;

	public Vector2 CaptionOffset;

	public Vector2 CaptionScale = new Vector2(0.5f);

	public Color CaptionColor;

	public event OnEventHandler OnActivate;

	public Combo(ShopMenu shopMenu)
		: base(shopMenu)
	{
		CaptionColor = ShopMenu.FONT_COLOR_LABEL;
		CaptionOffset = new Vector2(-275f, -10f);
		ValueList = new List<ComboValue>();
		SelectSoundId = "menu_button_select";
	}

	public void SetTextByValueList(int type)
	{
		string text = "";
		int num = 0;
		foreach (ComboValue value in ValueList)
		{
			if (type < 0 || value.Type == type)
			{
				if (num > 0)
				{
					text += ", ";
				}
				text += value.Text;
				num++;
				if (num > 3)
				{
					break;
				}
			}
		}
	}

	public void ClearValues()
	{
		ValueList.Clear();
	}

	public void AddValue(int value, int type, string text)
	{
		ValueList.Add(new ComboValue(value, type, text));
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			IsSelected = false;
			if (_shopMenu.Cursor.IsInRect(Position.X - 130f, Position.Y - 22.5f, 260, 45))
			{
				IsSelected = true;
			}
		}
		if ((IsSelected || IsActivated) && (_shopMenu.KeyPressed(ShopMenuKeys.Select, bOnlyOnce: false) || _shopMenu.KeyPressed(ShopMenuKeys.CancelSelection, bOnlyOnce: false)))
		{
			IsActivated = !IsActivated;
			if (IsActivated)
			{
				OnActivate?.Invoke(this);
			}
		}
	}

	public override void Draw()
	{
		_shopMenu.AtlasOptionsElement.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color, WorldTransformation, BlendModeEnum.Normal);
		float alpha = (float)(int)Color.A / 255f;
		Color value = new Color(CaptionColor, alpha);
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText(Caption) + ":", WorldPosition.X + CaptionOffset.X, WorldPosition.Y + CaptionOffset.Y, CaptionScale, value);
		if (IsSelected)
		{
			_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchLight, 3, Color, WorldTransformation, BlendModeEnum.Additive);
		}
	}
}
