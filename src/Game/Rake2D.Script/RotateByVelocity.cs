namespace Rake2D.Script;

public class RotateByVelocity : BehaviorScript
{
	public override void RegisterEvents()
	{
		if (_doInEditor && GameCore.IsEditorEnabled)
		{
			MyObject.OnEditorUpdate += Update;
		}
		else
		{
			MyObject.OnUpdate += Update;
		}
	}

	private void Update(float elapsedTime)
	{
		MyObject.SetRotation(Utils.GetRotationFromZero(MyObject.Velocity - MyLayer.ScrollSpeed));
	}
}
