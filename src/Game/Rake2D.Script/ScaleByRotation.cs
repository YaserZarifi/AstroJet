using System;
using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class ScaleByRotation : BehaviorScript
{
	private Vector2 _scaleFactor;

	private Vector2Range _scaleLimit;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEditorUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_scaleFactor = ParamVector2("scale_factor", 100f, 100f) / 100f;
		_scaleLimit = ParamVector2Range("scale_limit", 10f, 200f) / 100f;
	}

	private void OnUpdate(float elapsedTime)
	{
		Vector2 vector = MyObject.GetMainPart().WorldRotation / (float)Math.PI * _scaleFactor;
		MyObject.SetScale(MyObject.PartData.DefaultPartState.Scale * new Vector2(Utils.Clamp(1f + vector.X, _scaleLimit.FromX, _scaleLimit.ToX), Utils.Clamp(1f + vector.Y, _scaleLimit.FromY, _scaleLimit.ToY)));
	}
}
