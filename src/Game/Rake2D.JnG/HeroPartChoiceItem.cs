using System.Collections.Generic;

namespace Rake2D.JnG;

public class HeroPartChoiceItem
{
	public int NumId;

	public List<string> IdList;

	public Dictionary<string, int> ChoiceIdIndexDictionary;

	public HeroPartChoiceItem(string[] idArray, int numId)
	{
		NumId = numId;
		IdList = new List<string>(idArray);
	}

	public string GetCurrentId(int index)
	{
		if (IdList.IsNullOrEmpty())
		{
			return "undefined";
		}
		if (index < 0 || index >= IdList.Count)
		{
			return IdList[0];
		}
		return IdList[index];
	}

	public void AddChoiceIdIndex(string choiceId, int index)
	{
		if (ChoiceIdIndexDictionary == null)
		{
			ChoiceIdIndexDictionary = new Dictionary<string, int>();
		}
		ChoiceIdIndexDictionary[choiceId] = index;
	}

	public void ApplyChoiceIndexes()
	{
		if (ChoiceIdIndexDictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> item in ChoiceIdIndexDictionary)
		{
			GameJnG.GetHeroPartChoiceById(item.Key)?.SetIdIndex(item.Value);
		}
	}
}
