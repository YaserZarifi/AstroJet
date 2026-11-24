using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class OnStateCreateInLayer : CreateObjectBehavior
{
	private Layer _targetLayer;

	public override void Init()
	{
		base.Init();
		string layerId = ParamString("layer_id");
		if (layerId != null)
		{
			_targetLayer = MyLevel.LayerList.Find((Layer l) => Utils.Equals(l.Id, layerId));
		}
	}

	public override void RegisterEvents()
	{
		if (DoInEditor)
		{
			CreateObjectList(null, Vector2.Zero, _targetLayer);
		}
	}
}
