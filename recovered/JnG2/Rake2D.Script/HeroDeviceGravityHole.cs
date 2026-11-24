using Microsoft.Xna.Framework;

namespace Rake2D.Script;

public class HeroDeviceGravityHole : ItemBehaviorScript
{
	private GameObject _holeObject;

	private PartData _holeData;

	private PartData _blastData;

	private Vector2 _offset;

	private float _manaPerSec;

	private bool _isActivated;

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	public override void Init()
	{
		base.Init();
		_holeData = GameCore.GetPartData("gravity_hole");
		_blastData = GameCore.GetPartData("gravity_hole_blast");
		_manaPerSec = Param("mana");
		_offset = new Vector2(190f, 0f);
	}

	public override void StartInSlot()
	{
		base.StartInSlot();
	}

	private void OnActivated()
	{
		if (!_isActivated)
		{
			MyObject.OnUpdate += OnUpdate;
		}
	}

	private void ShotBlast()
	{
		_holeObject.MyLayer.CreateObjectByPartData(_blastData, _holeObject.WorldPosition, _holeObject.Scale, 0f, _holeObject.ZOrder);
		_holeObject.Destroy();
		_holeObject = null;
	}

	private void OnUpdate(float elapsedTime)
	{
		if (MySlot.IsButtonDown && MyHero.ManaPercent > 0f)
		{
			if (_holeObject == null)
			{
				_holeObject = MyObject.MyLayer.CreateObjectByPartData(_holeData, MyHero.MyObject.WorldPosition, Vector2.One, 0f, MyHero.MyObject.ZOrder + 1);
			}
			if (_holeObject != null)
			{
				_holeObject.Position = MyHero.MyObject.WorldPosition + _offset;
			}
			MyHero.UseMana(_manaPerSec * elapsedTime);
			_isActivated = true;
		}
		else
		{
			if (_holeObject != null)
			{
				ShotBlast();
			}
			_isActivated = false;
			MyObject.OnUpdate -= OnUpdate;
		}
	}
}
