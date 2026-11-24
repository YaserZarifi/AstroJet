using Microsoft.Xna.Framework;

namespace Rake2D.Script.Man;

public class OnKillBornMan : BehaviorScript
{
	private IntRange _manCount;

	private int _manType;

	public override void RegisterEvents()
	{
		MyObject.OnKill += OnKill;
	}

	private void OnKill()
	{
		Vector2Range vector2Range = ParamVector2Range("offset");
		_manCount = ParamIntRange("count");
		_manType = ParamInt("type");
		int manType = _manType;
		PartData partData = ((manType != 2) ? GameCore.GetPartData("man") : GameCore.GetPartData("man_parachute_open"));
		for (int num = _manCount.GetRandom(); num > 0; num--)
		{
			Vector2 random = vector2Range.GetRandom();
			GameObject gameObject = MyLayer.MyLevel.MainLayer.CreateObjectByPartData(partData, MyObject.WorldPosition + random, Vector2.One, 0f, 20);
			manType = _manType;
			if (manType == 2)
			{
				gameObject.SetVelocity(Utils.GetRandom(-200, 200), Utils.GetRandom(-250, -600));
			}
			else
			{
				gameObject.SetVelocity(Utils.GetRandom(-100, 100), Utils.GetRandom(-150, -200));
			}
			gameObject.SetFlip(gameObject.VelocityX > 0f, vertical: false);
		}
	}
}
