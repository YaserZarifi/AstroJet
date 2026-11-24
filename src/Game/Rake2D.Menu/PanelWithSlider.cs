namespace Rake2D.Menu;

public abstract class PanelWithSlider : ShopMenuPanel
{
	protected ShopObject _sliderObject;

	protected GameObject _sliderFlarePart;

	protected GameObject _sliderLightPart;

	protected float _currSliderLightAlpha;

	protected float _currSliderFlareAlpha;

	protected float _currSliderBarLightAlpha;

	protected bool _isSliderUnderCursor;

	protected bool _isSliderBarUnderCursor;

	protected bool _isSliderDragged;

	protected float _currsorSliderOffset;

	public PanelWithSlider(ShopMenu shopMenu)
		: base(shopMenu)
	{
	}

	public override void Init()
	{
		base.Init();
		PartData partData = GameCore.GetPartData("shop_slider");
		if (partData != null)
		{
			_sliderObject = new ShopObject(_shopMenu, _shopMenu.LayerShop, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
			_sliderObject.MyObject.ParentTransformation = this;
			_sliderFlarePart = _sliderObject.MyObject.GetChildByIdDeep("flare");
			_sliderLightPart = _sliderObject.MyObject.GetChildByIdDeep("light");
		}
	}

	public override void Show(float showDelay)
	{
		base.Show(showDelay);
		_currSliderFlareAlpha = 0f;
		_currSliderLightAlpha = 150f;
	}

	protected void UpdateSliderLight(float elapsedTime, bool bForceLight)
	{
		if ((_isSliderUnderCursor || _isSliderDragged) | bForceLight)
		{
			Utils.ChangeValueToTarget(ref _currSliderFlareAlpha, 255f, 1000f * elapsedTime);
			Utils.ChangeValueToTarget(ref _currSliderLightAlpha, 255f, 750f * elapsedTime);
		}
		else
		{
			Utils.ChangeValueToTarget(ref _currSliderFlareAlpha, 0f, 900f * elapsedTime);
			Utils.ChangeValueToTarget(ref _currSliderLightAlpha, 150f, 750f * elapsedTime);
		}
		_sliderFlarePart.Alpha = (byte)_currSliderFlareAlpha;
		_sliderLightPart.Alpha = (byte)_currSliderLightAlpha;
		if (((_isSliderBarUnderCursor && !_isSliderUnderCursor) || _isSliderDragged) | bForceLight)
		{
			Utils.ChangeValueToTarget(ref _currSliderBarLightAlpha, 255f, 750f * elapsedTime);
		}
		else
		{
			Utils.ChangeValueToTarget(ref _currSliderBarLightAlpha, 0f, 750f * elapsedTime);
		}
	}
}
