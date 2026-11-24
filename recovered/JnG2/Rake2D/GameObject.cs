using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Debug;
using Rake2D.Editor;
using Rake2D.JnG;
using Rake2D.Script;
using Rake2D.Sound;
using Rake2D.Spine;
using Spine;

namespace Rake2D;

public class GameObject : Transformation
{
	public delegate void OnEventHandler();

	public delegate void OnGameObjectEventHandler(GameObject go);

	public delegate void OnHitBeginHandler(ref float damage, DamageTypeEnum damageType, GameObject hittingObject);

	public delegate void OnHitHandler(float damage, DamageTypeEnum damageType, GameObject hittingObject);

	public delegate void OnShockHitHandler(DamageTypeEnum damageType, Vector2 physForce, GameObject hittingObject);

	public delegate void OnLinkHandler(GameObject senderobject, string linkName, int linkValue);

	public delegate void OnEditorUpdateHandler(Rake2D.Editor.Editor editor, float elapsedTime);

	public delegate void OnUpdateHandler(float elapsedTime);

	public delegate void OnDrawHandler(SpriteBatch spriteBatch);

	public delegate void OnSpineAnimHandler(int trackNumber);

	public delegate void OnSpineAnimCompletedHandler(int trackNumber, int loopCount);

	public delegate void OnSpineAnimEventHandler(string eventName, int intParam, float floatParam, string stringParam);

	public delegate void OnCollisionHandler(GameObject myObject, GameObject collisionObject, Vector2 contactPoint, Vector2 normal);

	public delegate void OnEvironmentChangedHandler(EnvironmentEnum env);

	public long Pk;

	public float CreateTime;

	public int ZOrder;

	public RigidBody RigidBody;

	public Vector2 TriggerPoint;

	public bool WasTriggerPointActivated;

	public float OldRotation;

	public Vector2 OldPosition;

	private Vector2 _velocity;

	public Motion Motion;

	public OutOfFrame DestroyOutOfView;

	public OutOfFrame DestroyOutOfLevel;

	public BezierPath MyPath;

	public ParticleEmitter Emitter;

	public float LifeTime = float.MaxValue;

	public float Timer;

	public float HitEffectTimer;

	public float KillTimer = float.MaxValue;

	public float PathSpeed;

	public int IgnoreCollisionMode;

	public TriggerPointModeEnum TriggerPointMode;

	public PathFollowModeEnum PathFollowMode = PathFollowModeEnum.FreeRotation;

	public KeyFrameValue SpeedKeyValue;

	public KeyFrameValue AngleKeyValue;

	public Vector2 PathOscilation;

	public bool CreatePhysBody = true;

	public List<GameObject> ChildObjectListFront;

	public List<GameObject> ChildObjectListBack;

	public List<GameObject> LinkedObjectList;

	public List<Particle> ParticleListFront;

	public List<Particle> ParticleListBack;

	public BehaviorTimeLine MyTimeLine;

	public HeroItem MyItem;

	public SpineConnector SpineConnector;

	public SpineRenderer SpineRenderer;

	public PolygonRenderer PolygonRenderer;

	public object BehaviorData;

	public GameObject TargetPart;

	public IntRange DifficultyRange;

	public List<int> GameModeList;

	public int ForPlayerCount;

	public float MaxHP;

	public float CurrentHP;

	public float Damage;

	public DamageTypeEnum DamageType = DamageTypeEnum.Unknown;

	public DamageTypeEnum LastDamageType = DamageTypeEnum.Unknown;

	public GameObject LastHittingObject;

	public bool HitOnCollision = true;

	public bool ShareHits = true;

	public bool IsImmortal;

	public Vector2 ScreenPosition;

	public Vector2 VirtualScreenPosition;

	public Vector2 TargetVirtualScreenPosition;

	public string RemconText;

	public Hero RemconedByHero;

	public Hero RemconedCancelByHero;

	public bool IsRemconable;

	public bool IsLevelObject;

	public bool IsInitDone;

	public bool IsLevelInitDone;

	public bool IsPathPaused;

	public bool IsInLevelStats;

	private int _sendedLinkCount;

	public PartState CurrentState;

	public PartState LastState;

	public string PartStateId;

	public PartData PartData;

	public string CurrAtlasAnimId;

	public bool HasEdge;

	public bool IsKilled;

	public bool ResetSpineTrackOnDestroy = true;

	public bool HasScreenRelativePath;

	private Rectangle _frameRect;

	private bool _isPhysBodyInitDone;

	private bool _wasStateChanged;

	private bool _isUpdateInEditorEnabled;

	private bool _isEnabled = true;

	private bool _isVisible;

	public Hero MyHero;

	public bool IsHero;

	private GameObject _mainPart;

	public bool DestroyOnCollision;

	public int DestroyOnEndPath;

	private bool _destroyOnKill;

	private PhysClass _physClass;

	public TargetClass TargetClass;

	private bool _wasActivated;

	private bool _isActivated;

	public ActiveModeEnum ActiveMode;

	private Vector2 _framePivot;

	private GameObject _parentObject;

	private List<BehaviorScript> _behaviorList;

	private List<StripLine> _stripLineListBack;

	private List<StripLine> _stripLineListFront;

	private List<ISound2D> _soundList;

	public float PathTime;

	public float TimeLineTime;

	private int _currentTimeNodeIndex;

	private int _currentPathEventNodeIndex;

	public Vector2 ParentThrowVelocity;

	public string Id
	{
		get
		{
			if (PartData != null)
			{
				return PartData.Id;
			}
			return "";
		}
	}

	public int WorldZOrder
	{
		get
		{
			if (ParentObject != null)
			{
				return ParentObject.WorldZOrder;
			}
			return ZOrder;
		}
	}

	public Layer MyLayer { get; private set; }

	public GameLevel MyLevel => MyLayer?.MyLevel;

	public Camera MyCamera
	{
		get
		{
			if (MyLayer != null)
			{
				return MyLayer.MyLevel.Camera;
			}
			return null;
		}
	}

	public bool IsTriggerFree
	{
		get
		{
			if (!WasTriggerPointActivated)
			{
				return TriggerPoint == Vector2.Zero;
			}
			return true;
		}
	}

	public EnvironmentEnum Environment { get; private set; }

	public bool IgnoreWater
	{
		get
		{
			if (!IsHero && PhysClass != PhysClass.ShotHero)
			{
				return PartData.IgnoreWater;
			}
			return true;
		}
	}

	public bool IsInEditorMode
	{
		get
		{
			if (GameCore.IsEditorEnabled && !IsUpdateInEditorEnabled)
			{
				return !GameCore.Editor.IsUpdateBehaviorsEnabled;
			}
			return false;
		}
	}

	public Matrix DrawMatrix => MyCamera.WorldMatrixCurrentLayer;

	public GameObject HitPart { get; protected set; }

	public int PartStateCount
	{
		get
		{
			if (PartData != null && PartData.PartStates != null)
			{
				return PartData.PartStates.Count;
			}
			return 0;
		}
	}

	public override Matrix WorldMatrix
	{
		get
		{
			if (SpineConnector != null && SpineConnector.RootObject != this)
			{
				return GetLocalMatrix() * SpineConnector.WorldMatrix * SpineConnector.RootObject.WorldMatrix;
			}
			return base.WorldMatrix;
		}
	}

	public int ChildZOder { get; private set; }

	public bool IsDestroyed { get; private set; }

	public bool IsDetached { get; private set; }

	public bool IsUpdateInEditorEnabled
	{
		get
		{
			return _isUpdateInEditorEnabled;
		}
		set
		{
			_isUpdateInEditorEnabled = value;
			if (ChildObjectListFront != null)
			{
				int count = ChildObjectListFront.Count;
				for (int i = 0; i < count; i++)
				{
					ChildObjectListFront[i].IsUpdateInEditorEnabled = value;
				}
			}
			if (ChildObjectListBack != null)
			{
				int count2 = ChildObjectListBack.Count;
				for (int j = 0; j < count2; j++)
				{
					ChildObjectListBack[j].IsUpdateInEditorEnabled = value;
				}
			}
		}
	}

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				try
				{
					if (!value)
					{
						OnDisable?.Invoke();
					}
				}
				catch (Exception ex)
				{
					GameCore.LogErr("OnDisable exception: {0}", ex.ToString());
				}
			}
			_isEnabled = value;
			if (ChildObjectListFront != null)
			{
				int count = ChildObjectListFront.Count;
				for (int i = 0; i < count; i++)
				{
					ChildObjectListFront[i].IsEnabled = value;
				}
			}
			if (ChildObjectListBack != null)
			{
				int count2 = ChildObjectListBack.Count;
				for (int j = 0; j < count2; j++)
				{
					ChildObjectListBack[j].IsEnabled = value;
				}
			}
		}
	}

	public bool IsVisible
	{
		get
		{
			if (_isVisible && (ParentObject == null || ParentObject.IsVisible))
			{
				return true;
			}
			return false;
		}
		set
		{
			_isVisible = value;
		}
	}

	public bool IsInLOD
	{
		get
		{
			if (PartData != null && PartData.MinLOD > Options.LevelOfDetails)
			{
				return GameCore.IsEditorEnabled;
			}
			return true;
		}
	}

	public GameObject ParentObject
	{
		get
		{
			return _parentObject;
		}
		set
		{
			_parentObject = value;
			_mainPart = null;
			ParentTransformation = _parentObject;
		}
	}

	public override Color Color
	{
		get
		{
			if (ColorMode == ColorMode.MainAlpha && !IsMainPart)
			{
				Color myColor = GetMyColor();
				return new Color(myColor.R, myColor.G, myColor.B, (byte)((float)(int)GetMainPart().Alpha * ((float)(int)myColor.A / 255f)));
			}
			return base.Color;
		}
	}

	public PhysClass PhysClass
	{
		get
		{
			return _physClass;
		}
		set
		{
			_physClass = value;
			if (RigidBody != null)
			{
				RigidBody.SetPhysClass(value);
			}
		}
	}

	public bool HasDefinedFrameRectangle
	{
		get
		{
			if (PartData != null)
			{
				return !PartData.FrameRect.IsEmpty;
			}
			return false;
		}
	}

	public bool HasDefinedVisibilityRect
	{
		get
		{
			if (PartData != null)
			{
				return !PartData.VisibilityRect.IsEmpty;
			}
			return false;
		}
	}

	public bool HasDefinedFxRectangle
	{
		get
		{
			if (PartData != null)
			{
				return !PartData.FxRect.IsEmpty;
			}
			return false;
		}
	}

	public bool HasGraphics
	{
		get
		{
			if (SpriteRenderer != null || SpineRenderer != null || PolygonRenderer != null)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsMainPart => ParentObject == null;

	public Rectangle VisibilityRect { get; private set; }

	public bool IsMovable
	{
		get
		{
			if (Motion != null)
			{
				return true;
			}
			return false;
		}
		set
		{
			if (value)
			{
				Motion = new Motion();
			}
			else
			{
				Motion = null;
			}
		}
	}

	public bool IsActivated
	{
		get
		{
			if (!_isActivated)
			{
				if (ParentObject != null)
				{
					return ParentObject.IsActivated;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsUpdatable
	{
		get
		{
			if (!IsDestroyed && IsEnabled && IsActivated && IsInLOD)
			{
				if (TriggerPointMode != TriggerPointModeEnum.Normal)
				{
					return GetMainPart().IsTriggerFree;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsAlive
	{
		get
		{
			if (!IsKilled)
			{
				return !IsDestroyed;
			}
			return false;
		}
	}

	public bool IsOnScreenX { get; private set; }

	public OutOfFrame OutOfView { get; internal set; }

	public OutOfFrame OutOfLevel { get; internal set; }

	public bool IsDynamic
	{
		get
		{
			if (RigidBody != null && !RigidBody.IsStatic)
			{
				return true;
			}
			return false;
		}
		set
		{
			if (RigidBody != null)
			{
				RigidBody.IsStatic = !value;
			}
		}
	}

	public Rectangle FrameRectangle
	{
		get
		{
			if (!HasDefinedFrameRectangle)
			{
				if (SpriteRenderer != null)
				{
					Rectangle frameRectangle = SpriteRenderer.FrameRectangle;
					Vector2 framePivot = SpriteRenderer.FramePivot;
					frameRectangle.X = -(int)framePivot.X;
					frameRectangle.Y = -(int)framePivot.Y;
					return frameRectangle;
				}
			}
			else if (PartData != null)
			{
				return PartData.FrameRect;
			}
			return _frameRect;
		}
	}

	public Rectangle FxRectangle
	{
		get
		{
			if (!HasDefinedFxRectangle)
			{
				return FrameRectangle;
			}
			if (PartData != null)
			{
				return PartData.FxRect;
			}
			return _frameRect;
		}
	}

	public Vector2 FramePivot
	{
		get
		{
			if (!HasDefinedFrameRectangle && SpriteRenderer != null)
			{
				return SpriteRenderer.FramePivot;
			}
			return _framePivot;
		}
	}

	public int Frame
	{
		get
		{
			if (SpriteRenderer != null)
			{
				return SpriteRenderer.Frame;
			}
			return -1;
		}
		set
		{
			if (SpriteRenderer != null)
			{
				SpriteRenderer.Frame = value;
			}
		}
	}

	public string FrameId
	{
		set
		{
			if (SpriteRenderer != null)
			{
				SpriteRenderer.FrameId = value;
			}
		}
	}

	public int FrameCount
	{
		get
		{
			if (SpriteRenderer != null && SpriteRenderer.Atlas != null)
			{
				return SpriteRenderer.Atlas.FrameCount;
			}
			return 0;
		}
	}

	public float HPPercent
	{
		get
		{
			if (IsImmortal)
			{
				return 1f;
			}
			if (!(MaxHP > 0f))
			{
				return 0f;
			}
			return CurrentHP / MaxHP;
		}
	}

	public SpriteRenderer SpriteRenderer { get; private set; }

	public BehaviorTimeLine CurrentTimeLine { get; private set; }

	public float VelocityX
	{
		get
		{
			return Velocity.X;
		}
		set
		{
			SetVelocity(value, Velocity.Y);
		}
	}

	public float VelocityY
	{
		get
		{
			return Velocity.Y;
		}
		set
		{
			SetVelocity(Velocity.X, value);
		}
	}

	public Vector2 Velocity
	{
		get
		{
			if (IsMainPart)
			{
				if (MyPath != null)
				{
					if (HasScreenRelativePath)
					{
						if (IsPathPaused)
						{
							return MyLayer.ScrollSpeed;
						}
						return Utils.GetVelocityVector2(PathSpeed, MyPath.GetDirection(PathTime)) + MyLayer.ScrollSpeed;
					}
					if (IsPathPaused)
					{
						return Vector2.Zero;
					}
					return Utils.GetVelocityVector2(PathSpeed, MyPath.GetDirection(PathTime));
				}
				if (RigidBody != null && !RigidBody.IsStatic)
				{
					return ConvertUnits.ToDisplayUnits(RigidBody.Velocity);
				}
				if (IsMovable)
				{
					return Motion.Velocity;
				}
				return _velocity;
			}
			return GetMainPart().Velocity;
		}
	}

	public Vector2 VelocityNoScroll
	{
		get
		{
			if (IsMainPart)
			{
				if (MyPath != null)
				{
					return Utils.GetVelocityVector2(PathSpeed, MyPath.GetDirection(PathTime));
				}
				if (RigidBody != null && !RigidBody.IsStatic)
				{
					return ConvertUnits.ToDisplayUnits(RigidBody.Velocity);
				}
				if (IsMovable)
				{
					return Motion.Velocity;
				}
				return _velocity;
			}
			return GetMainPart().VelocityNoScroll;
		}
	}

	public float RotationSpeed
	{
		get
		{
			if (RigidBody != null)
			{
				return RigidBody.RotationSpeed;
			}
			if (IsMovable)
			{
				return Motion.RotationSpeed;
			}
			return 0f;
		}
		set
		{
			if (RigidBody != null)
			{
				RigidBody.RotationSpeed = value;
			}
			else if (IsMovable)
			{
				Motion.RotationSpeed = value;
			}
		}
	}

	public event OnEventHandler OnLevelBegin;

	public event OnEventHandler OnTrigger;

	public event OnLinkHandler OnLink;

	public event OnEventHandler OnDestroy;

	public event OnEventHandler OnDisable;

	public event OnEventHandler OnUnregisterEvents;

	public event OnEventHandler OnKill;

	public event OnUpdateHandler OnUpdateBegin;

	public event OnUpdateHandler OnUpdate;

	public event OnUpdateHandler OnUpdateEnd;

	public event OnDrawHandler OnDraw;

	public event OnUpdateHandler OnEditorUpdate;

	public event OnGameObjectEventHandler OnReRegisterEvents;

	public event OnEventHandler OnActivate;

	public event OnEventHandler OnDeactivate;

	public event OnEditorUpdateHandler OnEditorUpdateBegin;

	public event OnEditorUpdateHandler OnEditorUpdateEnd;

	public event OnHitBeginHandler OnHitBegin;

	public event OnHitHandler OnHit;

	public event OnShockHitHandler OnShockHit;

	public event OnGameObjectEventHandler OnBeforeCollision;

	public event OnCollisionHandler OnCollision;

	public event OnCollisionHandler OnAfterCollision;

	public event OnGameObjectEventHandler OnSeparation;

	public event OnEventHandler OnPathEnd;

	public event OnEventHandler OnPathChanged;

	public event OnSpineAnimHandler OnSpineAnimStart;

	public event OnSpineAnimHandler OnSpineAnimEnd;

	public event OnSpineAnimEventHandler OnSpineAnimEvent;

	public event OnSpineAnimCompletedHandler OnSpineAnimCompleted;

	public event OnEventHandler OnAnimEnd;

	public event OnEvironmentChangedHandler OnEnvironmentChanged;

	public event OnEventHandler OnTimeLineChanged;

	public void ChangeMaxHP(float maxHP)
	{
		if (MaxHP > 0f)
		{
			CurrentHP *= maxHP / MaxHP;
		}
		else
		{
			CurrentHP = 0f;
		}
		MaxHP = maxHP;
	}

	public void SetPhysClass(string name)
	{
		PhysClass = DataUtils.GetPhysClassByString(name);
	}

	public void SetHitAndTargetClass(PhysClass physClass, TargetClass targetClass, bool bApplyToChildren = true)
	{
		PhysClass = physClass;
		TargetClass = targetClass;
		if (!bApplyToChildren)
		{
			return;
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].SetHitAndTargetClass(physClass, targetClass);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].SetHitAndTargetClass(physClass, targetClass);
			}
		}
	}

	public override Transformation GetComplexWorldTransformation()
	{
		if (SpineConnector != null && SpineConnector.RootObject != this && !float.IsNaN(WorldMatrix.Left.X))
		{
			if (_precalculatedWorldTransformation == null || !IsInitDone)
			{
				_precalculatedWorldTransformation = GameCore.Allocator.GetTransformation();
				Transformation worldTransformation = SpineConnector.WorldTransformation;
				Matrix worldMatrix = WorldMatrix;
				_ = ParentTransformation.WorldTransformation;
				if (IsRotationFixed)
				{
					_precalculatedWorldTransformation.Rotation = Rotation;
				}
				else
				{
					_precalculatedWorldTransformation.Rotation = Rotation + worldTransformation.Rotation;
					if (SpineConnector.RootObject.WorldScale.X < 0f)
					{
						_precalculatedWorldTransformation.Rotation = (float)Math.PI * 2f - _precalculatedWorldTransformation.Rotation;
					}
					if (SpineConnector.RootObject.WorldScale.Y < 0f)
					{
						_precalculatedWorldTransformation.Rotation = (float)Math.PI * 2f - _precalculatedWorldTransformation.Rotation;
					}
					_precalculatedWorldTransformation.Rotation += SpineConnector.RootObject.WorldRotation;
				}
				_precalculatedWorldTransformation.Position.X = worldMatrix.Translation.X;
				_precalculatedWorldTransformation.Position.Y = worldMatrix.Translation.Y;
				_precalculatedWorldTransformation.Scale = Scale * worldTransformation.Scale * SpineConnector.RootObject.WorldScale;
				GameCore.Allocator.DeleteTransformation(worldTransformation);
			}
			return _precalculatedWorldTransformation;
		}
		return base.GetComplexWorldTransformation();
	}

	public override void SetTransformation(Transformation transformation)
	{
		SetPosition(transformation.Position);
		SetScale(transformation.Scale);
		SetRotation(transformation.Rotation);
	}

	public GameObject GetMainPart()
	{
		if (_mainPart == null)
		{
			_mainPart = this;
			while (_mainPart.ParentObject != null)
			{
				if (_mainPart.ParentObject == _mainPart)
				{
					_mainPart.ParentObject = null;
					GameCore.LogErr("Object '{0}' is same as his parent object. Position: {1} in layer '{2}'", _mainPart.Id, _mainPart.WorldPosition, _mainPart.MyLayer.Id);
					break;
				}
				_mainPart = _mainPart.ParentObject;
			}
		}
		return _mainPart;
	}

	public void Revive()
	{
		CurrentHP = MaxHP;
		IsKilled = false;
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].Revive();
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].Revive();
			}
		}
	}

	public Vector2 GetScreenPos()
	{
		return MyLayer.MyLevel.Camera.CurrentLayerPosToScreen(WorldPosition);
	}

	public Vector2 GetPositionRelativeToMain()
	{
		return WorldPosition - GetMainPart().WorldPosition;
	}

	public void SetDynamic(bool isDynamic, float gravityScale)
	{
		IsDynamic = isDynamic;
		if (RigidBody != null)
		{
			RigidBody.GravityScale = gravityScale;
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = ChildObjectListFront[i];
				gameObject.SetDynamic(isDynamic, gravityScale);
				if (gameObject.RigidBody != null)
				{
					gameObject.RigidBody.Mass = 0f;
				}
			}
		}
		if (ChildObjectListBack == null)
		{
			return;
		}
		int count2 = ChildObjectListBack.Count;
		for (int j = 0; j < count2; j++)
		{
			GameObject gameObject2 = ChildObjectListBack[j];
			gameObject2.SetDynamic(isDynamic, gravityScale);
			if (gameObject2.RigidBody != null)
			{
				gameObject2.RigidBody.Mass = 0f;
			}
		}
	}

	public void SetEnvironment(EnvironmentEnum env)
	{
		if (Environment != env)
		{
			OnEnvironmentChanged?.Invoke(env);
			Environment = env;
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].SetEnvironment(env);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].SetEnvironment(env);
			}
		}
	}

	public BehaviorEvent FindNextEventFromCurrentTime(string name)
	{
		BehaviorEvent behaviorEvent = null;
		GameObject mainPart = GetMainPart();
		BehaviorTimeLine currentTimeLine = mainPart.CurrentTimeLine;
		if (currentTimeLine != null)
		{
			behaviorEvent = currentTimeLine.GetNextEvent(mainPart.TimeLineTime, name, this);
		}
		if (behaviorEvent == null)
		{
			behaviorEvent = GetNextPathEvent(mainPart.PathTime, name);
		}
		return behaviorEvent;
	}

	public Vector2[] GetFrameVerices(bool bWorld)
	{
		return GetFrameVerices(bWorld, FrameRectangle);
	}

	public Vector2[] GetFrameVerices(bool bWorld, Rectangle frameRect)
	{
		Transformation worldTransformation = WorldTransformation;
		if (PolygonRenderer != null)
		{
			return PolygonRenderer.GetBoundingBoxVerices(worldTransformation, bWorld);
		}
		Vector2[] rectangleVertices = GameCore.Allocator.GetRectangleVertices();
		Matrix matrix = Matrix.CreateScale(worldTransformation.Scale.X, worldTransformation.Scale.Y, 1f) * Matrix.CreateRotationZ(worldTransformation.Rotation);
		if (bWorld)
		{
			matrix *= Matrix.CreateTranslation(worldTransformation.Position.X, worldTransformation.Position.Y, 1f);
		}
		rectangleVertices[0] = Vector2.Transform(new Vector2(frameRect.X, frameRect.Y), matrix);
		rectangleVertices[1] = Vector2.Transform(new Vector2(frameRect.X + frameRect.Width, frameRect.Y), matrix);
		rectangleVertices[2] = Vector2.Transform(new Vector2(frameRect.X + frameRect.Width, frameRect.Y + frameRect.Height), matrix);
		rectangleVertices[3] = Vector2.Transform(new Vector2(frameRect.X, frameRect.Y + frameRect.Height), matrix);
		return rectangleVertices;
	}

	public bool IsPointInFrameRect(Vector2 pointPos)
	{
		Vector2 vector = Vector2.Transform(pointPos - WorldPosition, Matrix.CreateRotationZ(0f - Rotation));
		Rectangle frameRectangle = FrameRectangle;
		Transformation worldTransformation = WorldTransformation;
		Matrix matrix = Matrix.CreateScale(worldTransformation.Scale.X, worldTransformation.Scale.Y, 1f);
		Vector2 vector2;
		Vector2 vector3;
		if (PolygonRenderer != null)
		{
			vector2 = Vector2.Transform(PolygonRenderer.BoundingTopLeft, matrix);
			vector3 = Vector2.Transform(PolygonRenderer.BoundingBottomRight, matrix);
		}
		else
		{
			vector2 = Vector2.Transform(new Vector2(frameRectangle.X, frameRectangle.Y), matrix);
			vector3 = Vector2.Transform(new Vector2(frameRectangle.X + frameRectangle.Width, frameRectangle.Y + frameRectangle.Height), matrix);
		}
		if (vector.X >= Math.Min(vector2.X, vector3.X) && vector.X <= Math.Max(vector3.X, vector2.X) && vector.Y >= Math.Min(vector2.Y, vector3.Y) && vector.Y <= Math.Max(vector3.Y, vector2.Y))
		{
			return true;
		}
		return false;
	}

	internal void DoOnShockHit(DamageTypeEnum damageType, Vector2 physForce, GameObject hittingObject)
	{
		if (!IsDestroyed)
		{
			try
			{
				OnShockHit?.Invoke(damageType, physForce, hittingObject);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Error in OnShockHit method: {0}", ex.ToString());
			}
		}
	}

	internal void DoOnSpineAnimEvent(string eventName, int intParam, float floatParam, string stringParam)
	{
		if (IsDestroyed)
		{
			return;
		}
		try
		{
			OnSpineAnimEvent?.Invoke(eventName, intParam, floatParam, stringParam);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Error in OnSpineAnimEvent method: {0}", ex.ToString());
		}
		if (ChildObjectListFront != null)
		{
			for (int num = ChildObjectListFront.Count - 1; num >= 0; num--)
			{
				ChildObjectListFront[num].DoOnSpineAnimEvent(eventName, intParam, floatParam, stringParam);
			}
		}
		if (ChildObjectListBack != null)
		{
			for (int num2 = ChildObjectListBack.Count - 1; num2 >= 0; num2--)
			{
				ChildObjectListBack[num2].DoOnSpineAnimEvent(eventName, intParam, floatParam, stringParam);
			}
		}
	}

	public void UpdateVisibilityRect()
	{
		if (HasEdge && RigidBody != null)
		{
			List<Vector2> edgeVertexList = RigidBody.GetEdgeVertexList();
			BuildVisibilityRectByVertexList(edgeVertexList);
			GameCore.Allocator.DeleteVector2List(edgeVertexList);
			return;
		}
		Vector2[] array;
		if (HasDefinedVisibilityRect)
		{
			array = Utils.GetRectangleVertices(ref PartData.VisibilityRect, WorldTransformation);
		}
		else
		{
			ResetPrecalculations();
			array = GetFrameVerices(bWorld: false);
		}
		int num = (int)Math.Min(array[0].X, Math.Min(array[1].X, Math.Min(array[2].X, array[3].X)));
		int num2 = (int)Math.Max(array[0].X, Math.Max(array[1].X, Math.Max(array[2].X, array[3].X)));
		int num3 = (int)Math.Min(array[0].Y, Math.Min(array[1].Y, Math.Min(array[2].Y, array[3].Y)));
		int num4 = (int)Math.Max(array[0].Y, Math.Max(array[1].Y, Math.Max(array[2].Y, array[3].Y)));
		VisibilityRect = new Rectangle(num, num3, num2 - num, num4 - num3);
		GameCore.Allocator.DeleteRectangleVertices(array);
	}

	public void BuildVisibilityRectByVertexList(List<Vector2> vertexList)
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		foreach (Vector2 vertex in vertexList)
		{
			num = Math.Min(num, vertex.X - WorldTransformation.Position.X);
			num2 = Math.Min(num2, vertex.Y - WorldTransformation.Position.Y);
			num3 = Math.Max(num3, vertex.X - WorldTransformation.Position.X);
			num4 = Math.Max(num4, vertex.Y - WorldTransformation.Position.Y);
		}
		VisibilityRect = new Rectangle((int)num, (int)num2, (int)(num3 - num), (int)(num4 - num2));
	}

	public void Activate()
	{
		if (!IsInLOD)
		{
			if (RigidBody != null)
			{
				DestroyRigidBody();
				_isPhysBodyInitDone = false;
				_isActivated = false;
			}
			return;
		}
		if (!_isActivated)
		{
			_isActivated = true;
			ResetPrecalculations();
			if (IsEnabled && Emitter != null && Emitter.Data.PrewarmRepeatCount > 0 && !Emitter.IsPrewarmed)
			{
				Emitter.Prewarm();
			}
			if (!_isPhysBodyInitDone && PartData != null && PartData.UsePhysData && (GameCore.IsEditorEnabled || GetMainPart().IsTriggerFree || GetMainPart().TriggerPointMode == TriggerPointModeEnum.Normal))
			{
				CreateRigidBodyByData();
			}
			if (RigidBody != null && !RigidBody.IsStatic && (!_wasActivated || GameCore.IsEditorEnabled || GameCore.IgnoreDestroyOnEdge))
			{
				RigidBody.IsEnabled = true;
			}
			if (CurrentState != null)
			{
				PlayStateSound();
			}
			if (!_wasActivated)
			{
				_wasActivated = true;
				if (IsMainPart && (MyLayer == null || MyLayer.LoopX < 0f || Position.X < MyLayer.LoopX + MyLayer.LoopOffsetX))
				{
					AchievementCountTypeEnum achievementKillCountType = AchievementManager.GetAchievementKillCountType(this);
					if (achievementKillCountType != AchievementCountTypeEnum.Unknown)
					{
						GameJnG.AchievementManager.RaiseLevelAvailableCount(achievementKillCountType);
					}
				}
			}
			try
			{
				OnActivate?.Invoke();
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Error in behavior script - OnActivate event: {0}", ex.ToString());
			}
			DoZeroEvents();
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].Activate();
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].Activate();
			}
		}
	}

	public void Deactivate()
	{
		_isActivated = false;
		if (PartData != null && ((PartData.DestroyOutOfAction && !GameCore.IsEditorEnabled) || (!IsLevelObject && RigidBody == null && IsMainPart)))
		{
			Destroy();
			return;
		}
		if (RigidBody != null)
		{
			if (MyLayer != null && MyLayer.ScrollFactor != Vector2.One)
			{
				DestroyRigidBody();
				_isPhysBodyInitDone = false;
			}
			else if ((OutOfView & OutOfFrame.OnLeft) != 0 && !GameCore.IsEditorEnabled)
			{
				RigidBody.IsStatic = true;
			}
		}
		StopSound(bLoopedOnly: true);
		try
		{
			OnDeactivate?.Invoke();
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Error in behavior script - OnDectivate event: {0}", ex.ToString());
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].Deactivate();
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].Deactivate();
			}
		}
	}

	public void SetFrameRectangle(int width, int height, float pivotX, float pivotY)
	{
		_frameRect = new Rectangle((int)(pivotX - (float)(width / 2)), (int)(pivotY - (float)(height / 2)), width, height);
		_framePivot = new Vector2(pivotX, pivotY);
	}

	public GameObject()
	{
		IsDestroyed = false;
		ZOrder = 0;
		Color = Color.White;
		SetFrameRectangle(64, 64, 0f, 0f);
		IsVisible = true;
		Environment = EnvironmentEnum.Air;
		DifficultyRange = new IntRange(0, 999);
	}

	internal void LevelInit()
	{
		IsLevelInitDone = true;
		UpdateVirtualScreenPostion();
		ReRegisterEvents();
		if (IsMainPart)
		{
			UpdateVisibilityRect();
			UpdateOnScreenState();
		}
		else if (RigidBody != null)
		{
			RigidBody.Sleep();
		}
		ResetPrecalculations();
		if (ChildObjectListFront != null)
		{
			ChildObjectListFront.RemoveAll((GameObject go) => go.IsDestroyed);
			foreach (GameObject item in ChildObjectListFront)
			{
				item.LevelInit();
			}
		}
		if (ChildObjectListBack == null)
		{
			return;
		}
		ChildObjectListBack.RemoveAll((GameObject go) => go.IsDestroyed);
		foreach (GameObject item2 in ChildObjectListBack)
		{
			item2.LevelInit();
		}
	}

	internal virtual void Init()
	{
		if (SpineRenderer != null)
		{
			SpineRenderer.Init();
			SpineRenderer.UpdateBegin(0f);
			SpineRenderer.UpdateEnd();
		}
		if (MyPath != null)
		{
			MyPath.CalculateLength();
		}
		SetCurrentTimeLine(MyTimeLine);
		if (PartData != null && PartData.HitPartId != null)
		{
			if (!PartData.HitPartId.Equals("none"))
			{
				if (PartData.HitPartId.Equals("parent"))
				{
					HitPart = ParentObject;
				}
				else if (PartData.HitPartId.Equals("spine"))
				{
					HitPart = GetParentSpineRendererPart();
					if (HitPart == null)
					{
						HitPart = this;
					}
				}
				else
				{
					HitPart = GetMainPart().GetChildByIdDeep(PartData.HitPartId);
				}
			}
		}
		else
		{
			HitPart = GetMainPart();
		}
		if (HitPart == this)
		{
			HitPart = null;
		}
		if (_behaviorList != null)
		{
			for (int num = _behaviorList.Count - 1; num >= 0; num--)
			{
				BehaviorScript behaviorScript = null;
				try
				{
					behaviorScript = _behaviorList[num];
					behaviorScript.Init();
				}
				catch (Exception ex)
				{
					if (behaviorScript != null && behaviorScript.BehaviorData != null)
					{
						GameCore.LogErr("Error in behavior class {0} - Init method: {1}", behaviorScript.ClassName, ex.ToString());
					}
				}
			}
			_behaviorList.RemoveAll((BehaviorScript script) => script.IsDone);
		}
		if (Motion != null && PartData != null)
		{
			Motion.Damping = PartData.PhysLinearDamping * 100f;
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int num2 = 0; num2 < count; num2++)
			{
				ChildObjectListFront[num2].Init();
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int num3 = 0; num3 < count2; num3++)
			{
				ChildObjectListBack[num3].Init();
			}
		}
		if (IsMainPart)
		{
			UpdateVisibilityRect();
		}
		if ((TargetClass & TargetClass.AllHero) == 0 && (TargetClass & TargetClass.Bonus) == 0)
		{
			float hPFactor = FactorManager.HPFactor;
			MaxHP *= hPFactor;
			CurrentHP *= hPFactor;
		}
		if (PartData != null && PartData.UseDamageFactor)
		{
			Damage *= FactorManager.DamageFactor;
		}
		IsInitDone = true;
	}

	internal void ClearResources()
	{
		ChildObjectListFront = GameCore.Allocator.DeleteOwningGameObjectList(ChildObjectListFront);
		ChildObjectListBack = GameCore.Allocator.DeleteOwningGameObjectList(ChildObjectListBack);
		LinkedObjectList = GameCore.Allocator.DeleteAliasingGameObjectList(LinkedObjectList);
		ParticleListFront = GameCore.Allocator.DeleteOwningParticleList(ParticleListFront);
		ParticleListBack = GameCore.Allocator.DeleteOwningParticleList(ParticleListBack);
		GameCore.Allocator.DeletePolygonRenderer(PolygonRenderer);
		PolygonRenderer = null;
		GameCore.Allocator.DeleteSpriteRenderer(SpriteRenderer);
		SpriteRenderer = null;
		_stripLineListBack = GameCore.Allocator.DeleteOwningStripLineList(_stripLineListBack);
		_stripLineListFront = GameCore.Allocator.DeleteOwningStripLineList(_stripLineListFront);
	}

	internal void ClearGameObject()
	{
		ClearResources();
		Pk = 0L;
		CreateTime = 0f;
		ZOrder = 0;
		MyLayer = null;
		TriggerPoint = Vector2.Zero;
		WasTriggerPointActivated = false;
		OldRotation = 0f;
		OldPosition = Vector2.Zero;
		_velocity = Vector2.Zero;
		Motion = null;
		Environment = EnvironmentEnum.Air;
		DestroyOutOfView = OutOfFrame.None;
		DestroyOutOfLevel = OutOfFrame.None;
		MyPath = null;
		Emitter = null;
		LifeTime = float.MaxValue;
		Timer = 0f;
		HitEffectTimer = 0f;
		KillTimer = float.MaxValue;
		PathSpeed = 0f;
		IgnoreCollisionMode = 0;
		TriggerPointMode = TriggerPointModeEnum.Normal;
		PathFollowMode = PathFollowModeEnum.FreeRotation;
		SpeedKeyValue = null;
		AngleKeyValue = null;
		PathOscilation = Vector2.Zero;
		CreatePhysBody = true;
		MyTimeLine = null;
		MyItem = null;
		SpineConnector = null;
		SpineRenderer = null;
		BehaviorData = null;
		TargetPart = null;
		DifficultyRange = new IntRange(0, 999);
		GameModeList = null;
		ForPlayerCount = 0;
		MaxHP = 0f;
		CurrentHP = 0f;
		Damage = 0f;
		DamageType = DamageTypeEnum.Unknown;
		LastDamageType = DamageTypeEnum.Unknown;
		LastHittingObject = null;
		HitOnCollision = true;
		ShareHits = true;
		IsImmortal = false;
		ScreenPosition = Vector2.Zero;
		VirtualScreenPosition = Vector2.Zero;
		TargetVirtualScreenPosition = Vector2.Zero;
		RemconText = null;
		RemconedByHero = null;
		RemconedCancelByHero = null;
		IsRemconable = false;
		IsLevelObject = false;
		IsInitDone = false;
		IsLevelInitDone = false;
		IsPathPaused = false;
		IsInLevelStats = false;
		HitPart = null;
		_sendedLinkCount = 0;
		OnLevelBegin = null;
		OnTrigger = null;
		OnLink = null;
		OnDestroy = null;
		OnDisable = null;
		OnUnregisterEvents = null;
		OnKill = null;
		OnUpdateBegin = null;
		OnUpdate = null;
		OnUpdateEnd = null;
		OnDraw = null;
		OnEditorUpdate = null;
		OnReRegisterEvents = null;
		OnActivate = null;
		OnDeactivate = null;
		OnEditorUpdateBegin = null;
		OnEditorUpdateEnd = null;
		OnHitBegin = null;
		OnHit = null;
		OnShockHit = null;
		OnBeforeCollision = null;
		OnCollision = null;
		OnAfterCollision = null;
		OnSeparation = null;
		OnPathEnd = null;
		OnPathChanged = null;
		OnSpineAnimStart = null;
		OnSpineAnimEnd = null;
		OnSpineAnimEvent = null;
		OnSpineAnimCompleted = null;
		OnAnimEnd = null;
		OnEnvironmentChanged = null;
		OnTimeLineChanged = null;
		CurrentState = null;
		LastState = null;
		PartStateId = null;
		PartData = null;
		CurrAtlasAnimId = null;
		ChildZOder = 0;
		HasEdge = false;
		IsKilled = false;
		IsDestroyed = false;
		IsDetached = false;
		ResetSpineTrackOnDestroy = true;
		HasScreenRelativePath = false;
		SetFrameRectangle(64, 64, 0f, 0f);
		_isPhysBodyInitDone = false;
		_wasStateChanged = false;
		_isUpdateInEditorEnabled = false;
		_isEnabled = true;
		_isVisible = true;
		MyHero = null;
		IsHero = false;
		_mainPart = null;
		DestroyOnCollision = false;
		DestroyOnEndPath = 0;
		_destroyOnKill = false;
		_physClass = PhysClass.None;
		TargetClass = TargetClass.None;
		VisibilityRect = Rectangle.Empty;
		_wasActivated = false;
		_isActivated = false;
		ActiveMode = ActiveModeEnum.Normal;
		IsOnScreenX = false;
		OutOfView = OutOfFrame.None;
		OutOfLevel = OutOfFrame.None;
		_parentObject = null;
		_behaviorList = null;
		_soundList = null;
		PathTime = 0f;
		TimeLineTime = 0f;
		CurrentTimeLine = null;
		_currentTimeNodeIndex = 0;
		_currentPathEventNodeIndex = 0;
	}

	public void DoOnLevelBegin()
	{
		_wasActivated = false;
		_currentPathEventNodeIndex = 0;
		if (ActiveMode == ActiveModeEnum.AlwaysActive || (RigidBody != null && RigidBody.HasJoints()))
		{
			Activate();
		}
		if (RigidBody != null && IsLevelObject)
		{
			RigidBody.ResetDynamics();
		}
		try
		{
			OnLevelBegin?.Invoke();
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnTimeLineChanged script exception: {0} in object '{1}'", ex, Id);
		}
	}

	public void SetCurrentTimeLine(BehaviorTimeLine timeLine)
	{
		if (timeLine != CurrentTimeLine)
		{
			CurrentTimeLine = timeLine;
			TimeLineTime = 0f;
			_currentTimeNodeIndex = 0;
			try
			{
				OnTimeLineChanged?.Invoke();
			}
			catch (Exception ex)
			{
				GameCore.LogErr("OnTimeLineChanged script exception: {0} in object '{1}'", ex, Id);
			}
		}
	}

	private void UpdateTimeLine(float elapsedTime)
	{
		if (CurrentTimeLine.EventNodeList != null)
		{
			for (int i = _currentTimeNodeIndex; i < CurrentTimeLine.EventNodeList.Count; i++)
			{
				BehaviorEventNode behaviorEventNode = CurrentTimeLine.EventNodeList[i];
				if (behaviorEventNode.Time <= TimeLineTime)
				{
					behaviorEventNode.Execute(CurrentTimeLine);
					_currentTimeNodeIndex = i + 1;
				}
			}
		}
		if (CurrentTimeLine.EventNodeList == null || _currentTimeNodeIndex >= CurrentTimeLine.EventNodeList.Count)
		{
			SetCurrentTimeLine(null);
		}
		else if (IsTriggerFree)
		{
			TimeLineTime += elapsedTime;
		}
	}

	public GameObject SwitchToObject(string id, bool bAssumeChildren, Vector2 offset, SwitchObjectKillModeEnum killMode = SwitchObjectKillModeEnum.None)
	{
		PartData partData = GameCore.GetPartData(id);
		if (partData != null)
		{
			return SwitchToObject(partData, bAssumeChildren, offset, killMode);
		}
		return null;
	}

	public GameObject SwitchToObject(PartData partData, bool bAssumeChildren, Vector2 offset, SwitchObjectKillModeEnum killMode = SwitchObjectKillModeEnum.None)
	{
		Vector2 pos = Vector2.Transform(offset, WorldMatrix);
		GameObject gameObject = MyLayer.CreateObjectByPartData(partData, pos, WorldScale, WorldRotation, ZOrder);
		if (gameObject != null)
		{
			SwitchSharedChildren(gameObject);
			gameObject.SetVelocity(Velocity);
		}
		switch (killMode)
		{
		case SwitchObjectKillModeEnum.Kill:
			Kill();
			break;
		case SwitchObjectKillModeEnum.Destroy:
			Destroy();
			break;
		}
		return gameObject;
	}

	public void AddChildRelative(GameObject childObject, float offsetX = 0f, float offsetY = 0f, int zorder = 0)
	{
		if (childObject == null)
		{
			GameCore.LogErr("Unable to add child to object {0} - Child is null", Id);
			return;
		}
		if (childObject == this)
		{
			GameCore.LogErr("Unable to add child as myself {0} at position {1} in layer {2}", Id, WorldPosition, MyLayer?.Id);
			return;
		}
		if (childObject.ParentObject != null)
		{
			childObject.ParentObject.RemoveChild(childObject);
		}
		childObject.SetLayer(MyLayer);
		Vector2 position = Vector2.Transform(new Vector2(offsetX, offsetY), Matrix.CreateRotationZ(0f - Rotation) * Matrix.CreateScale(1f / Scale.X, 1f / Scale.Y, 1f));
		childObject.SetPosition(position);
		childObject.SetRotation(childObject.Rotation - Rotation);
		childObject.SetScale(childObject.Scale / Scale);
		childObject.ParentObject = this;
		childObject.ChildZOder = zorder;
		if (zorder >= 0)
		{
			if (ChildObjectListFront == null)
			{
				ChildObjectListFront = GameCore.Allocator.GetGameObjectList();
			}
			ChildObjectListFront.Add(childObject);
		}
		else
		{
			if (ChildObjectListBack == null)
			{
				ChildObjectListBack = GameCore.Allocator.GetGameObjectList();
			}
			ChildObjectListBack.Add(childObject);
		}
	}

	public void AddChild(GameObject childObject, int zorder = 0)
	{
		if (childObject == null)
		{
			GameCore.LogErr("Unable to add child to object {0} - Child is null", Id);
			return;
		}
		if (childObject.ParentObject != null)
		{
			childObject.ParentObject.RemoveChildImediately(childObject);
		}
		childObject.SetLayer(MyLayer);
		childObject.ParentObject = this;
		childObject.ChildZOder = zorder;
		childObject.IsDetached = false;
		if (zorder >= 0)
		{
			if (ChildObjectListFront == null)
			{
				ChildObjectListFront = GameCore.Allocator.GetGameObjectList();
			}
			ChildObjectListFront.Add(childObject);
		}
		else
		{
			if (ChildObjectListBack == null)
			{
				ChildObjectListBack = GameCore.Allocator.GetGameObjectList();
			}
			ChildObjectListBack.Add(childObject);
		}
	}

	public List<GameObject> GetPartList()
	{
		List<GameObject> gameObjectList = GameCore.Allocator.GetGameObjectList();
		AddPartsToList(this, gameObjectList);
		return gameObjectList;
	}

	private static void AddPartsToList(GameObject go, List<GameObject> partList)
	{
		partList.Add(go);
		if (go.ChildObjectListFront != null)
		{
			int count = go.ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				AddPartsToList(go.ChildObjectListFront[i], partList);
			}
		}
		if (go.ChildObjectListBack != null)
		{
			int count2 = go.ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				AddPartsToList(go.ChildObjectListBack[j], partList);
			}
		}
	}

	public GameObject GetChildByIdFlat(string childId)
	{
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				if (ChildObjectListFront[i].Id.Equals(childId, StringComparison.OrdinalIgnoreCase))
				{
					return ChildObjectListFront[i];
				}
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				if (ChildObjectListBack[j].Id.Equals(childId, StringComparison.OrdinalIgnoreCase))
				{
					return ChildObjectListBack[j];
				}
			}
		}
		return null;
	}

	public GameObject GetChildByPartDataFlat(PartData partData)
	{
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				if (ChildObjectListFront[i].PartData == partData)
				{
					return ChildObjectListFront[i];
				}
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				if (ChildObjectListBack[j].PartData == partData)
				{
					return ChildObjectListBack[j];
				}
			}
		}
		return null;
	}

	public GameObject GetChildByIdDeep(string childId)
	{
		if (Utils.Equals(Id, childId) || (IsMainPart && Utils.Equals(childId, "root")))
		{
			return this;
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject childByIdDeep = ChildObjectListFront[i].GetChildByIdDeep(childId);
				if (childByIdDeep != null)
				{
					return childByIdDeep;
				}
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				GameObject childByIdDeep2 = ChildObjectListBack[j].GetChildByIdDeep(childId);
				if (childByIdDeep2 != null)
				{
					return childByIdDeep2;
				}
			}
		}
		return null;
	}

	public void ChangeChildZOrder(GameObject childObject, int zorder)
	{
		if (zorder < 0)
		{
			if (ChildObjectListFront != null && ChildObjectListFront.Contains(childObject))
			{
				ChildObjectListFront.Remove(childObject);
				if (ChildObjectListBack == null)
				{
					ChildObjectListBack = GameCore.Allocator.GetGameObjectList();
				}
				ChildObjectListBack.Insert(0, childObject);
				childObject.ChildZOder = zorder;
			}
		}
		else if (ChildObjectListBack != null && ChildObjectListBack.Contains(childObject))
		{
			ChildObjectListBack.Remove(childObject);
			if (ChildObjectListFront == null)
			{
				ChildObjectListFront = GameCore.Allocator.GetGameObjectList();
			}
			ChildObjectListFront.Insert(0, childObject);
			childObject.ChildZOder = zorder;
		}
	}

	public GameObject CreateChildByPartData(string partDataId, bool createPhysBody)
	{
		return CreateChildByPartData(GameCore.GetPartData(partDataId), createPhysBody);
	}

	public GameObject CreateChildByPartData(PartData data, bool createPhysBody)
	{
		GameObject gameObject = CreateByPartDataSimple(data, createPhysBody, this);
		if (gameObject != null)
		{
			if (data.DefaultPartState.ZOrder >= 0)
			{
				if (ChildObjectListFront == null)
				{
					ChildObjectListFront = GameCore.Allocator.GetGameObjectList();
				}
				ChildObjectListFront.Add(gameObject);
			}
			else
			{
				if (ChildObjectListBack == null)
				{
					ChildObjectListBack = GameCore.Allocator.GetGameObjectList();
				}
				ChildObjectListBack.Add(gameObject);
			}
			gameObject.ChildZOder = data.DefaultPartState.ZOrder;
			if (!_destroyOnKill)
			{
				gameObject.SetDestroyOnKill(_destroyOnKill, bOnlyUndefined: true);
			}
			if (gameObject.PhysClass == PhysClass.Undefined)
			{
				gameObject.PhysClass = GetMainPart().PhysClass;
			}
			if (gameObject.TargetClass == TargetClass.Undefined)
			{
				gameObject.TargetClass = GetMainPart().TargetClass;
			}
			if (gameObject.PartData.IsTargetPart)
			{
				GetMainPart().TargetPart = gameObject;
			}
			gameObject.SetEnvironment(Environment);
		}
		return gameObject;
	}

	public void RemoveChild(GameObject go)
	{
		go.ParentObject = null;
		if (ChildObjectListFront != null && ChildObjectListFront.Contains(go))
		{
			go.IsDetached = true;
		}
		if (ChildObjectListBack != null && ChildObjectListBack.Contains(go))
		{
			go.IsDetached = true;
		}
		go.SpineConnector = null;
	}

	public void RemoveChildImediately(GameObject go)
	{
		go.ParentObject = null;
		if (ChildObjectListFront != null && ChildObjectListFront.Contains(go))
		{
			ChildObjectListFront.Remove(go);
		}
		if (ChildObjectListBack != null && ChildObjectListBack.Contains(go))
		{
			ChildObjectListBack.Remove(go);
		}
		go.SpineConnector = null;
	}

	public void DestroySameChildren(GameObject go)
	{
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = ChildObjectListFront[i];
				if (gameObject != go && gameObject.PartData == go.PartData)
				{
					gameObject.Destroy();
				}
				gameObject.DestroySameChildren(go);
			}
		}
		if (ChildObjectListBack == null)
		{
			return;
		}
		int count2 = ChildObjectListBack.Count;
		for (int j = 0; j < count2; j++)
		{
			GameObject gameObject2 = ChildObjectListBack[j];
			if (gameObject2 != go && gameObject2.PartData == go.PartData)
			{
				gameObject2.Destroy();
			}
			gameObject2.DestroySameChildren(go);
		}
	}

	public void AddLinkedObject(GameObject go)
	{
		if (LinkedObjectList == null)
		{
			LinkedObjectList = GameCore.Allocator.GetGameObjectList();
		}
		if (!LinkedObjectList.Contains(go))
		{
			LinkedObjectList.Add(go);
		}
	}

	public void RemoveLinkedObject(GameObject go)
	{
		if (LinkedObjectList == null)
		{
			return;
		}
		int count = LinkedObjectList.Count;
		for (int i = 0; i < count; i++)
		{
			if (LinkedObjectList[i] == go)
			{
				LinkedObjectList.RemoveAt(i);
				break;
			}
		}
	}

	public SpineConnector ConnectObjectToSpineSlot(GameObject go, string slotName)
	{
		if (SpineRenderer == null)
		{
			GameCore.LogErr("Unable to connect object '{2}' to spine slot '{0}' - Spine renderer not created for object '{1}'.", slotName, Id, go.Id);
			return null;
		}
		go.SpineConnector = new SpineConnector(go, this);
		if (go.SpineConnector.ConnectToSlot(slotName) < 0)
		{
			GameCore.LogErr("Spine slot '{0}' not found in object '{1}'", slotName, Id);
		}
		return go.SpineConnector;
	}

	public SpineConnector ConnectObjectToSpineBone(GameObject go, string boneName)
	{
		go.SpineConnector = new SpineConnector(go, this);
		if (go.SpineConnector.ConnectToBone(boneName) == null)
		{
			GameCore.LogErr("Spine bone '{0}' not found in object '{1}'", boneName, Id);
		}
		return go.SpineConnector;
	}

	public void DefaultSprite(string atlasId, int frame = 0)
	{
		CreateSpriteRenderer(atlasId);
		Frame = frame;
	}

	public void DestroySpineRenderer()
	{
		SpineRenderer = null;
		SpineConnector = null;
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].SpineConnector = null;
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].SpineConnector = null;
			}
		}
	}

	public void DestroySpriteRenderer()
	{
		GameCore.Allocator.DeleteSpriteRenderer(SpriteRenderer);
		SpriteRenderer = null;
	}

	public void AddAvailableEventType(GameObject go, string name, BehaviorEventTypeEnum type)
	{
		if (PartData.AvailableEventTypes == null)
		{
			PartData.AvailableEventTypes = new Dictionary<string, BehaviorEventType>();
		}
		string text = ((!go.IsMainPart) ? $"{go.Id}/{name}" : name);
		if (!PartData.AvailableEventTypes.ContainsKey(text))
		{
			PartData.AvailableEventTypes.Add(text, new BehaviorEventType(text, type));
		}
	}

	public void SetPhysicsTransformation(Transformation transformation)
	{
		SetPosition(transformation.Position);
		SetRotation(transformation.Rotation);
		SetScale(transformation.Scale);
	}

	public void SetPhysicsTransformationInt(Transformation transformation)
	{
		SetPosition(new Vector2((int)transformation.Position.X, (int)transformation.Position.Y));
		SetRotation(transformation.Rotation);
		SetScale(transformation.Scale);
	}

	public ISound2D PlaySound(string id, int volume = 100, bool looped = false)
	{
		if (GameCore.IsShopMenuEnabled || string.IsNullOrEmpty(id))
		{
			return null;
		}
		if (Utils.Equals(id, "none"))
		{
			return null;
		}
		float volumePercentageByVirtualScreePosition = SoundUtils.GetVolumePercentageByVirtualScreePosition(VirtualScreenPosition);
		float paningByVirtualScreePosition = SoundUtils.GetPaningByVirtualScreePosition(VirtualScreenPosition);
		ISound2D sound2D = GameCore.PlaySound(id, (float)volume * volumePercentageByVirtualScreePosition, paningByVirtualScreePosition, looped);
		if (sound2D != null)
		{
			sound2D.StartVolume = volume;
			if (!sound2D.IsLooped)
			{
				PartData partData = PartData;
				if (partData != null && partData.SoundUpdateMode == 0)
				{
					goto IL_008e;
				}
			}
			if (_soundList == null)
			{
				_soundList = new List<ISound2D>();
			}
			_soundList.Add(sound2D);
		}
		goto IL_008e;
		IL_008e:
		return sound2D;
	}

	private void PlayStateSound()
	{
		if (CurrentState.UseSound && !CurrentState.SoundIdArray.IsNullOrEmpty() && IsActivated && (PartData.PlaySoundInEditor || !GameCore.IsEditorEnabled))
		{
			StopSound(bLoopedOnly: false);
			string randomItem = Utils.GetRandomItem(CurrentState.SoundIdArray);
			if (PartData.SoundUpdateMode == 9)
			{
				GameCore.PlaySound(randomItem, 100f, 0f, CurrentState.IsSoundLooped);
			}
			else
			{
				PlaySound(randomItem, 100, CurrentState.IsSoundLooped);
			}
		}
	}

	public void UpdateSound()
	{
		for (int num = _soundList.Count - 1; num >= 0; num--)
		{
			ISound2D sound2D = _soundList[num];
			if (sound2D != null)
			{
				if (sound2D.IsFinished)
				{
					_soundList.RemoveAt(num);
				}
				else
				{
					sound2D.SetPan(SoundUtils.GetPaningByVirtualScreePosition(VirtualScreenPosition));
					PartData partData = PartData;
					if (partData == null || partData.SoundUpdateMode != 2)
					{
						sound2D.SetVolume(sound2D.StartVolume * SoundUtils.GetVolumePercentageByVirtualScreePosition(VirtualScreenPosition));
					}
					else
					{
						sound2D.SetVolume(sound2D.StartVolume);
					}
				}
			}
		}
	}

	public void StopSound(bool bLoopedOnly)
	{
		if (_soundList == null)
		{
			return;
		}
		foreach (ISound2D sound in _soundList)
		{
			if (sound != null && (!bLoopedOnly || sound.IsLooped || PartData == null || PartData.SoundUpdateMode != 0))
			{
				sound.Stop();
			}
		}
		_soundList = null;
	}

	public bool CreatePolygonRenderer(PolygonData data)
	{
		try
		{
			PolygonRenderer = GameCore.Allocator.GetPolygonRenderer(data, this);
			PolygonRenderer.SetVertices(data.VertextArray ?? data.GetDefaultVertices());
		}
		catch (Exception arg)
		{
			GameCore.LogErr($"Unable to create PolygonRenderer for object [{Id}]: {arg}");
			return false;
		}
		return true;
	}

	public SpineRenderer CreateSpineRenderer(SpineData skeleton)
	{
		try
		{
			SpineRenderer = new SpineRenderer(this, skeleton);
		}
		catch (Exception arg)
		{
			GameCore.LogErr($"Unable to create SpineRenderer for object [{Id}]: {arg}");
		}
		return SpineRenderer;
	}

	public SpineRenderer CreateSpineRenderer(string skeletonId)
	{
		SpineData spineData = GameCore.GetSpineData(skeletonId);
		if (spineData == null)
		{
			GameCore.LogErr("Unable to find Spine skeleton '{0}' for SpriteRenderer in object '{1}'.", skeletonId, Id);
			return null;
		}
		return CreateSpineRenderer(spineData);
	}

	internal SpineRenderer GetCurrentSpineRenderer()
	{
		if (SpineRenderer != null)
		{
			return SpineRenderer;
		}
		if (SpineConnector != null)
		{
			return SpineConnector.SpineRenderer;
		}
		if (ParentObject != null)
		{
			return ParentObject.GetCurrentSpineRenderer();
		}
		return null;
	}

	internal GameObject GetParentSpineRendererPart()
	{
		if (SpineRenderer != null)
		{
			return this;
		}
		if (ParentObject != null)
		{
			return ParentObject.GetParentSpineRendererPart();
		}
		return null;
	}

	public void LockSpineAnim(bool isLocked)
	{
		SpineRenderer currentSpineRenderer = GetCurrentSpineRenderer();
		if (currentSpineRenderer != null)
		{
			currentSpineRenderer.IsAnimLocked = isLocked;
		}
	}

	public void ResetSpineAnim()
	{
		GetCurrentSpineRenderer()?.ResetTracks();
	}

	public void ResetSpineTrack(int trackNumber)
	{
		GetCurrentSpineRenderer()?.ResetAnimation(trackNumber);
	}

	public bool SetSpineAnimation(int track, string animName, bool bLooped)
	{
		SpineRenderer currentSpineRenderer = GetCurrentSpineRenderer();
		if (currentSpineRenderer != null)
		{
			if (animName == null)
			{
				currentSpineRenderer.ResetAnimation(track);
			}
			else
			{
				Animation animation = currentSpineRenderer.FindAnimation(animName);
				if (animation != null)
				{
					currentSpineRenderer.SetAnimation(track, animation, bLooped);
					return true;
				}
			}
		}
		return false;
	}

	public void CreateAnimator(string atlasFramesId, float duration, bool isLooped)
	{
		if (SpriteRenderer != null && SpriteRenderer.Atlas.AnimFrames != null)
		{
			int[] value = null;
			SpriteRenderer.Atlas.AnimFrames.TryGetValue(atlasFramesId, out value);
			if (value != null)
			{
				SpriteRenderer.CreateAnimator(value, duration, isLooped);
			}
			CurrAtlasAnimId = atlasFramesId;
		}
		else
		{
			GameCore.LogErr("Unable to create atlas animator '{0}' for object id '{1}'.", atlasFramesId, Id);
			CurrAtlasAnimId = null;
		}
	}

	public SpriteRenderer CreateSpriteRenderer(SpriteAtlas atlas, string skinSuffix = null)
	{
		SpriteRenderer = GameCore.Allocator.GetSpriteRenderer(atlas, skinSuffix);
		return SpriteRenderer;
	}

	public SpriteRenderer CreateSpriteRenderer(string atlasId, string skinSuffix = null)
	{
		SpriteAtlas spriteAtlas = GameCore.GetSpriteAtlas(atlasId);
		if (spriteAtlas == null)
		{
			GameCore.LogErr("Unable to find SpriteAtlas '{0}' for SpriteRenderer in object '{1}'.", atlasId, Id);
			return null;
		}
		return CreateSpriteRenderer(spriteAtlas, skinSuffix);
	}

	public Texture2D GetTexture()
	{
		if (SpriteRenderer != null)
		{
			return SpriteRenderer.Texture;
		}
		return null;
	}

	public void SetTexture(Texture2D texture)
	{
		if (SpriteRenderer != null)
		{
			SpriteRenderer.Texture = texture;
		}
	}

	public void AddParticle(Particle particle, bool bAddToBack, SortModeEnum sortMode)
	{
		if (bAddToBack)
		{
			if (ParticleListBack == null)
			{
				ParticleListBack = GameCore.Allocator.GetParticleList();
			}
			if (sortMode == SortModeEnum.AsFirst || (sortMode == SortModeEnum.Random && Utils.GetRandom(1) == 1))
			{
				ParticleListBack.Insert(0, particle);
			}
			else
			{
				ParticleListBack.Add(particle);
			}
		}
		else
		{
			if (ParticleListFront == null)
			{
				ParticleListFront = GameCore.Allocator.GetParticleList();
			}
			if (sortMode == SortModeEnum.AsFirst || (sortMode == SortModeEnum.Random && Utils.GetRandom(1) == 1))
			{
				ParticleListFront.Insert(0, particle);
			}
			else
			{
				ParticleListFront.Add(particle);
			}
		}
	}

	public void AddStripLine(StripLine sl, ParticleSpace space)
	{
		if (space == ParticleSpace.MyPartBack || space == ParticleSpace.MainPartBack)
		{
			if (_stripLineListBack == null)
			{
				_stripLineListBack = GameCore.Allocator.GetStripLineList();
			}
			_stripLineListBack.Add(sl);
		}
		else
		{
			if (_stripLineListFront == null)
			{
				_stripLineListFront = GameCore.Allocator.GetStripLineList();
			}
			_stripLineListFront.Add(sl);
		}
	}

	public ParticleEmitter CreateEmitter(string dataId)
	{
		ParticleEmitterData particleEmitterData = GameCore.GetParticleEmitterData(dataId);
		if (particleEmitterData == null)
		{
			GameCore.LogErr($"Unable to find EmitterData '{dataId}' for emitter in object '{Id}'.");
		}
		ParticleEmitter particleEmitter = CreateEmitter(particleEmitterData);
		if (particleEmitter != null && PartData.EmitterParticleSpace != ParticleSpace.Undefined)
		{
			particleEmitter.Space = PartData.EmitterParticleSpace;
		}
		return particleEmitter;
	}

	public ParticleEmitter CreateEmitter(ParticleEmitterData emitterData)
	{
		Emitter = new ParticleEmitter(this, emitterData);
		return Emitter;
	}

	public RigidBody CreateRigidBody()
	{
		Transformation worldTransformation = WorldTransformation;
		RigidBody = new RigidBody(worldTransformation.Position, this);
		RigidBody.ChangeScale(Scale);
		RigidBody.Rotation = Rotation;
		RigidBody.Sleep();
		return RigidBody;
	}

	public void CreateRigidBodyByData()
	{
		DestroyRigidBody();
		_isPhysBodyInitDone = true;
		if (!CreatePhysBody || !CurrentState.PhysEnabled || PartData == null || MyLayer == null || !MyLayer.UsePhysics || !PartData.UsePhysData)
		{
			return;
		}
		if (RigidBody == null)
		{
			Transformation worldTransformation = WorldTransformation;
			RigidBody = new RigidBody(worldTransformation.Position, this);
		}
		if (PartData.PhysType == 1)
		{
			RigidBody.BodyType = BodyType.Dynamic;
		}
		RigidBody.Mass = PartData.PhysMass;
		RigidBody.Friction = PartData.PhysFriction;
		RigidBody.Restitution = PartData.PhysRestitution;
		RigidBody.LinearDamping = PartData.PhysLinearDamping;
		RigidBody.AngularDamping = PartData.PhysAngularDamping;
		RigidBody.FixedRotation = PartData.PhysFixedRotation;
		RigidBody.IsSensor = PartData.PhysIsSensor;
		RigidBody.IsBullet = PartData.IsBullet;
		RigidBody.SleepingAllowed = PartData.PhysSleepingAllowed;
		if (PartData.PhysOnCollisionEnabled < 0)
		{
			RigidBody.OnCollisionEnabled = _behaviorList != null;
		}
		else
		{
			RigidBody.OnCollisionEnabled = PartData.PhysOnCollisionEnabled != 0;
		}
		if (PartData.PhysGravityScale == 0f)
		{
			RigidBody.IgnoreGravity = true;
		}
		else
		{
			RigidBody.GravityScale = PartData.PhysGravityScale;
		}
		if (PartData.PhysRectangleList != null)
		{
			foreach (Vector4 physRectangle in PartData.PhysRectangleList)
			{
				RigidBody.AddRectangle(physRectangle.Z, physRectangle.W, physRectangle.X, physRectangle.Y);
			}
		}
		if (PartData.PhysCircleList != null)
		{
			foreach (Vector3 physCircle in PartData.PhysCircleList)
			{
				RigidBody.AddCircle(physCircle.Z, physCircle.X, physCircle.Y);
			}
		}
		if (PartData.PhysPolygonList != null)
		{
			foreach (float[] physPolygon in PartData.PhysPolygonList)
			{
				RigidBody.AddPolygon(physPolygon, Matrix.Identity);
			}
		}
		RigidBody.LocalCenter = PartData.PhysLocalCenter;
		if (!PartData.PhysFixedScale)
		{
			RigidBody.ChangeScale(Scale);
		}
		RigidBody.Rotation = Rotation;
		if (RigidBody.PhysBody != null)
		{
			if (RigidBody.SleepingAllowed)
			{
				RigidBody.Sleep();
			}
			RigidBody.SetPhysClass(PhysClass);
		}
	}

	public BezierPath CreatePath()
	{
		MyPath = new BezierPath(Id + " path", this);
		MyPath.AddPoint(Position);
		return MyPath;
	}

	public void ChangePath(BezierPath path)
	{
		MyPath = path;
		_currentPathEventNodeIndex = 0;
		OnPathChanged?.Invoke();
	}

	public BehaviorScript AddBehaviorScript(BehaviorData data, bool bAsFirst = false)
	{
		BehaviorScript behaviorScript = BehaviorScript.CreateFromBehaviorData(this, data);
		if (behaviorScript != null)
		{
			if (_behaviorList == null)
			{
				_behaviorList = new List<BehaviorScript>();
			}
			if (bAsFirst)
			{
				_behaviorList.Insert(0, behaviorScript);
			}
			else
			{
				_behaviorList.Add(behaviorScript);
			}
		}
		return behaviorScript;
	}

	public void ApplyForce(Vector2 force)
	{
		if (RigidBody != null)
		{
			RigidBody.ApplyForce(force);
		}
	}

	public void ApplyForce(float x, float y)
	{
		ApplyForce(new Vector2(x, y));
	}

	public void ApplyLinearImpulse(float x, float y)
	{
		if (RigidBody != null)
		{
			RigidBody.ApplyLinearImpulse(x, y);
		}
	}

	public void ApplyLinearImpulse(Vector2 impulse)
	{
		if (RigidBody != null)
		{
			RigidBody.ApplyLinearImpulse(impulse);
		}
	}

	public void ApplyAngularImpulse(float impulse)
	{
		if (RigidBody != null)
		{
			RigidBody.ApplyAngularImpulse(impulse);
		}
	}

	public void SetAcceleration(Vector2 acceleration)
	{
		if (IsMovable)
		{
			Motion.Acceleration = acceleration;
		}
	}

	public void SetAlphaPercent(float alphaPercent)
	{
		Alpha = (byte)(alphaPercent * 255f);
	}

	public void SetFadeAlpha(float alphaFactor, bool bAffectChildren)
	{
		Alpha = (byte)((float)(int)CurrentState.Color.A * alphaFactor);
		if (!bAffectChildren)
		{
			return;
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].SetFadeAlpha(alphaFactor, bAffectChildren: true);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].SetFadeAlpha(alphaFactor, bAffectChildren: true);
			}
		}
	}

	public void SetRotationSpeedInDeg(float speed)
	{
		RotationSpeed = Utils.DegToRad(speed);
	}

	public void SetVelocity(float x, float y)
	{
		SetVelocity(new Vector2(x, y));
	}

	public void SetVelocity(Vector2 velocity)
	{
		if (velocity.X == float.NaN)
		{
			velocity = Vector2.Zero;
		}
		if (RigidBody != null && !RigidBody.IsStatic)
		{
			RigidBody.Velocity = ConvertUnits.ToSimUnits(velocity);
		}
		else if (IsMovable)
		{
			Motion.Velocity = velocity;
		}
		_velocity = velocity;
	}

	public void SetRotationVelocity(float speed, float rotation, Vector2 forceVelocity)
	{
		SetVelocity(Utils.GetVelocityVector2(speed, rotation) + forceVelocity);
	}

	public void SetRotationVelocity(float velocity, float rotation)
	{
		SetRotationVelocity(velocity, rotation, Vector2.Zero);
	}

	public float GetPathRotation()
	{
		if (MyPath != null)
		{
			return MyPath.GetDirection(PathTime);
		}
		return 0f;
	}

	public BehaviorEvent GetNextPathEvent(float time, string eventName)
	{
		GameObject mainPart = GetMainPart();
		if (mainPart.MyPath != null)
		{
			foreach (BehaviorEventNode item in mainPart.MyPath.EventNodeList.FindAll((BehaviorEventNode o) => o.Time > time && o.EventList.Count > 0))
			{
				BehaviorEvent behaviorEvent = item.EventList.Find((BehaviorEvent o) => o.MyObject == this && Utils.Equals(o.Name, eventName));
				if (behaviorEvent != null)
				{
					return behaviorEvent;
				}
			}
		}
		return null;
	}

	public BehaviorEvent GetPathEventForTime(float time, string eventName)
	{
		GameObject mainPart = GetMainPart();
		if (mainPart.MyPath != null)
		{
			foreach (BehaviorEventNode item in mainPart.MyPath.EventNodeList.FindAll((BehaviorEventNode o) => o.Time == time && o.EventList.Count > 0))
			{
				BehaviorEvent behaviorEvent = item.EventList.Find((BehaviorEvent o) => o.MyObject == this && Utils.Equals(o.Name, eventName));
				if (behaviorEvent != null)
				{
					return behaviorEvent;
				}
			}
		}
		return null;
	}

	public void SetScale(Vector2 scale)
	{
		scale = Utils.Clamp(scale, new Vector2(-100f, -100f), new Vector2(100f, 100f));
		if (scale.X == 0f)
		{
			scale.X = 0.001f;
		}
		if (scale.Y == 0f)
		{
			scale.Y = 0.001f;
		}
		if (Scale == scale)
		{
			return;
		}
		Scale = scale;
		if (RigidBody != null && !PartData.PhysFixedScale)
		{
			if (ParentObject != null)
			{
				scale *= ParentObject.WorldTransformation.Scale;
			}
			Vector2 scaleFactor = scale / RigidBody.Scale;
			RigidBody.ChangeScale(scaleFactor);
		}
		if (IsInitDone)
		{
			UpdateVisibilityRect();
		}
	}

	public void SetScale(float x, float y)
	{
		SetScale(new Vector2(x, y));
	}

	public void SetScale(float scale)
	{
		SetScale(new Vector2(scale, scale));
	}

	public void SetRotation(float rotation)
	{
		Rotation = rotation;
		if (RigidBody != null)
		{
			if (ParentObject != null)
			{
				rotation += ParentObject.WorldTransformation.Rotation;
			}
			RigidBody.WorldRotation = rotation;
		}
		if (IsInitDone)
		{
			UpdateVisibilityRect();
		}
	}

	public void SetPosition(float x, float y)
	{
		SetPosition(new Vector2(x, y));
	}

	public void SetPosition(Vector2 pos)
	{
		Position = pos;
		UpdateRigidBodyTransformation();
	}

	public void SetScreenPosition(Vector2 pos)
	{
		Vector2 position = MyLayer.MyLevel.Camera.ScreenPosToCurrentLayer(pos);
		SetPosition(position);
	}

	private void UpdateRigidBodyTransformation()
	{
		if (RigidBody != null)
		{
			Transformation worldTransformation = WorldTransformation;
			RigidBody.SetWorldTransform(worldTransformation.Position, worldTransformation.Rotation);
			if (!PartData.PhysFixedScale)
			{
				Vector2 scaleFactor = worldTransformation.Scale / RigidBody.Scale;
				RigidBody.ChangeScale(scaleFactor);
			}
		}
	}

	public void SetDestroyOnKill(bool value, bool bOnlyUndefined)
	{
		if (!bOnlyUndefined || !PartData.IsDestroyOnKillDefined)
		{
			_destroyOnKill = value;
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].SetDestroyOnKill(value, bOnlyUndefined);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].SetDestroyOnKill(value, bOnlyUndefined);
			}
		}
	}

	public GameObject FindNearestTarget(TargetClass targetClasses, float maxDistance = 2000f, float minDistace = 0f, List<GameObject> ignoreList = null, bool bOnlyHittable = false)
	{
		GameObject result = null;
		List<GameObject> visibleObjectsInRangeInAllPhysLayers = MyLevel.GetVisibleObjectsInRangeInAllPhysLayers(WorldPosition, targetClasses, maxDistance, ignoreList, bOnlyHittable);
		float num = maxDistance * maxDistance;
		float num2 = minDistace * minDistace;
		foreach (GameObject item in visibleObjectsInRangeInAllPhysLayers)
		{
			if (item != this)
			{
				GameObject gameObject = ((item.TargetPart == null || item.TargetPart.IsDestroyed || item.TargetPart.IsKilled) ? item : item.TargetPart);
				float num3 = Vector2.DistanceSquared(WorldPosition, gameObject.GetTargetWorldPosition());
				if (num3 < num && num3 >= num2)
				{
					num = num3;
					result = gameObject;
				}
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(visibleObjectsInRangeInAllPhysLayers);
		return result;
	}

	public List<GameObject> FindAllTargets(TargetClass targetClasses, float maxDistance, List<GameObject> ignoreList = null, bool bOnlyHittable = false)
	{
		return MyLevel.GetVisibleObjectsInRangeInAllPhysLayers(WorldPosition, targetClasses, maxDistance, ignoreList, bOnlyHittable);
	}

	public GameObject FindNearestHeroObject(int jetPackMode)
	{
		GameObject result = null;
		if (GameJnG.HeroList.Count == 1)
		{
			result = (((jetPackMode != 1 && !GameJnG.HeroList[0].IsDestroyed) || !GameJnG.HeroList[0].IsJetPackActive) ? GameJnG.HeroList[0].MyObject : GameJnG.HeroList[0].JetPackObject);
		}
		else
		{
			float num = float.MaxValue;
			foreach (Hero hero in GameJnG.HeroList)
			{
				GameObject gameObject;
				if (hero.IsDestroyed)
				{
					if (!hero.IsJetPackActive)
					{
						continue;
					}
					gameObject = hero.JetPackObject;
				}
				else
				{
					gameObject = hero.MyObject;
				}
				float num2 = Vector2.Distance(gameObject.VirtualScreenPosition, VirtualScreenPosition);
				if (num2 < num)
				{
					num = num2;
					result = gameObject;
				}
			}
		}
		return result;
	}

	public Hero FindNearestHero()
	{
		if (GameJnG.HeroList.Count == 1)
		{
			return GameJnG.HeroList[0];
		}
		float num = float.MaxValue;
		Hero result = null;
		foreach (Hero hero in GameJnG.HeroList)
		{
			if (!hero.IsDestroyed)
			{
				float num2 = Vector2.Distance(hero.MyObject.VirtualScreenPosition, VirtualScreenPosition);
				if (num2 < num)
				{
					num = num2;
					result = hero;
				}
			}
		}
		return result;
	}

	public float GetRotationToTarget(GameObject targetObject, float defaultAngle = 0f)
	{
		if (targetObject == null)
		{
			return defaultAngle;
		}
		return Utils.GetRotationTo(VirtualScreenPosition, targetObject.TargetVirtualScreenPosition);
	}

	public float GetSpineAngleTo(int slotIndex, GameObject targetObject, float defaultAngle = 0f)
	{
		if (targetObject == null && SpineRenderer == null)
		{
			return defaultAngle;
		}
		Slot slot = SpineRenderer.GetSlot(slotIndex);
		if (slot == null)
		{
			return defaultAngle;
		}
		return Utils.GetRotationTo(Vector2.Transform(new Vector2(slot.Bone.WorldX, slot.Bone.WorldY), WorldMatrix), targetObject.Position);
	}

	public float GetRotationToHero(int heroNumber)
	{
		if (heroNumber >= 0 && heroNumber < GameJnG.HeroList.Count)
		{
			return GetRotationToHero(GameJnG.HeroList[heroNumber]);
		}
		return 0f;
	}

	public float GetRotationToHero(Hero hero)
	{
		if (hero != null)
		{
			return GetRotationToTarget(hero.MyObject);
		}
		return 0f;
	}

	public float GetRotationToNearestHero()
	{
		return GetRotationToTarget(FindNearestHeroObject(0));
	}

	public float GetSpineAngleToHero(int slotIndex, int heroNumber)
	{
		if (heroNumber >= 0 && heroNumber < GameJnG.HeroList.Count)
		{
			return GetSpineAngleTo(slotIndex, GameJnG.HeroList[heroNumber].MyObject);
		}
		return 0f;
	}

	public void AwakePhysics(bool resetDynamics)
	{
		if (RigidBody != null)
		{
			RigidBody.WakeUp(resetDynamics);
		}
	}

	public void HardDestroy()
	{
		IsDestroyed = true;
		UnregisterEvents();
		DestroyEmitter();
		DestroyRigidBody();
		StopSound(bLoopedOnly: true);
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].HardDestroy();
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].HardDestroy();
			}
		}
	}

	public void Destroy()
	{
		if (!IsDestroyed && (!GameCore.IsEditorEnabled || GameCore.Editor.CurrentTool != EditorToolEnum.Emiter || GameCore.Editor.CurrentObject == null || GetMainPart() != GameCore.Editor.CurrentObject.GetMainPart()))
		{
			OnDestroy?.Invoke();
			IsDestroyed = true;
			UnregisterEvents();
			DestroyEmitter();
			DestroyRigidBody();
			StopSound(bLoopedOnly: true);
			if (PartData != null && ResetSpineTrackOnDestroy && SpineConnector != null && IsMainPart)
			{
				ResetSpineTrack(PartData.SpineTrack);
			}
			HitEffectTimer = 0f;
			if (SpineConnector != null)
			{
				SpineConnector.UpdateHitFactor();
			}
			DestroyChildren();
		}
	}

	public void DestroyRigidBody()
	{
		if (RigidBody != null)
		{
			_velocity = RigidBody.Velocity;
			RigidBody.Destroy();
			RigidBody = null;
		}
	}

	public static void DestroyRigidBodyWithChildren(GameObject go)
	{
		go.DestroyRigidBody();
		if (go.ChildObjectListFront != null)
		{
			int count = go.ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				DestroyRigidBodyWithChildren(go.ChildObjectListFront[i]);
			}
		}
		if (go.ChildObjectListBack != null)
		{
			int count2 = go.ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				DestroyRigidBodyWithChildren(go.ChildObjectListBack[j]);
			}
		}
	}

	public void DestroyEmitter()
	{
		if (Emitter != null)
		{
			Emitter.Destroy();
			Emitter = null;
		}
	}

	public void DestroyChildren()
	{
		if (ChildObjectListFront != null)
		{
			for (int num = ChildObjectListFront.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = ChildObjectListFront[num];
				if (gameObject.PartData != null && gameObject.PartData.DetachOnParentDestroy)
				{
					DetachChild(gameObject, MyLayer, bRestoreClass: true);
				}
				else
				{
					gameObject.Destroy();
				}
			}
		}
		if (ChildObjectListBack == null)
		{
			return;
		}
		for (int num2 = ChildObjectListBack.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject2 = ChildObjectListBack[num2];
			if (gameObject2.PartData != null && gameObject2.PartData.DetachOnParentDestroy)
			{
				DetachChild(gameObject2, MyLayer, bRestoreClass: true);
			}
			else
			{
				gameObject2.Destroy();
			}
		}
	}

	public void DestroyMain()
	{
		GetMainPart()?.Destroy();
	}

	private void UpdateMotion(float elapsedTime)
	{
		Motion.Update(elapsedTime);
		if (ParentObject != null)
		{
			Position += Motion.Velocity * elapsedTime / ParentObject.WorldScale;
		}
		else
		{
			Position += Motion.Velocity * elapsedTime;
		}
		Rotation += Motion.RotationSpeed * elapsedTime;
	}

	protected void UnregisterEvents()
	{
		OnLevelBegin = null;
		OnActivate = null;
		OnDeactivate = null;
		OnDraw = null;
		OnBeforeCollision = null;
		OnCollision = null;
		OnAfterCollision = null;
		OnDestroy = null;
		OnEnvironmentChanged = null;
		OnHit = null;
		OnHitBegin = null;
		OnKill = null;
		OnLink = null;
		OnPathEnd = null;
		OnShockHit = null;
		OnSeparation = null;
		OnAnimEnd = null;
		OnSpineAnimEnd = null;
		OnSpineAnimEvent = null;
		OnSpineAnimStart = null;
		OnSpineAnimCompleted = null;
		OnTrigger = null;
		OnUpdateBegin = null;
		OnUpdate = null;
		OnUpdateEnd = null;
		OnEditorUpdate = null;
		OnEditorUpdateBegin = null;
		OnEditorUpdateEnd = null;
		try
		{
			OnUnregisterEvents?.Invoke();
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Exception in OnUnregisterEvents: {0}", ex);
		}
		OnUnregisterEvents = null;
	}

	internal void ApplyPartState(PartState partState)
	{
		if (PartData == null || partState == null)
		{
			return;
		}
		_wasStateChanged = true;
		ResetPrecalculations();
		LastState = CurrentState;
		CurrentState = partState;
		PartStateId = partState.Id;
		if (CurrentState.UseScale)
		{
			SetScale(CurrentState.Scale);
		}
		if (!IsMainPart)
		{
			if (CurrentState.UseRotation)
			{
				SetRotation(CurrentState.Rotation);
			}
			if (CurrentState.UseOffset)
			{
				SetPosition(CurrentState.Offset);
			}
		}
		if (CurrentState.UseVelocity)
		{
			SetVelocity(CurrentState.Velocity);
		}
		if (CurrentState.UseRemconText)
		{
			GetMainPart().RemconText = CurrentState.RemconText;
		}
		if (CurrentState.UseAtlas)
		{
			if (CurrentState.Atlas == null)
			{
				DestroySpriteRenderer();
			}
			else
			{
				if (SpriteRenderer == null)
				{
					CreateSpriteRenderer(CurrentState.Atlas, PartData.SkinId);
				}
				if (SpriteRenderer.Atlas != CurrentState.Atlas)
				{
					SpriteRenderer.SetAtlas(CurrentState.Atlas, PartData.SkinId);
					if (CurrAtlasAnimId != null && !CurrentState.UseAnim)
					{
						CreateAnimator(CurrAtlasAnimId, SpriteRenderer.Animator.Duration, SpriteRenderer.Animator.IsLooped);
					}
					else
					{
						CurrAtlasAnimId = null;
					}
				}
			}
		}
		if (CurrentState.UseAnim && !CurrentState.AnimId.Equals(CurrAtlasAnimId))
		{
			CreateAnimator(CurrentState.AnimId, CurrentState.AnimDuration, CurrentState.IsAnimLooped);
		}
		if (CurrentState.UseFrame)
		{
			Frame = CurrentState.Frame;
		}
		if (CurrentState.UseEnabled)
		{
			IsEnabled = CurrentState.IsEnabled;
		}
		if (CurrentState.UseVisible)
		{
			IsVisible = CurrentState.IsVisible;
		}
		if (CurrentState.UseImmortal)
		{
			IsImmortal = CurrentState.IsImmortal;
		}
		if (CurrentState.UseEmitterReset && Emitter != null)
		{
			Emitter.Reset();
		}
		if (CurrentState.UseColor)
		{
			SetColor(CurrentState.Color);
		}
		if (ParentObject != null && ChildZOder != CurrentState.ZOrder && CurrentState.UseZOrder)
		{
			ParentObject.ChangeChildZOrder(this, CurrentState.ZOrder);
		}
		if (CurrentState.UseZOrder)
		{
			ZOrder = CurrentState.ZOrder;
		}
		if (CurrentState.UsePathPaused)
		{
			IsPathPaused = CurrentState.PathPaused;
		}
		if (IsInitDone)
		{
			ReRegisterEvents();
		}
		if (CurrentState.UseSpineAnim)
		{
			SetSpineAnimation(PartData.SpineTrack, CurrentState.SpineAnimId, CurrentState.IsAnimLooped);
			if (SpineRenderer != null)
			{
				SpineRenderer.UpdateBegin(0f);
			}
		}
		if ((CurrentState.UsePhysEnabled || !_isPhysBodyInitDone) && (IsActivated || MyLayer == null || (MyLayer.UsePhysics && MyLayer.ScrollFactor == Vector2.One && !GetMainPart().PartData.PhysOnActivate)) && (GameCore.IsEditorEnabled || GetMainPart().WasTriggerPointActivated || GetMainPart().TriggerPointMode == TriggerPointModeEnum.Normal))
		{
			CreateRigidBodyByData();
		}
	}

	private void ReRegisterEvents()
	{
		OnGameObjectEventHandler onGameObjectEventHandler = OnReRegisterEvents;
		UnregisterEvents();
		if (!IsInLOD)
		{
			return;
		}
		if (_behaviorList != null)
		{
			for (int i = 0; i < Enumerable.Count(_behaviorList); i++)
			{
				BehaviorScript behaviorScript = _behaviorList[i];
				if (behaviorScript.IsForState(CurrentState))
				{
					try
					{
						behaviorScript.RegisterEvents();
					}
					catch (Exception ex)
					{
						GameCore.LogErr("ReRegister events exception: {0}", ex.ToString());
					}
				}
			}
		}
		try
		{
			onGameObjectEventHandler?.Invoke(this);
		}
		catch (Exception ex2)
		{
			GameCore.LogErr("OnReReregisterEvents script exception: {0} in object '{1}'", ex2, Id);
		}
	}

	public void ChangePartState(PartState state)
	{
		if (!IsDestroyed && state != null && state != CurrentState)
		{
			ApplyPartState(state);
		}
	}

	public void ChangePartStateToAll(string stateId)
	{
		ChangePartState(stateId);
		if (ChildObjectListFront != null)
		{
			for (int num = ChildObjectListFront.Count - 1; num >= 0; num--)
			{
				if (ChildObjectListFront[num].PartData != null && !ChildObjectListFront[num].PartData.IgnoreParentState)
				{
					ChildObjectListFront[num].ChangePartStateToAll(stateId);
				}
			}
		}
		if (ChildObjectListBack == null)
		{
			return;
		}
		for (int num2 = ChildObjectListBack.Count - 1; num2 >= 0; num2--)
		{
			if (ChildObjectListBack[num2].PartData != null && !ChildObjectListBack[num2].PartData.IgnoreParentState)
			{
				ChildObjectListBack[num2].ChangePartStateToAll(stateId);
			}
		}
	}

	public void ChangePartState(string stateId)
	{
		if (!IsDestroyed && stateId != null)
		{
			if (LastState != null && Utils.Equals(stateId, "last"))
			{
				stateId = LastState.Id;
			}
			if (PartData != null && PartData.PartStates.TryGetValue(stateId.ToLowerLazy(), out var value))
			{
				ChangePartState(value);
			}
		}
	}

	public BehaviorScript GetBehaviorScriptById(string id)
	{
		return _behaviorList?.Find((BehaviorScript b) => Utils.Equals(b.Id, id));
	}

	private void UpdatePathMotion(float elapsedTime)
	{
		if (SpeedKeyValue != null)
		{
			PathSpeed = SpeedKeyValue.CurrentValue;
		}
		if ((GameCore.IsEditorEnabled && IsLevelObject && !IsUpdateInEditorEnabled) || !IsTriggerFree)
		{
			PathTime = 0f;
		}
		else
		{
			PathTime = MyPath.GetNextPositionTime(PathTime, PathSpeed * elapsedTime);
		}
		if (IsDynamic && PathFollowMode == PathFollowModeEnum.Physical)
		{
			Position = RigidBody.WorldPosition;
			Rotation = RigidBody.WorldRotation;
		}
		else
		{
			Vector2 position = MyPath.GetPosition(PathTime) + PathOscilation;
			SetPosition(position);
		}
		if (!GameCore.IsEditorEnabled && MyPath.MyObject == this)
		{
			BezierPath myPath = MyPath;
			for (int i = _currentPathEventNodeIndex; i < myPath.EventNodeList.Count; i++)
			{
				BehaviorEventNode behaviorEventNode = myPath.EventNodeList[i];
				if (behaviorEventNode.Time <= PathTime)
				{
					_currentPathEventNodeIndex = i + 1;
					behaviorEventNode.Execute(null);
					if (myPath != MyPath)
					{
						break;
					}
				}
			}
		}
		if (PathTime >= 1f && !IsDestroyed)
		{
			try
			{
				OnPathEnd?.Invoke();
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Error in OnPathEnd method: {0}", ex.ToString());
			}
			switch (PartData.DestroyOnPathEndMode)
			{
			case 1:
				Destroy();
				break;
			case 2:
				Kill();
				break;
			case 3:
			{
				Vector2 velocity = Velocity;
				MyPath = null;
				SetVelocity(velocity);
				break;
			}
			case 4:
			{
				Vector2 velocity = Velocity;
				MyPath = null;
				SetVelocity(velocity + MyLevel.ScrollSpeed);
				break;
			}
			}
		}
	}

	internal void UpdatePosition(float elapsedTime)
	{
		if (SpeedKeyValue != null)
		{
			SpeedKeyValue.UpdateCurrentValue();
		}
		if (PartData != null && AngleKeyValue != null && !GameCore.IsEditorEnabled)
		{
			AngleKeyValue.UpdateCurrentValue();
			if (!IsMainPart)
			{
				if (SpineConnector != null)
				{
					SpineConnector.SetAngle(AngleKeyValue.CurrentValue, bWorld: false);
				}
				else
				{
					SetRotation(Utils.DegToRad(AngleKeyValue.CurrentValue));
				}
			}
			else if (PathFollowMode == PathFollowModeEnum.FreeRotation || MyPath == null)
			{
				SetRotation(Utils.DegToRad(AngleKeyValue.CurrentValue) - PartData.PathFront);
			}
			else
			{
				SetRotation(MyPath.GetDirection(PathTime) - PartData.PathFront);
			}
		}
		else if (MyPath != null && PathFollowMode == PathFollowModeEnum.Follow)
		{
			SetRotation(MyPath.GetDirection(PathTime) - PartData.PathFront);
		}
		if (MyPath != null && IsMainPart && (!IsKilled || PartData.MoveOnKill))
		{
			if (PartData.PathFollowMode == PathFollowModeEnum.NoMove)
			{
				if (GameCore.IsEditorEnabled || PathOscilation != Vector2.Zero)
				{
					UpdatePathMotion(0f);
				}
			}
			else if (IsPathPaused)
			{
				UpdatePathMotion(0f);
			}
			else
			{
				UpdatePathMotion(elapsedTime);
			}
		}
		else if (RigidBody != null && IsMainPart)
		{
			if (!RigidBody.IsStatic)
			{
				if (SpeedKeyValue != null)
				{
					SetRotationVelocity(SpeedKeyValue.CurrentValue, WorldRotation);
				}
				Position = GetMyLayerPositionInMainLayer(RigidBody.WorldPosition);
				Rotation = RigidBody.WorldRotation;
			}
			else if (MyLayer.ScrollFactor != Vector2.One)
			{
				RigidBody.WorldPosition = GetMainLayerPosition(Position);
				Rotation = RigidBody.WorldRotation;
			}
			else if (IsMovable && (!GameCore.IsEditorEnabled || GameCore.Editor.IsUpdateBehaviorsEnabled))
			{
				UpdateMotion(elapsedTime);
			}
		}
		else if (IsMovable && (!GameCore.IsEditorEnabled || GameCore.Editor.IsUpdateBehaviorsEnabled))
		{
			UpdateMotion(elapsedTime);
		}
		if ((!IsMainPart || IsMovable) && RigidBody != null)
		{
			SetVelocity(GetMainPart().Velocity);
			UpdateRigidBodyTransformation();
		}
		UpdateVirtualScreenPostion();
	}

	public void UpdateVirtualScreenPostion()
	{
		ScreenPosition = MyCamera.CurrentLayerPosToScreen(WorldPosition);
		VirtualScreenPosition = (ScreenPosition - MyCamera.ViewTopLeftOnScreen) / GameCore.VirtualAspectRatio / MyLayer.MyLevel.Camera.Zoom;
		if (TargetPart != null)
		{
			Vector2 vector = ((TargetPart.WorldRotation != 0f) ? Utils.RotateVector2(TargetPart.PartData.TargetOffset, TargetPart.WorldRotation) : TargetPart.PartData.TargetOffset);
			TargetVirtualScreenPosition = (MyCamera.CurrentLayerPosToScreen(TargetPart.WorldPosition + vector) - MyCamera.ViewTopLeftOnScreen) / GameCore.VirtualAspectRatio / TargetPart.MyLayer.MyLevel.Camera.Zoom;
		}
		else if (PartData != null && PartData.TargetOffset != Vector2.Zero)
		{
			Vector2 vector2 = ((WorldRotation != 0f) ? Utils.RotateVector2(PartData.TargetOffset, WorldRotation) : PartData.TargetOffset);
			TargetVirtualScreenPosition = (MyCamera.CurrentLayerPosToScreen(WorldPosition + vector2) - MyCamera.ViewTopLeftOnScreen) / GameCore.VirtualAspectRatio / MyLayer.MyLevel.Camera.Zoom;
		}
		else
		{
			TargetVirtualScreenPosition = VirtualScreenPosition;
		}
	}

	public void AddLightOrShadowToRender()
	{
		if (PartData == null)
		{
			return;
		}
		if (GameCore.UseShopLightMaps)
		{
			if (Options.IsLightsOn && PartData.LightOrShadow > 0 && MyLayer.ShadowToMapNumber >= 0 && MyLayer.ShadowToMapNumber < 3 && MyLayer.LightToMapNumber >= 0)
			{
				GameJnG.ShopMenu.LightMapArray[MyLayer.LightToMapNumber].LightObjectList.Add(this);
			}
		}
		else if (Options.IsShadowsOn && PartData.LightOrShadow < 0 && MyLayer.ShadowToMapNumber >= 0 && MyLayer.ShadowToMapNumber < 3)
		{
			GameCore.ShadowMapArray[MyLayer.ShadowToMapNumber].LightObjectList.Add(this);
		}
		else if (Options.IsLightsOn && MyLayer.LightToMapNumber >= 0 && MyLayer.LightToMapNumber < 3 && PartData.LightOrShadow > 0)
		{
			GameCore.LightMapArray[MyLayer.LightToMapNumber].LightObjectList.Add(this);
		}
	}

	public Vector2 GetMainLayerPosition(Vector2 position)
	{
		if (MyLayer == null || MyLayer.ScrollFactor == Vector2.One)
		{
			return position;
		}
		return position + MyCamera.Position * (Vector2.One - MyLayer.ScrollFactor);
	}

	public Vector2 GetMainLayerPosition()
	{
		if (MyLayer == null || MyLayer.ScrollFactor == Vector2.One)
		{
			return WorldPosition;
		}
		return WorldPosition + MyCamera.Position * (Vector2.One - MyLayer.ScrollFactor);
	}

	public Vector2 GetMyLayerPositionInMainLayer(Vector2 position)
	{
		if (MyLayer == null || MyLayer.ScrollFactor == Vector2.One)
		{
			return position;
		}
		return position - MyCamera.Position * (Vector2.One - MyLayer.ScrollFactor);
	}

	public static void SetHitEffectWithChildren(GameObject go, float hitEffectTimer)
	{
		if (go.HitEffectTimer < hitEffectTimer)
		{
			go.HitEffectTimer = hitEffectTimer;
		}
		if (go.ChildObjectListFront != null)
		{
			int count = go.ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = go.ChildObjectListFront[i];
				if (gameObject.PartData != null && !gameObject.PartData.IgnoreParentHitEffect)
				{
					SetHitEffectWithChildren(gameObject, hitEffectTimer);
				}
			}
		}
		if (go.ChildObjectListBack == null)
		{
			return;
		}
		int count2 = go.ChildObjectListBack.Count;
		for (int j = 0; j < count2; j++)
		{
			GameObject gameObject2 = go.ChildObjectListBack[j];
			if (gameObject2.PartData != null && !gameObject2.PartData.IgnoreParentHitEffect)
			{
				SetHitEffectWithChildren(gameObject2, hitEffectTimer);
			}
		}
	}

	public static void CancelHitEffectWithChildren(GameObject go)
	{
		go.HitEffectTimer = 0f;
		if (go.SpineConnector != null)
		{
			go.SpineConnector.UpdateHitFactor();
		}
		if (go.ChildObjectListFront != null)
		{
			int count = go.ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				CancelHitEffectWithChildren(go.ChildObjectListFront[i]);
			}
		}
		if (go.ChildObjectListBack != null)
		{
			int count2 = go.ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				CancelHitEffectWithChildren(go.ChildObjectListBack[j]);
			}
		}
	}

	public void EditorUpdateBegin(Rake2D.Editor.Editor editor, float elapsedTime)
	{
		try
		{
			OnEditorUpdateBegin?.Invoke(editor, elapsedTime);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Error in behavior script - EditorUpdateBegin method: {0}", ex.ToString());
		}
	}

	public void EditorUpdateEnd(Rake2D.Editor.Editor editor, float elapsedTime)
	{
		try
		{
			OnEditorUpdateEnd?.Invoke(editor, elapsedTime);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Error in behavior script - EditorUpdateEnd method: {0}", ex.ToString());
		}
	}

	public void DoZeroEvents()
	{
		if (MyPath != null)
		{
			for (int i = 0; i < MyPath.EventNodeList.Count; i++)
			{
				BehaviorEventNode behaviorEventNode = MyPath.EventNodeList[i];
				if (!(behaviorEventNode.Time <= 0f))
				{
					break;
				}
				behaviorEventNode.Execute(null);
				_currentPathEventNodeIndex = i + 1;
				if (behaviorEventNode.TimeLine != null)
				{
					SetCurrentTimeLine(behaviorEventNode.TimeLine);
				}
			}
			if (GameCore.IsEditorEnabled)
			{
				SetCurrentTimeLine(null);
			}
		}
		BehaviorTimeLine currentTimeLine = GetMainPart().CurrentTimeLine;
		if (currentTimeLine == null || currentTimeLine.EventNodeList.IsNullOrEmpty())
		{
			return;
		}
		for (int j = 0; j < currentTimeLine.EventNodeList.Count; j++)
		{
			BehaviorEventNode behaviorEventNode2 = currentTimeLine.EventNodeList[j];
			if (behaviorEventNode2.Time <= 0f)
			{
				behaviorEventNode2.Execute(currentTimeLine);
				_currentTimeNodeIndex = j + 1;
				continue;
			}
			break;
		}
	}

	private void UpdateEnd(float elapsedTime)
	{
		float elapsedTime2 = (IsTriggerFree ? elapsedTime : 0f);
		try
		{
			OnUpdateEnd?.Invoke(elapsedTime2);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Error in behavior script - UpdateEnd method in object '{0}': {1}", Id, ex.ToString());
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].UpdateEnd(elapsedTime);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].UpdateEnd(elapsedTime);
			}
		}
		RemconedCancelByHero = null;
	}

	public virtual void Update(float elapsedTime)
	{
		if (!IsUpdatable)
		{
			if (!IsDestroyed && IsActivated && IsInLOD)
			{
				if (!IsMainPart && RigidBody != null)
				{
					ResetPrecalculations();
					UpdatePosition(elapsedTime);
				}
				if (ChildObjectListFront != null)
				{
					int count = ChildObjectListFront.Count;
					for (int i = 0; i < count; i++)
					{
						GameObject gameObject = ChildObjectListFront[i];
						if (gameObject.RigidBody != null)
						{
							gameObject.ResetPrecalculations();
							gameObject.UpdatePosition(elapsedTime);
						}
					}
				}
				if (ChildObjectListBack != null)
				{
					int count2 = ChildObjectListBack.Count;
					for (int j = 0; j < count2; j++)
					{
						GameObject gameObject2 = ChildObjectListBack[j];
						if (gameObject2.RigidBody != null)
						{
							gameObject2.ResetPrecalculations();
							gameObject2.UpdatePosition(elapsedTime);
						}
					}
				}
			}
			if (_wasStateChanged)
			{
				PlayStateSound();
				_wasStateChanged = false;
			}
			return;
		}
		_sendedLinkCount = 0;
		if (!IsInitDone)
		{
			Init();
			LevelInit();
		}
		if (_wasStateChanged)
		{
			PlayStateSound();
			_wasStateChanged = false;
		}
		if (MyLevel != null && CreateTime == MyLevel.Timer)
		{
			elapsedTime = 0f;
		}
		Timer += elapsedTime;
		if (!IsKilled && KillTimer < float.MaxValue)
		{
			KillTimer -= elapsedTime;
			if (KillTimer <= 0f)
			{
				Kill();
			}
		}
		ResetPrecalculations();
		float elapsedTime2 = (IsTriggerFree ? elapsedTime : 0f);
		if (!IsInEditorMode)
		{
			try
			{
				OnUpdateBegin?.Invoke(elapsedTime2);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("Error in behavior script - OnUpdateBegin method in object '{0}': {1}", Id, ex.ToString());
			}
		}
		if (SpineRenderer != null)
		{
			SpineRenderer.UpdateBegin(elapsedTime);
		}
		if (SpineConnector != null)
		{
			SpineConnector.UpdateHitFactor();
		}
		UpdatePosition(elapsedTime);
		if (TargetPart != null && (TargetPart.IsDestroyed || TargetPart.IsKilled))
		{
			TargetPart = null;
		}
		if (!IsInEditorMode)
		{
			if (CurrentTimeLine != null)
			{
				UpdateTimeLine(elapsedTime2);
			}
			try
			{
				OnUpdate?.Invoke(elapsedTime2);
			}
			catch (Exception ex2)
			{
				GameCore.LogErr("Error in behavior script - OnUpdate method in object '{0}': {1}", Id, ex2.ToString());
			}
		}
		else
		{
			try
			{
				OnEditorUpdate?.Invoke(elapsedTime);
			}
			catch (Exception ex3)
			{
				GameCore.LogErr("Error in behavior script - OnEditorUpdate method: {0}", ex3.ToString());
			}
		}
		if (ChildObjectListFront != null)
		{
			for (int num = ChildObjectListFront.Count - 1; num >= 0; num--)
			{
				GameObject gameObject3 = ChildObjectListFront[num];
				gameObject3.Update(elapsedTime);
				if (gameObject3.IsDestroyed || gameObject3.IsDetached)
				{
					ChildObjectListFront.RemoveAt(num);
					if (gameObject3.IsDestroyed)
					{
						GameCore.Allocator.DeleteGameObject(gameObject3);
					}
				}
			}
		}
		if (ChildObjectListBack != null)
		{
			for (int num2 = ChildObjectListBack.Count - 1; num2 >= 0; num2--)
			{
				GameObject gameObject4 = ChildObjectListBack[num2];
				gameObject4.Update(elapsedTime);
				if (gameObject4.IsDestroyed || gameObject4.IsDetached)
				{
					ChildObjectListBack.RemoveAt(num2);
					if (gameObject4.IsDestroyed)
					{
						GameCore.Allocator.DeleteGameObject(gameObject4);
					}
				}
			}
		}
		if (LinkedObjectList != null && (PartData == null || PartData.DoLinkedListCheck || GameCore.IsEditorEnabled))
		{
			for (int num3 = LinkedObjectList.Count - 1; num3 >= 0; num3--)
			{
				if (LinkedObjectList[num3].IsDestroyed)
				{
					LinkedObjectList.RemoveAt(num3);
				}
			}
			if (LinkedObjectList.Count == 0)
			{
				LinkedObjectList = GameCore.Allocator.DeleteAliasingGameObjectList(LinkedObjectList);
			}
		}
		if (IsVisible && Options.IsLightsOn)
		{
			AddLightOrShadowToRender();
		}
		if (SpriteRenderer != null && SpriteRenderer.Update(elapsedTime))
		{
			OnAnimEnd?.Invoke();
		}
		if (SpineRenderer != null)
		{
			SpineRenderer.UpdateEnd();
		}
		if (IsDynamic)
		{
			UpdateVisibilityRect();
		}
		if (Emitter != null && IsVisible)
		{
			Emitter.Update(elapsedTime);
			if (Emitter.IsDestroyed)
			{
				Emitter = null;
			}
		}
		if (ParticleListBack != null)
		{
			for (int num4 = ParticleListBack.Count - 1; num4 >= 0; num4--)
			{
				Particle particle = ParticleListBack[num4];
				particle.Update(elapsedTime);
				if (particle.IsDestroyed)
				{
					GameCore.Allocator.DeleteParticle(particle);
					ParticleListBack.RemoveAt(num4);
				}
			}
			DebugInfo.ObjectParticleCountBack += ParticleListBack.Count;
		}
		if (ParticleListFront != null)
		{
			for (int num5 = ParticleListFront.Count - 1; num5 >= 0; num5--)
			{
				Particle particle2 = ParticleListFront[num5];
				particle2.Update(elapsedTime);
				if (particle2.IsDestroyed)
				{
					GameCore.Allocator.DeleteParticle(particle2);
					ParticleListFront.RemoveAt(num5);
				}
			}
			DebugInfo.ObjectParticleCountFront += ParticleListFront.Count;
		}
		if (_stripLineListFront != null)
		{
			for (int num6 = _stripLineListFront.Count - 1; num6 >= 0; num6--)
			{
				StripLine stripLine = _stripLineListFront[num6];
				stripLine.Update(elapsedTime);
				if (stripLine.IsDestroyed)
				{
					GameCore.Allocator.DeleteStripLine(stripLine);
					_stripLineListFront.RemoveAt(num6);
				}
			}
			DebugInfo.ObjectStripLineCountFront += _stripLineListFront.Count;
		}
		if (_stripLineListBack != null)
		{
			for (int num7 = _stripLineListBack.Count - 1; num7 >= 0; num7--)
			{
				StripLine stripLine2 = _stripLineListBack[num7];
				stripLine2.Update(elapsedTime);
				if (stripLine2.IsDestroyed)
				{
					GameCore.Allocator.DeleteStripLine(stripLine2);
					_stripLineListBack.RemoveAt(num7);
				}
			}
			DebugInfo.ObjectStripLineCountBack += _stripLineListBack.Count;
		}
		if (_soundList != null)
		{
			UpdateSound();
		}
		if (LifeTime < float.MaxValue)
		{
			LifeTime -= elapsedTime;
			if (LifeTime <= 0f)
			{
				Destroy();
				return;
			}
		}
		Utils.ChangeValueToTarget(ref HitEffectTimer, 0f, elapsedTime * 10f);
		if (IsActivated)
		{
			UpdateEnd(elapsedTime);
		}
	}

	public void ZeroEmiterTimer()
	{
		if (Emitter != null)
		{
			Emitter.ZeroTimer();
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].ZeroEmiterTimer();
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].ZeroEmiterTimer();
			}
		}
	}

	private void DoOnTrigger()
	{
		try
		{
			OnTrigger?.Invoke();
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnTrigger script exception: {0} in object '{1}'", ex, Id);
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].DoOnTrigger();
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].DoOnTrigger();
			}
		}
	}

	private void CreateRigidBodyWithChildren()
	{
		CreateRigidBodyByData();
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].CreateRigidBodyWithChildren();
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].CreateRigidBodyWithChildren();
			}
		}
	}

	internal void UpdateTriggerPoint()
	{
		if (!IsTriggerFree && TriggerPoint.X < MyLayer.MyLevel.Camera.ViewBottomRightInWorld.X)
		{
			WasTriggerPointActivated = true;
			if (!_isActivated)
			{
				Activate();
			}
			else if (!_isPhysBodyInitDone && PartData != null)
			{
				CreateRigidBodyWithChildren();
			}
			DoOnTrigger();
		}
	}

	internal void UpdateInActionState()
	{
		Transformation worldTransformation = WorldTransformation;
		Vector2 vector = worldTransformation.Position + new Vector2(VisibilityRect.Left, VisibilityRect.Top);
		Vector2 vector2 = worldTransformation.Position + new Vector2(VisibilityRect.Right, VisibilityRect.Bottom);
		OutOfView = OutOfFrame.None;
		OutOfLevel = OutOfFrame.None;
		if (vector2.X < MyLevel.Camera.ViewTopLeftInWorld.X)
		{
			OutOfView |= OutOfFrame.OnLeft;
		}
		else if (vector.X > MyLevel.Camera.ViewBottomRightInWorld.X)
		{
			OutOfView |= OutOfFrame.OnRight;
		}
		if (vector2.Y < MyLevel.Camera.ViewTopLeftInWorld.Y)
		{
			OutOfView |= OutOfFrame.OnTop;
		}
		else if (vector.Y > MyLevel.Camera.ViewBottomRightInWorld.Y)
		{
			OutOfView |= OutOfFrame.OnBottom;
		}
		if (vector2.X < -100f)
		{
			OutOfLevel |= OutOfFrame.OnLeft;
		}
		else if (vector.X > MyLevel.Size.X + 100f)
		{
			OutOfLevel |= OutOfFrame.OnRight;
		}
		if (vector2.Y < -100f)
		{
			OutOfLevel |= OutOfFrame.OnTop;
		}
		else if (vector.Y > MyLevel.Size.Y + 100f)
		{
			OutOfLevel |= OutOfFrame.OnBottom;
		}
		if (!IsActivated && (OutOfView & OutOfFrame.OnLeftAndRight) == 0)
		{
			Activate();
		}
		else if (!GameCore.IsEditorEnabled && ((DestroyOutOfView & OutOfView) != 0 || (IsDynamic && OutOfLevel != OutOfFrame.None && DestroyOutOfView == OutOfFrame.Undefined)))
		{
			Destroy();
		}
		else if (ParentObject == null && !GameCore.IgnoreDestroyOnRightEdge && DestroyOutOfView == OutOfFrame.Undefined && IsDynamic && (OutOfView & OutOfFrame.OnRight) != 0)
		{
			Destroy();
		}
		else if (!GameCore.IsEditorEnabled && (DestroyOutOfLevel & OutOfLevel) != 0)
		{
			Destroy();
		}
		else if (ActiveMode == ActiveModeEnum.Normal && IsActivated && (OutOfView & OutOfFrame.OnLeftAndRight) != 0 && (MyPath == null || PathTime >= 1f || GameCore.IsEditorEnabled) && GameCore.Editor.CurrentObject != this && (RigidBody == null || !RigidBody.HasJoints() || vector2.X < MyLevel.Camera.ScreenTopLeftInWorld.X - 1920f))
		{
			Deactivate();
		}
	}

	internal void UpdateOnScreenState()
	{
		Vector2 worldPosition = WorldPosition;
		if ((Options.ScissorsMode > 0 && worldPosition.X + (float)VisibilityRect.Left < MyLevel.Camera.ViewBottomRightInWorld.X && worldPosition.X + (float)VisibilityRect.Right > MyLevel.Camera.ViewBottomRightInWorld.X) || ((Options.ScissorsMode == 0 || GameCore.IsEditorEnabled) && worldPosition.X + (float)VisibilityRect.Left < MyLevel.Camera.ScreenBottomRightInWorld.X && worldPosition.X + (float)VisibilityRect.Right > MyLevel.Camera.ScreenTopLeftInWorld.X))
		{
			IsOnScreenX = true;
		}
		else
		{
			IsOnScreenX = false;
		}
	}

	public void DoOnRemcon(Hero hero)
	{
		if (IsDestroyed)
		{
			return;
		}
		RemconedByHero = hero;
		RemconedCancelByHero = null;
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].DoOnRemcon(hero);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].DoOnRemcon(hero);
			}
		}
	}

	public void DoOnRemconCancel(Hero hero)
	{
		if (IsDestroyed)
		{
			return;
		}
		RemconedByHero = null;
		RemconedCancelByHero = hero;
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].DoOnRemconCancel(hero);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].DoOnRemconCancel(hero);
			}
		}
	}

	public void InvokeOnCollision(GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		try
		{
			OnCollision?.Invoke(this, collisionObject, contactPoint, normal);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnCollision script exception: {0} in object '{1}'", ex, Id);
		}
	}

	public void DoBeforeCollision(GameObject collisionObject)
	{
		if (IsDestroyed)
		{
			return;
		}
		try
		{
			OnBeforeCollision?.Invoke(collisionObject);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnBeforeCollision script exception: {0} in object '{1}'", ex, Id);
		}
	}

	public bool DoOnCollision(GameObject collisionObject, Vector2 contactPoint, Vector2 normal)
	{
		if (IsDestroyed || (IgnoreCollisionMode != 0 && IgnoreCollisionMode != 3))
		{
			return false;
		}
		IgnoreCollisionMode = 0;
		InvokeOnCollision(collisionObject, contactPoint, normal);
		if (collisionObject != null)
		{
			if (collisionObject.RigidBody != null && collisionObject.RigidBody.IsSensor)
			{
				return true;
			}
			if (HitOnCollision && Damage != 0f && IgnoreCollisionMode < 2)
			{
				collisionObject.Hit(Damage, DamageType, this);
			}
			if ((IgnoreCollisionMode > 0 && IgnoreCollisionMode < 3) || (collisionObject.PhysClass != PhysClass.Ground && collisionObject.PhysClass != PhysClass.Platform && !collisionObject.IsImmortal && collisionObject.MaxHP > 0f && PartData.Hardness >= collisionObject.PartData.Hardness && (collisionObject.IsDestroyed || collisionObject.IsKilled)))
			{
				return false;
			}
		}
		if (DestroyOnCollision)
		{
			if (!IsKilled && MaxHP > 0f)
			{
				Kill();
			}
			Destroy();
		}
		else
		{
			try
			{
				if (IgnoreCollisionMode < 3)
				{
					OnAfterCollision?.Invoke(this, collisionObject, contactPoint, normal);
				}
			}
			catch (Exception ex)
			{
				GameCore.LogErr("OnAfterCollision script exception: {0} in object '{1}'", ex, Id);
			}
		}
		return true;
	}

	internal void DoOnSeparation(GameObject gameObject)
	{
		if (!IsDestroyed)
		{
			try
			{
				OnSeparation?.Invoke(gameObject);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("OnSeparation script exception: {0} in object '{1}'", ex, Id);
			}
		}
	}

	public void DoOnLink(GameObject senderObject, string eventName, int value, bool bApplyChildren)
	{
		if (IsDestroyed)
		{
			return;
		}
		if (!IsLevelInitDone)
		{
			LevelInit();
		}
		if (!_isActivated && ActiveMode != ActiveModeEnum.NotActivateByLink)
		{
			Activate();
		}
		try
		{
			_sendedLinkCount++;
			if (_sendedLinkCount > 30)
			{
				GameCore.LogErr("Link circle between '{0}' and '{1}' at ({2}, {3})", Id, senderObject.Id, (int)WorldPosition.X, (int)WorldPosition.Y);
			}
			else
			{
				OnLink?.Invoke(senderObject, eventName, value);
			}
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnLink script exception: {0} in object '{1}'", ex, Id);
		}
		if (!bApplyChildren)
		{
			return;
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].DoOnLink(senderObject, eventName, value, bApplyChildren: true);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].DoOnLink(senderObject, eventName, value, bApplyChildren: true);
			}
		}
	}

	public void SendLinkToAll(GameObject senderObject, List<GameObject> objectList, string eventName, int value, bool bApplyToChildren)
	{
		if (objectList == null)
		{
			return;
		}
		foreach (GameObject @object in objectList)
		{
			try
			{
				@object.DoOnLink(senderObject ?? this, eventName, value, bApplyToChildren);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("OnLink script exception: {0} in object '{1}'", ex, @object.Id);
			}
		}
	}

	public void SendLinkToAll(GameObject senderObject, string eventName, int value, bool bApplyToChildren = true)
	{
		SendLinkToAll(senderObject, LinkedObjectList, eventName, value, bApplyToChildren);
	}

	private float ApplyHeroDamageFactors(float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		return hittingObject.MyHero.ApplyHeroDamageFactors(damage, damageType);
	}

	public float Hit(float damage, DamageTypeEnum damageType, GameObject hittingObject)
	{
		float currentHP = CurrentHP;
		float num = 0f;
		if (damageType == DamageTypeEnum.None || damage < 0f)
		{
			return 0f;
		}
		if (!IsDestroyed && !IsKilled && !GameCore.IsEditorEnabled)
		{
			if (hittingObject != null && hittingObject.MyHero != null)
			{
				damage = ApplyHeroDamageFactors(damage, damageType, hittingObject);
			}
			try
			{
				OnHitBegin?.Invoke(ref damage, damageType, hittingObject);
				if (MaxHP > 0f && !IsImmortal)
				{
					CurrentHP -= damage;
					if (CurrentHP < 0f)
					{
						CurrentHP = 0f;
					}
				}
				OnHit?.Invoke(damage, damageType, hittingObject);
			}
			catch (Exception ex)
			{
				GameCore.LogErr("OnHit script exception: {0} in object '{1}'", ex, Id);
			}
			num = currentHP - CurrentHP;
			if (num > 0f)
			{
				LastDamageType = damageType;
				LastHittingObject = hittingObject;
				if (damageType == DamageTypeEnum.Fire)
				{
					SetHitEffectWithChildren(this, PartData.HitEffectFactor / 4f);
				}
				else
				{
					SetHitEffectWithChildren(this, PartData.HitEffectFactor);
				}
				if ((PartData.TargetClass & (TargetClass)0x122F8) != 0)
				{
					GameJnG.AchievementManager.DamageCaused += num;
				}
			}
			if (HitPart != null && !IsImmortal && damage > 0f)
			{
				HitPart.Hit((MaxHP > 0f) ? num : damage, damageType, hittingObject);
			}
		}
		if (!IsImmortal && MaxHP > 0f && CurrentHP <= 0f && !IsKilled)
		{
			Kill();
		}
		return num;
	}

	internal void DoOnSpineAnimStart(int trackIndex)
	{
		if (IsDestroyed)
		{
			return;
		}
		try
		{
			OnSpineAnimStart?.Invoke(trackIndex);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnSpineAnimStart script exception: {0} in object '{1}'", ex, Id);
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].DoOnSpineAnimStart(trackIndex);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].DoOnSpineAnimStart(trackIndex);
			}
		}
	}

	internal void DoOnSpineAnimEnd(int trackIndex)
	{
		if (IsDestroyed)
		{
			return;
		}
		try
		{
			OnSpineAnimEnd?.Invoke(trackIndex);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnSpineAnimEnd script exception: {0} in object '{1}'", ex, Id);
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].DoOnSpineAnimEnd(trackIndex);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].DoOnSpineAnimEnd(trackIndex);
			}
		}
	}

	internal void DoOnSpineAnimCompleted(int trackIndex, int loopCount)
	{
		if (IsDestroyed)
		{
			return;
		}
		try
		{
			OnSpineAnimCompleted?.Invoke(trackIndex, loopCount);
		}
		catch (Exception ex)
		{
			GameCore.LogErr("OnSpineAnimCompleted script exception: {0} in object '{1}'", ex, Id);
		}
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = ChildObjectListFront[i];
				if (gameObject.GetCurrentSpineRenderer() == GetCurrentSpineRenderer())
				{
					gameObject.DoOnSpineAnimCompleted(trackIndex, loopCount);
				}
			}
		}
		if (ChildObjectListBack == null)
		{
			return;
		}
		int count2 = ChildObjectListBack.Count;
		for (int j = 0; j < count2; j++)
		{
			GameObject gameObject2 = ChildObjectListBack[j];
			if (gameObject2.GetCurrentSpineRenderer() == GetCurrentSpineRenderer())
			{
				gameObject2.DoOnSpineAnimCompleted(trackIndex, loopCount);
			}
		}
	}

	public void AddScore(int addScore = -1)
	{
		if (addScore == -1)
		{
			addScore = PartData.Score;
		}
		int num = (int)((float)addScore * FactorManager.ScoreFactor);
		if (num == 0)
		{
			return;
		}
		GameJnG.LevelScore += num;
		if ((float)addScore >= 10f * FactorManager.ScoreFactor)
		{
			if (MyLayer.ScrollFactor != Vector2.One)
			{
				MyLevel.AddScoreParticle(num, Position + MyCamera.Position * (Vector2.One - MyLayer.ScrollFactor));
				return;
			}
			if ((float)num < 2000f * FactorManager.ScoreFactor)
			{
				MyLevel.AddScoreParticle(num, Position);
				return;
			}
			string text = TextBundle.GetText("hud_warning_score");
			text = text.Replace("@score", Utils.FormatBigNumber(num, keepSmallDecimal: false));
			MyLevel.AddWarningText(text, new Color(255, 255, 0));
		}
	}

	public void IncreaseLevelStatsAsKill()
	{
		GameObject mainPart = GetMainPart();
		if (!mainPart.IsInLevelStats)
		{
			return;
		}
		AddScore();
		if (IsMainPart)
		{
			AchievementCountTypeEnum achievementKillCountType = AchievementManager.GetAchievementKillCountType(mainPart);
			if (achievementKillCountType != AchievementCountTypeEnum.Unknown)
			{
				GameJnG.AchievementManager.RaiseLevelAchievedCount(achievementKillCountType);
			}
			IsInLevelStats = false;
		}
	}

	public void Kill()
	{
		if (!IsDestroyed && !IsKilled)
		{
			IsKilled = true;
			CurrentHP = 0f;
			IncreaseLevelStatsAsKill();
			try
			{
				OnKill?.Invoke();
			}
			catch (Exception ex)
			{
				GameCore.LogErr("OnKill script exception: {0} in object '{1}'", ex, Id);
			}
			MyLevel.DoOnObjectKill(this);
			if (CurrentHP > 0f && !IsMainPart)
			{
				GetMainPart().Hit(CurrentHP, LastDamageType, LastHittingObject);
			}
			if (!IsDetached && PartData.KillChildren)
			{
				KillChildren();
			}
			CancelHitEffectWithChildren(this);
			if (LastHittingObject != null && LastHittingObject.MyHero != null)
			{
				LastHittingObject.MyHero.AddMana(1f, LastHittingObject.WorldPosition, bTextParticle: false);
			}
			if (_destroyOnKill)
			{
				Destroy();
			}
		}
	}

	public void KillChildren()
	{
		if (ChildObjectListFront != null)
		{
			for (int num = ChildObjectListFront.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = ChildObjectListFront[num];
				if (gameObject.PartData == null || (!gameObject.PartData.DetachOnParentDestroy && gameObject.PartData.KillWithParent))
				{
					gameObject.Kill();
				}
			}
		}
		if (ChildObjectListBack == null)
		{
			return;
		}
		for (int num2 = ChildObjectListBack.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject2 = ChildObjectListBack[num2];
			if (gameObject2.PartData == null || (!gameObject2.PartData.DetachOnParentDestroy && gameObject2.PartData.KillWithParent))
			{
				gameObject2.Kill();
			}
		}
	}

	private void DrawRestore(SpriteBatch spriteBatch)
	{
		GameCore.EffectLight.SetLightFactor(MyLayer, PartData.LightFactor);
		GameCore.EffectLight.SetHitFactor(HitEffectTimer * Options.HitEffectFactor);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, GameCore.RasterizerState, GameCore.GetRenderEffect(MyLayer), DrawMatrix);
	}

	public virtual void Draw(SpriteBatch spriteBatch)
	{
		if (!IsEnabled || !IsVisible || !IsInLOD || (!GameCore.IsEditorEnabled && !IsTriggerFree && TriggerPointMode == TriggerPointModeEnum.Hidden))
		{
			return;
		}
		DebugInfo.CurrentLayer.ObjectDraws++;
		if (_stripLineListBack != null && _stripLineListBack.Count > 0)
		{
			GameCore.TriangleBatchTextured.Begin(BlendState.AlphaBlend, DrawMatrix, SamplerState.LinearWrap, 0f);
			for (int num = _stripLineListBack.Count - 1; num >= 0; num--)
			{
				StripLine stripLine = _stripLineListBack[num];
				if (!stripLine.IsDestroyed)
				{
					stripLine.Draw(GameCore.TriangleBatchTextured);
					DebugInfo.CurrentLayer.ObjectStrips++;
				}
				else
				{
					GameCore.Allocator.DeleteStripLine(stripLine);
					_stripLineListBack.RemoveAt(num);
				}
			}
			GameCore.TriangleBatchTextured.End();
		}
		if (ParticleListBack != null)
		{
			Particle.DrawParticleList(MyLayer, ParticleListBack);
			DebugInfo.CurrentLayer.ObjectParticleLists++;
			if (PartData != null)
			{
				GameCore.EffectLight.SetLightFactor(MyLayer, PartData.LightFactor);
			}
		}
		if (ChildObjectListBack != null)
		{
			foreach (GameObject item in ChildObjectListBack)
			{
				item.Draw(spriteBatch);
			}
		}
		if (PolygonRenderer != null)
		{
			spriteBatch.End();
			GameCore.TriangleBatchTextured.End();
			GameCore.TriangleBatchTextured.Begin(DataUtils.BlendMode2State(PartData.BlendMode), DrawMatrix, PolygonRenderer.Data.SamplerState, PartData.LightFactor);
			GameCore.EffectLightBasic.SetHitFactor(HitEffectTimer * Options.HitEffectFactor);
			if (GameCore.IsEditorEnabled && GameCore.Editor.DarkDisabledEditorSets && PartData != null && !GameCore.Editor.IsEditorSetEnabled(PartData.EditorSet))
			{
				PolygonRenderer.SetColor(Color.R / 3, Color.G / 3, Color.B / 3, Color.A);
			}
			else
			{
				PolygonRenderer.SetColor(Color.R, Color.G, Color.B, Color.A);
			}
			PolygonRenderer.Draw(GameCore.TriangleBatchTextured);
			DebugInfo.CurrentLayer.ObjectPolygons++;
			GameCore.TriangleBatchTextured.End();
			DrawRestore(spriteBatch);
		}
		if (SpineRenderer != null)
		{
			spriteBatch.End();
			if (GameCore.IsEditorEnabled && GameCore.Editor.DarkDisabledEditorSets && PartData != null && !GameCore.Editor.IsEditorSetEnabled(PartData.EditorSet))
			{
				SpineRenderer.SetColor(Color.R / 3, Color.G / 3, Color.B / 3, Color.A);
			}
			else
			{
				SpineRenderer.SetColor(Color.R, Color.G, Color.B, Color.A);
			}
			SpineRenderer.Draw(WorldMatrix, PartData.LightFactor);
			DebugInfo.CurrentLayer.ObjectSpines++;
			DrawRestore(spriteBatch);
		}
		if (PartData != null && PartData.LightOrShadow == 0 && SpriteRenderer != null)
		{
			if (GameCore.IsEditorEnabled && GameCore.Editor.DarkDisabledEditorSets && !GameCore.Editor.IsEditorSetEnabled(PartData.EditorSet))
			{
				SpriteRenderer.Color = new Color(Color.R / 3, Color.G / 3, Color.B / 3, Color.A);
			}
			else
			{
				SpriteRenderer.Color = Color;
			}
			if ((HitEffectTimer != GameCore.EffectLight.HitFactor || PartData.LightFactor != GameCore.EffectLight.LightFactor || GameCore.EffectLight.ShadowFactor != MyLayer.ShadowMapFactor * GameCore.EffectLight.LightFactor) && SpriteRenderer.Atlas.GetCurentBlendMode(spriteBatch, PartData.BlendMode) != BlendModeEnum.Additive)
			{
				spriteBatch.End();
				DrawRestore(spriteBatch);
			}
			SpriteRenderer.Draw(WorldTransformation, spriteBatch, PartData.BlendMode);
			DebugInfo.CurrentLayer.ObjectSprites++;
		}
		OnDraw?.Invoke(spriteBatch);
		if (IsMainPart && ChildObjectListFront != null)
		{
			foreach (GameObject item2 in ChildObjectListFront)
			{
				item2.Draw(spriteBatch);
			}
		}
		if (_stripLineListFront != null && _stripLineListFront.Count > 0)
		{
			GameCore.TriangleBatchTextured.Begin(BlendState.AlphaBlend, DrawMatrix, SamplerState.LinearWrap, 0f);
			for (int num2 = _stripLineListFront.Count - 1; num2 >= 0; num2--)
			{
				StripLine stripLine2 = _stripLineListFront[num2];
				if (!stripLine2.IsDestroyed)
				{
					stripLine2.Draw(GameCore.TriangleBatchTextured);
					DebugInfo.CurrentLayer.ObjectStrips++;
				}
				else
				{
					GameCore.Allocator.DeleteStripLine(stripLine2);
					_stripLineListFront.RemoveAt(num2);
				}
			}
			GameCore.TriangleBatchTextured.End();
		}
		if (ParticleListFront != null && ParticleListFront.Count > 0)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, GameCore.RasterizerState, GameCore.GetRenderEffect(MyLayer), DrawMatrix);
			Particle.DrawParticleList(MyLayer, ParticleListFront);
			DebugInfo.CurrentLayer.ObjectParticleLists++;
		}
		if (IsMainPart || ChildObjectListFront == null)
		{
			return;
		}
		foreach (GameObject item3 in ChildObjectListFront)
		{
			item3.Draw(spriteBatch);
		}
	}

	public void IgnoreCollisionWith(GameObject go)
	{
		if (RigidBody != null && go != null && go.RigidBody != null)
		{
			RigidBody.IgnoreCollisionWith(go);
		}
	}

	public void RestoreCollisionWith(GameObject go)
	{
		if (RigidBody != null && go != null && go.RigidBody != null)
		{
			RigidBody.RestoreCollisionWith(go);
		}
	}

	public void RestoreCollisionWithAll()
	{
		if (RigidBody != null)
		{
			RigidBody.RestoreCollisionWithAll();
		}
	}

	public void SetColor(int r, int g, int b, int a = 255)
	{
		SetColor(new Color(r, g, b, a));
	}

	public void SetColor(Color color)
	{
		Color = color;
		if (SpineRenderer != null)
		{
			SpineRenderer.SetColor(Color.R, Color.G, Color.B, Color.A);
		}
	}

	public void SetColor(Color color, int alpha)
	{
		Color = new Color(color, alpha);
		if (SpineRenderer != null)
		{
			SpineRenderer.SetColor(Color.R, Color.G, Color.B, alpha);
		}
	}

	public void SetAlphaToAll(byte a)
	{
		SetColor(new Color(Color.R, Color.G, Color.B, a));
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].SetAlphaToAll(a);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].SetAlphaToAll(a);
			}
		}
	}

	public void SetColorToAll(int? r, int? g, int? b, int? a = null)
	{
		SetColor(new Color(r ?? Color.R, g ?? Color.G, b ?? Color.B, a ?? Color.A));
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].SetColorToAll(r, g, b, a);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].SetColorToAll(r, g, b, a);
			}
		}
	}

	public void SetColor(int? r, int? g, int? b, int? a = null)
	{
		SetColor(new Color(r ?? Color.R, g ?? Color.G, b ?? Color.B, a ?? Color.A));
	}

	public void ChangeColor(int deltaR, int deltaG, int deltaB, int deltaA)
	{
		SetColor((byte)Utils.Clamp(Color.R + deltaR, 0, 255), (byte)Utils.Clamp(Color.G + deltaG, 0, 255), (byte)Utils.Clamp(Color.B + deltaB, 0, 255), (byte)Utils.Clamp(Color.A + deltaA, 0, 255));
	}

	public Vector2 GetWorldOffset(float x, float y)
	{
		return Vector2.Transform(new Vector2(x, y), WorldMatrix);
	}

	public Vector2 GetTargetWorldPosition()
	{
		return Vector2.Transform(PartData.TargetOffset, WorldMatrix);
	}

	public string GetHierarchyId()
	{
		string text = Id;
		for (GameObject parentObject = ParentObject; parentObject != null; parentObject = parentObject.ParentObject)
		{
			if (parentObject.PartData != null)
			{
				text = parentObject.Id + "/" + text;
			}
		}
		return text;
	}

	public void SwapFlip(bool horizontal, bool vertical)
	{
		SetScale(Scale.X * (float)((!horizontal) ? 1 : (-1)), Scale.Y * (float)((!vertical) ? 1 : (-1)));
	}

	public void SetFlip(bool horizontal, bool vertical)
	{
		SetScale(Math.Abs(Scale.X) * (float)((!horizontal) ? 1 : (-1)), Math.Abs(Scale.Y) * (float)((!vertical) ? 1 : (-1)));
	}

	public void UpdateFrameRectByPolygon()
	{
		_ = PolygonRenderer;
	}

	public void SetMaxHPByData()
	{
		if (PartData.MaxHP <= 0)
		{
			MaxHP = (int)(GetMainPart().MaxHP * PartData.HPMainPercentage);
		}
		else
		{
			MaxHP = PartData.MaxHP;
		}
	}

	public void SetLayer(Layer layer)
	{
		MyLayer = layer;
		if (ChildObjectListFront != null)
		{
			int count = ChildObjectListFront.Count;
			for (int i = 0; i < count; i++)
			{
				ChildObjectListFront[i].SetLayer(layer);
			}
		}
		if (ChildObjectListBack != null)
		{
			int count2 = ChildObjectListBack.Count;
			for (int j = 0; j < count2; j++)
			{
				ChildObjectListBack[j].SetLayer(layer);
			}
		}
	}

	public void ConnectBySpineData(BoneData spineBoneData)
	{
		GameObject gameObject = ((SpineRenderer == null || IsMainPart) ? this : ParentObject);
		while (gameObject != null && gameObject.SpineRenderer == null)
		{
			gameObject = gameObject.ParentObject;
		}
		if (gameObject != null)
		{
			gameObject.ConnectObjectToSpineBone(this, spineBoneData.Name);
			return;
		}
		if (SpineRenderer != null)
		{
			ConnectObjectToSpineBone(this, spineBoneData.Name);
			return;
		}
		GameCore.LogErr("Unable to connect to Spine bone '{0}'", spineBoneData.Name);
	}

	public static GameObject CreateByPartDataSimple(PartData data, bool bCreateRigidBody = true, GameObject parentObject = null, string baseId = null)
	{
		return CreateByPartData(data, parentObject, baseId, Vector2.Zero, Vector2.One, 0f, 0, null, bCreateRigidBody);
	}

	public static GameObject CreateByPartData(PartData data, GameObject parentObject, string baseId, Vector2 pos, Vector2 scale, float rotation, int zorder = 0, PartMap creatorMap = null, bool bCreateRigidBody = true, Layer layer = null)
	{
		GameObject gameObject = null;
		if (data == null)
		{
			GameCore.LogErr("Unable to create object by part data - data is null");
			return null;
		}
		gameObject = GameCore.Allocator.GetGameObject();
		gameObject.PartData = data;
		gameObject.IsRotationFixed = data.IsRotationFixed;
		if (data.SpineData != null)
		{
			gameObject.CreateSpineRenderer(data.SpineData);
		}
		if (parentObject != null)
		{
			gameObject.MyLayer = parentObject.MyLayer;
			gameObject.ParentObject = parentObject;
		}
		else if (layer != null)
		{
			gameObject.MyLayer = layer;
		}
		else
		{
			gameObject.MyLayer = GameCore.CurrentLevel.CurrentLayer;
		}
		if (data.LifeTime.From >= 0f)
		{
			gameObject.LifeTime = data.LifeTime.GetRandom();
		}
		gameObject.DifficultyRange = data.DifficultyRange;
		gameObject.DestroyOnCollision = data.DestroyOnCollision;
		gameObject.SetDestroyOnKill(data.DestroyOnKill, bOnlyUndefined: false);
		gameObject.PathSpeed = data.PathSpeed;
		gameObject.PathFollowMode = data.PathFollowMode;
		gameObject.ActiveMode = data.ActiveMode;
		gameObject.ColorMode = data.PartColorMode;
		gameObject.SetMaxHPByData();
		if (!gameObject.IsImmortal)
		{
			gameObject.CurrentHP = gameObject.MaxHP;
		}
		gameObject.Damage = data.Damage;
		gameObject.DamageType = data.DamageType;
		gameObject.HitOnCollision = data.HitOnCollision;
		if (data.EmitterData != null)
		{
			ParticleEmitter particleEmitter = gameObject.CreateEmitter(data.EmitterData);
			if (particleEmitter != null && data.EmitterParticleSpace != ParticleSpace.Undefined)
			{
				particleEmitter.Space = data.EmitterParticleSpace;
			}
		}
		_ = data.DefaultPartState;
		if (data.BehaviorDataList != null)
		{
			foreach (BehaviorData behaviorData in data.BehaviorDataList)
			{
				if (behaviorData.MyType != null && gameObject.AddBehaviorScript(behaviorData) == null)
				{
					GameCore.LogErr($"Unable to add behavior '{data.Id}' for object '{gameObject.Id}'");
				}
			}
		}
		if (data.PolygonData != null)
		{
			gameObject.CreatePolygonRenderer(data.PolygonData);
		}
		if (data.SpineBoneData != null)
		{
			gameObject.ConnectBySpineData(data.SpineBoneData);
		}
		gameObject.IsMovable = data.IsMovable;
		gameObject.PathSpeed = data.PathSpeed;
		gameObject.CreatePhysBody = bCreateRigidBody;
		gameObject.PhysClass = data.PhysClass;
		gameObject.TargetClass = data.TargetClass;
		gameObject.DestroyOutOfView = data.DestroyOutOfView;
		gameObject.DestroyOutOfLevel = data.DestroyOutOfLevel;
		if (data.FrameRect.Width > 0)
		{
			gameObject._framePivot = new Vector2(data.FrameRect.Width / 2, data.FrameRect.Height / 2);
		}
		gameObject.SetPosition(pos);
		gameObject.SetScale(scale);
		gameObject.SetRotation(rotation);
		gameObject.ZOrder = zorder;
		foreach (PartData child in data.ChildList)
		{
			gameObject.CreateChildByPartData(child, bCreateRigidBody);
		}
		if (GameCore.IsEditorEnabled && gameObject.PartData.EditorStateId != null && gameObject.PartData.PartStates.TryGetValue(gameObject.PartData.EditorStateId, out var value))
		{
			gameObject.ApplyPartState(value);
		}
		else if (gameObject.PartData.PartStates.TryGetValue(gameObject.PartData.InitStateId, out value))
		{
			gameObject.ApplyPartState(value);
		}
		else
		{
			GameCore.LogErr("Unable to find init state '{0}' for object '{1}'", gameObject.PartData.InitStateId, gameObject.Id);
			gameObject.ApplyPartState(gameObject.PartData.DefaultPartState);
		}
		gameObject.UpdateVisibilityRect();
		return gameObject;
	}

	public void CreateRigidBodyByPoygonRenderer()
	{
		if (PolygonRenderer != null && PartData.PolygonData != null && PartData.PolygonData.CreatePhysBody && CreatePhysBody && MyLayer != null && MyLayer.UsePhysics)
		{
			if (RigidBody != null)
			{
				RigidBody.RemoveFixtures();
			}
			else
			{
				CreateRigidBody();
			}
			for (int i = 0; i < PolygonRenderer.VertexList.Count - 3; i += 2)
			{
				Vertices vertices = new Vertices();
				vertices.Add(ConvertUnits.ToSimUnits(PolygonRenderer.VertexList[i].Position) * WorldScale);
				vertices.Add(ConvertUnits.ToSimUnits(PolygonRenderer.VertexList[i + 1].Position) * WorldScale);
				vertices.Add(ConvertUnits.ToSimUnits(PolygonRenderer.VertexList[i + 2].Position) * WorldScale);
				vertices.Add(ConvertUnits.ToSimUnits(PolygonRenderer.VertexList[i + 3].Position) * WorldScale);
				RigidBody.AddPolygon(vertices);
			}
		}
	}

	public void SwitchSharedChildren(GameObject toObject)
	{
		if (ChildObjectListFront != null)
		{
			for (int num = ChildObjectListFront.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = ChildObjectListFront[num];
				if (gameObject.PartData.IsSharedOnSwitch)
				{
					toObject.AddChildRelative(gameObject);
				}
			}
		}
		if (ChildObjectListBack == null)
		{
			return;
		}
		for (int num2 = ChildObjectListBack.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject2 = ChildObjectListBack[num2];
			if (gameObject2.PartData.IsSharedOnSwitch)
			{
				toObject.AddChildRelative(gameObject2, 0f, 0f, -1);
			}
		}
	}

	public void DetachChild(GameObject child, Layer toLayer, bool bRestoreClass)
	{
		child.ResetPrecalculations();
		Transformation worldTransformation = child.WorldTransformation;
		RemoveChild(child);
		child.SetPhysicsTransformation(worldTransformation);
		if (bRestoreClass)
		{
			child.SetHitAndTargetClass(child.PartData.PhysClass, child.PartData.TargetClass);
		}
		child.IsKilled = false;
		if (toLayer != null)
		{
			toLayer.AddGameObject(child);
			toLayer.VisibleGameObjectList.Add(child);
		}
	}

	private void DetachChildInEditor(GameObject child)
	{
		child.ResetPrecalculations();
		Transformation physicsTransformation = new Transformation(child.WorldTransformation);
		RemoveChildImediately(child);
		child.SetPhysicsTransformation(physicsTransformation);
		child.SetHitAndTargetClass(child.PartData.PhysClass, child.PartData.TargetClass);
	}

	public void DetachChildrenInEditor()
	{
		if (ChildObjectListBack != null)
		{
			for (int num = ChildObjectListBack.Count - 1; num >= 0; num--)
			{
				DetachChildInEditor(ChildObjectListBack[num]);
			}
		}
		if (ChildObjectListFront != null)
		{
			for (int num2 = ChildObjectListFront.Count - 1; num2 >= 0; num2--)
			{
				DetachChildInEditor(ChildObjectListFront[num2]);
			}
		}
	}

	public void LogErr(string message, params object[] args)
	{
		GameCore.LogErr(string.Format("Error in object '{0}' in layer '{1}' at {2}, {3}: {4}", Id, (MyLayer != null) ? MyLayer.Id : "none", GetMainPart().Position.X, GetMainPart().Position.Y, message), args);
	}

	public static GameObject CreateSimilarObject(GameObject go, PartData partData, Layer layer, Vector2 newPosition)
	{
		GameObject gameObject = null;
		if (go.PartData != null)
		{
			gameObject = layer.CreateObjectByPartData(partData, newPosition, go.Scale, go.Rotation, go.ZOrder, go.CreatePhysBody);
		}
		if (gameObject != null)
		{
			gameObject.SetColor(go.Color);
			gameObject.IsLevelObject = go.IsLevelObject;
			gameObject.OldPosition = go.WorldPosition;
			gameObject.DifficultyRange = go.DifficultyRange;
			gameObject.ForPlayerCount = go.ForPlayerCount;
			gameObject.ZOrder = go.ZOrder;
			gameObject.TriggerPointMode = go.TriggerPointMode;
		}
		return gameObject;
	}

	public static GameObject CloneObject(GameObject go, Layer layer, Vector2 newPosition, bool bMovePath)
	{
		GameObject gameObject = CreateSimilarObject(go, go.PartData, layer, newPosition);
		if (gameObject != null)
		{
			if (go.MyPath != null)
			{
				gameObject.MyPath = go.MyPath.Clone(gameObject, -1L);
				if (bMovePath && go.MyPath.ControlPointList.Count > 0)
				{
					gameObject.MyPath.Move(gameObject.WorldTransformation.Position - go.MyPath.ControlPointList[0]);
				}
			}
			if (go.PolygonRenderer != null && !go.PolygonRenderer.IsAutogenerated)
			{
				gameObject.PolygonRenderer = go.PolygonRenderer.Copy(gameObject);
			}
			if (go.MyTimeLine != null)
			{
				gameObject.MyTimeLine = go.MyTimeLine.Clone(gameObject, -1L);
				gameObject.SetCurrentTimeLine(gameObject.MyTimeLine);
			}
			if (go.TriggerPoint != Vector2.Zero)
			{
				gameObject.TriggerPoint = go.TriggerPoint + gameObject.WorldPosition - go.WorldPosition;
			}
		}
		return gameObject;
	}

	public static void DebugPrint(GameObject go, int childLevel)
	{
		if (go != null)
		{
			GameCore.Log("{0}{1}", new string(' ', childLevel), go.Id);
			if (go.GetCurrentSpineRenderer() != null)
			{
				GameCore.Log("{0}  SR: {1}", new string(' ', childLevel), go.GetCurrentSpineRenderer().Skeleton.Data.Name);
			}
			go.ChildObjectListFront?.ForEach((GameObject child) =>
			{
				DebugPrint(child, childLevel + 1);
			});
			go.ChildObjectListBack?.ForEach((GameObject child) =>
			{
				DebugPrint(child, childLevel + 1);
			});
		}
	}

	public List<string> SaveToLines(int childLevel)
	{
		List<string> list = new List<string>();
		string text = "";
		for (int i = 0; i < childLevel; i++)
		{
			text += "\t";
		}
		if (IsMainPart)
		{
			list.Add($"[main={Id}]");
		}
		else
		{
			list.Add("");
			list.Add($"{text}[part={Id}]");
		}
		if (Emitter != null)
		{
			list.Add($"\t{text}emitter={Emitter.Data.Id}");
		}
		if (!PartData.BehaviorDataList.IsNullOrEmpty())
		{
			list.Add("");
			for (int j = 0; j < PartData.BehaviorDataList.Count; j++)
			{
				BehaviorData behaviorData = PartData.BehaviorDataList[j];
				list.Add($"\t{text}behavior={behaviorData.Id}");
				if (behaviorData.ParametersInternal == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, BehaviorData.ParamValue> item in behaviorData.ParametersInternal)
				{
					string text2 = "";
					if (item.Value._floats != null)
					{
						for (int k = 0; k < item.Value._floats.Length; k++)
						{
							if (k > 0)
							{
								text2 += ",";
							}
							text2 += item.Value._floats[k];
						}
					}
					else if (item.Value._strings != null)
					{
						for (int l = 0; l < item.Value._strings.Length; l++)
						{
							if (l > 0)
							{
								text2 += ",";
							}
							text2 += item.Value._strings[l];
						}
					}
					else
					{
						text2 = "???";
					}
					list.Add($"\t\t{text}${item.Key}={text2}");
				}
			}
		}
		if (ChildObjectListBack != null)
		{
			for (int m = 0; m < ChildObjectListBack.Count; m++)
			{
				list.AddRange(ChildObjectListBack[m].SaveToLines(childLevel + 1));
			}
		}
		if (ChildObjectListFront != null)
		{
			for (int n = 0; n < ChildObjectListFront.Count; n++)
			{
				list.AddRange(ChildObjectListFront[n].SaveToLines(childLevel + 1));
			}
		}
		list.Add($"{text}[end]");
		return list;
	}

	public override string ToString()
	{
		return Id;
	}
}
