using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Debug;
using Rake2D.JnG;

namespace Rake2D;

public class EffectLightBase
{
	public static string[] LightFactorNames;

	public static string[] LightSamplerNames;

	public Effect MyEffect;

	private Layer _layer;

	protected bool _useLights = true;

	public float LightFactor { get; private set; }

	public float TotalLightFactor { get; private set; }

	public float HitFactor { get; private set; }

	public float ShadowFactor { get; private set; }

	static EffectLightBase()
	{
		LightFactorNames = new string[3];
		LightSamplerNames = new string[3];
		for (int i = 0; i < 3; i++)
		{
			LightFactorNames[i] = $"lightFactor{i}";
			LightSamplerNames[i] = $"lightSampler{i}";
		}
	}

	public bool SetHitFactor(float hitFactor)
	{
		if (hitFactor != HitFactor)
		{
			HitFactor = hitFactor;
			MyEffect.Parameters["hitFactor"].SetValue(hitFactor);
			return true;
		}
		return false;
	}

	public void SetUseLights(bool bUse)
	{
		_useLights = bUse;
	}

	public void SetShadowFactor(float factor)
	{
		ShadowFactor = factor;
		MyEffect.Parameters["shadowFactor"].SetValue(factor);
	}

	public void SetLightFactor(Layer layer, float lightFactor)
	{
		if (layer == null || (LightFactor == lightFactor && _layer == layer))
		{
			return;
		}
		LightFactor = lightFactor;
		_layer = layer;
		TotalLightFactor = 0f;
		for (int i = 0; i < GameCore.LightMapArray.Length; i++)
		{
			if (layer.LightMapFactorArray[i] > 0f)
			{
				float num = layer.LightMapFactorArray[i] * lightFactor;
				MyEffect.Parameters[LightFactorNames[i]].SetValue(num);
				TotalLightFactor += num;
			}
			else
			{
				MyEffect.Parameters[LightFactorNames[i]].SetValue(0f);
			}
		}
	}

	public void SetLightFactor(float lightFactor0, float lightFactor1, float lightFactor2)
	{
		MyEffect.Parameters[LightFactorNames[0]].SetValue(lightFactor0);
		MyEffect.Parameters[LightFactorNames[1]].SetValue(lightFactor1);
		MyEffect.Parameters[LightFactorNames[2]].SetValue(lightFactor2);
		LightFactor = lightFactor1;
		TotalLightFactor = lightFactor0 + lightFactor1 + lightFactor2;
		DebugInfo.CurrentLayer.AddLightFactor(new Vector3(lightFactor0, lightFactor1, lightFactor2));
	}

	public void Apply()
	{
		if (_useLights)
		{
			MyEffect.CurrentTechnique.Passes["PassNormal"].Apply();
		}
		else
		{
			MyEffect.CurrentTechnique.Passes["PassNoLight"].Apply();
		}
	}

	public void SetLightMaps(bool bUseShopMaps)
	{
		if (bUseShopMaps)
		{
			for (int i = 0; i < GameCore.LightMapArray.Length; i++)
			{
				MyEffect.Parameters[LightSamplerNames[i]].SetValue(GameJnG.ShopMenu.LightMapArray[i].RenderTarget);
			}
		}
		else
		{
			for (int j = 0; j < GameCore.LightMapArray.Length; j++)
			{
				MyEffect.Parameters[LightSamplerNames[j]].SetValue(GameCore.LightMapArray[j].RenderTarget);
			}
		}
	}
}
