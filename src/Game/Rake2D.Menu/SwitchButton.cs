using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class SwitchButton : ShopComponent
{
	public delegate void OnEventHandler(SwitchButton btn);

	public const int HEIGHT = 42;

	public int PaddingLeft = 10;

	public bool IsClickable;

	public bool IsSelectable = true;

	public GameInput.KeyTypeEnum KeyType;

	public string Text;

	public ControlsGroupEnum ControlsGroup;

	public ControlsButtonTypeEnum Type;

	public string Label;

	public Vector2 LabelOffset;

	public Vector2 LabelScale = ShopMenu.FONT_SCALE_BUTTON;

	public Vector2 TextScale = ShopMenu.FONT_SCALE_BUTTON;

	public Color LabelColor;

	public Color TextColor = ShopMenu.FONT_COLOR_PARAM_VALUE;

	public TextAlign TextAlign = TextAlign.Center;

	public int WIDTH
	{
		get
		{
			if (Type != ControlsButtonTypeEnum.Small)
			{
				return 244;
			}
			return 160;
		}
	}

	public event OnEventHandler OnClick;

	public SwitchButton(ShopMenu shopMenu)
		: base(shopMenu)
	{
		LabelColor = ShopMenu.FONT_COLOR_LABEL;
		LabelOffset = new Vector2(-185f, -11f);
		SelectSoundId = "menu_button_select";
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && IsSelectable)
		{
			if (_shopMenu.Cursor.IsInRect(WorldPosition.X - (float)WIDTH / 2f, WorldPosition.Y - 21f, WIDTH, 42))
			{
				Select();
			}
			else
			{
				Unselect();
			}
		}
		if (IsSelected && !IsActivated && (_shopMenu.KeyPressed(ShopMenuKeys.Select, bOnlyOnce: false) || _shopMenu.KeyPressed(ShopMenuKeys.CancelSelection, bOnlyOnce: false)))
		{
			if (!IsClickable)
			{
				IsActivated = true;
			}
			else
			{
				GameCore.PlaySound("menu_option_set");
			}
			OnClick?.Invoke(this);
		}
		if (IsSelected)
		{
			_shopMenu.CursorType = CursorTypeEnum.Select;
		}
	}

	public override void Draw()
	{
		_shopMenu.AtlasOptionsElement.DrawFrame(_shopMenu.SpriteBatchLight, (Type != ControlsButtonTypeEnum.Small) ? 1 : 0, Color, WorldTransformation, BlendModeEnum.Normal);
		float alpha = (float)(int)Color.A / 255f;
		if (Label != null)
		{
			Color value = new Color(LabelColor, alpha);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, Label, WorldPosition.X + LabelOffset.X, WorldPosition.Y + LabelOffset.Y, LabelScale * Options.FontScale, value);
		}
		if (IsSelected)
		{
			_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchLight, (Type == ControlsButtonTypeEnum.Small) ? 8 : 7, Color, WorldTransformation, BlendModeEnum.Additive);
		}
		if (Text != null)
		{
			Color value2 = new Color(TextColor, alpha);
			if (TextAlign == TextAlign.Center)
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, Text, WorldPosition.X, WorldPosition.Y - 20f * TextScale.Y * Options.FontScale, TextScale * Options.FontScale, value2, TextAlign.Center);
			}
			else
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, Text, WorldPosition.X - (float)(WIDTH / 2) + (float)PaddingLeft, WorldPosition.Y - 20f * TextScale.Y * Options.FontScale, TextScale * Options.FontScale, value2);
			}
		}
	}
}
