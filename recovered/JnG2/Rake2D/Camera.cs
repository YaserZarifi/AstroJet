using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Rake2D;

public class Camera
{
	public Vector2 Position;

	public float Zoom;

	public Matrix ScreenMatrixCurrentLayer;

	public Matrix ScreenMatrixMainLayer;

	public Matrix WorldMatrixMainLayer;

	public Matrix WorldMatrixCurrentLayer;

	public Vector2 Origin;

	public Vector2 ScreenCenterOffset;

	public Vector2 Scale;

	public Vector2 ScreenTopLeftInWorld;

	public Vector2 ScreenBottomRightInWorld;

	public Vector2 ViewTopLeftOnScreen;

	public Vector2 ViewBottomRightOnScreen;

	public Vector2 ViewTopLeftInWorld;

	public Vector2 ViewBottomRightInWorld;

	public Vector2 ScreenCenterInWorld;

	private Viewport _viewPort;

	private float _targetZoom;

	private float _zoomSpeed;

	private float _panSpeed;

	private bool _useFrameTime;

	private float _quakeTimer;

	private float _quakeAngle;

	public Vector2 QuakeOffset;

	private Vector2 _targetPos;

	public float Rotation { get; set; }

	public GameLevel MyLevel { get; private set; }

	public Layer MyLayer { get; private set; }

	public Camera(GameLevel level)
	{
		MyLevel = level;
		Rotation = 0f;
		Zoom = 1f;
		_targetZoom = Zoom;
		_panSpeed = 0f;
		Position = Vector2.Zero;
		Scale = Vector2.One;
	}

	public void AddQuake(float power)
	{
		if (Options.QuakeMode > 0)
		{
			power = Utils.Clamp(power, 0f, 100f);
			if (Options.QuakeMode == 1)
			{
				power /= 2f;
			}
			if (_quakeTimer < power)
			{
				_quakeTimer = power;
			}
		}
	}

	public void StopQuake()
	{
		_quakeTimer = 0f;
		_quakeAngle = 0f;
		QuakeOffset = Vector2.Zero;
	}

	public void Update(float elapsedTime)
	{
		float num = elapsedTime;
		if (_useFrameTime)
		{
			num = GameCore.FrameTime;
		}
		Utils.ChangeValueToTarget(ref Zoom, _targetZoom, _zoomSpeed * num);
		if (!Utils.ChangeValueToTarget(ref _quakeTimer, 0f, 100f * elapsedTime))
		{
			_quakeAngle += 55f * elapsedTime;
			QuakeOffset.X = (float)Math.Sin(_quakeAngle * 1.4f) * _quakeTimer / 3f;
			QuakeOffset.Y = (float)Math.Sin(_quakeAngle) * _quakeTimer / 2f;
			if (!GameCore.IsEditorEnabled)
			{
				if (Position.Y - QuakeOffset.Y < 0f)
				{
					QuakeOffset.Y = Position.Y;
				}
				else if (Position.Y + 1080f + QuakeOffset.Y > MyLevel.Size.Y)
				{
					QuakeOffset.Y = MyLevel.Size.Y - Position.Y - 1080f;
				}
			}
		}
		else
		{
			QuakeOffset = Vector2.Zero;
		}
		if (_panSpeed > 0f && _targetPos != Position)
		{
			float num2 = Math.Max(3000f, _panSpeed * (Utils.GetDistance(Position, _targetPos) / 3000f));
			if (Utils.ChangeValueToTarget(ref Position, _targetPos, num2 * elapsedTime))
			{
				_panSpeed = 0f;
			}
		}
	}

	public void ZoomTo(float targetZoom, float zoomSpeed, bool useFrameTime)
	{
		_targetZoom = targetZoom;
		_zoomSpeed = zoomSpeed;
		_useFrameTime = useFrameTime;
	}

	public void SetZoom(float zoom)
	{
		Zoom = zoom;
		_targetZoom = zoom;
		_zoomSpeed = 0f;
	}

	public void PanTo(Vector2 targetPos, float panSpeed, bool useFrameTime)
	{
		_targetPos = targetPos;
		_panSpeed = panSpeed;
		_useFrameTime = useFrameTime;
	}

	public void SetViewPort(Viewport viewPort)
	{
		Scale = GameCore.VirtualAspectRatio;
		_viewPort = viewPort;
		Origin = new Vector2((float)_viewPort.Width / 2f, (float)_viewPort.Height / 2f);
	}

	public void SetLayer(Layer layer)
	{
		if (layer != null)
		{
			UpdateMatrices(layer);
		}
		MyLayer = layer;
	}

	private Matrix GetLayerScreenMatrix(Layer layer)
	{
		return Matrix.CreateTranslation(new Vector3(-Origin, 0f)) * Matrix.CreateScale(1f / Scale.X, 1f / Scale.Y, 1f) * Matrix.CreateScale(1f / Zoom, 1f / Zoom, 1f) * Matrix.CreateTranslation(new Vector3(Position * layer.ScrollFactor - GameCore.ScreenCenterOffset / Scale, 0f)) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(new Vector3(Origin / Scale, 0f));
	}

	private void UpdateMatrices(Layer layer)
	{
		WorldMatrixCurrentLayer = GetLayerWorldMatrix(layer);
		ScreenMatrixCurrentLayer = GetLayerScreenMatrix(layer);
		ScreenTopLeftInWorld = ScreenPosToCurrentLayer(Vector2.Zero);
		ScreenBottomRightInWorld = ScreenPosToCurrentLayer(new Vector2(_viewPort.Width - 1, _viewPort.Height - 1));
		ScreenCenterInWorld = ScreenPosToCurrentLayer(new Vector2(_viewPort.Width / 2, _viewPort.Height / 2));
		ViewTopLeftOnScreen = MainLayerPosToScreen(Position - QuakeOffset);
		ViewBottomRightOnScreen = MainLayerPosToScreen(Position - QuakeOffset + new Vector2(1919f, 1079f));
		ViewTopLeftInWorld = Position * layer.ScrollFactor;
		ViewBottomRightInWorld = ViewTopLeftInWorld + new Vector2(1919f, 1079f);
		if (layer == MyLevel.MainLayer)
		{
			WorldMatrixMainLayer = WorldMatrixCurrentLayer;
			ScreenMatrixMainLayer = ScreenMatrixCurrentLayer;
		}
	}

	private Matrix GetLayerWorldMatrix(Layer layer)
	{
		if (layer == null)
		{
			return Matrix.Identity;
		}
		return Matrix.CreateTranslation(new Vector3(-(Position - QuakeOffset) * layer.ScrollFactor + GameCore.ScreenCenterOffset / Scale, 0f)) * Matrix.CreateTranslation(new Vector3(-Origin / Scale, 0f)) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateScale(Zoom * Scale.X, Zoom * Scale.Y, 1f) * Matrix.CreateTranslation(new Vector3(Origin, 0f));
	}

	public Vector2 LayerPosToScreen(Vector2 position, Layer layer)
	{
		return Vector2.Transform(position, GetLayerWorldMatrix(layer));
	}

	public Vector2 CurrentLayerPosToScreen(Vector2 position)
	{
		return Vector2.Transform(position, WorldMatrixCurrentLayer);
	}

	public Vector2 MainLayerPosToScreen(Vector2 position)
	{
		return Vector2.Transform(position, WorldMatrixMainLayer);
	}

	public Vector2 ScreenPosToCurrentLayer(Vector2 position)
	{
		return Vector2.Transform(position, ScreenMatrixCurrentLayer);
	}

	public Vector2 ScreenPosToMainLayer(Vector2 position)
	{
		return Vector2.Transform(position, ScreenMatrixMainLayer);
	}

	public Vector2 ScreenPosToLayer(Vector2 position, Layer layer)
	{
		return Vector2.Transform(position, GetLayerScreenMatrix(layer));
	}

	public bool IsPointOnScreen(Vector2 position)
	{
		if (position.X <= ScreenBottomRightInWorld.X && position.X >= ScreenTopLeftInWorld.X)
		{
			return true;
		}
		return false;
	}
}
