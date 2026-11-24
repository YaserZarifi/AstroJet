using Microsoft.Xna.Framework;
using Spine;

namespace Rake2D.Spine;

public class SpineConnector
{
	public GameObject MyObject;

	public GameObject RootObject;

	private int _spineSlotIndex;

	public SpineRenderer SpineRenderer { get; private set; }

	public Bone MyBone { get; private set; }

	public Matrix WorldMatrix
	{
		get
		{
			if (MyBone != null)
			{
				return GetBoneWorldMatrix(MyBone);
			}
			Slot slot = SpineRenderer.GetSlot(_spineSlotIndex);
			if (slot != null)
			{
				return GetBoneWorldMatrix(slot.Bone);
			}
			return Matrix.Identity;
		}
	}

	public Transformation WorldTransformation
	{
		get
		{
			if (MyBone != null)
			{
				return GetBoneWorldTransformation(MyBone);
			}
			Slot slot = SpineRenderer.GetSlot(_spineSlotIndex);
			if (slot != null)
			{
				return GetBoneWorldTransformation(slot.Bone);
			}
			return GameCore.Allocator.GetTransformation();
		}
	}

	public bool IsFlippedX
	{
		get
		{
			if (MyBone != null)
			{
				return MyBone.WorldScaleX < 0f;
			}
			return false;
		}
	}

	public bool IsFlippedY
	{
		get
		{
			if (MyBone != null)
			{
				return MyBone.WorldScaleY < 0f;
			}
			return false;
		}
	}

	public SpineConnector(GameObject myObject, GameObject rootObject)
	{
		MyObject = myObject;
		RootObject = rootObject;
		SpineRenderer = rootObject.GetCurrentSpineRenderer();
	}

	public int ConnectToSlot(string slotName)
	{
		_spineSlotIndex = SpineRenderer.FindSlotIndex(slotName);
		return _spineSlotIndex;
	}

	public Bone ConnectToBone(string boneName)
	{
		if (SpineRenderer == null)
		{
			return null;
		}
		MyBone = SpineRenderer.FindBone(boneName);
		return MyBone;
	}

	public void ConnectToBone(Bone bone)
	{
		if (SpineRenderer != null)
		{
			MyBone = bone;
		}
	}

	private Matrix GetBoneWorldMatrix(Bone bone)
	{
		float angle = ((MyObject != RootObject) ? (bone.WorldRotationX + 90f) : (-90f - bone.WorldRotationX));
		float worldScaleX = bone.WorldScaleX;
		float worldScaleY = bone.WorldScaleY;
		return Matrix.CreateScale(worldScaleX, worldScaleY, 1f) * Matrix.CreateRotationZ(Utils.DegToRad(angle)) * Matrix.CreateTranslation(bone.WorldX, bone.WorldY, 0f);
	}

	private Transformation GetBoneWorldTransformation(Bone bone)
	{
		float angle = ((MyObject != RootObject) ? (bone.WorldRotationX + 90f) : (-90f - bone.WorldRotationX));
		Transformation transformation = GameCore.Allocator.GetTransformation();
		transformation.Position = new Vector2(bone.WorldX, bone.WorldY);
		transformation.Scale = new Vector2(bone.WorldScaleX, bone.WorldScaleY);
		transformation.Rotation = Utils.DegToRad(angle);
		return transformation;
	}

	public void SetSlotAngle(float angle, bool bWorld)
	{
		SpineRenderer.SetSlotRotation(_spineSlotIndex, angle, bWorld);
	}

	public void SetAngle(float angle, bool bWorld)
	{
		if (MyBone != null)
		{
			SpineRenderer.SetBoneAngle(MyBone, angle, bWorld);
		}
		else
		{
			SetSlotAngle(angle, bWorld);
		}
	}

	public float GetAngle()
	{
		if (MyBone != null)
		{
			return SpineRenderer.GetBoneAngle(MyBone);
		}
		return 0f;
	}

	public float GetWorldAngle()
	{
		if (MyBone != null)
		{
			return SpineRenderer.GetBoneWorldAngle(MyBone);
		}
		return 0f;
	}

	public void UpdateHitFactor()
	{
		if (MyBone != null)
		{
			MyBone.HitFactor = MyObject.HitEffectTimer;
		}
		else if (MyObject.SpineRenderer != null)
		{
			Bone bone = MyObject.SpineRenderer.GetBone(0);
			if (bone != null)
			{
				bone.HitFactor = MyObject.HitEffectTimer;
			}
		}
	}
}
