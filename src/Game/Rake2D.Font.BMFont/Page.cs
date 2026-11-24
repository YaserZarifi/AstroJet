using System.IO;

namespace Rake2D.Font.BMFont;

public struct Page
{
	public string FileName { get; set; }

	public int Id { get; set; }

	public Page(int id, string fileName)
	{
		this = default;
		FileName = fileName;
		Id = id;
	}

	public override string ToString()
	{
		return $"{Id} ({Path.GetFileName(FileName)})";
	}
}
