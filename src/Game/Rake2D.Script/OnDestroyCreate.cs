using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnDestroyCreate : CreateObjectBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnDestroy += OnDestroy;
	}

	public void OnDestroy()
	{
		CreateObjectList(null, Vector2.Zero);
	}
}
