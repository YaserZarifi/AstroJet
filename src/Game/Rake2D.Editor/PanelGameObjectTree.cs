using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Rake2D.Editor;

public class PanelGameObjectTree : Panel
{
	private GameObject _mainObject;

	private List<PartLine> _partLineList;

	private Button _buttonSave;

	private Button _buttonLoad;

	public PanelGameObjectTree(Editor editor, int x, int y, int width, int height)
		: base(editor, x, y, width, height)
	{
		_partLineList = new List<PartLine>();
		_buttonLoad = new Button(this);
		_buttonLoad.Caption = "Reload";
		_buttonLoad.Height = 24;
		_buttonLoad.FrameColor = new Color(200, 150, 210, 200);
		_buttonLoad.Tooltip = "Reload whole object";
		_buttonLoad.OnClick += OnLoadButtonClick;
		_buttonSave = new Button(this);
		_buttonSave.Caption = "Save";
		_buttonSave.Height = 24;
		_buttonSave.FrameColor = new Color(200, 150, 210, 200);
		_buttonSave.Tooltip = "Save all emitters";
		_buttonSave.OnClick += OnSaveButtonClick;
		SetMainObject(null);
	}

	public void SetMainObject(GameObject go)
	{
		foreach (PartLine partLine in _partLineList)
		{
			partLine.Destroy();
		}
		_partLineList.Clear();
		Width = 230;
		Height = 50;
		_buttonLoad.IsVisible = false;
		_buttonSave.IsVisible = false;
		if (go != null)
		{
			_mainObject = go;
			TitleText = go.Id;
			AddPart(go, 0);
		}
		else
		{
			TitleText = "- no parts -";
			_mainObject = null;
		}
		foreach (PartLine partLine2 in _partLineList)
		{
			partLine2.NameButton.Width = (int)((float)Width - partLine2.NameButton.Position.X - 30f);
			partLine2.EnabledCheckbox.Position = partLine2.NameButton.Position + new Vector2(partLine2.NameButton.Width + 2, 2f);
		}
		_buttonLoad.Position.X = Width / 2 - _buttonSave.Width - 2;
		_buttonLoad.Position.Y = Height - 30;
		_buttonSave.Position.X = Width / 2 + 2;
		_buttonSave.Position.Y = _buttonLoad.Position.Y;
	}

	private void SaveEmitterData(PartData partData)
	{
		if (partData == null)
		{
			return;
		}
		if (partData.EmitterData != null)
		{
			partData.EmitterData.Save();
		}
		foreach (PartData child in partData.ChildList)
		{
			SaveEmitterData(child);
		}
	}

	private void OnLoadButtonClick(Button btn)
	{
		Editor.ReloadCurrentObject();
	}

	private void OnSaveButtonClick(Button btn)
	{
		if (_mainObject != null && _mainObject.PartData != null)
		{
			SaveEmitterData(_mainObject.PartData);
		}
	}

	private void OnPartButtonClick(Button btn)
	{
		Editor.SetCurrentObject(btn.UserData as GameObject);
	}

	private void OnPartEnabledCheckboxClick(Checkbox checkbox)
	{
		(checkbox.UserData as GameObject).IsEnabled = checkbox.IsChecked;
	}

	private void AddPart(GameObject go, int level)
	{
		if (go.IsDestroyed)
		{
			return;
		}
		Width = Math.Max(Width, 200 + level * 10);
		Height = _partLineList.Count * 23 + 90;
		_buttonLoad.IsVisible = true;
		_buttonSave.IsVisible = true;
		if (Position.Y < 0f)
		{
			Position.Y = 0f;
		}
		PartLine partLine = new PartLine();
		Button button = new Button(this);
		button.UserData = go;
		button.Caption = go.Id;
		button.ForegroundColor = new Color(80, 200, 255);
		button.FrameColor = new Color(150, 110, 160);
		button.CaptionAlign = TextAlign.Left;
		button.Height = 22;
		button.OnClick += OnPartButtonClick;
		button.Position = new Vector2(10 + level * 10, 30 + _partLineList.Count * 23);
		partLine.NameButton = button;
		Checkbox checkbox = new Checkbox(this);
		checkbox.UserData = go;
		checkbox.Width = (checkbox.Height = button.Height - 4);
		checkbox.FrameColor = new Color(150, 110, 160);
		checkbox.ForegroundColor = new Color(170, 50, 255);
		checkbox.Tooltip = "Enable / disable part";
		checkbox.OnClick += OnPartEnabledCheckboxClick;
		partLine.EnabledCheckbox = checkbox;
		_partLineList.Add(partLine);
		if (go.ChildObjectListBack != null)
		{
			foreach (GameObject item in go.ChildObjectListBack)
			{
				AddPart(item, level + 1);
			}
		}
		if (go.ChildObjectListFront == null)
		{
			return;
		}
		foreach (GameObject item2 in go.ChildObjectListFront)
		{
			AddPart(item2, level + 1);
		}
	}

	public override void Update(float elapsedTime)
	{
		if (!IsEnabled || IsOff)
		{
			return;
		}
		base.Update(elapsedTime);
		if (IsMinimized)
		{
			return;
		}
		if (Editor.CurrentObject == null && _mainObject != null)
		{
			SetMainObject(null);
		}
		else if (Editor.CurrentObject != null && (_mainObject == null || Editor.CurrentObject.GetMainPart() != _mainObject))
		{
			SetMainObject(Editor.CurrentObject.GetMainPart());
		}
		bool flag = false;
		foreach (PartLine partLine in _partLineList)
		{
			Button nameButton = partLine.NameButton;
			if (nameButton.UserData as GameObject == Editor.CurrentObject)
			{
				nameButton.ValueColor = new Color(255, 200, 255);
			}
			else
			{
				nameButton.ValueColor = new Color(255, 100, 255);
			}
			GameObject gameObject = nameButton.UserData as GameObject;
			nameButton.Caption = gameObject.Id;
			if (gameObject.Emitter != null)
			{
				if (gameObject.Emitter.Data.IsStripLine)
				{
					nameButton.Caption += " [s]";
				}
				else
				{
					nameButton.Caption += " [e]";
				}
			}
			if (gameObject.PartData != null)
			{
				if (gameObject.PartData.LightOrShadow > 0)
				{
					nameButton.Caption += " (l)";
				}
				else if (gameObject.PartData.LightOrShadow < 0)
				{
					nameButton.Caption += " (s)";
				}
			}
			if (nameButton.IsSelected)
			{
				GameObject gameObject2 = nameButton.UserData as GameObject;
				if (Editor.KeyPressed(Keys.Insert))
				{
					GameObject gameObject3 = GameObject.CreateByPartDataSimple(new PartData("new_part"));
					gameObject3.SetLayer(gameObject2.MyLayer);
					gameObject2.AddChild(gameObject3);
					flag = true;
					break;
				}
				if (Editor.KeyPressed(Keys.Delete))
				{
					gameObject2.HardDestroy();
					flag = true;
					break;
				}
				if (Editor.KeyPressed(Keys.E))
				{
					_ = gameObject2.Emitter;
				}
			}
			partLine.EnabledCheckbox.IsChecked = gameObject.IsEnabled;
		}
		if (flag)
		{
			SetMainObject(_mainObject);
		}
	}

	public override void Draw()
	{
		base.Draw();
	}
}
