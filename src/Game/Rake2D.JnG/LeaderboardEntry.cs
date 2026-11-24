using System;
using Microsoft.Xna.Framework;

namespace Rake2D.JnG;

public class LeaderboardEntry
{
	public int PlayerCount;

	public int Difficulty;

	public int StageNumber;

	public string UserName;

	public int Rank;

	public int Score;

	public int[] DetailsArray;

	public bool IsMyUser;

	public GameObject MyObject;

	public LeaderboardPlayer Player1;

	public LeaderboardPlayer Player2;

	public void Destroy()
	{
		Player1?.Destroy();
		Player1 = null;
		Player2?.Destroy();
		Player2 = null;
		MyObject?.Destroy();
		MyObject = null;
	}

	private void ParseHeroName(Hero hero, int[] detailsArray)
	{
		hero.PlayerName = "";
		for (int i = 48; i < detailsArray.Length && i < 64; i++)
		{
			if (detailsArray[i] > 0)
			{
				byte[] bytes = BitConverter.GetBytes(detailsArray[i]);
				hero.PlayerName += BitConverter.ToChar(bytes, (hero.PlayerNumber != 0) ? 2 : 0);
			}
		}
	}

	public void ParseDetails()
	{
		Player1 = null;
		Player2 = null;
		PlayerCount = 1;
		if (DetailsArray != null && DetailsArray.Length > 8)
		{
			PlayerCount = DetailsArray[0];
			Difficulty = DetailsArray[1];
			StageNumber = DetailsArray[2];
			Player1 = new LeaderboardPlayer();
			Player1.ParsePlayerDetails(DetailsArray, 3);
			Player1.MyHero.PlayerNumber = 0;
			ParseHeroName(Player1.MyHero, DetailsArray);
			if (PlayerCount > 1)
			{
				Player2 = new LeaderboardPlayer();
				Player2.ParsePlayerDetails(DetailsArray, 18);
				Player2.MyHero.PlayerNumber = 1;
				ParseHeroName(Player2.MyHero, DetailsArray);
			}
		}
	}

	public void Prepare(Layer layer)
	{
		MyObject = GameObject.CreateByPartDataSimple(GameCore.GetPartData("leaderboard_entry"), bCreateRigidBody: false);
		MyObject.SetLayer(layer);
		Player1.MyHero.CreateSmallShip(layer, Vector2.Zero);
		Player1.MyHero.DestroyRigidBody();
		MyObject.AddChild(Player1.MyHero.MyObject);
		if (Player2 != null)
		{
			Player2.MyHero.CreateSmallShip(layer, Vector2.Zero);
			Player2.MyHero.DestroyRigidBody();
			MyObject.AddChild(Player2.MyHero.MyObject, -1);
			Player1.MyHero.MyObject.Position = new Vector2(25f, 16f);
			Player1.MyHero.MyObject.SetScale(0.75f);
			if (Player2.MyHero.MyObject != null)
			{
				Player2.MyHero.MyObject.Position = new Vector2(-25f, -16f);
				Player2.MyHero.MyObject.SetScale(0.75f);
			}
		}
		MyObject.Activate();
	}
}
