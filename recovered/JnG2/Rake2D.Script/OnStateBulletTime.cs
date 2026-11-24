using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class OnStateBulletTime : ExitStateBehavior
{
	public override void RegisterEvents()
	{
		if (DoInEditor && !GameJnG.IsTimeCompressorEnabled && !MyLevel.NoBulletTime)
		{
			GameCore.TargetTimeFactor = MathHelper.Clamp(Param("factor", 100f) / 100f, 0.1f, 2f);
			if (GameCore.TargetTimeFactor < 1f)
			{
				GameCore.TimeFactorChangeSpeed = 1.75f;
			}
			else
			{
				GameCore.TimeFactorChangeSpeed = 0.75f;
			}
			StartExit();
		}
	}
}
