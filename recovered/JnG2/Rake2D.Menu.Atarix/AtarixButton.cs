using Microsoft.Xna.Framework;

namespace Rake2D.Menu.Atarix;

public class AtarixButton : Transformation
{
	public enum AtarixButtonStateEnum
	{
		Released,
		Pressing,
		Pressed,
		Releasing
	}

	public delegate void OnEventHandler(AtarixButton button);

	public AtarixButtonStateEnum State;

	public bool IsSelected;

	public bool IsUnderCursor;

	public int Frame;

	private Atarix _atarix;

	private ShopMenu _shopMenu;

	private float _clickPercentage;

	private float _highliteAlpha;

	private static SpriteAtlas _atlas;

	private static SpriteAtlas _atlasHighlight;

	public bool IsPressed
	{
		get
		{
			if (State != AtarixButtonStateEnum.Pressing)
			{
				return State == AtarixButtonStateEnum.Pressed;
			}
			return true;
		}
	}

	public event OnEventHandler OnClick;

	public AtarixButton(Atarix atarix, ShopMenu shopMenu)
	{
		_atarix = atarix;
		_shopMenu = shopMenu;
		ParentTransformation = atarix;
		ColorMode = ColorMode.ParentAlpha;
	}

	public static void InitStatic()
	{
		_atlas = GameCore.GetSpriteAtlas("atarix_button");
		_atlasHighlight = GameCore.GetSpriteAtlas("atarix_button_highlight");
	}

	public void Press()
	{
		if (State == AtarixButtonStateEnum.Released)
		{
			State = AtarixButtonStateEnum.Pressing;
			_clickPercentage = 0f;
		}
		OnClick?.Invoke(this);
	}

	public void Release()
	{
		if (State == AtarixButtonStateEnum.Pressed)
		{
			State = AtarixButtonStateEnum.Releasing;
		}
	}

	public void Update(float elapsedTime)
	{
		ResetPrecalculations();
		switch (State)
		{
		case AtarixButtonStateEnum.Pressing:
			if (Utils.ChangeValueToTarget(ref _clickPercentage, 1f, 12f * elapsedTime))
			{
				State = AtarixButtonStateEnum.Pressed;
			}
			break;
		case AtarixButtonStateEnum.Pressed:
			_clickPercentage = 1f;
			break;
		case AtarixButtonStateEnum.Releasing:
			if (Utils.ChangeValueToTarget(ref _clickPercentage, 0f, 12f * elapsedTime))
			{
				State = AtarixButtonStateEnum.Released;
			}
			break;
		}
		Rectangle frameRectangle = _atlas.GetFrameRectangle(Frame);
		IsUnderCursor = _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && _shopMenu.Cursor.IsInRect(WorldPosition.X - (float)(frameRectangle.Width / 2) - 3f, WorldPosition.Y - (float)(frameRectangle.Height / 2) - 3f, frameRectangle.Width + 6, frameRectangle.Height + 6);
		Utils.ChangeValueToTarget(ref _highliteAlpha, IsSelected ? 1 : 0, 5f * elapsedTime);
	}

	public void Draw()
	{
		int num = (int)(255f * (1f - _clickPercentage / 3f));
		Color color = new Color(num, num, num, Color.A);
		Transformation transformation = Transformation.CreateByPosition(WorldPosition);
		transformation.Scale = new Vector2(1f - _clickPercentage / 10f);
		if (_highliteAlpha < 1f)
		{
			_atlas.DrawFrame(_shopMenu.SpriteBatchLight, Frame, color, transformation, BlendModeEnum.Normal);
		}
		if (_highliteAlpha > 0f)
		{
			_atlasHighlight.DrawFrame(_shopMenu.SpriteBatchLight, Frame, new Color(color, _highliteAlpha), transformation, BlendModeEnum.Normal);
		}
	}
}
