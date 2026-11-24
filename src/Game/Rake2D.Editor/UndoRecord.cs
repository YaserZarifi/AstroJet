using System.Collections.Generic;

namespace Rake2D.Editor;

public class UndoRecord
{
	public string Name;

	public List<string> LoaderLines;

	public UndoRecord(string name, List<string> loaderLines)
	{
		Name = name;
		LoaderLines = loaderLines;
	}
}
