using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class KeepPosition : BehaviorScript
{
	private Vector2 _position;

	private Vector2 _offset;

	private GameObject _parentObject;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
	}

	public override void Init()
	{
		base.Init();
		_position = ParamVector2("position", -999f, -999f);
		_parentObject = MyObject.ParentObject;
		if (_parentObject != null)
		{
			_offset = MyObject.Position;
			MyObject.ParentTransformation = null;
		}
	}

	public void OnUpdate(float elapsedTime)
	{
		if (_parentObject == null)
		{
			MyObject.SetPosition((_position.X != -999f) ? _position.X : MyObject.Position.X, (_position.Y != -999f) ? _position.Y : MyObject.Position.Y);
			return;
		}
		if (_parentObject.IsDestroyed)
		{
			MyObject.Destroy();
		}
		MyObject.SetPosition((_position.X != -999f) ? _position.X : (_parentObject.WorldPosition.X + _offset.X), (_position.Y != -999f) ? _position.Y : (_parentObject.WorldPosition.Y + _offset.Y));
	}
}
