using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.JnG;

public class CarnageTV
{
	private GameObject _hudObject;

	private GameObject _signPart;

	private GameObject _counterPart;

	private GameObject _shakePart;

	private GameObject _carnagePart;

	private HUD _hud;

	private int _killCounter;

	private int _carnageLevel;

	private float _killTimer;

	private string _signText;

	private string _counterText;

	private const int MAX_CARNAGE_LEVEL = 7;

	public CarnageTV(HUD hud)
	{
		_hud = hud;
		Vector2 pos = ((GameJnG.PlayerCount <= 1) ? new Vector2(1920f, 0f) : new Vector2(1920f, 120f));
		_hudObject = _hud.HUDLevel.MainLayer.CreateObjectByPartData("carnage_tv", pos, Vector2.One, 0f, 0, bCreateRigidBody: false);
		_signPart = _hudObject.GetChildByIdDeep("sign");
		_counterPart = _hudObject.GetChildByIdDeep("count");
		_shakePart = _hudObject.GetChildByIdDeep("count_shake");
		_carnagePart = _hudObject.GetChildByIdDeep("carnage");
		_signPart.OnReRegisterEvents += OnReRegisterEventsSign;
		_counterPart.OnReRegisterEvents += OnReRegisterEventsCounter;
		_hud.MyHero.MyObject.MyLevel.OnObjectKill += OnKill;
	}

	private void OnKill(GameObject killedObject)
	{
		if (killedObject.TargetClass != TargetClass.Man || !killedObject.IsMainPart)
		{
			return;
		}
		if (++_killCounter >= 5)
		{
			_counterText = $"x {_killCounter}";
			_shakePart.ChangePartState("shake");
		}
		_killTimer = 1.5f;
		bool flag = false;
		switch (_killCounter)
		{
		case 5:
			_carnageLevel = 1;
			flag = true;
			break;
		case 10:
			_carnageLevel = 2;
			flag = true;
			break;
		case 25:
			_carnageLevel = 3;
			flag = true;
			break;
		case 50:
			_carnageLevel = 4;
			flag = true;
			break;
		case 100:
			_carnageLevel = 5;
			flag = true;
			break;
		case 250:
			_carnageLevel = 6;
			flag = true;
			break;
		case 500:
			_carnageLevel = 7;
			flag = true;
			GameJnG.UnlockAchievement(GameAchievementEnum.ACH_CARNAGE_TV_MAX);
			break;
		}
		if (flag)
		{
			_signText = $"{_carnageLevel} - " + TextBundle.GetText($"carnage_tv_level_{_carnageLevel}");
			GameCore.PlaySound("carnage_tv_next_level");
			_hudObject.ChangePartState("sign_in");
			if (_carnageLevel >= 7)
			{
				_carnagePart.ChangePartState("carnage");
			}
		}
	}

	private void OnReRegisterEventsSign(GameObject go)
	{
		_signPart.OnDraw += OnDrawSign;
	}

	private void OnReRegisterEventsCounter(GameObject go)
	{
		_counterPart.OnDraw += OnDrawCounter;
	}

	private void OnDrawSign(SpriteBatch spriteBatch)
	{
		if (_signText != null)
		{
			GameCore.FontNoBorder.Write(spriteBatch, _signText, _signPart.WorldPosition.X + 3f, _signPart.WorldPosition.Y - 6f, new Vector2(0.6f), new Color(0, 0, 0, (int)((float)(200 * _signPart.Color.A) / 255f)), TextAlign.Center, 0, 1f, -1f, _signPart.PartData.BlendMode);
			GameCore.FontNoBorder.Write(spriteBatch, _signText, _signPart.WorldPosition.X, _signPart.WorldPosition.Y - 9f, new Vector2(0.6f), _signPart.Color, TextAlign.Center, 0, 1f, -1f, _signPart.PartData.BlendMode);
		}
	}

	private void OnDrawCounter(SpriteBatch spriteBatch)
	{
		if (_counterText != null)
		{
			GameCore.FontNoBorder.Write(spriteBatch, _counterText, _counterPart.WorldPosition.X + 3f, _counterPart.WorldPosition.Y - 6f, new Vector2(0.6f) * _counterPart.WorldScale, new Color(0, 0, 0, (int)((float)(200 * _counterPart.Color.A) / 255f)), TextAlign.Center, 0, 1f, -1f, _signPart.PartData.BlendMode);
			GameCore.FontNoBorder.Write(spriteBatch, _counterText, _counterPart.WorldPosition.X, _counterPart.WorldPosition.Y - 9f, new Vector2(0.6f) * _counterPart.WorldScale, _counterPart.Color, TextAlign.Center, 0, 1f, -1f, _signPart.PartData.BlendMode);
		}
	}

	public void SetTexts()
	{
	}

	public void Update(float elapsedTime)
	{
		if (!(_killTimer > 0f) || !Utils.ChangeValueToTarget(ref _killTimer, 0f, elapsedTime))
		{
			return;
		}
		if (_carnageLevel > 0)
		{
			GameCore.PlaySound("carnage_tv_score");
			int num = _killCounter * 5;
			switch (_carnageLevel)
			{
			case 1:
				num += 50;
				break;
			case 2:
				num += 120;
				break;
			case 3:
				num += 450;
				break;
			case 4:
				num += 1000;
				break;
			case 5:
				num += 2500;
				break;
			case 6:
				num += 7000;
				break;
			case 7:
				num += 15000;
				break;
			}
			num = (int)((float)num * FactorManager.ScoreFactor);
			GameJnG.LevelScore += num;
			_counterText = $"+{num} ✐";
			_hudObject.ChangePartState("sign_out");
			_carnagePart.ChangePartState("idle");
		}
		else
		{
			_counterText = null;
			_signText = null;
		}
		_killCounter = 0;
		_killTimer = 0f;
		_carnageLevel = 0;
	}
}
