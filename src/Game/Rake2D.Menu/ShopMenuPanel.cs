using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public abstract class ShopMenuPanel : ShopMenuSection
{
	public ShopMenuState State;

	protected ShopMenu _shopMenu;

	protected Vector2 TargetPosition;

	protected SpriteAtlas _atlasFrame;

	protected SpriteAtlas _atlasBackground;

	protected SpriteRenderer _spriteRenderer;

	protected int _showBounceCount;

	protected float _showDelay;

	public ShopMenuPanel(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
	}

	public virtual void Show(float showDelay)
	{
		_showDelay = showDelay;
		State = ShopMenuState.Showing;
	}

	public virtual void Hide()
	{
		State = ShopMenuState.Hiding;
	}

	public virtual void Init()
	{
		_spriteRenderer = GameCore.Allocator.GetSpriteRenderer(null);
	}

	public virtual void UpdateMove(float elapsedTime)
	{
		if (State == ShopMenuState.Hiding)
		{
			if (Utils.ChangeValueToTarget(ref Position, TargetPosition, 3000f * elapsedTime))
			{
				State = ShopMenuState.Hidden;
			}
		}
		else
		{
			if (State != ShopMenuState.Showing)
			{
				return;
			}
			_showDelay -= elapsedTime;
			if (_showDelay <= 0f && Utils.ChangeValueToTarget(ref Position, TargetPosition, 3000f * elapsedTime))
			{
				if (_showBounceCount < 5)
				{
					_showBounceCount++;
				}
				State = ShopMenuState.Visible;
			}
		}
	}

	public virtual void Update(float elapsedTime)
	{
		ResetPrecalculations();
		UpdateMove(elapsedTime);
	}

	public virtual void Draw()
	{
	}
}
