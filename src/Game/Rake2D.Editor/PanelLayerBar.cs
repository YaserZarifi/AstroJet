using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class PanelLayerBar : Panel
{
	private SpriteAtlas _atlasButton;

	public PanelLayerBar(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		_atlasButton = GameCore.GlobalContent.GetSpriteAtlas("editor_button");
	}

	public void Refresh()
	{
		RemoveAllComponents();
		int num = TitleBarHeight + 1;
		int num2 = 0;
		foreach (Layer layer in Editor.CurrentLevel.LayerList)
		{
			num2++;
			Button button = new Button(_atlasButton, this);
			button.Caption = string.Concat(num2);
			button.UserData = layer;
			button.HasClickEffect = false;
			button.OnClick += OnButtonClick;
			button.OnDoubleClick += OnButtonDoubleClick;
			button.OnRightClick += OnButtonRightClick;
			if (layer.EditorButtonColor != Color.Black)
			{
				button.BackgroundColor = layer.EditorButtonColor;
				if (layer == Editor.CurrentLevel.MainLayer)
				{
					button.ForegroundColor = new Color(255, 240, 0, 255);
				}
				else
				{
					button.ForegroundColor = new Color(200, 200, 200, 255);
				}
			}
			else if (layer == Editor.CurrentLevel.MainLayer)
			{
				button.BackgroundColor = new Color(100, 90, 70, 240);
				button.ForegroundColor = new Color(255, 240, 0, 255);
				button.Caption = "M";
			}
			else if (layer.ScrollFactor.X < Editor.CurrentLevel.MainLayer.ScrollFactor.X)
			{
				button.BackgroundColor = new Color(70, 110, 140, 240);
				button.ForegroundColor = new Color(200, 200, 200, 255);
			}
			else if (layer.ScrollFactor.X > Editor.CurrentLevel.MainLayer.ScrollFactor.X)
			{
				button.BackgroundColor = new Color(70, 120, 90, 240);
				button.ForegroundColor = new Color(200, 200, 200, 255);
			}
			else
			{
				button.BackgroundColor = new Color(100, 70, 50, 240);
				button.ForegroundColor = new Color(200, 200, 200, 255);
			}
			if (layer.EditorButtonCaption != null)
			{
				button.Caption = layer.EditorButtonCaption;
			}
			button.Tooltip = $"Layer: {layer.Id}";
			if (layer == Editor.CurrentLayer)
			{
				button.AtlasFrameNumber = 1;
				button.SetHighlited();
				button.BackgroundColor = new Color(190, 20, 0, 240);
			}
			else
			{
				button.AtlasFrameNumber = 0;
			}
			if (!layer.IsEnabledByEditor)
			{
				button.Enabled = false;
			}
			else
			{
				button.Enabled = true;
			}
			button.Position.X = 2f;
			button.Position.Y = num;
			button.Width = 32;
			button.Height = 32;
			num += button.Height;
		}
		Height = num + 2;
	}

	private bool IsOtherLayerEnabled()
	{
		foreach (Layer layer in Editor.CurrentLevel.LayerList)
		{
			if (layer != Editor.CurrentLayer && layer.IsEnabledByEditor)
			{
				return true;
			}
		}
		return false;
	}

	private void OnButtonClick(Button button)
	{
		bool flag = IsOtherLayerEnabled();
		if (!(button.UserData.GetType() == typeof(Layer)))
		{
			return;
		}
		Editor.UnselectAll(unselectCurrentObject: false);
		Editor.CurrentLayer = button.UserData as Layer;
		if (!flag)
		{
			foreach (Layer layer in Editor.CurrentLevel.LayerList)
			{
				layer.IsEnabledByEditor = false;
			}
			Editor.CurrentLayer.IsEnabledByEditor = true;
		}
		Editor.RequestRefreshLayerBar();
	}

	private void OnButtonDoubleClick(Button button)
	{
		bool flag = IsOtherLayerEnabled();
		foreach (Layer layer in Editor.CurrentLevel.LayerList)
		{
			if (layer != Editor.CurrentLayer)
			{
				layer.IsEnabledByEditor = !flag;
			}
		}
		Editor.CurrentLayer.IsEnabledByEditor = true;
		Editor.RequestRefreshLayerBar();
	}

	private void OnButtonRightClick(Button button)
	{
		Layer layer = button.UserData as Layer;
		layer.IsEnabledByEditor = !layer.IsEnabledByEditor;
		button.Enabled = layer.IsEnabledByEditor;
		Editor.RequestRefreshLayerBar();
	}
}
