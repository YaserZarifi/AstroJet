using System;
using Microsoft.Xna.Framework;

namespace Rake2D;

public class Transformation
{
	public Vector2 Position;

	public Vector2 Scale;

	public float Rotation;

	public bool IsRotationFixed;

	private Color _color;

	public ColorMode ColorMode;

	public Transformation ParentTransformation;

	public Transformation _precalculatedWorldTransformation;

	public virtual Color Color
	{
		get
		{
			if (ParentTransformation != null)
			{
				switch (ColorMode)
				{
				case ColorMode.ParentRGBA:
					return ParentTransformation.Color;
				case ColorMode.ParentRGBA2:
					return new Color(ParentTransformation.Color, (int)((float)(int)ParentTransformation.Alpha * ((float)(int)_color.A / 255f)));
				case ColorMode.ParentAlpha:
					return new Color(_color.R, _color.G, _color.B, (byte)((float)(int)ParentTransformation.Alpha * ((float)(int)_color.A / 255f)));
				case ColorMode.ParentRGB:
				{
					Color color = ParentTransformation.Color;
					color.A = _color.A;
					return color;
				}
				}
			}
			return _color;
		}
		set
		{
			_color = value;
		}
	}

	public byte Alpha
	{
		get
		{
			return Color.A;
		}
		set
		{
			_color.A = value;
		}
	}

	public Color ColorValue => _color;

	public virtual Matrix WorldMatrix
	{
		get
		{
			if (ParentTransformation != null && ParentTransformation != this)
			{
				if (IsRotationFixed)
				{
					return GetLocalMatrix() * Matrix.CreateScale(ParentTransformation.WorldScale.X, ParentTransformation.WorldScale.Y, 1f) * Matrix.CreateTranslation(ParentTransformation.WorldPosition.X, ParentTransformation.WorldPosition.Y, 0f);
				}
				return GetLocalMatrix() * ParentTransformation.WorldMatrix;
			}
			return GetLocalMatrix();
		}
	}

	public Transformation WorldTransformation
	{
		get
		{
			if (ParentTransformation == null)
			{
				return this;
			}
			return GetComplexWorldTransformation();
		}
	}

	public Vector2 WorldPosition => WorldTransformation.Position;

	public float WorldRotation => WorldTransformation.Rotation;

	public Vector2 WorldScale => WorldTransformation.Scale;

	public void ChangeParentTransformation(Transformation parentTrans)
	{
		ResetPrecalculations();
		if (parentTrans == null)
		{
			if (ParentTransformation != null)
			{
				Transformation transformation = GameCore.Allocator.GetTransformation();
				transformation.SetTransformation(WorldTransformation);
				SetTransformation(transformation);
				GameCore.Allocator.DeleteTransformation(transformation);
			}
		}
		else if (ParentTransformation == null)
		{
			Position = parentTrans.GetRelativePosition(Position);
		}
		ParentTransformation = parentTrans;
	}

	public void DetachFromParentTransformation()
	{
		if (ParentTransformation != null)
		{
			_color = Color;
			ColorMode = ColorMode.Local;
			ChangeParentTransformation(null);
			ResetPrecalculations();
		}
	}

	public Vector2 GetRelativePosition(Vector2 worldPosition)
	{
		return Vector2.Transform(worldPosition - WorldPosition, Matrix.CreateRotationZ(0f - WorldRotation) * Matrix.CreateScale(1f / WorldScale.X, 1f / WorldScale.Y, 0f));
	}

	public Color GetMyColor()
	{
		return _color;
	}

	public void SetColorRGB(byte R, byte G, byte B)
	{
		_color.R = R;
		_color.G = G;
		_color.B = B;
	}

	public static Transformation CreateByPosition(float x, float y)
	{
		Transformation transformation = GameCore.Allocator.GetTransformation();
		transformation.Position = new Vector2(x, y);
		return transformation;
	}

	public static Transformation CreateByPosition(Vector2 pos)
	{
		Transformation transformation = GameCore.Allocator.GetTransformation();
		transformation.Position = pos;
		return transformation;
	}

	public virtual Matrix GetLocalMatrix()
	{
		return Matrix.CreateScale(Scale.X, Scale.Y, 1f) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(Position.X, Position.Y, 0f);
	}

	public virtual Transformation GetComplexWorldTransformation()
	{
		if (_precalculatedWorldTransformation == null)
		{
			Transformation worldTransformation = ParentTransformation.WorldTransformation;
			_precalculatedWorldTransformation = GameCore.Allocator.GetTransformation();
			_precalculatedWorldTransformation.Position = Vector2.Transform(Position, ParentTransformation.WorldMatrix);
			_precalculatedWorldTransformation.Rotation = Rotation;
			if (!IsRotationFixed)
			{
				if (worldTransformation.Scale.X < 0f)
				{
					_precalculatedWorldTransformation.Rotation = (float)Math.PI * 2f - _precalculatedWorldTransformation.Rotation;
				}
				if (worldTransformation.Scale.Y < 0f)
				{
					_precalculatedWorldTransformation.Rotation = (float)Math.PI * 2f - _precalculatedWorldTransformation.Rotation;
				}
				_precalculatedWorldTransformation.Rotation += worldTransformation.Rotation;
			}
			_precalculatedWorldTransformation.Scale = Scale * worldTransformation.Scale;
		}
		return _precalculatedWorldTransformation;
	}

	public void ResetPrecalculations()
	{
		if (_precalculatedWorldTransformation != null)
		{
			GameCore.Allocator.DeleteTransformation(_precalculatedWorldTransformation);
			_precalculatedWorldTransformation = null;
		}
	}

	public Transformation()
	{
		ClearTransformation(isInit: true);
	}

	public void ClearTransformation(bool isInit)
	{
		Position = Vector2.Zero;
		Scale = Vector2.One;
		Rotation = 0f;
		IsRotationFixed = false;
		Color = Color.White;
		ColorMode = ColorMode.Local;
		ParentTransformation = null;
		if (_precalculatedWorldTransformation != null)
		{
			GameCore.Allocator.DeleteTransformation(_precalculatedWorldTransformation);
			_precalculatedWorldTransformation = null;
		}
	}

	public Transformation(Transformation transformation)
	{
		SetTransformation(transformation);
	}

	public virtual void SetTransformation(Transformation transformation)
	{
		Position = transformation.Position;
		Scale = transformation.Scale;
		Rotation = transformation.Rotation;
	}

	public static void ConvertToScreen(ref Transformation trans, Layer layer)
	{
		if (layer != null)
		{
			trans.Position = layer.MyLevel.Camera.LayerPosToScreen(trans.Position, layer);
		}
	}
}
