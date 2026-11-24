using System.Collections.Generic;

namespace Rake2D.JnG;

public class HeroPartChoice
{
	public string Id;

	public List<HeroPartChoiceItem> ItemList;

	private int _currentItemIndex;

	private int _currentIdIndex;

	public int ItemCount => ItemList.Count;

	public int ItemIndex => _currentItemIndex;

	public HeroPartChoice(string id)
	{
		Id = id;
		ItemList = new List<HeroPartChoiceItem>();
	}

	public void SetItemIndex(int index)
	{
		if (index != _currentItemIndex)
		{
			_currentItemIndex = index;
			ApplyCurrentChoiceIndex();
		}
	}

	public void ApplyCurrentChoiceIndex()
	{
		GetItem(_currentItemIndex)?.ApplyChoiceIndexes();
	}

	public void SetIdIndex(int index)
	{
		_currentIdIndex = index;
	}

	public void AddItem(HeroPartChoiceItem item)
	{
		ItemList.Add(item);
	}

	public HeroPartChoiceItem GetItem(int index)
	{
		if (index >= 0 && index < ItemList.Count)
		{
			return ItemList[index];
		}
		return null;
	}

	public string GetCurrentId()
	{
		return GetItem(_currentItemIndex)?.GetCurrentId(_currentIdIndex);
	}

	public bool SetIndexesByPartId(string partId)
	{
		for (int i = 0; i < ItemList.Count; i++)
		{
			if (ItemList[i].IdList.Contains(partId))
			{
				SetItemIndex(i);
			}
		}
		return true;
	}
}
