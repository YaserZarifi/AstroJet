using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;
using Rake2D.Script.HeroShot;
using Rake2D.Sound;

namespace Rake2D.Script;

public class ShotStream
{
	public delegate void PrepareStream(ShotStream stream);

	public ItemWeapon MyWeapon;

	public GameObject MyObject;

	public ShotStreamData Data;

	public int ArcDirection;

	public float WidthOffsetPercentage;

	public FloatRange ShotSpeedRange;

	public BezierPath BezierPath;

	public GameObject GunFireObject;

	public long LastFireNumber;

	public long ShotNumber;

	public PrepareStream PrepareMyStream;

	private ISound2D _fireSound;

	private float _fireSoundTimer;

	private GameObject _shotObject;

	private GameObject _oldWeaponMyObject;

	private bool _wasStopSoundPlayed = true;

	public float GetShotDamage()
	{
		if (MyWeapon.DamageMode == ItemDamageMode.HeroTotalDPSPercent)
		{
			return Data.ShotDamage / 100f * MyWeapon.MyHero.GetTotalDPS();
		}
		return Data.ShotDamage;
	}

	public float GetHeatPerSec()
	{
		return Data.Heat * (float)Data.ShotCount * MyWeapon.HeatFactor / (Data.ShotDelay / MyWeapon.FireRateFactor);
	}

	public long GetCurrFireNumber()
	{
		return (long)((MyWeapon.FireTimer - Data.StartDelay) / (Data.ShotDelay / MyWeapon.FireRateFactor));
	}

	public ShotStream(ItemWeapon weapon, ShotStreamData data)
	{
		Data = data;
		MyWeapon = weapon;
		LastFireNumber = 0L;
		MyObject = new GameObject();
		MyWeapon.MyMuzzle.AddChild(MyObject);
		_oldWeaponMyObject = MyWeapon.MyObject;
		ShotSpeedRange = Data.ShotSpeedRange;
		ArcDirection = 1;
	}

	public void Destroy()
	{
		StopSound();
		MyObject?.Destroy();
	}

	private void CreateShots()
	{
		if (Data.ShotCount <= 0)
		{
			_shotObject = null;
			return;
		}
		for (int i = 0; i < Data.ShotCount; i++)
		{
			MyObject.ResetPrecalculations();
			float num = 0f;
			Vector2 vec;
			if (Data.ArcSpeed == 0f)
			{
				switch (Data.SpreadMode)
				{
				case 1:
				{
					float num2;
					Vector2 vector;
					if (Data.ShotCount > 1)
					{
						num2 = (Data.ArcRange.To - Data.ArcRange.From) / (float)(Data.ShotCount - 1);
						vector = new Vector2((Data.OffsetRange.ToX - Data.OffsetRange.FromX) / (float)(Data.ShotCount - 1), (Data.OffsetRange.ToY - Data.OffsetRange.FromY) / (float)(Data.ShotCount - 1));
					}
					else
					{
						num2 = 0f;
						vector = Vector2.Zero;
					}
					num = Data.ArcRange.From + (float)i * num2;
					vec = new Vector2(Data.OffsetRange.FromX + (float)i * vector.X, Data.OffsetRange.FromY + (float)i * vector.Y);
					break;
				}
				case 2:
				{
					float num2 = (Data.ArcRange.To - Data.ArcRange.From) / (float)Data.ShotCount;
					Vector2 vector = new Vector2((Data.OffsetRange.ToX - Data.OffsetRange.FromX) / (float)Data.ShotCount, (Data.OffsetRange.ToY - Data.OffsetRange.FromY) / (float)Data.ShotCount);
					num = Data.ArcRange.From + (float)i * num2 + Utils.GetRandom(0f, num2);
					vec = new Vector2(Data.OffsetRange.FromX + (float)i * vector.X + Utils.GetRandom(0f, vector.X), Data.OffsetRange.GetRandomY());
					break;
				}
				default:
					num = Data.ArcRange.GetRandom();
					vec = Data.OffsetRange.GetRandom();
					break;
				}
				if (!Data.ShotAsChild)
				{
					MyObject.SetRotation(num);
				}
			}
			else
			{
				if (Data.ResetFire)
				{
					if (Data.ArcSpeed < 0f)
					{
						num = Data.ArcRange.To;
						ArcDirection = -1;
					}
					else
					{
						num = Data.ArcRange.From;
						ArcDirection = 1;
					}
					MyObject.SetRotation(num);
				}
				vec = Data.OffsetRange.GetRandom();
			}
			float random = Utils.GetRandom((0f - MyWeapon.StreamAngleRandom) / 2f, MyWeapon.StreamAngleRandom / 2f);
			if (Data.ShotAsChild)
			{
				_shotObject = MyObject.CreateChildByPartData(Data.ShotData, createPhysBody: true);
				if (_shotObject != null)
				{
					_shotObject.SetScale(Data.ShotScaleRange.GetRandom());
				}
			}
			else
			{
				Vector2 pos = Utils.RotateVector2(vec, MyObject.WorldRotation);
				pos += MyObject.WorldPosition;
				float rotation = ((!Data.IsShotAngleAbsolute) ? (MyObject.WorldRotation + Data.ShotRotationRange.GetRandom() + random) : (MyWeapon.MyHero.MyObject.WorldRotation + Data.ShotRotationRange.GetRandom()));
				_shotObject = MyWeapon.MyHero.MyObject.MyLayer.CreateObjectByPartData(Data.ShotData, pos, new Vector2(Data.ShotScaleRange.GetRandom()) * MyWeapon.MyObject.WorldScale, rotation, Data.ZOrder);
			}
			if (_shotObject != null)
			{
				float rotation2 = MyObject.WorldRotation + random;
				_shotObject.MyItem = MyWeapon.MyItem;
				HeroShotData heroShotData = new HeroShotData
				{
					StartObject = MyObject,
					Number = ShotNumber,
					Speed = ShotSpeedRange.GetRandom()
				};
				_shotObject.BehaviorData = heroShotData;
				_shotObject.LifeTime = Utils.Clamp(Data.ShotLifeTimeRange.GetRandom(), 0f, 10f) * MyWeapon.LifeTimeFactor;
				_shotObject.Damage = GetShotDamage() * MyWeapon.GetDamageFactor();
				if (Data.IsLifeTimeUsedToDPS)
				{
					_shotObject.Damage *= Data.ShotLifeTimeRange.GetAverage() * MyWeapon.LifeTimeFactor;
				}
				_shotObject.Damage *= MyWeapon.MyHero.GetBonusWeaponDamageFactor();
				_shotObject.Damage *= MyWeapon.MyObject.Scale.X;
				_shotObject.OnReRegisterEvents += MyWeapon.OnReRegisterEvents;
				_shotObject.Init();
				_shotObject.LevelInit();
				_shotObject.Activate();
				if (_shotObject.RigidBody != null && MyWeapon.ExtraHitTo != PhysClass.Undefined)
				{
					_shotObject.RigidBody.HitTo |= MyWeapon.ExtraHitTo;
				}
				if (Data.ShotAsChild)
				{
					Vector2 vector2 = Utils.RotateVector2(vec, MyObject.Rotation);
					_shotObject.SetPosition(vector2.X, vector2.Y);
					_shotObject.SetRotation(num + random);
				}
				if (BezierPath != null)
				{
					_shotObject.MyPath = BezierPath;
					_shotObject.PathSpeed = heroShotData.Speed;
				}
				else if (Data.PathControlPointList != null)
				{
					_shotObject.MyPath = new BezierPath("shot_path", _shotObject);
					foreach (Vector2 pathControlPoint in Data.PathControlPointList)
					{
						_shotObject.MyPath.AddPoint(new Vector2(pathControlPoint.X * (float)((Data.PathFlipMode != 1 && Data.PathFlipMode != 3) ? 1 : (-1)), pathControlPoint.Y * (float)((Data.PathFlipMode <= 1) ? 1 : (-1))) + _shotObject.WorldPosition);
					}
					if (MyObject.WorldRotation != Utils.DegToRad(90f) || random != 0f)
					{
						_shotObject.MyPath.Rotate(MyObject.WorldRotation + Utils.DegToRad(-90f) + random);
					}
					else
					{
						_shotObject.MyPath.CalculateLength();
					}
					_shotObject.PathSpeed = heroShotData.Speed;
				}
				else
				{
					_shotObject.SetVelocity(Utils.GetVelocityVector2(heroShotData.Speed, rotation2) + MyWeapon.MyLevel.ScrollSpeed);
				}
				_shotObject.RotationSpeed = Data.ShotRotationSpeedRange.GetRandom();
				MyWeapon.DoOnShotStart(_shotObject, this);
				GameJnG.AchievementManager.RaiseLevelAchievedCount(AchievementCountTypeEnum.FiredShots);
				ProcessShot();
			}
			ShotNumber++;
			MyWeapon.MyHero.AddHeat(Data.Heat * MyWeapon.HeatFactor);
		}
	}

	private void ProcessShot()
	{
		if (MyWeapon.ShotBehaviorDataList == null)
		{
			return;
		}
		foreach (BehaviorData shotBehaviorData in MyWeapon.ShotBehaviorDataList)
		{
			_shotObject.AddBehaviorScript(shotBehaviorData);
		}
	}

	public bool IsShotReady()
	{
		return GetCurrFireNumber() > LastFireNumber;
	}

	public void StopSound()
	{
		if (_fireSound != null)
		{
			_fireSound.Stop();
			_fireSound = null;
		}
	}

	public bool Update(float elapsedTime)
	{
		bool result = false;
		if (MyObject == null || MyObject.IsDestroyed)
		{
			MyObject = new GameObject();
			MyWeapon.MyMuzzle.AddChild(MyObject);
		}
		else if (MyWeapon.MyMuzzle != null && MyObject.ParentObject != MyWeapon.MyObject)
		{
			MyWeapon.MyMuzzle.AddChild(MyObject);
		}
		if (_shotObject != null && _shotObject.IsDestroyed)
		{
			_shotObject = null;
		}
		if (Data.ArcSpeed != 0f)
		{
			float currentValue = MyObject.Rotation;
			if (ArcDirection > 0)
			{
				if (Utils.ChangeValueToTarget(ref currentValue, Data.ArcRange.To, Math.Abs(Data.ArcSpeed) * ((float)Math.PI / 180f) * elapsedTime))
				{
					ArcDirection = -1;
				}
			}
			else if (Utils.ChangeValueToTarget(ref currentValue, Data.ArcRange.From, Math.Abs(Data.ArcSpeed) * ((float)Math.PI / 180f) * elapsedTime))
			{
				ArcDirection = 1;
			}
			MyObject.SetRotation(currentValue);
		}
		if (MyWeapon.IsFiring)
		{
			if (IsShotReady())
			{
				PrepareMyStream?.Invoke(this);
				LastFireNumber = GetCurrFireNumber();
				if (Data.IsSingleShot && _shotObject != null)
				{
					_shotObject.Destroy();
					_shotObject = null;
				}
				CreateShots();
				if (!Data.IsSoundLooped && Data.SoundId != null)
				{
					_fireSound = MyObject.PlaySound(Data.SoundId);
				}
				if ((GunFireObject == null || GunFireObject.IsDestroyed || !Data.IsSingleGunFire) && Data.GunFireData != null)
				{
					GunFireObject = MyObject.CreateChildByPartData(Data.GunFireData, createPhysBody: true);
					if (GunFireObject != null)
					{
						GunFireObject.SetPosition(Data.GunFireoOffset);
						GunFireObject.SetScale(Data.GunFireScale);
					}
				}
				result = true;
			}
			if (MyWeapon.MyObject != _oldWeaponMyObject)
			{
				_oldWeaponMyObject = MyWeapon.MyObject;
				StopSound();
			}
			if (_fireSound == null && Data.IsSoundLooped && Data.SoundId != null)
			{
				_fireSound = MyObject.PlaySound(Data.SoundId, 100, Data.IsSoundLooped);
				_fireSoundTimer = 0.2f;
			}
			_wasStopSoundPlayed = false;
		}
		else
		{
			if (_fireSound != null && Data.IsSoundLooped)
			{
				_fireSoundTimer -= elapsedTime;
				if (_fireSoundTimer <= 0f)
				{
					_fireSound.Stop();
					_fireSound = null;
				}
			}
			if (!_wasStopSoundPlayed && Data.SoundStopId != null)
			{
				MyObject.PlaySound(Data.SoundStopId);
				_wasStopSoundPlayed = true;
			}
		}
		if (GunFireObject != null)
		{
			if (GunFireObject.IsDestroyed)
			{
				GunFireObject = null;
			}
			else
			{
				if (Data.IsGunFireLooped && MyWeapon.IsFiring)
				{
					GunFireObject.LifeTime = 0.1f;
				}
				if (Data.GunFireUseStreamRotation)
				{
					if (_shotObject != null)
					{
						GunFireObject.SetRotation(_shotObject.Rotation);
					}
				}
				else
				{
					GunFireObject.SetRotation(0f);
				}
			}
		}
		return result;
	}
}
