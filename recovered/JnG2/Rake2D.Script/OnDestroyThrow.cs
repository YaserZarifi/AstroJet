namespace Rake2D.Script;

public class OnDestroyThrow : ThrowObjectBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnDestroy += OnDestroy;
	}

	public void OnDestroy()
	{
		StartThrow();
	}
}
