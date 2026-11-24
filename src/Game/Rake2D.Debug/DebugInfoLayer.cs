using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rake2D.Debug;

public class DebugInfoLayer
{
	public string Name;

	public int LayerObjects;

	public int LayerStrips;

	public int LayerParticleLists;

	public int LayerZFlushes;

	public int ObjectDraws;

	public int ObjectStrips;

	public int ObjectPolygons;

	public int ObjectSpines;

	public int ObjectSprites;

	public int ObjectParticleLists;

	public int Particles;

	public int ListParticleBegins;

	public int DistortionParticleBegins;

	public int ParticleFactorCount;

	public List<float> ParticleFactors = new List<float>(32);

	public List<Vector3> LightFactors = new List<Vector3>(32);

	public void Clear()
	{
		LayerObjects = 0;
		LayerStrips = 0;
		LayerParticleLists = 0;
		LayerZFlushes = 0;
		ObjectDraws = 0;
		ObjectStrips = 0;
		ObjectPolygons = 0;
		ObjectSpines = 0;
		ObjectSprites = 0;
		ObjectParticleLists = 0;
		Particles = 0;
		ListParticleBegins = 0;
		DistortionParticleBegins = 0;
		ParticleFactorCount = 0;
		ParticleFactors.Clear();
		LightFactors.Clear();
	}

	public void Add(DebugInfoLayer other)
	{
	}

	public void AddParticleFactor(float factor)
	{
	}

	public void AddLightFactor(Vector3 factor)
	{
	}
}
