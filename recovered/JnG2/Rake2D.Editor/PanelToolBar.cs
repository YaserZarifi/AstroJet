using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class PanelToolBar : Panel
{
	private Button _buttonToolNormal;

	private Button _buttonToolPolygon;

	private Button _buttonToolPath;

	private Button _buttonToolEmitter;

	private Button _buttonPhysic;

	private Button _buttonColisoids;

	private Button _buttonLights;

	private Button _buttonLightMaps;

	private Button _buttonSymbolMode;

	private Button _buttonMinimalGizmo;

	private Button _buttonBulletTime;

	private Button _buttonSafeLine;

	private Button _buttonStoreCameraPosition;

	private SpriteAtlas _atlasButtonIcon;

	public PanelToolBar(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
	}

	public override void Init()
	{
		base.Init();
		Vector2 position = new Vector2(2f, 2 + TitleBarHeight);
		int num = 32;
		int num2 = 32;
		Vector2 vector = new Vector2(33f, 0f);
		Color backgroundColor = new Color(100, 100, 110);
		_atlasButtonIcon = GameCore.GlobalContent.GetSpriteAtlas("editor_button_symbol");
		_buttonToolNormal = new Button(this);
		_buttonToolNormal.Position = position;
		_buttonToolNormal.Width = num;
		_buttonToolNormal.Height = num2;
		_buttonToolNormal.BackgroundColor = backgroundColor;
		_buttonToolNormal.IconAtlas = _atlasButtonIcon;
		_buttonToolNormal.ShortcutKey = Keys.F1;
		_buttonToolNormal.ShortCutControl = false;
		_buttonToolNormal.Tooltip = $"Tool normal [{_buttonToolNormal.ShortcutKey}]";
		_buttonToolNormal.OnClick += OnButonToolNormalClick;
		position += vector;
		_buttonToolPolygon = new Button(this);
		_buttonToolPolygon.Position = position;
		_buttonToolPolygon.Width = num;
		_buttonToolPolygon.Height = num2;
		_buttonToolPolygon.BackgroundColor = backgroundColor;
		_buttonToolPolygon.IconAtlas = _atlasButtonIcon;
		_buttonToolPolygon.ShortcutKey = Keys.F2;
		_buttonToolPolygon.ShortCutControl = false;
		_buttonToolPolygon.Tooltip = $"Tool polygon [{_buttonToolPolygon.ShortcutKey}]";
		_buttonToolPolygon.OnClick += OnButonToolPolygonClick;
		position += vector;
		_buttonToolPath = new Button(this);
		_buttonToolPath.Position = position;
		_buttonToolPath.Width = num;
		_buttonToolPath.Height = num2;
		_buttonToolPath.BackgroundColor = backgroundColor;
		_buttonToolPath.IconAtlas = _atlasButtonIcon;
		_buttonToolPath.ShortcutKey = Keys.F3;
		_buttonToolPath.ShortCutControl = false;
		_buttonToolPath.Tooltip = $"Tool path[{_buttonToolPath.ShortcutKey}]";
		_buttonToolPath.OnClick += OnButonToolPathClick;
		position += vector;
		_buttonToolEmitter = new Button(this);
		_buttonToolEmitter.Position = position;
		_buttonToolEmitter.Width = num;
		_buttonToolEmitter.Height = num2;
		_buttonToolEmitter.BackgroundColor = backgroundColor;
		_buttonToolEmitter.IconAtlas = _atlasButtonIcon;
		_buttonToolEmitter.ShortcutKey = Keys.F4;
		_buttonToolEmitter.ShortCutControl = false;
		_buttonToolEmitter.Tooltip = $"Tool emitter [{_buttonToolEmitter.ShortcutKey}]";
		_buttonToolEmitter.OnClick += OnButonToolEmitterClick;
		position += vector;
		position.X += 3f;
		_buttonLights = new Button(this);
		_buttonLights.Position = position;
		_buttonLights.Width = num;
		_buttonLights.Height = num2;
		_buttonLights.BackgroundColor = backgroundColor;
		_buttonLights.IconAtlas = _atlasButtonIcon;
		_buttonLights.ShortcutKey = Keys.F5;
		_buttonLights.ShortCutControl = false;
		_buttonLights.Tooltip = $"Lights on / off [{_buttonLights.ShortcutKey}]";
		_buttonLights.OnClick += OnButonLightsClick;
		position += vector;
		_buttonLightMaps = new Button(this);
		_buttonLightMaps.Position = position;
		_buttonLightMaps.Width = num;
		_buttonLightMaps.Height = num2;
		_buttonLightMaps.BackgroundColor = backgroundColor;
		_buttonLightMaps.IconAtlas = _atlasButtonIcon;
		_buttonLightMaps.ShortcutKey = Keys.F5;
		_buttonLightMaps.ShortCutControl = true;
		_buttonLightMaps.Tooltip = $"Show light maps [Ctrl + {_buttonLightMaps.ShortcutKey}]";
		_buttonLightMaps.OnClick += OnButonLightMapsClick;
		position += vector;
		_buttonPhysic = new Button(this);
		_buttonPhysic.Position = position;
		_buttonPhysic.Width = num;
		_buttonPhysic.Height = num2;
		_buttonPhysic.BackgroundColor = backgroundColor;
		_buttonPhysic.IconAtlas = _atlasButtonIcon;
		_buttonPhysic.ShortcutKey = Keys.F6;
		_buttonPhysic.ShortCutControl = false;
		_buttonPhysic.Tooltip = $"Physics on / off [{_buttonPhysic.ShortcutKey}]";
		_buttonPhysic.OnClick += OnButonPhysicsClick;
		position += vector;
		_buttonColisoids = new Button(this);
		_buttonColisoids.Position = position;
		_buttonColisoids.Width = num;
		_buttonColisoids.Height = num2;
		_buttonColisoids.BackgroundColor = backgroundColor;
		_buttonColisoids.IconAtlas = _atlasButtonIcon;
		_buttonColisoids.ShortcutKey = Keys.F6;
		_buttonColisoids.ShortCutControl = true;
		_buttonColisoids.Tooltip = $"Show colisoids on / off [Ctrl + {_buttonColisoids.ShortcutKey}]";
		_buttonColisoids.OnClick += OnButonColisoidsClick;
		position += vector;
		_buttonSymbolMode = new Button(this);
		_buttonSymbolMode.Position = position;
		_buttonSymbolMode.Width = num;
		_buttonSymbolMode.Height = num2;
		_buttonSymbolMode.BackgroundColor = backgroundColor;
		_buttonSymbolMode.IconAtlas = _atlasButtonIcon;
		_buttonSymbolMode.ShortcutKey = Keys.G;
		_buttonSymbolMode.ShortCutControl = false;
		_buttonSymbolMode.Tooltip = $"Symbol mode [{_buttonSymbolMode.ShortcutKey}]";
		_buttonSymbolMode.OnClick += OnButonSymbolModeClick;
		position += vector;
		_buttonMinimalGizmo = new Button(this);
		_buttonMinimalGizmo.Position = position;
		_buttonMinimalGizmo.Width = num;
		_buttonMinimalGizmo.Height = num2;
		_buttonMinimalGizmo.BackgroundColor = backgroundColor;
		_buttonMinimalGizmo.IconAtlas = _atlasButtonIcon;
		_buttonMinimalGizmo.ShortcutKey = Keys.G;
		_buttonMinimalGizmo.ShortCutControl = true;
		_buttonMinimalGizmo.Tooltip = $"Editor stuff on / off [Ctrl + {_buttonMinimalGizmo.ShortcutKey}]";
		_buttonMinimalGizmo.OnClick += OnButonMnimalGizmoClick;
		position += vector;
		_buttonBulletTime = new Button(this);
		_buttonBulletTime.Position = position;
		_buttonBulletTime.Width = num;
		_buttonBulletTime.Height = num2;
		_buttonBulletTime.BackgroundColor = backgroundColor;
		_buttonBulletTime.IconAtlas = _atlasButtonIcon;
		_buttonBulletTime.Tooltip = $"Bullet time on / off [{_buttonBulletTime.ShortcutKey}]";
		_buttonBulletTime.OnClick += OnButtonBulletTimeLineClick;
		position += vector;
		_buttonSafeLine = new Button(this);
		_buttonSafeLine.Position = position;
		_buttonSafeLine.Width = num;
		_buttonSafeLine.Height = num2;
		_buttonSafeLine.BackgroundColor = backgroundColor;
		_buttonSafeLine.IconAtlas = _atlasButtonIcon;
		_buttonSafeLine.ShortcutKey = Keys.OemSemicolon;
		_buttonSafeLine.ShortCutControl = true;
		_buttonSafeLine.Tooltip = $"Show safe line on / off [Ctrl + {_buttonSafeLine.ShortcutKey}]";
		_buttonSafeLine.OnClick += OnButtonSafeLineClick;
		position += vector;
		_buttonStoreCameraPosition = new Button(this);
		_buttonStoreCameraPosition.Position = position;
		_buttonStoreCameraPosition.Width = num;
		_buttonStoreCameraPosition.Height = num2;
		_buttonStoreCameraPosition.BackgroundColor = backgroundColor;
		_buttonStoreCameraPosition.IconAtlas = _atlasButtonIcon;
		_buttonStoreCameraPosition.Tooltip = "Store camera position";
		_buttonStoreCameraPosition.OnClick += OnButtonStoreCameraPositionClick;
		Width = (int)(position.X + (float)num + 12f);
		Height = (int)(position.Y + (float)num2 + 3f);
	}

	private void OnButonToolNormalClick(Button button)
	{
		Editor.SetTool(EditorToolEnum.Object);
	}

	private void OnButonToolPolygonClick(Button button)
	{
		Editor.SetTool(EditorToolEnum.Polygon);
	}

	private void OnButonToolPathClick(Button button)
	{
		if (Editor.CurrentTool == EditorToolEnum.Path)
		{
			Editor.ToolPath.ToggleMode();
		}
		Editor.SetTool(EditorToolEnum.Path);
	}

	private void OnButonToolEmitterClick(Button button)
	{
		Editor.SetTool(EditorToolEnum.Emiter);
	}

	private void OnButonLightsClick(Button button)
	{
		Options.IsLightsOn = !Options.IsLightsOn;
	}

	private void OnButonPhysicsClick(Button button)
	{
		Editor.CurrentLevel.PhysicsPaused = !Editor.CurrentLevel.PhysicsPaused;
	}

	private void OnButonColisoidsClick(Button button)
	{
		Options.ShowDebugPhysics = !Options.ShowDebugPhysics;
	}

	private void OnButonSymbolModeClick(Button button)
	{
		Editor.MinimalGizmoMode = false;
		Editor.ShowAllSymbols = !Editor.ShowAllSymbols;
	}

	private void OnButonMnimalGizmoClick(Button button)
	{
		Editor.MinimalGizmoMode = !Editor.MinimalGizmoMode;
	}

	private void OnButtonSafeLineClick(Button button)
	{
		Editor.IsShowSafeLineEnabled = !Editor.IsShowSafeLineEnabled;
	}

	private void OnButtonStoreCameraPositionClick(Button button)
	{
		Editor.IsStoreCameraPositionEnabled = !Editor.IsStoreCameraPositionEnabled;
	}

	private void OnButtonBulletTimeLineClick(Button button)
	{
		GameCore.ChangeTimeFactor(GameInput.ShiftDown ? 2f : 0.25f);
	}

	private void OnButonLightMapsClick(Button button)
	{
		Options.ShowLightmaps = !Options.ShowLightmaps;
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (!IsMinimized)
		{
			_buttonToolNormal.IconFrame = ((Editor.CurrentTool == EditorToolEnum.Object) ? 17 : 18);
			_buttonToolPolygon.IconFrame = ((Editor.CurrentTool == EditorToolEnum.Polygon) ? 19 : 20);
			ref int iconFrame = ref _buttonToolPath.IconFrame;
			int num;
			if (Editor.CurrentTool != EditorToolEnum.Path)
			{
				num = 23;
			}
			else
			{
				num = ((Editor.ToolPath.Mode == ToolPathModeEnum.ControlPoints) ? 22 : 21);
			}
			iconFrame = num;
			_buttonToolEmitter.IconFrame = ((Editor.CurrentTool == EditorToolEnum.Emiter) ? 24 : 25);
			_buttonPhysic.IconFrame = (Editor.CurrentLevel.PhysicsPaused ? 1 : 0);
			_buttonColisoids.IconFrame = (Options.ShowDebugPhysics ? 2 : 3);
			_buttonLights.IconFrame = (Options.IsLightsOn ? 4 : 5);
			_buttonLightMaps.IconFrame = (Options.ShowLightmaps ? 11 : 12);
			_buttonSymbolMode.IconFrame = (Editor.ShowAllSymbols ? 13 : 14);
			_buttonMinimalGizmo.IconFrame = (Editor.MinimalGizmoMode ? 7 : 6);
			_buttonSafeLine.IconFrame = (Editor.IsShowSafeLineEnabled ? 15 : 16);
			ref int iconFrame2 = ref _buttonBulletTime.IconFrame;
			int num2;
			if (GameCore.TargetTimeFactor < 1f)
			{
				num2 = 9;
			}
			else
			{
				num2 = ((GameCore.TargetTimeFactor == 1f) ? 8 : 10);
			}
			iconFrame2 = num2;
			_buttonSafeLine.IconFrame = (Editor.IsShowSafeLineEnabled ? 15 : 16);
			_buttonStoreCameraPosition.IconFrame = (Editor.IsStoreCameraPositionEnabled ? 26 : 27);
		}
	}
}
