using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D.Editor;

public class GameObjectPreview
{
	public GameObject MyObject;

	public Vector2 Position;

	public int Width;

	public int Height;

	public Layer MyLayer;

	private Editor _editor;

	private GameLevel _level;

	public GameObjectPreview(Editor editor)
	{
		_editor = editor;
		Width = 100;
		Height = 100;
		_level = new GameLevel();
		_level.Init();
		MyLayer = _level.CreateLayer("Preview");
		MyLayer.Init();
		_level.MainLayer = MyLayer;
		_level.AfterLoad(resetCamera: true, mainLevel: false);
	}

	public void Update(float elapsedTime)
	{
		CorrectObjectPosition();
		_level.Update(elapsedTime);
		_level.Camera.StopQuake();
		GameCore.CurrentLevel = GameCore.GameLevel;
	}

	private void CorrectObjectPosition()
	{
		if (MyObject != null)
		{
			_editor.UpdateObjectPreviewPosition();
			if (!MyObject.HasGraphics || MyObject.PartData.LightOrShadow != 0)
			{
				MyObject.SetPosition(MyObject.MyLayer.MyLevel.Camera.ScreenPosToCurrentLayer(new Vector2(Position.X + (float)(Width / 2), Position.Y + (float)(Height / 2))));
				return;
			}
			Vector2 vector = new Vector2(MyObject.VisibilityRect.X + MyObject.VisibilityRect.Width / 2, MyObject.VisibilityRect.Y + MyObject.VisibilityRect.Height / 2);
			Vector2 position = MyObject.MyLayer.MyLevel.Camera.ScreenPosToCurrentLayer(new Vector2(Position.X + (float)(Width / 2), Position.Y + (float)(Height / 2)) - vector);
			MyObject.SetPosition(position);
		}
	}

	public void Draw()
	{
		if (MyObject != null)
		{
			_editor.TriangleBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
			_editor.TriangleBatch.DrawRect(Position.X, Position.Y, Width, Height, new Color(16, 16, 32, 150));
			_editor.TriangleBatch.End();
		}
		_level.Camera.SetViewPort(GameCore.MainViewPort);
		_level.Draw(showRenderTarget: false);
		GameCore.CurrentLevel = GameCore.GameLevel;
		if (MyObject != null)
		{
			_editor.LineBatch.Begin(BlendState.AlphaBlend, Matrix.Identity);
			_editor.LineBatch.DrawRect(Position.X, Position.Y, Position.X + (float)Width - 1f, Position.Y + (float)Height - 1f, new Color(255, 200, 50, 200));
			_editor.LineBatch.End();
			if (!MyObject.HasGraphics || MyObject.PartData.LightOrShadow != 0)
			{
				_editor.SpriteDrawBegin();
				_editor.DrawObjectGizmo(MyObject);
				_editor.SpriteDrawEnd();
			}
			_editor.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			string arg = ((MyObject.PartData.MyContent == GameCore.GlobalContent) ? "G" : "L");
			Editor.Font.Write(_editor.SpriteBatch, $"[{arg}] {MyObject.PartData.FileName}", Position.X + 4f, Position.Y + (float)Height + 2f, Color.White);
			_editor.SpriteBatch.End();
		}
	}

	public GameObject CreateObject(PartData partData)
	{
		Clear();
		MyObject = MyLayer.CreateObjectByPartData(partData, Vector2.Zero, Vector2.One, 0f, 0, bCreateRigidBody: false);
		Width = MyObject.VisibilityRect.Width + 4;
		Height = MyObject.VisibilityRect.Height + 4;
		if (Width < 200)
		{
			Width = 200;
		}
		if (Height < 200)
		{
			Height = 200;
		}
		CorrectObjectPosition();
		MyObject.Init();
		MyObject.LevelInit();
		MyObject.ActiveMode = ActiveModeEnum.AlwaysActive;
		return MyObject;
	}

	public void Clear()
	{
		MyLayer.MyLevel.WarningTextList.Clear();
		MyLayer.DestroyAll();
		MyLayer.Clear();
		MyObject = null;
	}
}
