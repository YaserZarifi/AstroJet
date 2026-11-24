using System.IO.Compression;

namespace Rake2D;

public class ContentArchive
{
	public string Name;

	public ContentArchiveTypeEnum Type;

	public ZipArchive ZipArchive;

	public ContentArchive(string fileName, ContentArchiveTypeEnum type)
	{
		Name = fileName;
		Type = type;
	}
}
