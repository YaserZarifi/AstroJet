using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class PopupDialog : ShopMenuSection
{
	public ShopMenuState State;

	protected ShopMenu _shopMenu;

	protected SpriteAtlas _atlasBgr;

	protected float _alpha;

	public PopupDialog(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
	}

	public virtual void Init()
	{
		_atlasBgr = _shopMenu.AtlasPopupDialog;
		_alpha = 0f;
		SetState(ShopMenuState.Hidden);
	}

	public virtual void SetState(ShopMenuState state)
	{
		switch (state)
		{
		case ShopMenuState.Hidden:
			_alpha = 0f;
			break;
		case ShopMenuState.Visible:
			_alpha = 255f;
			break;
		}
		Color = new Color(Color, _alpha);
		State = state;
	}

	public virtual void Show()
	{
		if (State != ShopMenuState.Showing && State != ShopMenuState.Visible)
		{
			State = ShopMenuState.Showing;
		}
	}

	public virtual void Hide()
	{
		if (State == ShopMenuState.Showing || State == ShopMenuState.Visible)
		{
			State = ShopMenuState.Hiding;
			GameCore.PlaySound("menu_yesno_hide");
		}
	}

	public virtual void Update(float elapsedTime)
	{
		GameCore.UseShopLightMaps = true;
		switch (State)
		{
		case ShopMenuState.Showing:
			if (Utils.ChangeValueToTarget(ref _alpha, 255f, elapsedTime * 2000f))
			{
				State = ShopMenuState.Visible;
			}
			break;
		case ShopMenuState.Hiding:
			if (Utils.ChangeValueToTarget(ref _alpha, 0f, elapsedTime * 2000f))
			{
				State = ShopMenuState.Hidden;
			}
			break;
		case ShopMenuState.Visible:
			_alpha = 255f;
			break;
		case ShopMenuState.Hidden:
			_alpha = 0f;
			break;
		}
		Color = new Color(255, 255, 255, (int)_alpha);
	}

	public virtual void Draw()
	{
		GameCore.EffectLight.SetLightFactor(0f, 1f, 0f);
		GameCore.EffectLight.SetLightMaps(bUseShopMaps: true);
		GameCore.EffectLight.SetShadowFactor(0f);
		_atlasBgr.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color, this, BlendModeEnum.Normal);
	}
}
