using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class GemMagnetrix : ItemBehaviorScript
{
	private struct GemBondData
	{
		public GameObject ShotObject;

		public float StartRotation;
	}

	private List<GemBondData> _shotList;

	public override void StartInSlot()
	{
		base.StartInSlot();
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_shotList = new List<GemBondData>();
	}

	public override void ApplyToItem(HeroItem item)
	{
		if (item != null && item.GetBehaviorByType(typeof(ItemWeapon)) is ItemWeapon { IsEnabledBondShot: false } itemWeapon)
		{
			itemWeapon.IsEnabledBondShot = true;
			itemWeapon.OnShotStart += OnShotStart;
		}
	}

	private void OnShotStart(GameObject shotObject, ShotStream stream)
	{
		GemBondData item = new GemBondData
		{
			ShotObject = shotObject,
			StartRotation = stream.MyObject.WorldRotation
		};
		_shotList.Add(item);
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		GameObject controlledObject = MyHero.GetControlledObject();
		for (int num = _shotList.Count - 1; num >= 0; num--)
		{
			GemBondData gemBondData = _shotList[num];
			if (gemBondData.ShotObject.IsDestroyed)
			{
				_shotList.RemoveAt(num);
			}
			else if (controlledObject.Velocity != Vector2.Zero)
			{
				Vector2 offset = new Vector2(Math.Abs(Utils.Cos(gemBondData.StartRotation)) * controlledObject.VelocityX, Math.Abs(Utils.Sin(gemBondData.StartRotation)) * controlledObject.VelocityY) * elapsedTime;
				if (gemBondData.ShotObject.MyPath != null)
				{
					gemBondData.ShotObject.MyPath.Move(offset);
				}
				else if (gemBondData.ShotObject.RigidBody != null)
				{
					gemBondData.ShotObject.SetPosition(gemBondData.ShotObject.RigidBody.WorldPosition.X + offset.X, gemBondData.ShotObject.RigidBody.WorldPosition.Y + offset.Y);
				}
			}
		}
	}
}
