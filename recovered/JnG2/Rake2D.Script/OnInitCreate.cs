using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnInitCreate : CreateObjectBehavior
{
	public override void RegisterEvents()
	{
	}

	public override void Init()
	{
		base.Init();
		CreateObjectList(null, Vector2.Zero);
		IsDone = true;
	}
}
