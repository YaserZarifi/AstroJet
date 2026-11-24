using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.JnG;

public class MouseGun : Transformation
{
	public bool IsActive;

	public bool IsGunTriggerPressed;

	private SpriteAtlas _atlas;

	private SpriteBatch _spriteBatch;

	private Hero _myHero;

	private Vector2 _oldMousePos;

	private Vector2 _scrrenPos;

	private Vector2 _stickMove;

	private bool _isMovedByMouse;

	public MouseGun(Hero hero)
	{
		_atlas = GameCore.GetSpriteAtlas("mouse_gun");
		_spriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		_myHero = hero;
	}

	public void Update(float elapsedTime)
	{
		IsGunTriggerPressed = false;
		if (GameCore.IsPaused)
		{
			return;
		}
		GameObject gameObject = ((!_myHero.IsJetPackActive) ? _myHero.MyObject : _myHero.JetPackObject);
		Vector2 vector = new Vector2(GameInput.MouseX, GameInput.MouseY);
		if (Options.PlayerController[_myHero.PlayerNumber] == GameInput.PlayerControllerEnum.Mouse)
		{
			Vector2 vector2 = ((!_myHero.KeyDown(GameInput.KeyTypeEnum.FireGuns)) ? GameInput.MouseMoveDelta : Vector2.Zero);
			Vector2 value = gameObject.WorldPosition + _stickMove;
			value = Utils.Clamp(value, gameObject.MyCamera.Position, gameObject.MyCamera.Position + new Vector2(1920f, 1080f));
			_stickMove -= gameObject.WorldPosition + _stickMove - value;
			_stickMove += vector2;
			_stickMove = Utils.Clamp(_stickMove, new Vector2(-300f, -300f), new Vector2(300f, 300f));
			float num = _stickMove.Length();
			if (num > 300f)
			{
				_stickMove /= num / 300f;
			}
			IsActive = true;
			Position = gameObject.WorldPosition + _stickMove;
			Alpha = byte.MaxValue;
		}
		else
		{
			_scrrenPos = vector;
			Vector2 rightStickPos = GameInput.GetRightStickPos(_myHero.PlayerNumber);
			if (rightStickPos.Length() > 0.2f)
			{
				_stickMove = rightStickPos * new Vector2(1f, -1f);
				IsGunTriggerPressed = true;
				_isMovedByMouse = false;
			}
			else if (vector - _oldMousePos != Vector2.Zero)
			{
				_oldMousePos = vector;
				Alpha = byte.MaxValue;
				IsActive = true;
				_isMovedByMouse = true;
			}
			else
			{
				Utils.ChangeValueToTarget(ref _stickMove, Vector2.Zero, 5f * elapsedTime);
			}
			if (_isMovedByMouse)
			{
				Position = gameObject.MyCamera.ScreenPosToMainLayer(_scrrenPos) - gameObject.MyCamera.QuakeOffset;
			}
			else
			{
				Position = Utils.GetVelocityVector2(300f * _stickMove.Length(), Utils.GetRotationFromZero(_stickMove)) + gameObject.Position + gameObject.MyLayer.ScrollSpeed * elapsedTime;
				Alpha = (byte)(255f * Utils.Clamp(_stickMove.Length(), 0f, 1f));
				IsActive = Alpha > 10;
			}
		}
		Position = Utils.Clamp(Position, gameObject.MyCamera.Position, gameObject.MyCamera.Position + new Vector2(1920f, 1080f));
	}

	public void Draw()
	{
		if (_myHero.MyObject != null && _myHero.MyObject.MyLevel.CutsceneMode == 0)
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, _myHero.MyObject.MyLevel.Camera.WorldMatrixMainLayer);
			_atlas.DrawFrame(_spriteBatch, 0, Color, this, BlendModeEnum.Normal);
			_spriteBatch.End();
		}
	}
}
