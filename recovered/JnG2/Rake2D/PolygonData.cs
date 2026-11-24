using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class PolygonData
{
	public string TextureId;

	public string AtlasId;

	public string BottomTextureId;

	public bool CreatePhysBody = true;

	public bool UseAtlasPivot = true;

	public Vector2 PivotOffset;

	public Texture2D Texture;

	public Texture2D TopTexture;

	public Texture2D BottomTexture;

	public SpriteAtlas Atlas;

	public int AtlasFrame;

	public string TopTextureId;

	public string TopAtlasId;

	public SpriteAtlas TopAtlas;

	public int[] TopAtlasFrameArray;

	public int TopOffset;

	public int BottomOffset;

	public PolygonTextureMappingModeEnum TextureMappingMode;

	public float[] VertextArray;

	public PolygonOritentation Orientation;

	public SamplerState SamplerState = SamplerState.PointWrap;

	public PolygonData Copy()
	{
		PolygonData polygonData = new PolygonData();
		polygonData.TextureId = TextureId;
		polygonData.Texture = Texture;
		polygonData.Orientation = Orientation;
		polygonData.AtlasId = AtlasId;
		polygonData.AtlasFrame = AtlasFrame;
		polygonData.Atlas = Atlas;
		polygonData.TopOffset = TopOffset;
		polygonData.TopTextureId = TopTextureId;
		polygonData.TopTexture = TopTexture;
		polygonData.TopAtlasId = TopAtlasId;
		if (TopAtlasFrameArray != null)
		{
			polygonData.TopAtlasFrameArray = new int[TopAtlasFrameArray.Length];
			TopAtlasFrameArray.CopyTo(polygonData.TopAtlasFrameArray, 0);
		}
		polygonData.TopAtlas = TopAtlas;
		polygonData.BottomOffset = BottomOffset;
		polygonData.BottomTextureId = BottomTextureId;
		polygonData.BottomTexture = BottomTexture;
		polygonData.TextureMappingMode = TextureMappingMode;
		polygonData.SamplerState = SamplerState;
		polygonData.CreatePhysBody = CreatePhysBody;
		polygonData.PivotOffset = PivotOffset;
		if (VertextArray != null)
		{
			polygonData.VertextArray = new float[VertextArray.Length];
			VertextArray.CopyTo(polygonData.VertextArray, 0);
		}
		return polygonData;
	}

	public void Init(PartData partData)
	{
		if (AtlasId != null)
		{
			Atlas = GameCore.GetSpriteAtlas(AtlasId);
			if (Atlas == null)
			{
				GameCore.LogErr("Unable to find atlas '{0}' in game content for polygon part data [{1}]' in file '{2}'.", AtlasId, partData.GetHierarchyId(), partData.FileName);
			}
			else
			{
				Texture = Atlas.Texture;
			}
		}
		else if (TextureId != null)
		{
			Texture = GameCore.GetTexture2D(TextureId);
			if (Texture == null)
			{
				GameCore.LogErr("Unable to find texture '{0}' in game content for polygon part data [{1}]' in file '{2}'.", TextureId, partData.GetHierarchyId(), partData.FileName);
			}
		}
		if (TopAtlasId != null)
		{
			TopAtlas = GameCore.GetSpriteAtlas(TopAtlasId);
			if (TopAtlas == null)
			{
				GameCore.LogErr("Unable to find top atlas '{0}' in game content for polygon part data [{1}]' in file '{2}'.", TopAtlasId, partData.GetHierarchyId(), partData.FileName);
			}
			else
			{
				TopTexture = TopAtlas.Texture;
			}
		}
		else if (TopTextureId != null)
		{
			TopTexture = GameCore.GetTexture2D(TopTextureId);
			if (TopTexture == null)
			{
				GameCore.LogErr("Unable to find texture '{0}' in game content for polygon part data [{1}]' in file '{2}'.", TopTextureId, partData.GetHierarchyId(), partData.FileName);
			}
		}
		if (BottomTextureId != null)
		{
			BottomTexture = GameCore.GetTexture2D(BottomTextureId);
			if (BottomTexture == null)
			{
				GameCore.LogErr("Unable to find texture '{0}' in game content for part data [{1}]' in file '{2}'.", BottomTextureId, partData.GetHierarchyId(), partData.FileName);
			}
		}
		if (Atlas != null && UseAtlasPivot)
		{
			Rectangle frameRectangle = Atlas.GetFrameRectangle(AtlasFrame);
			Vector2 framePivot = Atlas.GetFramePivot(AtlasFrame);
			PivotOffset = new Vector2(framePivot.X - (float)(frameRectangle.Width / 2), framePivot.Y - (float)(frameRectangle.Height / 2));
		}
	}

	public float[] GetDefaultVertices()
	{
		float[] array = null;
		if (VertextArray == null && Texture != null)
		{
			array = new float[8];
			float num;
			float num2;
			if (Atlas != null)
			{
				Rectangle frameRectangle = Atlas.GetFrameRectangle(AtlasFrame);
				num = frameRectangle.Width;
				num2 = frameRectangle.Height;
			}
			else
			{
				num = Texture.Width;
				num2 = Texture.Height;
			}
			switch (Orientation)
			{
			case PolygonOritentation.TopBottom:
				array[0] = (0f - num) / 2f - PivotOffset.X;
				array[1] = (0f - num2) / 2f - PivotOffset.Y;
				array[2] = num / 2f - PivotOffset.X;
				array[3] = (0f - num2) / 2f - PivotOffset.Y;
				array[4] = (0f - num) / 2f - PivotOffset.X;
				array[5] = num2 / 2f - PivotOffset.Y;
				array[6] = num / 2f - PivotOffset.X;
				array[7] = num2 / 2f - PivotOffset.Y;
				break;
			case PolygonOritentation.BottomTop:
				array[0] = num / 2f - PivotOffset.X;
				array[1] = num2 / 2f - PivotOffset.Y;
				array[2] = (0f - num) / 2f - PivotOffset.X;
				array[3] = num2 / 2f - PivotOffset.Y;
				array[4] = num / 2f - PivotOffset.X;
				array[5] = (0f - num2) / 2f - PivotOffset.Y;
				array[6] = (0f - num) / 2f - PivotOffset.X;
				array[7] = (0f - num2) / 2f - PivotOffset.Y;
				break;
			default:
				array[0] = (0f - num) / 2f - PivotOffset.X;
				array[1] = num2 / 2f - PivotOffset.Y;
				array[2] = (0f - num) / 2f - PivotOffset.X;
				array[3] = (0f - num2) / 2f - PivotOffset.Y;
				array[4] = num / 2f - PivotOffset.X;
				array[5] = num2 / 2f - PivotOffset.Y;
				array[6] = num / 2f - PivotOffset.X;
				array[7] = (0f - num2) / 2f - PivotOffset.Y;
				break;
			}
		}
		return array;
	}
}
