namespace Rake2D.Editor;

internal class PartLine
{
	public Button NameButton;

	public Checkbox EnabledCheckbox;

	public void Destroy()
	{
		NameButton?.Destroy();
		EnabledCheckbox?.Destroy();
	}
}
