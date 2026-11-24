using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FarseerPhysics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rake2D.Debug;
using Rake2D.JnG;
using Rake2D.Script;

namespace Rake2D.Editor;

public class Editor
{
	public SpriteBatch SpriteBatch;

	public LineBatch LineBatch;

	public TriangleBatch TriangleBatch;

	public TriangleBatchTextured TriangleBatchTextured;

	public Cursor Cursor;

	public Transformation CurrObjectScreenTrans;

	public Layer CurrentLayer;

	public bool[] IsMouseClickFree = new bool[3] { true, true, true };

	public ToolEdge ToolEdge;

	public ToolPath ToolPath;

	public ToolPolygon ToolPolygon;

	public ToolTimeLine ToolTimeLine;

	public EditorToolEnum CurrentTool;

	public Vector2 StartCameraPosition;

	public DragAction DragAction;

	public DragAction SelectedDragAction;

	public GameObject SelectedTriggerPointObject;

	public PanelComponent SelectedComponent;

	public Panel SelectedPanel;

	public Panel DraggedPanel;

	public GameInput.MouseButton DragButton;

	public Vector2 DragOffset;

	public Vector2 DragCenter;

	public bool SaveLevelOnHide;

	public bool DarkDisabledEditorSets;

	public bool IsShowPolygonEnabled = true;

	public bool IsShowPathEnabled = true;

	public bool IsShowSafeLineEnabled;

	public bool IsUpdateBehaviorsEnabled;

	public bool IsStoreCameraPositionEnabled;

	public int ScissorsMode = 1;

	public bool IsNoPanelModeEnabled;

	public bool IsInitialized;

	public bool DeleteObjectAllowed;

	public PanelComponent FocusComponent;

	public Undo Undo;

	public bool ShowAllSymbols;

	public bool MinimalGizmoMode;

	public bool[] EditorSetEnabledArray;

	public bool[] EditorSetManualyDisabledArray;

	public SpriteRenderer ComponentRenderer;

	private GameCore _gameCore;

	private PanelLayerBar _panelLayerBar;

	private PanelToolBar _panelToolBar;

	private SpriteRenderer _objectTransformGizmoRenderer;

	private SpriteRenderer _objectSymbolRenderer;

	private SpriteRenderer _diffSymbolRenderer;

	private SpriteRenderer _rotationRingRenderer;

	private PanelGameObjectList _panelObjectList;

	private PanelPartDataList _panelPartDataList;

	private PanelGameObject _panelGameObject;

	private PanelGameObjectDebug _panelGameObjectDebug;

	private PanelGameObjectTree _panelGameObjectTree;

	private PanelBeaviorEvent _panelBehaviorEvent;

	private PanelEmitter _panelEmitter;

	private PanelLayer _panelLayer;

	private PanelEmitterStrip _panelEmitterStrip;

	private PanelEmitterTime _panelEmitterTime;

	private PanelAtlasList _panelAtlasList;

	private LevelScrollBar _levelScrollBar;

	private GameObjectPreview _gameObjectPreview;

	private Vector2 _dragTresholdCenter;

	private bool _isDragTresholdReached;

	private ObjectMover _objectMover;

	private ObjectListModeEnum _objectListMode;

	private Vector2 _selectionTopLeft;

	private Vector2 _selectionBottomRight;

	private List<GameObject> _dragLayerObjectList;

	private List<SafePoint> _dragLayerSafePointList;

	private Vector2 _dragLayerOffset;

	private Vector2 _dragLayerStart;

	private List<Panel> _panelList;

	private static Color _colorControlNormal = new Color(150, 150, 150, 240);

	public static Color ColorControlNormalGroup = new Color(0, 200, 255, 200);

	private static Color _colorControlSelected = new Color(255, 255, 0, 240);

	private static Color _colorControlActivated = new Color(0, 255, 0, 240);

	private static Color _colorControlPolygon = new Color(200, 160, 0, 100);

	private static Color _colorControlEdge = new Color(160, 200, 0, 100);

	private static Color _colorControlPath = new Color(255, 20, 20, 200);

	private static Color _colorControlParticle = new Color(100, 200, 255, 200);

	private bool _keyboardDrag;

	private bool _refreshLayerBarRequest;

	private bool _refreshObjecttListRequest;

	public GameObject CurrentObject { get; internal set; }

	public PartData CurrentPartData { get; internal set; }

	public GameObject CreatorObject { get; internal set; }

	public GameObject SelectedObject { get; internal set; }

	public SafePoint SelectedSafePoint { get; internal set; }

	public SelectionGroup CurrentSelectionGroup { get; private set; }

	public static BitmapFont Font { get; internal set; }

	public GameLevel CurrentLevel { get; private set; }

	public Vector2 CursorWorldPos { get; private set; }

	public bool IsDragAvailable
	{
		get
		{
			if (DragAction == DragAction.None && SelectedDragAction == DragAction.None && CurrentPartData == null)
			{
				return ToolPath.DraggedAngleEvent == null;
			}
			return false;
		}
	}

	public bool IsDragUndoable
	{
		get
		{
			if (DragAction == DragAction.MoveX || DragAction == DragAction.MoveXY || DragAction == DragAction.MoveY || DragAction == DragAction.MovePathNode || DragAction == DragAction.MovePathPoint || DragAction == DragAction.MoveSafePoint || DragAction == DragAction.MoveTimeNode || DragAction == DragAction.ScaleEast || DragAction == DragAction.ScaleWest || DragAction == DragAction.ScaleNorth || DragAction == DragAction.ScaleSouth || DragAction == DragAction.ScaleBottomLeft || DragAction == DragAction.ScaleTopLeft || DragAction == DragAction.Rotate)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsKeyboardFree
	{
		get
		{
			if (FocusComponent == null || (FocusComponent.GetType() != typeof(TextField) && FocusComponent.GetType() != typeof(ComboBox) && FocusComponent.GetType() != typeof(EmitterTimeField)))
			{
				return !GameCore.Console.IsActive;
			}
			return false;
		}
	}

	public bool IsGroupSelected
	{
		get
		{
			if (CurrentSelectionGroup != null)
			{
				return CurrentSelectionGroup.SelectionObject == CurrentObject;
			}
			return false;
		}
	}

	public SpriteRenderer ObjectTransformGizmoRenderer
	{
		get
		{
			return _objectTransformGizmoRenderer;
		}
		set
		{
			_objectTransformGizmoRenderer = value;
		}
	}

	public BehaviorEventNode CurrentEventNode => _panelBehaviorEvent.CurrentEventNode;

	public void RequestRefreshLayerBar()
	{
		_refreshLayerBarRequest = true;
	}

	public Editor(GameCore gameCore, BitmapFont font)
	{
		_gameCore = gameCore;
		Font = font;
		_panelList = new List<Panel>();
		Undo = new Undo();
	}

	public void SetLevel(GameLevel level)
	{
		CurrentLevel = level;
		CurrentLayer = CurrentLevel.MainLayer;
		CurrentLevel.PhysWorld.Step(0f);
		CurrentPartData = null;
		CurrentObject = null;
		SetCurrentObject(null);
		_panelLayerBar.Refresh();
		_panelAtlasList.BuildAtlasList();
		RefreshObjectList();
		RefreshPartDataList();
	}

	internal void Init()
	{
		SpriteBatch = new SpriteBatch(GameCore.GraphicsDeviceStatic);
		LineBatch = new LineBatch(GameCore.GraphicsDeviceStatic);
		TriangleBatch = new TriangleBatch(GameCore.GraphicsDeviceStatic);
		TriangleBatchTextured = new TriangleBatchTextured(GameCore.GraphicsDeviceStatic);
		Cursor = new Cursor(this);
		EditorSetEnabledArray = new bool[10];
		EditorSetManualyDisabledArray = new bool[EditorSetEnabledArray.Length];
		for (int i = 0; i < EditorSetEnabledArray.Length; i++)
		{
			EditorSetEnabledArray[i] = true;
		}
		SpriteAtlas spriteAtlas = GameCore.GlobalContent.GetSpriteAtlas("editor_object_symbol");
		_objectSymbolRenderer = new SpriteRenderer(spriteAtlas);
		spriteAtlas = GameCore.GlobalContent.GetSpriteAtlas("editor_diff_symbol");
		_diffSymbolRenderer = new SpriteRenderer(spriteAtlas);
		spriteAtlas = GameCore.GlobalContent.GetSpriteAtlas("editor_object_transform_gizmo");
		_objectTransformGizmoRenderer = new SpriteRenderer(spriteAtlas);
		spriteAtlas = GameCore.GlobalContent.GetSpriteAtlas("editor_rotation_ring");
		_rotationRingRenderer = new SpriteRenderer(spriteAtlas);
		_panelLayerBar = new PanelLayerBar(this, 0, 0, 37, 50);
		_panelLayerBar.FrameColor = (_panelLayerBar.BackgroundColor = new Color(70, 70, 80, 240));
		_panelLayerBar.TitleBarBackgroundColor = new Color(40, 40, 50);
		_panelLayerBar.TitleBarHeight = 12;
		_panelLayerBar.ShadowSize = 0;
		_panelLayerBar.IsDocked = false;
		spriteAtlas = GameCore.GlobalContent.GetSpriteAtlas("editor_component");
		ComponentRenderer = new SpriteRenderer(spriteAtlas);
		_gameObjectPreview = new GameObjectPreview(this);
		_objectMover = new ObjectMover(this);
		_panelToolBar = new PanelToolBar(this, 220, 0, 33, 50);
		_panelToolBar.FrameColor = (_panelToolBar.BackgroundColor = new Color(50, 50, 50));
		_panelToolBar.IsDocked = false;
		_panelToolBar.ShadowSize = 3;
		_panelToolBar.Init();
		_levelScrollBar = new LevelScrollBar(this, _panelToolBar.Width, GameCore.MainViewPort.Bounds.Height - 16, GameCore.MainViewPort.Bounds.Width - _panelToolBar.Width, 16);
		_levelScrollBar.IsDocked = true;
		_levelScrollBar.ShadowSize = 0;
		_panelObjectList = new PanelGameObjectList(this, (int)_panelToolBar.Position.X, (int)(_panelToolBar.Position.Y + (float)_panelToolBar.Height + 4f), 180, 500);
		_panelObjectList.FrameColor = new Color(62, 81, 115, 240);
		_panelObjectList.BackgroundColor = new Color(40, 70, 60, 240);
		_panelObjectList.IsEnabled = true;
		_panelObjectList.IsMinimized = true;
		_panelObjectList.IsSizeable = true;
		_panelObjectList.TitleBarHeight = 22;
		_panelObjectList.TitleBarBackgroundColor = new Color(10, 40, 30);
		_panelObjectList.TitleText = "Level objects";
		_panelObjectList.TitleBarTextColor = new Color(200, 255, 0);
		_panelObjectList.MinWidth = 100;
		_panelObjectList.MinHeight = 50;
		_panelObjectList.SliderColor = new Color(10, 50, 30, 120);
		_panelObjectList.SliderSelectedColor = new Color(30, 70, 50, 120);
		_panelObjectList.SliderDragColor = new Color(60, 100, 80, 200);
		_panelObjectList.SliderDragSelectedColor = new Color(120, 180, 160, 220);
		_panelObjectList.SliderAreaOfsetTop = _panelObjectList.TitleBarHeight + 22;
		_panelObjectList.SliderAreaOfsetBottom = -5;
		_panelPartDataList = new PanelPartDataList(this, _panelLayerBar.Width, 0, _panelObjectList.Width, 500);
		_panelPartDataList.FrameColor = new Color(62, 81, 115, 240);
		_panelPartDataList.BackgroundColor = new Color(40, 60, 90, 240);
		_panelPartDataList.Height = GameCore.GraphicsDeviceStatic.Viewport.Height - _levelScrollBar.Height - 4;
		_panelPartDataList.TitleBarHeight = 12;
		_panelPartDataList.TitleBarTextColor = new Color(100, 120, 125, 180);
		_panelPartDataList.TitleBarBackgroundColor = new Color(20, 40, 50);
		_panelPartDataList.SliderColor = new Color(10, 30, 50, 120);
		_panelPartDataList.SliderSelectedColor = new Color(20, 50, 70, 120);
		_panelPartDataList.SliderDragColor = new Color(60, 80, 100, 200);
		_panelPartDataList.SliderDragSelectedColor = new Color(140, 140, 180, 220);
		_panelPartDataList.SliderAreaOfsetTop = _panelPartDataList.TitleBarHeight + 40;
		_panelPartDataList.SliderAreaOfsetBottom = -5;
		_panelPartDataList.IsSizeable = true;
		_panelPartDataList.MinWidth = _panelPartDataList.Width;
		_panelPartDataList.MinHeight = 50;
		_panelPartDataList.Init();
		_panelGameObject = new PanelGameObject(this, (int)(_panelToolBar.Position.X + (float)_panelToolBar.Width + 4f), 0, 212, 220);
		_panelGameObject.FrameColor = new Color(62, 81, 115, 240);
		_panelGameObject.BackgroundColor = new Color(60, 20, 80, 240);
		_panelGameObject.TitleBarBackgroundColor = new Color(40, 0, 60, 240);
		_panelGameObject.TitleBarTextColor = new Color(240, 255, 0, 255);
		_panelGameObject.TitleBarHeight = 22;
		_panelGameObjectDebug = new PanelGameObjectDebug(this, 0, 0, 500, 300);
		_panelGameObjectDebug.FrameColor = new Color(62, 100, 150, 240);
		_panelGameObjectDebug.BackgroundColor = new Color(0, 30, 50, 180);
		_panelGameObjectDebug.IsEnabled = false;
		_panelGameObjectTree = new PanelGameObjectTree(this, (int)_panelGameObject.Position.X, (int)(_panelGameObject.Position.Y + (float)_panelGameObject.Height + 4f), 500, 300);
		_panelGameObjectTree.FrameColor = new Color(70, 40, 100, 240);
		_panelGameObjectTree.BackgroundColor = new Color(40, 10, 60, 240);
		_panelGameObjectTree.TitleBarBackgroundColor = new Color(60, 30, 80, 240);
		_panelGameObjectTree.TitleBarTextColor = new Color(240, 255, 70, 255);
		_panelGameObjectTree.TitleBarHeight = 22;
		_panelGameObjectTree.IsEnabled = false;
		_panelLayer = new PanelLayer(this, 800, 400, 230, 200);
		_panelLayer.FrameColor = new Color(120, 120, 120, 240);
		_panelLayer.BackgroundColor = new Color(60, 60, 60, 240);
		_panelLayer.TitleBarBackgroundColor = new Color(40, 40, 40, 240);
		_panelLayer.TitleBarTextColor = new Color(240, 255, 200, 255);
		_panelLayer.TitleBarHeight = 22;
		_panelEmitter = new PanelEmitter(this, 500, 600, 318, 775);
		_panelEmitter.FrameColor = new Color(160, 160, 150, 240);
		_panelEmitter.BackgroundColor = new Color(85, 94, 90, 240);
		_panelEmitter.TitleBarBackgroundColor = new Color(50, 60, 55, 255);
		_panelEmitter.TitleBarTextColor = new Color(240, 255, 100, 255);
		_panelEmitter.TitleBarHeight = 22;
		_panelEmitter.IsEnabled = false;
		_panelEmitter.IsSizeable = false;
		_panelEmitterStrip = new PanelEmitterStrip(this, 500, 600, _panelEmitter.Width, 122);
		_panelEmitterStrip.FrameColor = new Color(160, 160, 150, 240);
		_panelEmitterStrip.BackgroundColor = new Color(75, 74, 80, 240);
		_panelEmitterStrip.TitleBarBackgroundColor = new Color(50, 60, 55, 255);
		_panelEmitterStrip.TitleBarTextColor = new Color(240, 255, 100, 255);
		_panelEmitterStrip.TitleBarHeight = 22;
		_panelEmitterStrip.IsEnabled = false;
		_panelEmitterStrip.IsSizeable = false;
		_panelEmitterStrip.TitleText = "Strip parameters";
		_panelEmitterTime = new PanelEmitterTime(this, 300, 700, 975, 230);
		_panelEmitterTime.FrameColor = new Color(60, 60, 50, 240);
		_panelEmitterTime.BackgroundColor = new Color(30, 30, 25, 240);
		_panelEmitterTime.TitleBarBackgroundColor = new Color(50, 50, 40, 255);
		_panelEmitterTime.TitleBarTextColor = new Color(240, 255, 100, 255);
		_panelEmitterTime.TitleBarHeight = 22;
		_panelEmitterTime.IsEnabled = false;
		_panelAtlasList = new PanelAtlasList(this, 800, 500, 340, 340);
		_panelAtlasList.FrameColor = new Color(50, 50, 60, 240);
		_panelAtlasList.BackgroundColor = new Color(25, 25, 30, 240);
		_panelAtlasList.TitleBarBackgroundColor = new Color(40, 40, 50, 255);
		_panelAtlasList.TitleBarTextColor = new Color(240, 255, 100, 255);
		_panelAtlasList.TitleBarHeight = 22;
		_panelAtlasList.TitleText = "Atlas list";
		_panelAtlasList.TitleBarHeight = 22;
		_panelAtlasList.IsEnabled = false;
		_panelAtlasList.IsSizeable = true;
		_panelAtlasList.OnAtlasSelected += OnAtlasSelected;
		_panelBehaviorEvent = new PanelBeaviorEvent(this, (int)_panelGameObject.Position.X, (int)_panelGameObject.Position.Y, 200, 300);
		_panelBehaviorEvent.FrameColor = new Color(90, 150, 100, 240);
		_panelBehaviorEvent.BackgroundColor = new Color(40, 100, 50, 220);
		_panelBehaviorEvent.TitleBarBackgroundColor = new Color(30, 80, 40, 220);
		_panelBehaviorEvent.TitleBarTextColor = new Color(240, 255, 0, 255);
		_panelBehaviorEvent.TitleBarHeight = 22;
		_panelBehaviorEvent.TitleText = "Path events";
		if (_panelGameObject.IsEnabled && _panelGameObject.IsDocked)
		{
			_panelBehaviorEvent.Position.X += _panelGameObject.Width;
		}
		if (_panelBehaviorEvent.Position.Y + (float)_panelBehaviorEvent.Height > (float)(GameCore.MainViewPort.Height - _levelScrollBar.Height))
		{
			_panelBehaviorEvent.Position.Y = GameCore.MainViewPort.Height - _levelScrollBar.Height - _panelBehaviorEvent.Height;
		}
		_panelBehaviorEvent.IsEnabled = true;
		_panelList.Add(_panelObjectList);
		_panelList.Add(_panelPartDataList);
		_panelList.Add(_panelGameObject);
		_panelList.Add(_panelGameObjectDebug);
		_panelList.Add(_panelGameObjectTree);
		_panelList.Add(_panelEmitterTime);
		_panelList.Add(_panelEmitterStrip);
		_panelList.Add(_panelEmitter);
		_panelList.Add(_panelAtlasList);
		_panelList.Add(_panelBehaviorEvent);
		_panelList.Add(_panelLayer);
		ToolPath = new ToolPath(this);
		ToolPolygon = new ToolPolygon(this);
		ToolTimeLine = new ToolTimeLine(this);
		LoadOptionsFromFile();
		IsInitialized = true;
	}

	public void RefreshObjectList()
	{
		if (CurrentLayer == null)
		{
			return;
		}
		List<GameObject> list = null;
		switch (_objectListMode)
		{
		case ObjectListModeEnum.All:
			list = CurrentLayer.GameObjectList;
			_panelObjectList.TitleText = $"All objects: {CurrentLayer.GameObjectList.Count}";
			break;
		case ObjectListModeEnum.Visible:
			list = CurrentLayer.VisibleGameObjectList.OrderBy((GameObject o) => o.Id).ToList();
			_panelObjectList.TitleText = $"Visible objects: {CurrentLayer.VisibleGameObjectList.Count} / {CurrentLayer.GameObjectList.Count}";
			break;
		case ObjectListModeEnum.Activated:
			list = CurrentLayer.GameObjectList.Where((GameObject go) => go.IsActivated).ToList();
			_panelObjectList.TitleText = $"Activated objects: {list.Count} / {CurrentLayer.GameObjectList.Count}";
			break;
		}
		string searchValue = _panelObjectList.SearchValue.Trim().ToLower();
		if (searchValue.StartsWith("*"))
		{
			list = (from obj in list
				where obj.PartData.EditorName.ToLower().Contains(searchValue.Substring(1))
				orderby obj.PartData.EditorName
				select obj).ToList();
		}
		else if (searchValue.Length > 0)
		{
			list = (from obj in list
				where obj.PartData.EditorName.ToLower().StartsWith(searchValue)
				orderby obj.PartData.EditorName
				select obj).ToList();
		}
		list.RemoveAll((GameObject obj) => obj.IsDestroyed);
		_panelObjectList.SetItems(list);
	}

	public void RefreshPartDataList()
	{
		List<PartData> list = GameCore.GlobalContent.PartDatas.Values.Where((PartData obj) => obj.EditorSet >= 0).ToList();
		list.AddRange(CurrentLevel.Content.PartDatas.Values.Where((PartData obj) => obj.EditorSet >= 0).ToList());
		string searchValue = _panelPartDataList.SearchValue.Trim().ToLower();
		if (searchValue.StartsWith("*"))
		{
			_panelPartDataList.SetItems((from obj in list
				where IsEditorSetEnabled(obj.EditorSet) && obj.EditorName.ToLower().Contains(searchValue.Substring(1))
				orderby obj.EditorName
				select obj).ToList());
		}
		else
		{
			_panelPartDataList.SetItems((from obj in list
				where IsEditorSetEnabled(obj.EditorSet) && obj.EditorName.ToLower().StartsWith(searchValue)
				orderby obj.EditorName
				select obj).ToList());
		}
		_panelPartDataList.SelectPartData();
	}

	private void MoveObjectToLayer()
	{
		if (CurrentObject == null || CurrentLayer == CurrentObject.MyLayer)
		{
			return;
		}
		if (CurrentSelectionGroup != null && CurrentObject == CurrentSelectionGroup.SelectionObject)
		{
			Vector2 screenPosition = CurrentSelectionGroup.SelectionObject.MyLevel.Camera.LayerPosToScreen(CurrentSelectionGroup.SelectionObject.WorldPosition, CurrentSelectionGroup.SelectionObject.MyLayer);
			for (int num = CurrentSelectionGroup.ObjectList.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = CurrentSelectionGroup.ObjectList[num];
				gameObject.MyLayer.GameObjectList.Remove(gameObject);
				CurrentLayer.AddGameObject(gameObject);
			}
			CurrentSelectionGroup.SelectionObject.SetLayer(CurrentLayer);
			CurrentSelectionGroup.SelectionObject.SetScreenPosition(screenPosition);
		}
		else
		{
			Vector2 screenPosition2 = CurrentObject.MyLevel.Camera.LayerPosToScreen(CurrentObject.WorldPosition, CurrentObject.MyLayer);
			CurrentObject.MyLayer.GameObjectList.Remove(CurrentObject);
			CurrentLayer.AddGameObject(CurrentObject);
			CurrentObject.SetScreenPosition(screenPosition2);
		}
	}

	public void SetDragAction(DragAction dragAction)
	{
		DragAction = dragAction;
	}

	public void Popup()
	{
		if (CurrentLayer == null)
		{
			CurrentLayer = CurrentLevel.MainLayer;
		}
		if (CurrentLayer == null)
		{
			GameCore.LogErr("No Layer in editor");
			GameCore.IsEditorEnabled = false;
			return;
		}
		DragAction = DragAction.None;
		RefreshObjectList();
		_panelLayerBar.Refresh();
		SetCurrentObject(null);
		CurrentLevel.PhysicsPaused = true;
		CurrentLevel.PhysWorld.Step(0f);
		CurrentLevel.Camera.StopQuake();
		GameCore.SoundSystem.StopAll();
		GameJnG.CheckpointInfo.Clear();
		_gameCore.Pause(paused: false);
		Undo.SaveUndo("Editor start");
		foreach (Panel panel in _panelList)
		{
			panel.CheckOutOfScreen();
		}
		if (IsStoreCameraPositionEnabled)
		{
			CurrentLevel.Camera.Position = StartCameraPosition;
		}
	}

	public void Hide()
	{
		UnselectAll();
		SaveOptionsToFile();
		if (SaveLevelOnHide)
		{
			SaveLevel();
		}
		CurrentLevel.Reload(null);
		_gameObjectPreview.Clear();
		CurrentLevel.PhysicsPaused = false;
		_gameCore.Pause(paused: false);
		StartCameraPosition = CurrentLevel.Camera.Position;
	}

	private DragAction RotationRingSelected(Vector2 center)
	{
		Vector2 pos = CurrentLevel.Camera.CurrentLayerPosToScreen(CurrentObject.WorldPosition) + Vector2.Transform(new Vector2(0f, -116f), Matrix.CreateRotationZ(CurrentObject.WorldRotation));
		if (Cursor.IsInCircle(pos, 18))
		{
			return DragAction.Rotate;
		}
		return DragAction.None;
	}

	private DragAction GetObjectTransformSelected(Vector2 center)
	{
		if (CurrentTool == EditorToolEnum.Path)
		{
			return DragAction.None;
		}
		if (Cursor.IsInCircle(center, 16))
		{
			return DragAction.MoveXY;
		}
		if (Cursor.IsInRect((int)center.X - 16, (int)center.Y - 48, 32, 32))
		{
			return DragAction.MoveY;
		}
		if (Cursor.IsInRect((int)center.X - 47, (int)center.Y - 16, 32, 32))
		{
			return DragAction.MoveX;
		}
		return DragAction.None;
	}

	private DragAction GetScaleActionSelected()
	{
		if (CurrentObject == null)
		{
			return DragAction.None;
		}
		Vector2[] frameVerices = CurrentObject.GetFrameVerices(bWorld: true);
		for (int i = 0; i < frameVerices.Length; i++)
		{
			if (Cursor.IsInCircle(CurrentLevel.Camera.CurrentLayerPosToScreen(frameVerices[i]), 12))
			{
				return (DragAction)(8 + i);
			}
		}
		if (Utils.GetMinimumDistance(frameVerices[0], frameVerices[1], CursorWorldPos) <= 6f)
		{
			return DragAction.ScaleNorth;
		}
		if (Utils.GetMinimumDistance(frameVerices[1], frameVerices[2], CursorWorldPos) <= 6f)
		{
			return DragAction.ScaleEast;
		}
		if (Utils.GetMinimumDistance(frameVerices[2], frameVerices[3], CursorWorldPos) <= 6f)
		{
			return DragAction.ScaleSouth;
		}
		if (Utils.GetMinimumDistance(frameVerices[3], frameVerices[0], CursorWorldPos) <= 6f)
		{
			return DragAction.ScaleWest;
		}
		return DragAction.None;
	}

	private Color GetDragControlColor(DragAction selectedAction, DragAction checkAction)
	{
		if (DragAction == checkAction)
		{
			return _colorControlActivated;
		}
		if (DragAction == DragAction.None && selectedAction == checkAction && !GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			return _colorControlSelected;
		}
		if (CurrentSelectionGroup != null)
		{
			return ColorControlNormalGroup;
		}
		if (CurrentObject != null && CurrentTool == EditorToolEnum.Polygon)
		{
			return _colorControlPolygon;
		}
		if (CurrentObject != null && CurrentTool == EditorToolEnum.Edge)
		{
			return _colorControlEdge;
		}
		if (CurrentObject != null && CurrentTool == EditorToolEnum.Emiter)
		{
			return _colorControlParticle;
		}
		if (CurrentObject != null && CurrentTool == EditorToolEnum.Path)
		{
			return _colorControlPath;
		}
		return _colorControlNormal;
	}

	private void UpdateSelectionGroup()
	{
		if (CurrentSelectionGroup != null && DragAction != DragAction.SelectionBox)
		{
			CurrentSelectionGroup.SelectionObject.ResetPrecalculations();
			if (CurrentObject != null && CurrentObject != CurrentSelectionGroup.SelectionObject)
			{
				UnselectSelectionGroup();
			}
		}
		if (SelectedComponent == null && ((IsDragAvailable && GameInput.MousePressed(GameInput.MouseButton.Left)) || GameInput.MousePressed(GameInput.MouseButton.Right)) && CurrentTool != EditorToolEnum.Path)
		{
			DragOffset = CursorWorldPos;
			DragAction = DragAction.SelectionBox;
			if (GameInput.MousePressed(GameInput.MouseButton.Right))
			{
				DragButton = GameInput.MouseButton.Right;
			}
			else
			{
				DragButton = GameInput.MouseButton.Left;
			}
			if (!GameInput.ControlDown)
			{
				UnselectSelectionGroup();
				SetCurrentObject(null);
			}
			if (CurrentSelectionGroup == null)
			{
				CurrentSelectionGroup = new SelectionGroup(this);
			}
		}
		if (DragAction == DragAction.SelectionBox)
		{
			if (CurrentSelectionGroup == null)
			{
				return;
			}
			CurrentSelectionGroup.TopLeft = DragOffset;
			CurrentSelectionGroup.BottomRight = CursorWorldPos;
			_selectionTopLeft = DragOffset;
			_selectionBottomRight = CursorWorldPos;
			if (!GameInput.MouseDown(GameInput.MouseButton.Left) && !GameInput.MouseDown(GameInput.MouseButton.Right))
			{
				CurrentSelectionGroup.SelectObjects();
				if (CurrentSelectionGroup.SelectionObject.ChildObjectListFront != null && CurrentSelectionGroup.SelectionObject.ChildObjectListFront.Count > 0)
				{
					SetCurrentObject(CurrentSelectionGroup.SelectionObject);
					CurrentSelectionGroup.TopLeft -= CurrentSelectionGroup.SelectionObject.Position;
					CurrentSelectionGroup.BottomRight -= CurrentSelectionGroup.SelectionObject.Position;
				}
				else
				{
					CurrentSelectionGroup = null;
				}
			}
		}
		else
		{
			_selectionTopLeft = (_selectionBottomRight = Vector2.Zero);
		}
	}

	private void DrawSelectionRectangle()
	{
		if (_selectionTopLeft != _selectionBottomRight)
		{
			LineBatch.DrawRect(CurrentLevel.Camera.CurrentLayerPosToScreen(_selectionTopLeft), CurrentLevel.Camera.CurrentLayerPosToScreen(_selectionBottomRight), ColorControlNormalGroup);
		}
		if (CurrentSelectionGroup == null || CurrentObject != CurrentSelectionGroup.SelectionObject)
		{
			return;
		}
		foreach (GameObject item in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
		{
			item.ResetPrecalculations();
			if (!MinimalGizmoMode)
			{
				DebugDraw.ShowGameObjectRect(item, item.FrameRectangle, new Color(200, 50, 200, 200));
			}
		}
	}

	private void UpdateMoveLayer()
	{
		Vector2 mainLayerPosition = Cursor.GetMainLayerPosition();
		if (SelectedDragAction == DragAction.LayerMoveX || SelectedDragAction == DragAction.LayerMoveY)
		{
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				DragAction = SelectedDragAction;
			}
			if (!GameInput.MouseDown(GameInput.MouseButton.Left))
			{
				DragAction = DragAction.None;
				_dragLayerObjectList = null;
				if (_dragLayerSafePointList != null)
				{
					_dragLayerSafePointList = null;
					CurrentLevel.SortSafePointList();
				}
				if (SelectedDragAction == DragAction.LayerMoveX)
				{
					Cursor.Text = $"Drag layers pos X: {(int)mainLayerPosition.X}";
				}
				else
				{
					Cursor.Text = $"Drag layers pos Y: {(int)mainLayerPosition.Y}";
				}
			}
			else if (SelectedDragAction == DragAction.LayerMoveX)
			{
				Cursor.Text = $"Drag layers offset X: {(int)(mainLayerPosition.X - _dragLayerStart.X)}";
			}
			else
			{
				Cursor.Text = $"Drag layers offset Y: {(int)(mainLayerPosition.Y - _dragLayerStart.Y)}";
			}
		}
		if (DragAction != DragAction.LayerMoveX && DragAction != DragAction.LayerMoveY)
		{
			return;
		}
		Vector2 vector = mainLayerPosition - _dragLayerOffset;
		foreach (GameObject dragLayerObject in _dragLayerObjectList)
		{
			Vector2 vector2 = ((DragAction != DragAction.LayerMoveX) ? new Vector2(dragLayerObject.Position.X, dragLayerObject.Position.Y + vector.Y * dragLayerObject.MyLayer.ScrollFactor.Y) : new Vector2(dragLayerObject.Position.X + vector.X * dragLayerObject.MyLayer.ScrollFactor.X, dragLayerObject.Position.Y));
			Vector2 vector3 = vector2 - dragLayerObject.WorldPosition;
			if (vector3 != Vector2.Zero)
			{
				if (dragLayerObject.MyPath != null)
				{
					dragLayerObject.MyPath.Move(vector3);
				}
				else
				{
					dragLayerObject.SetPosition(vector2);
				}
				if (dragLayerObject.TriggerPoint.X != 0f)
				{
					dragLayerObject.TriggerPoint += vector3;
				}
			}
		}
		if (_dragLayerSafePointList != null)
		{
			if (DragAction == DragAction.LayerMoveX)
			{
				foreach (SafePoint dragLayerSafePoint in _dragLayerSafePointList)
				{
					dragLayerSafePoint.Position.X += vector.X;
				}
			}
			else if (DragAction == DragAction.LayerMoveY)
			{
				foreach (SafePoint dragLayerSafePoint2 in _dragLayerSafePointList)
				{
					dragLayerSafePoint2.Position.Y += vector.Y;
				}
			}
		}
		foreach (Layer layer in CurrentLevel.LayerList)
		{
			if (layer.IsEnabled && layer.IsEnabledByEditor && layer.LoopX >= 0f)
			{
				float num = layer.LoopX + CurrentLevel.Camera.Position.X * (1f - layer.ScrollFactor.X);
				if (_dragLayerStart.X <= num)
				{
					layer.LoopX += vector.X * layer.ScrollFactor.X;
				}
				else if (_dragLayerStart.X <= num + layer.LoopOffsetX)
				{
					layer.LoopOffsetX += vector.X * layer.ScrollFactor.X;
				}
			}
		}
		_dragLayerOffset = mainLayerPosition;
	}

	private void DrawDragLayerLines()
	{
		Vector2 vector = CurrentLevel.Camera.ScreenPosToMainLayer(Vector2.Zero);
		Vector2 vector2 = CurrentLevel.Camera.ScreenPosToMainLayer(new Vector2(GameCore.MainViewPort.Width - 1, GameCore.MainViewPort.Height - 1));
		if (SelectedDragAction == DragAction.LayerMoveX)
		{
			if (_dragLayerObjectList != null)
			{
				LineBatch.DrawLine(CurrentLevel.Camera.CurrentLayerPosToScreen(new Vector2(_dragLayerStart.X, vector.Y)), CurrentLevel.Camera.MainLayerPosToScreen(new Vector2(_dragLayerStart.X, vector2.Y)), new Color(255, 150, 50, 150));
			}
			LineBatch.DrawLine(CurrentLevel.Camera.CurrentLayerPosToScreen(new Vector2(CursorWorldPos.X, vector.Y)), CurrentLevel.Camera.MainLayerPosToScreen(new Vector2(CursorWorldPos.X, vector2.Y)), new Color(0, 200, 255, 180));
		}
		else if (SelectedDragAction == DragAction.LayerMoveY)
		{
			if (_dragLayerObjectList != null)
			{
				LineBatch.DrawLine(CurrentLevel.Camera.CurrentLayerPosToScreen(new Vector2(vector.X, _dragLayerStart.Y)), CurrentLevel.Camera.MainLayerPosToScreen(new Vector2(vector2.X, _dragLayerStart.Y)), new Color(255, 150, 50, 150));
			}
			LineBatch.DrawLine(CurrentLevel.Camera.CurrentLayerPosToScreen(new Vector2(vector.X, CursorWorldPos.Y)), CurrentLevel.Camera.MainLayerPosToScreen(new Vector2(vector2.X, CursorWorldPos.Y)), new Color(0, 200, 255, 180));
		}
	}

	private void UpdateSafeLine()
	{
		if (!GameInput.ControlDown && KeyPressed(Keys.OemSemicolon))
		{
			CurrentLevel.AddSafePoint(CurrentLevel.Camera.ScreenPosToMainLayer(Cursor.Position));
			CurrentLevel.SortSafePointList();
		}
		else if (SelectedSafePoint != null && KeyPressed(Keys.Delete))
		{
			CurrentLevel.RemoveSafePoint(SelectedSafePoint);
			SelectedSafePoint = null;
			DeleteObjectAllowed = false;
		}
		if (DragAction != DragAction.MoveSafePoint && SelectedSafePoint != null)
		{
			CurrentLevel.SortSafePointList();
			SelectedSafePoint = null;
		}
		foreach (SafePoint safePoint in CurrentLevel.SafePointList)
		{
			Vector2 pos = CurrentLevel.Camera.MainLayerPosToScreen(safePoint.Position);
			if (Cursor.IsInCircle(pos, 8))
			{
				SelectedSafePoint = safePoint;
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					DragAction = DragAction.MoveSafePoint;
					DragOffset = safePoint.Position - Cursor.GetCurrentLayerPosition();
				}
				SelectedDragAction = DragAction.MoveSafePoint;
				break;
			}
		}
		if (SelectedSafePoint != null && DragAction == DragAction.MoveSafePoint)
		{
			SelectedSafePoint.Position = CurrentLevel.Camera.ScreenPosToMainLayer(Cursor.Position) + DragOffset;
		}
	}

	private void DrawLoopLine()
	{
		if (CurrentLayer.LoopX >= 0f)
		{
			LineBatch.Begin(BlendState.NonPremultiplied, Matrix.Identity);
			Vector2 vector = CurrentLevel.Camera.LayerPosToScreen(new Vector2(CurrentLayer.LoopX, 0f), CurrentLayer);
			LineBatch.DrawLine(new Vector2(vector.X, 0f), new Vector2(vector.X, GameCore.MainViewPort.Height), new Color(0, 255, 0, 250));
			vector = CurrentLevel.Camera.LayerPosToScreen(new Vector2(CurrentLayer.LoopX + CurrentLayer.LoopOffsetX, 0f), CurrentLayer);
			LineBatch.DrawLine(new Vector2(vector.X, 0f), new Vector2(vector.X, GameCore.MainViewPort.Height), new Color(0, 200, 0, 240));
			LineBatch.End();
		}
	}

	private void DrawSafeLine()
	{
		SafePoint safePoint = null;
		LineBatch.Begin(BlendState.Additive, Matrix.Identity);
		foreach (SafePoint safePoint2 in CurrentLevel.SafePointList)
		{
			Vector2 vector = CurrentLevel.Camera.MainLayerPosToScreen(safePoint2.Position);
			Matrix matrix = Matrix.CreateTranslation(vector.X, vector.Y, 0f);
			Color color;
			if (SelectedSafePoint == safePoint2 && DragAction == DragAction.MoveSafePoint)
			{
				color = new Color(255, 150, 255, 250);
				matrix = Matrix.CreateScale(1.6f) * matrix;
			}
			else if (SelectedSafePoint == safePoint2)
			{
				color = new Color(50, 150, 255, 250);
				matrix = Matrix.CreateScale(1.5f) * matrix;
			}
			else
			{
				color = new Color(0, 100, 255, 200);
			}
			if (safePoint != null)
			{
				Vector2 to = CurrentLevel.Camera.MainLayerPosToScreen(safePoint.Position);
				LineBatch.DrawLine(vector, to, color);
			}
			_objectTransformGizmoRenderer.Color = color;
			_objectTransformGizmoRenderer.Frame = 1;
			_objectTransformGizmoRenderer.Draw(matrix, SpriteBatch);
			safePoint = safePoint2;
		}
		LineBatch.End();
		foreach (SafePoint safePoint3 in CurrentLevel.SafePointList)
		{
			CurrentLevel.Camera.MainLayerPosToScreen(safePoint3.Position);
		}
	}

	private Vector2 GetScaleRectangleScale(DragAction dragAction, bool bFixedCenter)
	{
		Vector2 vector = Vector2.Transform(CursorWorldPos - CurrentObject.WorldPosition, Matrix.CreateRotationZ(0f - CurrentObject.WorldRotation));
		Vector2 vector2 = ((!bFixedCenter) ? (new Vector2(CurrentObject.FrameRectangle.Width, CurrentObject.FrameRectangle.Height) * CurrentLevel.Camera.Zoom) : (new Vector2(CurrentObject.FrameRectangle.Width, CurrentObject.FrameRectangle.Height) / 2f * CurrentLevel.Camera.Zoom));
		Vector2 worldScale = CurrentObject.WorldScale;
		if (DragAction == DragAction.ScaleTopRight || DragAction == DragAction.ScaleTopLeft)
		{
			worldScale.Y = (0f - vector.Y) / vector2.Y;
			if (GameInput.ShiftDown)
			{
				worldScale.X = worldScale.Y;
			}
		}
		else if (DragAction == DragAction.ScaleBottomRight || DragAction == DragAction.ScaleBottomLeft)
		{
			worldScale.Y = vector.Y / vector2.Y;
			if (GameInput.ShiftDown)
			{
				worldScale.X = worldScale.Y;
			}
		}
		if (DragAction == DragAction.ScaleTopRight || DragAction == DragAction.ScaleBottomRight)
		{
			if (!GameInput.ShiftDown)
			{
				worldScale.X = vector.X / vector2.X;
			}
		}
		else if ((DragAction == DragAction.ScaleTopLeft || DragAction == DragAction.ScaleBottomLeft) && !GameInput.ShiftDown)
		{
			worldScale.X = (0f - vector.X) / vector2.X;
		}
		return worldScale;
	}

	public void UpdateCursorType()
	{
		if (CurrentObject == null)
		{
			return;
		}
		if ((SelectedDragAction >= DragAction.ScaleTopLeft && SelectedDragAction <= DragAction.ScaleBottomLeft) || (DragAction >= DragAction.ScaleTopLeft && DragAction <= DragAction.ScaleBottomLeft))
		{
			Cursor.SetType(CursorType.Hand);
			Cursor.Text = $"ScaleXY: {Utils.FormatFloat2(CurrentObject.Scale.X)}, {Utils.FormatFloat2(CurrentObject.Scale.Y)}";
		}
		else if ((SelectedDragAction >= DragAction.ScaleNorth && SelectedDragAction <= DragAction.ScaleWest) || (DragAction >= DragAction.ScaleNorth && DragAction <= DragAction.ScaleWest))
		{
			Cursor.SetType(CursorType.ResizeVertical);
			Cursor.Rotation = CurrentObject.Rotation;
			if (SelectedDragAction == DragAction.ScaleEast || SelectedDragAction == DragAction.ScaleWest || DragAction == DragAction.ScaleEast || DragAction == DragAction.ScaleWest)
			{
				Cursor.Rotation += (float)Math.PI / 2f;
				Cursor.Text = $"ScaleX: {Utils.FormatFloat2(CurrentObject.Scale.X)}, {Utils.FormatFloat2(CurrentObject.Scale.Y)}";
			}
			else
			{
				Cursor.Text = $"ScaleY: {Utils.FormatFloat2(CurrentObject.Scale.X)}, {Utils.FormatFloat2(CurrentObject.Scale.Y)}";
			}
		}
	}

	private void FixSelectedObjectPosition(Vector2[] oldFrameVertices, int changedVertexIndex)
	{
		Vector2[] frameVerices = CurrentObject.GetFrameVerices(bWorld: true);
		int num = changedVertexIndex switch
		{
			0 => 2, 
			1 => 3, 
			2 => 0, 
			_ => 1, 
		};
		CurrentObject.SetPosition(CurrentObject.WorldPosition + (oldFrameVertices[num] - frameVerices[num]));
	}

	private void UpdateScaleRectangle(bool bFixedCenter)
	{
		int num = 0;
		Vector2[] array = null;
		array = CurrentObject.GetFrameVerices(bWorld: true);
		if (IsDragAvailable)
		{
			SelectedDragAction = GetScaleActionSelected();
			if (SelectedDragAction >= DragAction.ScaleTopLeft && SelectedDragAction <= DragAction.ScaleBottomLeft)
			{
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					num = (int)(SelectedDragAction - 8);
					if (GameInput.DoubleClick)
					{
						CurrentObject.SetScale(1f);
						FixSelectedObjectPosition(array, num);
					}
					else
					{
						DragAction = SelectedDragAction;
						DragOffset = CursorWorldPos - array[num];
					}
				}
			}
			else if (SelectedDragAction >= DragAction.ScaleNorth && SelectedDragAction <= DragAction.ScaleWest && GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				switch (SelectedDragAction)
				{
				case DragAction.ScaleNorth:
					num = 0;
					if (GameInput.DoubleClick)
					{
						CurrentObject.SetScale(CurrentObject.Scale.X, 1f);
						FixSelectedObjectPosition(array, num);
						return;
					}
					break;
				case DragAction.ScaleEast:
					num = 1;
					if (GameInput.DoubleClick)
					{
						CurrentObject.SetScale(1f, CurrentObject.Scale.Y);
						FixSelectedObjectPosition(array, num);
						return;
					}
					break;
				case DragAction.ScaleSouth:
					num = 2;
					if (GameInput.DoubleClick)
					{
						CurrentObject.SetScale(CurrentObject.Scale.X, 1f);
						FixSelectedObjectPosition(array, num);
						return;
					}
					break;
				case DragAction.ScaleWest:
					num = 3;
					if (GameInput.DoubleClick)
					{
						CurrentObject.SetScale(1f, CurrentObject.Scale.Y);
						FixSelectedObjectPosition(array, num);
						return;
					}
					break;
				}
				DragAction = SelectedDragAction;
				DragOffset = CursorWorldPos - array[num];
			}
		}
		if (DragAction >= DragAction.ScaleNorth && DragAction <= DragAction.ScaleBottomLeft)
		{
			Vector2 vector;
			switch (DragAction)
			{
			case DragAction.ScaleNorth:
			case DragAction.ScaleTopLeft:
				num = 0;
				vector = new Vector2(1f, 1f);
				break;
			case DragAction.ScaleEast:
			case DragAction.ScaleTopRight:
				num = 1;
				vector = new Vector2(-1f, 1f);
				break;
			case DragAction.ScaleSouth:
			case DragAction.ScaleBottomRight:
				num = 2;
				vector = new Vector2(-1f, -1f);
				break;
			default:
				num = 3;
				vector = new Vector2(1f, -1f);
				break;
			}
			Vector2 vector2 = Vector2.Transform(array[num] - CursorWorldPos + DragOffset, Matrix.CreateRotationZ(0f - CurrentObject.WorldRotation)) * vector;
			if (DragAction == DragAction.ScaleNorth || DragAction == DragAction.ScaleSouth)
			{
				vector2.X = 0f;
			}
			else if (DragAction == DragAction.ScaleWest || DragAction == DragAction.ScaleEast)
			{
				vector2.Y = 0f;
			}
			Vector2 vector3 = ((CurrentObject.PolygonRenderer == null) ? (new Vector2(CurrentObject.FrameRectangle.Width, CurrentObject.FrameRectangle.Height) * CurrentObject.Scale) : ((CurrentObject.PolygonRenderer.BoundingBottomRight - CurrentObject.PolygonRenderer.BoundingTopLeft) * CurrentObject.Scale));
			Vector2 scale = CurrentObject.Scale * ((vector3 + vector2) / vector3);
			CurrentObject.SetScale(scale);
			if (!bFixedCenter)
			{
				CurrentObject.ResetPrecalculations();
				FixSelectedObjectPosition(array, num);
			}
		}
	}

	private void DrawScaleRectangle()
	{
		if (CurrentObject == null)
		{
			return;
		}
		Vector2[] frameVerices = CurrentObject.GetFrameVerices(bWorld: true);
		if (frameVerices == null)
		{
			return;
		}
		_objectTransformGizmoRenderer.Frame = 1;
		for (int i = 0; i < frameVerices.Length; i++)
		{
			Vector2 pos = CurrentObject.MyCamera.LayerPosToScreen(frameVerices[i], CurrentObject.MyLayer);
			if (DragAction == (DragAction)(8 + i) || ((i == 0 || i == 1) && DragAction == DragAction.ScaleNorth) || ((i == 1 || i == 2) && DragAction == DragAction.ScaleEast) || ((i == 2 || i == 3) && DragAction == DragAction.ScaleSouth) || ((i == 3 || i == 0) && DragAction == DragAction.ScaleWest))
			{
				_objectTransformGizmoRenderer.Color = new Color(0, 255, 0);
			}
			else if (SelectedDragAction == (DragAction)(8 + i) || ((i == 0 || i == 1) && SelectedDragAction == DragAction.ScaleNorth) || ((i == 1 || i == 2) && SelectedDragAction == DragAction.ScaleEast) || ((i == 2 || i == 3) && SelectedDragAction == DragAction.ScaleSouth) || ((i == 3 || i == 0) && SelectedDragAction == DragAction.ScaleWest))
			{
				_objectTransformGizmoRenderer.Color = new Color(255, 255, 0);
			}
			else
			{
				_objectTransformGizmoRenderer.Color = new Color(255, 255, 255);
			}
			_objectTransformGizmoRenderer.Draw(Transformation.CreateByPosition(pos), SpriteBatch, BlendModeEnum.Additive);
		}
	}

	private void UpdateRotationRing()
	{
		if (SelectedDragAction == DragAction.None)
		{
			SelectedDragAction = RotationRingSelected(CurrObjectScreenTrans.Position);
		}
		if (SelectedDragAction == DragAction.Rotate)
		{
			if (GameInput.DoubleClick)
			{
				CurrentObject.SetRotation(0f);
				CurrentObject.AwakePhysics(resetDynamics: true);
			}
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				DragAction = DragAction.Rotate;
				SetDragRotationOffset();
			}
		}
		if (DragAction == DragAction.Rotate)
		{
			GameObject gameObject = (GameInput.ControlDown ? CurrentObject : CurrentObject.GetMainPart());
			float rotationTo;
			if (gameObject == CurrentObject)
			{
				rotationTo = Utils.GetRotationTo(CurrObjectScreenTrans.Position, Cursor.Position);
			}
			else
			{
				Transformation trans = new Transformation(gameObject.WorldTransformation);
				Transformation.ConvertToScreen(ref trans, gameObject.MyLayer);
				rotationTo = Utils.GetRotationTo(trans.Position, Cursor.Position);
			}
			gameObject.SetRotation(rotationTo + DragOffset.X);
		}
	}

	private void DrawRotationRing()
	{
		_rotationRingRenderer.Color = GetDragControlColor(SelectedDragAction, DragAction.Rotate);
		Matrix matrix = Matrix.CreateRotationZ(CurrObjectScreenTrans.Rotation) * Matrix.CreateTranslation(CurrObjectScreenTrans.Position.X, CurrObjectScreenTrans.Position.Y, 0f);
		_rotationRingRenderer.Draw(matrix, SpriteBatch);
		if (SelectedDragAction == DragAction.Rotate || DragAction == DragAction.Rotate)
		{
			if (CurrentObject.PathFollowMode == PathFollowModeEnum.Follow)
			{
				Cursor.Text = $"Path angle: {Utils.FormatDecimal1(Utils.RadToDeg(CurrentObject.Rotation))}";
				Cursor.TextColor = new Color(255, 50, 50);
			}
			else
			{
				Cursor.Text = $"Rotate: {Utils.FormatDecimal1(Utils.RadToDeg(CurrentObject.Rotation))}";
			}
		}
	}

	private void UpdateObjectMoveGizmo()
	{
		if (SelectedDragAction != DragAction.None)
		{
			return;
		}
		Transformation trans = new Transformation(CurrentObject.WorldTransformation);
		Transformation.ConvertToScreen(ref trans, CurrentObject.MyLayer);
		SelectedDragAction = GetObjectTransformSelected(trans.Position);
		if (SelectedDragAction != DragAction.None)
		{
			if (GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				SetDragMoveOffset();
				DragAction = SelectedDragAction;
				SetDragTreshold();
			}
			if (GameInput.DoubleClick)
			{
				CurrentObject.SetRotation(0f);
				CurrentObject.SetScale(new Vector2(1f, 1f));
				CurrentObject.AwakePhysics(resetDynamics: true);
			}
		}
	}

	private void DrawObjectMoveGizmo()
	{
		Matrix matrix = Matrix.CreateTranslation(CurrObjectScreenTrans.Position.X, CurrObjectScreenTrans.Position.Y, 0f);
		_objectTransformGizmoRenderer.Frame = 2;
		_objectTransformGizmoRenderer.Color = GetDragControlColor(SelectedDragAction, DragAction.MoveXY);
		_objectTransformGizmoRenderer.Draw(matrix, SpriteBatch, 0f, BlendModeEnum.Additive);
		if (CurrentTool != EditorToolEnum.Path)
		{
			matrix *= Matrix.CreateTranslation(0f, -32f, 0f);
			if (SelectedDragAction == DragAction.MoveY || !MinimalGizmoMode)
			{
				_objectTransformGizmoRenderer.Frame = 0;
				_objectTransformGizmoRenderer.Color = GetDragControlColor(SelectedDragAction, DragAction.MoveY);
				_objectTransformGizmoRenderer.Draw(matrix, SpriteBatch, 0f, BlendModeEnum.Additive);
			}
			matrix *= Matrix.CreateTranslation(-33f, 32f, 0f);
			if (SelectedDragAction == DragAction.MoveX || !MinimalGizmoMode)
			{
				_objectTransformGizmoRenderer.Frame = 3;
				_objectTransformGizmoRenderer.Color = GetDragControlColor(SelectedDragAction, DragAction.MoveX);
				_objectTransformGizmoRenderer.Draw(matrix, SpriteBatch, 0f, BlendModeEnum.Additive);
			}
		}
		if (SelectedDragAction == DragAction.MoveXY || DragAction == DragAction.MoveXY)
		{
			Cursor.Text = string.Format("Move XY: {0}, {1}", CurrentObject.Position.X.ToString("0.##", Utils.FloatFI), CurrentObject.Position.Y.ToString("0.##", Utils.FloatFI));
		}
		else if (SelectedDragAction == DragAction.MoveX || DragAction == DragAction.MoveX)
		{
			Cursor.Text = string.Format("Move X: {0}, {1}", CurrentObject.Position.X.ToString("0.##", Utils.FloatFI), CurrentObject.Position.Y.ToString("0.##", Utils.FloatFI));
		}
		else if (SelectedDragAction == DragAction.MoveY || DragAction == DragAction.MoveY)
		{
			Cursor.Text = string.Format("Move Y: {0}, {1}", CurrentObject.Position.X.ToString("0.##", Utils.FloatFI), CurrentObject.Position.Y.ToString("0.##", Utils.FloatFI));
		}
	}

	private void SetDragRotationOffset()
	{
		DragOffset = new Vector2(CurrentObject.Rotation - Utils.GetRotationTo(CurrentObject.WorldTransformation.Position, CursorWorldPos), DragOffset.Y);
	}

	private void SetDragMoveOffset()
	{
		DragOffset = CurrentObject.GetMainPart().Position - Cursor.GetCurrentLayerPosition();
	}

	public bool KeyPressed(Keys key)
	{
		if (!IsKeyboardFree)
		{
			return false;
		}
		return GameInput.SystemKeyPressed(key);
	}

	public bool KeyDown(Keys key)
	{
		if (!IsKeyboardFree)
		{
			return false;
		}
		return GameInput.SystemKeyDown(key);
	}

	private void SaveLevel()
	{
		string text;
		if (CurrentLevel.ContentArchive != null)
		{
			if (CurrentLevel.ContentArchive.Type == ContentArchiveTypeEnum.Directory)
			{
				text = Path.Combine(CurrentLevel.ContentArchive.Name, Path.GetDirectoryName(CurrentLevel.FileName), "loader.jnl");
			}
			else
			{
				ContentArchive firstArchive = GameContent.GetFirstArchive(ContentArchiveTypeEnum.Directory);
				if (firstArchive == null)
				{
					GameCore.LogErr("Unable to save level - No content dir is defined.");
					return;
				}
				text = Path.Combine(firstArchive.Name, Path.GetDirectoryName(CurrentLevel.FileName), "loader.jnl");
			}
		}
		else
		{
			text = Path.Combine(Path.GetDirectoryName(CurrentLevel.FileName), "loader.jnl");
		}
		string destFileName = Path.Combine(Path.GetDirectoryName(text), Path.GetFileNameWithoutExtension(text) + "_back" + Path.GetExtension(text));
		try
		{
			File.Copy(text, destFileName, overwrite: true);
		}
		catch
		{
		}
		CurrentLevel.SaveLoader(text);
		Undo.SaveUndo("Save level");
	}

	public int GetEnabledEditorSetCount()
	{
		int num = 0;
		bool[] editorSetEnabledArray = EditorSetEnabledArray;
		for (int i = 0; i < editorSetEnabledArray.Length; i++)
		{
			if (editorSetEnabledArray[i])
			{
				num++;
			}
		}
		return num;
	}

	private void OnAtlasSelected(SpriteAtlas atlas, int frame)
	{
		if (_panelEmitter.IsEnabled && !_panelEmitter.IsOff)
		{
			_panelEmitter.SetAtlas(atlas);
		}
	}

	private void SetSamePartEditValues(GameObject goSource, GameObject goTarget)
	{
		if (goSource == goTarget)
		{
			return;
		}
		foreach (GameObject part in goSource.GetPartList())
		{
			GameObject childByIdDeep = goTarget.GetChildByIdDeep(part.Id);
			if (childByIdDeep != null && childByIdDeep.IsEnabled != part.IsEnabled)
			{
				childByIdDeep.IsEnabled = part.IsEnabled;
			}
		}
	}

	private void ParseInputTools()
	{
		if (GameInput.ControlDown && KeyPressed(Keys.F2))
		{
			IsShowPolygonEnabled = !IsShowPolygonEnabled;
		}
		else if (GameInput.ControlDown && KeyPressed(Keys.F3))
		{
			IsShowPathEnabled = !IsShowPathEnabled;
		}
	}

	private void ParseInput(float elapsedTime)
	{
		_keyboardDrag = false;
		ToolPath.ParseInput();
		ToolTimeLine.ParseInput();
		_levelScrollBar.ParseInput();
		if (!GameCore.Console.IsActive && IsKeyboardFree && !GameInput.ControlDown && GameInput.SystemKeyPressed(Keys.P))
		{
			_gameCore.Pause(!GameCore.IsPaused);
		}
		if (!KeyDown(Keys.OemPipe))
		{
			int num = 1000;
			if (GameInput.ShiftDown)
			{
				num *= 3;
			}
			if (GameInput.ControlDown)
			{
				num *= 5;
			}
			if (KeyDown(Keys.Left))
			{
				CurrentLevel.Camera.Position.X -= (float)num * elapsedTime;
			}
			else if (KeyDown(Keys.Right))
			{
				CurrentLevel.Camera.Position.X += (float)num * elapsedTime;
			}
			if (KeyDown(Keys.Up))
			{
				CurrentLevel.Camera.Position.Y -= (float)num * elapsedTime;
			}
			else if (KeyDown(Keys.Down))
			{
				CurrentLevel.Camera.Position.Y += (float)num * elapsedTime;
			}
		}
		for (int i = 0; i <= 9; i++)
		{
			if (!KeyPressed((Keys)(48 + i)))
			{
				continue;
			}
			if (GameInput.ControlDown)
			{
				EditorSetEnabledArray[i] = !EditorSetEnabledArray[i];
				EditorSetManualyDisabledArray[i] = !EditorSetEnabledArray[i];
				RefreshPartDataList();
				continue;
			}
			EditorSetEnabledArray[i] = !EditorSetEnabledArray[i];
			if (GetEnabledEditorSetCount() == 0)
			{
				for (int j = 0; j <= 9; j++)
				{
					if (!EditorSetManualyDisabledArray[j])
					{
						EditorSetEnabledArray[j] = true;
					}
				}
			}
			else
			{
				for (int k = 0; k <= 9; k++)
				{
					EditorSetEnabledArray[k] = false;
				}
				EditorSetEnabledArray[i] = true;
			}
			RefreshPartDataList();
		}
		if (KeyPressed(Keys.OemMinus))
		{
			_panelPartDataList.SetSearchFocus(bIgnoreFirstInput: true);
		}
		else if (KeyPressed(Keys.Multiply))
		{
			if (!GameInput.ControlDown)
			{
				_panelPartDataList.SetSearchValue("*");
			}
			else if (CurrentTool == EditorToolEnum.Emiter)
			{
				_panelAtlasList.SetSearchValue("*");
			}
		}
		if (KeyPressed(Keys.M))
		{
			if (GameInput.AltDown)
			{
				if (_objectMover.Speed == 1f)
				{
					_objectMover.Speed = 2f;
				}
				else
				{
					_objectMover.Speed = 1f;
				}
			}
			else if (GameInput.ControlDown)
			{
				if (GameInput.ShiftDown)
				{
					_objectMover.NextMode();
				}
				else if (CurrentObject == null || _objectMover.MyObject == CurrentObject.GetMainPart())
				{
					_objectMover.Stop();
				}
				else
				{
					_objectMover.Start(CurrentObject.GetMainPart());
				}
			}
		}
		if (GameInput.ControlDown && KeyPressed(Keys.Z))
		{
			UnselectAll();
			Undo.DoUndo();
		}
		else if (GameInput.ControlDown && KeyPressed(Keys.Y))
		{
			UnselectAll();
			Undo.DoRedo();
		}
		else if (KeyPressed(Keys.F8))
		{
			if (GameInput.ControlDown)
			{
				ScissorsMode = ((ScissorsMode != 1) ? 1 : 0);
			}
			else
			{
				ScissorsMode = ((ScissorsMode != 2) ? 2 : 0);
			}
		}
		else if (KeyPressed(Keys.F9))
		{
			if (GameInput.ControlDown)
			{
				if (!IsUpdateBehaviorsEnabled)
				{
					Undo.SaveUndo("Behaviors");
				}
				IsUpdateBehaviorsEnabled = !IsUpdateBehaviorsEnabled;
			}
			else if (CurrentObject != null)
			{
				CurrentObject.IsUpdateInEditorEnabled = !CurrentObject.IsUpdateInEditorEnabled;
			}
			else if (SelectedObject != null)
			{
				SelectedObject.IsUpdateInEditorEnabled = !SelectedObject.IsUpdateInEditorEnabled;
			}
		}
		else if (KeyPressed(Keys.F10))
		{
			IsNoPanelModeEnabled = !IsNoPanelModeEnabled;
		}
		else if (GameInput.ControlDown && KeyPressed(Keys.S))
		{
			SaveLevel();
		}
		if (KeyPressed(Keys.L) && !GameInput.ControlDown)
		{
			DataConstant.Reload();
			if (CurrentPartData != null)
			{
				PartData partData = ReloadPartData(CurrentPartData.GetMainPartData().Id);
				if (CurrentLevel.Content.PartDatas.ContainsKey(partData.Id.ToLower()))
				{
					CurrentLevel.Content.PartDatas[partData.Id.ToLower()] = partData;
				}
				else
				{
					GameCore.GlobalContent.PartDatas[CurrentPartData.GetMainPartData().Id.ToLower()] = partData;
				}
				RefreshPartDataList();
				CurrentPartData = null;
				SetCurrentPartData(partData);
			}
			else
			{
				ReloadCurrentObject();
			}
		}
		else if (GameInput.SystemKeyPressed(Keys.L) && GameInput.ControlDown)
		{
			SetFocusComponent(null);
			DataConstant.Reload();
			UnselectAll();
			GameInput.CancelKeyPressed();
			SetFocusComponent(null);
			if (GameInput.ShiftDown)
			{
				ReloadLevelFull();
			}
			else
			{
				CurrentLevel.Reload(null);
			}
		}
		if (KeyPressed(Keys.I))
		{
			if (GameInput.ControlDown)
			{
				_panelGameObjectDebug.ToggleEnabled();
			}
			else if (GameInput.ShiftDown)
			{
				_panelBehaviorEvent.ToggleEnabled();
			}
			else
			{
				_panelGameObject.ToggleEnabled();
			}
		}
		else if (KeyPressed(Keys.O))
		{
			if (GameInput.AltDown)
			{
				if (CurrentObject != null || SelectedObject != null)
				{
					SetCurrentPartData((SelectedObject ?? CurrentObject).PartData);
				}
			}
			else if (GameInput.ControlDown)
			{
				if (GameInput.ShiftDown)
				{
					if (_objectListMode == ObjectListModeEnum.All)
					{
						_objectListMode = ObjectListModeEnum.Visible;
					}
					else if (_objectListMode == ObjectListModeEnum.Visible)
					{
						_objectListMode = ObjectListModeEnum.Activated;
					}
					else
					{
						_objectListMode = ObjectListModeEnum.All;
					}
					RefreshObjectList();
				}
				else
				{
					_panelObjectList.ToggleEnabled();
				}
			}
		}
		else if (CurrentTool == EditorToolEnum.Polygon)
		{
			GameObject gameObject = SelectedObject ?? CurrentObject;
			if (gameObject != null && gameObject.PolygonRenderer != null)
			{
				if (KeyPressed(Keys.Space))
				{
					Vector2 vector2;
					if (GameInput.ControlDown)
					{
						Vector2 vector = Vector2.Transform(gameObject.PolygonRenderer.BoundingTopLeft + (gameObject.PolygonRenderer.BoundingBottomRight - gameObject.PolygonRenderer.BoundingTopLeft) / 2f, gameObject.WorldMatrix);
						vector2 = gameObject.WorldPosition - vector;
					}
					else
					{
						vector2 = gameObject.WorldPosition - CursorWorldPos;
					}
					Matrix matrix = Matrix.CreateRotationZ(0f - gameObject.WorldRotation) * Matrix.CreateScale(1f / gameObject.WorldScale.X, 1f / gameObject.WorldScale.Y, 1f);
					gameObject.PolygonRenderer.MoveAllVertices(Vector2.Transform(vector2, matrix));
					gameObject.PolygonRenderer.RecalculateData();
					gameObject.UpdateVisibilityRect();
					gameObject.SetPosition(gameObject.WorldPosition - vector2);
				}
				else if (KeyPressed(Keys.R))
				{
					if (gameObject.PolygonRenderer.TextureMappingMode == PolygonTextureMappingModeEnum.RelativeToLevel)
					{
						gameObject.PolygonRenderer.TextureMappingMode = PolygonTextureMappingModeEnum.RelativeToPivot;
					}
					else if (gameObject.PolygonRenderer.TextureMappingMode == PolygonTextureMappingModeEnum.RelativeToPivot)
					{
						gameObject.PolygonRenderer.TextureMappingMode = PolygonTextureMappingModeEnum.RelativeToLevel;
					}
				}
				else if (KeyDown(Keys.T))
				{
					Cursor.Text = $"Move texture offset ({gameObject.PolygonRenderer.TextureOffset}) of {gameObject.Id}";
					if (KeyPressed(Keys.T) && GameInput.ControlDown)
					{
						gameObject.PolygonRenderer.TextureOffset = Vector2.Zero;
					}
					SelectedDragAction = DragAction.MoveTexture;
					Vector2 vector3 = new Vector2(gameObject.PolygonRenderer.Data.Texture.Width, gameObject.PolygonRenderer.Data.Texture.Height);
					if (GameInput.MousePressed(GameInput.MouseButton.Left))
					{
						DragOffset = CursorWorldPos / vector3 + gameObject.PolygonRenderer.TextureOffset;
					}
					if (GameInput.MouseDown(GameInput.MouseButton.Left))
					{
						DragAction = DragAction.MoveTexture;
						gameObject.PolygonRenderer.TextureOffset = -(CursorWorldPos / vector3 - DragOffset);
					}
				}
			}
		}
		else if (KeyPressed(Keys.H) && GameInput.ControlDown && GameInput.ShiftDown)
		{
			SetCurrentObject(GameJnG.HeroList[0].MyObject);
		}
		if (GameInput.AltDown && KeyPressed(Keys.Home))
		{
			CurrentLevel.Camera.Position = new Vector2(0f, 0f);
		}
		else if (GameInput.AltDown && KeyPressed(Keys.End))
		{
			CurrentLevel.Camera.Position = new Vector2(CurrentLevel.Size.X - 1920f, 0f);
		}
		if (CurrentSelectionGroup != null && CurrentSelectionGroup.SelectionObject != null && CurrentSelectionGroup.SelectionObject.ChildObjectListFront != null && CurrentSelectionGroup.SelectionObject.ChildObjectListFront.Count > 0 && !GameInput.AltDown && KeyPressed(Keys.Home))
		{
			Undo.SaveUndo("Same pos");
			Vector2 vector4 = new Vector2(float.MaxValue);
			Vector2 vector5 = new Vector2(float.MinValue);
			foreach (GameObject item2 in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
			{
				if (item2.Position.X < vector4.X)
				{
					vector4.X = item2.Position.X;
				}
				if (item2.Position.Y < vector4.Y)
				{
					vector4.Y = item2.Position.Y;
				}
				if (item2.Position.X > vector5.X)
				{
					vector5.X = item2.Position.X;
				}
				if (item2.Position.Y > vector5.Y)
				{
					vector5.Y = item2.Position.Y;
				}
			}
			Vector2 vector6 = (vector4 + vector5) / 2f;
			foreach (GameObject item3 in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
			{
				Vector2 position = item3.Position;
				if (GameInput.ControlDown)
				{
					Vector2 value = new Vector2(vector6.X, position.Y) - item3.Position;
					MoveObject(item3, new Vector2(vector6.X, item3.Position.Y) + CurrentSelectionGroup.SelectionObject.WorldPosition, value);
					item3.SetPosition(vector6.X, position.Y);
				}
				else
				{
					Vector2 value2 = new Vector2(position.X, vector6.Y) - item3.Position;
					MoveObject(item3, new Vector2(item3.Position.X, vector6.Y) + CurrentSelectionGroup.SelectionObject.WorldPosition, value2);
					item3.SetPosition(position.X, vector6.Y);
				}
			}
		}
		if (CurrentTool != EditorToolEnum.Path && CurrentTool != EditorToolEnum.Polygon && CurrentTool != EditorToolEnum.Emiter && KeyDown(Keys.Insert))
		{
			UnselectAll();
			if (GameInput.ControlDown)
			{
				SelectedDragAction = DragAction.LayerMoveY;
			}
			else
			{
				SelectedDragAction = DragAction.LayerMoveX;
			}
			if (_dragLayerObjectList == null && GameInput.MouseDown(GameInput.MouseButton.Left))
			{
				_dragLayerObjectList = new List<GameObject>();
				Vector2 mainLayerPosition = Cursor.GetMainLayerPosition();
				foreach (Layer layer in CurrentLevel.LayerList)
				{
					if (!layer.IsEnabledByEditor || !layer.IsEnabled)
					{
						continue;
					}
					foreach (GameObject gameObject3 in layer.GameObjectList)
					{
						if ((SelectedDragAction == DragAction.LayerMoveX && gameObject3.GetMainLayerPosition().X >= mainLayerPosition.X) || (SelectedDragAction == DragAction.LayerMoveY && gameObject3.GetMainLayerPosition().Y >= mainLayerPosition.Y))
						{
							_dragLayerObjectList.Add(gameObject3);
						}
					}
				}
				if (CurrentLevel.MainLayer.IsEnabledByEditor && CurrentLevel.MainLayer.IsEnabled)
				{
					_dragLayerSafePointList = new List<SafePoint>();
					foreach (SafePoint safePoint in CurrentLevel.SafePointList)
					{
						if ((SelectedDragAction == DragAction.LayerMoveX && safePoint.Position.X >= mainLayerPosition.X) || (SelectedDragAction == DragAction.LayerMoveY && safePoint.Position.Y >= mainLayerPosition.Y))
						{
							_dragLayerSafePointList.Add(safePoint);
						}
					}
				}
				else
				{
					_dragLayerSafePointList = null;
				}
				_dragLayerOffset = Cursor.GetMainLayerPosition();
				_dragLayerStart = _dragLayerOffset;
			}
			_keyboardDrag = true;
		}
		if (SelectedObject != null && KeyPressed(Keys.Space) && GameInput.ControlDown && CurrentObject != null && SelectedObject != CurrentObject)
		{
			SelectedObject.SetScale(CurrentObject.Scale);
			SelectedObject.SetRotation(CurrentObject.Rotation);
		}
		if (KeyPressed(Keys.P) && GameInput.ControlDown)
		{
			GameObject gameObject2 = SelectedObject ?? CurrentObject;
			if (gameObject2 != null)
			{
				if (gameObject2.RigidBody != null)
				{
					gameObject2.RigidBody.Destroy();
					gameObject2.RigidBody = null;
				}
				if (gameObject2.CreatePhysBody)
				{
					gameObject2.CreatePhysBody = false;
				}
				else
				{
					gameObject2.CreatePhysBody = true;
					if (gameObject2.CurrentState != null && (gameObject2.CurrentState.PhysEnabled || !gameObject2.CurrentState.UsePhysEnabled))
					{
						gameObject2.CreateRigidBodyByData();
						gameObject2.CreateRigidBodyByPoygonRenderer();
					}
				}
				if (IsGroupSelected)
				{
					foreach (GameObject item4 in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
					{
						if (item4.RigidBody != null)
						{
							item4.RigidBody.Destroy();
							item4.RigidBody = null;
						}
						item4.CreatePhysBody = gameObject2.CreatePhysBody;
						if (item4.CreatePhysBody && (item4.CurrentState.PhysEnabled || !item4.CurrentState.UsePhysEnabled))
						{
							item4.CreateRigidBodyByData();
							item4.CreateRigidBodyByPoygonRenderer();
						}
					}
				}
			}
		}
		if (KeyDown(Keys.OemPipe))
		{
			SelectedDragAction = DragAction.LoopX;
			float num2 = CurrentLayer.LoopX;
			float num3 = CurrentLayer.LoopOffsetX;
			if (GameInput.ControlDown)
			{
				Cursor.Text = $"Place loop offset - X: {CurrentLayer.LoopOffsetX}";
				if (KeyPressed(Keys.Left))
				{
					num3--;
				}
				else if (KeyPressed(Keys.Right))
				{
					num3++;
				}
				else if (GameInput.MouseDown(GameInput.MouseButton.Left) && num2 < 2.1474836E+09f)
				{
					num3 = CursorWorldPos.X - CurrentLayer.LoopX;
				}
			}
			else
			{
				Cursor.Text = $"Place loop line - X: {CurrentLayer.LoopX}";
				if (KeyPressed(Keys.Left))
				{
					num2--;
				}
				else if (KeyPressed(Keys.Right))
				{
					num2++;
				}
				else if (GameInput.MouseDown(GameInput.MouseButton.Left))
				{
					num2 = (int)CursorWorldPos.X;
				}
			}
			if (num3 < 1920f)
			{
				num3 = 1920f;
			}
			CurrentLayer.LoopX = num2;
			CurrentLayer.LoopOffsetX = num3;
		}
		if (CurrentObject != null)
		{
			if (KeyPressed(Keys.J) && GameInput.ControlDown)
			{
				GameObject gameObjectAtPoint = GameLevel.GetGameObjectAtPoint(CurrentLevel.CurrentLayer.VisibleGameObjectList, CursorWorldPos, bCheckIsAvailableFromEditor: true, bUsePivotOnly: false, bOnlyMainParts: true, CurrentObject);
				if (gameObjectAtPoint != null && gameObjectAtPoint.RigidBody != null && CurrentObject.RigidBody != null)
				{
					Vector2 myLocalAnchor = ConvertUnits.ToSimUnits(Cursor.GetCurrentLayerPosition() - CurrentObject.WorldPosition);
					Vector2 otherLocalAnchor = ConvertUnits.ToSimUnits(Cursor.GetCurrentLayerPosition() - gameObjectAtPoint.WorldPosition);
					CurrentObject.RigidBody.AddJoint(JointType.Revolute, gameObjectAtPoint.RigidBody, myLocalAnchor, otherLocalAnchor);
				}
			}
			else if (KeyPressed(Keys.A) && CurrentObject.RigidBody != null)
			{
				List<Joint> jointList = CurrentObject.RigidBody.GetJointList();
				if (jointList != null && jointList.FirstOrDefault() is RevoluteJoint revoluteJoint)
				{
					Vector2 vector7 = ConvertUnits.ToSimUnits(Cursor.GetCurrentLayerPosition());
					if (revoluteJoint.BodyA == CurrentObject.RigidBody.PhysBody)
					{
						revoluteJoint.WorldAnchorA = vector7;
					}
					else
					{
						revoluteJoint.WorldAnchorB = vector7;
					}
				}
			}
			int num4 = 0;
			if (!GameInput.ShiftDown)
			{
				if (KeyPressed(Keys.OemCloseBrackets))
				{
					num4 = ((!GameInput.ControlDown) ? 1 : 10);
				}
				else if (KeyPressed(Keys.OemOpenBrackets))
				{
					num4 = ((!GameInput.ControlDown) ? (-1) : (num4 - 10));
				}
				if (num4 != 0)
				{
					CurrentObject.ZOrder += num4;
					if (CurrentObject.ZOrder >= 1000)
					{
						CurrentObject.ZOrder = 1000;
					}
					else if (CurrentObject.ZOrder < -1000)
					{
						CurrentObject.ZOrder = -1000;
					}
					if (IsGroupSelected)
					{
						foreach (GameObject item5 in CurrentObject.ChildObjectListFront)
						{
							item5.ZOrder += num4;
							if (item5.ZOrder >= 1000)
							{
								item5.ZOrder = 1000;
							}
							else if (item5.ZOrder < -1000)
							{
								item5.ZOrder = -1000;
							}
						}
					}
				}
			}
			else if (KeyPressed(Keys.OemCloseBrackets))
			{
				if (CurrentSelectionGroup != null && CurrentSelectionGroup.SelectionObject == CurrentObject)
				{
					for (int num5 = CurrentSelectionGroup.ObjectList.Count - 1; num5 >= 0; num5--)
					{
						GameObject item = CurrentSelectionGroup.ObjectList[num5];
						int num6 = CurrentObject.MyLayer.GameObjectList.IndexOf(item);
						if (num6 >= 0)
						{
							CurrentObject.MyLayer.GameObjectList.RemoveAt(num6);
							CurrentObject.MyLayer.GameObjectList.Add(item);
						}
					}
				}
				else
				{
					int num7 = CurrentObject.MyLayer.GameObjectList.IndexOf(CurrentObject);
					if (num7 >= 0)
					{
						CurrentObject.MyLayer.GameObjectList.RemoveAt(num7);
						CurrentObject.MyLayer.GameObjectList.Add(CurrentObject);
					}
				}
			}
			else if (KeyPressed(Keys.OemOpenBrackets))
			{
				if (CurrentSelectionGroup != null && CurrentSelectionGroup.SelectionObject == CurrentObject)
				{
					foreach (GameObject @object in CurrentSelectionGroup.ObjectList)
					{
						int num8 = CurrentObject.MyLayer.GameObjectList.IndexOf(@object);
						if (num8 >= 0)
						{
							CurrentObject.MyLayer.GameObjectList.RemoveAt(num8);
							CurrentObject.MyLayer.GameObjectList.Insert(0, @object);
						}
					}
				}
				else
				{
					int num9 = CurrentObject.MyLayer.GameObjectList.IndexOf(CurrentObject);
					if (num9 >= 0)
					{
						CurrentObject.MyLayer.GameObjectList.RemoveAt(num9);
						CurrentObject.MyLayer.GameObjectList.Insert(0, CurrentObject);
					}
				}
			}
			if (KeyPressed(Keys.F))
			{
				if (!GameInput.ControlDown)
				{
					CurrentObject.SwapFlip(horizontal: true, vertical: false);
				}
				else
				{
					CurrentObject.SwapFlip(horizontal: false, vertical: true);
				}
			}
			if ((CurrentTool == EditorToolEnum.Object || (CurrentTool == EditorToolEnum.Path && SelectedDragAction == DragAction.None && DragAction == DragAction.None && CurrentEventNode == null)) && DeleteObjectAllowed && FocusComponent == null && !GameInput.ControlDown && SelectedTriggerPointObject == null && KeyPressed(Keys.Delete))
			{
				CurrentObject.Destroy();
				UnselectAll();
				UnlinkDestroyedObjects();
				_refreshObjecttListRequest = true;
				Undo.SaveUndo("del");
			}
			else if (GameInput.ControlDown && KeyPressed(Keys.C))
			{
				if (GameInput.ShiftDown)
				{
					CloneObject(CurrentObject, new Vector2(Cursor.GetCurrentLayerPosition().X, CurrentObject.WorldPosition.Y));
				}
				else
				{
					CloneObject(CurrentObject, Cursor.GetCurrentLayerPosition());
				}
				Undo.SaveUndo("copy");
			}
			else
			{
				float num10 = ((!GameInput.ControlDown) ? 1f : 10f);
				if (KeyPressed(Keys.NumPad4))
				{
					MoveObject(CurrentObject, new Vector2(CurrentObject.Position.X - num10, CurrentObject.Position.Y));
				}
				else if (KeyPressed(Keys.NumPad6))
				{
					MoveObject(CurrentObject, new Vector2(CurrentObject.Position.X + num10, CurrentObject.Position.Y));
				}
				else if (KeyPressed(Keys.NumPad8))
				{
					MoveObject(CurrentObject, new Vector2(CurrentObject.Position.X, CurrentObject.Position.Y - num10));
				}
				else if (KeyPressed(Keys.NumPad2))
				{
					MoveObject(CurrentObject, new Vector2(CurrentObject.Position.X, CurrentObject.Position.Y + num10));
				}
				num10 = ((!GameInput.ControlDown) ? 1f : 45f);
				if (KeyPressed(Keys.NumPad7))
				{
					CurrentObject.SetRotation(CurrentObject.Rotation - Utils.DegToRad(num10));
				}
				else if (KeyPressed(Keys.NumPad9))
				{
					CurrentObject.SetRotation(CurrentObject.Rotation + Utils.DegToRad(num10));
				}
				num10 = ((!GameInput.ControlDown) ? 0.1f : 0.5f);
				if (KeyPressed(Keys.NumPad1))
				{
					CurrentObject.SetScale(CurrentObject.Scale.X - num10);
				}
				else if (KeyPressed(Keys.NumPad3))
				{
					CurrentObject.SetScale(CurrentObject.Scale.X + num10);
				}
				else if (!GameInput.ControlDown && GameInput.AltDown && GameInput.AltDown && KeyPressed(Keys.C))
				{
					MoveObjectToLayer();
				}
			}
		}
		if (SelectedDragAction != DragAction.OnPanel && (GameInput.MousePressed(GameInput.MouseButton.Right) || KeyPressed(Keys.Escape)) && (CurrentTool != EditorToolEnum.Path || CurrentEventNode == null))
		{
			UnselectSelectionGroup();
			SetCurrentObject(null);
		}
		if (SelectedPanel == null && SelectedDragAction != DragAction.OnPanelComponent && SelectedDragAction != DragAction.OnPanel && !GameInput.MouseDown(GameInput.MouseButton.Middle) && DragAction != DragAction.OnPanel)
		{
			if (GameInput.GetMouseWheelDelta() > 0)
			{
				CurrentLevel.Camera.ZoomTo(CurrentLevel.Camera.Zoom / 0.8f, CurrentLevel.Camera.Zoom * 2f, useFrameTime: true);
			}
			else if (GameInput.GetMouseWheelDelta() < 0)
			{
				CurrentLevel.Camera.ZoomTo(CurrentLevel.Camera.Zoom * 0.8f, CurrentLevel.Camera.Zoom * 2f, useFrameTime: true);
			}
		}
	}

	public void ReloadCurrentObject()
	{
		if (CurrentObject == null || CurrentObject.PartData == null)
		{
			return;
		}
		GameObject mainPart = CurrentObject.GetMainPart();
		GameObject currentObject = CurrentObject;
		if (CurrentObject.Emitter != null && CurrentTool == EditorToolEnum.Emiter)
		{
			CurrentObject.Emitter.Data.Save();
		}
		int num = mainPart.MyLayer.GameObjectList.IndexOf(mainPart);
		mainPart.PartData = ReloadPartData(mainPart.PartData.Id);
		CloneObject(mainPart, mainPart.WorldTransformation.Position);
		if (CurrentObject != null)
		{
			if (num >= 0)
			{
				CurrentObject.MyLayer.GameObjectList.Remove(CurrentObject);
				CurrentObject.MyLayer.GameObjectList.Insert((num < CurrentObject.MyLayer.GameObjectList.Count) ? num : 0, CurrentObject);
			}
			CurrentObject.Activate();
			CurrentLevel.ChangeLinkedObject(mainPart, CurrentObject);
			if (CurrentLevel.Content.PartDatas.ContainsKey(CurrentObject.PartData.Id.ToLower()))
			{
				CurrentLevel.Content.PartDatas[mainPart.PartData.Id.ToLower()] = CurrentObject.PartData;
			}
			else
			{
				GameCore.GlobalContent.PartDatas[mainPart.PartData.Id.ToLower()] = CurrentObject.PartData;
			}
		}
		if (CurrentObject != null && currentObject != mainPart)
		{
			GameObject childByIdDeep = CurrentObject.GetChildByIdDeep(currentObject.Id);
			if (childByIdDeep != null)
			{
				CurrentObject = childByIdDeep;
			}
		}
		if (CurrentObject != null && mainPart != CurrentObject.GetMainPart() && CurrentTool == EditorToolEnum.Emiter)
		{
			SetSamePartEditValues(mainPart, CurrentObject.GetMainPart());
		}
		if (_objectMover.MyObject == mainPart)
		{
			_objectMover.RefresObject(CurrentObject.GetMainPart());
		}
		mainPart.Destroy();
		RefreshPartDataList();
	}

	public void ReloadLevelFull()
	{
		DataConstant.Reload();
		GameJnG.Campaign = Campaign.LoadCampaign(GameJnG.Difficulty);
		GameCore.Console.AddMessage($"- Campaign for difficulty {GameJnG.Difficulty} reloaded.", ConsoleMessageType.System);
		Layer currentLayer = CurrentLayer;
		StartCameraPosition = CurrentLevel.Camera.Position;
		GameCore.Console.AddMessage($"- Reloading current level: '{GameCore.GameLevel.Id}' from '{GameCore.GameLevel.FileName}'...", ConsoleMessageType.Command);
		LevelData levelData = ((GameCore.GameLevel.LevelData == null) ? null : GameJnG.Campaign.GetLevelData(GameCore.GameLevel.LevelData.Id));
		GameCore.LoadGameLevel(GameCore.GameLevel.FileName, levelData, !GameCore.IsEditorEnabled);
		if (!GameCore.IsEditorEnabled)
		{
			return;
		}
		if (currentLayer != null)
		{
			foreach (Layer layer in GameCore.GameLevel.LayerList)
			{
				if (Utils.Equals(layer.Id, currentLayer.Id))
				{
					CurrentLayer = layer;
					break;
				}
			}
		}
		Popup();
		CurrentLevel.Camera.Position = StartCameraPosition;
	}

	public void SetTool(EditorToolEnum tool)
	{
		CurrentTool = tool;
		UnselectAll(unselectCurrentObject: false);
		ToolEdge = null;
		if (tool == EditorToolEnum.Path)
		{
			if (CurrentObject != null)
			{
				ToolPath.CurrentPath = CurrentObject.MyPath;
			}
		}
		else
		{
			ToolPath.CurrentPath = null;
		}
		if (tool == EditorToolEnum.Emiter)
		{
			_panelEmitter.IsEnabled = true;
			_panelEmitterTime.IsEnabled = true;
			_panelEmitterStrip.IsEnabled = true;
			_panelGameObjectTree.IsEnabled = true;
			_panelAtlasList.IsEnabled = true;
		}
		else
		{
			_panelEmitter.IsEnabled = false;
			_panelEmitterTime.IsEnabled = false;
			_panelEmitterStrip.IsEnabled = false;
			_panelGameObjectTree.IsEnabled = false;
			_panelAtlasList.IsEnabled = false;
		}
	}

	private ParticleEmitterData ReloadEmitterData(PartData partData)
	{
		ParticleEmitterData result = null;
		if (partData.EmitterDataId != null)
		{
			result = ReloadEmitterData(partData.EmitterDataId);
		}
		partData.ChildList?.ForEach((PartData childData) =>
		{
			ReloadEmitterData(childData);
		});
		return result;
	}

	private ParticleEmitterData ReloadEmitterData(string emitterId)
	{
		GameContent gameContent = null;
		ParticleEmitterData result = null;
		ParticleEmitterData particleEmitterData = GameCore.GlobalContent.GetParticleEmitterData(emitterId);
		if (particleEmitterData != null)
		{
			gameContent = GameCore.GlobalContent;
		}
		else
		{
			particleEmitterData = CurrentLevel.Content.GetParticleEmitterData(emitterId);
			if (particleEmitterData != null)
			{
				gameContent = CurrentLevel.Content;
			}
		}
		if (gameContent != null)
		{
			if (particleEmitterData.TemplateId != null)
			{
				ReloadEmitterData(particleEmitterData.TemplateId);
			}
			gameContent.RemoveParticleEmitterData(emitterId);
			DataLoader.LoadDataFile(particleEmitterData.FileName, gameContent, DataType.Emitter, emitterId);
			GameCore.Log($"Emitter data {emitterId} reloaded.");
		}
		else
		{
			GameCore.LogErr("Unable to reload emitter data [{0}]", emitterId);
		}
		return result;
	}

	private void ReloadChildPartData(PartData partData)
	{
		partData.ChildList?.ForEach((PartData childData) =>
		{
			if (childData.TeplateId != null)
			{
				ReloadPartData(childData.TeplateId);
			}
			ReloadChildPartData(childData);
		});
	}

	private PartData ReloadPartData(string id)
	{
		GameContent gameContent = null;
		PartData partData = null;
		partData = GameCore.GlobalContent.GetPartData(id);
		if (partData != null)
		{
			gameContent = GameCore.GlobalContent;
		}
		else
		{
			partData = CurrentLevel.Content.GetPartData(id);
			if (partData != null)
			{
				gameContent = CurrentLevel.Content;
			}
		}
		if (gameContent != null)
		{
			gameContent.PartDatas.Remove(partData.Id.ToLower());
			if (partData.TeplateId != null)
			{
				ReloadPartData(partData.TeplateId);
			}
			DataLoader.LoadDataFile(partData.FileName, gameContent, DataType.Part, partData.Id);
			partData = gameContent.GetPartData(partData.Id);
			if (partData == null)
			{
				GameCore.LogErr("Reload part data [{0}] failed.", id);
			}
			else
			{
				if (partData.EmitterDataId != null)
				{
					partData.EmitterData = ReloadEmitterData(partData);
				}
				ReloadChildPartData(partData);
				ReloadEmitterData(partData);
				partData.Init();
				GameCore.Log($"Part data {partData.Id} reloaded.");
			}
		}
		else
		{
			GameCore.LogErr("Unable to reload part data [{0}]", id);
		}
		return partData;
	}

	private void UpdatePanelPositions()
	{
		Viewport viewport = GameCore.GraphicsDeviceStatic.Viewport;
		if (_panelToolBar.IsDocked)
		{
			_panelToolBar.Position.Y = _panelLayerBar.Height;
		}
		_levelScrollBar.Position = new Vector2(0f, viewport.Height - _levelScrollBar.Height);
		_levelScrollBar.Width = (int)((float)viewport.Width - _levelScrollBar.Position.X);
		_panelBehaviorEvent.CheckOutOfScreen();
		_panelGameObjectDebug.Position = _panelGameObject.Position;
		_panelGameObjectDebug.Height = (int)((float)GameCore.MainViewPort.Bounds.Bottom - _panelGameObjectDebug.Position.Y - (float)_levelScrollBar.Height);
		if (_panelBehaviorEvent.IsEnabled && !_panelBehaviorEvent.IsOff)
		{
			_panelGameObjectDebug.Position.X = _panelBehaviorEvent.Position.X + (float)_panelBehaviorEvent.Width;
		}
		else if (_panelGameObject.IsEnabled)
		{
			_panelGameObjectDebug.Position.X += _panelGameObject.Width;
		}
		UpdateObjectPreviewPosition();
	}

	public void UpdateObjectPreviewPosition()
	{
		if (_panelPartDataList.Position.X + (float)_panelPartDataList.Width + (float)_gameObjectPreview.Width > (float)_gameCore.Window.ClientBounds.Width)
		{
			_gameObjectPreview.Position.X = _panelPartDataList.Position.X - (float)_gameObjectPreview.Width;
		}
		else
		{
			_gameObjectPreview.Position.X = _panelPartDataList.Position.X + (float)_panelPartDataList.Width;
		}
		_gameObjectPreview.Position.Y = Math.Min(Math.Max(0f, Cursor.Position.Y - (float)(_gameObjectPreview.Height / 2)), _gameCore.Window.ClientBounds.Height - _gameObjectPreview.Height - 40);
	}

	private void UnlinkDestroyedObjects()
	{
		foreach (Layer layer in CurrentLevel.LayerList)
		{
			foreach (GameObject gameObject in layer.GameObjectList)
			{
				if (gameObject.MyTimeLine != null)
				{
					gameObject.MyTimeLine.UnlinkDestroyedObjects();
				}
				if (gameObject.MyPath == null)
				{
					continue;
				}
				foreach (BehaviorEventNode eventNode in gameObject.MyPath.EventNodeList)
				{
					if (eventNode.LinkedObjectList != null)
					{
						eventNode.LinkedObjectList.RemoveAll((GameObject o) => o.IsDestroyed);
					}
					if (eventNode.TimeLine != null)
					{
						eventNode.TimeLine.UnlinkDestroyedObjects();
					}
				}
			}
		}
	}

	public void ParseLinkInput()
	{
		if (CurrentObject == null || SelectedComponent != null || SelectedPanel != null || !KeyPressed(Keys.R) || ToolPath.ParseLinkInput())
		{
			return;
		}
		if (GameInput.AltDown)
		{
			if (CurrentSelectionGroup != null && CurrentObject == CurrentSelectionGroup.SelectionObject)
			{
				foreach (GameObject item in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
				{
					if (item.LinkedObjectList != null)
					{
						item.LinkedObjectList.Clear();
					}
				}
				return;
			}
			if (CurrentObject.LinkedObjectList != null)
			{
				CurrentObject.LinkedObjectList.Clear();
			}
		}
		else
		{
			if (SelectedObject == null || CurrentObject == SelectedObject)
			{
				return;
			}
			GameObject gameObject;
			GameObject gameObject2;
			if (GameInput.ControlDown)
			{
				gameObject = SelectedObject;
				gameObject2 = CurrentObject;
			}
			else
			{
				gameObject = CurrentObject;
				gameObject2 = SelectedObject;
			}
			if (CurrentSelectionGroup != null && gameObject == CurrentSelectionGroup.SelectionObject)
			{
				foreach (GameObject item2 in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
				{
					if (item2.LinkedObjectList != null && item2.LinkedObjectList.Contains(gameObject2))
					{
						item2.RemoveLinkedObject(gameObject2);
					}
					else
					{
						item2.AddLinkedObject(gameObject2);
					}
				}
			}
			else if (CurrentSelectionGroup != null && gameObject2 == CurrentSelectionGroup.SelectionObject)
			{
				foreach (GameObject item3 in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
				{
					if (gameObject.LinkedObjectList != null && gameObject.LinkedObjectList.Contains(item3))
					{
						gameObject.RemoveLinkedObject(item3);
					}
					else
					{
						gameObject.AddLinkedObject(item3);
					}
				}
			}
			else if (gameObject.LinkedObjectList != null && gameObject.LinkedObjectList.Contains(gameObject2))
			{
				gameObject.RemoveLinkedObject(gameObject2);
			}
			else
			{
				gameObject.AddLinkedObject(gameObject2);
			}
			if (GameInput.ShiftDown)
			{
				SetCurrentObject(gameObject2);
			}
		}
	}

	public void UpdateTriggerPoint(float elapsedTime)
	{
		if (!GameInput.MouseDown(GameInput.MouseButton.Left))
		{
			SelectedTriggerPointObject = null;
		}
		if (IsDragAvailable)
		{
			foreach (GameObject gameObject2 in CurrentLayer.GameObjectList)
			{
				if (gameObject2 == null || (!IsShowPathEnabled && gameObject2 != CurrentObject) || !(gameObject2.TriggerPoint != Vector2.Zero) || !Cursor.IsInCircle(CurrentLevel.Camera.CurrentLayerPosToScreen(gameObject2.TriggerPoint), 10))
				{
					continue;
				}
				if ((KeyPressed(Keys.Delete) || KeyPressed(Keys.D)) && !GameInput.ShiftDown && !GameInput.ControlDown && !GameInput.AltDown)
				{
					DeleteObjectAllowed = false;
					gameObject2.TriggerPoint = Vector2.Zero;
					SelectedTriggerPointObject = null;
					break;
				}
				SelectedDragAction = DragAction.MoveTriggerPoint;
				SelectedTriggerPointObject = gameObject2;
				if (GameInput.MousePressed(GameInput.MouseButton.Left))
				{
					DragAction = DragAction.MoveTriggerPoint;
					DragOffset = CursorWorldPos - gameObject2.TriggerPoint;
				}
				break;
			}
		}
		GameObject gameObject = SelectedTriggerPointObject ?? CurrentObject;
		if (gameObject != null && (KeyPressed(Keys.D) || (KeyPressed(Keys.T) && CurrentTool == EditorToolEnum.Object)))
		{
			if (GameInput.ShiftDown)
			{
				gameObject.TriggerPoint = Vector2.Zero;
				if (CurrentSelectionGroup != null)
				{
					foreach (GameObject item in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
					{
						item.TriggerPoint = Vector2.Zero;
					}
				}
			}
			else if (GameInput.AltDown && gameObject.TriggerPoint != Vector2.Zero)
			{
				CurrentLevel.Camera.Position.X = (gameObject.TriggerPoint.X - 1920f + 1f) / CurrentLayer.ScrollFactor.X;
			}
			else if (GameInput.ControlDown)
			{
				if (CurrentSelectionGroup != null)
				{
					foreach (GameObject item2 in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
					{
						if (item2.TriggerPoint != Vector2.Zero)
						{
							item2.TriggerPoint = new Vector2(CurrentLevel.Camera.ScreenBottomRightInWorld.X, gameObject.TriggerPoint.Y);
						}
						else
						{
							item2.TriggerPoint = new Vector2(CurrentLevel.Camera.ScreenBottomRightInWorld.X, gameObject.WorldPosition.Y);
						}
					}
				}
				else if (gameObject.TriggerPoint != Vector2.Zero)
				{
					gameObject.TriggerPoint = new Vector2(CurrentLevel.Camera.ScreenBottomRightInWorld.X, gameObject.TriggerPoint.Y);
				}
				else
				{
					gameObject.TriggerPoint = new Vector2(CurrentLevel.Camera.ScreenBottomRightInWorld.X, gameObject.WorldPosition.Y);
				}
			}
			else if (CurrentSelectionGroup != null)
			{
				foreach (GameObject item3 in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
				{
					item3.TriggerPoint = CursorWorldPos;
				}
			}
			else
			{
				gameObject.TriggerPoint = CursorWorldPos;
			}
		}
		if (SelectedTriggerPointObject != null)
		{
			Cursor.Text = $"Trigger point [{(int)SelectedTriggerPointObject.TriggerPoint.X}, {(int)SelectedTriggerPointObject.TriggerPoint.Y}]";
			if (DragAction == DragAction.MoveTriggerPoint)
			{
				SelectedTriggerPointObject.TriggerPoint = CursorWorldPos - DragOffset;
			}
		}
	}

	public void UpdateEdgePainter(float elapsedTime)
	{
		if (ToolEdge != null)
		{
			ToolEdge.Update(elapsedTime);
		}
	}

	private void SetDragTreshold()
	{
		_dragTresholdCenter = Cursor.Position;
		_isDragTresholdReached = false;
	}

	public void SetPanelFocus(Panel panel)
	{
		int num = _panelList.IndexOf(panel);
		if (num >= 0 && num < _panelList.Count - 1)
		{
			_panelList.RemoveAt(num);
			_panelList.Add(panel);
		}
	}

	public void MoveObject(GameObject go, Vector2 newPos, Vector2? triggerDelta = null)
	{
		Vector2 vector = newPos - go.WorldPosition;
		go.SetPosition(newPos);
		if (CurrentSelectionGroup != null && go == CurrentSelectionGroup.SelectionObject)
		{
			foreach (GameObject item in go.ChildObjectListFront)
			{
				item.ResetPrecalculations();
				if (item.MyPath != null && item.MyPath.MyObject == item)
				{
					item.MyPath.Rotate(item.WorldTransformation.Rotation - item.OldRotation);
					item.MyPath.Move(item.WorldTransformation.Position - item.OldPosition);
				}
				if (item.TriggerPoint != Vector2.Zero)
				{
					item.TriggerPoint += triggerDelta ?? vector;
				}
			}
		}
		else if (go.MyPath != null && go.MyPath.MyObject == go)
		{
			go.MyPath.Move(triggerDelta ?? (go.Position - go.OldPosition));
		}
		if (go.TriggerPoint != Vector2.Zero)
		{
			go.TriggerPoint += triggerDelta ?? vector;
		}
		go.AwakePhysics(resetDynamics: true);
	}

	public void Update(float elapsedTime)
	{
		DeleteObjectAllowed = true;
		switch (CurrentTool)
		{
		case EditorToolEnum.Object:
			Cursor.Color = Color.White;
			break;
		case EditorToolEnum.Polygon:
			Cursor.Color = new Color(255, 255, 100);
			break;
		case EditorToolEnum.Emiter:
			Cursor.Color = new Color(100, 200, 255);
			break;
		case EditorToolEnum.Edge:
			Cursor.Color = Color.Green;
			break;
		case EditorToolEnum.Path:
			Cursor.Color = new Color(255, 50, 50);
			break;
		}
		Cursor.TextColor = Color.White;
		Cursor.Update(elapsedTime);
		if (CurrentLayer != null)
		{
			CurrentLevel.Camera.SetLayer(CurrentLayer);
			CursorWorldPos = Cursor.GetCurrentLayerPosition();
		}
		if ((_objectListMode == ObjectListModeEnum.Visible || _objectListMode == ObjectListModeEnum.Activated) && _panelObjectList.IsEnabled)
		{
			RefreshObjectList();
		}
		_objectMover.Update(GameCore.ElapsedTime);
		if (CurrentObject != null)
		{
			CurrentObject.EditorUpdateBegin(this, elapsedTime);
			if (CurrentSelectionGroup != null && CurrentObject == CurrentSelectionGroup.SelectionObject)
			{
				foreach (GameObject item in CurrentObject.ChildObjectListFront)
				{
					item.OldPosition = item.WorldTransformation.Position;
					item.OldRotation = item.WorldTransformation.Rotation;
				}
			}
			else
			{
				CurrentObject.OldPosition = CurrentObject.Position;
				CurrentObject.OldRotation = CurrentObject.Rotation;
			}
		}
		SelectedDragAction = DragAction.None;
		SelectedPanel = null;
		Cursor.Text = null;
		Cursor.SetType(CursorType.Arrow);
		Cursor.Rotation = 0f;
		if (DragAction == DragAction.MoveCamera)
		{
			CurrentLevel.Camera.Position = DragCenter - (Cursor.Position / CurrentLevel.Camera.Zoom - DragOffset);
			Cursor.Text = $"Move camera ({CurrentLevel.Camera.Position.X}, {CurrentLevel.Camera.Position.Y})";
		}
		else if (DragAction == DragAction.ZoomCamera)
		{
			float num = (Cursor.Position - DragCenter).Y * CurrentLevel.Camera.Zoom;
			DragCenter = Cursor.Position;
			if (num < 0f)
			{
				CurrentLevel.Camera.SetZoom(CurrentLevel.Camera.Zoom - num / 300f);
				if (CurrentLevel.Camera.Zoom > 10f)
				{
					CurrentLevel.Camera.SetZoom(10f);
				}
			}
			else if (num > 0f)
			{
				CurrentLevel.Camera.SetZoom(CurrentLevel.Camera.Zoom - num / 300f);
				if (CurrentLevel.Camera.Zoom < 0.01f)
				{
					CurrentLevel.Camera.SetZoom(0.01f);
				}
			}
			Cursor.Text = $"Zoom camera ({CurrentLevel.Camera.Zoom * 100f} %)";
		}
		ParseInputTools();
		_levelScrollBar.Update(elapsedTime);
		UpdatePanelPositions();
		_panelGameObject.IsOff = CurrentObject == null;
		_panelGameObjectDebug.IsOff = _panelGameObject.IsOff;
		_panelToolBar.Update(elapsedTime);
		if (!IsNoPanelModeEnabled)
		{
			_panelLayerBar.Update(elapsedTime);
			for (int num2 = _panelList.Count - 1; num2 >= 0; num2--)
			{
				_panelList[num2].Update(elapsedTime);
			}
		}
		_panelToolBar.UpdateComponents(elapsedTime);
		if (!IsNoPanelModeEnabled)
		{
			_panelLayerBar.UpdateComponents(elapsedTime);
			for (int num3 = _panelList.Count - 1; num3 >= 0; num3--)
			{
				_panelList[num3].UpdateComponents(elapsedTime);
			}
		}
		_panelPartDataList.AfterUpdate();
		if (DragAction == DragAction.MovePanel && DraggedPanel != null)
		{
			SetPanelFocus(DraggedPanel);
		}
		if (CurrentObject == null || CurrentObject.Emitter == null)
		{
			_panelEmitter.IsOff = true;
			_panelEmitterTime.IsOff = true;
			_panelEmitterStrip.IsOff = true;
			_panelAtlasList.IsOff = true;
		}
		else
		{
			_panelEmitter.IsOff = false;
			_panelEmitterTime.IsOff = false;
			_panelAtlasList.IsOff = false;
			if (CurrentObject.Emitter.Data.IsStripLine)
			{
				_panelEmitterStrip.IsOff = false;
			}
			else
			{
				_panelEmitterStrip.IsOff = true;
			}
		}
		if (CurrentEventNode != null && CurrentEventNode.IsDestroyd)
		{
			SetCurretEventNode(null);
			_panelBehaviorEvent.CurrentEventNode = null;
		}
		CurrentLevel.CurrentLayer = CurrentLayer;
		_gameObjectPreview.Update(GameCore.ElapsedTime);
		if (_refreshLayerBarRequest)
		{
			_panelLayerBar.Refresh();
			RefreshObjectList();
			_refreshLayerBarRequest = false;
		}
		if (_refreshObjecttListRequest)
		{
			RefreshObjectList();
			_refreshObjecttListRequest = false;
		}
		if (CurrentTool == EditorToolEnum.Polygon)
		{
			ToolPolygon.Begin();
			if (IsShowPolygonEnabled)
			{
				foreach (GameObject visibleGameObject in CurrentLayer.VisibleGameObjectList)
				{
					if (visibleGameObject.PolygonRenderer != null)
					{
						ToolPolygon.Update(visibleGameObject, elapsedTime);
					}
				}
			}
			else if (CurrentObject != null && CurrentObject.PolygonRenderer != null)
			{
				ToolPolygon.Update(CurrentObject, elapsedTime);
			}
		}
		UpdateTriggerPoint(elapsedTime);
		if (CurrentObject != null)
		{
			CurrObjectScreenTrans = new Transformation(CurrentObject.WorldTransformation);
			Transformation.ConvertToScreen(ref CurrObjectScreenTrans, CurrentObject.MyLayer);
			UpdateRotationRing();
			UpdateObjectMoveGizmo();
			if ((CurrentTool != EditorToolEnum.Polygon || CurrentObject.PolygonRenderer == null) && CurrentTool != EditorToolEnum.Path)
			{
				UpdateScaleRectangle(GameInput.ShiftDown);
			}
		}
		if (IsShowSafeLineEnabled)
		{
			UpdateSafeLine();
		}
		UpdateEdgePainter(elapsedTime);
		if (CurrentTool == EditorToolEnum.Path)
		{
			GameObject currentObject = CurrentObject;
			if (currentObject != null && ToolTimeLine.CurrentTimeLine == null && KeyPressed(Keys.T))
			{
				BehaviorEventNode behaviorEventNode = ToolPath.SelectedEventNode ?? ToolPath.DraggedEventNode ?? CurrentEventNode;
				if (behaviorEventNode != null)
				{
					if (behaviorEventNode.TimeLine == null)
					{
						behaviorEventNode.TimeLine = new BehaviorTimeLine();
						behaviorEventNode.TimeLine.MyObject = currentObject;
						if (currentObject.MyPath != null)
						{
							Vector2 position = currentObject.MyPath.GetPosition(behaviorEventNode.Time);
							Vector2 screenPosition = GameCore.CurrentLevel.Camera.CurrentLayerPosToScreen(position);
							behaviorEventNode.TimeLine.ScreenPosition = screenPosition;
						}
						ToolTimeLine.CurrentTimeLine = behaviorEventNode.TimeLine;
					}
				}
				else if (currentObject.MyTimeLine == null || currentObject.MyTimeLine.EventNodeList.IsNullOrEmpty())
				{
					currentObject.MyTimeLine = new BehaviorTimeLine();
					currentObject.MyTimeLine.MyObject = currentObject;
					currentObject.SetCurrentTimeLine(currentObject.MyTimeLine);
					currentObject.MyTimeLine.ScreenPosition = currentObject.GetScreenPos();
					ToolTimeLine.CurrentTimeLine = currentObject.MyTimeLine;
				}
			}
			ToolTimeLine.Update(elapsedTime);
			ToolPath.Update(elapsedTime);
		}
		ParseInput(elapsedTime);
		if (CreatorObject != null)
		{
			CreatorObject.ActiveMode = ActiveModeEnum.AlwaysActive;
			CurrentLevel.CurrentLayer = CurrentLayer;
			CreatorObject.SetPosition(CursorWorldPos);
			if (!GameCore.IsPaused)
			{
				CreatorObject.OutOfView = OutOfFrame.None;
				CreatorObject.Update(GameCore.ElapsedTime);
			}
			if (SelectedDragAction == DragAction.None && DragAction == DragAction.None && GameInput.MousePressed(GameInput.MouseButton.Left))
			{
				PartData partData = null;
				if (CurrentPartData != null)
				{
					partData = CurrentPartData;
				}
				else if (CreatorObject.PartData != null)
				{
					partData = CreatorObject.PartData;
				}
				GameObject gameObject;
				if (partData != null)
				{
					gameObject = CurrentLayer.CreateObjectByPartData(partData, CreatorObject.Position, CreatorObject.Scale, CreatorObject.Rotation);
					gameObject.IsLevelObject = true;
					gameObject.CreateRigidBodyByPoygonRenderer();
				}
				else
				{
					gameObject = new GameObject();
				}
				gameObject.Frame = CreatorObject.Frame;
				gameObject.SetColor(CreatorObject.Color);
				if (gameObject.RigidBody != null)
				{
					gameObject.RigidBody.IsEnabled = true;
					gameObject.RigidBody.WakeUp(resetDynamics: true);
				}
				gameObject.Init();
				gameObject.LevelInit();
				RefreshObjectList();
				if (gameObject.IsDestroyed)
				{
					SetCurrentObject(null);
				}
				else
				{
					SetCurrentObject(gameObject);
				}
				SelectedDragAction = DragAction.MoveXY;
			}
			CurrentLevel.CurrentLayer = null;
		}
		else
		{
			if (IsDragAvailable)
			{
				SelectedObject = GameLevel.GetGameObjectAtPoint(CurrentLayer.VisibleGameObjectList, CursorWorldPos, bCheckIsAvailableFromEditor: true, KeyDown(Keys.X), bOnlyMainParts: true);
				if (SelectedObject != null)
				{
					SelectedDragAction = DragAction.SelecObject;
					if (GameInput.ControlDown)
					{
						if (CurrentSelectionGroup == null || SelectedObject != CurrentSelectionGroup.SelectionObject)
						{
							Cursor.Text = string.Format("Add to selection {0} [{1}, {2}]", SelectedObject.Id, SelectedObject.Position.X.ToString("0.#", Utils.FloatFI), SelectedObject.Position.Y.ToString("0.#", Utils.FloatFI));
							if (GameInput.MousePressed(GameInput.MouseButton.Left))
							{
								if (CurrentSelectionGroup == null)
								{
									CurrentSelectionGroup = new SelectionGroup(this);
								}
								if (CurrentObject != null && CurrentObject != SelectedObject && CurrentObject != CurrentSelectionGroup.SelectionObject)
								{
									if (CurrentSelectionGroup.SelectionObject.ChildObjectListFront == null)
									{
										CurrentSelectionGroup.SelectionObject.Position = CurrentObject.Position;
									}
									CurrentSelectionGroup.ObjectList.Add(CurrentObject);
									CurrentSelectionGroup.UpdateCenter();
								}
								else if (CurrentSelectionGroup.SelectionObject.ChildObjectListFront == null)
								{
									CurrentSelectionGroup.SelectionObject.Position = SelectedObject.Position;
								}
								CurrentSelectionGroup.DetachSelectedObjects();
								CurrentSelectionGroup.SelectionObject.Rotation = 0f;
								CurrentSelectionGroup.SelectionObject.SetScale(1f);
								CurrentSelectionGroup.ObjectList.Add(SelectedObject);
								CurrentSelectionGroup.AddObjectListToSelection();
								SetCurrentObject(CurrentSelectionGroup.SelectionObject);
							}
						}
					}
					else
					{
						Cursor.Text = string.Format("Select {0} [{1}, {2}]", SelectedObject.Id, SelectedObject.Position.X.ToString("0.#", Utils.FloatFI), SelectedObject.Position.Y.ToString("0.#", Utils.FloatFI));
						if (GameInput.MousePressed(GameInput.MouseButton.Left))
						{
							SetCurrentObject(SelectedObject);
							SetDragMoveOffset();
							if (CurrentTool != EditorToolEnum.Polygon)
							{
								DragAction = DragAction.MoveXY;
							}
							SetDragTreshold();
						}
					}
				}
				else if (CurrentSelectionGroup != null && CurrentSelectionGroup.SelectionObject != null && GameInput.ControlDown)
				{
					SelectedObject = GameLevel.GetGameObjectAtPoint(CurrentSelectionGroup.SelectionObject.ChildObjectListFront, CursorWorldPos, bCheckIsAvailableFromEditor: true, KeyDown(Keys.X), bOnlyMainParts: false);
					if (SelectedObject != null)
					{
						Cursor.Text = string.Format("Remove from selection {0} [{1}, {2}]", SelectedObject.Id, SelectedObject.Position.X.ToString("0.#", Utils.FloatFI), SelectedObject.Position.Y.ToString("0.#", Utils.FloatFI));
						if (GameInput.MousePressed(GameInput.MouseButton.Left))
						{
							CurrentSelectionGroup.DetachSelectedObjects();
							CurrentSelectionGroup.SelectionObject.Rotation = 0f;
							CurrentSelectionGroup.SelectionObject.SetScale(1f);
							CurrentSelectionGroup.ObjectList.Remove(SelectedObject);
							CurrentSelectionGroup.UpdateCenter();
							CurrentSelectionGroup.AddObjectListToSelection();
							SetCurrentObject(CurrentSelectionGroup.SelectionObject);
						}
					}
				}
			}
			ParseLinkInput();
			if (CurrentObject != null)
			{
				if (_isDragTresholdReached)
				{
					Vector2 newPos = CursorWorldPos + DragOffset;
					if (DragAction == DragAction.MoveXY)
					{
						MoveObject(CurrentObject.GetMainPart(), newPos);
					}
					else if (DragAction == DragAction.MoveX)
					{
						MoveObject(CurrentObject, new Vector2(newPos.X, CurrentObject.GetMainPart().Position.Y));
					}
					else if (DragAction == DragAction.MoveY)
					{
						MoveObject(CurrentObject, new Vector2(CurrentObject.GetMainPart().Position.X, newPos.Y));
					}
				}
				else if ((Cursor.Position - _dragTresholdCenter).Length() > 2f)
				{
					_isDragTresholdReached = true;
				}
				CurrentObject.EditorUpdateEnd(this, elapsedTime);
			}
		}
		UpdateMoveLayer();
		UpdateSelectionGroup();
		if (IsDragUndoable && GameInput.MouseReleased(GameInput.MouseButton.Left))
		{
			Undo.SaveUndo("Drag");
		}
		if (!GameInput.ControlDown && GameInput.MouseDown(GameInput.MouseButton.Middle))
		{
			DragAction = DragAction.MoveCamera;
			DragCenter = Cursor.Position / CurrentLevel.Camera.Zoom;
			DragOffset = CurrentLevel.Camera.Position;
		}
		else if (GameInput.ControlDown && GameInput.MouseDown(GameInput.MouseButton.Middle))
		{
			DragAction = DragAction.ZoomCamera;
			DragCenter = Cursor.Position;
		}
		else if (!GameInput.MouseDown(GameInput.MouseButton.Left) && !_keyboardDrag && (!GameInput.MouseDown(GameInput.MouseButton.Right) || DragButton != GameInput.MouseButton.Right))
		{
			DragAction = DragAction.None;
			DragOffset = Vector2.Zero;
			DragCenter = Vector2.Zero;
			DragButton = GameInput.MouseButton.Left;
			SelectedComponent = null;
			DraggedPanel = null;
		}
		CurrentLevel.PhysWorld.ProcessChanges();
		UpdateCursorType();
		for (int i = 0; i < 3; i++)
		{
			if (!IsMouseClickFree[i])
			{
				IsMouseClickFree[i] = !GameInput.MouseDown((GameInput.MouseButton)i);
			}
		}
	}

	public void DrawObjectGizmo(GameObject go)
	{
		Vector2 vector = go.MyLevel.Camera.LayerPosToScreen(go.WorldTransformation.Position, go.MyLayer);
		if (go.PartData != null)
		{
			_objectSymbolRenderer.Frame = go.PartData.EditorGizmo;
			_objectSymbolRenderer.Color = go.PartData.EditorColor;
		}
		else
		{
			_objectSymbolRenderer.Frame = 0;
			_objectSymbolRenderer.Color = Color.LightGray;
		}
		_objectSymbolRenderer.Draw(Matrix.CreateTranslation(vector.X, vector.Y, 0f), SpriteBatch, 0f, BlendModeEnum.Additive);
	}

	private void DrawObjectDiffSymbol(GameObject go)
	{
		if (!go.DifficultyRange.IsInRange(GameJnG.Difficulty) || (go.GameModeList != null && !go.GameModeList.Contains((int)GameJnG.GameMode)))
		{
			Vector2 vector = go.MyLevel.Camera.LayerPosToScreen(go.WorldTransformation.Position, go.MyLayer);
			_diffSymbolRenderer.Frame = 0;
			if (go.DifficultyRange.From > GameJnG.Difficulty)
			{
				_diffSymbolRenderer.Color = new Color(255, 0, 0);
			}
			else if (go.DifficultyRange.To < GameJnG.Difficulty)
			{
				_diffSymbolRenderer.Color = new Color(0, 255, 0);
			}
			else
			{
				_diffSymbolRenderer.Color = new Color(0, 100, 255);
			}
			_diffSymbolRenderer.Draw(Matrix.CreateTranslation(vector.X, vector.Y, 0f), SpriteBatch, 0f, BlendModeEnum.Additive);
		}
	}

	private void DrawObjectLinks(GameObject go)
	{
		if (go.LinkedObjectList == null || go.LinkedObjectList.Count <= 0)
		{
			return;
		}
		Vector2 vector = CurrentLevel.Camera.LayerPosToScreen(go.WorldTransformation.Position, go.MyLayer);
		foreach (GameObject linkedObject in go.LinkedObjectList)
		{
			Vector2 to = CurrentLevel.Camera.LayerPosToScreen(linkedObject.WorldTransformation.Position, linkedObject.MyLayer);
			LineBatch.DrawLine(vector, to, Color.Green, Color.DarkRed);
		}
		LineBatch.Flush();
		_objectTransformGizmoRenderer.Frame = 4;
		_objectTransformGizmoRenderer.Color = new Color(255, 0, 0, 255);
		foreach (GameObject linkedObject2 in go.LinkedObjectList)
		{
			Vector2 vec = CurrentLevel.Camera.LayerPosToScreen(linkedObject2.WorldTransformation.Position, linkedObject2.MyLayer);
			_objectTransformGizmoRenderer.Draw(Matrix.CreateRotationZ(Utils.GetRotationTo(vector, vec)) * Matrix.CreateTranslation(vec.X, vec.Y, 0f), SpriteBatch);
		}
	}

	private void DrawObjectDecorations(GameObject go)
	{
		if (!MinimalGizmoMode)
		{
			DrawObjectLinks(go);
		}
		if ((go.PartData == null || IsEditorSetEnabled(go.PartData.EditorSet) || (go.PartData.EditorSet < 0 && ShowAllSymbols)) && (KeyDown(Keys.X) || (!MinimalGizmoMode && (ShowAllSymbols || (!go.HasGraphics && (go.PartData == null || go.PartData.EditorGizmo >= 0)) || go.PartData.LightOrShadow != 0))))
		{
			DrawObjectGizmo(go);
		}
	}

	public void DrawQuickInfo(GameObject go, ref Vector2 screenPos, int childLevel)
	{
		if (go.PartData != null && go.PartData.ShowDebugInfo)
		{
			Color color;
			if (go.IsEnabled)
			{
				if (go.IsActivated)
				{
					color = ((!go.IsTriggerFree) ? new Color(255, 0, 0) : new Color(0, 255, 0));
				}
				else
				{
					color = new Color(150, 150, 150, 200);
				}
			}
			else
			{
				color = new Color(150, 120, 120, 200);
			}
			string text = ((childLevel <= 0) ? "" : new string(' ', childLevel));
			if (Options.DebugQuickInfoMode == 1)
			{
				string text2 = $"{text}{go.Id}: {go.CurrentState.Id} (Ang: {Utils.FormatDecimal1(Utils.RadToDeg(go.Rotation))} / {Utils.FormatDecimal1(Utils.RadToDeg(go.WorldRotation))}, Spd: {Utils.FormatDecimal1(go.Velocity.Length())}), Pos: {go.VirtualScreenPosition}";
				Font.Write(SpriteBatch, text2, screenPos.X, screenPos.Y, color);
				screenPos.Y += 14f;
			}
			if (go.IsKilled)
			{
				color = new Color(100, 100, 100);
			}
			else if (go.IsImmortal)
			{
				color = new Color(255, 150, 50);
			}
			else
			{
				color = ((go.CurrentHP == go.MaxHP) ? new Color(50, 150, 255) : new Color(255, 220, 80));
			}
			Font.Write(SpriteBatch, $"{text}HP: {Utils.FormatDecimal1(go.CurrentHP)} / {Utils.FormatDecimal1(go.MaxHP)}", screenPos.X, screenPos.Y, color);
			screenPos.Y += 14f;
		}
		if (go.ChildObjectListFront != null)
		{
			foreach (GameObject item in go.ChildObjectListFront)
			{
				DrawQuickInfo(item, ref screenPos, childLevel + 1);
			}
		}
		if (go.ChildObjectListBack == null)
		{
			return;
		}
		foreach (GameObject item2 in go.ChildObjectListBack)
		{
			DrawQuickInfo(item2, ref screenPos, childLevel + 1);
		}
	}

	public void DrawPath(GameObject go)
	{
		if (go.MyPath != null)
		{
			ToolPath.DrawPath(go.MyPath, go);
		}
		if (IsShowPathEnabled || go == CurrentObject)
		{
			ToolPath.DrawTriggerPoint(go);
			if (go.CurrentTimeLine != null)
			{
				ToolTimeLine.Draw(CurrentLevel.Camera.CurrentLayerPosToScreen(go.WorldPosition), go.CurrentTimeLine, go.TimeLineTime, new Vector2(0f, -25f));
			}
		}
	}

	private void DrawAllObjectDecorations()
	{
		LineBatch.Begin(BlendState.Additive, Matrix.Identity);
		foreach (GameObject visibleGameObject in CurrentLayer.VisibleGameObjectList)
		{
			DrawObjectDecorations(visibleGameObject);
		}
		LineBatch.End();
		SpriteDrawEnd();
		SpriteDrawBegin();
		if (!MinimalGizmoMode || CurrentTool == EditorToolEnum.Path)
		{
			foreach (GameObject gameObject in CurrentLayer.GameObjectList)
			{
				DrawPath(gameObject);
			}
			if (CurrentSelectionGroup != null && CurrentSelectionGroup.SelectionObject.ChildObjectListFront != null)
			{
				foreach (GameObject item in CurrentSelectionGroup.SelectionObject.ChildObjectListFront)
				{
					ToolPath.DrawTriggerPoint(item);
				}
			}
		}
		LineBatch.End();
	}

	public void SpriteDrawBegin()
	{
		SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
	}

	public void SpriteDrawEnd()
	{
		SpriteBatch.End();
	}

	public void Draw()
	{
		CurrentLevel.CurrentLayer = CurrentLayer;
		CurrentLevel.Camera.SetLayer(CurrentLayer);
		GameCore.EffectLight.LayerBeginDraw(CurrentLayer);
		if (ToolEdge != null)
		{
			ToolEdge.Draw();
		}
		SpriteDrawBegin();
		foreach (Layer layer in CurrentLevel.LayerList)
		{
			if (!layer.IsEnabledByEditor)
			{
				continue;
			}
			foreach (GameObject visibleGameObject in layer.VisibleGameObjectList)
			{
				DrawObjectDiffSymbol(visibleGameObject);
			}
		}
		if (CurrentLayer.IsEnabledByEditor)
		{
			DrawAllObjectDecorations();
		}
		if (CurrentTool == EditorToolEnum.Polygon)
		{
			if (IsShowPolygonEnabled)
			{
				foreach (GameObject visibleGameObject2 in CurrentLayer.VisibleGameObjectList)
				{
					if (visibleGameObject2.PolygonRenderer != null)
					{
						ToolPolygon.Draw(visibleGameObject2);
					}
				}
			}
			else if (CurrentObject != null && CurrentObject.PolygonRenderer != null)
			{
				ToolPolygon.Draw(CurrentObject);
			}
		}
		if (CurrentObject != null)
		{
			CurrObjectScreenTrans = new Transformation(CurrentObject.WorldTransformation);
			Transformation.ConvertToScreen(ref CurrObjectScreenTrans, CurrentObject.MyLayer);
			DrawRotationRing();
			if ((CurrentTool != EditorToolEnum.Polygon || CurrentObject.PolygonRenderer == null) && CurrentTool != EditorToolEnum.Path)
			{
				DrawScaleRectangle();
			}
			DrawObjectMoveGizmo();
			if (!MinimalGizmoMode)
			{
				DebugDraw.ShowGameObjectRect(color: (CurrentSelectionGroup == null || CurrentObject != GameCore.Editor.CurrentSelectionGroup.SelectionObject) ? new Color(200, 50, 200, 200) : ColorControlNormalGroup, go: CurrentObject, rect: CurrentObject.FrameRectangle);
			}
			if (KeyDown(Keys.V))
			{
				if (GameInput.ControlDown)
				{
					CurrentObject.UpdateVisibilityRect();
				}
				if (GameInput.ShiftDown)
				{
					DebugDraw.ShowGameObjectRect(CurrentObject, CurrentObject.FxRectangle, new Color(200, 255, 0, 200));
				}
				else
				{
					DebugDraw.ShowVisibilityRect(CurrentObject);
				}
			}
		}
		if (SelectedDragAction == DragAction.SelecObject && SelectedObject != null)
		{
			_objectTransformGizmoRenderer.Frame = 2;
			_objectTransformGizmoRenderer.Color = GetDragControlColor(SelectedDragAction, DragAction.MoveXY);
			Vector2 vector = CurrentLevel.Camera.CurrentLayerPosToScreen(SelectedObject.WorldTransformation.Position);
			_objectTransformGizmoRenderer.Draw(Matrix.CreateTranslation(vector.X, vector.Y, 0f), SpriteBatch);
			if (!MinimalGizmoMode)
			{
				DebugDraw.ShowGameObjectRect(SelectedObject, SelectedObject.FrameRectangle, new Color(255, 0, 255, 220));
			}
		}
		GameCore.SetScissorRectangle();
		LineBatch.Begin(BlendState.Additive, Matrix.Identity);
		DrawSelectionRectangle();
		DrawDragLayerLines();
		LineBatch.End();
		if (IsShowSafeLineEnabled)
		{
			DrawSafeLine();
		}
		DrawLoopLine();
		SpriteDrawEnd();
		int scissorsMode = ScissorsMode;
		ScissorsMode = 0;
		GameCore.SetScissorRectangle();
		if (CreatorObject != null)
		{
			bool useDistortionShader = Options.UseDistortionShader;
			GameCore.UseRenderTarget = false;
			GameCore.DrawBegin();
			CreatorObject.Draw(GameCore.SpriteBatch);
			GameCore.UseRenderTarget = useDistortionShader;
			if (!CreatorObject.HasGraphics || CreatorObject.PartData.LightOrShadow != 0)
			{
				SpriteDrawBegin();
				DrawObjectGizmo(CreatorObject);
				SpriteDrawEnd();
			}
			GameCore.DrawEnd();
			ScissorsMode = scissorsMode;
		}
		DebugDraw.ShowScreenFrames();
		if (!IsNoPanelModeEnabled)
		{
			foreach (Panel panel in _panelList)
			{
				panel.Draw();
			}
		}
		_panelToolBar.Draw();
		if (!IsNoPanelModeEnabled)
		{
			_panelLayerBar.Draw();
		}
		_levelScrollBar.Draw();
		if (GameCore.IsApplicationActive)
		{
			_gameObjectPreview.Draw();
		}
		CurrentLevel.CurrentLayer = null;
		SpriteDrawBegin();
		Cursor.Draw();
		SpriteDrawEnd();
		ScissorsMode = scissorsMode;
	}

	public void CreatePreviewObject(PartData partData)
	{
		CurrentLevel.DestroyPreviewList();
		if (partData != null)
		{
			CurrentLevel.CurrentLayer = CurrentLayer;
			_gameObjectPreview.CreateObject(partData);
		}
		else
		{
			_gameObjectPreview.Clear();
		}
	}

	public void SetCurrentObject(GameObject gameObject)
	{
		SetFocusComponent(null);
		if (CurrentSelectionGroup != null && CurrentSelectionGroup.SelectionObject != gameObject && gameObject != null)
		{
			UnselectSelectionGroup();
		}
		if (CurrentObject != null && CurrentObject.MyPath != null)
		{
			CurrentObject.MyPath.CalculateLength();
		}
		CurrentObject = gameObject;
		if (CurrentObject != null && CurrentTool == EditorToolEnum.Path && ToolPath.CurrentPath != CurrentObject.MyPath)
		{
			ToolPath.CurrentPath = CurrentObject.MyPath;
			SetCurretEventNode(null);
		}
		UnselectCurrentPartData();
		ToolTimeLine?.SetCurrentEventNode(null);
		_panelObjectList?.SelectObject();
	}

	public void SetCurretEventNode(BehaviorEventNode node)
	{
		_panelBehaviorEvent.SetCurrentEventNode(CurrentObject, node);
	}

	public void SetSelectedEventNode(BehaviorEventNode node)
	{
		_panelBehaviorEvent.SetSelectedEventNode((node != null) ? node.MyObject : CurrentObject, node);
	}

	public void SetCurrentPartData(PartData partData)
	{
		if (CurrentPartData != partData)
		{
			CurrentLevel.CurrentLayer = CurrentLayer;
			CurrentPartData = partData;
			if (CurrentPartData != null)
			{
				GameObject gameObject = GameObject.CreateByPartDataSimple(CurrentPartData, bCreateRigidBody: false, null, CurrentPartData.Id);
				gameObject.SetPosition(Cursor.GetCurrentLayerPosition());
				gameObject.Init();
				gameObject.LevelInit();
				gameObject.Activate();
				if (gameObject.RigidBody != null)
				{
					gameObject.RigidBody.IsEnabled = false;
				}
				DestroyCurrentPartData();
				CreatorObject = gameObject;
			}
			else
			{
				DestroyCurrentPartData();
			}
		}
		CurrentObject = null;
		_panelPartDataList.SelectPartData();
		CurrentLevel.DestroyPreviewList();
		SetFocusComponent(null);
	}

	private void DestroyCurrentPartData()
	{
		if (CreatorObject != null)
		{
			CreatorObject.Destroy();
			CreatorObject = null;
		}
	}

	private void UnselectCurrentPartData()
	{
		DestroyCurrentPartData();
		CurrentPartData = null;
		if (_panelPartDataList != null)
		{
			_panelPartDataList.SelectPartData();
		}
	}

	private void UnselectSelectionGroup()
	{
		if (CurrentSelectionGroup != null)
		{
			CurrentSelectionGroup.DetachSelectedObjects();
			CurrentSelectionGroup = null;
		}
	}

	public void SetFocusComponent(PanelComponent component)
	{
		if (FocusComponent != null)
		{
			FocusComponent.CancelFocus();
		}
		FocusComponent = component;
	}

	public void UnselectAll(bool unselectCurrentObject = true)
	{
		if (unselectCurrentObject)
		{
			SetCurrentObject(null);
			UnselectCurrentPartData();
			UnselectSelectionGroup();
		}
		_gameObjectPreview.Clear();
		ToolEdge = null;
		ToolPath.CurrentPath = null;
		ToolPolygon.CurrentVertexIndex = -1;
		ToolTimeLine.SetCurrentEventNode(null);
		_panelBehaviorEvent.CurrentEventNode = null;
		SetFocusComponent(null);
	}

	public GameObject CloneObject(GameObject gameObject, Vector2 newPosition)
	{
		if (gameObject == null)
		{
			return null;
		}
		Dictionary<GameObject, GameObject> dictionary = new Dictionary<GameObject, GameObject>();
		CurrentLevel.CurrentLayer = CurrentLayer;
		if (CurrentSelectionGroup != null && gameObject == CurrentSelectionGroup.SelectionObject)
		{
			SelectionGroup newGroup = new SelectionGroup(this);
			Transformation transformation = new Transformation(CurrentSelectionGroup.SelectionObject.WorldTransformation);
			transformation.Position = newPosition;
			newGroup.SelectionObject.SetPhysicsTransformation(transformation);
			foreach (GameObject item in Enumerable.Reverse(CurrentSelectionGroup.SelectionObject.ChildObjectListFront))
			{
				GameObject gameObject2 = GameObject.CloneObject(item, CurrentLayer, item.Position, bMovePath: false);
				if (gameObject2 != null)
				{
					if (gameObject2.TriggerPoint != Vector2.Zero)
					{
						gameObject2.TriggerPoint += newPosition;
					}
					gameObject2.Init();
					gameObject2.LevelInit();
					newGroup.ObjectList.Add(gameObject2);
					dictionary.Add(item, gameObject2);
				}
			}
			newGroup.SelectionObject.SetFrameRectangle(CurrentSelectionGroup.SelectionObject.FrameRectangle.Width, CurrentSelectionGroup.SelectionObject.FrameRectangle.Height, CurrentSelectionGroup.SelectionObject.FramePivot.X, CurrentSelectionGroup.SelectionObject.FramePivot.Y);
			newGroup.ObjectList.ForEach((GameObject go) =>
			{
				newGroup.AddObjectToSelection(go, bWorldPos: false);
				if (go.MyPath != null)
				{
					go.MyPath.Move(go.WorldPosition - go.OldPosition);
				}
			});
			CurrentSelectionGroup.DetachSelectedObjects();
			CurrentSelectionGroup = newGroup;
			CurrentObject = CurrentSelectionGroup.SelectionObject;
		}
		else
		{
			GameObject gameObject3 = GameObject.CloneObject(gameObject, CurrentLayer, newPosition, bMovePath: true);
			if (gameObject3 != null)
			{
				gameObject3.Init();
				gameObject3.LevelInit();
				dictionary.Add(gameObject, gameObject3);
			}
			SetCurrentObject(gameObject3);
		}
		RecreateLinks(dictionary, bRecreateIncomingLinks: false);
		CurrentLevel.CurrentLayer = null;
		RefreshObjectList();
		return gameObject;
	}

	public void RecreateLinks(Dictionary<GameObject, GameObject> objectDictionary, bool bRecreateIncomingLinks)
	{
		foreach (KeyValuePair<GameObject, GameObject> item in objectDictionary)
		{
			GameObject key = item.Key;
			GameObject clone = item.Value;
			if (key.LinkedObjectList != null)
			{
				foreach (GameObject linkedObject in key.LinkedObjectList)
				{
					if (objectDictionary.TryGetValue(linkedObject, out var value))
					{
						clone.AddLinkedObject(value);
					}
					else
					{
						clone.AddLinkedObject(linkedObject);
					}
				}
			}
			if (bRecreateIncomingLinks)
			{
				foreach (Layer layer in key.MyLevel.LayerList)
				{
					foreach (GameObject gameObject2 in layer.GameObjectList)
					{
						if (gameObject2 == key || gameObject2 == clone || gameObject2.LinkedObjectList == null)
						{
							continue;
						}
						for (int i = 0; i < gameObject2.LinkedObjectList.Count; i++)
						{
							if (gameObject2.LinkedObjectList[i] == key)
							{
								gameObject2.LinkedObjectList[i] = clone;
							}
						}
					}
				}
			}
			if (key.RigidBody == null)
			{
				continue;
			}
			key.RigidBody.GetJointList()?.ForEach((Joint joint) =>
			{
				GameObject gameObject = joint.BodyB.UserData as GameObject;
				if (!objectDictionary.TryGetValue(gameObject, out var value2))
				{
					value2 = gameObject;
				}
				if (joint.JointType == JointType.Revolute)
				{
					RevoluteJoint revoluteJoint = joint as RevoluteJoint;
					clone.RigidBody.AddJoint(joint.JointType, value2.RigidBody, revoluteJoint.LocalAnchorA, revoluteJoint.LocalAnchorB);
				}
			});
		}
	}

	public bool IsEditorSetEnabled(int setNumber)
	{
		if (setNumber < 0 || setNumber >= EditorSetEnabledArray.Length)
		{
			return false;
		}
		return EditorSetEnabledArray[setNumber];
	}

	public void DeleteOutOfLevelObjects()
	{
		foreach (Layer layer in CurrentLevel.LayerList)
		{
			foreach (GameObject gameObject in layer.GameObjectList)
			{
				if (gameObject.OutOfLevel != OutOfFrame.None)
				{
					if (gameObject == CurrentObject)
					{
						CurrentObject = null;
					}
					gameObject.Destroy();
				}
			}
		}
	}

	private void WritePathToStream(GameObject go, StreamWriter file)
	{
		if (go.MyPath == null)
		{
			return;
		}
		string text = "";
		bool flag = true;
		foreach (Vector2 controlPoint in go.MyPath.ControlPointList)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				text += ", ";
			}
			Vector2 vector = controlPoint - go.WorldPosition;
			text += $"{(int)vector.X},{(int)vector.Y}";
		}
		file.WriteLine($"$path = {text}");
	}

	public void SavePathToFile(GameObject go, string fileName)
	{
		using StreamWriter streamWriter = new StreamWriter(fileName);
		if (streamWriter == null)
		{
			GameCore.LogErr($"Unable save editor options to file '{fileName}'");
		}
		else
		{
			WritePathToStream(go, streamWriter);
		}
	}

	public void SaveOptionsToFile()
	{
		string editorIniPath = DataUtils.GetEditorIniPath();
		using StreamWriter streamWriter = new StreamWriter(editorIniPath);
		if (streamWriter == null)
		{
			GameCore.LogErr($"Unable save editor options to file '{editorIniPath}'");
			return;
		}
		streamWriter.WriteLine($"symbol_mode={(ShowAllSymbols ? 1 : 0)},{(MinimalGizmoMode ? 1 : 0)}");
		streamWriter.WriteLine($"show_safe_line={(IsShowSafeLineEnabled ? 1 : 0)}");
		streamWriter.WriteLine($"show_phys={(Options.ShowDebugPhysics ? 1 : 0)}");
		streamWriter.WriteLine($"lights_on={(Options.IsLightsOn ? 1 : 0)}");
		streamWriter.WriteLine($"debug_path={(IsShowPathEnabled ? 1 : 0)},{(Options.DebugPath ? 1 : 0)}");
		streamWriter.WriteLine($"store_cam={(IsStoreCameraPositionEnabled ? 1 : 0)}");
		streamWriter.WriteLine($"scissors_ode={ScissorsMode}");
		streamWriter.WriteLine($"layer_bar={(int)_panelLayerBar.Position.X},{(int)_panelLayerBar.Position.Y},{(_panelLayerBar.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"toolbar_panel={(int)_panelToolBar.Position.X},{(int)_panelToolBar.Position.Y},{(_panelToolBar.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"event_panel={(int)_panelBehaviorEvent.Position.X},{(int)_panelBehaviorEvent.Position.Y},{(_panelBehaviorEvent.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"object_panel={(int)_panelGameObject.Position.X},{(int)_panelGameObject.Position.Y},{(_panelGameObject.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"emitter_panel={(int)_panelEmitter.Position.X},{(int)_panelEmitter.Position.Y},{(_panelEmitter.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"emitter_time_panel={(int)_panelEmitterTime.Position.X},{(int)_panelEmitterTime.Position.Y},{(_panelEmitterTime.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"emitter_strip_panel={(int)_panelEmitterStrip.Position.X},{(int)_panelEmitterStrip.Position.Y},{(_panelEmitterStrip.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"object_tree_panel={(int)_panelGameObjectTree.Position.X},{(int)_panelGameObjectTree.Position.Y},{(_panelGameObjectTree.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"atlas_list_panel={(int)_panelAtlasList.Position.X},{(int)_panelAtlasList.Position.Y},{_panelAtlasList.Width},{_panelAtlasList.Height},{(_panelAtlasList.IsMinimized ? 1 : 0)}");
		streamWriter.WriteLine($"object_list_panel={(int)_panelObjectList.Position.X},{(int)_panelObjectList.Position.Y},{_panelObjectList.Width},{_panelObjectList.Height},{(_panelObjectList.IsMinimized ? 1 : 0)},{(_panelObjectList.IsEnabled ? 1 : 0)}");
		streamWriter.WriteLine($"part_list_panel={(int)_panelPartDataList.Position.X},{(int)_panelPartDataList.Position.Y},{_panelPartDataList.Width},{_panelPartDataList.Height},{(_panelPartDataList.IsMinimized ? 1 : 0)},{(_panelPartDataList.IsEnabled ? 1 : 0)}");
		streamWriter.WriteLine($"layer_panel={(int)_panelLayer.Position.X},{(int)_panelLayer.Position.Y},{(_panelLayer.IsMinimized ? 1 : 0)}");
	}

	public void LoadOptionsFromFile()
	{
		string editorIniPath = DataUtils.GetEditorIniPath();
		if (!File.Exists(editorIniPath))
		{
			return;
		}
		IniFile iniFile = new IniFile();
		if (iniFile.OpenFromFile(editorIniPath))
		{
			while (iniFile.NextLine())
			{
				if (iniFile.KeyEquals("symbol_mode") && iniFile.TryGetIntArray(2, out var resultArray))
				{
					ShowAllSymbols = resultArray[0] != 0;
					MinimalGizmoMode = resultArray[1] != 0;
				}
				else if (iniFile.KeyEquals("show_safe_line"))
				{
					IsShowSafeLineEnabled = iniFile.GetBoolValue();
				}
				else if (iniFile.KeyEquals("show_phys"))
				{
					Options.ShowDebugPhysics = iniFile.GetBoolValue();
				}
				else if (iniFile.KeyEquals("debug_path") && iniFile.TryGetIntArray(2, out resultArray))
				{
					IsShowPathEnabled = resultArray[0] != 0;
					Options.DebugPath = resultArray[1] != 0;
				}
				else if (iniFile.KeyEquals("lights_on"))
				{
					Options.IsLightsOn = iniFile.GetBoolValue();
				}
				else if (iniFile.KeyEquals("store_cam"))
				{
					IsStoreCameraPositionEnabled = iniFile.GetBoolValue();
				}
				else if (iniFile.KeyEquals("scissors_mode"))
				{
					ScissorsMode = iniFile.GetIntValue();
				}
				else if (iniFile.KeyEquals("toolbar_panel") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelToolBar.Position = Utils.GetVector2(resultArray);
					_panelToolBar.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("layer_bar") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelLayerBar.Position = Utils.GetVector2(resultArray);
					_panelLayerBar.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("event_panel") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelBehaviorEvent.Position = Utils.GetVector2(resultArray);
					_panelBehaviorEvent.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("object_panel") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelGameObject.Position = Utils.GetVector2(resultArray);
					_panelGameObject.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("emitter_panel") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelEmitter.Position = Utils.GetVector2(resultArray);
					_panelEmitter.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("emitter_time_panel") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelEmitterTime.Position = Utils.GetVector2(resultArray);
					_panelEmitterTime.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("emitter_strip_panel") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelEmitterStrip.Position = Utils.GetVector2(resultArray);
					_panelEmitterStrip.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("layer_panel") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelLayer.Position = Utils.GetVector2(resultArray);
					_panelLayer.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("object_tree_panel") && iniFile.TryGetIntArray(3, out resultArray))
				{
					_panelGameObjectTree.Position = Utils.GetVector2(resultArray);
					_panelGameObjectTree.IsMinimized = resultArray[2] != 0;
				}
				else if (iniFile.KeyEquals("atlas_list_panel") && iniFile.TryGetIntArray(5, out resultArray))
				{
					_panelAtlasList.Position = Utils.GetVector2(resultArray);
					_panelAtlasList.Width = resultArray[2];
					_panelAtlasList.Height = resultArray[3];
					_panelAtlasList.IsMinimized = resultArray[4] != 0;
				}
				else if (iniFile.KeyEquals("object_list_panel") && iniFile.TryGetIntArray(6, out resultArray))
				{
					_panelObjectList.Position = new Vector2(resultArray[0], resultArray[1]);
					_panelObjectList.Width = resultArray[2];
					_panelObjectList.Height = resultArray[3];
					_panelObjectList.IsMinimized = resultArray[4] != 0;
					_panelObjectList.IsEnabled = resultArray[5] != 0;
				}
				else if (iniFile.KeyEquals("part_list_panel") && iniFile.TryGetIntArray(6, out resultArray))
				{
					_panelPartDataList.Position = new Vector2(resultArray[0], resultArray[1]);
					_panelPartDataList.Width = resultArray[2];
					_panelPartDataList.Height = resultArray[3];
					_panelPartDataList.IsMinimized = resultArray[4] != 0;
					_panelPartDataList.IsEnabled = resultArray[5] != 0;
				}
			}
		}
		iniFile.Close();
	}
}
