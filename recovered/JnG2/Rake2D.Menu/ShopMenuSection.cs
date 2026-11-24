namespace Rake2D.Menu;

public abstract class ShopMenuSection : Transformation
{
	public bool HasFocus;

	public virtual void SetFocus(bool focus)
	{
		HasFocus = focus;
	}

	public virtual void CancelCursorSelection()
	{
	}
}
