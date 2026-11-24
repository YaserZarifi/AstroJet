using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class PanelComponent
{
	public Vector2 Position;

	public int Width;

	public int Height;

	public Color BackgroundColor;

	public Color ForegroundColor;

	public object UserData;

	public bool Enabled;

	public bool IsFocusable;

	public string Tooltip;

	public Vector2 _oldScreenPosition;

	public Panel Panel;

	protected SpriteRenderer _spriteRenderer;

	public Vector2 ScreenPosition => Panel.Position + Position;

	public Editor Editor => Panel.Editor;

	public int AtlasFrameNumber
	{
		get
		{
			if (_spriteRenderer == null)
			{
				return 0;
			}
			return _spriteRenderer.Frame;
		}
		set
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.Frame = value;
			}
		}
	}

	public int ScreenX => (int)(Panel.Position.X + Position.X);

	public int ScreenY => (int)(Panel.Position.Y + Position.Y);

	public bool IsMoved => _oldScreenPosition != ScreenPosition;

	public bool HasFocus => Editor.FocusComponent == this;

	public PanelComponent(SpriteAtlas atlas, Panel panel)
	{
		Panel = panel;
		if (Panel != null)
		{
			Panel.AddComponent(this);
		}
		if (atlas != null)
		{
			_spriteRenderer = new SpriteRenderer(atlas);
		}
		Position = default;
		Enabled = true;
		BackgroundColor = Color.White;
		ForegroundColor = Color.Black;
	}

	public void Destroy()
	{
		Panel?.RemoveComponent(this);
	}

	public virtual void Update(float elapsedTime)
	{
		if (Editor.DragAction == DragAction.None)
		{
			_oldScreenPosition = ScreenPosition;
		}
	}

	public virtual void UpdateTop(float elapsedTime)
	{
	}

	public virtual void Draw()
	{
		if (_spriteRenderer != null)
		{
			Matrix matrix = Matrix.CreateTranslation(Panel.Position.X + Position.X, Panel.Position.Y + Position.Y, 0f);
			_spriteRenderer.Draw(matrix, Panel.Editor.SpriteBatch);
		}
	}

	public virtual void DrawTop()
	{
	}

	public virtual void SetFocus(bool bIgnoreFirstInput)
	{
		Editor.SetFocusComponent(this);
		Editor.SetPanelFocus(Panel);
	}

	public virtual void CancelFocus()
	{
	}
}
