using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Spine;

namespace Rake2D.Spine;

public class SpineRenderer
{
	public bool IsAnimLocked;

	private AnimationState _animationState;

	private GameObject _gameObject;

	public GameObject MyObject => _gameObject;

	public Skeleton Skeleton { get; private set; }

	internal SpineRenderer(GameObject go, SpineData spineData)
	{
		_gameObject = go;
		if (spineData.SkeletonData != null)
		{
			Skeleton = new Skeleton(spineData.SkeletonData);
		}
		else
		{
			GameCore.LogErr("No skeleton data for spine '{0}'", spineData.Id);
		}
		_animationState = new AnimationState(spineData.AnimationStateData);
	}

	public void Init()
	{
		_animationState.Start += Start;
		_animationState.End += OnEnd;
		_animationState.Complete += OnComplete;
		_animationState.Event += OnEvent;
	}

	public void SetColor(int r, int g, int b, int a)
	{
		Skeleton.R = (float)r / 255f;
		Skeleton.G = (float)g / 255f;
		Skeleton.B = (float)b / 255f;
		Skeleton.A = (float)a / 255f;
	}

	public int FindBoneIndex(string name)
	{
		return Skeleton.FindBoneIndex(name);
	}

	public Bone FindBone(string name)
	{
		return Skeleton.FindBone(name);
	}

	public Bone GetBone(int boneIndex)
	{
		if (boneIndex < 0 || Skeleton.Bones.Count <= 0)
		{
			return null;
		}
		return Skeleton.Bones.Items[boneIndex];
	}

	public int FindSlotIndex(string name)
	{
		return Skeleton.FindSlotIndex(name);
	}

	public List<Slot> FindSlotsForBone(string boneName)
	{
		List<Slot> list = new List<Slot>();
		Bone bone = FindBone(boneName);
		foreach (Slot slot in Skeleton.Slots)
		{
			if (slot.Bone == bone)
			{
				list.Add(slot);
			}
		}
		return list;
	}

	public Slot FindSlot(string name)
	{
		return Skeleton.FindSlot(name);
	}

	public Animation FindAnimation(string name)
	{
		return Skeleton.Data.FindAnimation(name);
	}

	public Slot GetSlot(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= Skeleton.Slots.Count)
		{
			return null;
		}
		return Skeleton.Slots.Items[slotIndex];
	}

	public void ResetTracks()
	{
		_animationState.ClearTracks();
	}

	public void SetAnimation(int track, string name, bool loop = true)
	{
		try
		{
			if (!IsAnimLocked)
			{
				_animationState.SetAnimation(track, name, loop);
			}
		}
		catch (ArgumentException ex)
		{
			GameCore.LogErr("Unable to set animation for Spine skeleton '{0}': {1}", _animationState.Data.SkeletonData.Name, ex);
		}
	}

	public void SetAnimation(int track, Animation animation, bool loop = true)
	{
		try
		{
			TrackEntry current = _animationState.GetCurrent(track);
			if (!IsAnimLocked && (current == null || current.Animation != animation))
			{
				_animationState.SetAnimation(track, animation, loop);
			}
			else if (current != null && current.Animation == animation)
			{
				current.Time = 0f;
			}
		}
		catch (ArgumentException ex)
		{
			GameCore.LogErr("Unable to set animation for Spine skeleton '{0}': {1}", _animationState.Data.SkeletonData.Name, ex);
		}
	}

	public void ResetAnimation(int track)
	{
		_animationState.ClearTrack(track);
	}

	public void AddAnimation(int track, string name, bool loop = true, float delay = 0f)
	{
		try
		{
			if (!IsAnimLocked)
			{
				_animationState.AddAnimation(track, name, loop, delay);
			}
		}
		catch (ArgumentException ex)
		{
			GameCore.LogErr("Unable to add animation for Spine skeleton '{0}': {1}", _animationState.Data.SkeletonData.Name, ex);
		}
	}

	public void AddAnimation(int track, Animation animation, bool loop = true, float delay = 0f)
	{
		try
		{
			if (!IsAnimLocked)
			{
				_animationState.AddAnimation(track, animation, loop, delay);
			}
		}
		catch (ArgumentException ex)
		{
			GameCore.LogErr("Unable to add animation for Spine skeleton '{0}': {1}", _animationState.Data.SkeletonData.Name, ex);
		}
	}

	public void SetSkin(string name)
	{
		try
		{
			Skeleton.SetSkin(name);
		}
		catch (ArgumentException ex)
		{
			GameCore.LogErr("Unable to set skeleton skin for Spine skeleton '{0}': {1}", _animationState.Data.SkeletonData.Name, ex);
		}
	}

	public void SetSlotRotation(int slotIndex, float angle, bool bWorld)
	{
		if (slotIndex >= 0)
		{
			if (bWorld && Skeleton.Slots.Items[slotIndex].Bone.Parent != null)
			{
				angle -= Skeleton.Slots.Items[slotIndex].Bone.Parent.WorldRotationX;
			}
			Skeleton.Slots.Items[slotIndex].Bone.Rotation = 0f - angle;
		}
	}

	public void SetBoneAngle(Bone bone, float angle, bool bWorld)
	{
		if (bWorld && bone.Parent != null)
		{
			angle -= Utils.RadToDeg(_gameObject.WorldRotation);
		}
		if (bone.WorldScaleX < 0f)
		{
			bone.Rotation = angle;
		}
		else
		{
			bone.Rotation = 0f - angle;
		}
	}

	public float GetBoneAngle(Bone bone)
	{
		if (bone.WorldScaleX < 0f)
		{
			return bone.Rotation;
		}
		return 0f - bone.Rotation;
	}

	public float GetBoneWorldAngle(Bone bone)
	{
		if (bone.Parent == null)
		{
			return -90f - bone.WorldRotationX;
		}
		return bone.WorldRotationX + 90f;
	}

	internal void SetTrackTime(int trackIndex, float time)
	{
		TrackEntry current = _animationState.GetCurrent(trackIndex);
		if (current != null)
		{
			current.Time = time;
		}
	}

	internal void UpdateBegin(float elapsedTime)
	{
		_animationState.Update(elapsedTime);
		_animationState.Apply(Skeleton);
	}

	public void UpdateEnd()
	{
		Skeleton.UpdateWorldTransform();
	}

	internal void Draw(Matrix matrix, float lightFactor)
	{
		GameCore.SpineSkeletonRenderer.Begin(_gameObject.MyLayer, lightFactor);
		GameCore.EffectLightBasic.Begin(GameCore.SpineSkeletonRenderer.Device.BlendState, _gameObject.MyCamera.WorldMatrixCurrentLayer, _gameObject.MyLayer, lightFactor);
		GameCore.SpineSkeletonRenderer.Draw(Skeleton, matrix);
		GameCore.SpineSkeletonRenderer.End();
	}

	internal void Start(AnimationState state, int trackIndex)
	{
		if (_gameObject != null)
		{
			_gameObject.DoOnSpineAnimStart(trackIndex);
		}
	}

	internal void OnEnd(AnimationState state, int trackIndex)
	{
		if (_gameObject != null)
		{
			_gameObject.DoOnSpineAnimEnd(trackIndex);
		}
	}

	internal void OnComplete(AnimationState state, int trackIndex, int loopCount)
	{
		if (_gameObject != null)
		{
			_gameObject.DoOnSpineAnimCompleted(trackIndex, loopCount);
		}
	}

	private void OnEvent(AnimationState state, int trackIndex, Event e)
	{
		_gameObject.DoOnSpineAnimEvent(e.Data.Name, e.Int, e.Float, e.String);
	}
}
