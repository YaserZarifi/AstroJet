using Rake2D.JnG;

namespace Rake2D.Script;

public class OnHeroDistanceExit : ExitStateBehavior
{
	private float _minDistance;

	private float _maxDistance;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_minDistance = Param("min_distance", -1f);
		_maxDistance = Param("max_distance", 9999999f);
	}

	private void OnUpdate(float elepsedTime)
	{
		foreach (Hero hero in GameJnG.HeroList)
		{
			float distance = Utils.GetDistance(MyObject.WorldPosition, hero.MyObject.WorldPosition);
			if (distance <= _minDistance || distance >= _maxDistance)
			{
				MyObject.OnUpdate -= OnUpdate;
				StartExit();
				break;
			}
		}
	}
}
