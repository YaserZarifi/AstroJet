namespace Rake2D.Script;

public class CalcHPFromLinked : BehaviorScript
{
	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	private void OnUpdate(float elapsedTime)
	{
		float maxHP = MyObject.MaxHP;
		MyObject.MaxHP = 0f;
		MyObject.CurrentHP = 0f;
		if (MyObject.LinkedObjectList != null)
		{
			foreach (GameObject linkedObject in MyObject.LinkedObjectList)
			{
				MyObject.MaxHP += linkedObject.MaxHP;
				MyObject.CurrentHP += linkedObject.CurrentHP;
			}
		}
		if (maxHP > MyObject.MaxHP)
		{
			MyObject.MaxHP = maxHP;
		}
	}
}
