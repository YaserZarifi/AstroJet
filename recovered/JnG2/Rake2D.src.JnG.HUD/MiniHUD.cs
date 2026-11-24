using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;

namespace Rake2D.src.JnG.HUD;

public class MiniHUD
{
	private SpriteBatch _spriteBatch;

	private TriangleBatchTextured _triangleBatchTextured;

	private SpriteAtlas _atlas;

	private SpriteAtlas _colisoidAtlas;

	private Vector2 _position;

	private Vector2 _size;

	private Hero _myHero;

	private float _hpBlink;

	private float _drawManaPercent;

	public MiniHUD(Hero hero)
	{
		_myHero = hero;
		_atlas = GameCore.GetSpriteAtlas("hud_mini");
		_colisoidAtlas = GameCore.GetSpriteAtlas("hud_colisoid");
		_triangleBatchTextured = new TriangleBatchTextured(GameCore.GraphicsDeviceStatic);
		_size = new Vector2(_atlas.GetFrameRectangle(0).Width, _atlas.GetFrameRectangle(0).Height);
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
	}

	public void Update(float elapsedTime)
	{
		_position = _myHero.MyObject.WorldPosition - new Vector2(_size.X / 2f, 55f);
		_hpBlink += elapsedTime * 15f;
		Utils.ChangeValueToTarget(ref _drawManaPercent, _myHero.ManaPercent, 5f * elapsedTime);
	}

	public void DrawJetPackHUD()
	{
		Vector2 vector = _myHero.JetPackObject.WorldPosition - new Vector2(_size.X / 2f, 40f);
		float hPPercent = _myHero.JetPackObject.HPPercent;
		Vector2 topLeft;
		Vector2 bottomRight;
		if (hPPercent < 1f)
		{
			_atlas.GetFrameUV(0, out topLeft, out bottomRight);
		}
		else
		{
			_atlas.GetFrameUV(1, out topLeft, out bottomRight);
		}
		Color color;
		if (hPPercent >= 1f)
		{
			color = new Color(0, 255, 0);
		}
		else if (hPPercent >= 0.75f)
		{
			color = new Color(20, 230, 0);
		}
		else if (hPPercent >= 0.5f)
		{
			color = new Color(240, 240, 0);
		}
		else
		{
			color = ((!(hPPercent > 0.25f)) ? new Color(255, 0, 0, (int)(150f + 100f * Utils.Sin(_hpBlink))) : new Color(255, 100, 0));
		}
		if (hPPercent < 1f)
		{
			_triangleBatchTextured.DrawRect(vector, vector + _size, color * 0.5f, _atlas.Texture, topLeft, bottomRight);
		}
		_triangleBatchTextured.DrawRect(vector, vector + new Vector2(_size.X * hPPercent, _size.Y), color, _atlas.Texture, topLeft, new Vector2(topLeft.X + (bottomRight.X - topLeft.X) * hPPercent, bottomRight.Y));
	}

	private void DrawShipHUD()
	{
		Vector2 position = _position;
		Color color = Color.White;
		float num;
		int frameNumber;
		if (_myHero.Shield > 0f)
		{
			num = _myHero.ShieldPercent;
			frameNumber = ((!(num < 1f)) ? 5 : 4);
		}
		else
		{
			num = _myHero.MyObject.HPPercent;
			frameNumber = ((!(num < 1f)) ? 1 : 0);
			if (num < 0.25f)
			{
				color = new Color(1f, 1f, 1f, 0.5f + 0.5f * Utils.Sin(_hpBlink));
			}
		}
		_atlas.GetFrameUV(frameNumber, out var topLeft, out var bottomRight);
		if (num < 1f)
		{
			_triangleBatchTextured.DrawRect(position, position + _size, color * 0.5f, _atlas.Texture, topLeft, bottomRight);
		}
		_triangleBatchTextured.DrawRect(position, position + new Vector2(_size.X * num, _size.Y), color, _atlas.Texture, topLeft, new Vector2(topLeft.X + (bottomRight.X - topLeft.X) * num, bottomRight.Y));
		if (_drawManaPercent < 1f)
		{
			_atlas.GetFrameUV(2, out topLeft, out bottomRight);
		}
		else
		{
			_atlas.GetFrameUV(3, out topLeft, out bottomRight);
		}
		position.Y += _size.Y + 1f;
		color = Color.White;
		if (_drawManaPercent < 1f)
		{
			_triangleBatchTextured.DrawRect(position, position + _size, new Color(color * 0.5f, 100), _atlas.Texture, topLeft, bottomRight);
		}
		_triangleBatchTextured.DrawRect(position, position + new Vector2(_size.X * _drawManaPercent, _size.Y), color, _atlas.Texture, topLeft, new Vector2(topLeft.X + (bottomRight.X - topLeft.X) * _drawManaPercent, bottomRight.Y));
	}

	private void DrawBombs()
	{
		if (!(_myHero.CurrentBombCount < (float)_myHero.MaxBombCount))
		{
			return;
		}
		GameCore.EffectLight.SetLightFactor(0f, 0f, 0f);
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, GameCore.EffectLight.MyEffect, _myHero.MyObject.MyCamera.WorldMatrixMainLayer);
		Transformation transformation = Transformation.CreateByPosition(_myHero.MyObject.WorldPosition + new Vector2((float)(-_myHero.MaxBombCount) / 2f * 5f, 45f));
		for (int i = 0; i < _myHero.MaxBombCount; i++)
		{
			if ((float)i <= _myHero.CurrentBombCount - 1f)
			{
				_atlas.DrawFrame(_spriteBatch, 6, Color.White, transformation, BlendModeEnum.Additive);
			}
			else
			{
				_atlas.DrawFrame(_spriteBatch, 7, Color.White, transformation, BlendModeEnum.Normal);
			}
			transformation.Position.X += 5f;
		}
		_spriteBatch.End();
	}

	public void DrawColisoid()
	{
		_triangleBatchTextured.Begin(BlendState.Additive, _myHero.MyObject.MyCamera.WorldMatrixMainLayer, null, 0f);
		Transformation transformation = Transformation.CreateByPosition(_myHero.MyObject.WorldPosition);
		transformation.Rotation = _myHero.MyObject.Rotation;
		transformation.Scale = _myHero.MyObject.Scale;
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, GameCore.EffectLight.MyEffect, _myHero.MyObject.MyCamera.WorldMatrixMainLayer);
		_colisoidAtlas.DrawFrame(_spriteBatch, _myHero.PlayerNumber, Color.White, transformation, BlendModeEnum.Additive);
		_spriteBatch.End();
	}

	public void Draw()
	{
		_triangleBatchTextured.Begin(BlendState.Additive, _myHero.MyObject.MyCamera.WorldMatrixMainLayer, null, 0f);
		if (_myHero.IsJetPackActive)
		{
			DrawJetPackHUD();
		}
		if (!_myHero.IsDestroyed)
		{
			DrawShipHUD();
		}
		_triangleBatchTextured.End();
		if (!_myHero.IsDestroyed)
		{
			DrawBombs();
		}
	}
}
