namespace Rake2D.JnG;

public class SaveGameInfo
{
	public int Number;

	public int Score;

	public bool Mia;

	public GameModeEnum GameMode = GameModeEnum.Standard;

	public int Difficulty;

	public Hero Hero;

	public Hero Hero2;

	public GameObject MyObject;

	public void Destroy()
	{
		MyObject?.Destroy();
		Hero?.DestroySmallShip();
		Hero?.DestroyRigidBody();
		Hero2?.DestroySmallShip();
		Hero2?.DestroyRigidBody();
	}

	public void Clear()
	{
		Hero = null;
		Hero2 = null;
		Score = 0;
		Mia = false;
		Difficulty = 0;
	}
}
