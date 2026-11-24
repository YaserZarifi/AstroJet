using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Menu;

public class PanelFaceSelector : PanelWithSlider
{
	public delegate void OnEventHandler(PanelFaceSelector panelFaceSelector);

	public delegate void OnFaceChangeHandler(int faceNumber);

	public int CursorFaceIndex;

	public Vector2 StartPos;

	public Vector2 EndPos;

	private float _showPercentage;

	private const int SLIDER_AREA_WIDTH = 689;

	private const int VISIBLE_FACE_COUNT = 7;

	private SpriteAtlas _atlasFaces;

	private int _firstVisibleFace;

	public int FaceNumber { get; private set; }

	public int FaceCount => _atlasFaces.FrameCount;

	public event OnEventHandler OnHide;

	public event OnFaceChangeHandler OnFaceChanged;

	public PanelFaceSelector(ShopMenu shopMenu)
		: base(shopMenu)
	{
		Init();
	}

	public override void Init()
	{
		base.Init();
		_atlasFrame = GameCore.GetSpriteAtlas("face_selector");
		_atlasFaces = GameCore.GetSpriteAtlas("profile_face");
		StartPos = new Vector2(465f, 210f);
		EndPos = new Vector2(960f, 210f);
		_sliderObject.MyObject.ParentTransformation = null;
		_sliderObject.MyObject.Rotation = Utils.DegToRad(270f);
		_sliderObject.MyObject.Scale.X = -1f;
	}

	public void Show(int faceNumber, float showDelay)
	{
		base.Show(showDelay);
		Position = StartPos;
		TargetPosition = EndPos;
		_shopMenu.FocusSection = this;
		SelectFace(faceNumber);
		GameCore.PlaySound("menu_yesno_show");
	}

	public override void Hide()
	{
		if (State != ShopMenuState.Hidden)
		{
			GameCore.PlaySound("menu_yesno_hide");
			OnHide?.Invoke(this);
		}
		base.Hide();
		TargetPosition = StartPos;
	}

	private void ParseInput()
	{
		if (_shopMenu.FocusSection != this)
		{
			return;
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight, bOnlyOnce: false))
		{
			if (CursorFaceIndex < FaceCount - 1)
			{
				if (CursorFaceIndex >= 0)
				{
					SelectFace(CursorFaceIndex + 1);
				}
				else
				{
					SelectFace(FaceNumber);
				}
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft, bOnlyOnce: false))
		{
			if (CursorFaceIndex > 0)
			{
				SelectFace(CursorFaceIndex - 1);
			}
			else if (CursorFaceIndex < 0)
			{
				SelectFace(FaceNumber);
			}
		}
	}

	public void SelectFace(int index)
	{
		if (index >= 0 && index < FaceCount && CursorFaceIndex != index)
		{
			CursorFaceIndex = index;
			GameCore.PlaySound("menu_item_select");
			_shopMenu.ChangeFocus(this);
		}
	}

	private void UpdateSlider(float elapsedTime)
	{
		Vector2 sliderPosition = GetSliderPosition();
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && _shopMenu.Cursor.IsInRect(sliderPosition.X - 50f, sliderPosition.Y - 7f, 100, 28))
		{
			_isSliderUnderCursor = true;
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				_isSliderDragged = true;
			}
		}
		else
		{
			_isSliderUnderCursor = false;
		}
		if (!_isSliderUnderCursor && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && _shopMenu.Cursor.IsInRect(Position.X - 344f - 45f, sliderPosition.Y - 7f, 789, 28))
		{
			_isSliderBarUnderCursor = true;
		}
		else
		{
			_isSliderBarUnderCursor = false;
		}
		if (GameInput.MouseDown(GameInput.MouseButton.Left) && (_isSliderDragged || _isSliderBarUnderCursor))
		{
			_isSliderDragged = true;
			_firstVisibleFace = (int)Utils.Clamp((_shopMenu.Cursor.Position.X + 30f - (Position.X - 344f)) / 689f * (float)(FaceCount - 7), 0f, FaceCount - 7);
		}
		else
		{
			_isSliderDragged = false;
		}
		UpdateSliderLight(elapsedTime, bForceLight: false);
		_currSliderLightAlpha *= _showPercentage;
		_sliderObject.MyObject.Position = GetSliderPosition();
		_sliderObject.MyObject.Update(elapsedTime);
	}

	public override void Update(float elapsedTime)
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		base.Update(elapsedTime);
		ParseInput();
		if (CursorFaceIndex >= 0 && CursorFaceIndex < _firstVisibleFace)
		{
			_firstVisibleFace = CursorFaceIndex;
		}
		else if (CursorFaceIndex > _firstVisibleFace + 7 - 1)
		{
			_firstVisibleFace = CursorFaceIndex - 6;
		}
		bool flag = false;
		for (int i = _firstVisibleFace; i < _firstVisibleFace + 7; i++)
		{
			Vector2 facePosition = GetFacePosition(i);
			if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && !_isSliderDragged && _shopMenu.Cursor.IsInRect(facePosition - new Vector2(55f, 80f), 111, 160))
			{
				SelectFace(i);
				_shopMenu.ChangeFocus(this);
				flag = true;
				break;
			}
		}
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && !flag)
		{
			CursorFaceIndex = -1;
		}
		if ((CursorFaceIndex < 0 && !_isSliderUnderCursor && !_isSliderBarUnderCursor && _shopMenu.KeyPressed(ShopMenuKeys.Select)) || _shopMenu.KeyPressed(ShopMenuKeys.CancelSelection) || _shopMenu.KeyPressed(ShopMenuKeys.CancelAll, bOnlyOnce: false) || GameInput.SystemGPButtonPressedAll(Buttons.B))
		{
			Hide();
		}
		else if (CursorFaceIndex >= 0 && _shopMenu.KeyPressed(ShopMenuKeys.Select))
		{
			if (GameCore.IsShopMenuEnabled)
			{
				FaceNumber = CursorFaceIndex;
			}
			GameCore.PlaySound("menu_item_tv_show");
			OnFaceChanged?.Invoke(CursorFaceIndex);
			Hide();
			if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
			{
				_shopMenu.PanelHeroStats.SelectFaceTV();
			}
		}
		_showPercentage = Utils.Clamp((EndPos.X - Position.X) / (EndPos.X - StartPos.X), 0f, 1f);
		UpdateSlider(elapsedTime);
		if (_isSliderDragged)
		{
			_shopMenu.CursorType = CursorTypeEnum.Selected;
		}
		else if (_isSliderUnderCursor || _isSliderBarUnderCursor || CursorFaceIndex >= 0)
		{
			_shopMenu.CursorType = CursorTypeEnum.Select;
		}
	}

	private Vector2 GetFacePosition(int index)
	{
		return new Vector2(Position.X - 385f + 55f + (float)((index - _firstVisibleFace) * 111), Position.Y - 9f);
	}

	private Vector2 GetSliderPosition()
	{
		float num = ((FaceCount - 7 > 0) ? ((float)_firstVisibleFace / (float)(FaceCount - 7)) : 0f);
		return new Vector2(Position.X - 344f + 689f * num + 3f, Position.Y + 78f);
	}

	public override void Draw()
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		base.Draw();
		_shopMenu.SpriteBatchBegin(bLight: true);
		Color color = new Color(1f, 1f, 1f, 1f - _showPercentage);
		Transformation transformation;
		for (int i = _firstVisibleFace; i < _firstVisibleFace + 7; i++)
		{
			transformation = Transformation.CreateByPosition(GetFacePosition(i));
			_atlasFaces.DrawFrame(_shopMenu.SpriteBatchLight, i, color, transformation, BlendModeEnum.Normal);
			_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 0, color, Transformation.CreateByPosition(transformation.Position.X, transformation.Position.Y - 87f), BlendModeEnum.Normal);
			_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 1, color, Transformation.CreateByPosition(transformation.Position.X, transformation.Position.Y + 96f), BlendModeEnum.Normal);
			_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 4, new Color(color, (float)(color.A / 255) * _currSliderBarLightAlpha / 255f), Transformation.CreateByPosition(transformation.Position.X, transformation.Position.Y + 96f), BlendModeEnum.Normal);
			if (i == CursorFaceIndex)
			{
				_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchLight, 5, color, transformation, BlendModeEnum.Additive);
			}
		}
		transformation = Transformation.CreateByPosition(Position.X - 400f, Position.Y);
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 2, color, transformation, BlendModeEnum.Normal);
		transformation = Transformation.CreateByPosition(Position.X + 405f, Position.Y);
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 3, color, transformation, BlendModeEnum.Normal);
		_sliderObject.MyObject.Alpha = color.A;
		_sliderObject.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
