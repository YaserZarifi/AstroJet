using System;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class PanelShipParams : ShopMenuPanel
{
	public int CursorSliderIndex;

	public int CursorDragSliderIndex;

	public int ActivatedSliderIndex;

	private HorizontalSlider[] _sliderArray;

	private SpriteAtlas _atlasTopTV;

	public PanelShipParams(ShopMenu shopMenu)
		: base(shopMenu)
	{
		_atlasTopTV = GameCore.GetSpriteAtlas("shop_price_tv", logWhenNotFound: true);
		_sliderArray = new HorizontalSlider[8];
		for (int i = 0; i < _sliderArray.Length; i++)
		{
			_sliderArray[i] = new HorizontalSlider(_shopMenu);
			string text = null;
			switch ((ShipSliderEnum)i)
			{
			case ShipSliderEnum.Body:
				text = "body";
				break;
			case ShipSliderEnum.Nose:
				text = "body/nose";
				break;
			case ShipSliderEnum.Side:
				text = "body/side_front";
				break;
			case ShipSliderEnum.SidePos:
				_sliderArray[i].LabelValueOffset = 0;
				_sliderArray[i].MaxValue = 100;
				break;
			case ShipSliderEnum.Engine:
				text = "body/jet";
				break;
			case ShipSliderEnum.Wings:
				text = "body/side_front/wing";
				break;
			case ShipSliderEnum.Rudder:
				text = "body/rudder";
				break;
			case ShipSliderEnum.Camouflage:
				_sliderArray[i].MaxValue = GameJnG.HeroCamouflageIdList.Count - 1;
				break;
			}
			if (text != null)
			{
				HeroPartChoice value = null;
				if (GameJnG.HeroPartChoices.TryGetValue(text, out value))
				{
					_sliderArray[i].MaxValue = value.ItemCount - 1;
				}
				else
				{
					GameCore.LogErr("Unable to find hero part choice '{0}' for ship param slider", text);
				}
			}
			_sliderArray[i].GroupIndex = i;
			_sliderArray[i].OnActivate += ActivateSlider;
			_sliderArray[i].OnSetValue += DoOnSetSliderValue;
		}
		Init();
	}

	public void SetTexts()
	{
		for (int i = 0; i < _sliderArray.Length; i++)
		{
			switch ((ShipSliderEnum)i)
			{
			case ShipSliderEnum.Body:
				_sliderArray[i].Label = TextBundle.GetText("ship_param_body");
				break;
			case ShipSliderEnum.Nose:
				_sliderArray[i].Label = TextBundle.GetText("ship_param_nose");
				break;
			case ShipSliderEnum.Side:
				_sliderArray[i].Label = TextBundle.GetText("ship_param_side");
				break;
			case ShipSliderEnum.SidePos:
				_sliderArray[i].Label = TextBundle.GetText("ship_param_side_position");
				break;
			case ShipSliderEnum.Engine:
				_sliderArray[i].Label = TextBundle.GetText("ship_param_engine");
				break;
			case ShipSliderEnum.Wings:
				_sliderArray[i].Label = TextBundle.GetText("ship_param_wings");
				break;
			case ShipSliderEnum.Rudder:
				_sliderArray[i].Label = TextBundle.GetText("ship_param_rudder");
				break;
			case ShipSliderEnum.Camouflage:
				_sliderArray[i].Label = TextBundle.GetText("ship_param_camouflage");
				break;
			}
		}
	}

	public void OnSwapHero()
	{
		SetValuesByHeroPartChoices(bCallEvents: false);
	}

	public void SetValuesByHeroPartChoices(bool bCallEvents)
	{
		for (int i = 0; i < _sliderArray.Length; i++)
		{
			string text = null;
			switch ((ShipSliderEnum)i)
			{
			case ShipSliderEnum.Body:
				text = "body";
				break;
			case ShipSliderEnum.Nose:
				text = "body/nose";
				break;
			case ShipSliderEnum.Side:
				text = "body/side_front";
				break;
			case ShipSliderEnum.Wings:
				text = "body/side_front/wing";
				break;
			case ShipSliderEnum.Rudder:
				text = "body/rudder";
				break;
			case ShipSliderEnum.Engine:
				text = "body/jet";
				break;
			case ShipSliderEnum.Camouflage:
				_sliderArray[i].SetValue(_shopMenu.MyHero.GetCamouflageIndex(), bCallEvents);
				break;
			case ShipSliderEnum.SidePos:
			{
				if (_shopMenu.MyHero.MyShip.PartMap.PartOffsetXPercentages.TryGetValue("side_front", out var value))
				{
					int value2 = (int)(Math.Round(value, 2) * 100.0);
					_sliderArray[i].SetValue(value2, bCallEvents);
				}
				else
				{
					_sliderArray[i].SetValue(30, bCallEvents);
					_shopMenu.MyHero.MyShip.PartMap.PartOffsetXPercentages["side_front"] = (float)_sliderArray[i].Value / 100f;
					_shopMenu.MyHero.MyShip.PartMap.PartOffsetXPercentages["side_back"] = (float)_sliderArray[i].Value / 100f;
				}
				break;
			}
			}
			if (text == null)
			{
				continue;
			}
			if (GameJnG.HeroPartChoices.TryGetValue(text, out var value3))
			{
				if (value3 != null)
				{
					_sliderArray[i].SetValue(value3.ItemIndex, bCallEvents);
				}
			}
			else
			{
				GameCore.LogErr("Unable to find hero part choice '{0}' for slider {1} value.", text, i);
			}
		}
	}

	private void DoOnSetSliderValue(HorizontalSlider slider)
	{
		HeroPartChoice heroPartChoice = null;
		HeroPartChoice heroPartChoice2 = null;
		bool flag = false;
		switch ((ShipSliderEnum)slider.GroupIndex)
		{
		case ShipSliderEnum.Body:
			heroPartChoice = GameJnG.GetHeroPartChoiceById("body");
			break;
		case ShipSliderEnum.Nose:
			heroPartChoice = GameJnG.GetHeroPartChoiceById("body/nose");
			break;
		case ShipSliderEnum.Side:
			heroPartChoice = GameJnG.GetHeroPartChoiceById("body/side_front");
			heroPartChoice2 = GameJnG.GetHeroPartChoiceById("body/side_back");
			break;
		case ShipSliderEnum.Wings:
			heroPartChoice = GameJnG.GetHeroPartChoiceById("body/side_front/wing");
			heroPartChoice2 = GameJnG.GetHeroPartChoiceById("body/side_back/wing");
			break;
		case ShipSliderEnum.Rudder:
			heroPartChoice = GameJnG.GetHeroPartChoiceById("body/rudder");
			break;
		case ShipSliderEnum.Engine:
			heroPartChoice = GameJnG.GetHeroPartChoiceById("body/jet");
			break;
		case ShipSliderEnum.Camouflage:
			_shopMenu.MyHero.MyShip.PartMap.SkinId = GameJnG.HeroCamouflageIdList[slider.Value];
			flag = true;
			break;
		case ShipSliderEnum.SidePos:
			_shopMenu.MyHero.MyShip.PartMap.PartOffsetXPercentages["side_front"] = (float)slider.Value / 100f;
			_shopMenu.MyHero.MyShip.PartMap.PartOffsetXPercentages["side_back"] = (float)slider.Value / 100f;
			_shopMenu.MyHero.ActualizeSideOffsets();
			_shopMenu.MyHero.UpdateState(-2);
			break;
		}
		if (heroPartChoice != null)
		{
			heroPartChoice.SetItemIndex(slider.Value);
			heroPartChoice2?.SetItemIndex(slider.Value);
			GameJnG.ApplyHeroPartChoicesToPartMap(_shopMenu.MyHero.MyShip.PartMap);
			flag = true;
		}
		else if (_shopMenu.MyHero.MyShip.PartMap != null)
		{
			_shopMenu.MyHero.MyShip.PartMap.ApplyOffsetXToAllParts(_shopMenu.BigShip.MyObject);
		}
		if (flag)
		{
			_shopMenu.MyHero.CreateSmallShip(null, _shopMenu.MyHero.MyObject.WorldPosition);
			_shopMenu.BigShip.Init(_shopMenu.MyHero);
		}
		if (slider.GroupIndex != 5)
		{
			GameCore.PlaySound("menu_option_set");
		}
	}

	public void ActivateSlider(HorizontalSlider slider)
	{
		if (slider != null)
		{
			_shopMenu.ChangeFocus(this);
		}
		for (int i = 0; i < _sliderArray.Length; i++)
		{
			if (_sliderArray[i] != slider)
			{
				_sliderArray[i].IsActivated = false;
			}
		}
	}

	public override void CancelCursorSelection()
	{
		base.CancelCursorSelection();
		SelectSlider(-1);
		ActivateSlider(null);
	}

	public void SelectSliderByY(float sliderY)
	{
		int index = _sliderArray.Length - 1;
		for (int i = 0; i < _sliderArray.Length; i++)
		{
			if (_sliderArray[i].WorldPosition.Y >= sliderY)
			{
				index = i;
				break;
			}
		}
		SelectSlider(index);
	}

	public void SelectSlider(int index)
	{
		if (index >= 0)
		{
			_shopMenu.ChangeFocus(this);
			_shopMenu.BigShipSlotSet.CancelCursorSelection();
			_shopMenu.CancelCursorSelection();
		}
		for (int i = 0; i < _sliderArray.Length; i++)
		{
			if (i == index)
			{
				_sliderArray[i].Select();
				CursorSliderIndex = i;
			}
			else
			{
				_sliderArray[i].Unselect();
			}
		}
		if (ActivatedSliderIndex >= 0 && index >= 0 && ActivatedSliderIndex != index)
		{
			ActivateSlider(null);
		}
	}

	public override void Init()
	{
		base.Init();
		Position = new Vector2(2320f, 0f);
		_atlasFrame = GameCore.GetSpriteAtlas("shop_panel", logWhenNotFound: true);
		_atlasBackground = GameCore.GetSpriteAtlas("shop_panel_bgr", logWhenNotFound: true);
	}

	public override void Show(float showDelay)
	{
		base.Show(showDelay);
		CursorSliderIndex = -1;
		CursorDragSliderIndex = -1;
		ActivatedSliderIndex = -1;
		TargetPosition = new Vector2(1920f, 0f);
		SetValuesByHeroPartChoices(bCallEvents: false);
	}

	public override void Hide()
	{
		base.Hide();
		TargetPosition = new Vector2(2320f, 0f);
	}

	private void ParseInput()
	{
		if (_shopMenu.FocusSection != this)
		{
			return;
		}
		if (_shopMenu.KeyPressed(ShopMenuKeys.MoveUp, bOnlyOnce: false))
		{
			if (CursorSliderIndex > 0)
			{
				SelectSlider(CursorSliderIndex - 1);
			}
		}
		else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveDown, bOnlyOnce: false) && CursorSliderIndex >= 0 && CursorSliderIndex < _sliderArray.Length - 1)
		{
			SelectSlider(CursorSliderIndex + 1);
		}
		if (ActivatedSliderIndex < 0 && HasFocus && _shopMenu.KeyPressed(ShopMenuKeys.MoveLeft, bOnlyOnce: false))
		{
			if (CursorSliderIndex < 2)
			{
				_shopMenu.PanelTopButtons.SelectButton(4);
			}
			else
			{
				_shopMenu.SelectTestRangeButton();
			}
		}
	}

	public override void Update(float elapsedTime)
	{
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		base.Update(elapsedTime);
		if (!_shopMenu.IsAnyPopupMenuVisible)
		{
			ParseInput();
		}
		CursorSliderIndex = -1;
		ActivatedSliderIndex = -1;
		CursorDragSliderIndex = -1;
		for (int i = 0; i < _sliderArray.Length; i++)
		{
			HorizontalSlider obj = _sliderArray[i];
			obj.Position = new Vector2(Position.X - 162f, Position.Y + 230f + (float)(i * 100));
			obj.IsInputEnabled = !_shopMenu.IsAnyPopupMenuVisible;
			obj.Update(elapsedTime);
			if (obj.IsSelected && CursorSliderIndex != i)
			{
				CursorSliderIndex = i;
				_shopMenu.ChangeFocus(this);
			}
			if (obj.IsDragSelected && CursorDragSliderIndex != i)
			{
				CursorDragSliderIndex = i;
				_shopMenu.ChangeFocus(this);
			}
			if (obj.IsActivated && ActivatedSliderIndex != i)
			{
				ActivatedSliderIndex = i;
				_shopMenu.ChangeFocus(this);
			}
		}
	}

	public override void Draw()
	{
		if (State != ShopMenuState.Hidden)
		{
			_shopMenu.SpriteBatchBegin(bLight: true);
			_atlasBackground.DrawFrame(_shopMenu.SpriteBatchLight, 0, Color.White, this);
			for (int i = 0; i < _sliderArray.Length; i++)
			{
				_sliderArray[i].Draw();
			}
			_spriteRenderer.SetAtlas(_atlasFrame);
			for (int j = 0; j <= 4; j++)
			{
				_spriteRenderer.Frame = j;
				_spriteRenderer.Draw(this, _shopMenu.SpriteBatchLight, BlendModeEnum.Normal);
			}
			Transformation transformation = Transformation.CreateByPosition(Position + new Vector2(-158f, 45f));
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchLight, TextBundle.GetText("game_menu_customize_ship"), transformation.Position.X, transformation.Position.Y - 11f * Options.FontScale, ShopMenu.FONT_SCALE_PRICE_TV, null, TextAlign.Center);
			_shopMenu.SpriteBatchEnd(bLight: true);
		}
	}
}
