using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class EffectDistortion
{
	public Effect Effect;

	public RenderTarget2D RenderTarget;

	public void Init()
	{
		byte[] effectCode = File.ReadAllBytes(string.Format("shader/EffectDistortion_{0}.mgfxo", "GL"));
		Effect = new Effect(GameCore.GraphicsDeviceStatic, effectCode);
		RenderTarget = new RenderTarget2D(GameCore.GraphicsDeviceStatic, 512, 512, mipMap: false, GameCore.GraphicsDeviceStatic.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents, shared: false);
	}

	public void Begin()
	{
		Effect.Parameters["distortionSampler"].SetValue(RenderTarget);
	}

	public void Apply()
	{
		Effect.CurrentTechnique.Passes["PassNormal"].Apply();
	}
}
