using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroWaterSurfaceWave : BehaviorScript
{
	private GameObject[] _waveArray;

	private float _surfaceY;

	public override void Init()
	{
		base.Init();
		_surfaceY = Param("surface_y", 1000f);
		_waveArray = new GameObject[GameJnG.HeroList.Count];
		for (int i = 0; i < _waveArray.Length; i++)
		{
			_waveArray[i] = MyLayer.CreateObjectByPartData("hero_water_surface_wave", MyObject.Position, Vector2.One, 0f);
		}
	}

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += Update;
	}

	public void Update(float elapsedTime)
	{
		for (int i = 0; i < GameJnG.HeroList.Count; i++)
		{
			Hero hero = GameJnG.HeroList[i];
			if (hero != null && hero.MyObject != null)
			{
				GameObject gameObject = _waveArray[i];
				float num = hero.MyObject.Scale.X * 1.3f - (_surfaceY - hero.MyObject.WorldPosition.Y) / 120f;
				if (!hero.MyObject.IsAlive || num <= 0.2f * hero.MyObject.Scale.X || num > 1.35f * hero.MyObject.Scale.X)
				{
					gameObject.IsEnabled = false;
					continue;
				}
				gameObject.IsEnabled = true;
				gameObject.SetPosition(new Vector2(hero.MyObject.WorldPosition.X - 130f * hero.MyObject.Scale.X + 140f * num, _surfaceY - 20f));
				gameObject.SetScale(num);
			}
		}
	}
}
