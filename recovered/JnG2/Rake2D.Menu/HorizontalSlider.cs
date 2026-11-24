using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class HorizontalSlider : ShopComponent
{
	public delegate void OnEventHandler(HorizontalSlider slider);

	public const int WIDTH = 202;

	public const int HEIGHT = 27;

	public const int RANGE_WIDTH = 170;

	public SliderLabeValueModeEnum LabelValueMode;

	public int MaxValue;

	public int LabelValueOffset = 1;

	public bool IsInputEnabled = true;

	public string Label;

	public Color LabelColor;

	public Color LabelValueColor;

	public Vector2 LabelScale;

	public Vector2 LabelOffset = new Vector2(-109f, -45f);

	public bool IsDragSelected;

	public bool IsDragged;

	public string[] ValueStringArray;

	private float _currSliderLightAlpha;

	private float _currSliderBarLightAlpha;

	private float _dragOffsetX;

	private SpriteAtlas _atlas;

	private ShopObject _sliderObject;

	private GameObject _sliderFlarePart;

	public int Value { get; protected set; }

	public event OnEventHandler OnActivate;

	public event OnEventHandler OnDeactivate;

	public event OnEventHandler OnSetValue;

	public HorizontalSlider(ShopMenu shopMenu)
		: base(shopMenu)
	{
		_atlas = _shopMenu.AtlasHorizontalSlider;
		Label = "";
		LabelScale = ShopMenu.FONT_SCALE_BUTTON;
		LabelColor = ShopMenu.FONT_COLOR_LABEL;
		LabelValueColor = new Color(255, 255, 50);
		SelectSoundId = "menu_button_select";
		PartData partData = GameCore.GetPartData("shop_h_slider");
		if (partData != null)
		{
			_sliderObject = new ShopObject(shopMenu, _shopMenu.LayerShop, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
			_sliderFlarePart = _sliderObject.MyObject.GetChildByIdDeep("flare");
		}
	}

	public void SetValue(int value, bool bCallEvents)
	{
		if (value >= 0 && value <= MaxValue && value != Value)
		{
			Value = value;
			if (bCallEvents && OnSetValue != null)
			{
				OnSetValue(this);
			}
		}
	}

	public void SetValueByPos(float posX)
	{
		SetValue((int)Utils.Clamp((posX - (WorldPosition.X - 85f)) / 170f * (float)(MaxValue + 1), 0f, MaxValue), bCallEvents: true);
	}

	public void Activate()
	{
		if (!IsActivated)
		{
			IsActivated = true;
			OnActivate?.Invoke(this);
		}
	}

	public void Deactivate()
	{
		if (IsActivated)
		{
			IsActivated = false;
			OnDeactivate?.Invoke(this);
		}
	}

	public void SetDeactivated()
	{
		IsActivated = false;
		_currSliderLightAlpha = 0f;
		_currSliderBarLightAlpha = 0f;
	}

	private void ParseInput()
	{
		if (IsActivated)
		{
			if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				SetValue(Value - 1, bCallEvents: true);
			}
			else if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.MoveRight))
			{
				SetValue(Value + 1, bCallEvents: true);
			}
		}
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		ParseInput();
		Vector2 dragPos = GetDragPos();
		IsDragSelected = false;
		if (IsInputEnabled && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			if (_shopMenu.Cursor.IsInRect(dragPos.X - 15f, dragPos.Y - 20f, 30, 40))
			{
				IsDragSelected = true;
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					IsDragged = true;
					_dragOffsetX = dragPos.X - _shopMenu.Cursor.Position.X;
				}
			}
			if (IsDragSelected || _shopMenu.Cursor.IsInRect(WorldPosition.X - 101f, WorldPosition.Y - 13.5f, 202, 27))
			{
				Select();
			}
			else
			{
				Unselect();
			}
		}
		if ((IsSelected || IsActivated) && (_shopMenu.KeyPressed(ShopMenuKeys.Select, bOnlyOnce: false) || _shopMenu.KeyPressed(ShopMenuKeys.CancelSelection, bOnlyOnce: false)))
		{
			if (!IsActivated)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}
		if ((IsSelected && GameInput.MousePressed(GameInput.MouseButton.Left)) || (IsDragged && GameInput.MouseDown(GameInput.MouseButton.Left)))
		{
			SetValueByPos(_shopMenu.Cursor.Position.X + _dragOffsetX);
			IsDragged = true;
		}
		if (!GameInput.MouseDown(GameInput.MouseButton.Left) && IsDragged)
		{
			IsDragged = false;
			IsActivated = false;
			OnDeactivate?.Invoke(this);
		}
		if (IsDragged)
		{
			_shopMenu.CursorType = CursorTypeEnum.Selected;
		}
		else if (IsSelected)
		{
			_shopMenu.CursorType = CursorTypeEnum.Select;
		}
		if ((IsSelected && !IsDragSelected) || IsDragged || IsActivated)
		{
			Utils.ChangeValueToTarget(ref _currSliderBarLightAlpha, 255f, 750f * elapsedTime);
		}
		else
		{
			Utils.ChangeValueToTarget(ref _currSliderBarLightAlpha, 0f, 750f * elapsedTime);
		}
		_sliderObject.SetPosition(dragPos);
		if (IsDragSelected || IsActivated)
		{
			Utils.ChangeValueToTarget(ref _currSliderLightAlpha, 1f, 5f * elapsedTime);
		}
		else
		{
			Utils.ChangeValueToTarget(ref _currSliderLightAlpha, 0f, 5f * elapsedTime);
		}
		_sliderFlarePart.Alpha = (byte)(_currSliderLightAlpha * (float)(int)Color.A);
		_sliderObject.MyObject.Alpha = Color.A;
		_sliderObject.Update(elapsedTime);
	}

	public Vector2 GetDragPos()
	{
		float x = -85f + (float)Value / (float)((MaxValue == 0) ? 1 : MaxValue) * 170f;
		return WorldPosition + new Vector2(x, 0f);
	}

	public override void Draw()
	{
		Vector2 vector = new Vector2(WorldPosition.X, WorldPosition.Y) + LabelOffset;
		string text = Label + ":";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, text, vector.X, vector.Y, LabelScale * Options.FontScale, new Color(LabelColor, (float)(int)Color.A / 255f));
		float num = _shopMenu.FontDefault.MeasureFont(text).X * LabelScale.X * Options.FontScale;
		switch (LabelValueMode)
		{
		case SliderLabeValueModeEnum.Ratio:
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, $"{Value + LabelValueOffset}/{MaxValue + LabelValueOffset}", vector.X + num + 4f, vector.Y, LabelScale * Options.FontScale, new Color(LabelValueColor, (float)(int)Color.A / 255f));
			break;
		case SliderLabeValueModeEnum.Percentage:
		{
			int num2 = (int)((float)(Value + LabelValueOffset) / (float)MaxValue * 100f);
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, $"{num2}%", vector.X + num + 4f, vector.Y, LabelScale * Options.FontScale, new Color(LabelValueColor, (float)(int)Color.A / 255f));
			break;
		}
		case SliderLabeValueModeEnum.StringArray:
			if (ValueStringArray != null && Value >= 0 && Value < ValueStringArray.Length)
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, ValueStringArray[Value], vector.X + num + 4f, vector.Y, LabelScale * Options.FontScale, new Color(LabelValueColor, (float)(int)Color.A / 255f));
			}
			break;
		}
		if (IsSelected)
		{
			_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchLight, 3, Color, WorldTransformation, BlendModeEnum.Normal);
		}
		GameCore.EffectLight.SetLightMaps(bUseShopMaps: true);
		GameCore.EffectLight.SetLightFactor(0f, 1f, 0f);
		_atlas.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color, WorldTransformation, BlendModeEnum.Normal);
		_atlas.DrawFrame(_shopMenu.SpriteBatchLight, 1, new Color(Color, _currSliderBarLightAlpha / 255f), WorldTransformation, BlendModeEnum.Normal);
		_sliderObject.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
		_shopMenu.SpriteBatchBegin(bLight: true);
	}
}
