using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class CompensateScrolling : BehaviorScript
{
	private Vector2 _minScrollSpeed;

	public override void RegisterEvents()
	{
		if (MyLayer.ScrollSpeed.X > _minScrollSpeed.X || MyLayer.ScrollSpeed.Y > _minScrollSpeed.Y)
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	public override void Init()
	{
		base.Init();
		_minScrollSpeed = ParamVector2("min_speed");
	}

	public void OnUpdate(float elapsedTime)
	{
		MyObject.SetPosition(MyObject.Position + (MyLayer.ScrollSpeed - _minScrollSpeed) * elapsedTime);
	}
}
