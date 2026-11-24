using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ScaleByCameraY : BehaviorScript
{
	private Vector2 _originalScale;

	private float scaleFactor;

	public override void Init()
	{
		base.Init();
		_originalScale = new Vector2(MyObject.Scale.X, 1f);
		scaleFactor = Param("scale_factor", 30f);
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		if (DoInEditor)
		{
			MyObject.OnEditorUpdate += OnUpdate;
		}
	}

	private void OnUpdate(float elapsedTime)
	{
		MyObject.SetScale(_originalScale.X, _originalScale.Y * (MyObject.VirtualScreenPosition.Y - 540f) / 1080f / 2f * scaleFactor);
	}
}
