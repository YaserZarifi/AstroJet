using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnActivateCreate : CreateObjectBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnActivate += OnActivate;
	}

	private void OnActivate()
	{
		CreateObjectList(null, Vector2.Zero);
		MyObject.OnActivate -= OnActivate;
	}
}
