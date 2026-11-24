using System.Collections.Generic;

namespace Rake2D.JnG;

public class PartTree
{
	public string PartId;

	public List<PartTree> SubParts;

	public void AddSubPart(PartTree subPart)
	{
		if (SubParts == null)
		{
			SubParts = new List<PartTree>();
		}
		SubParts.Add(subPart);
	}
}
