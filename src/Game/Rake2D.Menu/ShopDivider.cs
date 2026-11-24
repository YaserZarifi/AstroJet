using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class ShopDivider : Transformation
{
	public enum ShopDividerTypeEnum
	{
		Weapon,
		Gem,
		Missile,
		Bomb,
		Device,
		Equipment,
		Other,
		None
	}

	public Vector2 Offset;

	public ShopDividerTypeEnum Type;

	public int SymbolFrame;

	public Vector2 SymbolOffset;

	public static ShopDividerTypeEnum GetDividerType(HeroItem item)
	{
		if (item == null)
		{
			return ShopDividerTypeEnum.None;
		}
		if (item.IsTypeWeapon)
		{
			return ShopDividerTypeEnum.Weapon;
		}
		if (item.IsTypeBomb)
		{
			return ShopDividerTypeEnum.Bomb;
		}
		if (item.IsTypeMissile)
		{
			return ShopDividerTypeEnum.Missile;
		}
		if (item.IsTypeDevice)
		{
			return ShopDividerTypeEnum.Device;
		}
		if (item.IsTypeEquipment || item.IsTypeShield || item.IsTypeArmor)
		{
			return ShopDividerTypeEnum.Equipment;
		}
		if (item.IsTypeGem)
		{
			return ShopDividerTypeEnum.Gem;
		}
		return ShopDividerTypeEnum.Other;
	}
}
