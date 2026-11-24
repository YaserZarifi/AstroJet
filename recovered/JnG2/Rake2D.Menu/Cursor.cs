using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class Cursor : Transformation
{
	public bool IsPositionChanged;

	private SpriteRenderer _spriteRenderer;

	private SpriteAtlas _atlasSymbol;

	private ShopMenu _shopMenu;

	private Vector2 _oldPosition;

	private float _symbolAngle;

	private CursorSymbolEnum _symbol;

	public CursorTypeEnum CursorType { get; private set; }

	public ShopObject LightObject { get; private set; }

	public Cursor(ShopMenu menu)
	{
		SpriteAtlas spriteAtlas = GameCore.GetSpriteAtlas("cursor");
		_atlasSymbol = GameCore.GetSpriteAtlas("cursor_symbol");
		_spriteRenderer = GameCore.Allocator.GetSpriteRenderer(spriteAtlas);
		_shopMenu = menu;
		Position = default;
		PartData partData = GameCore.GetPartData("shop_cursor_light");
		if (partData != null)
		{
			LightObject = new ShopObject(_shopMenu, _shopMenu.LayerShop, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
			LightObject.MyObject.Activate();
			LightObject.MyObject.ParentTransformation = this;
			LightObject.MyObject.CreateTime = -1f;
		}
	}

	public void SetCursorType(CursorTypeEnum type)
	{
		if (CursorType != type)
		{
			CursorType = type;
			switch (CursorType)
			{
			case CursorTypeEnum.NoAction:
				_spriteRenderer.ChangeAnim("hover_none", 1f);
				_symbol = CursorSymbolEnum.None;
				break;
			case CursorTypeEnum.Select:
				_spriteRenderer.ChangeAnim("hover_active", 0.5f);
				_symbol = CursorSymbolEnum.None;
				break;
			case CursorTypeEnum.Unselect:
				_spriteRenderer.ChangeAnim("selected", 1f);
				_symbol = CursorSymbolEnum.Unselect;
				break;
			case CursorTypeEnum.Selected:
				_spriteRenderer.ChangeAnim("selected", 1f);
				_symbol = CursorSymbolEnum.None;
				break;
			case CursorTypeEnum.Swap:
				_spriteRenderer.ChangeAnim("selected", 1f);
				_symbol = CursorSymbolEnum.Swap;
				break;
			case CursorTypeEnum.Place:
				_spriteRenderer.ChangeAnim("selected", 1f);
				_symbol = CursorSymbolEnum.Place;
				break;
			case CursorTypeEnum.Buy:
				_spriteRenderer.ChangeAnim("selected", 1f);
				_symbol = CursorSymbolEnum.Buy;
				break;
			case CursorTypeEnum.TooExpensive:
				_spriteRenderer.ChangeAnim("denied", 1f);
				_symbol = CursorSymbolEnum.TooExpensive;
				break;
			case CursorTypeEnum.Denied:
				_spriteRenderer.ChangeAnim("denied", 1f);
				_symbol = CursorSymbolEnum.Denied;
				break;
			case CursorTypeEnum.Install:
				_spriteRenderer.ChangeAnim("hover_active", 0.5f);
				_symbol = CursorSymbolEnum.Install;
				break;
			case CursorTypeEnum.NoRoom:
				_spriteRenderer.ChangeAnim("denied", 0.5f);
				_symbol = CursorSymbolEnum.NoRoom;
				break;
			case CursorTypeEnum.DragHorizontalyHover:
				_spriteRenderer.ChangeAnim("drag_h_hover", 1f);
				break;
			case CursorTypeEnum.DragHorizontalyPressed:
				_spriteRenderer.ChangeAnim("drag_h_pressed", 1f);
				break;
			}
		}
	}

	public void UpdatePosition()
	{
		ResetPrecalculations();
		Position.X = GameInput.MouseX;
		Position.Y = GameInput.MouseY;
		Vector2 vector = new Vector2(1920f, 1080f) * GameCore.VirtualAspectRatio;
		Vector2 vector2 = new Vector2(GameCore.GraphicsDeviceStatic.Viewport.Width, GameCore.GraphicsDeviceStatic.Viewport.Height) - vector;
		Position -= vector2 / 2f;
		Position /= GameCore.VirtualAspectRatio;
		if (_oldPosition != Position)
		{
			IsPositionChanged = true;
			_oldPosition = Position;
		}
		else
		{
			IsPositionChanged = false;
		}
	}

	public void Update(float elapsedTime)
	{
		if (GameCore.IsApplicationActive)
		{
			UpdatePosition();
			if (_symbol == CursorSymbolEnum.Buy)
			{
				_symbolAngle += 15f * elapsedTime;
			}
			else
			{
				_symbolAngle += 10f * elapsedTime;
			}
			_spriteRenderer.Update(elapsedTime);
			LightObject.MyObject.MyCamera.SetViewPort(GameCore.MainViewPort);
			LightObject.MyObject.MyCamera.SetLayer(LightObject.MyObject.MyLayer);
			LightObject.Update(elapsedTime);
		}
	}

	public void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: false);
		if (_shopMenu.State == ShopMenuState.Hiding)
		{
			_spriteRenderer.Color = new Color(255, 255, 255, (int)(255f * _shopMenu.ShowHidePercentage));
		}
		else
		{
			_spriteRenderer.Color = Color.White;
		}
		_spriteRenderer.Draw(this, _shopMenu.SpriteBatchNoLight, BlendModeEnum.Normal);
		_shopMenu.SpriteBatchEnd(bLight: false);
	}

	public void DrawSymbol(Vector2 position)
	{
		Transformation transformation = Transformation.CreateByPosition(position);
		_shopMenu.SpriteBatchBegin(bLight: false);
		switch (_symbol)
		{
		case CursorSymbolEnum.Place:
			transformation.Position.Y += (float)Math.Sin(_symbolAngle) * 5f;
			_atlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, Color.White, transformation, BlendModeEnum.Normal);
			break;
		case CursorSymbolEnum.Swap:
			transformation.Rotation += (float)Math.Sin(_symbolAngle) * 0.75f;
			_atlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, Color.White, transformation, BlendModeEnum.Normal);
			break;
		case CursorSymbolEnum.Buy:
			transformation.Rotation += (float)Math.Sin(_symbolAngle) * 0.35f;
			_atlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 2, Color.White, transformation, BlendModeEnum.Normal);
			break;
		case CursorSymbolEnum.TooExpensive:
			transformation.Scale.X += (float)Math.Sin(_symbolAngle) * 0.15f;
			transformation.Scale.Y = transformation.Scale.X;
			_atlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 3, Color.White, transformation, BlendModeEnum.Normal);
			break;
		case CursorSymbolEnum.Denied:
			transformation.Scale.X += (float)Math.Sin(_symbolAngle) * 0.15f;
			transformation.Scale.Y = transformation.Scale.X;
			_atlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 4, Color.White, transformation, BlendModeEnum.Normal);
			break;
		case CursorSymbolEnum.Install:
			transformation.Rotation += (float)Math.Sin(_symbolAngle) * 0.35f;
			_atlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 6, Color.White, transformation, BlendModeEnum.Normal);
			break;
		case CursorSymbolEnum.NoRoom:
			transformation.Rotation += (float)Math.Sin(_symbolAngle) * 0.35f;
			_atlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 7, Color.White, transformation, BlendModeEnum.Normal);
			break;
		case CursorSymbolEnum.Unselect:
			transformation.Scale.X += (float)Math.Sin(_symbolAngle) * 0.15f;
			transformation.Scale.Y = transformation.Scale.X;
			_atlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 5, Color.White, transformation, BlendModeEnum.Normal);
			break;
		}
		_shopMenu.SpriteBatchEnd(bLight: false);
	}

	public bool IsInCircle(Vector2 pos, int radius)
	{
		return Vector2.Distance(Position, pos) <= (float)radius;
	}

	public bool IsInRect(ref Rectangle rect)
	{
		return IsInRect(rect.X, rect.Y, rect.Width, rect.Height);
	}

	public bool IsInRect(Vector2 position, int width, int height)
	{
		return IsInRect(position.X, position.Y, width, height);
	}

	public bool IsInRect(float x, float y, int width, int height)
	{
		if (Position.X >= x && Position.X < x + (float)width && Position.Y >= y)
		{
			return Position.Y < y + (float)height;
		}
		return false;
	}
}
