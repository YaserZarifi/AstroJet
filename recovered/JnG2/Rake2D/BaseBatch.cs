using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class BaseBatch
{
	protected const int MAX_VERTICES = 512;

	protected const int MAX_INDICES = 1024;

	private BasicEffect _basicEffect;

	private bool _hasBegun;

	protected VertexPositionColor[] _vertexArray;

	protected int _vertexCount;

	protected int[] _indexArray;

	protected int _indexCount;

	protected GraphicsDevice _graphics;

	public BaseBatch(GraphicsDevice graphics)
	{
		_graphics = graphics;
		_basicEffect = new BasicEffect(_graphics)
		{
			Alpha = 1f,
			LightingEnabled = false,
			VertexColorEnabled = true,
			FogEnabled = false
		};
		_basicEffect.View = Matrix.CreateLookAt(new Vector3(0f, 0f, 1f), Vector3.Zero, Vector3.Up);
		_vertexArray = new VertexPositionColor[512];
		_indexArray = new int[1024];
	}

	public virtual void Begin(BlendState blendState, Matrix worldMatrix)
	{
		Flush();
		_graphics.BlendState = blendState;
		_basicEffect.World = worldMatrix;
		_vertexCount = 0;
		_indexCount = 0;
		_hasBegun = true;
	}

	protected virtual void DrawPrimitives()
	{
	}

	public void Flush()
	{
		_basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0f, _graphics.Viewport.Width, _graphics.Viewport.Height, 0f, 0f, 1f);
		_basicEffect.CurrentTechnique.Passes[0].Apply();
		if (_vertexCount > 1)
		{
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
}
