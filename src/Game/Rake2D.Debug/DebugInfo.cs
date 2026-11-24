using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rake2D.JnG;

namespace Rake2D.Debug;

public static class DebugInfo
{
	public static float CurrFPS;

	public static float AverageFPS;

	public static float FrameCount;

	public static float FrameTime;

	public static int ObjectParticleCountFront;

	public static int ObjectParticleCountBack;

	public static int LayerParticleCountFront;

	public static int LayerParticleCountBack;

	public static int ObjectStripLineCountFront;

	public static int ObjectStripLineCountBack;

	public static int LayerStripLineCountFront;

	public static int LayerStripLineCountBack;

	public static int[] LightCount = new int[3];

	public static int[] ShadowCount = new int[3];

	public static int DistortionParticles;

	public static int StripLineParticles;

	public static List<DebugInfoLayer> Layers = new List<DebugInfoLayer>();

	private static InstanceBuffer<DebugInfoLayer> LayerCache = new InstanceBuffer<DebugInfoLayer>(64);

	private static Stopwatch _frameStopwatch;

	private static BitmapFont _font;

	private static GameCore _gameCore;

	public static int ObjectParticlesTotal => ObjectParticleCountFront + ObjectParticleCountBack;

	public static int LayerParticlesTotal => LayerParticleCountFront + LayerParticleCountBack;

	public static int ObjectStripLinesTotal => ObjectStripLineCountFront + ObjectStripLineCountBack;

	public static int LayerStripLinesTotal => LayerStripLineCountFront + LayerStripLineCountBack;

	public static DebugInfoLayer CurrentLayer => AddLayer("unknown");

	public static void Init(GameCore gameCore, BitmapFont font)
	{
		_gameCore = gameCore;
		_font = font;
	}

	public static void BeginFrame()
	{
		_frameStopwatch = Stopwatch.StartNew();
		FrameCount++;
	}

	public static void EndFrame()
	{
		FrameTime += _frameStopwatch.ElapsedMilliseconds;
		if (FrameTime >= 1000f)
		{
			FrameTime -= 1000f;
			AverageFPS = FrameCount;
			FrameCount = 0f;
		}
	}

	public static void ClearFrameInfo()
	{
		ObjectParticleCountFront = 0;
		ObjectParticleCountBack = 0;
		LayerParticleCountFront = 0;
		LayerParticleCountBack = 0;
		ObjectStripLineCountFront = 0;
		ObjectStripLineCountBack = 0;
		LayerStripLineCountFront = 0;
		LayerStripLineCountBack = 0;
		StripLineParticles = 0;
	}

	public static DebugInfoLayer AddLayer(string name)
	{
		if (Layers.Count == 0)
		{
			DebugInfoLayer debugInfoLayer = new DebugInfoLayer();
			debugInfoLayer.Name = name;
			Layers.Add(debugInfoLayer);
		}
		return Layers[0];
	}

	private static void DrawFps(GameTime gameTime, int posX, int posY)
	{
		Color color;
		if (_gameCore.FpsCounter.AverageFramesPerSecond < 60)
		{
			color = new Color(255, 0, 0);
		}
		else
		{
			color = ((_gameCore.FpsCounter.AverageFramesPerSecond >= 100) ? Color.White : new Color(255, 255, 0));
		}
		string text = (GameCore.GraphicsDeviceManager.SynchronizeWithVerticalRetrace ? "V" : "");
		text += ((_gameCore.MaxFPS > 0) ? "M" : "");
		_font.Write(GameCore.Editor.SpriteBatch, string.Format("FPS: {0}{1}, T: {2:0.00}, TF: {3:0.0}, CX: {4}, Mpos: {5:0.0}", _gameCore.FpsCounter.AverageFramesPerSecond, (text.Length > 0) ? (" " + text) : "", GameCore.GameLevel.Timer, GameCore.TimeFactor, (int)GameCore.CurrentLevel.Camera.Position.X, GameCore.SoundSystem.GetMusicPositionInSec()), posX, posY += 16, color, TextAlign.Right);
	}

	private static void DrawShort(GameTime gameTime)
	{
		Viewport mainViewPort = GameCore.MainViewPort;
		int posY = -16;
		int posX = mainViewPort.Width - 2;
		DrawFps(gameTime, posX, posY);
	}

	private static void DrawShop(GameTime gameTime)
	{
		Viewport mainViewPort = GameCore.MainViewPort;
		int num = -16;
		int num2 = mainViewPort.Width - 2;
		_font.Write(GameCore.Editor.SpriteBatch, $"Cursor: {GameJnG.ShopMenu.Cursor.Position.X}, {GameJnG.ShopMenu.Cursor.Position.Y}", num2, num += 16, Color.White, TextAlign.Right);
		DrawFps(gameTime, num2, num);
	}

	private static void DrawFull(GameTime gameTime)
	{
		int num = 0;
		Viewport mainViewPort = GameCore.MainViewPort;
		string text = "";
		int num2 = -16;
		int num3 = mainViewPort.Width - 2;
		text = "";
		foreach (Layer layer in GameCore.CurrentLevel.LayerList)
		{
			if (num > 0)
			{
				text += ", ";
			}
			text += $"{layer.VisibleGameObjectList.Count}/{layer.GameObjectList.Count}";
			num++;
		}
		_font.Write(GameCore.Editor.SpriteBatch, $"Objects: {text}", num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, $"Phys: {GameCore.GameLevel.PhysWorld.BodyList.Count}", num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, string.Format("Lights: {0}, Shadows: {1}", string.Join(",", LightCount), string.Join(",", ShadowCount)), num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, $"Particles: {ObjectParticlesTotal + LayerParticlesTotal}, Strip Lines: {ObjectStripLinesTotal + LayerStripLinesTotal}, Strip prtcs: {StripLineParticles}, Dist prtcs: {DistortionParticles}", num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, $"Particles: Obj: {ObjectParticleCountBack},{ObjectParticleCountFront}, Lay: {LayerParticleCountBack},{LayerParticleCountFront}", num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, $"Strip lines: Obj: {ObjectStripLineCountBack},{ObjectStripLineCountFront}, Lay: {LayerStripLineCountBack},{LayerStripLineCountFront}", num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, GameCore.Allocator.FormatGameForDebug(), num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, GameCore.Allocator.FormatRenderAForDebug(), num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, GameCore.Allocator.FormatRenderBForDebug(), num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, GameCore.Allocator.FormatHelpersForDebug(), num3, num2 += 16, Color.White, TextAlign.Right);
		_font.Write(GameCore.Editor.SpriteBatch, $"Camera: {GameCore.CurrentLevel.Camera.Position.X:0.0}, {GameCore.CurrentLevel.Camera.Position.Y:0.0} | SSpd: {GameCore.CurrentLevel.ScrollSpeed.X:0.0}", num3, num2 += 16, Color.White, TextAlign.Right);
		if (GameCore.IsEditorEnabled)
		{
			Vector2 vector = ((GameCore.Editor.CurrentLayer == null) ? Vector2.Zero : GameCore.Editor.Cursor.GetCurrentLayerPosition());
			if (!float.IsNaN(vector.X))
			{
				_font.Write(GameCore.Editor.SpriteBatch, $"Cursor: {GameCore.Editor.Cursor.Position.X}, {GameCore.Editor.Cursor.Position.Y} / {(int)vector.X}, {(int)vector.Y}", num3, num2 += 16, Color.White, TextAlign.Right);
			}
		}
		Color color;
		if (_gameCore.FpsCounter.AverageFramesPerSecond < 60)
		{
			color = new Color(255, 0, 0);
		}
		else
		{
			color = ((_gameCore.FpsCounter.AverageFramesPerSecond >= 100) ? Color.White : new Color(255, 255, 0));
		}
		string text2 = (GameCore.GraphicsDeviceManager.SynchronizeWithVerticalRetrace ? "V" : "");
		text2 += ((_gameCore.MaxFPS > 0) ? "M" : "");
		_font.Write(GameCore.Editor.SpriteBatch, string.Format("FPS: {0}{1}, T: {2:0.00}, TF: {3:0.0}", _gameCore.FpsCounter.AverageFramesPerSecond, (text2.Length > 0) ? (" " + text2) : "", GameCore.GameLevel.Timer, GameCore.TimeFactor), num3, num2 += 16, color, TextAlign.Right);
	}

	public static void Draw(GameTime gameTime)
	{
		GameCore.Editor.SpriteDrawBegin();
		if (GameCore.IsShopMenuEnabled)
		{
			DrawShop(gameTime);
		}
		else if (Options.DebugInfoMode == 1)
		{
			DrawFull(gameTime);
		}
		else
		{
			DrawShort(gameTime);
		}
		GameCore.Editor.SpriteDrawEnd();
	}
}
