using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;
using Rake2D.Script;
using Spine;

namespace Rake2D;

public class PartData
{
	public string Id;

	public GameContent MyContent;

	public PartData RootPart;

	public string DefaultStateName;

	public string InitStateId;

	public string TeplateId;

	public string FileName;

	public FloatRange LifeTime;

	public string SpineDataId;

	public PhysClass PhysClass = PhysClass.Undefined;

	public TargetClass TargetClass = TargetClass.Undefined;

	public long MaxHP;

	public long Damage;

	public bool UseDamageFactor;

	public float HPMainPercentage;

	public float PathSpeed = 100f;

	public float PathFront = -(float)Math.PI / 2f;

	public PathFollowModeEnum PathFollowMode = PathFollowModeEnum.FreeRotation;

	public int Hardness = 3;

	public int DefaultLayerNumber = 1;

	public float BehaviorRotation = -999f;

	public int SoundUpdateMode;

	public bool PlaySoundInEditor = true;

	public bool DoLinkedListCheck = true;

	public int LightOrShadow;

	public DamageTypeEnum DamageType = DamageTypeEnum.Collision;

	public bool HitOnCollision = true;

	public bool IsSharedOnSwitch;

	public bool IgnoreShock;

	public bool IgnoreParentState;

	public bool IgnoreParentHitEffect;

	public bool IsTargetPart;

	public bool IsAlwaysInStats;

	public int MinLOD;

	public bool ShowDebugInfo;

	public Vector2 DebugOffset;

	public string HitPartId;

	public int SkipExpired = -1;

	public IntRange DifficultyRange;

	public float LightFactor;

	public float FxMass = 250f;

	public Vector2 TargetOffset;

	public HashSet<string> ListedStateSet;

	public string EditorStateId;

	public float RemconHackSpeedFactor = 1f;

	public int RemconLevel;

	private string _editorName;

	public int EditorSet;

	public int EditorGizmo;

	public Color EditorColor = Color.White;

	public Rectangle VisibilityRect;

	public Rectangle FrameRect;

	public Rectangle FxRect;

	public OutOfFrame DestroyOutOfView = OutOfFrame.Undefined;

	public OutOfFrame DestroyOutOfLevel;

	public BlendModeEnum BlendMode = BlendModeEnum.Default;

	public int Score = -9999;

	public int AutolinkMode;

	public PolygonData PolygonData;

	public List<BehaviorData> BehaviorDataList;

	public int PhysType;

	public float PhysMass = 1f;

	public float PhysRestitution;

	public float PhysFriction = 0.5f;

	public float PhysLinearDamping;

	public float PhysAngularDamping;

	public float PhysGravityScale = 1f;

	public List<Vector4> PhysRectangleList;

	public List<Vector3> PhysCircleList;

	public List<float[]> PhysPolygonList;

	public int PhysOnCollisionEnabled = -1;

	public bool PhysFixedRotation;

	public bool PhysFixedScale;

	public bool PhysIsSensor;

	public bool PhysEvent;

	public bool PhysOnActivate;

	public bool IgnoreWater;

	public bool UsePhysData;

	public Vector2 PhysLocalCenter;

	public bool IsMovable;

	public bool IsBullet;

	public bool PhysSleepingAllowed = true;

	public bool DestroyOnKill = true;

	public bool IsDestroyOnKillDefined;

	public bool DestroyOnCollision;

	public bool DestroyOutOfAction;

	public bool KillChildren = true;

	public bool MoveOnKill;

	public int DestroyOnPathEndMode;

	public ActiveModeEnum ActiveMode;

	public string SkinId;

	public string SpineBoneId;

	public BoneData SpineBoneData;

	public int SpineTrack;

	public string EmitterDataId;

	public ParticleEmitterData EmitterData;

	public ParticleSpace EmitterParticleSpace = ParticleSpace.Undefined;

	public float HitEffectFactor;

	public ColorMode PartColorMode;

	public SpineData SpineData;

	public Dictionary<string, PartState> PartStates;

	public PartData ParentData;

	public List<PartData> ChildList;

	public bool DetachOnParentDestroy;

	public bool IsRotationFixed;

	public bool KillWithParent = true;

	public Dictionary<string, BehaviorEventType> AvailableEventTypes;

	public bool IsInitialised { get; private set; }

	public string EditorName
	{
		get
		{
			if (_editorName == null)
			{
				return Id;
			}
			return _editorName;
		}
		set
		{
			_editorName = value;
		}
	}

	public PartState DefaultPartState
	{
		get
		{
			try
			{
				return PartStates["idle"];
			}
			catch
			{
				return PartStates.Values.First();
			}
		}
	}

	private PartData()
	{
	}

	public PartData(string id)
	{
		Id = id;
		ChildList = new List<PartData>();
		DefaultStateName = null;
		CreatePartState("idle");
		LifeTime = new FloatRange(float.MaxValue);
		DifficultyRange = new IntRange(0, 999);
		LightFactor = 1f;
		PhysEvent = true;
		InitStateId = "idle";
	}

	public void CopyValuesFrom(PartData partData, bool mergeStates)
	{
		DefaultStateName = partData.DefaultStateName;
		LightOrShadow = partData.LightOrShadow;
		InitStateId = partData.InitStateId;
		LifeTime = partData.LifeTime;
		PhysClass = partData.PhysClass;
		TargetClass = partData.TargetClass;
		MaxHP = partData.MaxHP;
		HPMainPercentage = partData.HPMainPercentage;
		Damage = partData.Damage;
		DamageType = partData.DamageType;
		UseDamageFactor = partData.UseDamageFactor;
		Hardness = partData.Hardness;
		DefaultLayerNumber = partData.DefaultLayerNumber;
		BehaviorRotation = partData.BehaviorRotation;
		HitOnCollision = partData.HitOnCollision;
		HitEffectFactor = partData.HitEffectFactor;
		LightFactor = partData.LightFactor;
		FxMass = partData.FxMass;
		PathSpeed = partData.PathSpeed;
		PathFront = partData.PathFront;
		PathFollowMode = partData.PathFollowMode;
		PartColorMode = partData.PartColorMode;
		Score = partData.Score;
		AutolinkMode = partData.AutolinkMode;
		TargetOffset = partData.TargetOffset;
		IsSharedOnSwitch = partData.IsSharedOnSwitch;
		IsTargetPart = partData.IsTargetPart;
		IsAlwaysInStats = partData.IsAlwaysInStats;
		MinLOD = partData.MinLOD;
		SoundUpdateMode = partData.SoundUpdateMode;
		PlaySoundInEditor = partData.PlaySoundInEditor;
		DoLinkedListCheck = partData.DoLinkedListCheck;
		IgnoreShock = partData.IgnoreShock;
		IgnoreParentState = partData.IgnoreParentState;
		IgnoreParentHitEffect = partData.IgnoreParentHitEffect;
		ShowDebugInfo = partData.ShowDebugInfo;
		DebugOffset = partData.DebugOffset;
		RemconHackSpeedFactor = partData.RemconHackSpeedFactor;
		RemconLevel = partData.RemconLevel;
		HitPartId = partData.HitPartId;
		IsRotationFixed = partData.IsRotationFixed;
		KillWithParent = partData.KillWithParent;
		VisibilityRect = partData.VisibilityRect;
		FrameRect = partData.FrameRect;
		FxRect = partData.FxRect;
		if (partData.BehaviorDataList != null)
		{
			BehaviorDataList = new List<BehaviorData>();
			partData.BehaviorDataList.ForEach((BehaviorData o) =>
			{
				BehaviorDataList.Add((BehaviorData)o.Clone());
			});
		}
		if (partData.ListedStateSet != null)
		{
			ListedStateSet = new HashSet<string>(partData.ListedStateSet);
		}
		EditorStateId = partData.EditorStateId;
		DestroyOutOfView = partData.DestroyOutOfView;
		DestroyOnKill = partData.DestroyOnKill;
		IsDestroyOnKillDefined = partData.IsDestroyOnKillDefined;
		DestroyOutOfAction = partData.DestroyOutOfAction;
		DestroyOnCollision = partData.DestroyOnCollision;
		DestroyOutOfLevel = partData.DestroyOutOfLevel;
		DestroyOnPathEndMode = partData.DestroyOnPathEndMode;
		BlendMode = partData.BlendMode;
		MoveOnKill = partData.MoveOnKill;
		KillChildren = partData.KillChildren;
		if (partData.PolygonData != null)
		{
			PolygonData = partData.PolygonData.Copy();
		}
		PhysType = partData.PhysType;
		PhysMass = partData.PhysMass;
		PhysRestitution = partData.PhysRestitution;
		PhysFriction = partData.PhysFriction;
		PhysLinearDamping = partData.PhysLinearDamping;
		PhysAngularDamping = partData.PhysAngularDamping;
		PhysGravityScale = partData.PhysGravityScale;
		PhysSleepingAllowed = partData.PhysSleepingAllowed;
		IsBullet = partData.IsBullet;
		IsMovable = partData.IsMovable;
		PhysOnCollisionEnabled = partData.PhysOnCollisionEnabled;
		PhysFixedRotation = partData.PhysFixedRotation;
		PhysFixedScale = partData.PhysFixedScale;
		PhysIsSensor = partData.PhysIsSensor;
		PhysEvent = partData.PhysEvent;
		PhysOnActivate = partData.PhysOnActivate;
		SkipExpired = partData.SkipExpired;
		DifficultyRange = partData.DifficultyRange;
		IgnoreWater = partData.IgnoreWater;
		if (!UsePhysData)
		{
			UsePhysData = partData.UsePhysData;
		}
		PhysLocalCenter = partData.PhysLocalCenter;
		ActiveMode = partData.ActiveMode;
		EmitterDataId = partData.EmitterDataId;
		EmitterData = partData.EmitterData;
		EmitterParticleSpace = partData.EmitterParticleSpace;
		SpineDataId = partData.SpineDataId;
		SpineBoneId = partData.SpineBoneId;
		SpineBoneData = partData.SpineBoneData;
		SpineTrack = partData.SpineTrack;
		SkinId = partData.SkinId;
		SpineData = partData.SpineData;
		ParentData = partData.ParentData;
		DetachOnParentDestroy = partData.DetachOnParentDestroy;
		if (partData.PartStates != null)
		{
			if (mergeStates && PartStates != null)
			{
				foreach (KeyValuePair<string, PartState> partState2 in partData.PartStates)
				{
					PartState partState = FindPartState(partState2.Value.Id);
					if (partState != null)
					{
						PartStates[partState2.Value.Id.ToLowerLazy()] = partState.MergeCopy(partState2.Value);
					}
					else
					{
						PartStates.Add(partState2.Key.ToLowerLazy(), partState2.Value.Clone());
					}
				}
			}
			else
			{
				PartStates = new Dictionary<string, PartState>();
				foreach (KeyValuePair<string, PartState> partState3 in partData.PartStates)
				{
					PartStates.Add(partState3.Key.ToLowerLazy(), partState3.Value.Clone());
				}
			}
		}
		if (partData.ChildList != null)
		{
			if (ChildList == null)
			{
				ChildList = new List<PartData>();
			}
			foreach (PartData child in partData.ChildList)
			{
				PartData partData2 = new PartData(child.Id);
				partData2.CopyValuesFrom(child, mergeStates);
				partData2.ParentData = this;
				ChildList.Add(partData2);
			}
		}
		if (partData.PhysRectangleList != null)
		{
			PhysRectangleList = new List<Vector4>();
			partData.PhysRectangleList.ForEach((Vector4 o) =>
			{
				PhysRectangleList.Add(o);
			});
		}
		if (partData.PhysCircleList != null)
		{
			PhysCircleList = new List<Vector3>();
			partData.PhysCircleList.ForEach((Vector3 o) =>
			{
				PhysCircleList.Add(o);
			});
		}
		if (partData.PhysPolygonList != null)
		{
			PhysPolygonList = new List<float[]>();
			partData.PhysPolygonList.ForEach((float[] o) =>
			{
				float[] array = new float[o.Length];
				o.CopyTo(array, 0);
				PhysPolygonList.Add(array);
			});
		}
	}

	private bool CopyValuesAsPathTemplate(PartData originalData)
	{
		bool flag = false;
		if (originalData.BehaviorDataList != null)
		{
			BehaviorDataList = new List<BehaviorData>();
			foreach (BehaviorData behaviorData in originalData.BehaviorDataList)
			{
				if (behaviorData.Id.StartsWith("Path"))
				{
					BehaviorDataList.Add((BehaviorData)behaviorData.Clone());
					flag = true;
				}
			}
		}
		if (flag)
		{
			if (originalData.ListedStateSet != null)
			{
				ListedStateSet = new HashSet<string>(originalData.ListedStateSet);
			}
			if (originalData.PartStates != null)
			{
				PartStates = new Dictionary<string, PartState>();
				foreach (KeyValuePair<string, PartState> partState in originalData.PartStates)
				{
					PartStates.Add(partState.Key.ToLowerLazy(), new PartState(partState.Value.Id));
				}
			}
		}
		if (originalData.ChildList != null)
		{
			if (ChildList == null)
			{
				ChildList = new List<PartData>();
			}
			foreach (PartData child in originalData.ChildList)
			{
				PartData partData = new PartData(child.Id);
				if (partData.CopyValuesAsPathTemplate(child))
				{
					partData.ParentData = this;
					ChildList.Add(partData);
					flag = true;
				}
			}
		}
		return flag;
	}

	private float GetBoneDataAngle(BoneData boneData)
	{
		float num = boneData.Rotation;
		if (boneData.Parent != null)
		{
			num += GetBoneDataAngle(boneData.Parent);
		}
		return num;
	}

	private Vector2 GetBoneDataOffset(BoneData boneData)
	{
		Vector2 vector = new Vector2(boneData.X, 0f - boneData.Y);
		if (boneData.Parent != null)
		{
			return vector - GetBoneDataOffset(boneData.Parent);
		}
		return vector;
	}

	private void SetFrameRectByPhys()
	{
		Vector2 zero = Vector2.Zero;
		Vector2 zero2 = Vector2.Zero;
		if (PhysRectangleList != null)
		{
			foreach (Vector4 physRectangle in PhysRectangleList)
			{
				zero.X = Math.Min(zero.X, physRectangle.Z - physRectangle.X - physRectangle.Z);
				zero.Y = Math.Min(zero.Y, physRectangle.W - physRectangle.Y - physRectangle.W);
				zero2.X = Math.Max(zero2.X, physRectangle.Z - physRectangle.X);
				zero2.Y = Math.Max(zero2.Y, physRectangle.W - physRectangle.Y);
			}
		}
		if (PhysCircleList != null)
		{
			foreach (Vector3 physCircle in PhysCircleList)
			{
				zero.X = Math.Min(zero.X, physCircle.X - physCircle.Z);
				zero.Y = Math.Min(zero.Y, physCircle.Y - physCircle.Z);
				zero2.X = Math.Max(zero2.X, physCircle.X + physCircle.Z);
				zero2.Y = Math.Max(zero2.Y, physCircle.Y + physCircle.Z);
			}
		}
		if (PhysPolygonList != null)
		{
			foreach (float[] physPolygon in PhysPolygonList)
			{
				if ((physPolygon.Length > 1) & (physPolygon.Length % 2 == 0))
				{
					for (int i = 0; i < physPolygon.Length; i += 2)
					{
						float val = physPolygon[i];
						float val2 = physPolygon[i + 1];
						zero.X = Math.Min(zero.X, val);
						zero.Y = Math.Min(zero.Y, val2);
						zero2.X = Math.Max(zero2.X, val);
						zero2.Y = Math.Max(zero2.Y, val2);
					}
				}
			}
		}
		int width = (int)(zero2.X - zero.X);
		int height = (int)(zero2.Y - zero.Y);
		FrameRect = new Rectangle((int)zero.X, (int)zero.Y, width, height);
	}

	public static BoneData FindBoneDataByName(string name, PartData partData)
	{
		if (partData == null)
		{
			return null;
		}
		BoneData boneData = null;
		if (partData.SpineData != null && partData.SpineData.SkeletonData != null)
		{
			boneData = partData.SpineData.SkeletonData.FindBone(name);
			if (boneData != null)
			{
				return boneData;
			}
		}
		if (partData.ChildList != null)
		{
			foreach (PartData child in partData.ChildList)
			{
				boneData = FindBoneDataByName(name, child);
				if (boneData != null)
				{
					return boneData;
				}
			}
		}
		return null;
	}

	private void LogErr(string message)
	{
		GameCore.LogErr("{0} for part data [{1}] in file '{2}'.", message, GetHierarchyId(), FileName);
	}

	public void Init()
	{
		if (PartStates != null)
		{
			foreach (KeyValuePair<string, PartState> partState in PartStates)
			{
				PartState value = partState.Value;
				if (value.AtlasId != null)
				{
					value.Atlas = GameCore.GetSpriteAtlas(value.AtlasId);
					if (value.Atlas == null)
					{
						LogErr($"Atlas '{value.AtlasId}' not found");
					}
				}
				else
				{
					value.Atlas = null;
				}
				if (value.AnimId == null)
				{
					continue;
				}
				if (value.Atlas != null)
				{
					if (value.Atlas.AnimFrames.IsNullOrEmpty() || !value.Atlas.AnimFrames.ContainsKey(value.AnimId))
					{
						LogErr($"Anim '{value.AnimId}' not defined in Atlas '{value.AtlasId}' in state '{partState.Key}'");
					}
				}
				else
				{
					LogErr($"Anim '{value.AnimId}' has undefined Atlas in state '{partState.Key}'");
				}
			}
		}
		if (SpineDataId != null)
		{
			SpineData spineData = GameCore.GetSpineData(SpineDataId);
			if (spineData == null)
			{
				LogErr($"Spine '{SpineDataId}' not found");
			}
			SpineData = spineData;
		}
		if (SpineBoneData == null && SpineBoneId != null)
		{
			SpineBoneData = FindBoneDataByName(SpineBoneId, GetMainPartData());
			if (SpineBoneData == null)
			{
				LogErr($"Unable to find spine_bone = {SpineBoneId}");
			}
		}
		InitBehaviorData();
		if (EmitterDataId != null)
		{
			EmitterData = GameCore.GetParticleEmitterData(EmitterDataId);
			if (EmitterData == null)
			{
				LogErr($"Unable to find emmitter '{EmitterDataId}'");
			}
		}
		if (PolygonData != null)
		{
			PolygonData.Init(this);
		}
		if (Score < 0)
		{
			if (ParentData == null)
			{
				if (MaxHP >= 1000)
				{
					Score = 1000;
				}
				else if (MaxHP >= 500)
				{
					Score = 500;
				}
				else if (MaxHP >= 250)
				{
					Score = 250;
				}
				else if (MaxHP >= 100)
				{
					Score = 100;
				}
				else if (MaxHP >= 50)
				{
					Score = 50;
				}
				else
				{
					Score = 0;
				}
			}
			else
			{
				Score = 0;
			}
		}
		if (TargetClass == TargetClass.Undefined)
		{
			TargetClass = DataUtils.GetTargetClassByPhysClass(PhysClass);
		}
		else if (PhysClass == PhysClass.Undefined)
		{
			PhysClass = DataUtils.GetPhysClassByTargetClass(TargetClass);
		}
		if (FxRect.IsEmpty)
		{
			FxRect = FrameRect;
		}
		if (ChildList != null)
		{
			ChildList = ChildList.OrderBy((PartData o) => o.DefaultPartState.ZOrder).ToList();
			ChildList.ForEach((PartData childData) =>
			{
				childData.Init();
			});
		}
		IsInitialised = true;
	}

	private Matrix GetSpineBoneDataMatrix(BoneData boneData)
	{
		Matrix matrix = Matrix.CreateScale(boneData.ScaleX, boneData.ScaleY, 1f) * Matrix.CreateRotationZ(MathHelper.ToRadians(boneData.Rotation)) * Matrix.CreateTranslation(0f - boneData.X, boneData.Y, 0f);
		if (boneData.Parent != null)
		{
			return matrix * GetSpineBoneDataMatrix(boneData.Parent);
		}
		return matrix;
	}

	public void InitBehaviorData()
	{
		if (BehaviorDataList == null)
		{
			return;
		}
		foreach (BehaviorData behaviorData in BehaviorDataList)
		{
			if (!(behaviorData.MyType != null))
			{
				continue;
			}
			TypeInfo typeInfo = behaviorData.MyType.GetTypeInfo();
			if (!Utils.Equals(typeInfo.Name, behaviorData.TypeName))
			{
				continue;
			}
			FieldInfo field = typeInfo.GetField("PathEvents", BindingFlags.Static | BindingFlags.Public);
			if (!(field != null))
			{
				continue;
			}
			string[] array = field.GetValue(null) as string[];
			foreach (string text in array)
			{
				if (AvailableEventTypes == null)
				{
					AvailableEventTypes = new Dictionary<string, BehaviorEventType>();
				}
				string text2 = ((ParentData == null) ? $"{behaviorData.Id}/{text}" : $"{Id}/{behaviorData.Id}/{text}");
				BehaviorEventTypeEnum type = (Utils.Equals(text, "State") ? BehaviorEventTypeEnum.State : BehaviorEventTypeEnum.Number);
				if (AvailableEventTypes.ContainsKey(text2))
				{
					GameCore.LogErr("Event '{0}' already added to Script class '{1}' part data '{2}'", text2, behaviorData.TypeName, GetHierarchyId());
				}
				else
				{
					AvailableEventTypes.Add(text2, new BehaviorEventType(text2, type));
				}
			}
		}
	}

	public PartData AddChild(string id)
	{
		PartData partData = new PartData(id);
		partData.ParentData = this;
		ChildList.Add(partData);
		return partData;
	}

	public void SetChild(PartData childData)
	{
		if (childData.ParentData != null)
		{
			childData.ParentData.ChildList.Remove(childData);
		}
		childData.ParentData = this;
		ChildList.Add(childData);
	}

	public PartData FindRootPart()
	{
		PartData result = this;
		for (PartData parentData = ParentData; parentData != null; parentData = parentData.ParentData)
		{
			result = parentData;
			if (parentData == parentData.ParentData)
			{
				parentData.ParentData = null;
				LogErr("FindRootPart: Parent = child warning");
				break;
			}
		}
		return result;
	}

	public PartData FindChild(string id)
	{
		if (Utils.Equals(Id, id))
		{
			return this;
		}
		foreach (PartData child in ChildList)
		{
			PartData partData = child.FindChild(id);
			if (partData != null)
			{
				return partData;
			}
		}
		return null;
	}

	public bool RemoveChild(PartData removePart)
	{
		for (int num = ChildList.Count - 1; num >= 0; num--)
		{
			PartData partData = ChildList[num];
			if (removePart == partData)
			{
				ChildList.RemoveAt(num);
				return true;
			}
			if (partData.RemoveChild(removePart))
			{
				return true;
			}
		}
		return false;
	}

	public PartData FindChildOnPath(string path)
	{
		string[] array = path.Split('/');
		PartData partData = this;
		PartData partData2 = null;
		string[] array2 = array;
		foreach (string value in array2)
		{
			partData2 = null;
			if (partData.ChildList != null)
			{
				foreach (PartData child in partData.ChildList)
				{
					if (Utils.Equals(child.Id, value))
					{
						partData2 = child;
						break;
					}
				}
			}
			if (partData2 == null)
			{
				break;
			}
			partData = partData2;
		}
		return partData2;
	}

	public PartState FindPartState(string id)
	{
		if (PartStates != null)
		{
			foreach (PartState value in PartStates.Values)
			{
				if (Utils.Equals(value.Id, id))
				{
					return value;
				}
			}
		}
		return null;
	}

	public string GetHierarchyId()
	{
		string text = Id;
		for (PartData parentData = ParentData; parentData != null; parentData = parentData.ParentData)
		{
			text = parentData.Id + "/" + text;
			if (parentData == parentData.ParentData)
			{
				GameCore.LogErr($"GetHierarchyId: Parent = child warning part '{parentData.Id}'");
				break;
			}
		}
		return text;
	}

	public PartState CreatePartState(string id)
	{
		bool flag = false;
		PartState value = null;
		if (PartStates == null)
		{
			PartStates = new Dictionary<string, PartState>();
		}
		else
		{
			flag = PartStates.TryGetValue(id, out value);
		}
		if (!flag)
		{
			value = new PartState(id);
			PartStates.Add(id.ToLowerLazy(), value);
		}
		return value;
	}

	private void BuildPhysPolyByBoneData(SkeletonData skeletonData, BoneData boneData)
	{
		foreach (SlotData item in skeletonData.Slots.FindAll((SlotData s) => s.BoneData == boneData && s.AttachmentName != null))
		{
			if (skeletonData.Skins.First().GetAttachment(item.Index, item.AttachmentName) is BoundingBoxAttachment { Vertices: not null } boundingBoxAttachment && ((boundingBoxAttachment.Vertices.Length > 1) & (boundingBoxAttachment.Vertices.Length % 2 == 0)))
			{
				Matrix matrix = Matrix.CreateRotationZ(MathHelper.ToRadians(90f));
				float[] array = new float[boundingBoxAttachment.Vertices.Length];
				boundingBoxAttachment.Vertices.CopyTo(array, 0);
				for (int num = 0; num < array.Length; num += 2)
				{
					Vector2 vector = Vector2.Transform(new Vector2(array[num], array[num + 1]), matrix);
					array[num] = vector.X;
					array[num + 1] = vector.Y * -1f;
				}
				if (PhysPolygonList == null)
				{
					PhysPolygonList = new List<float[]>();
				}
				PhysPolygonList.Add(array);
				UsePhysData = true;
			}
		}
	}

	private void BuildBySkeletonData(SkeletonData skeletonData, BoneData myBoneData)
	{
		SpineBoneData = myBoneData;
		string text = ((myBoneData.Parent != null) ? $"{Id}/idle" : "idle");
		if (skeletonData.FindAnimation(text) != null)
		{
			DefaultPartState.SpineAnimId = text;
			DefaultPartState.UseSpineAnim = true;
		}
		BuildPhysPolyByBoneData(skeletonData, myBoneData);
		foreach (BoneData bone in skeletonData.Bones)
		{
			if (bone.Parent == myBoneData)
			{
				PartData partData = FindRootPart();
				PartData partData2 = partData.FindChild(bone.Name);
				if (partData == partData2)
				{
					GameCore.LogErr("Bone name '{0}' is same as root part.", bone.Name);
					partData2 = AddChild(bone.Name);
				}
				else if (partData2 == null)
				{
					partData2 = AddChild(bone.Name);
				}
				else
				{
					SetChild(partData2);
				}
				partData2.BuildBySkeletonData(skeletonData, bone);
			}
		}
	}

	private void BuildBySpine(string spineId)
	{
		SpineData spineData = GameCore.GetSpineData(spineId);
		if (spineData == null)
		{
			return;
		}
		if (!spineData.IsInitialized)
		{
			spineData.Init();
		}
		if (spineData.SkeletonData != null)
		{
			BoneData boneData = spineData.SkeletonData.Bones.First();
			if (boneData.Parent == null)
			{
				SpineData = spineData;
				BuildBySkeletonData(spineData.SkeletonData, boneData);
			}
		}
	}

	public PartData GetMainPartData()
	{
		PartData partData = this;
		while (partData.ParentData != null)
		{
			partData = partData.ParentData;
		}
		return partData;
	}

	internal static bool TemplateByIniFile(PartData currentData, IniFile iniFile)
	{
		string text = iniFile.Id;
		if (text != null)
		{
			PartData partData;
			if (text.StartsWith("main/"))
			{
				text = text.Substring(5);
				partData = currentData.FindRootPart().FindChild(text);
			}
			else
			{
				partData = GameCore.GetPartData(text);
			}
			if (partData == null)
			{
				iniFile.LogErr("Unable to find [Main = {0}] for template of part '{1}'", text, currentData.Id);
				return false;
			}
			if (partData == currentData)
			{
				iniFile.LogErr("Unable to teplate from myself '{0}'", text);
				return false;
			}
			BoneData spineBoneData = currentData.SpineBoneData;
			PartData parentData = currentData.ParentData;
			currentData.CopyValuesFrom(partData, mergeStates: false);
			currentData.TeplateId = partData.Id;
			currentData.ParentData = parentData;
			currentData.SpineBoneData = spineBoneData;
		}
		return true;
	}

	internal BehaviorData GetBehaviorDataById(string id)
	{
		return BehaviorDataList?.Find((BehaviorData o) => Utils.Equals(o.Id, id));
	}

	private void SetUsePolygonData(bool bUse)
	{
		if (bUse)
		{
			if (PolygonData == null)
			{
				PolygonData = new PolygonData();
			}
		}
		else
		{
			PolygonData = null;
		}
	}

	private List<PartState> GetPartStateList(IniFile iniFile, PartData partData)
	{
		string[] idStringArray = iniFile.GetIdStringArray();
		List<PartState> list = new List<PartState>();
		if (idStringArray.IsNullOrEmpty())
		{
			return partData.PartStates.Values.ToList();
		}
		string[] array = idStringArray;
		foreach (string text in array)
		{
			if (!partData.PartStates.TryGetValue(text.ToLowerLazy(), out var value))
			{
				value = new PartState(text);
				partData.PartStates.Add(value.Id.ToLowerLazy(), value);
			}
			list.Add(value);
		}
		return list;
	}

	private void AddPartStateIdArray(string[] idArray, PartData partData)
	{
		foreach (string text in idArray)
		{
			if (!partData.PartStates.ContainsKey(text.ToLowerLazy()))
			{
				PartState partState = new PartState(text);
				partData.PartStates.Add(partState.Id.ToLowerLazy(), partState);
			}
		}
	}

	internal void LoadFromIniFile(IniFile iniFile, GameContent content)
	{
		BehaviorData behaviorData = null;
		PartData currentData = this;
		PartData partData = this;
		FileName = iniFile.FileName;
		TemplateByIniFile(currentData, iniFile);
		bool flag = false;
		while (iniFile.NextLine())
		{
			if (DataLoader.CheckParams(iniFile, content))
			{
				continue;
			}
			if (iniFile.ValueType == IniFileValueType.BlockSection && iniFile.KeyEquals("/diff"))
			{
				flag = false;
			}
			else
			{
				if (flag)
				{
					continue;
				}
				int[] resultArray;
				if (iniFile.ValueType == IniFileValueType.BlockSection && iniFile.KeyEquals("diff"))
				{
					IntRange range = default;
					if (iniFile.TryGetIntRange(ref range) && (GameJnG.Difficulty < range.From || GameJnG.Difficulty > range.To))
					{
						flag = true;
					}
				}
				else if (iniFile.ValueType == IniFileValueType.BlockSection && iniFile.KeyEquals("diff_set") && iniFile.TryGetIntArray(1, 100, out resultArray))
				{
					flag = true;
					int[] array = resultArray;
					foreach (int num in array)
					{
						if (GameJnG.Difficulty == num)
						{
							flag = false;
							break;
						}
					}
				}
				else if (iniFile.ValueType == IniFileValueType.MainSection)
				{
					if (iniFile.KeyEquals("part"))
					{
						partData = currentData;
						currentData = currentData.AddChild(iniFile.Value);
						currentData.FileName = iniFile.FileName;
						TemplateByIniFile(currentData, iniFile);
						behaviorData = null;
						continue;
					}
					if (iniFile.KeyEquals("mod_part"))
					{
						partData = currentData;
						currentData.FileName = iniFile.FileName;
						PartData partData2 = currentData.FindRootPart();
						PartData partData3 = null;
						string value = iniFile.Value;
						int num2 = value.IndexOf('/');
						if (num2 >= 0)
						{
							string text = value.Substring(0, num2);
							value = value.Substring(num2 + 1);
							PartData partData4 = partData2.FindChild(text);
							if (partData4 != null)
							{
								partData3 = partData4.FindChild(value);
							}
							else
							{
								iniFile.LogErr("Unable to find parent [part = {0}]", text);
							}
						}
						else
						{
							partData3 = currentData.FindChild(iniFile.Value) ?? partData2.FindChild(iniFile.Value);
						}
						if (partData3 != null)
						{
							currentData = partData3;
							TemplateByIniFile(currentData, iniFile);
						}
						else
						{
							iniFile.LogErr($"Unable to find [mod_part = {iniFile.Value}] - create new part");
							currentData = currentData.AddChild(iniFile.Value);
							currentData.FileName = iniFile.FileName;
							TemplateByIniFile(currentData, iniFile);
						}
						behaviorData = null;
						continue;
					}
					if (!iniFile.KeyEquals("end"))
					{
						break;
					}
					if (currentData.ParentData != null)
					{
						currentData = partData;
						if (partData != null)
						{
							partData = partData.ParentData;
						}
					}
					behaviorData = null;
				}
				else
				{
					if (currentData == null)
					{
						continue;
					}
					Color color;
					float[] resultArray2;
					if (iniFile.KeyEquals("del_part"))
					{
						PartData partData5 = currentData.FindChild(iniFile.Value);
						if (partData5 != null)
						{
							currentData.RemoveChild(partData5);
							continue;
						}
						iniFile.LogErr("Unable to delete unefined part '{0}'", iniFile.Value);
					}
					else if (iniFile.KeyEquals("part_id"))
					{
						currentData.Id = iniFile.Value;
					}
					else if (iniFile.KeyEquals("default_state_name"))
					{
						currentData.DefaultStateName = iniFile.Value;
					}
					else if (iniFile.KeyEquals("init_state"))
					{
						AddPartStateIdArray(iniFile.GetStringArray(), currentData);
						currentData.InitStateId = iniFile.Value.ToLowerLazy();
					}
					else if (iniFile.KeyEquals("listed_state"))
					{
						AddPartStateIdArray(iniFile.GetStringArray(), currentData);
						currentData.ListedStateSet = new HashSet<string>(iniFile.GetStringArray());
					}
					else if (iniFile.KeyEquals("editor_state"))
					{
						string text2 = iniFile.Value.ToLowerLazy();
						if (!currentData.PartStates.TryGetValue(text2, out var _))
						{
							currentData.PartStates.Add(text2, new PartState(text2));
						}
						currentData.EditorStateId = text2;
					}
					else if (iniFile.KeyEquals("target_part"))
					{
						currentData.IsTargetPart = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("always_in_stats"))
					{
						currentData.IsAlwaysInStats = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("state"))
					{
						AddPartStateIdArray(iniFile.GetStringArray(), currentData);
					}
					else if (iniFile.KeyEquals("phys_enabled"))
					{
						foreach (PartState partState in GetPartStateList(iniFile, currentData))
						{
							partState.PhysEnabled = iniFile.GetBoolValue();
							partState.UsePhysEnabled = true;
						}
					}
					else if (iniFile.KeyEquals("path_paused"))
					{
						foreach (PartState partState2 in GetPartStateList(iniFile, currentData))
						{
							partState2.PathPaused = iniFile.GetBoolValue();
							partState2.UsePathPaused = true;
						}
					}
					else if (iniFile.KeyEquals("offset") && iniFile.TryGetIntArray(1, 2, out resultArray))
					{
						foreach (PartState partState3 in GetPartStateList(iniFile, currentData))
						{
							partState3.Offset.X = resultArray[0];
							if (resultArray.Length > 1)
							{
								partState3.Offset.Y = resultArray[1];
							}
							else
							{
								partState3.Offset.Y = partState3.Offset.X;
							}
							partState3.UseOffset = true;
						}
					}
					else if (iniFile.KeyEquals("Scale") && iniFile.TryGetIntArray(1, 2, out resultArray))
					{
						foreach (PartState partState4 in GetPartStateList(iniFile, currentData))
						{
							partState4.Scale.X = (float)resultArray[0] / 100f;
							if (resultArray.Length > 1)
							{
								partState4.Scale.Y = (float)resultArray[1] / 100f;
							}
							else
							{
								partState4.Scale.Y = partState4.Scale.X;
							}
							partState4.UseScale = true;
						}
					}
					else if (iniFile.KeyEquals("Color") && iniFile.TryGetColorValue(out color))
					{
						foreach (PartState partState5 in GetPartStateList(iniFile, currentData))
						{
							partState5.Color = color;
							partState5.UseColor = true;
						}
					}
					else if (iniFile.KeyEquals("alpha"))
					{
						foreach (PartState partState6 in GetPartStateList(iniFile, currentData))
						{
							partState6.Color.A = (byte)iniFile.GetIntValue();
							partState6.UseColor = true;
						}
					}
					else if (iniFile.KeyEquals("zorder"))
					{
						foreach (PartState partState7 in GetPartStateList(iniFile, currentData))
						{
							partState7.ZOrder = iniFile.GetIntValue();
							partState7.UseZOrder = true;
						}
					}
					else if (iniFile.KeyEquals("enabled"))
					{
						foreach (PartState partState8 in GetPartStateList(iniFile, currentData))
						{
							partState8.IsEnabled = iniFile.GetBoolValue();
							partState8.UseEnabled = true;
						}
					}
					else if (iniFile.KeyEquals("visible"))
					{
						foreach (PartState partState9 in GetPartStateList(iniFile, currentData))
						{
							partState9.IsVisible = iniFile.GetBoolValue();
							partState9.UseVisible = true;
						}
					}
					else if (iniFile.KeyEquals("immortal"))
					{
						foreach (PartState partState10 in GetPartStateList(iniFile, currentData))
						{
							partState10.IsImmortal = iniFile.GetBoolValue();
							partState10.UseImmortal = true;
						}
					}
					else if (iniFile.KeyEquals("emitter_reset"))
					{
						foreach (PartState partState11 in GetPartStateList(iniFile, currentData))
						{
							partState11.EmitterReset = iniFile.GetBoolValue();
							partState11.UseEmitterReset = true;
						}
					}
					else if (iniFile.KeyEquals("sound"))
					{
						foreach (PartState partState12 in GetPartStateList(iniFile, currentData))
						{
							partState12.SoundIdArray = iniFile.GetStringArray();
							partState12.UseSound = true;
						}
					}
					else if (iniFile.KeyEquals("sound_loop"))
					{
						foreach (PartState partState13 in GetPartStateList(iniFile, currentData))
						{
							partState13.IsSoundLooped = iniFile.GetBoolValue();
						}
					}
					else if (iniFile.KeyEquals("sound_update"))
					{
						currentData.SoundUpdateMode = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("sound_in_editor"))
					{
						currentData.PlaySoundInEditor = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("linked_list_check"))
					{
						currentData.DoLinkedListCheck = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("Anim"))
					{
						foreach (PartState partState14 in GetPartStateList(iniFile, currentData))
						{
							if (int.TryParse(iniFile.Value, out var result))
							{
								partState14.Frame = result;
								partState14.UseFrame = true;
							}
							else
							{
								partState14.AnimId = iniFile.Value;
								partState14.UseAnim = true;
							}
						}
					}
					else if (iniFile.KeyEquals("spine_anim"))
					{
						foreach (PartState partState15 in GetPartStateList(iniFile, currentData))
						{
							if (iniFile.ValueEquals("none"))
							{
								partState15.SpineAnimId = null;
								partState15.UseSpineAnim = true;
							}
							else
							{
								partState15.SpineAnimId = iniFile.Value;
								partState15.UseSpineAnim = true;
							}
						}
					}
					else if (iniFile.KeyEquals("anim_duration"))
					{
						foreach (PartState partState16 in GetPartStateList(iniFile, currentData))
						{
							partState16.AnimDuration = (float)iniFile.GetIntValue() / 1000f;
						}
					}
					else if (iniFile.KeyEquals("anim_loop"))
					{
						foreach (PartState partState17 in GetPartStateList(iniFile, currentData))
						{
							partState17.IsAnimLooped = iniFile.GetBoolValue();
						}
					}
					else if (iniFile.KeyEquals("angle"))
					{
						foreach (PartState partState18 in GetPartStateList(iniFile, currentData))
						{
							partState18.Rotation = Utils.DegToRad(iniFile.GetIntValue());
							partState18.UseRotation = true;
						}
					}
					else if (iniFile.KeyEquals("velocity"))
					{
						foreach (PartState partState19 in GetPartStateList(iniFile, currentData))
						{
							partState19.Velocity = iniFile.GetVector2();
							partState19.UseVelocity = true;
						}
					}
					else if (iniFile.KeyEquals("remcon_text"))
					{
						foreach (PartState partState20 in GetPartStateList(iniFile, currentData))
						{
							partState20.RemconText = iniFile.Value;
							partState20.UseRemconText = true;
						}
					}
					else if (iniFile.KeyEquals("atlas"))
					{
						foreach (PartState partState21 in GetPartStateList(iniFile, currentData))
						{
							if (Utils.Equals(iniFile.Value, "none"))
							{
								partState21.AtlasId = null;
								partState21.UseAtlas = true;
							}
							else
							{
								partState21.AtlasId = iniFile.Value;
								partState21.UseAtlas = true;
							}
						}
					}
					else if (iniFile.KeyEquals("path_template"))
					{
						PartData partData6 = GameCore.GetPartData(iniFile.Value);
						currentData.CopyValuesAsPathTemplate(partData6);
					}
					else if (iniFile.KeyEquals("detach"))
					{
						currentData.DetachOnParentDestroy = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("beh"))
					{
						behaviorData = new BehaviorData();
						if (behaviorData.SetTypeName(iniFile.Value) != null)
						{
							if (!string.IsNullOrEmpty(iniFile.Id))
							{
								behaviorData.AddPartStates(GetPartStateList(iniFile, currentData));
							}
							if (currentData.BehaviorDataList == null)
							{
								currentData.BehaviorDataList = new List<BehaviorData>();
							}
							currentData.BehaviorDataList.Add(behaviorData);
						}
						else
						{
							iniFile.LogErr("Undefined behavior class '{0}'", iniFile.Value);
							behaviorData = null;
						}
					}
					else if (iniFile.KeyEquals("beh_remove"))
					{
						bool flag2 = false;
						if (currentData.BehaviorDataList != null)
						{
							int count = currentData.BehaviorDataList.Count;
							currentData.BehaviorDataList?.RemoveAll((BehaviorData bd) => Utils.Equals(bd.Id, iniFile.Value));
							flag2 = currentData.BehaviorDataList.Count < count;
						}
						if (!flag2)
						{
							iniFile.LogErr("Unable to remove behavior '{0}'", iniFile.Value);
						}
					}
					else if (iniFile.KeyEquals("beh_angle"))
					{
						currentData.BehaviorRotation = Utils.DegToRad(iniFile.GetIntValue());
					}
					else if (iniFile.KeyEquals("target_offset"))
					{
						currentData.TargetOffset = iniFile.GetVector2();
					}
					else if (iniFile.KeyEquals("shared_on_switch"))
					{
						currentData.IsSharedOnSwitch = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("ignore_shock"))
					{
						currentData.IgnoreShock = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("ignore_parent_state"))
					{
						currentData.IgnoreParentState = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("ignore_parent_hit_effect"))
					{
						currentData.IgnoreParentHitEffect = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("lod"))
					{
						currentData.MinLOD = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("debug"))
					{
						currentData.ShowDebugInfo = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("debug_offset"))
					{
						currentData.DebugOffset = iniFile.GetVector2();
					}
					else if (iniFile.KeyEquals("skip_expired"))
					{
						currentData.SkipExpired = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("diff"))
					{
						if (!iniFile.TryGetIntRange(ref currentData.DifficultyRange))
						{
							currentData.DifficultyRange = new IntRange(0, 999);
						}
					}
					else if (iniFile.KeyEquals("beh_id"))
					{
						if (behaviorData != null)
						{
							behaviorData.Id = iniFile.Value;
						}
						else
						{
							iniFile.LogErr($"Undefined behavior for beh_id = {iniFile.Value}");
						}
					}
					else if (iniFile.Key.StartsWith("$") && iniFile.Key.Length > 1 && iniFile.Value.Length > 0)
					{
						BehaviorData behaviorData2;
						if (iniFile.Id != null)
						{
							behaviorData2 = currentData.GetBehaviorDataById(iniFile.Id);
							if (behaviorData2 == null)
							{
								iniFile.LogErr($"Undefined behavior '{iniFile.Id}' for param '{iniFile.Key}'");
								continue;
							}
						}
						else
						{
							behaviorData2 = behaviorData;
						}
						if (behaviorData2 != null)
						{
							string key = iniFile.Key.Substring(1);
							if (char.IsDigit(iniFile.Value[0]) || (iniFile.Value.Length > 1 && iniFile.Value[0] == '-' && char.IsDigit(iniFile.Value[1])))
							{
								behaviorData2.SetParam(key, iniFile.GetFloatArray());
							}
							else
							{
								behaviorData2.SetParam(key, iniFile.GetStringArray());
							}
						}
					}
					else if (iniFile.KeyEquals("blend"))
					{
						currentData.BlendMode = ((iniFile.GetIntValue() != 0) ? BlendModeEnum.Additive : BlendModeEnum.Normal);
					}
					else if (iniFile.KeyEquals("spine"))
					{
						currentData.SpineDataId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("spine_tree"))
					{
						currentData.BuildBySpine(iniFile.Value);
					}
					else if (iniFile.KeyEquals("spine_bone"))
					{
						currentData.SpineBoneId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("spine_track"))
					{
						currentData.SpineTrack = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("destroy_out_of_view") && iniFile.TryGetIntArray(2, out resultArray))
					{
						currentData.DestroyOutOfView = OutOfFrame.None;
						if (resultArray[0] == 1)
						{
							currentData.DestroyOutOfView |= OutOfFrame.OnLeft;
						}
						else if (resultArray[0] == 2)
						{
							currentData.DestroyOutOfView |= OutOfFrame.OnRight;
						}
						else if (resultArray[0] == 3)
						{
							currentData.DestroyOutOfView |= OutOfFrame.OnLeftAndRight;
						}
						if (resultArray[1] == 1)
						{
							currentData.DestroyOutOfView |= OutOfFrame.OnTop;
						}
						else if (resultArray[1] == 2)
						{
							currentData.DestroyOutOfView |= OutOfFrame.OnBottom;
						}
						else if (resultArray[1] == 3)
						{
							currentData.DestroyOutOfView |= OutOfFrame.OnTopAndBottom;
						}
					}
					else if (iniFile.KeyEquals("destroy_out_of_level") && iniFile.TryGetIntArray(2, out resultArray))
					{
						currentData.DestroyOutOfLevel = OutOfFrame.None;
						if (resultArray[0] == 1)
						{
							currentData.DestroyOutOfLevel |= OutOfFrame.OnLeft;
						}
						else if (resultArray[0] == 2)
						{
							currentData.DestroyOutOfLevel |= OutOfFrame.OnRight;
						}
						else if (resultArray[0] == 3)
						{
							currentData.DestroyOutOfLevel |= OutOfFrame.OnLeftAndRight;
						}
						if (resultArray[1] == 1)
						{
							currentData.DestroyOutOfLevel |= OutOfFrame.OnTop;
						}
						else if (resultArray[1] == 2)
						{
							currentData.DestroyOutOfLevel |= OutOfFrame.OnBottom;
						}
						else if (resultArray[1] == 3)
						{
							currentData.DestroyOutOfLevel |= OutOfFrame.OnTopAndBottom;
						}
					}
					else if (iniFile.KeyEquals("destroy_on_kill"))
					{
						currentData.DestroyOnKill = iniFile.GetBoolValue();
						currentData.IsDestroyOnKillDefined = true;
						currentData.ChildList?.ForEach((PartData child) =>
						{
							child.DestroyOnKill = currentData.DestroyOnKill;
						});
					}
					else if (iniFile.KeyEquals("destroy_on_path_end"))
					{
						currentData.DestroyOnPathEndMode = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("destroy_on_collision"))
					{
						currentData.DestroyOnCollision = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("destroy_out_of_action"))
					{
						currentData.DestroyOutOfAction = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("active_mode"))
					{
						switch (iniFile.GetIntValue())
						{
						case 1:
							currentData.ActiveMode = ActiveModeEnum.AlwaysActive;
							break;
						case 2:
							currentData.ActiveMode = ActiveModeEnum.NeverDeactivate;
							break;
						case 3:
							currentData.ActiveMode = ActiveModeEnum.NotActivateByLink;
							break;
						default:
							currentData.ActiveMode = ActiveModeEnum.Normal;
							break;
						}
					}
					else if (iniFile.KeyEquals("move_on_kill"))
					{
						currentData.MoveOnKill = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("kill_children"))
					{
						currentData.KillChildren = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("vis_rect") && iniFile.TryGetIntArray(2, 4, out resultArray))
					{
						if (resultArray.Length >= 4)
						{
							currentData.VisibilityRect = new Rectangle(-resultArray[0] / 2 + resultArray[2], -resultArray[1] / 2 + resultArray[3], resultArray[0], resultArray[1]);
						}
						else
						{
							currentData.VisibilityRect = new Rectangle(-resultArray[0] / 2, -resultArray[1] / 2, resultArray[0], resultArray[1]);
						}
					}
					else if (iniFile.KeyEquals("fx_rect") && iniFile.TryGetIntArray(2, 4, out resultArray))
					{
						if (resultArray.Length >= 4)
						{
							currentData.FxRect = new Rectangle(-resultArray[0] / 2 + resultArray[2], -resultArray[1] / 2 + resultArray[3], resultArray[0], resultArray[1]);
						}
						else
						{
							currentData.FxRect = new Rectangle(-resultArray[0] / 2, -resultArray[1] / 2, resultArray[0], resultArray[1]);
						}
					}
					else if (iniFile.KeyEquals("frame_rect") && iniFile.TryGetIntArray(2, 4, out resultArray))
					{
						if (resultArray.Length >= 4)
						{
							currentData.FrameRect = new Rectangle(resultArray[2] - resultArray[0] / 2, resultArray[3] - resultArray[1] / 2, resultArray[0], resultArray[1]);
						}
						else
						{
							currentData.FrameRect = new Rectangle(-resultArray[0] / 2, -resultArray[1] / 2, resultArray[0], resultArray[1]);
						}
					}
					else if (iniFile.KeyEquals("movable"))
					{
						currentData.IsMovable = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("autolink"))
					{
						currentData.AutolinkMode = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("poly") && iniFile.TryGetFloatArray(6, 99, out resultArray2))
					{
						if (resultArray2.Length % 2 != 0)
						{
							iniFile.LogErr("Polygon vertex count must be even number.");
							continue;
						}
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.VertextArray = resultArray2;
					}
					else if (iniFile.KeyEquals("poly_texture"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.TextureId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("poly_atlas"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.AtlasId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("poly_frame"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.AtlasFrame = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("poly_pivot_offset"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.PivotOffset = iniFile.GetVector2();
						currentData.PolygonData.UseAtlasPivot = false;
					}
					else if (iniFile.KeyEquals("poly_orientation"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.Orientation = (PolygonOritentation)iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("poly_sampler"))
					{
						currentData.SetUsePolygonData(bUse: true);
						switch (iniFile.GetIntValue())
						{
						case 1:
							currentData.PolygonData.SamplerState = SamplerState.LinearWrap;
							break;
						case 2:
							currentData.PolygonData.SamplerState = SamplerState.PointClamp;
							break;
						case 3:
							currentData.PolygonData.SamplerState = SamplerState.LinearClamp;
							break;
						default:
							currentData.PolygonData.SamplerState = SamplerState.PointWrap;
							break;
						}
					}
					else if (iniFile.KeyEquals("poly_top_atlas"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.TopAtlasId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("poly_top_frame") && iniFile.TryGetIntArray(1, 999, out currentData.PolygonData.TopAtlasFrameArray))
					{
						currentData.SetUsePolygonData(bUse: true);
					}
					else if (iniFile.KeyEquals("poly_top_texture"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.TopTextureId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("poly_bottom_texture"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.BottomTextureId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("poly_top_offset"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.TopOffset = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("poly_bottom_offset"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.BottomOffset = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("poly_mapping"))
					{
						currentData.SetUsePolygonData(bUse: true);
						switch (iniFile.GetIntValue())
						{
						case 1:
							currentData.PolygonData.TextureMappingMode = PolygonTextureMappingModeEnum.RelativeToPivot;
							break;
						case 2:
							currentData.PolygonData.TextureMappingMode = PolygonTextureMappingModeEnum.FixedRect;
							break;
						case 3:
							currentData.PolygonData.TextureMappingMode = PolygonTextureMappingModeEnum.FixedHeight;
							break;
						default:
							currentData.PolygonData.TextureMappingMode = PolygonTextureMappingModeEnum.RelativeToLevel;
							break;
						}
					}
					else if (iniFile.KeyEquals("poly_phys"))
					{
						currentData.SetUsePolygonData(bUse: true);
						currentData.PolygonData.CreatePhysBody = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("phys_remove") && iniFile.GetBoolValue())
					{
						currentData.PhysRectangleList?.Clear();
						currentData.PhysCircleList?.Clear();
						currentData.PhysPolygonList?.Clear();
					}
					else if (iniFile.KeyEquals("phys_bullet"))
					{
						currentData.IsBullet = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("phys_sleeping_allowed"))
					{
						currentData.PhysSleepingAllowed = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("phys_dynamic"))
					{
						currentData.PhysType = iniFile.GetIntValue();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_rect") && iniFile.TryGetIntArray(2, 4, out resultArray))
					{
						if (resultArray[0] <= 0 || resultArray[1] <= 0)
						{
							iniFile.LogErr("phys rectangle width and height must be > 0");
							continue;
						}
						if (currentData.PhysRectangleList == null)
						{
							currentData.PhysRectangleList = new List<Vector4>();
						}
						if (resultArray.Length >= 4)
						{
							currentData.PhysRectangleList.Add(new Vector4((float)resultArray[0] / 2f - (float)resultArray[2], (float)resultArray[1] / 2f - (float)resultArray[3], resultArray[0], resultArray[1]));
						}
						else
						{
							currentData.PhysRectangleList.Add(new Vector4((float)resultArray[0] / 2f, (float)resultArray[1] / 2f, resultArray[0], resultArray[1]));
						}
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_poly") && iniFile.TryGetFloatArray(6, 16, out resultArray2))
					{
						if (resultArray2.Length % 2 != 0)
						{
							iniFile.LogErr("Phys polygon vertex count must be even number.");
							continue;
						}
						if (currentData.PhysPolygonList == null)
						{
							currentData.PhysPolygonList = new List<float[]>();
						}
						currentData.PhysPolygonList.Add(resultArray2);
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_circle") && iniFile.TryGetIntArray(1, 3, out resultArray))
					{
						if (resultArray[0] <= 0)
						{
							iniFile.LogErr("phys circle radius must be > 0");
							continue;
						}
						if (currentData.PhysCircleList == null)
						{
							currentData.PhysCircleList = new List<Vector3>();
						}
						if (resultArray.Length > 2)
						{
							currentData.PhysCircleList.Add(new Vector3(resultArray[1], resultArray[2], resultArray[0]));
						}
						else
						{
							currentData.PhysCircleList.Add(new Vector3(0f, 0f, resultArray[0]));
						}
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_mass"))
					{
						currentData.PhysMass = iniFile.GetIntValue();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_restitution"))
					{
						currentData.PhysRestitution = iniFile.GetPercentage();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_friction"))
					{
						currentData.PhysFriction = iniFile.GetPercentage();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_damping"))
					{
						currentData.PhysLinearDamping = iniFile.GetPercentage();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_angular_damping"))
					{
						currentData.PhysAngularDamping = iniFile.GetPercentage();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_gravity"))
					{
						currentData.PhysGravityScale = iniFile.GetPercentage();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_on_collision"))
					{
						if (iniFile.ValueEquals("0"))
						{
							currentData.PhysOnCollisionEnabled = 0;
						}
						else
						{
							currentData.PhysOnCollisionEnabled = 1;
						}
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_rotation"))
					{
						currentData.PhysFixedRotation = !iniFile.GetBoolValue();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_scale"))
					{
						currentData.PhysFixedScale = !iniFile.GetBoolValue();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_center"))
					{
						currentData.PhysLocalCenter = iniFile.GetVector2();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_sensor"))
					{
						currentData.PhysIsSensor = iniFile.GetBoolValue();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_event"))
					{
						currentData.PhysEvent = iniFile.GetBoolValue();
						currentData.UsePhysData = true;
					}
					else if (iniFile.KeyEquals("phys_on_activate"))
					{
						currentData.PhysOnActivate = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("ignore_water"))
					{
						currentData.IgnoreWater = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("editor_name"))
					{
						currentData.EditorName = iniFile.Value;
					}
					else if (iniFile.KeyEquals("hp"))
					{
						currentData.MaxHP = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("hp_main"))
					{
						currentData.HPMainPercentage = iniFile.GetPercentage();
					}
					else if (iniFile.KeyEquals("hit_on_collision"))
					{
						currentData.HitOnCollision = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("light"))
					{
						currentData.LightOrShadow = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("hit_part"))
					{
						currentData.HitPartId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("path_speed"))
					{
						currentData.PathSpeed = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("path_front"))
					{
						currentData.PathFront = Utils.DegToRad(iniFile.GetIntValue());
					}
					else if (iniFile.KeyEquals("path_follow"))
					{
						switch (iniFile.GetIntValue())
						{
						case -1:
							currentData.PathFollowMode = PathFollowModeEnum.NoMove;
							break;
						case 1:
							currentData.PathFollowMode = PathFollowModeEnum.Follow;
							break;
						default:
							currentData.PathFollowMode = PathFollowModeEnum.FreeRotation;
							break;
						}
					}
					else if (iniFile.KeyEquals("light_factor"))
					{
						currentData.LightFactor = iniFile.GetPercentage();
					}
					else if (iniFile.KeyEquals("fx_mass"))
					{
						currentData.FxMass = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("remcon_factor"))
					{
						currentData.RemconHackSpeedFactor = iniFile.GetPercentage();
					}
					else if (iniFile.KeyEquals("remcon_level"))
					{
						currentData.RemconLevel = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("hit_effect"))
					{
						currentData.HitEffectFactor = iniFile.GetPercentage();
					}
					else if (iniFile.KeyEquals("score"))
					{
						currentData.Score = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("damage"))
					{
						currentData.Damage = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("use_damage_factor"))
					{
						currentData.UseDamageFactor = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("damage_type"))
					{
						DamageTypeEnum damageTypeByString = DataUtils.GetDamageTypeByString(iniFile.Value);
						if (damageTypeByString != DamageTypeEnum.Unknown)
						{
							currentData.DamageType = damageTypeByString;
							continue;
						}
						iniFile.LogErr("Unknown damage type '{0}'", iniFile.Value);
					}
					else if (iniFile.KeyEquals("hardness"))
					{
						currentData.Hardness = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("default_layer"))
					{
						currentData.DefaultLayerNumber = iniFile.GetIntValue();
					}
					else if (iniFile.KeyEquals("phys_class"))
					{
						PhysClass physClassByString = DataUtils.GetPhysClassByString(iniFile.Value);
						if (physClassByString != PhysClass.Undefined)
						{
							currentData.PhysClass = physClassByString;
							continue;
						}
						iniFile.LogErr("Unknown hit class '{0}'", iniFile.Value);
					}
					else if (iniFile.KeyEquals("target_class"))
					{
						TargetClass targetClassByString = DataUtils.GetTargetClassByString(iniFile.Value);
						if (targetClassByString != TargetClass.Undefined)
						{
							currentData.TargetClass = targetClassByString;
							continue;
						}
						iniFile.LogErr("Unknown target class '{0}'", iniFile.Value);
					}
					else if (iniFile.KeyEquals("color_mode"))
					{
						currentData.PartColorMode = DataUtils.GetColorModeByString(iniFile.Value);
						if (currentData.PartColorMode == ColorMode.Unknown)
						{
							currentData.PartColorMode = ColorMode.Local;
							iniFile.LogErr("Unknown value for color_mode");
						}
					}
					else if (iniFile.KeyEquals("emitter"))
					{
						currentData.EmitterDataId = iniFile.Value;
					}
					else if (iniFile.KeyEquals("fixed_angle"))
					{
						currentData.IsRotationFixed = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("kill_with_parent"))
					{
						currentData.KillWithParent = iniFile.GetBoolValue();
					}
					else if (iniFile.KeyEquals("emitter_space"))
					{
						int result2 = 1;
						int.TryParse(iniFile.Value, out result2);
						switch (result2)
						{
						case -1:
							currentData.EmitterParticleSpace = ParticleSpace.MyPartBack;
							break;
						case 11:
							currentData.EmitterParticleSpace = ParticleSpace.MainPartFront;
							break;
						case -11:
							currentData.EmitterParticleSpace = ParticleSpace.MainPartBack;
							break;
						case 2:
							currentData.EmitterParticleSpace = ParticleSpace.MyLayerFront;
							break;
						case -2:
							currentData.EmitterParticleSpace = ParticleSpace.MyLayerBack;
							break;
						case 3:
							currentData.EmitterParticleSpace = ParticleSpace.LastParticleLayerFront;
							break;
						case -3:
							currentData.EmitterParticleSpace = ParticleSpace.FirstMainLayerBack;
							break;
						case 4:
							currentData.EmitterParticleSpace = ParticleSpace.LevelFront;
							break;
						case -9:
							currentData.EmitterParticleSpace = ParticleSpace.Distortion;
							break;
						case 1:
							currentData.EmitterParticleSpace = ParticleSpace.MyPartFront;
							break;
						default:
							iniFile.LogErr("Wrong emitter_space = {0}", result2);
							break;
						}
					}
					else if (iniFile.KeyEquals("life_time") && iniFile.TryGetFloatRange(ref currentData.LifeTime))
					{
						currentData.LifeTime /= 1000f;
					}
					else
					{
						iniFile.LogErr("Unknown part command");
					}
				}
			}
		}
	}

	public override string ToString()
	{
		return Id;
	}
}
