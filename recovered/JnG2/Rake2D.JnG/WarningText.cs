using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;

namespace Rake2D.JnG;

public class WarningText
{
	public bool IsDestroyed;

	public Vector2 Offset;

	public float StartDelay;

	private static BMFont _font;

	private WarningTextStateEnum _state;

	private string _text;

	private SpriteBatch _spriteBatch;

	private Vector2 _scale;

	private Vector2 _targetScale;

	private Vector2 _initScale;

	private Color _color;

	private float _showDelay;

	public static bool Init()
	{
		_font = GameCore.GetBMFont("level_font");
		return _font != null;
	}

	public WarningText(string text, Color color)
	{
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		_initScale = new Vector2(10f, 0.005f);
		_scale = _initScale;
		_targetScale = Vector2.One;
		_color = color;
		_text = text;
		_state = WarningTextStateEnum.Showing;
	}

	public void Destroy()
	{
		IsDestroyed = true;
	}

	public void Update(float elapsedTime)
	{
		switch (_state)
		{
		case WarningTextStateEnum.Showing:
			if (StartDelay >= 0f)
			{
				StartDelay -= elapsedTime;
			}
			if (StartDelay <= 0f && Utils.ChangeValueToTarget(ref _scale, _targetScale, 50f * elapsedTime))
			{
				_showDelay = 3f;
				SetState(WarningTextStateEnum.Visible);
			}
			break;
		case WarningTextStateEnum.Visible:
			if (Utils.ChangeValueToTarget(ref _showDelay, 0f, elapsedTime))
			{
				SetState(WarningTextStateEnum.Hiding);
			}
			break;
		case WarningTextStateEnum.Hiding:
			if (Utils.ChangeValueToTarget(ref _scale, _initScale, 50f * elapsedTime))
			{
				Destroy();
			}
			break;
		}
	}

	public void SetState(WarningTextStateEnum state)
	{
		_state = state;
	}

	public void Draw()
	{
		if (StartDelay <= 0f)
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			Vector2 vector = new Vector2(960f, 540f - 20f * _scale.Y) + Offset;
			float alphaFactor = (_initScale.X - _scale.X) / MathHelper.Max(0.1f, _initScale.X - _targetScale.X);
			_font.Write(_spriteBatch, _text, vector.X, vector.Y, _scale, _color, TextAlign.Center, 0, alphaFactor);
			_spriteBatch.End();
		}
	}
}
