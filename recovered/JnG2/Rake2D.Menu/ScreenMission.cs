using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class ScreenMission : ShopMenuSection
{
	public ShopMenuState State;

	public float ShowHidePercentage;

	private const int _mapCenterY = 380;

	public LevelData CurrentLevelData;

	public LevelData MouseSelectedLevelData;

	private ShopMenu _shopMenu;

	private SpriteAtlas _atlasSky;

	private SpriteAtlas _atlasGrid;

	private SpriteAtlas _atlasPlanet;

	private SpriteAtlas _atlasFrame;

	private SpriteAtlas _atlasPlenetInfo;

	private float _skyScrollX;

	private float _planetScrollX;

	private IntRange _planetScrollRange;

	private float _oldPlanetScrollX;

	private float _targetPlanetScrollX;

	private float _dragDeltaX;

	private float _maxDragDeltaX;

	private Vector2 _targetPlanetSelectorPosition;

	private Vector2 _planetSelectorPosition;

	private float _mouseSelectorTimer;

	private bool _isCursorOnMap;

	private Vector2 _dragOffset;

	private ShopObject _gridLightObject;

	private ShopObject _starFieldObject;

	private float _gridLightPosY;

	private float _targetGridLightPosY;

	private float _gridLightTimer;

	private List<Particle> _starFieldPartileList;

	private List<LevelData> _levelDataList;

	public bool IsDraggingPlanets { get; private set; }

	public ScreenMission(ShopMenu shopMenu)
	{
		_shopMenu = shopMenu;
		Init();
	}

	public void Init()
	{
		_atlasSky = GameCore.GetSpriteAtlas("mission_sky", logWhenNotFound: true);
		_atlasGrid = GameCore.GetSpriteAtlas("map_grid", logWhenNotFound: true);
		_atlasPlanet = GameCore.GetSpriteAtlas("map_planet", logWhenNotFound: true);
		_atlasFrame = GameCore.GetSpriteAtlas("shop_mission_frame", logWhenNotFound: true);
		_atlasPlenetInfo = GameCore.GetSpriteAtlas("shop_mission_planet_info", logWhenNotFound: true);
		PartData partData = GameCore.GetPartData("shop_grid_light");
		if (partData != null)
		{
			_gridLightObject = new ShopObject(_shopMenu, _shopMenu.LayerMissionLight, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
		}
		partData = GameCore.GetPartData("shop_star_field");
		if (partData != null)
		{
			_starFieldObject = new ShopObject(_shopMenu, _shopMenu.LayerMissionStarfield, GameObject.CreateByPartDataSimple(partData, bCreateRigidBody: false));
			if (_starFieldObject != null && _starFieldObject.MyObject.Emitter != null)
			{
				_starFieldObject.MyObject.Emitter.IsEmitedByBehavior = true;
			}
		}
	}

	public void ActualizeCampaign()
	{
		_planetScrollRange = default;
		_levelDataList = new List<LevelData>();
		for (int i = 0; i < GameJnG.Campaign.StageDataList.Count; i++)
		{
			StageData stageData = GameJnG.Campaign.StageDataList[i];
			if (stageData.Number > GameJnG.StageNumber)
			{
				break;
			}
			foreach (LevelData levelData in stageData.LevelDataList)
			{
				_levelDataList.Add(levelData);
			}
		}
		foreach (LevelData levelData2 in _levelDataList)
		{
			if (levelData2.IsVisible)
			{
				if ((float)_planetScrollRange.From > levelData2.PlanetMapPosition.X)
				{
					_planetScrollRange.From = (int)levelData2.PlanetMapPosition.X;
				}
				if ((float)_planetScrollRange.To < levelData2.PlanetMapPosition.X)
				{
					_planetScrollRange.To = (int)levelData2.PlanetMapPosition.X;
				}
			}
		}
		if (CurrentLevelData != null)
		{
			CurrentLevelData = GameJnG.Campaign.GetLevelData(CurrentLevelData.Id);
		}
	}

	public void SelectUnfinishedStageLevel(int stageNumber)
	{
		StageData stageData = GameJnG.Campaign.GetStageData(stageNumber);
		if (stageData == null || stageData.LevelDataList.IsNullOrEmpty())
		{
			return;
		}
		foreach (LevelData levelData in stageData.LevelDataList)
		{
			if ((!GameJnG.TryGetLevelStats(levelData, out var levelStats) || levelStats.FinishCountInCampaign == 0) && levelData.IsVisible)
			{
				CurrentLevelData = levelData;
				return;
			}
		}
		CurrentLevelData = stageData.LevelDataList[stageData.LevelDataList.Count - 1];
	}

	public LevelData GetNextLevelData(LevelData currLevelData)
	{
		List<LevelData> list = new List<LevelData>(from ld in _levelDataList
			where ld.IsVisible
			orderby ld.PlanetMapPosition.X
			select ld);
		int num = list.IndexOf(currLevelData);
		if (num < list.Count - 1)
		{
			return list[num + 1];
		}
		return list[0];
	}

	public LevelData GetPreviousLevelData(LevelData currLevelData)
	{
		List<LevelData> list = new List<LevelData>(from ld in _levelDataList
			where ld.IsVisible
			orderby ld.PlanetMapPosition.X
			select ld);
		int num = list.IndexOf(currLevelData);
		if (num > 0)
		{
			return list[num - 1];
		}
		return list[list.Count - 1];
	}

	public void Show()
	{
		GameJnG.SetStage(GameJnG.GetLastUnFinishedStage().Number);
		if (State != ShopMenuState.Visible)
		{
			State = ShopMenuState.Showing;
		}
		_planetScrollX = -1000f;
		if (CurrentLevelData != null)
		{
			_targetPlanetScrollX = CurrentLevelData.PlanetMapPosition.X;
		}
		else
		{
			_targetPlanetScrollX = 0f;
		}
		_shopMenu.PanelInfo.IsStartButtonSelected = false;
		_shopMenu.PanelTopButtons.SelectButton(0);
		_gridLightTimer = Utils.GetRandom(1f, 5f);
		_gridLightPosY = 1180f;
		_targetGridLightPosY = -100f;
		if (CurrentLevelData == null)
		{
			SetCursorLevelData(GetNextLevelData(GameCore.GameLevel.LevelData));
		}
		else
		{
			SetCursorLevelData(CurrentLevelData);
		}
		if (CurrentLevelData != null)
		{
			_planetSelectorPosition = CurrentLevelData.PlanetMapPosition;
		}
		else
		{
			_planetSelectorPosition = new Vector2(-1000f, 0f);
		}
	}

	public void Hide()
	{
		if (State != ShopMenuState.Hidden)
		{
			State = ShopMenuState.Hiding;
		}
		_shopMenu.PanelInfo.SetPreviewLevel(null);
	}

	public void SetTexts()
	{
	}

	public void SetMouseSelectedLevelData(LevelData levelData)
	{
		if (MouseSelectedLevelData != levelData)
		{
			if (levelData != null)
			{
				GameCore.PlaySound("menu_item_select");
			}
			MouseSelectedLevelData = levelData;
		}
		_shopMenu.ChangeFocus(this);
	}

	public void SetCursorLevelData(LevelData levelData)
	{
		if (levelData != null)
		{
			CurrentLevelData = levelData;
			_shopMenu.PanelInfo.SetPreviewLevel(levelData);
			_targetPlanetScrollX = CurrentLevelData.PlanetMapPosition.X;
		}
	}

	protected void ParseInput(float elapsedTime)
	{
		if (State != ShopMenuState.Visible)
		{
			MouseSelectedLevelData = null;
			return;
		}
		if (GameJnG.PopupMenuOptions.State != ShopMenuState.Hidden)
		{
			MouseSelectedLevelData = null;
			return;
		}
		if (_shopMenu.IsAnyPopupMenuVisible)
		{
			MouseSelectedLevelData = null;
			return;
		}
		if (_shopMenu.FocusSection == this)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.MoveLeft))
			{
				SetCursorLevelData(GetPreviousLevelData(CurrentLevelData));
				MouseSelectedLevelData = CurrentLevelData;
				_shopMenu.PanelInfo.IsStartButtonSelected = false;
			}
			else if (_shopMenu.KeyPressed(ShopMenuKeys.MoveRight))
			{
				SetCursorLevelData(GetNextLevelData(CurrentLevelData));
				MouseSelectedLevelData = CurrentLevelData;
				_shopMenu.PanelInfo.IsStartButtonSelected = false;
			}
		}
		if (_shopMenu.PanelTopButtons.SelectedButtonIndex < 0 && _shopMenu.KeyPressed(ShopMenuKeys.MoveUp))
		{
			if (_shopMenu.PanelInfo.IsStartButtonSelected)
			{
				SetMouseSelectedLevelData(CurrentLevelData);
				_shopMenu.PanelInfo.IsStartButtonSelected = false;
			}
			else
			{
				_shopMenu.PanelTopButtons.SelectButton(0);
				SetMouseSelectedLevelData(null);
			}
		}
		else if (_shopMenu.PanelTopButtons.SelectedButtonIndex < 0 && !_shopMenu.PanelInfo.IsStartButtonSelected && _shopMenu.KeyPressed(ShopMenuKeys.MoveDown))
		{
			_shopMenu.PanelTopButtons.SelectButton(-1);
			_shopMenu.PanelInfo.IsStartButtonSelected = true;
			_shopMenu.ChangeFocus(_shopMenu.PanelInfo);
			SetMouseSelectedLevelData(null);
		}
		if (MouseSelectedLevelData != null)
		{
			if (_shopMenu.KeyPressed(ShopMenuKeys.Select))
			{
				SetCursorLevelData(MouseSelectedLevelData);
				if (_shopMenu.CurrentController != GameInput.ControllerTypeEnum.Mouse)
				{
					_shopMenu.ChangeFocus(_shopMenu.PanelInfo);
					if (MouseSelectedLevelData != null)
					{
						SetMouseSelectedLevelData(null);
					}
					_shopMenu.PanelInfo.IsStartButtonSelected = true;
				}
				else if (GameInput.DoubleClick)
				{
					_shopMenu.ChangeFocus(_shopMenu.PanelInfo);
					if (MouseSelectedLevelData != null)
					{
						SetMouseSelectedLevelData(null);
					}
					_shopMenu.PanelInfo.DoOnButtonStartLevelClick(null);
				}
			}
		}
		else if (_isCursorOnMap && MouseSelectedLevelData == null && GameInput.MousePressed(GameInput.MouseButton.Left) && State == ShopMenuState.Visible)
		{
			IsDraggingPlanets = true;
			_dragOffset = new Vector2(_planetScrollX + _shopMenu.Cursor.Position.X, _shopMenu.Cursor.Position.Y);
		}
		if (!GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			if (IsDraggingPlanets)
			{
				if (_planetScrollX > (float)_planetScrollRange.From && _planetScrollX < (float)_planetScrollRange.To)
				{
					_targetPlanetScrollX += Utils.Clamp(_maxDragDeltaX * 15f, -1000f, 1000f);
					_targetPlanetScrollX = Utils.Clamp(_targetPlanetScrollX, _planetScrollRange.From, _planetScrollRange.To);
				}
				_maxDragDeltaX = 0f;
				IsDraggingPlanets = false;
			}
		}
		else if ((_maxDragDeltaX >= 0f && _dragDeltaX > 0f && _dragDeltaX > _maxDragDeltaX) || (_maxDragDeltaX <= 0f && _dragDeltaX < 0f && _dragDeltaX < _maxDragDeltaX) || (_maxDragDeltaX <= 0f && _dragDeltaX > 0f) || (_maxDragDeltaX >= 0f && _dragDeltaX < 0f))
		{
			_maxDragDeltaX = _dragDeltaX;
		}
		else
		{
			Utils.ChangeValueToTarget(ref _maxDragDeltaX, 0f, 200f * elapsedTime);
		}
	}

	private void UpdateCursorType()
	{
		if (!_shopMenu.IsAnyPopupMenuVisible)
		{
			if (MouseSelectedLevelData != null)
			{
				_shopMenu.CursorType = CursorTypeEnum.Select;
			}
			else if (IsDraggingPlanets)
			{
				_shopMenu.CursorType = CursorTypeEnum.DragHorizontalyPressed;
			}
			else if (_isCursorOnMap)
			{
				_shopMenu.CursorType = CursorTypeEnum.DragHorizontalyHover;
			}
		}
	}

	private void ClearStarField()
	{
		if (_starFieldPartileList == null)
		{
			return;
		}
		foreach (Particle starFieldPartile in _starFieldPartileList)
		{
			starFieldPartile.Destroy();
		}
		GameCore.Allocator.DeleteOwningParticleList(_shopMenu.LayerMissionStarfield.ParticleListFront);
		_shopMenu.LayerMissionStarfield.ParticleListFront = GameCore.Allocator.GetParticleList();
		GameCore.Allocator.DeleteAliasingParticleList(_starFieldPartileList);
		_starFieldPartileList = null;
	}

	public void UpdateShowHide(float elapsedTime)
	{
		switch (State)
		{
		case ShopMenuState.Showing:
			if (Utils.ChangeValueToTarget(ref ShowHidePercentage, 1f, 3f * elapsedTime))
			{
				State = ShopMenuState.Visible;
			}
			break;
		case ShopMenuState.Hiding:
			if (Utils.ChangeValueToTarget(ref ShowHidePercentage, 0f, 3f * elapsedTime))
			{
				State = ShopMenuState.Hidden;
				ClearStarField();
			}
			break;
		}
	}

	public override void CancelCursorSelection()
	{
		base.CancelCursorSelection();
	}

	public void Update(float elapsedTime)
	{
		UpdateShowHide(elapsedTime);
		if (State == ShopMenuState.Hidden)
		{
			return;
		}
		ParseInput(elapsedTime);
		if (_shopMenu.PanelTopButtons.SelectedButtonIndex < 0 && Utils.IsPointInFixedRect(_shopMenu.Cursor.Position, new Vector2(960f, 355f), 1880f, 710f))
		{
			_isCursorOnMap = true;
		}
		else
		{
			_isCursorOnMap = false;
		}
		if (IsDraggingPlanets)
		{
			_planetScrollX = _dragOffset.X - _shopMenu.Cursor.Position.X;
			_planetScrollX = Utils.Clamp(_planetScrollX, _planetScrollRange.From - 500, _planetScrollRange.To + 500);
			if (_planetScrollX < (float)_planetScrollRange.From)
			{
				_targetPlanetScrollX = _planetScrollRange.From;
			}
			else if (_planetScrollX > (float)_planetScrollRange.To)
			{
				_targetPlanetScrollX = _planetScrollRange.To;
			}
			else
			{
				_targetPlanetScrollX = _planetScrollX;
			}
		}
		bool flag = false;
		if (!_shopMenu.IsAnyPopupMenuVisible && State != ShopMenuState.Hiding)
		{
			for (int num = _levelDataList.Count - 1; num >= 0; num--)
			{
				LevelData levelData = _levelDataList[num];
				if (levelData.IsVisible)
				{
					Vector2 planetPosition = GetPlanetPosition(levelData);
					if (_shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse && _isCursorOnMap && Utils.GetDistance(_shopMenu.Cursor.Position, planetPosition) <= GetPlanetRadius(levelData))
					{
						SetMouseSelectedLevelData(levelData);
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag && _shopMenu.CurrentController == GameInput.ControllerTypeEnum.Mouse)
		{
			MouseSelectedLevelData = null;
		}
		if (!IsDraggingPlanets)
		{
			Utils.ChangeValueToTarget(ref _planetScrollX, _targetPlanetScrollX, Utils.Clamp(Math.Abs(_planetScrollX - _targetPlanetScrollX), 50f, 2000f) * 5f * elapsedTime);
		}
		_skyScrollX -= 10f * elapsedTime + (_planetScrollX - _oldPlanetScrollX) / 4f;
		if (_skyScrollX <= -2048f)
		{
			_skyScrollX += 2048f;
		}
		else if (_skyScrollX >= 0f)
		{
			_skyScrollX -= 2048f;
		}
		if (CurrentLevelData != null)
		{
			_targetPlanetSelectorPosition = CurrentLevelData.PlanetMapPosition;
			Utils.ChangeValueToTarget(ref _planetSelectorPosition, _targetPlanetSelectorPosition, 2000f * elapsedTime);
		}
		UpdateCursorType();
		_mouseSelectorTimer += elapsedTime;
		_dragDeltaX = _planetScrollX - _oldPlanetScrollX;
		_oldPlanetScrollX = _planetScrollX;
		if (Options.IsLightsOn)
		{
			_gridLightTimer -= elapsedTime;
			if (_gridLightTimer <= 0f)
			{
				if (Utils.ChangeValueToTarget(ref _gridLightPosY, _targetGridLightPosY, 800f * elapsedTime))
				{
					_gridLightPosY = 1180f;
					_gridLightTimer = Utils.GetRandom(2, 6);
				}
				_gridLightObject.MyObject.Position = new Vector2(960f, _gridLightPosY);
				_gridLightObject.Update(elapsedTime);
			}
		}
		UpdateStarField(elapsedTime);
	}

	private void UpdateStarField(float elapsedTime)
	{
		float num = (float)_planetScrollRange.To + 1920f;
		if (_starFieldPartileList == null || _starFieldPartileList.Count == 0)
		{
			_starFieldObject.MyObject.Emitter.Emit(bIgnoreLoopCount: true);
			_starFieldPartileList = GameCore.Allocator.GetParticleList();
			_starFieldPartileList.AddRange(_shopMenu.LayerMissionStarfield.ParticleListFront);
			{
				foreach (Particle starFieldPartile in _starFieldPartileList)
				{
					starFieldPartile.Position = Utils.GetRandomVector2((0f - num) / 2f, 0f, num / 2f, 700f);
					starFieldPartile.Scale = new Vector2(Utils.GetRandom(0.5f, 1f));
					starFieldPartile.StartupScale = starFieldPartile.Scale;
					starFieldPartile.Velocity = new Vector2(-50f * starFieldPartile.Scale.X, 0f);
					starFieldPartile.StartupVelocity = starFieldPartile.Velocity;
				}
				return;
			}
		}
		_starFieldObject.SetPosition(new Vector2((float)(_planetScrollRange.To / 2) - _planetScrollX / 4f, 0f));
		foreach (Particle starFieldPartile2 in _starFieldPartileList)
		{
			if (starFieldPartile2.Position.X < (0f - num) / 2f - 20f)
			{
				starFieldPartile2.Position = new Vector2(num / 2f + 150f, Utils.GetRandom(0, 700));
			}
			starFieldPartile2.SpriteColor = new Color(220, 240, 255, (int)(255f * Utils.Clamp(starFieldPartile2.Scale.X * ShowHidePercentage, 0f, 1f)));
			starFieldPartile2.StartupColor = starFieldPartile2.SpriteColor;
			starFieldPartile2.Color = starFieldPartile2.SpriteColor;
		}
	}

	private Vector2 GetPlanetPosition(LevelData levelData)
	{
		return new Vector2(960f - _planetScrollX, 380f) + levelData.PlanetMapPosition;
	}

	private float GetPlanetRadius(LevelData levelData)
	{
		return _atlasPlanet.GetFrameRectangle(levelData.PlanetNumber).Width / 2;
	}

	public void DrawPlanets()
	{
		_shopMenu.SpriteBatchBegin(bLight: false);
		float alphaFactor = ShowHidePercentage * _shopMenu.ShowHidePercentage;
		Color color = new Color(1f, 1f, 1f, ShowHidePercentage * _shopMenu.ShowHidePercentage);
		Color color2 = new Color(255, 255, 255);
		Color color3 = new Color(255, 255, 20);
		Color color4 = new Color(50, 255, 255);
		Vector2 fONT_SCALE_LABEL_SMALL = ShopMenu.FONT_SCALE_LABEL_SMALL;
		foreach (LevelData levelData in _levelDataList)
		{
			if (!levelData.IsVisible)
			{
				continue;
			}
			Transformation transformation = Transformation.CreateByPosition(GetPlanetPosition(levelData));
			_atlasPlanet.DrawFrame(_shopMenu.SpriteBatchNoLight, levelData.PlanetNumber, color, transformation, BlendModeEnum.Normal);
			transformation.Position.Y += levelData.PlanetInfoOffset;
			if (levelData.DebriefingMode != 0)
			{
				_atlasPlenetInfo.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, transformation, BlendModeEnum.Additive);
			}
			transformation.Position.Y += 10f;
			string arg = levelData.TextId ?? levelData.Id;
			Color value = ((!levelData.IsHidden) ? color2 : color3);
			if (GameJnG.TryGetLevelStats(levelData, out var levelStats) && levelStats.FinishCountInCampaign > 0 && levelData.DebriefingMode != 0)
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText($"level_name_{arg}").ToUpper(), transformation.Position.X + 10f, transformation.Position.Y, fONT_SCALE_LABEL_SMALL, value, TextAlign.Left, 0, alphaFactor, -1f, BlendModeEnum.Additive);
				transformation.Position.Y += 18f;
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("{0}", TextBundle.GetText("shop_planet_completed")), transformation.Position.X + 9f, transformation.Position.Y + 8f - 13.5f * fONT_SCALE_LABEL_SMALL.Y, fONT_SCALE_LABEL_SMALL, new Color(20, 255, 10), TextAlign.Left, 0, alphaFactor, -1f, BlendModeEnum.Additive);
				_atlasPlenetInfo.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, transformation, BlendModeEnum.Additive);
				transformation.Position.Y += 19f;
			}
			else
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, TextBundle.GetText($"level_name_{arg}").ToUpper(), transformation.Position.X + 10f, transformation.Position.Y, fONT_SCALE_LABEL_SMALL, value, TextAlign.Left, 0, alphaFactor, -1f, BlendModeEnum.Additive);
				transformation.Position.Y += 35f;
			}
			if (levelData.DebriefingMode == 0)
			{
				continue;
			}
			if (levelStats != null)
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("{0}: {1}", TextBundle.GetText("shop_planet_finish_count"), levelStats.FinishCountInCampaign), transformation.Position.X + 10f, transformation.Position.Y, fONT_SCALE_LABEL_SMALL, color4, TextAlign.Left, 0, alphaFactor, -1f, BlendModeEnum.Additive);
			}
			else
			{
				_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, string.Format("{0}: \\90", TextBundle.GetText("shop_planet_finish_count")), transformation.Position.X + 10f, transformation.Position.Y, fONT_SCALE_LABEL_SMALL, color4, TextAlign.Left, 0, alphaFactor, -1f, BlendModeEnum.Additive);
			}
			transformation.Position.Y += 18f;
			int num = 0;
			int num2 = 0;
			HashSet<int> hashSet = null;
			if (levelData.AvailableFindDictionary.ContainsKey("secret"))
			{
				hashSet = levelData.AvailableFindDictionary["secret"];
				num = hashSet.Count;
			}
			if (levelStats != null && levelStats.FindDictionary != null && levelStats.FindDictionary.ContainsKey("secret"))
			{
				foreach (int item in levelStats.FindDictionary["secret"])
				{
					if (hashSet != null && hashSet.Contains(item))
					{
						num2++;
					}
				}
			}
			string text = ((num2 >= num) ? string.Format("{0}: {1} / {2}", TextBundle.GetText("shop_planet_secrets"), num2, num) : string.Format("{0}: \\9{1} / {2}", TextBundle.GetText("shop_planet_secrets"), num2, num));
			_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, text, transformation.Position.X + 10f, transformation.Position.Y, fONT_SCALE_LABEL_SMALL, color4, TextAlign.Left, 0, alphaFactor, -1f, BlendModeEnum.Additive);
			int num3 = 0;
			Color value2 = Color.Red;
			HashSet<int> hashSet2 = null;
			if (levelData.AvailableFindDictionary.ContainsKey("hitchhiker"))
			{
				hashSet2 = levelData.AvailableFindDictionary["hitchhiker"];
				num3 = hashSet2.Count;
			}
			if (levelStats != null && levelStats.FindDictionary != null && levelStats.FindDictionary.ContainsKey("hitchhiker"))
			{
				foreach (int item2 in levelStats.FindDictionary["hitchhiker"])
				{
					if (hashSet2 != null && hashSet2.Contains(item2))
					{
						value2 = color4;
						break;
					}
				}
			}
			if (num3 > 0)
			{
				float num4 = _shopMenu.FontNoBorder.MeasureFont(text).X * fONT_SCALE_LABEL_SMALL.X;
				_shopMenu.FontNoBorder.Write(_shopMenu.SpriteBatchNoLight, "  ✓", transformation.Position.X + 10f + num4, transformation.Position.Y - 21f * fONT_SCALE_LABEL_SMALL.Y, fONT_SCALE_LABEL_SMALL * 1.75f, value2, TextAlign.Left, 0, alphaFactor, -1f, BlendModeEnum.Additive);
			}
		}
		_shopMenu.SpriteBatchEnd(bLight: false);
		_shopMenu.SpriteBatchBegin(bLight: false);
		if (CurrentLevelData != null)
		{
			Transformation transformation = Transformation.CreateByPosition(GetPlanetPosition(CurrentLevelData));
			Vector2 pos = new Vector2(960f - _planetScrollX, 380f) + _planetSelectorPosition;
			_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, Transformation.CreateByPosition(pos), BlendModeEnum.Additive);
		}
		if (MouseSelectedLevelData != null)
		{
			Transformation transformation = Transformation.CreateByPosition(GetPlanetPosition(MouseSelectedLevelData));
			transformation.Rotation = _mouseSelectorTimer;
			transformation.Scale = new Vector2(1f + (float)Math.Sin(_mouseSelectorTimer * 5f) * 0.1f);
			_shopMenu.AtlasSymbol.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, transformation, BlendModeEnum.Additive);
		}
		_shopMenu.SpriteBatchEnd(bLight: false);
	}

	public void DrawFrame()
	{
		_shopMenu.SpriteBatchBegin(bLight: true);
		Color color = new Color(1f, 1f, 1f, ShowHidePercentage * _shopMenu.ShowHidePercentage);
		float num = 80f;
		Transformation transformation = Transformation.CreateByPosition((0f - (1f - ShowHidePercentage)) * num - 1f, 0f);
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 0, color, transformation);
		transformation.Position.Y += _atlasFrame.GetFrameRectangle(0).Height;
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 1, color, transformation);
		transformation.Position.Y += _atlasFrame.GetFrameRectangle(1).Height;
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 2, color, transformation);
		num = 69f;
		transformation.Position = new Vector2(1920f - num + (1f - ShowHidePercentage) * num + 1f, 0f);
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 3, color, transformation);
		transformation.Position.Y += _atlasFrame.GetFrameRectangle(3).Height;
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 4, color, transformation);
		transformation.Position.Y += _atlasFrame.GetFrameRectangle(4).Height;
		_atlasFrame.DrawFrame(_shopMenu.SpriteBatchLight, 5, color, transformation);
		_shopMenu.SpriteBatchEnd(bLight: true);
	}

	public void Draw()
	{
		_shopMenu.SpriteBatchBegin(bLight: false);
		int num = (byte)(255f * ShowHidePercentage * _shopMenu.ShowHidePercentage);
		Color color = new Color(num, num, num, num);
		Transformation transformation = Transformation.CreateByPosition(_skyScrollX, 0f);
		if (transformation.Position.X >= -2048f)
		{
			_atlasSky.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, transformation, BlendModeEnum.Normal);
		}
		transformation.Position.X += 1024f;
		if (transformation.Position.X >= -1024f)
		{
			_atlasSky.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, transformation, BlendModeEnum.Normal);
		}
		transformation.Position.X += 1024f;
		if (transformation.Position.X < 2048f)
		{
			_atlasSky.DrawFrame(_shopMenu.SpriteBatchNoLight, 0, color, transformation, BlendModeEnum.Normal);
		}
		transformation.Position.X += 1024f;
		if (transformation.Position.X < 2048f)
		{
			_atlasSky.DrawFrame(_shopMenu.SpriteBatchNoLight, 1, color, transformation, BlendModeEnum.Normal);
		}
		_shopMenu.SpriteBatchEnd(bLight: false);
		GameCore.EffectLight.LayerBeginDraw(_shopMenu.LayerShop);
		GameCore.EffectLightBasic.LayerBeginDraw(_shopMenu.LayerShop);
		_shopMenu.SpriteBatchBegin(bLight: true);
		GameCore.EffectLight.SetLightFactor(0f, 0f, 1f);
		color = new Color(255, 255, 255, (int)(0.5f * (float)num));
		transformation = Transformation.CreateByPosition(480f, 361f);
		transformation.Scale.Y = ShowHidePercentage;
		_atlasGrid.DrawFrame(_shopMenu.SpriteBatchLight, 0, color, transformation, BlendModeEnum.Normal);
		transformation.Position.X += 957f;
		transformation.Scale.X = -1f;
		_atlasGrid.DrawFrame(_shopMenu.SpriteBatchLight, 0, color, transformation, BlendModeEnum.Normal);
		_shopMenu.SpriteBatchEnd(bLight: true);
		GameCore.EffectLight.SetLightFactor(0f, 1f, 0f);
		_shopMenu.LayerMissionStarfield.Draw();
		DrawPlanets();
		_shopMenu.SpriteBatchBegin(bLight: false);
		string text = $"\\3{TextBundle.GetDifficultyLabel(GameJnG.GameMode)}: \\1{GameJnG.Difficulty} - {TextBundle.GetDifficultyName(GameJnG.Difficulty)} ";
		_shopMenu.FontDefault.Write(_shopMenu.SpriteBatchNoLight, text, 960f, 670f, ShopMenu.FONT_SCALE_LABEL, null, TextAlign.Center, 0, (float)num / 255f);
		_shopMenu.SpriteBatchEnd(bLight: false);
	}
}
