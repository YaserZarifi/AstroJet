using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D;

public class Allocator
{
	private InstanceBuffer<Transformation> Transformations = new InstanceBuffer<Transformation>(4096);

	private InstanceBuffer<GameObject> GameObjects = new InstanceBuffer<GameObject>(4096);

	private InstanceBuffer<List<GameObject>> GameObjectLists = new InstanceBuffer<List<GameObject>>(4096);

	private InstanceBuffer<Particle> Particles = new InstanceBuffer<Particle>(4096);

	private InstanceBuffer<List<Particle>> ParticleLists = new InstanceBuffer<List<Particle>>(4096);

	private InstanceBuffer<StripLine> StripLines = new InstanceBuffer<StripLine>(4096);

	private InstanceBuffer<List<StripLine>> StripLineLists = new InstanceBuffer<List<StripLine>>(4096);

	private InstanceBuffer<PolygonVertex> PolygonVertices = new InstanceBuffer<PolygonVertex>(16384);

	private InstanceBuffer<List<PolygonVertex>> PolygonVertexLists = new InstanceBuffer<List<PolygonVertex>>(4096);

	private InstanceBuffer<PolygonRenderer> PolygonRenderers = new InstanceBuffer<PolygonRenderer>(4096);

	private InstanceBuffer<SpriteAnimator> SpriteAnimators = new InstanceBuffer<SpriteAnimator>(4096);

	private InstanceBuffer<SpriteRenderer> SpriteRenderers = new InstanceBuffer<SpriteRenderer>(4096);

	private InstanceBuffer<BatchVertextTextured[]> BatchVertextTextured8 = new InstanceBuffer<BatchVertextTextured[]>(4096);

	private InstanceBuffer<BatchVertextTextured[]> BatchVertextTextured16 = new InstanceBuffer<BatchVertextTextured[]>(4096);

	private InstanceBuffer<BatchVertextTextured[]> BatchVertextTextured32 = new InstanceBuffer<BatchVertextTextured[]>(4096);

	private InstanceBuffer<BatchVertextTextured[]> BatchVertextTexturedLarge = new InstanceBuffer<BatchVertextTextured[]>(4096);

	private int BatchVertextTexturedLargeSize = 64;

	private InstanceBuffer<List<Vector2>> Vector2Lists = new InstanceBuffer<List<Vector2>>(4096);

	private InstanceBuffer<Vector2[]> RectangleVertices = new InstanceBuffer<Vector2[]>(4096);

	private const int GoCooldownLength = 10;

	private const int GoCooldownClearResources = 5;

	private List<GameObject>[] GoCooldown = new List<GameObject>[10];

	public Allocator()
	{
		for (int i = 0; i < GoCooldown.Length; i++)
		{
			GoCooldown[i] = new List<GameObject>();
		}
	}

	public void Update()
	{
		if (GameCore.IsEditorEnabled)
		{
			for (int i = 0; i < GoCooldown.Length; i++)
			{
				GoCooldown[i].Clear();
			}
			return;
		}
		List<GameObject> list = GoCooldown[5];
		int count = list.Count;
		for (int j = 0; j < count; j++)
		{
			list[j].ClearResources();
		}
		list = GoCooldown[GoCooldown.Length - 1];
		list.Clear();
		for (int k = 0; k < GoCooldown.Length - 1; k++)
		{
			GoCooldown[k + 1] = GoCooldown[k];
		}
		GoCooldown[0] = list;
	}

	public Transformation GetTransformation()
	{
		Transformation transformation = Transformations.Get();
		if (transformation == null)
		{
			return new Transformation();
		}
		return transformation;
	}

	public void DeleteTransformation(Transformation t)
	{
		if (t != null)
		{
			t.ClearTransformation(isInit: false);
			Transformations.Put(t);
		}
	}

	public GameObject GetGameObject()
	{
		GameObject gameObject = GameObjects.Get();
		if (gameObject == null)
		{
			return new GameObject();
		}
		return gameObject;
	}

	public void DeleteGameObject(GameObject go)
	{
		if (go != null)
		{
			GoCooldown[0].Add(go);
		}
	}

	public List<GameObject> GetGameObjectList()
	{
		return GameObjectLists.Get() ?? new List<GameObject>();
	}

	public List<GameObject> GetGameObjectList(int capacity)
	{
		return GameObjectLists.Get() ?? new List<GameObject>(capacity);
	}

	public List<GameObject> CloneGameObjectList(List<GameObject> other)
	{
		List<GameObject> gameObjectList = GetGameObjectList();
		gameObjectList.AddList(other);
		return gameObjectList;
	}

	public List<GameObject> DeleteOwningGameObjectList(List<GameObject> l)
	{
		if (l == null)
		{
			return null;
		}
		int count = l.Count;
		for (int i = 0; i < count; i++)
		{
			l[i].Destroy();
			DeleteGameObject(l[i]);
		}
		l.Clear();
		GameObjectLists.Put(l);
		return null;
	}

	public List<GameObject> DeleteAliasingGameObjectList(List<GameObject> l)
	{
		if (l == null)
		{
			return null;
		}
		l.Clear();
		GameObjectLists.Put(l);
		return null;
	}

	public Particle GetParticle()
	{
		return Particles.Get() ?? new Particle();
	}

	public void DeleteParticle(Particle p)
	{
		if (p != null)
		{
			p.ClearParticle();
			Particles.Put(p);
		}
	}

	public List<Particle> GetParticleList()
	{
		return ParticleLists.Get() ?? new List<Particle>();
	}

	public List<Particle> DeleteOwningParticleList(List<Particle> l)
	{
		if (l == null)
		{
			return null;
		}
		int count = l.Count;
		for (int i = 0; i < count; i++)
		{
			l[i].Destroy();
			DeleteParticle(l[i]);
		}
		l.Clear();
		ParticleLists.Put(l);
		return null;
	}

	public List<Particle> DeleteAliasingParticleList(List<Particle> l)
	{
		if (l == null)
		{
			return null;
		}
		l.Clear();
		ParticleLists.Put(l);
		return null;
	}

	public StripLine GetStripLine(Layer l)
	{
		StripLine stripLine = StripLines.Get();
		if (stripLine == null)
		{
			return new StripLine(l);
		}
		stripLine.MyLayer = l;
		return stripLine;
	}

	public void DeleteStripLine(StripLine s)
	{
		s?.ClearStripLine();
	}

	public List<StripLine> GetStripLineList()
	{
		return StripLineLists.Get() ?? new List<StripLine>();
	}

	public List<StripLine> DeleteOwningStripLineList(List<StripLine> l)
	{
		if (l == null)
		{
			return null;
		}
		int count = l.Count;
		for (int i = 0; i < count; i++)
		{
			l[i].Destroy();
			DeleteStripLine(l[i]);
		}
		l.Clear();
		StripLineLists.Put(l);
		return null;
	}

	public List<StripLine> DeleteAliasingStripLineList(List<StripLine> l)
	{
		if (l == null)
		{
			return null;
		}
		l.Clear();
		StripLineLists.Put(l);
		return null;
	}

	public PolygonVertex GetPolygonVertex(Vector2 position)
	{
		PolygonVertex polygonVertex = PolygonVertices.Get();
		if (polygonVertex == null)
		{
			return new PolygonVertex(position);
		}
		polygonVertex.Position = position;
		polygonVertex.Color = Color.White;
		return polygonVertex;
	}

	public PolygonVertex GetPolygonVertex(Vector2 position, Color color)
	{
		PolygonVertex polygonVertex = PolygonVertices.Get();
		if (polygonVertex == null)
		{
			return new PolygonVertex(position, color);
		}
		polygonVertex.Position = position;
		polygonVertex.Color = color;
		return polygonVertex;
	}

	public PolygonVertex ClonePolygonVertex(PolygonVertex v)
	{
		return GetPolygonVertex(v.Position, v.Color);
	}

	public void DeletePolygonVertex(PolygonVertex v)
	{
		if (v != null)
		{
			PolygonVertices.Put(v);
		}
	}

	public List<PolygonVertex> GetPolygonVertexList()
	{
		return PolygonVertexLists.Get() ?? new List<PolygonVertex>();
	}

	public List<PolygonVertex> DeleteOwningPolygonVertexList(List<PolygonVertex> l)
	{
		if (l == null)
		{
			return null;
		}
		int count = l.Count;
		for (int i = 0; i < count; i++)
		{
			DeletePolygonVertex(l[i]);
		}
		l.Clear();
		PolygonVertexLists.Put(l);
		return null;
	}

	public List<PolygonVertex> DeleteAliasingPolygonVertexList(List<PolygonVertex> l)
	{
		if (l == null)
		{
			return null;
		}
		l.Clear();
		PolygonVertexLists.Put(l);
		return null;
	}

	public PolygonRenderer GetPolygonRenderer(PolygonData data, GameObject go)
	{
		PolygonRenderer polygonRenderer = PolygonRenderers.Get() ?? new PolygonRenderer();
		polygonRenderer.Init(data, go);
		return polygonRenderer;
	}

	public void DeletePolygonRenderer(PolygonRenderer p)
	{
		if (p != null)
		{
			p.ClearPolygonRenderer();
			PolygonRenderers.Put(p);
		}
	}

	public SpriteAnimator GetSpriteAnimator()
	{
		return SpriteAnimators.Get() ?? new SpriteAnimator();
	}

	public void DeleteSpriteAnimator(SpriteAnimator s)
	{
		if (s != null)
		{
			s.ClearSpriteAnimator();
			SpriteAnimators.Put(s);
		}
	}

	public SpriteRenderer GetSpriteRenderer(SpriteAtlas atlas, string skinSuffix = null)
	{
		SpriteRenderer spriteRenderer = SpriteRenderers.Get();
		if (spriteRenderer == null)
		{
			return new SpriteRenderer(atlas, skinSuffix);
		}
		spriteRenderer.SetAtlas(atlas, skinSuffix);
		return spriteRenderer;
	}

	public void DeleteSpriteRenderer(SpriteRenderer s)
	{
		if (s != null)
		{
			s.ClearSpriteRenderer();
			SpriteRenderers.Put(s);
		}
	}

	public BatchVertextTextured[] GetBatchVertextTextured(int size)
	{
		BatchVertextTextured[] array = null;
		int num = size;
		if (size <= 8)
		{
			array = BatchVertextTextured8.Get();
			num = 8;
		}
		else if (size <= 16)
		{
			array = BatchVertextTextured16.Get();
			num = 16;
		}
		else if (size <= 32)
		{
			array = BatchVertextTextured32.Get();
			num = 32;
		}
		else
		{
			array = BatchVertextTexturedLarge.Get();
			num = (BatchVertextTexturedLargeSize = Math.Max(size, BatchVertextTexturedLargeSize));
		}
		if (array == null || array.Length < size)
		{
			return new BatchVertextTextured[num];
		}
		return array;
	}

	public void DeleteBatchVertextTextured(BatchVertextTextured[] a)
	{
		if (a != null)
		{
			if (a.Length <= 8)
			{
				BatchVertextTextured8.Put(a);
			}
			else if (a.Length <= 16)
			{
				BatchVertextTextured16.Put(a);
			}
			else if (a.Length <= 32)
			{
				BatchVertextTextured32.Put(a);
			}
			else
			{
				BatchVertextTexturedLarge.Put(a);
			}
		}
	}

	public List<Vector2> GetVector2List()
	{
		return Vector2Lists.Get() ?? new List<Vector2>();
	}

	public List<Vector2> DeleteVector2List(List<Vector2> l)
	{
		if (l == null)
		{
			return null;
		}
		l.Clear();
		Vector2Lists.Put(l);
		return null;
	}

	public Vector2[] GetRectangleVertices()
	{
		return RectangleVertices.Get() ?? new Vector2[4];
	}

	public Vector2[] DeleteRectangleVertices(Vector2[] r)
	{
		if (r == null)
		{
			return null;
		}
		RectangleVertices.Put(r);
		return null;
	}

	public void Report()
	{
		Transformations.Report();
		GameObjects.Report();
		GameObjectLists.Report();
		Particles.Report();
		ParticleLists.Report();
		StripLines.Report();
		StripLineLists.Report();
		PolygonVertices.Report();
		PolygonVertexLists.Report();
		PolygonRenderers.Report();
		SpriteAnimators.Report();
		SpriteRenderers.Report();
		BatchVertextTextured8.Report();
		BatchVertextTextured16.Report();
		BatchVertextTextured32.Report();
		BatchVertextTexturedLarge.Report();
		Vector2Lists.Report();
		RectangleVertices.Report();
	}

	public string FormatGameForDebug()
	{
		return $"Alloc - Game: trn {Transformations.Position} / {Transformations.Capacity},  go {GameObjects.Position} / {GameObjects.Capacity}, gol {GameObjectLists.Position} / {GameObjectLists.Capacity}";
	}

	public string FormatRenderAForDebug()
	{
		return $"Alloc - Render: prt {Particles.Position} / {Particles.Capacity}, prtl {ParticleLists.Position} / {ParticleLists.Capacity}, str {StripLines.Position} / {StripLines.Capacity}, strl {StripLineLists.Position} / {StripLineLists.Capacity}, pv {PolygonVertices.Position} / {PolygonVertices.Capacity}, pvl {PolygonVertexLists.Position} / {PolygonVertexLists.Capacity}, pr {PolygonRenderers.Position} / {PolygonRenderers.Capacity}";
	}

	public string FormatRenderBForDebug()
	{
		return $"Alloc - Render: sa {SpriteAnimators.Position} / {SpriteAnimators.Capacity}, sr {SpriteRenderers.Position} / {SpriteRenderers.Capacity}, bvt8 {BatchVertextTextured8.Position} / {BatchVertextTextured8.Capacity}, bvt16 {BatchVertextTextured16.Position} / {BatchVertextTextured16.Capacity}, bvt32 {BatchVertextTextured32.Position} / {BatchVertextTextured32.Capacity}, bvtl {BatchVertextTexturedLarge.Position} / {BatchVertextTexturedLarge.Capacity}";
	}

	public string FormatHelpersForDebug()
	{
		return $"Helpers: v2l {Vector2Lists.Position} / {Vector2Lists.Capacity}, rv {RectangleVertices.Position} / {RectangleVertices.Capacity}";
	}
}
