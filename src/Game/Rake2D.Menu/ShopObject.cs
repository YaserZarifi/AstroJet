using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Menu;

public class ShopObject
{
	public GameObject MyObject;

	private ShopMenu _shopMenu;

	private float _oldFactor;

	public ShopObject(ShopMenu shopMenu, Layer layer, GameObject go)
	{
		_shopMenu = shopMenu;
		MyObject = go;
		MyObject.SetLayer(layer);
		MyObject.Activate();
	}

	public void Update(float elapsedTime)
	{
		MyObject.Update(elapsedTime);
	}

	public void SetPosition(Vector2 pos)
	{
		MyObject.SetPosition(pos);
	}

	private void DrawRestore()
	{
		_oldFactor = GameCore.EffectLight.LightFactor;
		GameCore.EffectLight.SetLightFactor(MyObject.MyLayer, MyObject.PartData.LightFactor);
		GameCore.EffectLight.SetLightMaps(bUseShopMaps: true);
		_shopMenu.SpriteBatchBegin(bLight: true);
	}

	public void Draw()
	{
		if (!MyObject.IsEnabled || !MyObject.IsVisible)
		{
			return;
		}
		if (MyObject.ParticleListBack != null)
		{
			Particle.DrawParticleList(MyObject.MyLayer, MyObject.ParticleListBack);
			if (MyObject.PartData != null)
			{
				GameCore.EffectLight.SetLightFactor(MyObject.MyLayer, MyObject.PartData.LightFactor);
			}
		}
		if (MyObject.ChildObjectListBack != null)
		{
			foreach (GameObject item in MyObject.ChildObjectListBack)
			{
				item.Draw(_shopMenu.SpriteBatchLight);
			}
		}
		if (MyObject.PolygonRenderer != null)
		{
			_shopMenu.SpriteBatchLight.End();
			GameCore.TriangleBatchTextured.End();
			GameCore.TriangleBatchTextured.Begin(DataUtils.BlendMode2State(MyObject.PartData.BlendMode), GameCore.LevelMatrix, MyObject.PolygonRenderer.Data.SamplerState, MyObject.PartData.LightFactor);
			GameCore.EffectLightBasic.SetHitFactor(MyObject.HitEffectTimer * Options.HitEffectFactor);
			MyObject.PolygonRenderer.SetColor(MyObject.Color.R, MyObject.Color.G, MyObject.Color.B, MyObject.Color.A);
			MyObject.PolygonRenderer.Draw(GameCore.TriangleBatchTextured);
			GameCore.TriangleBatchTextured.End();
			DrawRestore();
		}
		if (MyObject.SpineRenderer != null)
		{
			_shopMenu.SpriteBatchLight.End();
			MyObject.SpineRenderer.SetColor(MyObject.Color.R, MyObject.Color.G, MyObject.Color.B, MyObject.Color.A);
			MyObject.SpineRenderer.Draw(MyObject.WorldMatrix, MyObject.PartData.LightFactor);
			DrawRestore();
		}
		if (MyObject.PartData != null && MyObject.PartData.LightOrShadow == 0 && MyObject.SpriteRenderer != null)
		{
			MyObject.SpriteRenderer.Color = MyObject.Color;
			if (MyObject.HitEffectTimer != GameCore.EffectLight.HitFactor || ((MyObject.PartData.LightFactor != GameCore.EffectLight.LightFactor || GameCore.EffectLight.ShadowFactor != MyObject.MyLayer.ShadowMapFactor * GameCore.EffectLight.LightFactor) && MyObject.SpriteRenderer.Atlas.GetCurentBlendMode(GameCore.SpriteBatch, MyObject.PartData.BlendMode) != BlendModeEnum.Additive))
			{
				_shopMenu.SpriteBatchLight.End();
				DrawRestore();
				MyObject.SpriteRenderer.Draw(MyObject.WorldTransformation, _shopMenu.SpriteBatchLight, MyObject.PartData.BlendMode);
				_shopMenu.SpriteBatchLight.End();
				GameCore.EffectLight.SetLightFactor(MyObject.MyLayer, _oldFactor);
				_shopMenu.SpriteBatchLight.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, GameCore.RasterizerState, GameCore.GetRenderEffect(MyObject.MyLayer), GameCore.ViewPortMatrix);
			}
			else
			{
				MyObject.SpriteRenderer.Draw(MyObject.WorldTransformation, _shopMenu.SpriteBatchLight, MyObject.PartData.BlendMode);
			}
		}
		if (MyObject.ChildObjectListFront == null)
		{
			return;
		}
		foreach (GameObject item2 in MyObject.ChildObjectListFront)
		{
			item2.Draw(_shopMenu.SpriteBatchLight);
		}
	}
}
