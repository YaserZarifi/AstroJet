namespace Rake2D.Script;

public class FollowLinkedX : BehaviorScript
{
	private GameObject _targetObject;

	private float _startOffset;

	public override void RegisterEvents()
	{
		if (_targetObject != null)
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	public override void Init()
	{
		base.Init();
		if (MyObject.LinkedObjectList != null && MyObject.LinkedObjectList.Count > 0)
		{
			_targetObject = MyObject.LinkedObjectList[0];
			_startOffset = MyObject.WorldPosition.X - _targetObject.WorldPosition.X;
		}
	}

	public void OnUpdate(float elapsedTime)
	{
		if (_targetObject.IsDestroyed)
		{
			MyObject.Destroy();
		}
		else
		{
			MyObject.SetPosition(_targetObject.WorldPosition.X + _startOffset, MyObject.Position.Y);
		}
	}
}
