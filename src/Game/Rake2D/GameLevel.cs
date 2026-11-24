using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Debug;
using Rake2D.JnG;
using Rake2D.Script;

namespace Rake2D;

public class GameLevel
{
	public delegate void OnEventHandler();

	public delegate void OnGameObjectEventHandler(GameObject go);

	public const string LOADER_FILENAME = "loader.jnl";

	public LevelData LevelData;

	public string MusicFileName;

	public float Timer;

	public World PhysWorld;

	public ParticleEmitterData ScoreEmitterData;

	public ParticleEmitterData HerotTextEmitterData;

	public Layer CurrentLayer;

	public Layer MainLayer;

	public Layer FirstMainLayer;

	public Layer LastMainLayer;

	public Layer LastParticleLayer;

	public Layer FrontParticleLayer;

	public Layer LastDistortionLayer;

	public Vector3 AmbientFactor;

	public Vector2 Gravity;

	public int CutsceneMode;

	public int FirstObjectPk;

	public long MaxObjectPk;

	public List<GameObject> PreviewObjectList;

	public List<WarningText> WarningTextList;

	public List<Layer> LayerList;

	public List<Particle> ParticleListDistortion;

	public List<SafePoint> SafePointList;

	public List<Hero> HeroList;

	public GameContent Content;

	public bool PhysicsPaused;

	public bool IgnoreTakenValues;

	public string FileName;

	public ContentArchive ContentArchive;

	public Camera Camera;

	public Vector2 Size;

	public Vector2 OriginalSize;

	public Vector2 ScrollSpeed;

	public Vector2 StartScrollSpeed;

	public List<string> LocationTextList;

	public bool NoBulletTime;

	public long FinishScore;

	public string Id { get; private set; }

	public int DebriefingMode
	{
		get
		{
			if (LevelData != null)
			{
				return LevelData.DebriefingMode;
			}
			return 0;
		}
	}

	public bool AlwaysCompensateScrolling => ScrollSpeed.X > 200f;

	public float Quake
	{
		set
		{
			Camera.AddQuake(value);
		}
	}

	public event OnEventHandler OnLevelBegin;

	public event OnGameObjectEventHandler OnObjectKill;

	public void Init()
	{
		Content = new GameContent();
		LayerList = new List<Layer>();
		Gravity = new Vector2(0f, 9.8f);
		PhysWorld = new World(Gravity);
		ParticleListDistortion = GameCore.Allocator.GetParticleList();
		SafePointList = new List<SafePoint>();
		FinishScore = 0L;
		Camera = new Camera(this);
		ConvertUnits.SetDisplayUnitToSimUnitRatio(64f);
		LocationTextList = new List<string>();
		WarningTextList = new List<WarningText>();
	}

	public Layer CreateLayer(string id, float scrollFactorX, float scrollFactorY, Color? ambientColor = null)
	{
		Layer layer = new Layer(this, id, scrollFactorX, scrollFactorY);
		if (ambientColor.HasValue)
		{
			layer.AmbientLightColor = ambientColor.Value;
		}
		LayerList.Add(layer);
		return layer;
	}

	public Layer CreateLayer(string id)
	{
		return CreateLayer(id, 1f, 1f, Color.White);
	}

	private void SetDefaultMainLayer()
	{
		foreach (Layer layer in LayerList)
		{
			if (layer.ScrollFactor.X == 1f)
			{
				MainLayer = layer;
				break;
			}
		}
		if (MainLayer == null && LayerList.Count > 0)
		{
			MainLayer = LayerList[0];
		}
		MainLayer.UsePhysics = true;
	}

	public void LoopLayer(Layer loopLayer)
	{
		Dictionary<long, GameObject> pkDictionary = new Dictionary<long, GameObject>();
		List<KeyValuePair<GameObject, long>> linkList = new List<KeyValuePair<GameObject, long>>();
		List<GameObject> createdObjectList = new List<GameObject>();
		long maxObjectPk = MaxObjectPk;
		do
		{
			loopLayer.LoopCount++;
			if (loopLayer == MainLayer)
			{
				Size.X += loopLayer.LoopOffsetX;
			}
		}
		while (loopLayer.MyLevel.Camera.Position.X * loopLayer.ScrollFactor.X >= loopLayer.LoopX + loopLayer.LoopOffsetX * (float)(loopLayer.LoopCount + 1));
		if (loopLayer.SegmentList != null && loopLayer.SegmentList.Count > 1)
		{
			loopLayer.SegmentList[1].Build(maxObjectPk, new Vector2(loopLayer.LoopOffsetX * (float)loopLayer.LoopCount, 0f), pkDictionary, linkList, bSkipExpiredObjects: false, ref createdObjectList);
		}
		BuildLinks(pkDictionary, linkList);
		BuildEventLinks(pkDictionary, createdObjectList);
		BuildAutoLinks(createdObjectList);
		foreach (GameObject item in createdObjectList)
		{
			item.Init();
			item.LevelInit();
			item.DoOnLevelBegin();
		}
	}

	public Layer GetLayerById(string id)
	{
		foreach (Layer layer in LayerList)
		{
			if (id.Equals(layer.Id, StringComparison.OrdinalIgnoreCase))
			{
				return layer;
			}
		}
		return null;
	}

	public void UseLayer(string id)
	{
		CurrentLayer = GetLayerById(id);
		if (CurrentLayer == null)
		{
			GameCore.LogErr("Unable to use undefined layer '{0}'", id);
		}
	}

	public void AddOjectToPreviewList(GameObject go)
	{
		if (PreviewObjectList == null)
		{
			PreviewObjectList = GameCore.Allocator.GetGameObjectList();
		}
		PreviewObjectList.Add(go);
	}

	public void DestroyPreviewList()
	{
		if (PreviewObjectList != null)
		{
			PreviewObjectList = GameCore.Allocator.DeleteOwningGameObjectList(PreviewObjectList);
		}
	}

	public WarningText AddWarningText(string text, Color color, bool bHideOld = true)
	{
		if (!Options.WarningTexts)
		{
			return null;
		}
		WarningText warningText = new WarningText(text, color);
		if (bHideOld)
		{
			WarningTextList.ForEach((WarningText txt) =>
			{
				txt.SetState(WarningTextStateEnum.Hiding);
			});
		}
		WarningTextList.Add(warningText);
		return warningText;
	}

	public BezierPath CreatePath(string id, GameObject gameObject)
	{
		BezierPath bezierPath = new BezierPath(id, gameObject);
		CurrentLayer.AddPath(bezierPath);
		return bezierPath;
	}

	public BezierPath CreatePath(string id, float x, float y, GameObject gameObject)
	{
		BezierPath bezierPath = CreatePath(id, gameObject);
		bezierPath.AddPoint(x, y);
		return bezierPath;
	}

	public SafePoint AddSafePoint(Vector2 pos)
	{
		SafePoint safePoint = new SafePoint();
		safePoint.Position = pos;
		SafePointList.Add(safePoint);
		return safePoint;
	}

	public void RemoveSafePoint(SafePoint safePoint)
	{
		SafePointList.Remove(safePoint);
		SortSafePointList();
	}

	public void SortSafePointList()
	{
		SafePointList = SafePointList.OrderBy((SafePoint sp) => sp.Position.X).ToList();
	}

	public Vector2 GetNearestSafePosition(float posX)
	{
		SafePoint safePoint = null;
		SafePoint safePoint2 = null;
		foreach (SafePoint safePoint3 in SafePointList)
		{
			if (safePoint3.Position.X >= posX)
			{
				safePoint2 = safePoint3;
				break;
			}
			safePoint = safePoint3;
		}
		if (safePoint2 == null)
		{
			if (safePoint != null)
			{
				return new Vector2(posX, safePoint.Position.Y);
			}
			return new Vector2(posX, 0f);
		}
		if (safePoint == null)
		{
			return new Vector2(posX, safePoint2.Position.Y);
		}
		Vector2 vector = safePoint2.Position - safePoint.Position;
		return new Vector2(posX, safePoint.Position.Y + vector.Y * (posX - safePoint.Position.X) / vector.X);
	}

	public List<GameObject> GetVisibleObjectsInRangeInAllPhysLayers(Vector2 center, TargetClass targetClasses, float maxDistance, List<GameObject> ignoreList = null, bool bOlyHittable = false)
	{
		float num = maxDistance * maxDistance;
		List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
		foreach (Layer layer in LayerList)
		{
			if (!layer.UsePhysics)
			{
				continue;
			}
			int count = layer.VisibleGameObjectList.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = layer.VisibleGameObjectList[i];
				if (!gameObject.IsDestroyed && (!bOlyHittable || (!gameObject.IsKilled && !gameObject.IsImmortal && gameObject.IsActivated && gameObject.IsTriggerFree) || (gameObject.TargetPart != null && !gameObject.TargetPart.IsDestroyed && !gameObject.TargetPart.IsKilled)) && (targetClasses & gameObject.TargetClass) != 0 && Vector2.DistanceSquared(center, gameObject.GetTargetWorldPosition()) <= num && (ignoreList == null || !ignoreList.Contains(gameObject)))
				{
					gameObjectList.Add(gameObject);
				}
			}
		}
		return gameObjectList;
	}

	public List<GameObject> GetPhysPartsInRangeInAllPhsLayers(Vector2 center, TargetClass targetClasses, float maxDistance, List<GameObject> ignoreList = null, bool bOlyHittable = false)
	{
		List<GameObject> affectedObjects = GameCore.Allocator.GetGameObjectList();
		AABB aabb = default;
		aabb.LowerBound = ConvertUnits.ToSimUnits(center) - new Vector2(ConvertUnits.ToSimUnits(maxDistance));
		aabb.UpperBound = ConvertUnits.ToSimUnits(center) + new Vector2(ConvertUnits.ToSimUnits(maxDistance));
		PhysWorld.QueryAABB((Fixture fixture) =>
		{
			if (fixture.Body.UserData.GetType() == typeof(GameObject))
			{
				GameObject gameObject = (GameObject)fixture.Body.UserData;
				if (!gameObject.IsDestroyed && (!bOlyHittable || (!gameObject.IsKilled && !gameObject.IsImmortal && gameObject.IsActivated)) && (targetClasses & gameObject.TargetClass) != 0 && (ignoreList == null || !ignoreList.Contains(gameObject)) && !affectedObjects.Contains(gameObject))
				{
					affectedObjects.Add(gameObject);
				}
			}
			return true;
		}, ref aabb);
		return affectedObjects;
	}

	public GameObject FindNearestObject(Vector2 center, TargetClass targetClasses, float maxDistance = 2000f, List<GameObject> ignoreList = null, bool bOnlyHittable = false)
	{
		GameObject result = null;
		List<GameObject> visibleObjectsInRangeInAllPhysLayers = GetVisibleObjectsInRangeInAllPhysLayers(center, targetClasses, maxDistance, ignoreList, bOnlyHittable);
		float num = maxDistance * maxDistance;
		foreach (GameObject item in visibleObjectsInRangeInAllPhysLayers)
		{
			float num2 = Vector2.DistanceSquared(center, item.WorldPosition);
			if (num2 < num)
			{
				num = num2;
				result = item;
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(visibleObjectsInRangeInAllPhysLayers);
		return result;
	}

	public List<GameObject> ShockHit(GameObject hittingObject, List<GameObject> objectsInRange, Vector2 epicentrumPos, TargetClass targetClasses, DamageTypeEnum damageType, float damage, float physForce, List<GameObject> ignoreList, bool onlyHitable, bool wantResult)
	{
		List<GameObject> list = (wantResult ? GameCore.Allocator.GetGameObjectList() : null);
		Dictionary<GameObject, List<GameObject>> dictionary = new Dictionary<GameObject, List<GameObject>>();
		foreach (GameObject item in objectsInRange)
		{
			if (item == hittingObject || !item.IsActivated || item.IsKilled || item.IsDestroyed)
			{
				continue;
			}
			if (item.RigidBody != null && physForce != 0f && item.IsDynamic)
			{
				Vector2 vector = item.RigidBody.WorldCenter - epicentrumPos;
				vector = ((!(vector != Vector2.Zero)) ? Utils.GetVelocityVector2(physForce, Utils.GetRandom(0f, (float)Math.PI * 2f)) : (Vector2.Normalize(vector) * physForce));
				if (item.Velocity.LengthSquared() < 250000f && !item.PartData.IgnoreShock)
				{
					item.RigidBody.ApplyLinearImpulse(vector);
					if (item.RigidBody.AngularVelocity > -40f && item.RigidBody.AngularVelocity < 40f)
					{
						float rotationFromZero = Utils.GetRotationFromZero(vector);
						float num = item.RigidBody.Mass;
						if (rotationFromZero < 0f)
						{
							num *= -1f;
						}
						item.RigidBody.ApplyAngularImpulse(num);
						if (item.RigidBody.AngularVelocity > 20f)
						{
							item.RigidBody.AngularVelocity = 20f;
						}
						else if (item.RigidBody.AngularVelocity < -20f)
						{
							item.RigidBody.AngularVelocity = -20f;
						}
					}
				}
				item.DoOnShockHit(damageType, vector, hittingObject);
			}
			GameObject mainPart = item.GetMainPart();
			if (dictionary.ContainsKey(mainPart))
			{
				dictionary[mainPart].Add(item);
				continue;
			}
			List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
			gameObjectList.Add(item);
			dictionary.Add(mainPart, gameObjectList);
			list?.Add(mainPart);
		}
		foreach (KeyValuePair<GameObject, List<GameObject>> item2 in dictionary)
		{
			int count = item2.Value.Count;
			float damage2 = damage / (float)count;
			for (int i = 0; i < count; i++)
			{
				item2.Value[i].Hit(damage2, damageType, hittingObject);
			}
			GameCore.Allocator.DeleteAliasingGameObjectList(item2.Value);
		}
		return list;
	}

	public List<GameObject> ShockHit(GameObject hittingObject, Vector2 epicentrumPos, float maxDistance, TargetClass targetClasses, DamageTypeEnum damageType, float damage, float physForce, List<GameObject> ignoreList, bool onlyHitable, bool wantResults)
	{
		List<GameObject> physPartsInRangeInAllPhsLayers = GetPhysPartsInRangeInAllPhsLayers(epicentrumPos, targetClasses, maxDistance, ignoreList, onlyHitable);
		List<GameObject> result = ShockHit(hittingObject, physPartsInRangeInAllPhsLayers, epicentrumPos, targetClasses, damageType, damage, physForce, ignoreList, onlyHitable, wantResults);
		GameCore.Allocator.DeleteAliasingGameObjectList(physPartsInRangeInAllPhsLayers);
		return result;
	}

	public Particle AddTextParticle(string text, Color color, Vector2 position)
	{
		Particle particle = GameCore.Allocator.GetParticle();
		particle.Init(ScoreEmitterData, position);
		particle.Text = text;
		particle.Velocity = particle.Data.VelocityRange.GetRandom();
		particle.Velocity.X += ScrollSpeed.X - 50f;
		particle.StartupVelocity = particle.Velocity;
		particle.Color = color;
		particle.StartupColor = particle.Color;
		particle.Scale = new Vector2(0.5f);
		particle.StartupScale = particle.Scale;
		FrontParticleLayer.ParticleListFront.Add(particle);
		return particle;
	}

	public Particle AddScoreParticle(long value, Vector2 position)
	{
		Particle particle = GameCore.Allocator.GetParticle();
		particle.Init(ScoreEmitterData, position);
		particle.Text = $"+{Utils.FormatBigNumber(value, keepSmallDecimal: false)}";
		long num = ((FactorManager.ScoreFactor > 0f) ? ((long)((float)value / FactorManager.ScoreFactor)) : 0);
		particle.Position += Utils.GetRandomVector2(-10f, -10f, 10f, 10f);
		particle.Velocity = particle.Data.VelocityRange.GetRandom();
		if (ScrollSpeed.X > 200f)
		{
			particle.Velocity.X += ScrollSpeed.X - 200f;
		}
		particle.StartupVelocity = particle.Velocity;
		particle.StartupScale = new Vector2(0.45f + Utils.Clamp((float)num / 2000f, 0f, 0.5f));
		particle.LifeTime = 1.5f + 2f * Utils.Clamp((float)num / 1000f, 0f, 1f);
		if (num >= 1200)
		{
			particle.Color = new Color(255, 255, 50);
		}
		else if (num >= 500)
		{
			particle.Color = new Color(255, 230, 0);
		}
		else if (num >= 300)
		{
			particle.Color = new Color(255, 200, 0);
		}
		else
		{
			particle.Color = new Color(255, 150, 0);
		}
		particle.StartupColor = particle.Color;
		LastMainLayer.ParticleListFront.Add(particle);
		return particle;
	}

	public void ChangeLinkedObject(GameObject oldObject, GameObject newObject)
	{
		LayerList?.ForEach((Layer layer) =>
		{
			layer.GameObjectList?.ForEach((GameObject go) =>
			{
				if (go.LinkedObjectList != null)
				{
					for (int num = go.LinkedObjectList.Count - 1; num >= 0; num--)
					{
						if (go.LinkedObjectList[num] == oldObject)
						{
							go.LinkedObjectList[num] = newObject;
						}
					}
				}
			});
		});
	}

	public void UpdateHeroes(float elapsedTime)
	{
		if (!GameCore.UpdateHeroes || HeroList == null)
		{
			return;
		}
		foreach (Hero hero in HeroList)
		{
			hero.Update(elapsedTime);
			if (hero.RecreateSmallShip || (hero.MyObject == null && !GameCore.IsMainMenuEnabled))
			{
				hero.DestroySmallShip();
				if (GameCore.IsShopMenuEnabled)
				{
					hero.CreateSmallShip(GameJnG.ShopMenu.HeroPreviewLevel.MainLayer, hero.LastPosition);
					hero.StartAutopilotShopMenu();
				}
				else
				{
					hero.CreateSmallShip(null, hero.LastPosition);
				}
			}
			if (!GameCore.IsMainMenuEnabled && (hero.AutopilotMode == HeroAutopilotMode.None || hero.AutopilotMode == HeroAutopilotMode.ShopMenuPreview || hero.AutopilotMode == HeroAutopilotMode.ShopMenuProfile) && hero.MyObject.IsAlive)
			{
				hero.CheckOutOfGameView();
			}
		}
	}

	public void DoOnObjectKill(GameObject go)
	{
		try
		{
			OnObjectKill?.Invoke(go);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnObjecKill script exception: {0} fo object '{1}' in level '{2}'", ex, go.Id, Id);
		}
	}

	public void Update(float elapsedTime)
	{
		GameCore.CurrentLevel = this;
		Timer += elapsedTime;
		Camera.SetLayer(MainLayer);
		if (GameCore.UpdateHeroes && HeroList != null)
		{
			if (GameJnG.IsTimeCompressorEnabled)
			{
				GameJnG.ScrollCameraToHero(elapsedTime / GameCore.TimeFactor);
			}
			else
			{
				GameJnG.ScrollCameraToHero(elapsedTime);
			}
		}
		if (!GameCore.IsEditorEnabled)
		{
			Camera.Position += ScrollSpeed * elapsedTime;
		}
		Camera.Update(elapsedTime);
		CurrentLayer = MainLayer;
		if (!PhysicsPaused)
		{
			try
			{
				if (elapsedTime > 0f)
				{
					PhysWorld.Step(elapsedTime);
				}
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Physic exception: {0}", ex.Message);
			}
		}
		for (int num = WarningTextList.Count - 1; num >= 0; num--)
		{
			WarningText warningText = WarningTextList[num];
			if (warningText.IsDestroyed)
			{
				WarningTextList.RemoveAt(num);
			}
			else
			{
				warningText.Update(elapsedTime);
			}
		}
		foreach (Layer layer in LayerList)
		{
			SetCurrentLayer(layer);
			if (layer == MainLayer)
			{
				UpdateHeroes(elapsedTime);
			}
			if (layer.IsEnabledByEditor)
			{
				if (!GameCore.IsEditorEnabled && layer.LoopX >= 0f && Camera.Position.X * layer.ScrollFactor.X >= layer.LoopX + layer.LoopOffsetX * (float)layer.LoopCount)
				{
					LoopLayer(layer);
				}
				if (layer.IsUpdatable)
				{
					layer.Update(elapsedTime);
				}
			}
		}
		if (Options.UseDistortionShader)
		{
			for (int num2 = ParticleListDistortion.Count - 1; num2 >= 0; num2--)
			{
				ParticleListDistortion[num2].Update(elapsedTime);
				if (ParticleListDistortion[num2].IsDestroyed)
				{
					GameCore.Allocator.DeleteParticle(ParticleListDistortion[num2]);
					ParticleListDistortion.RemoveAt(num2);
				}
			}
			DebugInfo.DistortionParticles = ParticleListDistortion.Count;
		}
		if (GameJnG.IsTestRangeEnabled || MusicFileName == null || !(elapsedTime > 0f) || GameCore.IsEditorEnabled || GameCore.SoundSystem.IsMusicPlaying || GameJnG.MissionCompletedScreen.State != MissionScreenStateEnum.Hidden)
		{
			return;
		}
		if (GameCore.SoundSystem.LoadMusic(MusicFileName))
		{
			if (GameJnG.MissionFailedScreen.State != MissionScreenStateEnum.Hidden)
			{
				GameCore.SoundSystem.PlayMusic(isLooped: false);
			}
			else if (!GameCore.IsMainMenuEnabled && !GameCore.IsShopMenuEnabled)
			{
				GameCore.SoundSystem.PlayMusicFromPositionInSec(isLooped: true, Camera.Position.X / ScrollSpeed.X);
			}
			else
			{
				GameCore.SoundSystem.PlayMusic(isLooped: true);
			}
		}
		else
		{
			MusicFileName = null;
		}
	}

	public void RenderDistortion()
	{
		GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.EffectDistortion.RenderTarget);
		GameCore.GraphicsDeviceStatic.Clear(ClearOptions.Target, new Color(0.5f, 0.5f, 0f, 1f), 0f, 0);
		Vector2 vector = new Vector2((float)GameCore.EffectDistortion.RenderTarget.Width / (float)GameCore.MainViewPort.Width, (float)GameCore.EffectDistortion.RenderTarget.Height / (float)GameCore.MainViewPort.Height);
		GameCore.GraphicsDeviceStatic.ScissorRectangle = new Rectangle((int)((float)GameCore.ScissorRectangle.X * vector.X), (int)((float)GameCore.ScissorRectangle.Y * vector.Y), (int)((float)GameCore.ScissorRectangle.Width * vector.X), (int)((float)GameCore.ScissorRectangle.Height * vector.Y));
		GameCore.SpriteBatchDistortion.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Camera.WorldMatrixMainLayer * Matrix.CreateScale(vector.X, vector.Y, 1f));
		DebugInfo.CurrentLayer.DistortionParticleBegins++;
		foreach (Particle item in ParticleListDistortion)
		{
			item.Draw(GameCore.SpriteBatchDistortion);
		}
		GameCore.SpriteBatchDistortion.End();
	}

	internal void DrawHeroes(Layer layer)
	{
		if (!GameCore.UpdateHeroes)
		{
			return;
		}
		GameCore.DrawEnd();
		if (HeroList != null)
		{
			for (int num = HeroList.Count - 1; num >= 0; num--)
			{
				HeroList[num].Draw();
			}
		}
		GameCore.EffectLight.LayerBeginDraw(layer);
		GameCore.EffectLightBasic.LayerBeginDraw(layer);
		GameCore.DrawBegin(layer.MyLevel.Camera.WorldMatrixCurrentLayer);
	}

	public void Draw(bool showRenderTarget = true)
	{
		GameCore.CurrentLevel = this;
		foreach (Layer layer in LayerList)
		{
			SetCurrentLayer(layer);
			layer.Draw();
			if (((layer == LastDistortionLayer) & showRenderTarget) && GameCore.UseRenderTarget)
			{
				GameCore.DrawLevelRenderTarget(bUseDistortion: true, Color.White);
				GameCore.EffectLightBasic.Update();
			}
		}
		foreach (WarningText warningText in WarningTextList)
		{
			warningText.Draw();
		}
	}

	public void SetCurrentLayer(Layer layer)
	{
		CurrentLayer = layer;
		Camera.SetLayer(CurrentLayer);
	}

	public static GameObject GetGameObjectAtPoint(List<GameObject> objectList, Vector2 pointPos, bool bCheckIsAvailableFromEditor, bool bUsePivotOnly, bool bOnlyMainParts, GameObject ignoreObject = null)
	{
		if (objectList == null)
		{
			return null;
		}
		for (int i = 0; i < objectList.Count; i++)
		{
			GameObject gameObject = objectList[i];
			if (ignoreObject == gameObject || (bOnlyMainParts && !gameObject.IsMainPart) || (bCheckIsAvailableFromEditor && GameCore.IsEditorEnabled && gameObject.PartData != null && !GameCore.Editor.IsEditorSetEnabled(gameObject.PartData.EditorSet)))
			{
				continue;
			}
			if (bUsePivotOnly)
			{
				if (Vector2.Distance(gameObject.Position, pointPos) <= 22f)
				{
					return gameObject;
				}
			}
			else if (gameObject.IsPointInFrameRect(pointPos))
			{
				return gameObject;
			}
		}
		return null;
	}

	public bool LoadFromDir(string directory, bool resetCamera = true)
	{
		return LoadFromFile(Path.Combine(directory, "level.txt"), resetCamera);
	}

	public bool LoadFromFile(string fileName, bool resetCamera = true)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		Layer layer = null;
		LastMainLayer = null;
		FirstMainLayer = null;
		LastDistortionLayer = null;
		LastParticleLayer = null;
		FrontParticleLayer = null;
		ContentArchive = null;
		FileName = fileName;
		FirstObjectPk = 1;
		Reset();
		using (IniFile iniFile = IniFile.OpenFromContent(fileName))
		{
			if (iniFile == null)
			{
				AfterLoad(resetCamera);
				return false;
			}
			ContentArchive = iniFile.ContentArchive;
			while (iniFile.NextLine())
			{
				if (iniFile.ValueType == IniFileValueType.MainSection && iniFile.KeyEquals("layer"))
				{
					layer = CreateLayer(iniFile.Value);
					continue;
				}
				if (iniFile.KeyEquals("data"))
				{
					DataLoader.LoadDataFile(iniFile.GetPathFromValue(), Content);
					continue;
				}
				if (iniFile.KeyEquals("music"))
				{
					MusicFileName = iniFile.GetPathFromValue();
					continue;
				}
				if (iniFile.KeyEquals("id"))
				{
					Id = iniFile.Value;
					continue;
				}
				if (iniFile.KeyEquals("first_pk"))
				{
					FirstObjectPk = iniFile.GetIntValue();
					continue;
				}
				if (iniFile.KeyEquals("cutscene"))
				{
					CutsceneMode = iniFile.GetIntValue();
					continue;
				}
				if (iniFile.KeyEquals("size") && iniFile.TryGetIntArray(2, out var resultArray))
				{
					Size = new Vector2(resultArray[0], resultArray[1]);
					continue;
				}
				if (iniFile.KeyEquals("gravity"))
				{
					FloatRange range = default;
					if (iniFile.TryGetFloatRange(ref range))
					{
						Gravity = new Vector2(range.From / 1000f, range.To / 1000f);
					}
					continue;
				}
				if (iniFile.KeyEquals("scroll_speed"))
				{
					ScrollSpeed = new Vector2(iniFile.GetIntValue(), 0f);
					continue;
				}
				if (iniFile.KeyEquals("ignore_taken_values"))
				{
					IgnoreTakenValues = iniFile.GetBoolValue();
					continue;
				}
				if (iniFile.KeyEquals("size") && iniFile.TryGetIntArray(2, out resultArray))
				{
					Size = new Vector2(resultArray[0], resultArray[1]);
					continue;
				}
				if (iniFile.KeyEquals("finish_score"))
				{
					FinishScore = iniFile.GetIntValue();
					continue;
				}
				if (layer != null)
				{
					if (iniFile.KeyEquals("scroll") && iniFile.TryGetIntArray(1, 2, out resultArray))
					{
						if (resultArray.Length > 1)
						{
							layer.ScrollFactor = new Vector2((float)resultArray[0] / 100f, (float)resultArray[1] / 100f);
						}
						else
						{
							layer.ScrollFactor = new Vector2((float)resultArray[0] / 100f, (float)resultArray[0] / 100f);
						}
						continue;
					}
					if (iniFile.KeyEquals("enabled"))
					{
						layer.IsEnabledByEditor = iniFile.GetIntValue() != 0;
						continue;
					}
					if (iniFile.KeyEquals("editor_button_caption"))
					{
						layer.EditorButtonCaption = iniFile.Value;
						continue;
					}
					if (iniFile.KeyEquals("editor_button_color") && iniFile.TryGetIntArray(3, out resultArray))
					{
						layer.EditorButtonColor = new Color(resultArray[0], resultArray[1], resultArray[2]);
						continue;
					}
					if (iniFile.KeyEquals("copy_loop"))
					{
						layer.CopyLoopLayerId = iniFile.Value;
						continue;
					}
					if (iniFile.KeyEquals("light_factor") && iniFile.TryGetIntArray(layer.LightMapFactorArray.Length, out resultArray))
					{
						for (int i = 0; i < layer.LightMapFactorArray.Length; i++)
						{
							layer.LightMapFactorArray[i] = (float)Utils.Clamp(resultArray[i], 0, 100) / 100f;
						}
						continue;
					}
					if (iniFile.KeyEquals("light_to"))
					{
						layer.LightToMapNumber = iniFile.GetIntValue(1);
						if (layer.LightToMapNumber >= 3)
						{
							iniFile.LogErr("Value {0} out of range [0 - 2]", layer.LightToMapNumber);
							layer.LightToMapNumber = -1;
						}
						continue;
					}
					if (iniFile.KeyEquals("shadow_to"))
					{
						layer.ShadowToMapNumber = iniFile.GetIntValue(1);
						if (layer.ShadowToMapNumber >= 3)
						{
							iniFile.LogErr("Value {0} out of range [0 - 2]", layer.ShadowToMapNumber);
							layer.LightToMapNumber = -1;
						}
						continue;
					}
					if (iniFile.KeyEquals("shadow_from"))
					{
						layer.ShadowFromMapNumber = iniFile.GetIntValue(1);
						if (layer.ShadowFromMapNumber >= 3)
						{
							iniFile.LogErr("Value {0} out of range [0 - 2]", layer.ShadowFromMapNumber);
							layer.ShadowFromMapNumber = -1;
						}
						continue;
					}
					if (iniFile.KeyEquals("shadow_factor"))
					{
						layer.ShadowMapFactor = (float)Utils.Clamp(iniFile.GetIntValue(), 0, 100) / 100f;
						continue;
					}
					if (iniFile.KeyEquals("main"))
					{
						if (MainLayer != null)
						{
							iniFile.LogErr($"Main layer already defined for layer = {MainLayer.Id}");
							continue;
						}
						MainLayer = layer;
						MainLayer.UsePhysics = true;
						continue;
					}
					if (iniFile.KeyEquals("first_main"))
					{
						FirstMainLayer = layer;
						continue;
					}
					if (iniFile.KeyEquals("last_main"))
					{
						LastMainLayer = layer;
						continue;
					}
					if (iniFile.KeyEquals("last_particle"))
					{
						LastParticleLayer = layer;
						continue;
					}
					if (iniFile.KeyEquals("front_particle"))
					{
						FrontParticleLayer = layer;
						continue;
					}
					if (iniFile.KeyEquals("last_distortion"))
					{
						LastDistortionLayer = layer;
						continue;
					}
					if (iniFile.KeyEquals("use_phys"))
					{
						if (layer != MainLayer)
						{
							layer.UsePhysics = iniFile.GetBoolValue();
						}
						continue;
					}
					if (iniFile.KeyEquals("use_distortion"))
					{
						layer.UseDistortion = iniFile.GetBoolValue();
						continue;
					}
					if (iniFile.KeyEquals("lod"))
					{
						layer.MinLOD = iniFile.GetIntValue();
						continue;
					}
				}
				iniFile.LogErr("Unknown command");
			}
		}
		stopwatch.Stop();
		AfterLoad(resetCamera);
		return true;
	}

	internal void AfterLoad(bool resetCamera, bool mainLevel = true)
	{
		if (LayerList.Count == 0)
		{
			GameCore.LogErr("No layers defined.");
			CreateLayer("MAIN");
		}
		if (MainLayer == null)
		{
			SetDefaultMainLayer();
			GameCore.LogErr("Undefined Main layer. Set main to layer = '{0}'", MainLayer.Id);
		}
		if (LastMainLayer == null)
		{
			LastMainLayer = MainLayer;
		}
		if (FirstMainLayer == null)
		{
			FirstMainLayer = MainLayer;
		}
		if (LastParticleLayer == null)
		{
			LastParticleLayer = LastMainLayer;
		}
		if (FrontParticleLayer == null)
		{
			FrontParticleLayer = LastParticleLayer;
		}
		if (LastDistortionLayer == null)
		{
			LastDistortionLayer = LayerList.Last();
		}
		bool flag = false;
		for (int i = 0; i < LayerList.Count; i++)
		{
			Layer layer = LayerList[i];
			if (layer == FirstMainLayer)
			{
				flag = true;
			}
			if (flag)
			{
				layer.IsInMainGroup = true;
			}
			if (layer == LastMainLayer)
			{
				break;
			}
			if (layer.CopyLoopLayerId != null && GetLayerById(layer.CopyLoopLayerId) == null)
			{
				GameCore.LogErr("Undefined layer '{0}' for copy_loop in layer '{1}'", layer.CopyLoopLayerId, layer.Id);
			}
		}
		OriginalSize = Size;
		Content.Init();
		if ((FileName != null) & mainLevel)
		{
			LoadLoader();
		}
		if (Id == null)
		{
			Id = LevelData?.Id ?? Path.GetFileName(Path.GetDirectoryName(FileName)) ?? "undefined_id";
		}
		GameJnG.IsBountyHunterEnabled = GameJnG.IsItemIdInCargo("other/bounty_hunter");
		LayerList?.ForEach((Layer layer2) =>
		{
			layer2.Init();
		});
		ScoreEmitterData = GameCore.GetParticleEmitterData("hero_score");
		HerotTextEmitterData = GameCore.GetParticleEmitterData("hero_hp");
		if (mainLevel)
		{
			StartScrollSpeed = ScrollSpeed;
			SetCurrentLayer(MainLayer);
			Camera.SetViewPort(GameCore.GraphicsDeviceStatic.Viewport);
			Camera.SetLayer(MainLayer);
			if (resetCamera)
			{
				Camera.Position = Vector2.Zero;
			}
			for (int num = 1; num <= 3; num++)
			{
				LocationTextList.Add(TextBundle.GetText($"loc_{num}_{Id}"));
			}
		}
	}

	public void Begin()
	{
		GameCore.CurrentLevel = this;
		FactorManager.ActualizeFactors(LevelData);
		GameJnG.LevelBegin(this);
		GameJnG.CancelFinishLevel();
		GameJnG.BossMeterList.Clear();
		GameCore.MaxParticleCountPerLayer = 500 + Options.LevelOfDetails * 300;
		GameJnG.IsTimeCompressorEnabled = false;
		PhysWorld.Gravity = Gravity;
		ScrollSpeed = StartScrollSpeed;
		AmbientFactor = Vector3.One;
		if (!GameCore.IsEditorEnabled)
		{
			BuildAutoLinks(null);
		}
		foreach (Layer layer in LayerList)
		{
			SetCurrentLayer(layer);
			foreach (GameObject gameObject in layer.GameObjectList)
			{
				gameObject.DoOnLevelBegin();
			}
		}
		if (!GameCore.IsEditorEnabled)
		{
			GameCore.SetFadeEffect(bFadeIn: true);
		}
		FxEffect.InitData();
		if (GameJnG.IsDrunkenEffectEnabled && this == GameCore.GameLevel && this != GameJnG.ShopMenu.HeroPreviewLevel)
		{
			FxEffect.StartDrunkenEffect(this);
		}
		if (!GameJnG.IsTestRangeEnabled)
		{
			GameCore.SoundSystem.StopMusic();
		}
		GameCore.SoundSystem.SetAllSoundData(GameCore.GetAllSoundData());
		OnLevelBegin?.Invoke();
		GameCore.Allocator.Report();
	}

	public bool SaveLoader(string fileName)
	{
		List<string> list = SaveToStringList();
		try
		{
			new FileInfo(fileName).Directory.Create();
			File.WriteAllLines(fileName, list.ToArray());
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unable to save loader to '{0}'. Ex: {1}", fileName, ex);
			return false;
		}
		return true;
	}

	public List<string> SaveToStringList()
	{
		if (GameCore.Editor.IsGroupSelected)
		{
			GameCore.Editor.CurrentSelectionGroup.DetachSelectedObjects();
		}
		int maxPk = FirstObjectPk;
		foreach (Layer layer in LayerList)
		{
			layer.GameObjectList.ForEach((GameObject go) =>
			{
				go.Pk = maxPk++;
			});
		}
		List<string> lineList = new List<string>();
		foreach (Layer layer2 in LayerList)
		{
			if (layer2.LoopX >= 0f && layer2.CopyLoopLayerId == null)
			{
				lineList.Add($"loop.{layer2.Id}={(int)layer2.LoopX},{(int)layer2.LoopOffsetX}");
			}
		}
		foreach (Layer layer3 in LayerList)
		{
			lineList.Add($"//===== LAYER {layer3.Id} =====");
			lineList.Add($"l.{layer3.Id}={((layer3 == MainLayer) ? 1 : 0)}");
			lineList.Add($"amb={layer3.AmbientLightColor.R},{layer3.AmbientLightColor.G},{layer3.AmbientLightColor.B}");
			lineList.Add($"lig={(int)(layer3.LightMapFactorArray[0] * 100f)},{(int)(layer3.LightMapFactorArray[1] * 100f)},{(int)(layer3.LightMapFactorArray[2] * 100f)},{layer3.LightToMapNumber},{(int)(layer3.ShadowMapFactor * 100f)},{layer3.ShadowFromMapNumber},{layer3.ShadowToMapNumber}");
			foreach (GameObject gameObject3 in layer3.GameObjectList)
			{
				if (!gameObject3.IsLevelObject)
				{
					continue;
				}
				string text = ((gameObject3.PartData == null) ? $"e.{gameObject3.Id},{gameObject3.Pk}" : $"p.{gameObject3.PartData.Id},{gameObject3.Pk}");
				lineList.Add(text + $"={(int)gameObject3.Position.X},{(int)gameObject3.Position.Y},{gameObject3.ZOrder},{(gameObject3.CreatePhysBody ? 1 : 0)}");
				if (gameObject3.Scale.X != 1f || gameObject3.Scale.Y != 1f || gameObject3.Rotation != 0f)
				{
					lineList.Add($"tra={Utils.FormatFloat4(gameObject3.Scale.X)},{Utils.FormatFloat4(gameObject3.Scale.Y)},{Utils.FormatFloat4(gameObject3.Rotation)}");
				}
				if (gameObject3.PolygonRenderer != null && !gameObject3.PolygonRenderer.VertexList.IsNullOrEmpty() && !gameObject3.PolygonRenderer.IsAutogenerated)
				{
					string text2 = null;
					foreach (PolygonVertex vertex in gameObject3.PolygonRenderer.VertexList)
					{
						if (text2 != null)
						{
							text2 += ",";
						}
						text2 += $"{(int)vertex.Position.X},{(int)vertex.Position.Y}";
					}
					lineList.Add($"vx={text2}");
					if (!gameObject3.PolygonRenderer.IsTopEnabled || !gameObject3.PolygonRenderer.IsBottomEnabled)
					{
						lineList.Add($"vxs={(gameObject3.PolygonRenderer.IsTopEnabled ? 1 : 0)},{(gameObject3.PolygonRenderer.IsBottomEnabled ? 1 : 0)}");
					}
					if (gameObject3.PolygonRenderer.TextureOffset != Vector2.Zero)
					{
						lineList.Add($"txy={Utils.FormatFloat4(gameObject3.PolygonRenderer.TextureOffset.X)},{Utils.FormatFloat4(gameObject3.PolygonRenderer.TextureOffset.Y)}");
					}
					if (gameObject3.PolygonRenderer.TextureMappingMode != PolygonTextureMappingModeEnum.RelativeToLevel)
					{
						lineList.Add($"tm={(int)gameObject3.PolygonRenderer.TextureMappingMode}");
					}
				}
				if (gameObject3.DifficultyRange.From > 0 || gameObject3.DifficultyRange.To < 999)
				{
					lineList.Add(string.Format($"dff={gameObject3.DifficultyRange.From},{gameObject3.DifficultyRange.To}"));
				}
				if (!gameObject3.GameModeList.IsNullOrEmpty())
				{
					int num = 0;
					text = "gm=";
					foreach (int gameMode in gameObject3.GameModeList)
					{
						if (num > 0)
						{
							text += ",";
						}
						text += gameMode;
						num++;
					}
					lineList.Add(text);
				}
				if (gameObject3.ForPlayerCount > 0 && gameObject3.ForPlayerCount < 3)
				{
					lineList.Add(string.Format($"ply={gameObject3.ForPlayerCount}"));
				}
				if (gameObject3.CurrentState != null && gameObject3.Color != gameObject3.CurrentState.Color)
				{
					lineList.Add(string.Format($"rgb={(int)gameObject3.Color.R},{(int)gameObject3.Color.G},{(int)gameObject3.Color.B},{(int)gameObject3.Color.A}"));
				}
				if (gameObject3.TriggerPoint != Vector2.Zero)
				{
					lineList.Add(string.Format($" tp={(int)gameObject3.TriggerPoint.X},{(int)gameObject3.TriggerPoint.Y}"));
				}
				if (gameObject3.TriggerPointMode != TriggerPointModeEnum.Normal)
				{
					lineList.Add(string.Format($" tpm={(int)gameObject3.TriggerPointMode}"));
				}
				if (gameObject3.LinkedObjectList != null && gameObject3.LinkedObjectList.Count > 0)
				{
					text = " lnk=";
					for (int num2 = 0; num2 < gameObject3.LinkedObjectList.Count; num2++)
					{
						if (num2 > 0)
						{
							text += ",";
						}
						text += gameObject3.LinkedObjectList[num2].Pk;
					}
					lineList.Add(text);
				}
				if (gameObject3.HasEdge)
				{
					List<Vector2> edgeVertexList = gameObject3.RigidBody.GetEdgeVertexList();
					for (int num3 = 0; num3 < edgeVertexList.Count; num3++)
					{
						Vector2 worldPosition = edgeVertexList[num3];
						Vector2 relativePosition = gameObject3.GetRelativePosition(worldPosition);
						text = "edg";
						if (num3 == edgeVertexList.Count - 1)
						{
							text += ".end";
						}
						lineList.Add(text + $"={Utils.FormatFloat4(relativePosition.X)},{Utils.FormatFloat4(relativePosition.Y)}");
					}
				}
				if (gameObject3.MyTimeLine != null && !gameObject3.MyTimeLine.EventNodeList.IsNullOrEmpty())
				{
					foreach (BehaviorEventNode eventNode in gameObject3.MyTimeLine.EventNodeList)
					{
						lineList.Add($" tln={(int)(eventNode.Time * 1000f)}");
						if (eventNode.DifficultyRange.From != 0 || eventNode.DifficultyRange.To != 999)
						{
							lineList.Add($"  tln_dif={eventNode.DifficultyRange.From},{eventNode.DifficultyRange.To}");
						}
						if (eventNode.LinkedObjectList != null && eventNode.LinkedObjectList.Count > 0)
						{
							text = "  tln_lnk=";
							for (int num4 = 0; num4 < eventNode.LinkedObjectList.Count; num4++)
							{
								if (num4 > 0)
								{
									text += ",";
								}
								text += eventNode.LinkedObjectList[num4].Pk;
							}
							lineList.Add(text);
						}
						foreach (BehaviorEvent @event in eventNode.EventList)
						{
							string text3 = ((!@event.ValueString.IsNullOrEmpty()) ? @event.ValueString : @event.ValueInt.ToString());
							if (@event.MyObject == null || @event.MyObject.IsMainPart)
							{
								lineList.Add($"  tle={@event.BehaviorId},{@event.Name},{text3}");
								continue;
							}
							lineList.Add($"  tle.{@event.MyObject.Id}={@event.BehaviorId},{@event.Name},{text3}");
						}
					}
				}
				if (gameObject3.MyPath == null)
				{
					continue;
				}
				foreach (Vector2 controlPoint in gameObject3.MyPath.ControlPointList)
				{
					lineList.Add($" lp={(int)controlPoint.X},{(int)controlPoint.Y}");
				}
				foreach (BehaviorEventNode eventNode2 in gameObject3.MyPath.EventNodeList)
				{
					if (eventNode2.Time < 0f || eventNode2.Time > 1f)
					{
						GameCore.LogErr("Wrong node time: {0} for object '{1}' at {2}, {3}", eventNode2.Time, gameObject3.Id, (int)gameObject3.WorldPosition.X, (int)gameObject3.WorldPosition.Y);
					}
					else
					{
						lineList.Add($" lpn={(int)(eventNode2.Time * 1000f)}");
						if (eventNode2.DifficultyRange.From != 0 || eventNode2.DifficultyRange.To != 999)
						{
							lineList.Add($"  lpn_dif={eventNode2.DifficultyRange.From},{eventNode2.DifficultyRange.To}");
						}
						if (eventNode2.LinkedObjectList != null && eventNode2.LinkedObjectList.Count > 0)
						{
							text = "  lpn_lnk=";
							for (int num5 = 0; num5 < eventNode2.LinkedObjectList.Count; num5++)
							{
								if (num5 > 0)
								{
									text += ",";
								}
								text += eventNode2.LinkedObjectList[num5].Pk;
							}
							lineList.Add(text);
						}
						foreach (BehaviorEvent event2 in eventNode2.EventList)
						{
							string text4 = ((!event2.ValueString.IsNullOrEmpty()) ? event2.ValueString : event2.ValueInt.ToString());
							if (event2.MyObject == null || event2.MyObject.IsMainPart)
							{
								lineList.Add($"  lpe={event2.BehaviorId},{event2.Name},{text4}");
								continue;
							}
							lineList.Add($"  lpe.{event2.MyObject.Id}={event2.BehaviorId},{event2.Name},{text4}");
						}
					}
					if (eventNode2.TimeLine == null || eventNode2.TimeLine.EventNodeList.IsNullOrEmpty())
					{
						continue;
					}
					foreach (BehaviorEventNode eventNode3 in eventNode2.TimeLine.EventNodeList)
					{
						lineList.Add($" lptln={(int)(eventNode3.Time * 1000f)}");
						if (eventNode3.LinkedObjectList != null && eventNode3.LinkedObjectList.Count > 0)
						{
							text = "  lptln_lnk=";
							for (int num6 = 0; num6 < eventNode3.LinkedObjectList.Count; num6++)
							{
								if (num6 > 0)
								{
									text += ",";
								}
								text += eventNode3.LinkedObjectList[num6].Pk;
							}
							lineList.Add(text);
						}
						foreach (BehaviorEvent event3 in eventNode3.EventList)
						{
							string text5 = ((!event3.ValueString.IsNullOrEmpty()) ? event3.ValueString : event3.ValueInt.ToString());
							if (event3.MyObject == null || event3.MyObject.IsMainPart)
							{
								lineList.Add($"   lptle={event3.BehaviorId},{event3.Name},{text5}");
								continue;
							}
							lineList.Add($"   lptle.{event3.MyObject.Id}={event3.BehaviorId},{event3.Name},{text5}");
						}
					}
				}
			}
		}
		if (SafePointList != null)
		{
			lineList.Add("//===== SAFE LINE =====");
			SafePointList.ForEach((SafePoint sfp) =>
			{
				lineList.Add($"sfp={(int)sfp.Position.X},{(int)sfp.Position.Y}");
			});
		}
		foreach (Joint joint in PhysWorld.JointList)
		{
			GameObject gameObject = joint.BodyA.UserData as GameObject;
			GameObject gameObject2 = joint.BodyB.UserData as GameObject;
			if (gameObject != null && gameObject2 != null)
			{
				switch (joint.JointType)
				{
				case JointType.Rope:
				{
					RopeJoint ropeJoint = joint as RopeJoint;
					lineList.Add($"jo.rop={gameObject.Pk},{gameObject2.Pk},{ropeJoint.LocalAnchorA.X.ToString(Utils.FloatFI)},{ropeJoint.LocalAnchorA.Y.ToString(Utils.FloatFI)},{ropeJoint.LocalAnchorB.X.ToString(Utils.FloatFI)},{ropeJoint.LocalAnchorB.Y.ToString(Utils.FloatFI)}");
					break;
				}
				case JointType.Revolute:
				{
					RevoluteJoint revoluteJoint = joint as RevoluteJoint;
					lineList.Add($"jo.rev={gameObject.Pk},{gameObject2.Pk},{revoluteJoint.LocalAnchorA.X.ToString(Utils.FloatFI)},{revoluteJoint.LocalAnchorA.Y.ToString(Utils.FloatFI)},{revoluteJoint.LocalAnchorB.X.ToString(Utils.FloatFI)},{revoluteJoint.LocalAnchorB.Y.ToString(Utils.FloatFI)}");
					break;
				}
				}
			}
		}
		if (GameCore.Editor.IsGroupSelected)
		{
			foreach (GameObject @object in GameCore.Editor.CurrentSelectionGroup.ObjectList)
			{
				GameCore.Editor.CurrentSelectionGroup.SelectionObject.AddChildRelative(@object, @object.Position.X - GameCore.Editor.CurrentSelectionGroup.SelectionObject.Position.X, @object.Position.Y - GameCore.Editor.CurrentSelectionGroup.SelectionObject.Position.Y);
			}
		}
		return lineList;
	}

	public string[] LoadLoader()
	{
		if (FileName == null)
		{
			return null;
		}
		using IniFile iniFile = IniFile.OpenFromContent(Path.Combine(Path.GetDirectoryName(FileName), "loader.jnl"));
		if (iniFile == null)
		{
			return null;
		}
		LoadLoaderFromIniFile(iniFile);
		return iniFile.LineArray;
	}

	public void LoadLoaderFromLineArray(string[] lines)
	{
		IniFile iniFile = new IniFile();
		iniFile.SetLines(lines);
		LoadLoaderFromIniFile(iniFile);
	}

	public void LoadLoaderFromIniFile(IniFile iniFile)
	{
		BehaviorEventNode behaviorEventNode = null;
		BehaviorEventNode behaviorEventNode2 = null;
		BehaviorTimeLine behaviorTimeLine = null;
		SafePointList.Clear();
		FactorManager.ActualizeFactors(LevelData);
		GameObject gameObject = null;
		List<Vector2> list = null;
		LayerSegmentObjectData layerSegmentObjectData = null;
		LayerSegmentData layerSegmentData = null;
		LayerSegmentData layerSegmentData2 = null;
		while (iniFile.NextLine())
		{
			float[] resultArray;
			int[] resultArray2;
			if (iniFile.KeyEquals("l"))
			{
				UseLayer(iniFile.Id);
				gameObject = null;
				list = null;
				if (CurrentLayer != null)
				{
					layerSegmentData = CurrentLayer.AddSegment();
					layerSegmentData2 = CurrentLayer.AddSegment();
				}
			}
			else if (iniFile.KeyEquals("sfp") && iniFile.TryGetFloatArray(2, out resultArray))
			{
				AddSafePoint(new Vector2(resultArray[0], resultArray[1]));
			}
			else if (iniFile.KeyEquals("loop") && iniFile.TryGetIntArray(2, out resultArray2))
			{
				Layer layer = ((iniFile.Id == null) ? CurrentLayer : GetLayerById(iniFile.Id));
				if (layer != null)
				{
					layer.LoopX = resultArray2[0];
					layer.LoopOffsetX = resultArray2[1];
					if (layer.LoopOffsetX < 1920f)
					{
						iniFile.LogErr("LoopOffsetX = {0} is too low - fixed to {1}", layer.LoopOffsetX, 1920);
						layer.LoopOffsetX = 1920f;
					}
					foreach (Layer layer2 in LayerList)
					{
						if (layer.Id.Equals(layer2.CopyLoopLayerId) && layer != layer2)
						{
							layer2.LoopX = layer.LoopX;
							layer2.LoopOffsetX = layer.LoopOffsetX;
						}
					}
				}
				else
				{
					iniFile.LogErr("Unable to find layer '{0}' for loop", iniFile.Id);
				}
			}
			else
			{
				if (CurrentLayer == null)
				{
					continue;
				}
				if (iniFile.KeyEquals("amb") && iniFile.TryGetColorValue(out var color))
				{
					CurrentLayer.AmbientLightColor = color;
				}
				else if (iniFile.KeyEquals("lig") && iniFile.TryGetFloatArray(7, out resultArray))
				{
					CurrentLayer.LightMapFactorArray[0] = resultArray[0] / 100f;
					CurrentLayer.LightMapFactorArray[1] = resultArray[1] / 100f;
					CurrentLayer.LightMapFactorArray[2] = resultArray[2] / 100f;
					CurrentLayer.LightToMapNumber = (int)resultArray[3];
					CurrentLayer.ShadowMapFactor = resultArray[4] / 100f;
					CurrentLayer.ShadowFromMapNumber = (int)resultArray[5];
					CurrentLayer.ShadowToMapNumber = (int)resultArray[6];
				}
				else if (iniFile.KeyEquals("p") && iniFile.TryGetFloatArray(4, out resultArray))
				{
					string id = null;
					int result = 0;
					if (iniFile.Id != null)
					{
						string[] idStringArray = iniFile.GetIdStringArray();
						id = idStringArray[0];
						if (idStringArray.Length > 2)
						{
							int.TryParse(idStringArray[2], out result);
						}
						else
						{
							int.TryParse(idStringArray[1], out result);
						}
					}
					Vector2 position = new Vector2(resultArray[0], resultArray[1]);
					layerSegmentObjectData = ((!(position.X < CurrentLayer.LoopX)) ? layerSegmentData2.AddObjectData(id) : layerSegmentData.AddObjectData(id));
					layerSegmentObjectData.Pk = result;
					layerSegmentObjectData.Position = position;
					layerSegmentObjectData.ZOrder = (int)resultArray[2];
					layerSegmentObjectData.CreatePhysBody = resultArray[3] != 0f;
					layerSegmentObjectData.Color = layerSegmentObjectData.PartData.DefaultPartState.Color;
					list = null;
					behaviorEventNode = null;
					behaviorEventNode2 = null;
				}
				else
				{
					if (layerSegmentObjectData == null)
					{
						continue;
					}
					if (iniFile.KeyEquals("tra") && iniFile.TryGetFloatArray(3, out resultArray))
					{
						layerSegmentObjectData.Scale = new Vector2(resultArray[0], resultArray[1]);
						layerSegmentObjectData.Rotation = resultArray[2];
					}
					else if (iniFile.KeyEquals("vx") && iniFile.TryGetFloatArray(2, 998, out resultArray))
					{
						layerSegmentObjectData.PolygonVertexArray = resultArray;
					}
					else if (iniFile.KeyEquals("vxs") && iniFile.TryGetIntArray(2, out resultArray2))
					{
						layerSegmentObjectData.IsPolygonTopEnabled = resultArray2[0] != 0;
						layerSegmentObjectData.IsPolygonBottomEnabled = resultArray2[1] != 0;
					}
					if (iniFile.KeyEquals("txy"))
					{
						layerSegmentObjectData.PolygonTextureOffset = iniFile.GetVector2();
					}
					string[] resultArray3;
					if (iniFile.KeyEquals("tm"))
					{
						switch (iniFile.GetIntValue())
						{
						case 1:
							layerSegmentObjectData.PolygonTextureMappingMode = PolygonTextureMappingModeEnum.RelativeToPivot;
							break;
						case 2:
							layerSegmentObjectData.PolygonTextureMappingMode = PolygonTextureMappingModeEnum.FixedRect;
							break;
						case 3:
							layerSegmentObjectData.PolygonTextureMappingMode = PolygonTextureMappingModeEnum.FixedHeight;
							break;
						default:
							layerSegmentObjectData.PolygonTextureMappingMode = PolygonTextureMappingModeEnum.RelativeToLevel;
							break;
						}
					}
					else if (iniFile.KeyEquals("dff"))
					{
						int[] intArray = iniFile.GetIntArray();
						if (!intArray.IsNullOrEmpty())
						{
							layerSegmentObjectData.DifficultyRange.From = intArray[0];
							if (intArray.Length > 1)
							{
								layerSegmentObjectData.DifficultyRange.To = intArray[1];
							}
							else
							{
								layerSegmentObjectData.DifficultyRange.To = 999;
							}
						}
					}
					else if (iniFile.KeyEquals("gm"))
					{
						int[] intArray2 = iniFile.GetIntArray();
						if (!intArray2.IsNullOrEmpty())
						{
							layerSegmentObjectData.GameModeList = new List<int>(intArray2);
						}
					}
					else if (iniFile.KeyEquals("ply"))
					{
						layerSegmentObjectData.ForPlayerCount = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("rgb") && iniFile.TryGetFloatArray(4, out resultArray))
					{
						layerSegmentObjectData.Color = new Color((int)resultArray[0], (int)resultArray[1], (int)resultArray[2], (int)resultArray[3]);
					}
					else if (iniFile.KeyEquals("edg") && iniFile.TryGetFloatArray(2, out resultArray))
					{
						if (list == null)
						{
							list = new List<Vector2>();
						}
						list.Add(new Vector2(resultArray[0], resultArray[1]));
						if (iniFile.IdEquals("end"))
						{
							if (gameObject.RigidBody == null)
							{
								gameObject.CreateRigidBody();
								gameObject.PhysClass = PhysClass.Ground;
							}
							else
							{
								gameObject.RigidBody.ResetBody();
							}
							gameObject.HasEdge = true;
							gameObject.RigidBody.BuildEdgeBody(list);
						}
					}
					else if (iniFile.KeyEquals("tp") && iniFile.TryGetFloatArray(2, out resultArray))
					{
						layerSegmentObjectData.TriggerPoint = new Vector2(resultArray[0], resultArray[1]);
					}
					else if (iniFile.KeyEquals("tpm"))
					{
						layerSegmentObjectData.TriggerMode = (TriggerPointModeEnum)iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("lnk"))
					{
						layerSegmentObjectData.LinkPkArrray = iniFile.GetIntArray();
					}
					else if (iniFile.KeyEquals("lp") && iniFile.TryGetFloatArray(2, out resultArray))
					{
						if (layerSegmentObjectData.Path == null)
						{
							layerSegmentObjectData.Path = new BezierPath("", null);
						}
						layerSegmentObjectData.Path.AddPoint(resultArray[0], resultArray[1]);
					}
					else if (iniFile.KeyEquals("tln"))
					{
						if (layerSegmentObjectData.TimeLine == null)
						{
							layerSegmentObjectData.TimeLine = new BehaviorTimeLine();
						}
						behaviorEventNode2 = layerSegmentObjectData.TimeLine.AddEventNode(iniFile.GetFloatValue() / 1000f);
					}
					else if (behaviorEventNode2 != null && iniFile.KeyEquals("tln_dif") && iniFile.TryGetIntRange(ref behaviorEventNode2.DifficultyRange))
					{
						if (!GameCore.IsEditorEnabled && (behaviorEventNode2.DifficultyRange.From > GameJnG.Difficulty || behaviorEventNode2.DifficultyRange.To < GameJnG.Difficulty))
						{
							layerSegmentObjectData.TimeLine.RemoveEventNode(behaviorEventNode2);
							behaviorEventNode2 = null;
						}
					}
					else if (behaviorEventNode2 != null && iniFile.KeyEquals("tln_lnk"))
					{
						if (behaviorEventNode2.LinkedPkList == null)
						{
							behaviorEventNode2.LinkedPkList = new List<long>();
						}
						int[] intArray3 = iniFile.GetIntArray();
						foreach (int num in intArray3)
						{
							behaviorEventNode2.LinkedPkList.Add(num);
						}
					}
					else if (behaviorTimeLine == null && behaviorEventNode2 != null && iniFile.KeyEquals("tle") && iniFile.TryGetStringArray(3, out resultArray3))
					{
						int result2 = 0;
						string arg = ((iniFile.Id == null) ? $"main/{resultArray3[0]}.{resultArray3[1]}" : $"{iniFile.Id}/{resultArray3[0]}.{resultArray3[1]}");
						if (layerSegmentObjectData.TimeLine != null)
						{
							bool flag = true;
							if (int.TryParse(resultArray3[2], out result2))
							{
								flag = false;
							}
							BehaviorEvent behaviorEvent = behaviorEventNode2.AddEvent(iniFile.Id, resultArray3[0], resultArray3[1], result2);
							if ((behaviorEvent != null) & flag)
							{
								behaviorEvent.ValueString = resultArray3[2];
							}
						}
						else
						{
							iniFile.LogErr($"Unavailable time event name {arg}");
						}
					}
					else if (behaviorTimeLine != null && behaviorEventNode2 != null && iniFile.KeyEquals("lptle") && iniFile.TryGetStringArray(3, out resultArray3))
					{
						int result3 = 0;
						if (iniFile.Id != null)
						{
							_ = $"{iniFile.Id}/{resultArray3[0]}.{resultArray3[1]}";
						}
						else
						{
							_ = $"main/{resultArray3[0]}.{resultArray3[1]}";
						}
						bool flag2 = true;
						if (int.TryParse(resultArray3[2], out result3))
						{
							flag2 = false;
						}
						BehaviorEvent behaviorEvent2 = behaviorEventNode2.AddEvent(iniFile.Id, resultArray3[0], resultArray3[1], result3);
						if ((behaviorEvent2 != null) & flag2)
						{
							behaviorEvent2.ValueString = resultArray3[2];
						}
					}
					else if (layerSegmentObjectData.Path != null && iniFile.KeyEquals("lpn"))
					{
						behaviorEventNode = layerSegmentObjectData.Path.AddEventNode(iniFile.GetFloatValue() / 1000f);
						behaviorTimeLine = null;
					}
					else if (behaviorEventNode != null && iniFile.KeyEquals("lpn_dif") && iniFile.TryGetIntRange(ref behaviorEventNode.DifficultyRange))
					{
						if (!GameCore.IsEditorEnabled && (behaviorEventNode.DifficultyRange.From > GameJnG.Difficulty || behaviorEventNode.DifficultyRange.To < GameJnG.Difficulty))
						{
							layerSegmentObjectData.Path.EventNodeList.Remove(behaviorEventNode);
							behaviorEventNode = null;
						}
					}
					else if (behaviorEventNode != null && layerSegmentObjectData.Path != null && iniFile.KeyEquals("lpn_lnk"))
					{
						if (behaviorEventNode.LinkedPkList == null)
						{
							behaviorEventNode.LinkedPkList = new List<long>();
						}
						int[] intArray3 = iniFile.GetIntArray();
						foreach (int num2 in intArray3)
						{
							behaviorEventNode.LinkedPkList.Add(num2);
						}
						behaviorTimeLine = null;
					}
					else
					{
						if (behaviorEventNode == null)
						{
							continue;
						}
						if (iniFile.KeyEquals("lpe") && iniFile.TryGetStringArray(3, out resultArray3))
						{
							int result4 = 0;
							string arg2 = ((iniFile.Id == null) ? $"main/{resultArray3[0]}.{resultArray3[1]}" : $"{iniFile.Id}/{resultArray3[0]}.{resultArray3[1]}");
							if (layerSegmentObjectData.Path != null)
							{
								bool flag3 = true;
								if (int.TryParse(resultArray3[2], out result4))
								{
									flag3 = false;
								}
								BehaviorEvent behaviorEvent3 = behaviorEventNode.AddEvent(iniFile.Id, resultArray3[0], resultArray3[1], result4);
								if ((behaviorEvent3 != null) & flag3)
								{
									behaviorEvent3.ValueString = resultArray3[2];
								}
							}
							else
							{
								iniFile.LogErr($"Unavailable event name {arg2}");
							}
						}
						else if (iniFile.KeyEquals("lptln"))
						{
							if (behaviorEventNode.TimeLine == null)
							{
								behaviorTimeLine = (behaviorEventNode.TimeLine = new BehaviorTimeLine());
							}
							behaviorEventNode2 = behaviorTimeLine.AddEventNode(iniFile.GetFloatValue() / 1000f);
						}
						else if (behaviorEventNode2 != null && iniFile.KeyEquals("lptln_lnk"))
						{
							if (behaviorEventNode2.LinkedPkList == null)
							{
								behaviorEventNode2.LinkedPkList = new List<long>();
							}
							int[] intArray3 = iniFile.GetIntArray();
							foreach (int num3 in intArray3)
							{
								behaviorEventNode2.LinkedPkList.Add(num3);
							}
						}
					}
				}
			}
		}
		iniFile.Close();
		Build();
	}

	public void BuildLinks(Dictionary<long, GameObject> pkDictionary, List<KeyValuePair<GameObject, long>> linkList)
	{
		foreach (KeyValuePair<GameObject, long> link in linkList)
		{
			GameObject value = null;
			pkDictionary.TryGetValue(link.Value, out value);
			if (value != null)
			{
				link.Key.AddLinkedObject(value);
			}
		}
	}

	public void BuildEventLinks(Dictionary<long, GameObject> pkDictionary, List<GameObject> goList)
	{
		foreach (GameObject go in goList)
		{
			if (go.MyTimeLine != null)
			{
				go.MyTimeLine.BuildLinkedList(pkDictionary);
			}
			if (go.MyPath == null)
			{
				continue;
			}
			foreach (BehaviorEventNode eventNode in go.MyPath.EventNodeList)
			{
				eventNode.BuildLinkedList(pkDictionary);
				if (eventNode.TimeLine != null)
				{
					eventNode.TimeLine.BuildLinkedList(pkDictionary);
				}
			}
		}
	}

	public void BuildAutoLinks(List<GameObject> inputObjectList)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		bool flag;
		if (inputObjectList != null)
		{
			flag = false;
			list2.AddRange(inputObjectList.Where((GameObject go) => go.PartData.AutolinkMode > 0 && go.MyLayer.IsInMainGroup));
		}
		else
		{
			flag = true;
		}
		foreach (Layer layer in LayerList)
		{
			if (!layer.IsInMainGroup)
			{
				continue;
			}
			list.AddRange(layer.GameObjectList.Where((GameObject go) => (go.PartData.TargetClass & TargetClass.Building) != 0 || ((go.PartData.PhysClass & PhysClass.Ground) != 0 && go.MaxHP > 0f)));
			if (flag)
			{
				list2.AddRange(layer.GameObjectList.Where((GameObject go) => go.PartData.AutolinkMode > 0));
			}
		}
		list.Reverse();
		list = list.OrderByDescending((GameObject go) => go.ZOrder).ToList();
		int num = 0;
		foreach (GameObject item in list2)
		{
			foreach (GameObject item2 in list)
			{
				if (item2.IsPointInFrameRect(item.WorldPosition) && item2 != item && item.ZOrder >= item2.ZOrder)
				{
					if (item.PartData.AutolinkMode == 2 || item.PartData.AutolinkMode == 3)
					{
						item2.AddLinkedObject(item);
					}
					if (item.PartData.AutolinkMode == 1 || item.PartData.AutolinkMode == 3)
					{
						item.AddLinkedObject(item2);
					}
					num++;
					break;
				}
			}
		}
		stopwatch.Stop();
	}

	public void Build()
	{
		MaxObjectPk = 0L;
		Dictionary<long, GameObject> pkDictionary = new Dictionary<long, GameObject>();
		List<KeyValuePair<GameObject, long>> linkList = new List<KeyValuePair<GameObject, long>>();
		List<GameObject> createdObjectList = new List<GameObject>();
		foreach (Layer layer in LayerList)
		{
			layer.SegmentList?.ForEach((LayerSegmentData sd) =>
			{
				sd.Build(0L, new Vector2(0f, 0f), pkDictionary, linkList, bSkipExpiredObjects: true, ref createdObjectList);
			});
		}
		BuildLinks(pkDictionary, linkList);
		BuildEventLinks(pkDictionary, createdObjectList);
		SortSafePointList();
		GameCore.SoundSystem.MusicVolumeFactor = 1f;
		GameCore.SoundSystem.SoundVolumeFactor = 1f;
	}

	public void ClearAll()
	{
		Clear();
		LayerList.Clear();
		Content.Clear();
		LocationTextList.Clear();
		GameJnG.CheckpointInfo.Clear();
	}

	public void Clear()
	{
		Timer = 0f;
		NoBulletTime = false;
		Camera.StopQuake();
		if (this == GameCore.Editor.CurrentLevel && GameCore.Editor.IsInitialized)
		{
			GameCore.Editor.UnselectAll();
		}
		GameCore.Allocator.DeleteOwningParticleList(ParticleListDistortion);
		ParticleListDistortion = GameCore.Allocator.GetParticleList();
		WarningTextList.Clear();
		foreach (Layer layer in LayerList)
		{
			layer.DestroyAll();
			layer.Clear();
		}
		if (HeroList != null)
		{
			foreach (Hero hero in HeroList)
			{
				hero.DestroySmallShip();
				hero.Hud.ClearCounters();
			}
		}
		PhysWorld.Clear();
		PreviewObjectList = GameCore.Allocator.DeleteOwningGameObjectList(PreviewObjectList);
		Size = OriginalSize;
		OnObjectKill = null;
		GameJnG.AchievementManager.ClearLevelAvailableValues();
	}

	private void Reset()
	{
		ClearAll();
		GameCore.Editor.Undo.Reset();
		HeroList = null;
		MusicFileName = null;
		MainLayer = null;
		OnLevelBegin = null;
		ScrollSpeed = new Vector2(100f, 0f);
		Size = new Vector2(20000f, 3000f);
		Gravity = new Vector2(0f, 9.8f);
	}

	internal void Reload(string[] lines, bool bUseChecpoint = false)
	{
		if (GameJnG.IsTestRangeEnabled)
		{
			GameCore.SoundSystem.StopAllSounds();
		}
		else
		{
			GameCore.SoundSystem.StopAll();
		}
		Clear();
		if (bUseChecpoint)
		{
			if (GameJnG.CheckpointInfo.IsSaved)
			{
				GameJnG.CheckpointInfo.ApplyPosition(this);
			}
			else
			{
				Camera.Position = Vector2.Zero;
			}
		}
		if (lines == null)
		{
			GameCore.Editor.Undo.Reset();
			string[] array = LoadLoader();
			if (array != null)
			{
				GameCore.Editor.Undo.AddUndo("Start", array);
			}
		}
		else
		{
			LoadLoaderFromLineArray(lines);
		}
		foreach (Layer layer in LayerList)
		{
			SetCurrentLayer(layer);
			layer.Init();
		}
		SetCurrentLayer(MainLayer);
		SpawnHeroes(HeroList);
		PhysWorld.ProcessChanges();
		if (bUseChecpoint && GameJnG.CheckpointInfo.IsSaved)
		{
			GameJnG.CheckpointInfo.AplyValuesBeforeBeginLevel();
		}
		Begin();
		if (bUseChecpoint && GameJnG.CheckpointInfo.IsSaved)
		{
			GameJnG.CheckpointInfo.ApplyValues();
		}
	}

	public void SpawnHeroes(List<Hero> heroList)
	{
		GameJnG.CarnageTV = null;
		HeroList = heroList;
		if (HeroList != null)
		{
			foreach (Hero hero in HeroList)
			{
				hero.SpawnInLevel(this);
				if (hero.IsCarnageTVEnabled && GameJnG.CarnageTV == null)
				{
					GameJnG.CarnageTV = new CarnageTV(HeroList[0].Hud);
				}
			}
		}
		GameJnG.ScoreHUD.Clear();
	}

	public RayCastResult GetRayCastResult(Vector2 start, float rotation, float maxLength, PhysClass physClasses, List<GameObject> ignoreList = null)
	{
		RayCastResult rayCastResult = new RayCastResult();
		Vector2 displayUnits = new Vector2(start.X + Utils.Sin(rotation) * maxLength, start.Y - Utils.Cos(rotation) * maxLength);
		rayCastResult.StartPoint = start;
		start = ConvertUnits.ToSimUnits(start);
		displayUnits = ConvertUnits.ToSimUnits(displayUnits);
		if (start == displayUnits)
		{
			return rayCastResult;
		}
		rayCastResult.CollisionPosition = displayUnits;
		float minDistance = 100000000f;
		PhysWorld.RayCast((Fixture f, Vector2 p, Vector2 n, float fr) =>
		{
			if (f.Body.UserData is GameObject { IsActivated: not false } gameObject && (gameObject.PhysClass & physClasses) != 0 && (ignoreList == null || !ignoreList.Contains(gameObject)))
			{
				float num = Vector2.Distance(start, p);
				if (num < minDistance)
				{
					minDistance = num;
					rayCastResult.TargetObject = gameObject;
					rayCastResult.CollisionPosition = p;
					rayCastResult.CollisionNormal = n;
				}
			}
			return 1f;
		}, start, displayUnits);
		rayCastResult.CollisionPosition = ConvertUnits.ToDisplayUnits(rayCastResult.CollisionPosition);
		return rayCastResult;
	}

	public List<RayCastResult> GetRayCastResultList(Vector2 start, float rotation, float maxLength, PhysClass physClasses, List<GameObject> ignoreList = null)
	{
		return GetRayCastResultList(end: new Vector2(start.X + Utils.Sin(rotation) * maxLength, start.Y - Utils.Cos(rotation) * maxLength), start: start, physClasses: physClasses, ignoreList: ignoreList);
	}

	public List<RayCastResult> GetRayCastResultList(Vector2 start, Vector2 end, PhysClass physClasses, List<GameObject> ignoreList = null)
	{
		List<RayCastResult> resultList = new List<RayCastResult>();
		if (start == end)
		{
			return resultList;
		}
		start = ConvertUnits.ToSimUnits(start);
		end = ConvertUnits.ToSimUnits(end);
		PhysWorld.RayCast((Fixture f, Vector2 p, Vector2 n, float fr) =>
		{
			if (f.Body.UserData is GameObject { IsActivated: not false } gameObject && (gameObject.PhysClass & physClasses) != 0 && (ignoreList == null || !ignoreList.Contains(gameObject)))
			{
				RayCastResult rayCastResult = new RayCastResult();
				rayCastResult.StartPoint = start;
				rayCastResult.TargetObject = gameObject;
				rayCastResult.CollisionPosition = p;
				rayCastResult.CollisionNormal = n;
				rayCastResult.CollisionPosition = ConvertUnits.ToDisplayUnits(rayCastResult.CollisionPosition);
				resultList.Add(rayCastResult);
			}
			return 1f;
		}, start, end);
		return resultList;
	}

	public override string ToString()
	{
		return Id;
	}
}
