using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Spine;

public class MeshBatcher
{
	private readonly List<MeshItem> items;

	private readonly Queue<MeshItem> freeItems;

	private VertexPositionColorTexture[] vertexArray = new VertexPositionColorTexture[0];

	private short[] triangles = new short[0];

	public MeshBatcher()
	{
		items = new List<MeshItem>(256);
		freeItems = new Queue<MeshItem>(256);
		EnsureCapacity(256, 512);
	}

	public MeshItem NextItem(int vertexCount, int triangleCount)
	{
		MeshItem meshItem = ((freeItems.Count > 0) ? freeItems.Dequeue() : new MeshItem());
		if (meshItem.vertices.Length < vertexCount)
		{
			meshItem.vertices = new VertexPositionColorTexture[vertexCount];
		}
		if (meshItem.triangles.Length < triangleCount)
		{
			meshItem.triangles = new int[triangleCount];
		}
		meshItem.vertexCount = vertexCount;
		meshItem.triangleCount = triangleCount;
		items.Add(meshItem);
		return meshItem;
	}

	private void EnsureCapacity(int vertexCount, int triangleCount)
	{
		if (vertexArray.Length < vertexCount)
		{
			vertexArray = new VertexPositionColorTexture[vertexCount];
		}
		if (triangles.Length < triangleCount)
		{
			triangles = new short[triangleCount];
		}
	}

	public void Draw(GraphicsDevice device)
	{
		if (items.Count == 0)
		{
			return;
		}
		int count = items.Count;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			MeshItem meshItem = items[i];
			num += meshItem.vertexCount;
			num2 += meshItem.triangleCount;
		}
		EnsureCapacity(num, num2);
		num = 0;
		num2 = 0;
		Texture2D texture2D = null;
		for (int j = 0; j < count; j++)
		{
			MeshItem meshItem2 = items[j];
			int vertexCount = meshItem2.vertexCount;
			if (meshItem2.texture != texture2D || num + vertexCount > 32767)
			{
				GameCore.EffectLightBasic.SetTexture(meshItem2.texture);
				FlushVertexArray(device, num, num2);
				num = 0;
				num2 = 0;
				texture2D = meshItem2.texture;
				device.Textures[0] = texture2D;
			}
			int[] array = meshItem2.triangles;
			int triangleCount = meshItem2.triangleCount;
			int num3 = 0;
			int num4 = num2;
			while (num3 < triangleCount)
			{
				triangles[num4] = (short)(array[num3] + num);
				num3++;
				num4++;
			}
			num2 += triangleCount;
			Array.Copy(meshItem2.vertices, 0, vertexArray, num, vertexCount);
			num += vertexCount;
			meshItem2.texture = null;
			freeItems.Enqueue(meshItem2);
		}
		GameCore.EffectLightBasic.SetTexture(texture2D);
		FlushVertexArray(device, num, num2);
		items.Clear();
	}

	private void FlushVertexArray(GraphicsDevice device, int vertexCount, int triangleCount)
	{
		if (vertexCount != 0)
		{
			GameCore.EffectLightBasic.Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexArray, 0, vertexCount, triangles, 0, triangleCount / 3, VertexPositionColorTexture.VertexDeclaration);
		}
	}
}
