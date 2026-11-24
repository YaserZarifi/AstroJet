using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class BaseBatchTextured
{
	internal const int MAX_VERTICES = 512;

	internal const int MAX_INDICES = 1530;

	private Texture2D _lastTexture;

	private bool _hasBegun;

	protected VertexPositionColorTexture[] _vertexArray;

	protected int _vertexCount;

	protected int[] _indexArray;

	protected int _indexCount;

	public Matrix CurrentMatrix;

	protected GraphicsDevice _graphics;

	static BaseBatchTextured()
	{
	}

	public BaseBatchTextured(GraphicsDevice graphics)
	{
		_graphics = graphics;
		_vertexArray = new VertexPositionColorTexture[512];
		_indexArray = new int[1530];
	}

	public virtual void Begin(BlendState blendState, Matrix worldMatrix, SamplerState samplerState = null, float lightFactor = 1f)
	{
		if (samplerState != null)
		{
			_graphics.SamplerStates[0] = samplerState;
		}
		CurrentMatrix = worldMatrix;
		_graphics.BlendState = blendState;
		GameCore.EffectLightBasic.Begin(blendState, worldMatrix, GameCore.CurrentLevel.CurrentLayer, lightFactor);
		_vertexCount = 0;
		_indexCount = 0;
		_hasBegun = true;
	}

	protected virtual void DrawPrimitives()
	{
	}

	protected void Flush()
	{
		if (_vertexCount > 1)
		{
			GameCore.EffectLightBasic.SetTexture(_lastTexture);
			GameCore.EffectLightBasic.Apply();
			DrawPrimitives();
		}
		_vertexCount = 0;
		_indexCount = 0;
	}

	public void End()
	{
		Flush();
		_hasBegun = false;
	}

	protected void SetTexture(Texture2D texture)
	{
		if (_lastTexture != texture)
		{
			Flush();
			_lastTexture = texture;
		}
	}
}
