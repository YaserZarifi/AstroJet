using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class TVNoise : Transformation
{
	public bool IsAutoHiding;

	public float FadeSpeed = 750f;

	public FloatRange StripOffsetYRange;

	public TVNoiseTypeEnum Type;

	private ShopMenu _shopMenu;

	private SpriteAtlas _atlas;

	private float _alpha;

	private float _flipTimer;

	private float _stripTimer;

	private float _stripOffsetY;

	private float _targetAlpha;

	private float _stripSpeed;

	private float _stripStartDelay;

	private int _stripFrame;

	public bool IsShowing
	{
		get
		{
			if (_alpha >= 0f)
			{
				return _targetAlpha > 0f;
			}
			return false;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (_alpha > 0f)
			{
				return _alpha == _targetAlpha;
			}
			return false;
		}
	}

	public bool IsHiding
	{
		get
		{
			if (_alpha > 0f)
			{
				return _targetAlpha <= 0f;
			}
			return false;
		}
	}

	public bool IsHidden => _alpha <= 0f;

	public TVNoise(ShopMenu shopMenu, TVNoiseTypeEnum type = TVNoiseTypeEnum.Normal)
	{
		_shopMenu = shopMenu;
		_alpha = 0f;
		_stripFrame = 1;
		_stripSpeed = 75f;
		_targetAlpha = 0f;
		Type = type;
		StripOffsetYRange = new FloatRange(108f, -105f);
		_stripOffsetY = StripOffsetYRange.From;
	}

	public void Init()
	{
		switch (Type)
		{
		case TVNoiseTypeEnum.Normal:
			_atlas = GameCore.GetSpriteAtlas("shop_tv_noise");
			break;
		case TVNoiseTypeEnum.PlayerFace:
			_atlas = GameCore.GetSpriteAtlas("shop_tv_noise_player_face");
			break;
		}
	}

	public void Show()
	{
		_targetAlpha = 255f;
	}

	public void Hide()
	{
		_targetAlpha = 0f;
	}

	private void UpdateNoiseStrip(float elapsedTime)
	{
		_stripStartDelay -= elapsedTime;
		if (!(_stripStartDelay <= 0f))
		{
			return;
		}
		_stripTimer -= elapsedTime;
		if (_stripTimer <= 0f)
		{
			_stripTimer = 0.075f;
			_stripFrame++;
			if (_stripFrame > 3)
			{
				_stripFrame = 1;
			}
		}
		if (Utils.ChangeValueToTarget(ref _stripOffsetY, StripOffsetYRange.To, _stripSpeed * elapsedTime))
		{
			_stripOffsetY = StripOffsetYRange.From;
			_stripSpeed = Utils.GetRandom(50, 90);
			_stripStartDelay = Utils.GetRandom(1, 3);
		}
	}

	public void Update(float elapsedTime)
	{
		ResetPrecalculations();
		if (Utils.ChangeValueToTarget(ref _alpha, _targetAlpha, elapsedTime * FadeSpeed) && IsAutoHiding && _targetAlpha > 0f)
		{
			Hide();
		}
		if (!IsHidden)
		{
			_flipTimer -= elapsedTime;
			if (_flipTimer < 0f)
			{
				_flipTimer = 0.08f;
				switch (Utils.GetRandom(2))
				{
				case 0:
					Scale.X *= -1f;
					break;
				case 1:
					Scale.Y *= -1f;
					break;
				default:
					Scale.X *= -1f;
					Scale.Y *= -1f;
					break;
				}
			}
		}
		UpdateNoiseStrip(elapsedTime);
	}

	public void Draw()
	{
		if (!IsHidden)
		{
			_atlas.DrawFrame(_shopMenu.SpriteBatchLight, 0, new Color(255, 255, 255, (int)(_alpha * ((ParentTransformation != null) ? ((float)(int)ParentTransformation.Alpha / 255f) : 1f))), WorldTransformation);
		}
		if (_stripStartDelay <= 0f)
		{
			Transformation transformation = Transformation.CreateByPosition(WorldPosition);
			transformation.Position.Y += _stripOffsetY;
			transformation.Scale.X = 2.9f;
			_atlas.DrawFrame(_shopMenu.SpriteBatchLight, _stripFrame, new Color(150, 150, 140, (int)((ParentTransformation != null) ? ParentTransformation.Alpha : byte.MaxValue)), transformation, BlendModeEnum.Additive);
		}
	}
}
