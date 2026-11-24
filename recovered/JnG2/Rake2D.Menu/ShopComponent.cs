namespace Rake2D.Menu;

public abstract class ShopComponent : Transformation
{
	public bool IsSelected;

	public bool IsActivated;

	public string SelectSoundId;

	public int GroupIndex;

	protected ShopMenu _shopMenu;

	public ShopComponent(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
	}

	public virtual void Select()
	{
		if (!IsSelected)
		{
			if (SelectSoundId != null)
			{
				GameCore.PlaySound(SelectSoundId);
			}
			IsSelected = true;
		}
	}

	public virtual void Unselect()
	{
		if (IsSelected)
		{
			IsSelected = false;
		}
	}

	public virtual void Update(float elapsedTime)
	{
		ResetPrecalculations();
		if (ParentTransformation != null)
		{
			Color = ParentTransformation.Color;
		}
	}

	public abstract void Draw();
}
