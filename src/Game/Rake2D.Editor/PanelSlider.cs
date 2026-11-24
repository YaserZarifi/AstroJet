namespace Rake2D.Editor;

public class PanelSlider
{
	public int CurrIndex;

	public int MaxIndex;

	private Panel _panel;

	public PanelSlider(Panel panel)
	{
		_panel = panel;
	}

	public void Update(float elepsedTime)
	{
		int mouseWheelDelta = GameInput.GetMouseWheelDelta();
		if (mouseWheelDelta < 0 && _panel.Editor.SelectedPanel == _panel && CurrIndex < MaxIndex)
		{
			CurrIndex++;
		}
		else if (mouseWheelDelta > 0 && _panel.Editor.SelectedPanel == _panel && CurrIndex > 0)
		{
			CurrIndex--;
		}
	}

	public void Draw()
	{
	}
}
