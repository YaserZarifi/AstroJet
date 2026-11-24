using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;

namespace Rake2D.JnG;

public class BossMeter
{
	public Vector2 Position;

	public float TargetPositionY;

	public static readonly Vector2 Size = new Vector2(512f, 18f);

	private static SpriteAtlas _atlas;

	private static BMFont _font;

	private GameObject _bossObject;

	private string _bossName;

	private SpriteBatch _spriteBatch;

	private TriangleBatchTextured _triangleBatchTextured;

	private float _hideAlphaFactor;

	public bool IsDestroyed
	{
		get
		{
			if (_bossObject != null)
			{
				return !_bossObject.IsAlive;
			}
			return true;
		}
	}

	public BossMeter(GameObject go, string name)
	{
		_bossObject = go;
		_bossName = name;
		_triangleBatchTextured = new TriangleBatchTextured(GameCore.GraphicsDeviceStatic);
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
	}

	public static void Init()
	{
		_atlas = GameCore.GetSpriteAtlas("boss_meter");
		_font = GameCore.GetBMFont("shop_font");
	}

	public void Update(float elapsedTime)
	{
		if (_bossObject != null)
		{
			Utils.ChangeValueToTarget(ref Position.Y, TargetPositionY, 200f * elapsedTime);
		}
		bool flag = false;
		foreach (Hero hero in GameJnG.HeroList)
		{
			GameObject controlledObject = hero.GetControlledObject();
			if (controlledObject != null && controlledObject.ScreenPosition.Y < Position.Y + 40f && controlledObject.ScreenPosition.X > Position.X - 40f && controlledObject.ScreenPosition.X < Position.X + Size.X + 40f)
			{
				flag = true;
				break;
			}
		}
		Utils.ChangeValueToTarget(ref _hideAlphaFactor, flag ? 0.3f : 1f, 3f * elapsedTime);
	}

	public void Draw()
	{
		if (_bossObject != null)
		{
			Color color = new Color(1f, 1f, 1f, _hideAlphaFactor);
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			_atlas.DrawFrame(_spriteBatch, 0, color, Transformation.CreateByPosition(Position), BlendModeEnum.Normal);
			_spriteBatch.End();
			_triangleBatchTextured.Begin(BlendState.Additive, GameCore.ViewPortMatrix, null, 0f);
			_atlas.GetFrameUV(1, out var topLeft, out var bottomRight);
			float num = ((_bossObject.MaxHP > 0f) ? (_bossObject.CurrentHP / _bossObject.MaxHP) : 0f);
			_triangleBatchTextured.DrawRect(Position, Position + new Vector2(Size.X * num, Size.Y), color, _atlas.Texture, topLeft, new Vector2(topLeft.X + (bottomRight.X - topLeft.X) * num, bottomRight.Y));
			_triangleBatchTextured.End();
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			_font.Write(_spriteBatch, _bossName ?? _bossObject.Id, Position.X + 4f, Position.Y - 14f, new Vector2(0.5f), new Color(255, 255, 255), TextAlign.Left, (int)(Size.X - 10f), _hideAlphaFactor);
			long num2 = ((_bossObject.CurrentHP > 0f && _bossObject.CurrentHP <= 1f) ? 1 : ((long)_bossObject.CurrentHP));
			_font.Write(_spriteBatch, $"\\3{num2}\\- / {(long)_bossObject.MaxHP}", Position.X + Size.X - 4f, Position.Y - 14f, new Vector2(0.5f), new Color(255, 200, 0), TextAlign.Right, (int)(Size.X - 10f), _hideAlphaFactor);
			_spriteBatch.End();
		}
	}
}
