using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class ObjectMover
{
	public GameObject MyObject;

	public float Speed;

	private Vector2 _originalPosition;

	private Vector2 _originalVelocity;

	private Vector2 _centerPosition;

	private Vector2 _offset;

	private Editor _editor;

	private float _radius;

	private float _rotation;

	private bool _originalIsMovable;

	private ObjectMoverModeEnum _mode;

	public bool IsActive => MyObject != null;

	public ObjectMover(Editor editor)
	{
		_editor = editor;
		Speed = 1f;
		_radius = 400f;
		_mode = ObjectMoverModeEnum.Circle;
	}

	public void Start(GameObject go)
	{
		if (MyObject != null)
		{
			Stop();
		}
		if (MyObject != go)
		{
			_originalIsMovable = go.IsMovable;
			_originalPosition = go.WorldPosition;
			_originalVelocity = go.Velocity;
			MyObject = go;
			MyObject.SetScreenPosition(new Vector2(GameCore.MainViewPort.Width / 2, GameCore.MainViewPort.Height / 2));
			_centerPosition = MyObject.WorldPosition;
			MyObject.IsMovable = true;
			_rotation = 0f;
		}
	}

	public void RefresObject(GameObject go)
	{
		if (MyObject != go)
		{
			_originalIsMovable = go.IsMovable;
			MyObject = go;
			MyObject.IsMovable = true;
		}
	}

	public void Stop()
	{
		if (MyObject != null)
		{
			MyObject.SetPosition(_originalPosition);
			MyObject.IsMovable = _originalIsMovable;
			MyObject.SetVelocity(_originalVelocity);
			MyObject = null;
		}
	}

	public void NextMode()
	{
		switch (_mode)
		{
		case ObjectMoverModeEnum.Circle:
			_mode = ObjectMoverModeEnum.Vertical;
			break;
		case ObjectMoverModeEnum.Vertical:
			_mode = ObjectMoverModeEnum.Horizontal;
			break;
		case ObjectMoverModeEnum.Horizontal:
			_mode = ObjectMoverModeEnum.Circle;
			break;
		}
	}

	public void Update(float elapsedTime)
	{
		if (MyObject == null)
		{
			return;
		}
		if (MyObject.IsDestroyed)
		{
			MyObject = null;
			return;
		}
		_rotation += Speed * elapsedTime;
		switch (_mode)
		{
		case ObjectMoverModeEnum.Circle:
			_offset = new Vector2(_radius * Utils.Sin(_rotation), (0f - _radius) * Utils.Cos(_rotation));
			break;
		case ObjectMoverModeEnum.Horizontal:
			_offset = new Vector2(_radius * Utils.Sin(_rotation), 0f);
			break;
		case ObjectMoverModeEnum.Vertical:
			_offset = new Vector2(0f, (0f - _radius) * Utils.Cos(_rotation));
			break;
		}
		Vector2 worldPosition = MyObject.WorldPosition;
		MyObject.SetPosition(_centerPosition + _offset);
		if (elapsedTime > 0f)
		{
			Vector2 vector = (MyObject.WorldPosition - worldPosition) / elapsedTime;
			MyObject.SetVelocity(vector);
			MyObject.SetRotation(Utils.GetRotationFromZero(vector));
		}
	}
}
