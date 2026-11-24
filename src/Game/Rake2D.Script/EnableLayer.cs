namespace Rake2D.Script;

public class EnableLayer : LayerListBehavior
{
	public override void RegisterEvents()
	{
		MyObject.OnActivate += OnActivate;
	}

	private void OnActivate()
	{
		if (GameCore.IsEditorEnabled)
		{
			return;
		}
		foreach (Layer layer in _layerList)
		{
			layer.IsEnabled = true;
		}
	}
}
