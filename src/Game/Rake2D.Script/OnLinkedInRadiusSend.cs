namespace Rake2D.Script;

public class OnLinkedInRadiusSend : BehaviorScript
{
	private float _radius;

	private int _value;

	private string _linkOut;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
	}

	public override void Init()
	{
		_linkOut = ParamString("link_out");
		_value = ParamInt("link_value");
		_radius = Param("radius");
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MyObject.LinkedObjectList == null)
		{
			return;
		}
		for (int num = MyObject.LinkedObjectList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = MyObject.LinkedObjectList[num];
			if (Utils.GetDistance(gameObject.WorldPosition, MyObject.WorldPosition) <= _radius)
			{
				gameObject.DoOnLink(MyObject, _linkOut, _value, bApplyChildren: true);
				MyObject.LinkedObjectList.RemoveAt(num);
			}
		}
	}
}
