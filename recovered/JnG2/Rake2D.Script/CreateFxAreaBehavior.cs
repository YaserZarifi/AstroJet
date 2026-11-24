using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public abstract class CreateFxAreaBehavior : BehaviorScript
{
	private int _step = 64;

	private FloatRange _loopDelayRange;

	private FloatRange _scaleRange;

	private float _loopTimer;

	private GameObject _parentPart;

	private PartData _objectData;

	private int _damage;

	private bool _useParentPart;

	public override void Init()
	{
		base.Init();
		_step = ParamInt("step", 64);
		_loopDelayRange = ParamRange("loop_delay", 100f) / 1000f;
		_scaleRange = ParamRange("scale", 100f) / 100f;
		_damage = ParamInt("damage", -999);
		_objectData = GameCore.GetPartData(ParamString("object_id"));
		_useParentPart = ParamBool("use_parent_part");
	}

	protected void StartCreate()
	{
		_parentPart = ((_useParentPart && MyObject.ParentObject != null) ? MyObject.ParentObject : MyObject);
		_loopTimer = 0f;
		MyObject.OnUpdate += UpdateLoop;
		if (_doInEditor)
		{
			MyObject.OnEditorUpdate += UpdateLoop;
		}
	}

	protected void UpdateLoop(float elapsedTime)
	{
		if (!Utils.ChangeValueToTarget(ref _loopTimer, 0f, elapsedTime))
		{
			return;
		}
		Rectangle fxRectangle = _parentPart.FxRectangle;
		float num = (float)_step / _parentPart.Scale.X;
		float num2 = (float)_step / _parentPart.Scale.Y;
		int num3 = Utils.Clamp((int)((float)(fxRectangle.Width * fxRectangle.Height) / (num * num2)), 1, 10);
		for (int i = 0; i < num3; i++)
		{
			GameObject gameObject = _parentPart.CreateChildByPartData(_objectData, createPhysBody: true);
			gameObject.SetPosition(fxRectangle.Left + Utils.GetRandom(fxRectangle.Width), fxRectangle.Top + Utils.GetRandom(fxRectangle.Height));
			float random = _scaleRange.GetRandom();
			gameObject.SetScale(random / _parentPart.WorldScale.X, random / _parentPart.WorldScale.Y);
			gameObject.Alpha = MyObject.Alpha;
			if (_damage >= 0)
			{
				gameObject.Damage = _damage;
			}
			else if (_damage != -999)
			{
				gameObject.Damage = MyObject.Damage * (float)_damage / -100f;
			}
		}
		_loopTimer = _loopDelayRange.GetRandom();
	}
}
