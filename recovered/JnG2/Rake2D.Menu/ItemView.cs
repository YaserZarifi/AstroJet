using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class ItemView
{
	public ItemViewStateEnum State;

	public GameLevel MyLevel;

	public Layer MyLayer;

	public HeroItem MyItem;

	public GameObject MyObject;

	private ShopMenu _shopMenu;

	private float _alpha;

	private const int _maxAlpha = 180;

	private bool _wasGateOpened;

	public ItemView(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		MyLevel = new GameLevel();
		MyLevel.Init();
		MyLevel.Begin();
		MyLayer = MyLevel.CreateLayer("main");
		MyLayer.LightMapFactorArray[0] = 0f;
		MyLayer.LightMapFactorArray[1] = 1f;
		MyLayer.LightMapFactorArray[2] = 0f;
		MyLayer.LightToMapNumber = 1;
		MyLevel.MainLayer = MyLayer;
		MyLevel.AfterLoad(resetCamera: true, mainLevel: false);
	}

	public void SetState(ItemViewStateEnum state)
	{
		State = state;
		switch (State)
		{
		case ItemViewStateEnum.Hidden:
			MyLevel.Clear();
			MyItem = null;
			MyObject = null;
			break;
		case ItemViewStateEnum.FadeOut:
			if (_wasGateOpened && (_shopMenu.ShopBackground.GateState == GateStateEnum.Closed || _shopMenu.ShopBackground.GateState == GateStateEnum.Closing))
			{
				_shopMenu.ShopBackground.ToggleGate();
			}
			break;
		case ItemViewStateEnum.Visible:
			_alpha = 1f;
			if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
			{
				_shopMenu.PanelInfo.UnselectAllButtons();
			}
			break;
		case ItemViewStateEnum.FadeIn:
			_alpha = 0f;
			if (MyItem != null && MyItem.Data.ViewPartData != null)
			{
				MyObject = MyLayer.CreateObjectByPartData(MyItem.Data.ViewPartData, new Vector2(960f, 540f), Vector2.One, 0f);
			}
			break;
		}
	}

	public void Show(HeroItem item)
	{
		MyItem = item;
		MyObject = null;
		MyLevel.Clear();
		_wasGateOpened = _shopMenu.ShopBackground.GateState == GateStateEnum.Opened || _shopMenu.ShopBackground.GateState == GateStateEnum.Opening;
		if (_wasGateOpened)
		{
			_shopMenu.ShopBackground.ToggleGate();
		}
		SetState(ItemViewStateEnum.FadeIn);
		GameCore.PlaySound("menu_yesno_show");
	}

	public void Hide()
	{
		SetState(ItemViewStateEnum.FadeOut);
		GameCore.PlaySound("menu_yesno_hide");
	}

	public void Update(float elapsedTime)
	{
		float speed = 5f * elapsedTime;
		switch (State)
		{
		case ItemViewStateEnum.FadeOut:
			if (Utils.ChangeValueToTarget(ref _alpha, 0f, speed))
			{
				SetState(ItemViewStateEnum.Hidden);
				return;
			}
			break;
		case ItemViewStateEnum.FadeIn:
			if (Utils.ChangeValueToTarget(ref _alpha, 1f, speed))
			{
				SetState(ItemViewStateEnum.Visible);
				return;
			}
			break;
		}
		if (State == ItemViewStateEnum.Visible && (GameInput.AnySystemKeyPressed() || GameInput.AnyMouseButtonPressed() || GameInput.AnyGamepadButtonPressedAll()))
		{
			Hide();
		}
		if (State == ItemViewStateEnum.FadeIn || State == ItemViewStateEnum.FadeOut || State == ItemViewStateEnum.Visible)
		{
			MyLevel.Camera.SetViewPort(GameCore.MainViewPort);
			MyLevel.Update(elapsedTime);
		}
	}

	public void Draw()
	{
		GameCore.DrawLevelRenderTarget(bUseDistortion: false, Color.White);
		GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.LevelRenderTarget);
		GameCore.GraphicsDeviceStatic.Clear(new Color(0, 0, 0, (int)(180f * _alpha)));
		MyLevel.Draw(showRenderTarget: false);
		if (_alpha < 1f)
		{
			GameCore.DrawLevelRenderTarget(bUseDistortion: false, Color.FromNonPremultiplied(255, 255, 255, (int)(255f * _alpha)));
			GameCore.GraphicsDeviceStatic.SetRenderTarget(GameCore.LevelRenderTarget);
			GameCore.GraphicsDeviceStatic.Clear(Color.TransparentBlack);
		}
	}
}
