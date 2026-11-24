using System;
using Rake2D.JnG;

namespace Rake2D.Script;

public class HeroDeviceRotaryCage : ItemBehaviorScript
{
	private HeroSlot _topSlot;

	private HeroSlot _bottomSlot;

	private float _angleOffset;

	private float _targetAngleOffset;

	private float _rotationStep;

	private float _maxAngleOffset;

	public override void RegisterEvents()
	{
		MyItem.OnActivated += OnActivated;
	}

	public override void StartInSlot()
	{
		_topSlot = MyHero.GetSlot(HeroSlotEnum.top_gun);
		_bottomSlot = MyHero.GetSlot(HeroSlotEnum.bottom_gun);
		_rotationStep = (float)Math.PI / 4f;
		_maxAngleOffset = (float)(MyItem.Level + 1) * (float)Math.PI / 4f;
		if (_targetAngleOffset >= _maxAngleOffset)
		{
			_targetAngleOffset = _maxAngleOffset;
		}
	}

	private void OnActivated()
	{
		MyHero.MyObject.PlaySound("device_rotary_cage");
		_targetAngleOffset += _rotationStep;
		if (_targetAngleOffset == _maxAngleOffset && _maxAngleOffset > 0f)
		{
			_targetAngleOffset = _maxAngleOffset;
		}
		else if (_targetAngleOffset > _maxAngleOffset)
		{
			_targetAngleOffset = 0f;
			_rotationStep = (float)Math.PI / 4f;
		}
		else if (_targetAngleOffset <= 0f)
		{
			_targetAngleOffset = 0f;
			_rotationStep = (float)Math.PI / 4f;
		}
	}

	public override void Update(float elapsedTime)
	{
		Utils.ChangeValueToTarget(ref _angleOffset, _targetAngleOffset, (float)((_targetAngleOffset == 0f) ? ((MyItem.Level + 1) * 3) : 3) * elapsedTime);
		_topSlot.Rotation = _topSlot.DefaultRotation - _angleOffset;
		_bottomSlot.Rotation = _bottomSlot.DefaultRotation + _angleOffset;
	}
}
