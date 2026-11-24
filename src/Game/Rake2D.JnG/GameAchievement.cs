namespace Rake2D.JnG;

public class GameAchievement
{
	public string Name;

	public string Description;

	public bool IsAchieved;

	public GameAchievementEnum Id { get; private set; }

	public string IdStr
	{
		get
		{
			return Id.ToString();
		}
		private set
		{
			IdStr = value;
		}
	}

	public GameAchievement(GameAchievementEnum id)
	{
		Id = id;
	}
}
