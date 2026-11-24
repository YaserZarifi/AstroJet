using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class EffectLightBasic : EffectLightBase
{
	public void Init()
	{
		try
		{
			byte[] effectCode = File.ReadAllBytes(string.Format("shader/EffectLightBasic_{0}.mgfxo", "GL"));
			MyEffect = new Effect(GameCore.GraphicsDeviceStatic, effectCode);
			MyEffect.Parameters["view"].SetValue(Matrix.CreateLookAt(new Vector3(0f, 0f, 1f), Vector3.Zero, Vector3.Up));
			SetLightMaps(bUseShopMaps: false);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to init Basic Light effect: {0}", ex);
		}
	}

	public void Update()
	{
		Viewport viewport = GameCore.GraphicsDeviceStatic.Viewport;
		MyEffect.Parameters["projection"].SetValue(Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f));
		MyEffect.Parameters["viewPos"].SetValue(new Vector2(viewport.X, viewport.Y));
		MyEffect.Parameters["viewSize"].SetValue(new Vector2(viewport.Width, viewport.Height));
	}

	public void Begin(BlendState blendState, Matrix worldMatrix, Layer layer, float lightFactor)
	{
		MyEffect.Parameters["world"].SetValue(worldMatrix);
		if (blendState == BlendState.Additive || !GameCore.IsLightRenderingEnabled || layer == null)
		{
			SetUseLights(bUse: false);
			return;
		}
		MyEffect.Parameters["ambientLight"].SetValue(layer.AmbientLightColor.ToVector3() * layer.MyLevel.AmbientFactor);
		SetLightFactor(layer, lightFactor);
		float num = layer.ShadowMapFactor * lightFactor;
		if (Options.IsShadowsOn && layer.ShadowFromMapNumber >= 0 && layer.ShadowFromMapNumber < 3 && num > 0f)
		{
			MyEffect.Parameters["shadowSampler"].SetValue(GameCore.ShadowMapArray[layer.ShadowFromMapNumber].RenderTarget);
		}
		else
		{
			num = 0f;
		}
		SetShadowFactor(num);
		SetUseLights(num > 0f || TotalLightFactor > 0f);
	}

	public void SetTexture(Texture2D texture)
	{
		if (texture == null)
		{
			MyEffect.Parameters["useTexture"].SetValue(value: true);
			MyEffect.Parameters["s0"].SetValue((Texture)null);
		}
		else
		{
			MyEffect.Parameters["useTexture"].SetValue(value: true);
			MyEffect.Parameters["s0"].SetValue(texture);
		}
	}

	public void LayerBeginDraw(Layer layer)
	{
		SetHitFactor(0f);
	}
}
