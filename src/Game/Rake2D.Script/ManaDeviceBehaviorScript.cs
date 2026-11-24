using Rake2D.JnG;

namespace Rake2D.Script;

public class ManaDeviceBehaviorScript : ItemBehaviorScript
{
	protected PartData _readyObjectData;

	protected GameObject _readyObject;

	protected float _mana;

	protected bool _isActive;

	public override void Init()
	{
		base.Init();
		string text = ParamString("ready_id");
		if (text != null)
		{
			_readyObjectData = GameCore.GetPartData(text);
		}
	}

	public override void RegisterEvents()
	{
		base.RegisterEvents();
		_readyObject?.Destroy();
		_readyObject = null;
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
		_mana = Param("mana", 100f);
	}

	public override void Update(float elapsedTime)
	{
		base.Update(elapsedTime);
		if (!_isActive && MyHero.Mana >= MyHero.MaxMana)
		{
			if (_readyObject == null && _readyObjectData != null)
			{
				_readyObject = MySlot.MyObject.CreateChildByPartData(_readyObjectData, createPhysBody: false);
				if (MySlot.SlotType == HeroSlotEnum.primary_device)
				{
					_readyObject.SetPosition(-10f, 0f);
				}
				else
				{
					_readyObject.SetPosition(20f, 0f);
				}
			}
		}
		else if (_readyObject != null)
		{
			_readyObject.Destroy();
			_readyObject = null;
		}
	}
}
