using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.Font.BMFont;

namespace Rake2D.JnG;

public class ScoreHUD
{
	private double _score;

	private float _scoreTimer;

	private float _hideAlphaFactor;

	private BMFont _font;

	private SpriteBatch _spriteBatch;

	private string _strScoreTitle;

	private LevelStats _levelStats;

	public ScoreHUD()
	{
		_font = GameCore.GetBMFont("shop_font");
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
	}

	public void SetTexts()
	{
		_strScoreTitle = TextBundle.GetText("hud_score");
	}

	public void Clear()
	{
		_levelStats = GameJnG.GetLevelStatsForLevel(GameCore.GameLevel);
	}

	public void Update(float elapsedTime)
	{
		if ((double)GameJnG.LevelScore < _score || Options.ScreenShotModeEnabled)
		{
			_score = GameJnG.LevelScore;
			_scoreTimer = 0f;
		}
		else
		{
			Utils.ChangeValueToTarget(ref _score, GameJnG.LevelScore, 50.0 * Utils.Clamp((double)GameJnG.LevelScore - _score, 100.0, 9.223372036854776E+18) * (double)elapsedTime);
			if ((double)GameJnG.LevelScore > _score)
			{
				_scoreTimer = 1f;
			}
			else
			{
				Utils.ChangeValueToTarget(ref _scoreTimer, 0f, 5f * elapsedTime);
			}
		}
		bool flag = false;
		foreach (Hero hero in GameJnG.HeroList)
		{
			GameObject controlledObject = hero.GetControlledObject();
			if (controlledObject != null && Utils.IsPointInFixedRect(controlledObject.VirtualScreenPosition, new Vector2(960f, 50f), 600f, 150f))
			{
				flag = true;
			}
		}
		if (GameJnG.BossMeterList.Count > 0)
		{
			Utils.ChangeValueToTarget(ref _hideAlphaFactor, 0f, 3f * elapsedTime);
		}
		else
		{
			Utils.ChangeValueToTarget(ref _hideAlphaFactor, flag ? 0.3f : 1f, 3f * elapsedTime);
		}
	}

	public void Draw()
	{
		float x = 960f;
		float num = 30f;
		_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, GameCore.ViewPortMatrix);
		string arg = Utils.FormatMoney((long)_score);
		if (_scoreTimer > 0f)
		{
			_font.Write(_spriteBatch, $"{_strScoreTitle}: {arg}", x, num - 4f * _scoreTimer, new Vector2(1f + 0.1f * _scoreTimer, 1f + 0.3f * _scoreTimer), new Color(255, 255, (int)(255f * _scoreTimer)), TextAlign.Center, 0, _hideAlphaFactor);
		}
		else
		{
			_font.Write(_spriteBatch, $"{_strScoreTitle}: {arg}", x, num, Vector2.One, new Color(255, 255, 0), TextAlign.Center, 0, _hideAlphaFactor);
		}
		num += 35f;
		if (_levelStats != null && _levelStats.FinishCountInCampaign > 0)
		{
			if ((double)_levelStats.TopScore > _score)
			{
				_font.Write(_spriteBatch, string.Format("{0}: {1}", TextBundle.GetText("hud_score_remains_to_record"), Utils.FormatMoney((long)((double)_levelStats.TopScore - _score))), x, num, new Vector2(0.5f), new Color(230, 190, 20), TextAlign.Center, 0, _hideAlphaFactor);
			}
			else
			{
				_font.Write(_spriteBatch, string.Format("{0}: {1}", TextBundle.GetText("hud_score_over_record"), Utils.FormatMoney((long)(_score - (double)_levelStats.TopScore))), x, num, new Vector2(0.5f), new Color(40, 200, 255), TextAlign.Center, 0, _hideAlphaFactor);
			}
		}
		_spriteBatch.End();
	}
}
