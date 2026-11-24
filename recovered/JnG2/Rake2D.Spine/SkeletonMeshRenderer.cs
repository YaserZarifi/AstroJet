using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spine;

namespace Rake2D.Spine;

public class SkeletonMeshRenderer
{
	private const int TL = 0;

	private const int TR = 1;

	private const int BL = 2;

	private const int BR = 3;

	private MeshBatcher batcher;

	private float[] vertices = new float[8];

	private int[] quadTriangles = new int[6] { 0, 1, 2, 1, 3, 2 };

	private BlendState defaultBlendState;

	private float _lightFactor;

	private Layer _myLayer;

	private bool _premultipliedAlpha;

	public GraphicsDevice Device { get; private set; }

	public bool PremultipliedAlpha
	{
		get
		{
			return _premultipliedAlpha;
		}
		set
		{
			_premultipliedAlpha = value;
		}
	}

	public SkeletonMeshRenderer(GraphicsDevice device)
	{
		Device = device;
		batcher = new MeshBatcher();
		Bone.yDown = true;
	}

	public void Begin(Layer layer, float lightFactor)
	{
		_lightFactor = lightFactor;
		_myLayer = layer;
		defaultBlendState = (_premultipliedAlpha ? BlendState.AlphaBlend : BlendState.NonPremultiplied);
		GameCore.EffectLightBasic.SetLightFactor(layer, lightFactor);
		Device.RasterizerState = GameCore.RasterizerState;
		Device.BlendState = defaultBlendState;
	}

	public void End()
	{
		batcher.Draw(Device);
	}

	public void Draw(Skeleton skeleton, Matrix worldMatrix, bool bForceAdditive = false)
	{
		float[] array = vertices;
		ExposedList<Slot> drawOrder = skeleton.DrawOrder;
		Slot[] items = skeleton.DrawOrder.Items;
		float r = skeleton.R;
		float g = skeleton.G;
		float b = skeleton.B;
		float a = skeleton.A;
		int i = 0;
		for (int count = drawOrder.Count; i < count; i++)
		{
			Slot slot = items[i];
			Attachment attachment = slot.Attachment;
			if (attachment is RegionAttachment)
			{
				RegionAttachment regionAttachment = (RegionAttachment)attachment;
				if (slot.Bone.HitFactor != GameCore.EffectLightBasic.HitFactor)
				{
					End();
					Begin(_myLayer, _lightFactor);
					GameCore.EffectLightBasic.SetHitFactor(slot.Bone.HitFactor * Options.HitEffectFactor);
				}
				MeshItem meshItem = batcher.NextItem(4, 6);
				meshItem.triangles = quadTriangles;
				VertexPositionColorTexture[] array2 = meshItem.vertices;
				AtlasRegion atlasRegion = (AtlasRegion)regionAttachment.RendererObject;
				meshItem.texture = (Texture2D)atlasRegion.page.rendererObject;
				float num = a * slot.A * regionAttachment.A;
				Color color;
				if (_premultipliedAlpha)
				{
					color = ((!((slot.Data.BlendMode == BlendMode.additive) | bForceAdditive)) ? new Color(r * slot.R * regionAttachment.R * num, g * slot.G * regionAttachment.G * num, b * slot.B * regionAttachment.B * num, num) : new Color(r * slot.R * regionAttachment.R * num, g * slot.G * regionAttachment.G * num, b * slot.B * regionAttachment.B * num, 0f));
				}
				else
				{
					color = new Color(r * slot.R * regionAttachment.R, g * slot.G * regionAttachment.G, b * slot.B * regionAttachment.B, num);
				}
				array2[0].Color = color;
				array2[2].Color = color;
				array2[3].Color = color;
				array2[1].Color = color;
				regionAttachment.ComputeWorldVertices(slot.Bone, array);
				array2[0].Position.X = array[0];
				array2[0].Position.Y = array[1];
				array2[0].Position.Z = 0f;
				array2[2].Position.X = array[2];
				array2[2].Position.Y = array[3];
				array2[2].Position.Z = 0f;
				array2[3].Position.X = array[4];
				array2[3].Position.Y = array[5];
				array2[3].Position.Z = 0f;
				array2[1].Position.X = array[6];
				array2[1].Position.Y = array[7];
				array2[1].Position.Z = 0f;
				array2[0].Position = Vector3.Transform(array2[0].Position, worldMatrix);
				array2[2].Position = Vector3.Transform(array2[2].Position, worldMatrix);
				array2[3].Position = Vector3.Transform(array2[3].Position, worldMatrix);
				array2[1].Position = Vector3.Transform(array2[1].Position, worldMatrix);
				float[] uVs = regionAttachment.UVs;
				array2[0].TextureCoordinate.X = uVs[0];
				array2[0].TextureCoordinate.Y = uVs[1];
				array2[2].TextureCoordinate.X = uVs[2];
				array2[2].TextureCoordinate.Y = uVs[3];
				array2[3].TextureCoordinate.X = uVs[4];
				array2[3].TextureCoordinate.Y = uVs[5];
				array2[1].TextureCoordinate.X = uVs[6];
				array2[1].TextureCoordinate.Y = uVs[7];
			}
			else if (attachment is MeshAttachment)
			{
				MeshAttachment meshAttachment = (MeshAttachment)attachment;
				int num2 = meshAttachment.Vertices.Length;
				if (array.Length < num2)
				{
					array = new float[num2];
				}
				meshAttachment.ComputeWorldVertices(slot, array);
				int[] triangles = meshAttachment.Triangles;
				MeshItem meshItem2 = batcher.NextItem(num2, triangles.Length);
				meshItem2.triangles = triangles;
				AtlasRegion atlasRegion2 = (AtlasRegion)meshAttachment.RendererObject;
				meshItem2.texture = (Texture2D)atlasRegion2.page.rendererObject;
				float num3 = a * slot.A * meshAttachment.A;
				Color color2 = ((!_premultipliedAlpha) ? new Color(r * slot.R * meshAttachment.R, g * slot.G * meshAttachment.G, b * slot.B * meshAttachment.B, num3) : new Color(r * slot.R * meshAttachment.R * num3, g * slot.G * meshAttachment.G * num3, b * slot.B * meshAttachment.B * num3, num3));
				float[] uVs2 = meshAttachment.UVs;
				VertexPositionColorTexture[] array3 = meshItem2.vertices;
				int num4 = 0;
				for (int j = 0; j < num2; j += 2)
				{
					array3[num4].Color = color2;
					array3[num4].Position.X = array[j];
					array3[num4].Position.Y = array[j + 1];
					array3[num4].Position.Z = 0f;
					array3[num4].TextureCoordinate.X = uVs2[j];
					array3[num4].TextureCoordinate.Y = uVs2[j + 1];
					array3[num4].Position = Vector3.Transform(array3[num4].Position, worldMatrix);
					num4++;
				}
			}
		}
	}
}
