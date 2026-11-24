using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class KeepCameraPosition : BehaviorScript
{
	private Vector2 _position;

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
	}

	public void OnUpdate(float elapsedTime)
	{
		MyObject.SetPosition((_position.X != -999f) ? (_position.X + MyLevel.Camera.Position.X) : MyObject.Position.X, (_position.Y != -999f) ? (_position.Y + MyLevel.Camera.Position.Y) : MyObject.Position.Y);
	}
}
