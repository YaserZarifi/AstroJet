using System.Runtime.InteropServices;

namespace Rake2D.JnG;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Cheat
{
	public static bool NoHeat;

	public static bool NoDamage;

	public static bool FullMana;
}
