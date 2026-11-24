using Rake2D.JnG;

namespace Rake2D.Script;

public class ItemModifyParameter : ItemBehaviorScript
{
	private string _paramName;

	private float _percent;

	private float _increase;

	private float _set;

	private bool _isGem;

	public override void StartInSlot()
	{
		base.StartInSlot();
	}

	public override void RecalculateParams()
	{
		base.RecalculateParams();
		_paramName = ParamString("parameter");
		_percent = Param("percent") / 100f;
		_increase = Param("increase");
		_set = Param("set", -9999f);
		_isGem = ParamBool("gem");
	}

	public override bool UpdateParamModifier(ref ParamModifier modifier, string paramName, HeroItem item)
	{
		if ((!_isGem || item == null || (MyItem.MySlot.SlotType == HeroSlotEnum.gem_front && item.MySlot.SlotType == HeroSlotEnum.front_gun) || (MyItem.MySlot.SlotType == HeroSlotEnum.gem_top && item.MySlot.SlotType == HeroSlotEnum.top_gun) || (MyItem.MySlot.SlotType == HeroSlotEnum.gem_bottom && item.MySlot.SlotType == HeroSlotEnum.bottom_gun) || (MyItem.MySlot.SlotType == HeroSlotEnum.gem_rear && item.MySlot.SlotType == HeroSlotEnum.rear_gun)) && Utils.Equals(paramName, _paramName))
		{
			if (_set != -9999f)
			{
				modifier.Increase = _set;
				modifier.Percentage = 0f;
				modifier.IgnoreBaseValue = true;
			}
			else
			{
				modifier.Increase += _increase;
				modifier.Percentage += _percent;
			}
			return true;
		}
		return false;
	}
}
