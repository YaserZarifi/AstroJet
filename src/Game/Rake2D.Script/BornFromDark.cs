using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class BornFromDark : CreateObjectBehavior
{
	private float _alpha;

	private float _fadeInSpeed;

	public override void RegisterEvents()
	{
		MyObject.OnUpdate += OnUpdate;
		MyObject.OnEditorUpdate += OnUpdate;
	}

	public override void Init()
	{
		base.Init();
		_fadeInSpeed = Param("fade_in", 255f) / 255f;
		IntRange intRange = ParamIntRange("flip", 0, 0);
		if (intRange.From == 1 || (intRange.From > 1 && Utils.GetRandom(1) != 0))
		{
			MyObject.Scale.X *= -1f;
			_scaleRange.FromX *= -1f;
			_scaleRange.ToX *= -1f;
		}
		if (intRange.To == 1 || (intRange.To > 1 && Utils.GetRandom(1) != 0))
		{
			MyObject.Scale.Y *= -1f;
			_scaleRange.FromY *= -1f;
			_scaleRange.ToY *= -1f;
		}
	}

	protected void OnUpdate(float elapsedTime)
	{
		if (Utils.ChangeValueToTarget(ref _alpha, 1f, _fadeInSpeed * elapsedTime))
		{
			CreateObjectList(null, Vector2.Zero);
			MyObject.Destroy();
		}
		MyObject.Color = new Color(_alpha, _alpha, _alpha, _alpha);
	}
}
