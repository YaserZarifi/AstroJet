using Microsoft.Xna.Framework;

namespace Rake2D.Script;

internal class OnTriggerCreate : CreateObjectBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnTrigger += OnTrigger;
	}

	private void OnTrigger()
	{
		MyObject.OnTrigger -= OnTrigger;
		CreateObjectList(null, Vector2.Zero);
	}
}
