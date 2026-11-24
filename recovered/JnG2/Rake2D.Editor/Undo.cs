using System;
using System.Collections.Generic;
using System.Linq;

namespace Rake2D.Editor;

public class Undo
{
	public int MaxRecords;

	public List<UndoRecord> RecordList;

	private int CurrentUndoNumber;

	public Undo()
	{
		MaxRecords = 25;
		RecordList = new List<UndoRecord>();
	}

	public void Reset()
	{
		RecordList.Clear();
		CurrentUndoNumber = 0;
	}

	public void SaveUndo(string name)
	{
		RecordList.RemoveRange(0, CurrentUndoNumber);
		if (RecordList.Count > MaxRecords)
		{
			RecordList.RemoveAt(RecordList.Count - 1);
		}
		try
		{
			RecordList.Insert(0, new UndoRecord(name, GameCore.GameLevel.SaveToStringList()));
		}
		catch (Exception ex)
		{
			GameCore.LogErr("Save undo exception: {0}", ex);
		}
		CurrentUndoNumber = 0;
	}

	public void AddUndo(string name, string[] stringArray)
	{
		RecordList.RemoveRange(0, CurrentUndoNumber);
		if (RecordList.Count > MaxRecords)
		{
			RecordList.RemoveAt(RecordList.Count - 1);
		}
		new List<string>();
		RecordList.Insert(0, new UndoRecord(name, stringArray.OfType<string>().ToList()));
		CurrentUndoNumber = 0;
	}

	public void DoUndo()
	{
		if (CurrentUndoNumber < RecordList.Count - 1)
		{
			CurrentUndoNumber++;
			GameCore.CurrentLevel.Reload(RecordList[CurrentUndoNumber].LoaderLines.ToArray());
		}
	}

	public void DoRedo()
	{
		if (CurrentUndoNumber > 0)
		{
			CurrentUndoNumber--;
			GameCore.CurrentLevel.Reload(RecordList[CurrentUndoNumber].LoaderLines.ToArray());
		}
	}
}
