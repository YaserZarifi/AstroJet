using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class LightMap
{
	public RenderTarget2D RenderTarget;

	private const int _targetW = 1024;

	private const int _targetH = 1024;

	private SpriteBatch _spriteBatch;

	public List<GameObject> LightObjectList;

	public LightMap()
	{
		LightObjectList = GameCore.Allocator.GetGameObjectList();
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
	}

	public void Reset()
	{
		LightObjectList.Clear();
	}

	public void Init()
	{
		RenderTarget = new RenderTarget2D(GameCore.GraphicsDeviceManager.GraphicsDevice, 1024, 1024, mipMap: false, GameCore.GraphicsDeviceStatic.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents, shared: false);
	}

	public void Render(Camera camera)
	{
		Matrix value = Matrix.CreateScale(1024f / (float)GameCore.MainViewPort.Width, 1024f / (float)GameCore.MainViewPort.Height, 1f);
		GameCore.GraphicsDeviceStatic.SetRenderTarget(RenderTarget);
		GameCore.GraphicsDeviceStatic.Clear(Color.Black);
		_spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, value);
		foreach (GameObject lightObject in LightObjectList)
		{
			if (lightObject.SpriteRenderer != null && !lightObject.IsDestroyed)
			{
				Color color = lightObject.Color;
				color *= (float)(int)color.A / 255f;
				color.A = 0;
				_spriteBatch.Draw(lightObject.SpriteRenderer.Atlas.Texture, lightObject.ScreenPosition, lightObject.SpriteRenderer.Atlas.GetFrameRectangle(lightObject.Frame), color, lightObject.WorldRotation, lightObject.SpriteRenderer.Atlas.GetFramePivot(lightObject.Frame), lightObject.WorldScale * camera.Zoom * camera.Scale, SpriteEffects.None, 0f);
			}
		}
		_spriteBatch.End();
	}
}
