using System;

namespace Rake2D;

public static class Program
{
	[STAThread]
	private static void Main()
	{
		using GameCore gameCore = new GameCore();
		try
		{
			gameCore.Run();
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Unexpected exception: {0}", ex);
			gameCore.Exit();
		}
	}
}
