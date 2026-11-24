using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;
using Rake2D.Sound;

namespace Rake2D.JnG;

public class Remcon
{
	public List<RemconConnection> ConnectionList;

	public Hero MyHero;

	private static SpriteAtlas _atlasWave;

	private static PartData _remconableEffectData;

	private static PartData _heroEffectData;

	private static BMFont _font;

	private static Vector2 _fontScale;

	private static string _hackingText;

	private int _waveState;

	private float _waveAlpha;

	private float _currRadius;

	private int _maxRadius;

	private int _maxCount;

	private int _level;

	private int _connectedCount;

	private SpriteBatch _spriteBatch;

	private GameObject _heroEffectObject;

	private ISound2D _soundHacking;

	public static void Init()
	{
		_atlasWave = GameCore.GetSpriteAtlas("shockwave");
		_remconableEffectData = GameCore.GetPartData("remcon_remconable_effect");
		_heroEffectData = GameCore.GetPartData("remcon_hero_efect");
		_font = GameCore.GetBMFont("level_font");
		_fontScale = new Vector2(0.38f);
		_hackingText = TextBundle.GetText("remcon_hacking");
		RemconConnection.Init();
	}

	public Remcon(Hero hero)
	{
		MyHero = hero;
		ConnectionList = new List<RemconConnection>();
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
	}

	public void Clear()
	{
		ConnectionList.ForEach((RemconConnection c) =>
		{
			c.Destroy();
		});
		ConnectionList.Clear();
		_heroEffectObject?.Destroy();
		_heroEffectObject = null;
		_maxRadius = 0;
		_maxCount = 0;
		_currRadius = 0f;
		_waveAlpha = 0f;
		_waveState = 0;
		_level = -1;
		_connectedCount = 0;
	}

	public void Activate(int level)
	{
		Clear();
		_maxRadius = 300 + level * 100;
		_maxCount = level + 1;
		_currRadius = 0f;
		_waveState = 1;
		_waveAlpha = 1f;
		_level = level;
		MyHero.MyObject.PlaySound("remcon_activate");
	}

	private bool IsConnected(GameObject go)
	{
		foreach (RemconConnection connection in ConnectionList)
		{
			if (connection.TargetObject == go)
			{
				return true;
			}
		}
		return false;
	}

	private int GetConnectedEnemyCount()
	{
		int num = 0;
		foreach (RemconConnection connection in ConnectionList)
		{
			if (DataUtils.IsEnemyTargetClass(connection.TargetObject.TargetClass))
			{
				num++;
			}
		}
		return num;
	}

	private void CreateConnection()
	{
		List<GameObject> list = MyHero.MyObject.FindAllTargets((TargetClass)8760, (float)_maxRadius * MyHero.MyObject.WorldScale.X);
		if (!list.IsNullOrEmpty())
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = list[num];
				if (!gameObject.IsRemconable || gameObject.PartData.RemconLevel > _level || IsConnected(gameObject))
				{
					list.RemoveAt(num);
				}
				else
				{
					gameObject.MyLayer.CreateObjectByPartData(_remconableEffectData, gameObject.PartData.TargetOffset, Vector2.One, 0f, gameObject.ZOrder + 1, bCreateRigidBody: false).ParentTransformation = gameObject;
				}
			}
			_connectedCount = GetConnectedEnemyCount();
			while (list.Count > 0 && _connectedCount < _maxCount)
			{
				int random = Utils.GetRandom(list.Count - 1);
				GameObject targetObject = list[random];
				RemconConnection remconConnection = new RemconConnection(MyHero, targetObject);
				remconConnection.HackSpeed = 0.5f + (float)_level * 0.1f;
				remconConnection.HackSpeed -= (float)ConnectionList.Count * 0.1f;
				ConnectionList.Add(remconConnection);
				list.RemoveAt(random);
				_connectedCount++;
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(list);
		list = MyHero.MyObject.FindAllTargets(TargetClass.MissileEnemy, (float)_maxRadius * MyHero.MyObject.WorldScale.X);
		if (!list.IsNullOrEmpty())
		{
			int num2 = 0;
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				GameObject gameObject2 = list[num3];
				if (!gameObject2.IsRemconable || gameObject2.PartData.RemconLevel > _level || IsConnected(gameObject2))
				{
					list.RemoveAt(num3);
				}
				else
				{
					RemconConnection remconConnection2 = new RemconConnection(MyHero, gameObject2);
					remconConnection2.HackSpeed = 0.5f + (float)_level * 0.1f;
					if (DataUtils.IsEnemyTargetClass(remconConnection2.TargetObject.TargetClass))
					{
						remconConnection2.HackSpeed -= (float)num2 * 0.1f;
						num2++;
					}
					ConnectionList.Add(remconConnection2);
				}
			}
		}
		GameCore.Allocator.DeleteAliasingGameObjectList(list);
	}

	public void Update(float elapsedTime)
	{
		switch (_waveState)
		{
		case 1:
			CreateConnection();
			_waveState = 2;
			break;
		case 2:
			if (Utils.ChangeValueToTarget(ref _currRadius, (float)_maxRadius * MyHero.MyObject.WorldScale.X, elapsedTime * 3000f))
			{
				CreateConnection();
				_waveState = 3;
			}
			break;
		case 3:
			if (Utils.ChangeValueToTarget(ref _waveAlpha, 0f, elapsedTime * 5f))
			{
				_waveState = 4;
			}
			break;
		}
		bool flag = false;
		bool flag2 = false;
		for (int num = ConnectionList.Count - 1; num >= 0; num--)
		{
			RemconConnection remconConnection = ConnectionList[num];
			if (remconConnection.TargetObject.IsDestroyed || remconConnection.TargetObject.IsKilled || !remconConnection.TargetObject.IsActivated || !remconConnection.TargetObject.IsVisible || !remconConnection.TargetObject.IsEnabled || (remconConnection.State == RemconStateEnum.HackDone && !remconConnection.TargetObject.IsRemconable))
			{
				remconConnection.Destroy();
				ConnectionList.RemoveAt(num);
			}
			else
			{
				if (remconConnection.State == RemconStateEnum.Hacking)
				{
					flag = true;
				}
				else if (remconConnection.State == RemconStateEnum.HackDone)
				{
					flag2 = true;
				}
				remconConnection.Update(elapsedTime);
			}
		}
		if (flag | flag2)
		{
			if (_heroEffectObject == null || _heroEffectObject.IsDestroyed)
			{
				_heroEffectObject = MyHero.MyObject.MyLayer.CreateObjectByPartData(_heroEffectData, MyHero.MyObject.WorldPosition, Vector2.One, 0f, MyHero.MyObject.ZOrder + 1, bCreateRigidBody: false);
			}
			else
			{
				_heroEffectObject.IsEnabled = true;
				_heroEffectObject.SetPosition(MyHero.MyObject.WorldPosition);
			}
			if (flag)
			{
				_heroEffectObject.SetScale(1f, 1f);
			}
			else
			{
				_heroEffectObject.SetScale(0.6f, 0.6f);
			}
			if (flag && _soundHacking == null)
			{
				_soundHacking = MyHero.MyObject.PlaySound("remcon_hacking", 100, looped: true);
			}
		}
		else if (_heroEffectObject != null)
		{
			_heroEffectObject.IsEnabled = false;
		}
		if (!flag && _soundHacking != null)
		{
			_soundHacking.Stop();
			_soundHacking = null;
		}
	}

	public void Draw()
	{
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, MyHero.MyObject.MyCamera.WorldMatrixMainLayer);
		if (_currRadius > 0f && _waveAlpha > 0f)
		{
			Transformation transformation = Transformation.CreateByPosition(MyHero.MyObject.WorldPosition);
			transformation.Scale = new Vector2(_currRadius / (float)_atlasWave.GetFrameRectangle(0).Width * 2f);
			_atlasWave.DrawFrame(_spriteBatch, 0, new Color(0, (int)(255f * _waveAlpha), 0, 180), transformation, BlendModeEnum.Additive);
		}
		foreach (RemconConnection connection in ConnectionList)
		{
			if (connection.LockObject != null && !connection.LockObject.IsDestroyed)
			{
				switch (connection.State)
				{
				case RemconStateEnum.Hacking:
					_font.Write(_spriteBatch, $"{_hackingText} {(int)(connection.HackPercent * 100f)} %", connection.LockObject.Position.X, connection.LockObject.Position.Y - 48f, _fontScale, Color.FromNonPremultiplied(0, 255, 0, 200), TextAlign.Center);
					break;
				case RemconStateEnum.HackDone:
					_font.Write(_spriteBatch, (connection.TargetObject.RemconText != null) ? TextBundle.GetText(connection.TargetObject.RemconText) : "#E0987-HACKED", connection.LockObject.Position.X, connection.LockObject.Position.Y - 48f, _fontScale, Color.FromNonPremultiplied(0, 255, 0, 255), TextAlign.Center);
					break;
				}
			}
		}
		_spriteBatch.End();
	}
}
