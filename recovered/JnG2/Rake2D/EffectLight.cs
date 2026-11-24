using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class EffectLight : EffectLightBase
{
	public void Init()
	{
		try
		{
			byte[] effectCode = File.ReadAllBytes(string.Format("shader/EffectLight_{0}.mgfxo", "GL"));
			MyEffect = new Effect(GameCore.GraphicsDeviceManager.GraphicsDevice, effectCode);
			SetLightMaps(bUseShopMaps: false);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to init Light effect: {0}", ex);
		}
	}

	public void LayerBeginDraw(Layer layer)
	{
		MyEffect.Parameters["ambientLight"].SetValue(layer.AmbientLightColor.ToVector3() * layer.MyLevel.AmbientFactor);
		SetHitFactor(0f);
		SetLightFactor(layer, 1f);
		SetShadowFactor(layer.ShadowMapFactor * LightFactor);
		UpdateView();
	}

	public void UpdateView()
	{
		Viewport viewport = GameCore.GraphicsDeviceStatic.Viewport;
		MyEffect.Parameters["viewPos"].SetValue(new Vector2(viewport.X, viewport.Y));
		MyEffect.Parameters["viewSize"].SetValue(new Vector2(viewport.Width, viewport.Height));
	}
}
