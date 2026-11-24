using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Menu;

public class Button : Transformation
{
	public delegate void OnEventHandler(Button button);

	public int Width;

	public int Height;

	public string Caption;

	public string CaptionSymbol;

	public long PriceValue;

	public Vector2 CaptionScale = ShopMenu.FONT_SCALE_BUTTON;

	public ShopButtonType Type;

	public bool IsDisabled;

	public bool IsDenied;

	public bool HasPressEffectEnabled;

	public bool IsPressed;

	public bool IsVisible;

	public bool IsInputEnabled = true;

	public string Label;

	public Color LabelColor;

	public Vector2 LabelScale;

	public Vector2 LabelOffset;

	protected ShopMenu _shopMenu;

	protected SpriteAtlas _atlas;

	private float _highliteFactor;

	private float _pressTimer;

	private Vector2 _priceGlowScale;

	public bool IsSelected { get; private set; }

	public event OnEventHandler OnClick;

	public event OnEventHandler OnSelected;

	public event OnEventHandler OnCursorOver;

	public Button(ShopMenu shopMenu, ShopButtonType type)
	{
		_shopMenu = shopMenu;
		Type = type;
		IsVisible = true;
		Label = null;
		LabelScale = ShopMenu.FONT_SCALE_LABEL;
		LabelColor = ShopMenu.FONT_COLOR_LABEL;
		_priceGlowScale = Vector2.One;
		switch (type)
		{
		case ShopButtonType.Normal:
		case ShopButtonType.Price:
			Width = 190;
			Height = 40;
			_atlas = _shopMenu.AtlasButton;
			break;
		case ShopButtonType.Big:
			Width = 230;
			Height = 55;
			_atlas = _shopMenu.AtlasBigButton;
			break;
		case ShopButtonType.Tab:
			Width = 190;
			Height = 50;
			_atlas = _shopMenu.AtlasTabButton;
			break;
		case ShopButtonType.TabSmall:
			Width = 158;
			Height = 50;
			_atlas = _shopMenu.AtlasTabButtonSmall;
			break;
		}
	}

	public void Select(bool bPlaySound = true)
	{
		if (!IsSelected)
		{
			IsSelected = true;
			OnSelected?.Invoke(this);
			if (bPlaySound)
			{
				GameCore.PlaySound("menu_button_select");
			}
		}
	}

	public void Unselect()
	{
		if (IsSelected)
		{
			IsSelected = false;
		}
	}

	public void SetSelected()
	{
		IsSelected = true;
		_highliteFactor = 1f;
	}

	public void SetUnselected()
	{
		IsSelected = false;
		_highliteFactor = 0f;
	}

	public void Update(float elapsedTime)
	{
		if (ParentTransformation != null)
		{
			Color = ParentTransformation.Color;
		}
		ResetPrecalculations();
		if (!IsVisible)
		{
			return;
		}
		Utils.ChangeValueToTarget(ref _pressTimer, 0f, 20f * elapsedTime);
		ResetPrecalculations();
		if (IsInputEnabled)
		{
			if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
			{
				if (_shopMenu.Cursor.IsInRect(WorldPosition.X - (float)(Width / 2), WorldPosition.Y - _atlas.GetFramePivot(0).Y + 5f, Width, Height))
				{
					if (!IsDenied && !IsDisabled)
					{
						_shopMenu.CursorType = CursorTypeEnum.Select;
					}
					else
					{
						_shopMenu.CursorType = CursorTypeEnum.Denied;
					}
					OnCursorOver?.Invoke(this);
					Select();
				}
				else
				{
					IsSelected = false;
				}
			}
			if (IsSelected && _shopMenu.KeyPressed(ShopMenuKeys.Select))
			{
				Click();
			}
		}
		if (IsSelected)
		{
			Utils.ChangeValueToTarget(ref _highliteFactor, 1f, 5f * elapsedTime);
		}
		else
		{
			Utils.ChangeValueToTarget(ref _highliteFactor, 0f, 5f * elapsedTime);
		}
		_priceGlowScale.X += elapsedTime * 5f;
		_priceGlowScale.Y += elapsedTime * 2f;
	}

	public void CancelPressEffect()
	{
		_pressTimer = 0f;
	}

	public void Click()
	{
		OnClick?.Invoke(this);
		if (HasPressEffectEnabled)
		{
			_pressTimer = (float)Math.PI;
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch = null)
	{
		if (!IsVisible)
		{
			return;
		}
		int frameNumber = 0;
		int frameNumber2 = 1;
		if (spriteBatch == null)
		{
			spriteBatch = _shopMenu.SpriteBatchLight;
		}
		if (IsPressed)
		{
			frameNumber = 2;
			frameNumber2 = 3;
		}
		float num = (float)(int)Color.A / 255f;
		Color value;
		if (IsDenied)
		{
			value = new Color(255, 0, 0, (int)(200f * num));
		}
		else if (IsPressed)
		{
			value = new Color(255, 220, 20, (int)(255f * num));
		}
		else
		{
			value = ((!IsDisabled) ? new Color(255, 186, 1, (int)(255f * num)) : new Color(150, 150, 150, (int)(150f * num)));
		}
		if (Type == ShopButtonType.Price)
		{
			float num2 = Utils.Sin(_priceGlowScale.X * 0.7f) * 20f;
			Color value2 = ((!IsDenied) ? new Color(0f, (int)(200f * num + num2), (int)(235f * num + num2), 255f * num) : new Color((int)(235f * num + num2), 0f, 0f, 255f * num));
			if (PriceValue >= 0)
			{
				Transformation transformation = new Transformation(WorldTransformation);
				transformation.Position.Y += 35f;
				transformation.Scale = new Vector2(0.9f + Utils.Sin(_priceGlowScale.X) * 0.1f, 0.99f + Utils.Sin(_priceGlowScale.X) * 0.01f);
				_shopMenu.AtlasButtonPriceGlow.DrawFrame(spriteBatch, IsDenied ? 1 : 0, new Color(255, 255, 255, (int)(200f * num + Utils.Sin(_priceGlowScale.X * 0.8f) * 20f)), transformation, BlendModeEnum.Additive);
				_shopMenu.FontDefault.Write(spriteBatch, $"✐ {Utils.FormatMoney(PriceValue)}", transformation.Position.X, transformation.Position.Y - 16f, ShopMenu.FONT_SCALE_LABEL_SMALL, value2, TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
			}
		}
		float value3;
		Color color;
		if (_pressTimer > 0f)
		{
			value3 = Utils.Sin(_pressTimer) * 2f;
			color = Color * (1f - 0.25f * Utils.Sin(_pressTimer));
			color.A = Color.A;
		}
		else
		{
			value3 = 0f;
			color = Color;
		}
		Transformation transformation2 = new Transformation(WorldTransformation);
		transformation2.Position += new Vector2(value3);
		if (_highliteFactor < 1f)
		{
			_atlas.DrawFrame(spriteBatch, frameNumber, color, transformation2, BlendModeEnum.Normal);
		}
		if (_highliteFactor > 0f)
		{
			_atlas.DrawFrame(spriteBatch, frameNumber2, new Color(color, (int)((float)(int)color.A * _highliteFactor)), transformation2, BlendModeEnum.Normal);
		}
		if (Caption != null)
		{
			_shopMenu.FontDefault.Write(spriteBatch, ((CaptionSymbol != null) ? (CaptionSymbol + " ") : "") + Caption, transformation2.Position.X, transformation2.Position.Y - (IsPressed ? 18.5f : 20.5f) * CaptionScale.Y * Options.FontScale, CaptionScale * Options.FontScale, value, TextAlign.Center);
		}
		if (Label != null)
		{
			Vector2 vector = new Vector2(WorldPosition.X, WorldPosition.Y) + LabelOffset;
			string text = Label + ":";
			_shopMenu.FontDefault.Write(spriteBatch, text, vector.X, vector.Y, LabelScale, new Color(LabelColor, (float)(int)Color.A / 255f));
		}
	}
}
