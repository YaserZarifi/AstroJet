namespace Rake2D.Script;

public class OnBombCollisionThrow : ItemBehaviorScript
{
	private BehaviorData _behData;

	public override void StartInSlot()
	{
		MyHero.OnBombStart += OnBombStart;
		_behData = new BehaviorData();
		_behData.SetTypeName("OnCollisionThrow");
		_behData.AddParametersFromItemBehaviorData(BehaviorData, MyItem.Level);
		_behData.SetParam("repeat_delay", new float[1] { 200f });
		float[] floats;
		if ((floats = ParamArray("max_bounce")) != null)
		{
			_behData.SetParam("max_bounce", floats);
		}
		if ((floats = ParamArray("min_bounce_speed")) != null)
		{
			_behData.SetParam("min_speed", floats);
		}
	}

	private void OnBombStart(GameObject go)
	{
		float[] floats = _behData.GetFloats("bomb_damage_percent");
		if (floats != null && floats.Length != 0)
		{
			float num = floats[0] / 100f;
			float num2 = go.Damage * num;
			floats = _behData.GetFloats("damage");
			if (floats == null || floats.Length != 1 || floats[0] != num2)
			{
				_behData.SetParam("damage", new float[1] { num2 });
			}
		}
		BehaviorScript behaviorScript = go.AddBehaviorScript(_behData, bAsFirst: true);
		behaviorScript.Init();
		behaviorScript.RegisterEvents();
	}
}
