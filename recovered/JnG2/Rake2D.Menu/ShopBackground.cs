using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Menu;

public class ShopBackground
{
	private ShopMenu _shopMenu;

	private SpriteAtlas _atlasBackgroundLeft;

	private SpriteAtlas _atlasBackgroundRight;

	private SpriteAtlas _atlasGate;

	private SpriteAtlas _atlasGateTV;

	private float _gateDelay;

	private float _gateOpenY;

	private float _gateTVHighliteFactor;

	private Transformation _gateTransform;

	private float _podTimer;

	private int _lastPodMode;

	private int _podModeCount;

	private HangarPodStateEnum _podState;

	private Layer _layerBack;

	private Layer _layerFront;

	private List<ShopObject> _diodeList;

	private ShopObject _objHangarPod1;

	private ShopObject _objHangarPod2;

	private GameObject _objHangarSpine;

	private ShopObject _objFogBack;

	private ShopObject _objFogFront;

	private ShopObject _objSparks;

	private ShopObject _objSparks2;

	private Vector2 _gateTVOffset;

	public GateStateEnum GateState { get; private set; }

	public bool IsGateTVSelected { get; private set; }

	public ShopBackground(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		_layerBack = _shopMenu.GUILevel.GetLayerById("shop_bgr_back");
		_layerFront = _shopMenu.GUILevel.GetLayerById("shop_bgr_front");
		_atlasBackgroundLeft = GameCore.GetSpriteAtlas("shop_menu_bgr_l", logWhenNotFound: true);
		_atlasBackgroundRight = GameCore.GetSpriteAtlas("shop_menu_bgr_r", logWhenNotFound: true);
		_atlasGate = GameCore.GetSpriteAtlas("shop_gate", logWhenNotFound: true);
		_atlasGateTV = GameCore.GetSpriteAtlas("shop_gate_tv", logWhenNotFound: true);
		_gateTransform = new Transformation();
		_gateTransform.ParentTransformation = _shopMenu;
		_diodeList = new List<ShopObject>();
		_gateTVOffset = new Vector2(1463f, 97f);
		PartData partData = GameCore.GetPartData("shop_hangar_pod_1");
		_objHangarPod1 = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		_objHangarPod1.SetPosition(new Vector2(2120f, Utils.GetRandom(150, 350)));
		partData = GameCore.GetPartData("shop_hangar_pod_2");
		_objHangarPod2 = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		_objHangarPod2.SetPosition(new Vector2(-200f, Utils.GetRandom(150, 350)));
		partData = GameCore.GetPartData("shop_fog_back");
		_objFogBack = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		_objFogBack.SetPosition(new Vector2(730f, 740f));
		partData = GameCore.GetPartData("shop_fog_front");
		_objFogFront = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		_objFogFront.SetPosition(new Vector2(730f, 760f));
		partData = GameCore.GetPartData("shop_sparks");
		_objSparks = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		_objSparks.SetPosition(new Vector2(1062f, 105f));
		partData = GameCore.GetPartData("shop_sparks_2");
		_objSparks2 = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		_objSparks2.SetPosition(new Vector2(480f, 375f));
		partData = GameCore.GetPartData("shop_diode_3");
		ShopObject shopObject = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		shopObject.MyObject.SetPosition(770f, 110f);
		_diodeList.Add(shopObject);
		partData = GameCore.GetPartData("shop_diode_1");
		shopObject = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		shopObject.MyObject.SetPosition(838f, 110f);
		_diodeList.Add(shopObject);
		partData = GameCore.GetPartData("shop_diode_2");
		shopObject = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		shopObject.MyObject.SetPosition(644f, 376f);
		_diodeList.Add(shopObject);
		shopObject = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		shopObject.MyObject.SetPosition(1565f, 475f);
		shopObject.MyObject.SetScale(0.8f);
		shopObject.MyObject.SetRotation(Utils.DegToRad(45f));
		_diodeList.Add(shopObject);
		partData = GameCore.GetPartData("shop_diode_lock_on");
		shopObject = new ShopObject(_shopMenu, _shopMenu.LayerShopBackground, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		shopObject.MyObject.SetPosition(1463f, 94f);
		_diodeList.Add(shopObject);
		partData = GameCore.GetPartData("shop_random_spine");
		_objHangarSpine = _layerFront.CreateObjectByPartData("shop_random_spine", new Vector2(960f, 540f), Vector2.One, 0f, 0, bCreateRigidBody: false);
	}

	public void Show()
	{
		_gateTVHighliteFactor = 0f;
		SetGateClosed();
		OpenGate();
	}

	public void OpenGate()
	{
		if (GateState != GateStateEnum.Opening && GateState != GateStateEnum.Opened)
		{
			_gateDelay = 1f;
			_gateOpenY = 0f;
		}
		GateState = GateStateEnum.Opening;
	}

	public void CloseGate()
	{
		if (GateState != GateStateEnum.Closing && GateState != GateStateEnum.Closed)
		{
			GateState = GateStateEnum.Closing;
		}
	}

	public void ToggleGate()
	{
		if (GateState != GateStateEnum.Closing && GateState != GateStateEnum.Closed)
		{
			GateState = GateStateEnum.Closing;
		}
		else if (GateState != GateStateEnum.Opened)
		{
			GateState = GateStateEnum.Opening;
		}
	}

	public void SetGateClosed()
	{
		GateState = GateStateEnum.Closed;
	}

	private void UpdatePod(float elapsedTime)
	{
		_ = _shopMenu.ShowHidePercentage;
		switch (_podState)
		{
		case HangarPodStateEnum.Hidden:
			if (Utils.ChangeValueToTarget(ref _podTimer, 0f, elapsedTime))
			{
				int num;
				if (_podModeCount == 0 && Utils.GetRandom(3) == 0)
				{
					num = _lastPodMode;
					_podModeCount++;
				}
				else
				{
					num = (_lastPodMode = ((_lastPodMode == 0) ? 1 : 0));
					_podModeCount = 0;
				}
				switch (num)
				{
				case 0:
					_podState = HangarPodStateEnum.FlyLeft1;
					_objHangarPod1.SetPosition(new Vector2(2120f, Utils.GetRandom(100, 380)));
					_objHangarPod1.MyObject.SetVelocity(-300f, 0f);
					_objHangarPod1.MyObject.ApplyPartState(_objHangarPod1.MyObject.CurrentState);
					break;
				case 1:
					_podState = HangarPodStateEnum.FlyRight2;
					_objHangarPod2.SetPosition(new Vector2(-200f, Utils.GetRandom(100, 380)));
					_objHangarPod2.MyObject.SetVelocity(300f, 0f);
					_objHangarPod2.MyObject.ApplyPartState(_objHangarPod2.MyObject.CurrentState);
					break;
				}
			}
			break;
		case HangarPodStateEnum.FlyLeft1:
			if (_objHangarPod1.MyObject.WorldPosition.X <= -100f)
			{
				_podState = HangarPodStateEnum.Hidden;
				SetPodStartDelay();
			}
			_objHangarPod1.Update(elapsedTime);
			break;
		case HangarPodStateEnum.FlyRight2:
			if (_objHangarPod2.MyObject.WorldPosition.X >= 2120f)
			{
				_podState = HangarPodStateEnum.Hidden;
				SetPodStartDelay();
			}
			_objHangarPod2.Update(elapsedTime);
			break;
		}
	}

	public void SetPodStartDelay()
	{
		if (Utils.GetRandom(4) == 0)
		{
			_podTimer = Utils.GetRandom(0.5f, 1.5f);
		}
		else
		{
			_podTimer = Utils.GetRandom(1, 2);
		}
	}

	public void UpdateAlpha()
	{
		_objSparks2.MyObject.Alpha = (byte)((float)(int)_objSparks2.MyObject.CurrentState.Color.A * _shopMenu.ShowHidePercentage);
		_objFogBack.MyObject.Alpha = (byte)((float)(int)_objFogBack.MyObject.CurrentState.Color.A * _shopMenu.ShowHidePercentage);
		_objFogFront.MyObject.Alpha = (byte)((float)(int)_objFogBack.MyObject.CurrentState.Color.A * _shopMenu.ShowHidePercentage);
		_objSparks.MyObject.Alpha = (byte)((float)(int)_objSparks.MyObject.CurrentState.Color.A * _shopMenu.ShowHidePercentage);
		byte alpha = (byte)(255f * _shopMenu.ShowHidePercentage);
		foreach (ShopObject diode in _diodeList)
		{
			diode.MyObject.Alpha = alpha;
		}
		_objHangarPod1.MyObject.Alpha = alpha;
		_objHangarPod2.MyObject.Alpha = alpha;
		foreach (GameObject gameObject in _layerFront.GameObjectList)
		{
			gameObject.Alpha = alpha;
		}
	}

	public void ParseInput()
	{
		if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			if (_shopMenu.Cursor.IsInRect(_shopMenu.Position.X + _gateTVOffset.X - 30f, _shopMenu.Position.Y + _gateTVOffset.Y - 30f, 60, 60))
			{
				SelectGateTV();
			}
			else
			{
				IsGateTVSelected = false;
			}
		}
		if (!IsGateTVSelected)
		{
			return;
		}
		_shopMenu.SetHintText("hint_toggle_gate");
		if (_shopMenu.ShopInput.KeyPressed(ShopMenuKeys.Select))
		{
			ToggleGate();
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown, bOnlyOnce: false))
		{
			_shopMenu.SelectTestRangeButton();
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp, bOnlyOnce: false) || _shopMenu.KeyPressed(ShopMenuKeys.MoveLeft, bOnlyOnce: false))
		{
			_shopMenu.SelectTopButton(4);
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight, bOnlyOnce: false))
		{
			CancelSelection();
			if (_shopMenu.CurrentScreen == ShopScreenEnum.Shop)
			{
				_shopMenu.ChangeFocus(_shopMenu.PanelShop);
			}
			else if (_shopMenu.CurrentScreen == ShopScreenEnum.HeroProfile)
			{
				CancelSelection();
				_shopMenu.PanelShipParams.SelectSlider(0);
			}
		}
	}

	public void CancelSelection()
	{
		IsGateTVSelected = false;
	}

	public void SelectGateTV()
	{
		if (!IsGateTVSelected)
		{
			GameCore.PlaySound("menu_button_select");
			IsGateTVSelected = true;
		}
	}

	public void Update(float elapsedTime)
	{
		if (!_shopMenu.IsAnyPopupMenuVisible)
		{
			ParseInput();
		}
		switch (GateState)
		{
		case GateStateEnum.Opening:
			if (Utils.ChangeValueToTarget(ref _gateDelay, 0f, elapsedTime) && Utils.ChangeValueToTarget(ref _gateOpenY, 200f, 300f * elapsedTime))
			{
				GateState = GateStateEnum.Opened;
			}
			break;
		case GateStateEnum.Closing:
			if (Utils.ChangeValueToTarget(ref _gateOpenY, 0f, 700f * elapsedTime))
			{
				GateState = GateStateEnum.Closed;
			}
			break;
		}
		UpdateAlpha();
		if (IsGateTVSelected)
		{
			Utils.ChangeValueToTarget(ref _gateTVHighliteFactor, 1f, 5f * elapsedTime);
		}
		else
		{
			Utils.ChangeValueToTarget(ref _gateTVHighliteFactor, 0f, 5f * elapsedTime);
		}
		foreach (ShopObject diode in _diodeList)
		{
			diode.Update(elapsedTime);
		}
		UpdatePod(elapsedTime);
		_objFogBack.Update(elapsedTime);
		_objFogFront.Update(elapsedTime);
		_objSparks.Update(elapsedTime);
		_objSparks2.Update(elapsedTime);
	}

	public void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: false);
		int num = (int)(255f * _shopMenu.ShowHidePercentage);
		Color color = new Color(num, num, num, 255);
		if (GateState != GateStateEnum.Opened)
		{
			_gateTransform.Position = new Vector2(1345f, 300f - _gateOpenY);
			_atlasGate.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, _gateTransform, BlendModeEnum.Normal);
			_gateTransform.Position.Y += 2f * _gateOpenY;
			_atlasGate.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, _gateTransform, BlendModeEnum.Normal);
		}
		_shopMenu.DrawSummaryStats();
		_atlasBackgroundLeft.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, _shopMenu, BlendModeEnum.Normal);
		_atlasBackgroundRight.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, _shopMenu, BlendModeEnum.Normal);
		Transformation transform = Transformation.CreateByPosition(_shopMenu.Position + _gateTVOffset);
		int frameNumber = ((GateState == GateStateEnum.Closed || GateState == GateStateEnum.Closing) ? 1 : 0);
		_atlasGateTV.DrawFrame(_shopMenu.SpriteBatchNoLight, frameNumber, color, transform, BlendModeEnum.Normal);
		if (_gateTVHighliteFactor > 0f)
		{
			_atlasGateTV.DrawFrame(_shopMenu.SpriteBatchNoLight, 2, new Color(color, _gateTVHighliteFactor), transform, BlendModeEnum.Normal);
		}
		_shopMenu.SpriteBatchEnd(bLight: false);
		_shopMenu.SpriteBatchBegin(bLight: true);
		foreach (ShopObject diode in _diodeList)
		{
			diode.Draw();
		}
		_objSparks.Draw();
		_objSparks2.Draw();
		_layerBack.Draw();
		switch (_podState)
		{
		case HangarPodStateEnum.FlyLeft1:
			_objHangarPod1.Draw();
			break;
		case HangarPodStateEnum.FlyRight2:
			_objHangarPod2.Draw();
			break;
		}
		_objFogBack.Draw();
		_shopMenu.BigShip.Draw();
		_layerFront.Draw();
		_objFogFront.Draw();
		_shopMenu.SpriteBatchEnd(bLight: true);
	}
}
