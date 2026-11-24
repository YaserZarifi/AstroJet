using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Editor;
using Rake2D.Font.BMFont;
using Rake2D.Sound;
using Rake2D.src.JnG.HUD;

namespace Rake2D.JnG;

public class HUD : Transformation
{
	public Hero MyHero;

	public MiniHUD MiniHUD;

	public GameLevel HUDLevel;

	private TriangleBatchTextured _triangleBatchTextured;

	private float _drawManaPercent;

	private BitmapFont _fontDebug;

	private BMFont _fontNormal;

	private BMFont _fontDigital;

	private BMFont _fontNoBorder;

	private Vector2 _bombPosition;

	private SpriteBatch _spriteBatch;

	private SpriteAtlas _bgrAtlas;

	private SpriteAtlas _heatDialAtlas;

	private SpriteAtlas _circleDialAtlas;

	private SpriteAtlas _bombMeterAtlas;

	private SpriteAtlas _manaBarAtlas;

	private SpriteAtlas _shipRecoveryAtlas;

	private PartData _hitMarkerData;

	private float _hideAlphaFactor;

	private float _heatNeedleVibration;

	private float _heatNeedleVibrationTimer;

	private float _heatNeedlePercent;

	private float _hpBarPercentage;

	private float _heatFreezeAlpha;

	private float _hpWarningTimer;

	private float _overheatDiodeBlink;

	private bool _hpWarningActivated;

	private bool _manaWarningActivated;

	private ISound2D _hpWarningSound;

	private static string _strDanger;

	private static string _strHP;

	private static string _strJetPack;

	private static string _strRecovery;

	private static string _strShipReady;

	private float _shieldChargeTimer;

	private float _currentHPPercent;

	private float _currShieldPercent;

	private List<HUDCounter> _hudCounterList;

	public bool IsShipRecoveryDisplayed
	{
		get
		{
			if (MyHero.IsDestroyed)
			{
				if (!MyHero.IsJetPackActive)
				{
					if (GameJnG.PlayerCount > 1)
					{
						return GameJnG.IsAnyHeroAlive;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public HUD(Hero hero)
	{
		MyHero = hero;
		if (hero.PlayerNumber == 0)
		{
			Position = new Vector2(150f, 90f);
		}
		else
		{
			Position = new Vector2(1520f, 90f);
		}
		_bgrAtlas = GameCore.GetSpriteAtlas("hud_bgr");
		_circleDialAtlas = GameCore.GetSpriteAtlas("hud_circle_dial");
		_heatDialAtlas = GameCore.GetSpriteAtlas("hud_heat_dial");
		_bombMeterAtlas = GameCore.GetSpriteAtlas("hud_bomb_meter");
		_manaBarAtlas = GameCore.GetSpriteAtlas("hud_mana_meter");
		_shipRecoveryAtlas = GameCore.GetSpriteAtlas("hud_ship_recovery");
		_hitMarkerData = GameCore.GetPartData("hud_hit_marker");
		_fontNormal = GameCore.GetBMFont("shop_font");
		_fontDigital = GameCore.GetBMFont("digital_font");
		_fontNoBorder = GameCore.GetBMFont("level_font");
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		MiniHUD = new MiniHUD(MyHero);
		HUDLevel = new GameLevel();
		HUDLevel.Init();
		Layer layer = HUDLevel.CreateLayer("main");
		layer.Init();
		HUDLevel.MainLayer = layer;
		_triangleBatchTextured = new TriangleBatchTextured(GameCore.GraphicsDeviceStatic);
	}

	public static void SetTexts()
	{
		_strHP = TextBundle.GetText("hud_hp");
		_strDanger = TextBundle.GetText("hud_danger");
		_strJetPack = TextBundle.GetText("hud_jet_pack");
		_strRecovery = TextBundle.GetText("hud_recovery");
		_strShipReady = TextBundle.GetText("hud_ship_ready");
	}

	public void Clear()
	{
		_hpWarningActivated = false;
		_manaWarningActivated = true;
		_hpBarPercentage = 0f;
	}

	public void ClearCounters()
	{
		_hudCounterList?.Clear();
		HUDLevel.MainLayer.Clear();
	}

	public void UpdateWarnings(float elasedTime)
	{
		if (!_hpWarningActivated && _currentHPPercent <= 0.25f && _currentHPPercent > 0f)
		{
			_hpWarningActivated = true;
			_hpWarningTimer = 0f;
			if (!GameCore.IsShopMenuEnabled)
			{
				_hpWarningSound = GameCore.PlaySound("hud_hp_warning", 100f, 0f, looped: true);
				GameCore.PlaySound("voice_warning");
				MyHero.MyObject.MyLevel.AddWarningText(TextBundle.GetText("hud_warning_hull_critical"), new Color(255, 0, 0));
			}
		}
		else if (_hpWarningActivated && _hpWarningTimer >= 1f && _currentHPPercent > 0.25f)
		{
			_hpWarningActivated = false;
			_hpWarningTimer = 0f;
		}
		if (_hpWarningActivated)
		{
			_hpWarningTimer += elasedTime;
		}
		if ((!_hpWarningActivated || _currentHPPercent <= 0f) && _hpWarningSound != null)
		{
			_hpWarningSound.Stop();
			_hpWarningSound = null;
		}
		if (MyHero.ManaPercent >= 1f)
		{
			if (!_manaWarningActivated)
			{
				_manaWarningActivated = true;
				GameCore.PlaySound("voice_mana_ready");
				MyHero.MyObject.MyLevel.AddWarningText(TextBundle.GetText("hud_warning_mana_rady"), new Color(240, 0, 255));
			}
		}
		else
		{
			_manaWarningActivated = false;
		}
	}

	public HUDCounter GetOrCreateCounter(string id)
	{
		HUDCounter hUDCounter = null;
		if (_hudCounterList == null)
		{
			_hudCounterList = new List<HUDCounter>();
		}
		else
		{
			hUDCounter = _hudCounterList.FirstOrDefault((HUDCounter c) => Utils.Equals(c.Id, id));
		}
		if (hUDCounter == null)
		{
			hUDCounter = new HUDCounter(this, id);
			_hudCounterList.Add(hUDCounter);
		}
		return hUDCounter;
	}

	public List<HUDCounter> GetCounterList()
	{
		return _hudCounterList;
	}

	public void SetCounterList(List<HUDCounter> hudCounterList)
	{
		if (hudCounterList != null)
		{
			_hudCounterList = new List<HUDCounter>(hudCounterList);
		}
		else
		{
			_hudCounterList = null;
		}
	}

	public void Update(float elapsedTime)
	{
		if (MyHero.IsJetPackActive)
		{
			_currentHPPercent = MyHero.JetPackObject.HPPercent;
			_currShieldPercent = 0f;
		}
		else
		{
			_currentHPPercent = MyHero.MyObject.HPPercent;
			_currShieldPercent = MyHero.ShieldPercent;
		}
		if (MyHero.IsShieldWaiting)
		{
			_shieldChargeTimer += 10f * elapsedTime;
		}
		else
		{
			_shieldChargeTimer = -(float)Math.PI / 2f;
		}
		if (MyHero.IsOverheated || MyHero.IsBurning)
		{
			_heatNeedleVibrationTimer -= elapsedTime;
			if (_heatNeedleVibrationTimer <= 0f)
			{
				_heatNeedleVibrationTimer = Utils.GetRandom(0.03f, 0.05f);
				_heatNeedleVibration = Utils.GetRandom(-0.07f, 0.07f);
			}
		}
		else
		{
			_heatNeedleVibration = 0f;
		}
		float num = Math.Min(1.1f, MyHero.HeatPercent);
		Utils.ChangeValueToTarget(ref _heatNeedlePercent, num, Math.Abs(_heatNeedlePercent - num) * 20f * elapsedTime);
		Utils.ChangeValueToTarget(ref _drawManaPercent, MyHero.ManaPercent, 10f * elapsedTime);
		num = ((!(MyHero.Shield <= 0f) && !MyHero.IsJetPackActive) ? MyHero.ShieldPercent : _currentHPPercent);
		Utils.ChangeValueToTarget(ref _hpBarPercentage, num, Math.Abs(_hpBarPercentage - num) * 5f * elapsedTime);
		if (MyHero.NoHeatTimer < 0.5f)
		{
			_heatFreezeAlpha = MyHero.NoHeatTimer * 2f;
		}
		else if (MyHero.NoHeatTimer > 0.5f)
		{
			Utils.ChangeValueToTarget(ref _heatFreezeAlpha, 1f, 2f * elapsedTime);
		}
		else
		{
			_heatFreezeAlpha = 0f;
		}
		_bombPosition = Position + new Vector2(42f, 50f);
		bool flag = false;
		foreach (Hero hero in GameJnG.HeroList)
		{
			GameObject controlledObject = hero.GetControlledObject();
			if (controlledObject != null && Utils.IsPointInFixedRect(controlledObject.VirtualScreenPosition, Position + new Vector2(100f, 0f), 450f, 220f) && controlledObject.IsAlive)
			{
				flag = true;
			}
		}
		Utils.ChangeValueToTarget(ref _hideAlphaFactor, flag ? 0.3f : 1f, 3f * elapsedTime);
		UpdateWarnings(elapsedTime);
		if (Options.IsMiniHUDVisible)
		{
			MiniHUD.Update(elapsedTime);
		}
		GameLevel currentLevel = GameCore.CurrentLevel;
		HUDLevel.Update(elapsedTime);
		GameCore.CurrentLevel = currentLevel;
		if (_hudCounterList == null)
		{
			return;
		}
		float num2 = ((Options.HUDMode == HUDModeEnum.Both || Options.HUDMode == HUDModeEnum.BigOnly) ? 210 : 75);
		if (IsShipRecoveryDisplayed)
		{
			num2 += 58f;
		}
		foreach (HUDCounter hudCounter in _hudCounterList)
		{
			hudCounter.Position = new Vector2(65f, num2);
			hudCounter.Update(elapsedTime);
			num2 += 35f;
		}
	}

	private void DrawHUDCounters()
	{
		GameLevel currentLevel = GameCore.CurrentLevel;
		HUDLevel.Camera.SetViewPort(GameCore.MainViewPort);
		HUDLevel.Draw();
		GameCore.CurrentLevel = currentLevel;
		if (_hudCounterList == null)
		{
			return;
		}
		foreach (HUDCounter hudCounter in _hudCounterList)
		{
			hudCounter.Draw(_spriteBatch);
		}
	}

	private void DrawDebugInfo()
	{
		string text = "Kills: ";
		for (int i = 2; i <= 5; i++)
		{
			if (i != 2)
			{
				text += ", ";
			}
			string text2 = text;
			object[] array = new object[4];
			AchievementCountTypeEnum achievementCountTypeEnum = (AchievementCountTypeEnum)i;
			array[0] = achievementCountTypeEnum.ToString()[9];
			array[1] = GameJnG.AchievementManager.LevelAchievedCountArray[i];
			array[2] = GameJnG.AchievementManager.LevelAvailableCountArray[i];
			array[3] = GameJnG.AchievementManager.GetLevelCountPercent((AchievementCountTypeEnum)i);
			text = text2 + string.Format("{0}:{1}/{2}={3}%", array);
		}
		string arg = $"Bns: {GameJnG.AchievementManager.LevelAchievedCountArray[1]}/{GameJnG.AchievementManager.LevelAvailableCountArray[1]}={GameJnG.AchievementManager.GetLevelCountPercent(AchievementCountTypeEnum.BonusCount)}%";
		Vector2 vector = new Vector2(50f, 920f);
		_fontDebug.Write(_spriteBatch, $"Score: {GameJnG.LevelScore}, {text}, {arg}", vector.X, vector.Y, new Color(255, 255, 0));
		int num = (int)(MyHero.HPPercent * 100f);
		Color color;
		if (num <= 25)
		{
			color = new Color(255, 0, 0);
		}
		else
		{
			color = ((num > 50) ? new Color(155, 155, 155) : new Color(255, 255, 0));
		}
		_fontDebug.Write(_spriteBatch, $"HP: {num}% ({Utils.FormatFloat2(MyHero.MyObject.CurrentHP)} / {Utils.FormatFloat2(MyHero.MyObject.MaxHP)}), DmgCaus: {GameJnG.AchievementManager.DamageCaused}, DmgAbs: {GameJnG.AchievementManager.HeroDamageAbsorbed}, Shots: {GameJnG.AchievementManager.LevelAchievedCountArray[6]}", vector.X, vector.Y += 20f, color);
		if (MyHero.Shield < 0.25f)
		{
			color = new Color(255, 0, 0);
		}
		else
		{
			color = ((!(MyHero.Shield < 0.75f)) ? new Color(155, 155, 255) : new Color(255, 150, 0));
		}
		_fontDebug.Write(_spriteBatch, $"Shield: {Utils.FormatDecimal1(MyHero.ShieldPercent * 100f)}% ({Utils.FormatDecimal1(MyHero.Shield)} / {(int)MyHero.MaxShield}) +{Utils.FormatDecimal1(MyHero.ShieldRechargeSpeed)} / sec, delay: {MyHero.ShieldWaitDelay}", vector.X, vector.Y += 20f, color);
		num = (int)(MyHero.Heat / MyHero.MaxHeat * 100f);
		if (MyHero.IsBurning)
		{
			color = Color.Red;
		}
		else
		{
			color = ((!MyHero.IsOverheated) ? new Color(0, 150, 255) : Color.Yellow);
		}
		_fontDebug.Write(_spriteBatch, $"Heat: {num}%, Cooling: {MyHero.GetCoolingSpeed()} / sec, Max: {MyHero.MaxHeat}", vector.X, vector.Y += 20f, color);
		if (MyHero.ManaPercent < 0.25f)
		{
			color = new Color(255, 0, 0);
		}
		else
		{
			color = ((!(MyHero.ManaPercent < 0.75f)) ? new Color(255, 255, 0) : new Color(255, 200, 0));
		}
		_fontDebug.Write(_spriteBatch, $"Mana: {Utils.FormatDecimal1(MyHero.ManaPercent * 100f)}% ({Utils.FormatDecimal1(MyHero.Mana)} / {(int)MyHero.MaxMana}) +{Utils.FormatDecimal1(MyHero.ManaRechargeSpeed)} / sec", vector.X, vector.Y += 20f, color);
	}

	private void DrawBombMeter()
	{
		if (MyHero.MaxBombCount <= 0)
		{
			return;
		}
		int num = ((MyHero.MaxBombCount <= 12) ? MyHero.MaxBombCount : Math.Min(12, MyHero.MaxBombCount / 2));
		Transformation transformation = Transformation.CreateByPosition(_bombPosition);
		transformation.Position.X -= (num - 1) * 10 / 2;
		if (MyHero.IsBombDuplicatorEnabled)
		{
			_fontNormal.Write(_spriteBatch, "x 2", _bombPosition.X + (float)(5 * (num + 1)) + 3f, _bombPosition.Y - (float)((MyHero.MaxBombCount > 12) ? 2 : (-1)), new Vector2(0.4f), new Color(255, 220, 0), TextAlign.Left, 0, _hideAlphaFactor);
		}
		for (int i = 0; i < MyHero.MaxBombCount; i++)
		{
			if ((float)i < MyHero.CurrentBombCount)
			{
				_bombMeterAtlas.DrawFrame(_spriteBatch, 0, new Color(1f, 1f, 1f, _hideAlphaFactor), transformation);
			}
			else
			{
				_bombMeterAtlas.DrawFrame(_spriteBatch, 1, new Color(1f, 1f, 1f, _hideAlphaFactor * 0.5f), transformation);
			}
			transformation.Position.X += 10f;
			if (i == num - 1)
			{
				transformation.Position = _bombPosition;
				transformation.Position.X -= (MyHero.MaxBombCount - num - 1) * 10 / 2;
				transformation.Position.Y += 12f;
			}
		}
	}

	private Color GetHPColor()
	{
		if (_currShieldPercent > 0f)
		{
			return new Color(50, 200, 255);
		}
		if (_currentHPPercent < 0.25f)
		{
			return new Color(255, 50, 0);
		}
		if (_currentHPPercent < 0.5f)
		{
			return new Color(255, 200, 0);
		}
		return new Color(0, 255, 0);
	}

	private void DrawShipRecovery()
	{
		Transformation transformation = ((Options.HUDMode != HUDModeEnum.BigOnly && Options.HUDMode != HUDModeEnum.Both) ? Transformation.CreateByPosition(new Vector2(Position.X - 47f, Position.Y - 35f)) : Transformation.CreateByPosition(new Vector2(Position.X - 47f, Position.Y + 95f)));
		Color color = new Color(Color.White, _hideAlphaFactor);
		_shipRecoveryAtlas.DrawFrame(_spriteBatch, 0, color, transformation, BlendModeEnum.Additive);
		_shipRecoveryAtlas.GetFrameUV(1, out var topLeft, out var bottomRight);
		Vector2 position = transformation.Position;
		position.X -= 59f;
		position.Y -= 21f;
		float num = MyHero.ShipRecoveryValue / MyHero.MyObject.MaxHP;
		Rectangle frameRectangle = _shipRecoveryAtlas.GetFrameRectangle(1);
		_triangleBatchTextured.Begin(BlendState.Additive, GameCore.ViewPortMatrix, null, 0f);
		_triangleBatchTextured.DrawRect(position, position + new Vector2((float)frameRectangle.Width * num, frameRectangle.Height), color, _shipRecoveryAtlas.Texture, topLeft, new Vector2(topLeft.X + (bottomRight.X - topLeft.X) * num, bottomRight.Y));
		_triangleBatchTextured.End();
		if (MyHero.ShipRecoveryValue < MyHero.GetShipMaxHP())
		{
			_fontNoBorder.Write(_spriteBatch, $"{_strRecovery}: {(int)MyHero.ShipRecoveryValue}/{(int)MyHero.MyObject.MaxHP}", transformation.Position.X, transformation.Position.Y + 12f, new Vector2(0.46f), new Color(0f, 0.5f, 1f, _hideAlphaFactor), TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
		}
		else
		{
			_fontNoBorder.Write(_spriteBatch, _strShipReady, transformation.Position.X, transformation.Position.Y + 12f, new Vector2(0.46f), new Color(0f, 1f, 1f, _hideAlphaFactor), TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
		}
	}

	private void DrawHPDial()
	{
		Transformation transformation = Transformation.CreateByPosition(Position.X - 54f, Position.Y);
		float num = ((!_hpWarningActivated) ? 0.8f : (0.6f + Utils.Sin(_hpWarningTimer * 10f) * 0.4f));
		if (MyHero.HPPercent <= 0f)
		{
			_circleDialAtlas.DrawFrame(_spriteBatch, 6, new Color(1f, 1f, 1f, _hideAlphaFactor), Transformation.CreateByPosition(Position), BlendModeEnum.Normal);
		}
		else
		{
			Color hPColor = GetHPColor();
			_circleDialAtlas.DrawFrame(_spriteBatch, 3, new Color(hPColor, num * _hideAlphaFactor), transformation, BlendModeEnum.Additive);
			int num2;
			string text;
			if (MyHero.IsJetPackActive)
			{
				num2 = (int)MyHero.JetPackObject.CurrentHP;
				text = _strJetPack;
			}
			else
			{
				num2 = (int)MyHero.MyObject.CurrentHP;
				text = _strHP;
			}
			if (num2 <= 0)
			{
				num2 = 1;
			}
			_fontDigital.Write(_spriteBatch, $"{num2}", Position.X - 48f, Position.Y - 28f, Vector2.One, new Color(hPColor, 0.7f * num * _hideAlphaFactor), TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
			if (_hpWarningActivated)
			{
				_fontNoBorder.Write(_spriteBatch, _strDanger, Position.X - 48f, Position.Y + 22f, new Vector2(0.38f), new Color(1f, 0f, 0f, 0.7f * num * _hideAlphaFactor), TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
			}
			else
			{
				_fontNoBorder.Write(_spriteBatch, text, Position.X - 48f, Position.Y + 24f, new Vector2(0.38f), new Color(hPColor, 0.7f * num * _hideAlphaFactor), TextAlign.Center, 0, 1f, -1f, BlendModeEnum.Additive);
			}
		}
		transformation.Position.X += 14f;
		transformation.Position.Y += 2f;
		_circleDialAtlas.DrawFrame(_spriteBatch, 4, new Color(1f, 1f, 1f, _hideAlphaFactor), transformation, BlendModeEnum.Normal);
	}

	private void DrawHeatDial()
	{
		Transformation transformation = Transformation.CreateByPosition(Position);
		transformation.Position.X += 45f;
		transformation.Position.Y += 6f;
		if (MyHero.IsOverheated || MyHero.IsBurning)
		{
			_overheatDiodeBlink += 10f * GameCore.ElapsedTime;
			_heatDialAtlas.DrawFrame(_spriteBatch, 1, new Color(Color.White, (0.5f + Utils.Sin(_overheatDiodeBlink) / 2f) * _hideAlphaFactor), transformation, BlendModeEnum.Additive);
		}
		else
		{
			_overheatDiodeBlink = 0f;
		}
		Color color = new Color(Color.White, _hideAlphaFactor);
		transformation.Rotation = Utils.DegToRad(-152f + 300f * _heatNeedlePercent) + _heatNeedleVibration;
		_heatDialAtlas.DrawFrame(_spriteBatch, 0, color, transformation);
		transformation = Transformation.CreateByPosition(Position + new Vector2(43f, 0f));
		if (_heatFreezeAlpha > 0f)
		{
			_heatDialAtlas.DrawFrame(_spriteBatch, 3, new Color(1f, 1f, 1f, _heatFreezeAlpha * _hideAlphaFactor), transformation);
		}
		if (MyHero.IsElDiabloEnabled)
		{
			transformation = Transformation.CreateByPosition(Position + new Vector2(74f, 25f));
			_heatDialAtlas.DrawFrame(_spriteBatch, 2, color, transformation);
			color = new Color(1f, 0f, 0f, _hideAlphaFactor);
			_fontDigital.Write(_spriteBatch, $"{(int)MyHero.Heat}", transformation.Position.X, transformation.Position.Y + 3f, new Vector2(0.3f), color, TextAlign.Center, 80);
		}
	}

	private void DrawHPBar()
	{
		Vector2 vector = Position - new Vector2(50f, 0f);
		float num = Utils.DegToRad(130f);
		float num2 = (float)Math.PI / 3f;
		int frameNumber;
		float alpha;
		if (MyHero.Shield <= 0f || MyHero.IsJetPackActive)
		{
			frameNumber = 0;
			alpha = (0.7f + Utils.Sin(_hpWarningTimer * 10f) * 0.3f) * _hideAlphaFactor;
		}
		else
		{
			frameNumber = 1;
			alpha = _hideAlphaFactor;
		}
		if (MyHero.IsJetPackActive)
		{
			_circleDialAtlas.DrawFrame(_spriteBatch, 5, new Color(1f, 1f, 1f, _hideAlphaFactor), Transformation.CreateByPosition(Position.X - 110f, Position.Y - 50f), BlendModeEnum.Normal);
		}
		else if (MyHero.IsShieldEnabled)
		{
			Color color = ((!MyHero.IsShieldWaiting) ? new Color(1f, 1f, 1f, _hideAlphaFactor) : new Color(1f, 1f, 1f, (0.4f + 0.4f * Utils.Sin(_shieldChargeTimer)) * _hideAlphaFactor));
			_circleDialAtlas.DrawFrame(_spriteBatch, 2, color, Transformation.CreateByPosition(Position.X - 100f, Position.Y - 50f), BlendModeEnum.Normal);
		}
		Color color2 = new Color(Color.White, alpha);
		float num3 = Utils.DegToRad(_hpBarPercentage * 287f);
		int num4 = (int)(num3 / num2);
		float num5 = 0f;
		BatchVertextTextured[] batchVertextTextured = GameCore.Allocator.GetBatchVertextTextured(num4 + 3);
		_circleDialAtlas.GetFrameUV(frameNumber, out var topLeft, out var bottomRight);
		Rectangle frameRectangle = _circleDialAtlas.GetFrameRectangle(frameNumber);
		float num6 = (float)frameRectangle.Height / 2f;
		Vector2 vector2 = new Vector2((float)frameRectangle.Width / (float)_circleDialAtlas.Texture.Width, (float)frameRectangle.Height / (float)_circleDialAtlas.Texture.Height);
		Vector2 textureCoordinate = new Vector2(topLeft.X + (bottomRight.X - topLeft.X) / 2f, topLeft.Y - (topLeft.Y - bottomRight.Y) / 2f);
		batchVertextTextured[0] = default;
		batchVertextTextured[0].TextureCoordinate = textureCoordinate;
		batchVertextTextured[0].Position = vector;
		batchVertextTextured[0].Color = color2;
		int num7 = 1;
		int i;
		for (i = 1; i <= num4 + 1; i++)
		{
			float num8 = Utils.Sin(num5 + num);
			float num9 = 0f - Utils.Cos(num5 + num);
			batchVertextTextured[i] = default;
			batchVertextTextured[i].TextureCoordinate = new Vector2(textureCoordinate.X + num8 * vector2.X * 0.5f, textureCoordinate.Y + num9 * vector2.Y * 0.5f);
			batchVertextTextured[i].Position = vector + new Vector2(num8 * num6, num9 * num6);
			batchVertextTextured[i].Color = color2;
			num5 += num2;
			num7++;
		}
		if (num5 - num2 < num3)
		{
			float num10 = Utils.Sin(num3 + num);
			float num11 = 0f - Utils.Cos(num3 + num);
			batchVertextTextured[i] = default;
			batchVertextTextured[i].TextureCoordinate = new Vector2(textureCoordinate.X + num10 * vector2.X * 0.5f, textureCoordinate.Y + num11 * vector2.Y * 0.5f);
			batchVertextTextured[i].Position = vector + new Vector2(num10 * num6, num11 * num6);
			batchVertextTextured[i].Color = color2;
			num7++;
		}
		_triangleBatchTextured.Begin(BlendState.Additive, GameCore.ViewPortMatrix, null, 0f);
		_triangleBatchTextured.DrawFan(batchVertextTextured, num7, _circleDialAtlas.Texture);
		_triangleBatchTextured.End();
		GameCore.Allocator.DeleteBatchVertextTextured(batchVertextTextured);
	}

	private void DrawManaBar()
	{
		Color color = new Color(Color.White, _hideAlphaFactor);
		Transformation transform = Transformation.CreateByPosition(Position + new Vector2(62f, -35f));
		_manaBarAtlas.DrawFrame(_spriteBatch, 3, color, transform);
		_spriteBatch.End();
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
		Transformation transformation = Transformation.CreateByPosition(Position + new Vector2(87f, -34f));
		int frameNumber = ((_drawManaPercent < 1f) ? 1 : 2);
		_manaBarAtlas.GetFrameUV(frameNumber, out var topLeft, out var bottomRight);
		Rectangle frameRectangle = _manaBarAtlas.GetFrameRectangle(frameNumber);
		_triangleBatchTextured.Begin(BlendState.Additive, GameCore.ViewPortMatrix, null, 0f);
		_triangleBatchTextured.DrawRect(transformation.Position, transformation.Position + new Vector2((float)frameRectangle.Width * _drawManaPercent, frameRectangle.Height), color, _manaBarAtlas.Texture, topLeft, new Vector2(topLeft.X + (bottomRight.X - topLeft.X) * _drawManaPercent, bottomRight.Y));
		_triangleBatchTextured.End();
		_manaBarAtlas.DrawFrame(_spriteBatch, 0, color, transform);
	}

	public void AddHit(float rotation)
	{
		MyHero.MyObject.MyLevel.LastMainLayer.CreateObjectByPartData(_hitMarkerData, Vector2.Zero, Vector2.One, rotation, 9999, bCreateRigidBody: false).ParentObject = MyHero.MyObject;
	}

	public void Draw()
	{
		if (Options.IsMiniHUDVisible)
		{
			MiniHUD.Draw();
		}
		if (Options.ShowHeroColisoid && !MyHero.IsDestroyed)
		{
			MiniHUD.DrawColisoid();
		}
		_fontDebug = Rake2D.Editor.Editor.Font;
		if (MyHero.PlayerNumber == 0)
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			if (Options.DebugInfoMode != 0)
			{
				DrawDebugInfo();
			}
			DrawHUDCounters();
			_spriteBatch.End();
		}
		if (Options.IsBigHUDVisible)
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			_bgrAtlas.DrawFrame(_spriteBatch, 0, new Color(Color.White, _hideAlphaFactor), Transformation.CreateByPosition(Position));
			_spriteBatch.End();
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			DrawHPDial();
			DrawHPBar();
			DrawHeatDial();
			DrawManaBar();
			DrawBombMeter();
			_spriteBatch.End();
		}
		if (IsShipRecoveryDisplayed)
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
			DrawShipRecovery();
			_spriteBatch.End();
		}
	}
}
