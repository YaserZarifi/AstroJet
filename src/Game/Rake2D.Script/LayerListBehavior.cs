using System.Collections.Generic;

namespace Rake2D.Script;

public abstract class LayerListBehavior : BehaviorScript
{
	protected List<Layer> _layerList;

	public override void Init()
	{
		base.Init();
		string[] array = ParamStringArray("layer_id");
		_layerList = new List<Layer>();
		if (array == null)
		{
			return;
		}
		string[] array2 = array;
		foreach (string layerId in array2)
		{
			Layer layer = MyLevel.LayerList.Find((Layer l) => Utils.Equals(l.Id, layerId));
			if (layer != null)
			{
				_layerList.Add(layer);
			}
		}
	}
}
