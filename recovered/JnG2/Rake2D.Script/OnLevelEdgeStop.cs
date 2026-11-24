namespace Rake2D.Script;

public class OnLevelEdgeStop : BehaviorScript
{
	private float _distance;

	public override void RegisterEvents()
	{
		if (MyObject.IsMainPart)
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	public override void Init()
	{
		base.Init();
		_distance = Param("distance");
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.Position.Y < _distance)
		{
			MyObject.Position.Y = _distance;
			if (MyObject.VelocityY < 0f)
			{
				MyObject.VelocityY = 0f;
			}
		}
		else if (MyObject.Position.Y > MyLevel.Size.Y - _distance)
		{
			MyObject.Position.Y = MyLevel.Size.Y - _distance;
			if (MyObject.VelocityY > 0f)
			{
				MyObject.VelocityY = 0f;
			}
		}
	}
}
