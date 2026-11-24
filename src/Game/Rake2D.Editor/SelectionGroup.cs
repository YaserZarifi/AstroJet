using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Editor;

public class SelectionGroup
{
	public Vector2 TopLeft;

	public Vector2 BottomRight;

	public List<GameObject> ObjectList;

	public GameObject SelectionObject;

	private Editor _editor;

	public SelectionGroup(Editor editor)
	{
		ObjectList = new List<GameObject>();
		_editor = editor;
		SelectionObject = new GameObject();
		SelectionObject.SetLayer(GameCore.CurrentLevel.CurrentLayer);
		SelectionObject.ActiveMode = ActiveModeEnum.AlwaysActive;
	}

	public void DetachObject(GameObject go)
	{
		if (SelectionObject.ChildObjectListFront != null && SelectionObject.ChildObjectListFront.Contains(go))
		{
			go.SetPhysicsTransformation(go.WorldTransformation);
			if (go.RigidBody != null)
			{
				go.RigidBody.WakeUp(resetDynamics: true);
			}
			SelectionObject.RemoveChild(go);
		}
	}

	public void DetachSelectedObjects()
	{
		SelectionObject.DetachChildrenInEditor();
	}

	public void UpdateCenter()
	{
		if (ObjectList == null || ObjectList.Count <= 0)
		{
			return;
		}
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
		foreach (GameObject @object in ObjectList)
		{
			@object.ResetPrecalculations();
			Vector2[] frameVerices = @object.GetFrameVerices(bWorld: true);
			for (int i = 0; i < frameVerices.Length; i++)
			{
				Vector2 vector3 = frameVerices[i];
				vector.X = Math.Min(vector.X, vector3.X);
				vector.Y = Math.Min(vector.Y, vector3.Y);
				vector2.X = Math.Max(vector2.X, vector3.X);
				vector2.Y = Math.Max(vector2.Y, vector3.Y);
			}
		}
		Vector2 position = (vector + vector2) / 2f;
		SelectionObject.Position = position;
		float num = vector2.X - vector.X;
		float num2 = vector2.Y - vector.Y;
		SelectionObject.SetFrameRectangle((int)num, (int)num2, 0f, 0f);
	}

	public void AddObjectToSelection(GameObject go, bool bWorldPos)
	{
		if (go.RigidBody != null)
		{
			go.RigidBody.ResetDynamics();
			go.RigidBody.Sleep();
		}
		if (bWorldPos)
		{
			SelectionObject.ResetPrecalculations();
			go.Position = SelectionObject.GetRelativePosition(go.WorldPosition);
		}
		SelectionObject.AddChild(go);
	}

	public void SelectObjects()
	{
		Vector2 topLeft = TopLeft;
		Vector2 bottomRight = BottomRight;
		if (topLeft.X > bottomRight.X)
		{
			topLeft.X = BottomRight.X;
			bottomRight.X = TopLeft.X;
		}
		if (topLeft.Y > bottomRight.Y)
		{
			topLeft.Y = BottomRight.Y;
			bottomRight.Y = TopLeft.Y;
		}
		List<GameObject> resultList = new List<GameObject>();
		_editor.CurrentLayer.GetGameObjectsInRect(topLeft, bottomRight, ref resultList);
		if (_editor.CurrentObject != null && !_editor.IsGroupSelected && resultList.Count > 0)
		{
			resultList.Add(_editor.CurrentObject);
		}
		if (resultList.Count <= 0)
		{
			return;
		}
		resultList.RemoveAll((GameObject go) => !GameCore.Editor.IsEditorSetEnabled(go.PartData.EditorSet));
		for (int num = resultList.Count - 1; num >= 0; num--)
		{
			GameObject gameObject = resultList[num];
			if (!GameCore.IsEditorEnabled && ((gameObject.PartData != null && !GameCore.Editor.IsEditorSetEnabled(gameObject.PartData.EditorSet)) || (gameObject.PartData == null && !GameCore.Editor.IsEditorSetEnabled(0))))
			{
				resultList.RemoveAt(num);
			}
			else
			{
				ObjectList.Add(gameObject);
			}
		}
		DetachSelectedObjects();
		SelectionObject.Rotation = 0f;
		SelectionObject.SetScale(1f);
		UpdateCenter();
		AddObjectListToSelection();
	}

	public void AddObjectListToSelection()
	{
		ObjectList.ForEach((GameObject go) =>
		{
			AddObjectToSelection(go, bWorldPos: true);
		});
		CorrectBoundingBox();
	}

	public void CorrectBoundingBox()
	{
		if (ObjectList.Count != 0)
		{
			GameObject gameObject = ObjectList[0];
			float rotation = gameObject.Rotation;
			Vector2 worldPosition = gameObject.WorldPosition;
			SelectionObject.Rotation = 0f - gameObject.Rotation;
			DetachSelectedObjects();
			UpdateCenter();
			SelectionObject.Rotation = 0f;
			SelectionObject.Scale = Vector2.One;
			SelectionObject.ResetPrecalculations();
			gameObject.ResetPrecalculations();
			ObjectList.ForEach((GameObject go) =>
			{
				AddObjectToSelection(go, bWorldPos: true);
			});
			UpdateCenter();
			SelectionObject.Rotation = rotation;
			gameObject.ResetPrecalculations();
			SelectionObject.Position -= gameObject.WorldPosition - worldPosition;
		}
	}
}
