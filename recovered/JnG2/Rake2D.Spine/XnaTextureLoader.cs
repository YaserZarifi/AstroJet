using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace Rake2D.Spine;

public class XnaTextureLoader : TextureLoader
{
	private GraphicsDevice device;

	public XnaTextureLoader(GraphicsDevice device)
	{
		this.device = device;
	}

	public void Load(AtlasPage page, string path)
	{
		Texture2D texture2D = (Texture2D)(page.rendererObject = DataLoader.LoadTexture(path));
		page.width = texture2D.Width;
		page.height = texture2D.Height;
	}

	public void Unload(object texture)
	{
		((Texture2D)texture).Dispose();
	}
}
