using System.Linq;

namespace NEAT
{
	/// <summary>
	/// Saveable genome.
	/// This class can be saved into a file.
	/// </summary>
	[System.Serializable]
	public class PackedGenome
	{
		public PackedGene[] genes;

		public PackedGenome(Genome genome)
		{
			genes = genome.Genes.Select(x => new PackedGene(x)).ToArray();
		}
	}
}
