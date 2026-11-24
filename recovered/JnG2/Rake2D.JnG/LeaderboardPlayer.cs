namespace Rake2D.JnG;

public class LeaderboardPlayer
{
	public Hero MyHero;

	private void AddPartMapEntry(PartMap partMap, string choiceId, int numId, int idIndex)
	{
		HeroPartChoiceItem heroPartChoiceByNumId = GameJnG.GetHeroPartChoiceByNumId(choiceId, numId);
		if (heroPartChoiceByNumId != null)
		{
			if (idIndex >= 0 && idIndex < heroPartChoiceByNumId.IdList.Count)
			{
				partMap.AddItem(choiceId, heroPartChoiceByNumId.IdList[idIndex]);
			}
			else
			{
				partMap.AddItem(choiceId, heroPartChoiceByNumId.IdList[0]);
			}
		}
	}

	public void ParsePlayerDetails(int[] details, int startIndex)
	{
		MyHero = new Hero();
		MyHero.FaceNumber = details[startIndex];
		HeroShip heroShip = new HeroShip("ship");
		MyHero.SetShip(heroShip);
		int num = details[startIndex + 1];
		if (num >= 0 && num < GameJnG.HeroCamouflageIdList.Count)
		{
			heroShip.PartMap.SkinId = GameJnG.HeroCamouflageIdList[num];
		}
		else
		{
			heroShip.PartMap.SkinId = GameJnG.HeroCamouflageIdList[0];
		}
		AddPartMapEntry(heroShip.PartMap, "body", details[startIndex + 2], details[startIndex + 3]);
		AddPartMapEntry(heroShip.PartMap, "body/nose", details[startIndex + 4], details[startIndex + 5]);
		AddPartMapEntry(heroShip.PartMap, "body/side_front", details[startIndex + 6], details[startIndex + 7]);
		AddPartMapEntry(heroShip.PartMap, "body/side_front/wing", details[startIndex + 8], details[startIndex + 9]);
		AddPartMapEntry(heroShip.PartMap, "body/rudder", details[startIndex + 10], details[startIndex + 11]);
		AddPartMapEntry(heroShip.PartMap, "body/jet", details[startIndex + 12], details[startIndex + 13]);
		heroShip.PartMap.PartOffsetXPercentages["side_front"] = (float)details[startIndex + 14] / 100f;
	}

	public void Destroy()
	{
		if (MyHero != null)
		{
			MyHero.DestroySmallShip();
			MyHero = null;
		}
	}
}
