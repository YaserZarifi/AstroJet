using System.IO;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Common;

public static class WorldSerializer
{
	public static void Serialize(World world, string filename)
	{
		using FileStream stream = new FileStream(filename, FileMode.Create);
		WorldXmlSerializer.Serialize(world, stream);
	}

	public static World Deserialize(string filename)
	{
		using FileStream stream = new FileStream(filename, FileMode.Open);
		return WorldXmlDeserializer.Deserialize(stream);
	}
}
