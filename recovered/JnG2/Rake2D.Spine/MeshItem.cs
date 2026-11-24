using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Spine;

public class MeshItem
{
	public Texture2D texture;

	public int vertexCount;

	public int triangleCount;

	public VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[0];

	public int[] triangles = new int[0];
}
