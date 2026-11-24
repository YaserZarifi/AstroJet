using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class FaceTV : Transformation
{
	public int FaceNumber;

	public GameModeEnum GameMode;

	public bool IsSelected;

	public bool CheckCheater = true;

	private ShopMenu _shopMenu;

	private SpriteAtlas _atlasFaceTV;

	private SpriteAtlas _atlasFace;

	private TVNoise _tvNoise;

	public bool IsNoiseVisible
	{
		get
		{
			if (_tvNoise == null)
			{
				return false;
			}
			return _tvNoise.IsVisible;
		}
	}

	public FaceTV(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		_atlasFaceTV = GameCore.GetSpriteAtlas("profile_face_tv", logWhenNotFound: true);
		_atlasFace = GameCore.GetSpriteAtlas("profile_face", logWhenNotFound: true);
		_tvNoise = new TVNoise(_shopMenu, TVNoiseTypeEnum.PlayerFace);
		_tvNoise.ParentTransformation = this;
		_tvNoise.Position = new Vector2(0f, 0f);
		_tvNoise.StripOffsetYRange = new FloatRange(87f, -87f);
		_tvNoise.Init();
		_tvNoise.IsAutoHiding = false;
		_tvNoise.FadeSpeed = 1500f;
	}

	public void ShowNoise()
	{
		_tvNoise.Show();
	}

	public void HideNoise()
	{
		_tvNoise.Hide();
	}

	public void Update(float elapsedTime)
	{
		_tvNoise.Update(elapsedTime);
	}

	public void Draw()
	{
		Transformation transformation = Transformation.CreateByPosition(WorldPosition);
		_shopMenu.TriangleBatch.Begin(BlendState.AlphaBlend, GameCore.ViewPortMatrix);
		_shopMenu.TriangleBatch.DrawRect(WorldPosition.X - 62f, WorldPosition.Y - 87f, 125f, 176f, Color.FromNonPremultiplied(0, 0, 0, Color.A));
		_shopMenu.TriangleBatch.End();
		_shopMenu.SpriteBatchBegin(bLight: true);
		_atlasFace.DrawFrame(_shopMenu.SpriteBatchLight, FaceNumber, Color, transformation);
		_tvNoise.Draw();
		_atlasFaceTV.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color, transformation);
		if (IsSelected)
		{
			_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchLight, 4, Color, Transformation.CreateByPosition(WorldPosition), BlendModeEnum.Additive);
		}
		if (GameJnG.IsCheater && CheckCheater)
		{
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("hero_profile_cheater"), Position.X, Position.Y - 30f, new Vector2(0.65f), new Color(255, 0, 0, 255), TextAlign.Center, 130);
		}
		_shopMenu.DrawGameModeSkull(GameMode, transformation, Color.A);
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
