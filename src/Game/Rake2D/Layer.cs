using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Debug;

namespace Rake2D;

public class Layer
{
	public string Id;

	public GameLevel MyLevel;

	public string EditorButtonCaption;

	public Color EditorButtonColor = Color.Black;

	public Vector2 ScrollFactor;

	public float LoopX = -1f;

	public float LoopOffsetX;

	public int LoopCount;

	public string CopyLoopLayerId;

	public int MinLOD;

	public Color AmbientLightColor;

	public bool IsEnabledByEditor;

	public bool IsEnabled = true;

	public bool UsePhysics;

	public bool UseDistortion = true;

	public bool IsInMainGroup;

	public List<LayerSegmentData> SegmentList;

	public List<Particle> ParticleListFront;

	public List<Particle> ParticleListBack;

	public List<StripLine> StripLineListFront;

	public List<StripLine> StripLineListBack;

	public List<GameObject> GameObjectList;

	public List<GameObject> VisibleGameObjectList;

	private VisibleObjectSorter VisibleObjectSorter;

	public int LightToMapNumber = 1;

	public int ShadowFromMapNumber = -1;

	public int ShadowToMapNumber = 1;

	public float ShadowMapFactor;

	public Vector2 ScrollSpeed => MyLevel.ScrollSpeed * ScrollFactor;

	public bool IsUpdatable
	{
		get
		{
			if (IsEnabled && IsEnabledByEditor)
			{
				if (MinLOD > Options.LevelOfDetails)
				{
					return GameCore.IsEditorEnabled;
				}
				return true;
			}
			return false;
		}
	}

	public OrderedDictionary PathDictionary { get; internal set; }

	public float[] LightMapFactorArray { get; private set; }

	public Layer(GameLevel level, string id, float scrollFactorX, float scrollFactorY)
	{
		MyLevel = level;
		Id = id;
		LightToMapNumber = -1;
		ScrollFactor = new Vector2(scrollFactorX, scrollFactorY);
		GameObjectList = GameCore.Allocator.GetGameObjectList();
		PathDictionary = new OrderedDictionary();
		ParticleListFront = GameCore.Allocator.GetParticleList();
		ParticleListBack = GameCore.Allocator.GetParticleList();
		StripLineListFront = GameCore.Allocator.GetStripLineList();
		StripLineListBack = GameCore.Allocator.GetStripLineList();
		AmbientLightColor = Color.White;
		LightMapFactorArray = new float[3];
		IsEnabledByEditor = true;
	}

	public void DestroyAll()
	{
		for (int num = GameObjectList.Count - 1; num >= 0; num--)
		{
			GameObjectList[num].Destroy();
		}
	}

	public void Clear()
	{
		LoopCount = 0;
		IsEnabled = true;
		if (SegmentList != null)
		{
			SegmentList.Clear();
		}
		GameCore.Allocator.DeleteOwningGameObjectList(GameObjectList);
		GameObjectList = GameCore.Allocator.GetGameObjectList();
		VisibleGameObjectList.Clear();
		PathDictionary.Clear();
		GameCore.Allocator.DeleteOwningParticleList(ParticleListFront);
		ParticleListFront = GameCore.Allocator.GetParticleList();
		GameCore.Allocator.DeleteOwningParticleList(ParticleListBack);
		ParticleListBack = GameCore.Allocator.GetParticleList();
		GameCore.Allocator.DeleteOwningStripLineList(StripLineListFront);
		StripLineListFront = GameCore.Allocator.GetStripLineList();
		GameCore.Allocator.DeleteOwningStripLineList(StripLineListBack);
		StripLineListBack = GameCore.Allocator.GetStripLineList();
	}

	public void Init()
	{
		LoopCount = 0;
		for (int num = GameObjectList.Count - 1; num >= 0; num--)
		{
			if (GameObjectList[num].IsDestroyed)
			{
				GameCore.Allocator.DeleteGameObject(GameObjectList[num]);
				GameObjectList.RemoveAt(num);
			}
		}
		VisibleGameObjectList = GameCore.Allocator.GetGameObjectList();
		VisibleObjectSorter = new VisibleObjectSorter();
		foreach (BezierPath value in PathDictionary.Values)
		{
			value.CalculateLength();
		}
		List<GameObject> list = GameObjectList.ToList();
		foreach (GameObject item in list)
		{
			item.Init();
		}
		foreach (GameObject item2 in list)
		{
			if (!item2.IsLevelInitDone)
			{
				item2.LevelInit();
			}
		}
	}

	public LayerSegmentData AddSegment()
	{
		LayerSegmentData layerSegmentData = new LayerSegmentData(this);
		if (SegmentList == null)
		{
			SegmentList = new List<LayerSegmentData>();
		}
		SegmentList.Add(layerSegmentData);
		return layerSegmentData;
	}

	public void Update(float elapsedTime)
	{
		if (IsEnabledByEditor)
		{
			DebugInfo.LayerParticleCountBack += ParticleListBack.Count;
			DebugInfo.LayerParticleCountFront += ParticleListFront.Count;
			DebugInfo.LayerStripLineCountBack += StripLineListBack.Count;
			DebugInfo.LayerStripLineCountFront += StripLineListFront.Count;
		}
		VisibleGameObjectList.Clear();
		for (int num = GameObjectList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = GameObjectList[num];
			gameObject.UpdateOnScreenState();
			if (!GameCore.IsEditorEnabled && gameObject.ActiveMode == ActiveModeEnum.Normal && gameObject.WorldPosition.X + (float)gameObject.VisibilityRect.Right < gameObject.MyCamera.Position.X * gameObject.MyLayer.ScrollFactor.X - 1920f)
			{
				gameObject.Destroy();
			}
			if (gameObject.IsDestroyed || (gameObject.ParentObject != null && !GameCore.IsEditorEnabled && !gameObject.ParentObject.IsHero))
			{
				if (gameObject.IsDestroyed)
				{
					GameCore.Allocator.DeleteGameObject(gameObject);
				}
				GameObjectList.RemoveAt(num);
			}
			else
			{
				if (!GameCore.IsEditorEnabled)
				{
					gameObject.UpdateTriggerPoint();
				}
				gameObject.UpdateInActionState();
				if (gameObject.IsInLOD && (gameObject.IsActivated || gameObject.IsOnScreenX))
				{
					VisibleGameObjectList.Add(gameObject);
				}
				else if (!gameObject.IsInLOD && gameObject.RigidBody != null && gameObject.RigidBody.IsEnabled)
				{
					gameObject.RigidBody.IsEnabled = false;
				}
			}
		}
		VisibleObjectSorter.Sort(VisibleGameObjectList);
		for (int num2 = VisibleGameObjectList.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject2 = VisibleGameObjectList[num2];
			gameObject2.Update(elapsedTime);
			if (!gameObject2.IsUpdatable && GameCore.IsEditorEnabled)
			{
				gameObject2.UpdatePosition(0f);
			}
		}
		for (int num3 = ParticleListBack.Count - 1; num3 >= 0; num3--)
		{
			Particle particle = ParticleListBack[num3];
			particle.Update(elapsedTime);
			if (ParticleListFront.Count > GameCore.MaxParticleCountPerLayer && particle.GetLifePercentage() < 0.5f)
			{
				particle.Destroy();
			}
			if (particle.IsDestroyed)
			{
				GameCore.Allocator.DeleteParticle(particle);
				ParticleListBack.RemoveAt(num3);
			}
		}
		for (int num4 = ParticleListFront.Count - 1; num4 >= 0; num4--)
		{
			Particle particle2 = ParticleListFront[num4];
			particle2.Update(elapsedTime);
			if (ParticleListFront.Count > GameCore.MaxParticleCountPerLayer && particle2.GetLifePercentage() < 0.5f)
			{
				particle2.Destroy();
			}
			if (particle2.IsDestroyed)
			{
				GameCore.Allocator.DeleteParticle(particle2);
				ParticleListFront.RemoveAt(num4);
			}
		}
		for (int num5 = StripLineListFront.Count - 1; num5 >= 0; num5--)
		{
			StripLine stripLine = StripLineListFront[num5];
			stripLine.Update(elapsedTime);
			if (stripLine.IsDestroyed && !stripLine.HasParticles)
			{
				GameCore.Allocator.DeleteStripLine(stripLine);
				StripLineListFront.RemoveAt(num5);
			}
		}
		for (int num6 = StripLineListBack.Count - 1; num6 >= 0; num6--)
		{
			StripLine stripLine2 = StripLineListBack[num6];
			stripLine2.Update(elapsedTime);
			if (stripLine2.IsDestroyed && !stripLine2.HasParticles)
			{
				GameCore.Allocator.DeleteStripLine(stripLine2);
				StripLineListBack.RemoveAt(num6);
			}
		}
	}

	public void Draw()
	{
		if (!IsUpdatable)
		{
			return;
		}
		DebugInfo.AddLayer(Id);
		bool flag = false;
		GameCore.EffectLight.LayerBeginDraw(this);
		GameCore.EffectLightBasic.LayerBeginDraw(this);
		GameCore.DrawBegin(MyLevel.Camera.WorldMatrixCurrentLayer);
		GameCore.EffectParticle.UpdateView();
		GameCore.EffectParticle.MyEffect.Parameters["ambientLight"].SetValue(AmbientLightColor.ToVector3() * MyLevel.AmbientFactor);
		GameCore.TriangleBatchTextured.Begin(BlendState.AlphaBlend, MyLevel.Camera.WorldMatrixCurrentLayer, SamplerState.LinearWrap, 0f);
		if (StripLineListBack.Count > 0)
		{
			foreach (StripLine item in StripLineListBack)
			{
				item.Draw(GameCore.TriangleBatchTextured);
				DebugInfo.CurrentLayer.LayerStrips++;
			}
		}
		GameCore.TriangleBatchTextured.End();
		Particle.DrawParticleList(this, ParticleListBack);
		if (ParticleListBack.Count > 0)
		{
			DebugInfo.CurrentLayer.LayerParticleLists++;
		}
		GameCore.DrawEnd();
		GameCore.EffectLight.LayerBeginDraw(this);
		GameCore.DrawBegin(MyLevel.Camera.WorldMatrixCurrentLayer);
		int num = 0;
		for (int num2 = VisibleGameObjectList.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject = VisibleGameObjectList[num2];
			if (!flag && MyLevel.HeroList != null && this == MyLevel.MainLayer && gameObject.ZOrder > 0)
			{
				MyLevel.DrawHeroes(this);
				flag = true;
			}
			gameObject.Draw(GameCore.SpriteBatch);
			if (gameObject.ZOrder != num)
			{
				GameCore.DrawEnd();
				GameCore.EffectLight.LayerBeginDraw(this);
				GameCore.DrawBegin(MyLevel.Camera.WorldMatrixCurrentLayer);
				num = gameObject.ZOrder;
				DebugInfo.CurrentLayer.LayerZFlushes++;
			}
			DebugInfo.CurrentLayer.LayerObjects++;
		}
		if (!flag && this == MyLevel.MainLayer && MyLevel.HeroList != null)
		{
			MyLevel.DrawHeroes(this);
		}
		DebugInfo.CurrentLayer.LayerZFlushes++;
		GameCore.DrawEnd();
		if (StripLineListFront.Count > 0)
		{
			GameCore.TriangleBatchTextured.Begin(BlendState.AlphaBlend, MyLevel.Camera.WorldMatrixCurrentLayer, SamplerState.LinearWrap, 0f);
			foreach (StripLine item2 in StripLineListFront)
			{
				item2.Draw(GameCore.TriangleBatchTextured);
				DebugInfo.CurrentLayer.LayerStrips++;
			}
			GameCore.TriangleBatchTextured.End();
		}
		Particle.DrawParticleList(this, ParticleListFront);
		if (ParticleListFront.Count > 0)
		{
			DebugInfo.CurrentLayer.LayerParticleLists++;
		}
		if (!Options.DebugPath || GameCore.IsEditorEnabled || GameCore.IsShopMenuEnabled)
		{
			return;
		}
		foreach (GameObject visibleGameObject in VisibleGameObjectList)
		{
			GameCore.Editor.SpriteDrawBegin();
			if (Options.DebugQuickInfoMode == 1)
			{
				GameCore.Editor.DrawPath(visibleGameObject);
				if (visibleGameObject.CurrentTimeLine != null)
				{
					GameCore.Editor.ToolTimeLine.Draw(MyLevel.Camera.CurrentLayerPosToScreen(visibleGameObject.WorldPosition), visibleGameObject.CurrentTimeLine, visibleGameObject.TimeLineTime, new Vector2(0f, -25f));
				}
			}
			if (visibleGameObject.PartData != null)
			{
				Vector2 screenPos = visibleGameObject.GetScreenPos();
				screenPos += visibleGameObject.PartData.DebugOffset;
				GameCore.Editor.DrawQuickInfo(visibleGameObject, ref screenPos, 0);
			}
			GameCore.Editor.SpriteDrawEnd();
		}
	}

	public void AddGameObject(GameObject go, int index = -1)
	{
		if (index >= 0)
		{
			GameObjectList.Insert(index, go);
		}
		else
		{
			GameObjectList.Add(go);
		}
		go.SetLayer(this);
		go.CreateTime = MyLevel.Timer;
		if (LoopCount == 0 || (go.PartData != null && go.PartData.IsAlwaysInStats))
		{
			go.IsInLevelStats = true;
		}
	}

	public void AddStripLine(StripLine sl, ParticleSpace space)
	{
		if (space == ParticleSpace.MyLayerBack || space == ParticleSpace.FirstMainLayerBack)
		{
			StripLineListBack.Add(sl);
		}
		else
		{
			StripLineListFront.Add(sl);
		}
	}

	public void AddPath(BezierPath path)
	{
		if (!PathDictionary.Contains(path.Id))
		{
			PathDictionary.Add(path.Id, path);
		}
		else
		{
			GameCore.LogErr($"Path '{path.Id}' is already in layer '{Id}'.");
		}
	}

	public GameObject CreateObjectByPartData(string partDataId, Vector2 pos, Vector2 scale, float rotation, int zorder = 0, bool bCreateRigidBody = true)
	{
		return CreateObjectByPartData(GameCore.GetPartData(partDataId), pos, scale, rotation, zorder, bCreateRigidBody);
	}

	public GameObject CreateObjectByPartData(PartData partData, Vector2 pos, Vector2 scale, float rotation, int zorder = 0, bool bCreateRigidBody = true)
	{
		if (partData == null)
		{
			GameCore.LogErr("Unable to create object - part data is null.");
			return null;
		}
		GameObject gameObject = GameObject.CreateByPartData(partData, null, partData.Id, pos, scale, rotation, zorder, null, bCreateRigidBody, this);
		AddGameObject(gameObject);
		return gameObject;
	}

	public void GetGameObjectsInRect(Vector2 topLeft, Vector2 bottomRight, ref List<GameObject> resultList)
	{
		resultList.Clear();
		foreach (GameObject gameObject in GameObjectList)
		{
			if (gameObject.Position.X >= topLeft.X && gameObject.Position.Y >= topLeft.Y && gameObject.Position.X <= bottomRight.X && gameObject.Position.Y <= bottomRight.Y)
			{
				resultList.Add(gameObject);
			}
		}
	}

	public override string ToString()
	{
		return Id;
	}
}
