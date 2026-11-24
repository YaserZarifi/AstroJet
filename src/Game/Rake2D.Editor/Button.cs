using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

internal class Button : FieldComponent
{
	public delegate void OnClickHandler(Button button);

	public delegate void OnRightClickHandler(Button button);

	public string Caption;

	public bool HasClickEffect = true;

	public TextAlign CaptionAlign = TextAlign.Center;

	public SpriteAtlas IconAtlas;

	public int IconFrame;

	public Keys ShortcutKey;

	public bool ShortCutControl;

	public bool ShortCutShift;

	public bool ShortCutAlt;

	private float _colorFactor;

	private int _clickDirection;

	private int _highliteDirection;

	private const float _maxHigliteFactor = 1.25f;

	public bool IsEnabled = true;

	public event OnClickHandler OnClick;

	public event OnClickHandler OnDoubleClick;

	public event OnRightClickHandler OnRightClick;

	public Button(Panel panel)
		: this(null, panel)
	{
	}

	public Button(SpriteAtlas atlas, Panel panel)
		: base(atlas, panel)
	{
		_colorFactor = 1f;
		LabelWidth = 0;
		Width = 100;
		Height = 20;
		ValueColor = Color.White;
		ValueColorSelected = Color.Yellow;
		FrameColor = new Color(150, 150, 150, 200);
		BackgroundColor = new Color(50, 50, 50, 50);
	}

	public void Click()
	{
		if (HasClickEffect)
		{
			_clickDirection = -1;
		}
		else
		{
			_clickDirection = 0;
		}
		_highliteDirection = 0;
		OnClick?.Invoke(this);
	}

	public override void Update(float elapsedTime)
	{
		if (!IsVisible)
		{
			return;
		}
		base.Update(elapsedTime);
		if (IsEnabled && Panel.Editor.Cursor.IsInRect(ScreenPosition, Width, Height) && Editor.SelectedPanel == Panel)
		{
			IsSelected = true;
			Editor.Cursor.Text = Tooltip;
			Editor.SelectedComponent = this;
			Editor.SelectedPanel = Panel;
			if (GameInput.MousePressed(GameInput.MouseButton.Right))
			{
				OnRightClick?.Invoke(this);
			}
			else if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				Editor.DragAction = DragAction.OnPanelComponent;
				SetFocus(bIgnoreFirstInput: false);
				if (GameInput.DoubleClick)
				{
					OnDoubleClick?.Invoke(this);
				}
				else
				{
					Click();
				}
			}
			else
			{
				_highliteDirection = 1;
			}
		}
		else if (_colorFactor > 1f)
		{
			_highliteDirection = -1;
		}
		else
		{
			_highliteDirection = 0;
		}
		if (_clickDirection < 0)
		{
			_colorFactor -= 7f * elapsedTime;
			if (_colorFactor <= 0.75f)
			{
				_colorFactor = 0.75f;
				_clickDirection = 1;
			}
		}
		else if (_clickDirection > 0)
		{
			_colorFactor += 7f * elapsedTime;
			if (_colorFactor >= 1f)
			{
				_colorFactor = 1f;
				_clickDirection = 0;
			}
		}
		else if (_highliteDirection > 0)
		{
			_colorFactor += 5f * elapsedTime;
			if (_colorFactor >= 1.25f)
			{
				_colorFactor = 1.25f;
			}
		}
		else if (_highliteDirection < 0)
		{
			_colorFactor -= 6f * elapsedTime;
			if (_colorFactor <= 1f)
			{
				_colorFactor = 1f;
			}
		}
		if (IsVisible && Editor.IsKeyboardFree && !GameCore.Console.IsActive && ShortcutKey != Keys.None && GameInput.SystemKeyPressed(ShortcutKey) && ShortCutControl == GameInput.ControlDown && ShortCutShift == GameInput.ShiftDown && ShortCutAlt == GameInput.AltDown)
		{
			Click();
		}
	}

	private void DrawFrame()
	{
		Vector2 screenPosition = ScreenPosition;
		if (LabelPosition == LabelPosition.Left)
		{
			screenPosition.X += LabelWidth;
		}
		else
		{
			screenPosition.Y += LabelHeight + 1;
		}
		Vector2 bottomRight = new Vector2(screenPosition.X + (float)Width - 1f, screenPosition.Y + (float)Height - 1f);
		Editor.TriangleBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
		Editor.TriangleBatch.DrawRect(screenPosition + Vector2.One, bottomRight, BackgroundColor);
		Editor.TriangleBatch.End();
		Editor.LineBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
		Editor.LineBatch.DrawRect(screenPosition, Width, Height, FrameColor, RectangleFrameType.Raised);
		Editor.LineBatch.End();
	}

	public override void Draw()
	{
		if (!IsVisible)
		{
			return;
		}
		Color color = (IsSelected ? ValueColorSelected : ValueColor);
		if (!Enabled)
		{
			color *= 0.5f;
		}
		if (_spriteRenderer != null)
		{
			_spriteRenderer.Color = BackgroundColor * _colorFactor;
			_spriteRenderer.Color.A = BackgroundColor.A;
			if (!Enabled)
			{
				_spriteRenderer.Color *= 0.5f;
			}
			base.Draw();
		}
		else
		{
			base.Draw();
			DrawFrame();
		}
		if (LabelPosition == LabelPosition.Left)
		{
			Editor.Font.Write(Editor.SpriteBatch, Label, ScreenX, ScreenY, LabelColor);
		}
		else
		{
			Editor.Font.Write(Editor.SpriteBatch, Label, ScreenX, ScreenY, LabelColor, TextAlign.Left, Panel.Width - 10);
		}
		Vector2 vector = new Vector2(ScreenPosition.X + (float)LabelWidth, ScreenPosition.Y + (float)(Height / 2) - 8f);
		if (CaptionAlign == TextAlign.Center)
		{
			vector.X += Width / 2;
		}
		else
		{
			vector.X += 4f;
		}
		if (IconAtlas != null)
		{
			IconAtlas.DrawFrame(Editor.SpriteBatch, IconFrame, Color.White, Transformation.CreateByPosition(vector.X, vector.Y + 8f), BlendModeEnum.Normal);
		}
		if (Caption != null)
		{
			Editor.Font.Write(Panel.Editor.SpriteBatch, Caption, vector.X, vector.Y, color, CaptionAlign, Width - 8);
		}
	}

	public void SetHighlited()
	{
		_colorFactor = 1.25f;
	}
}
