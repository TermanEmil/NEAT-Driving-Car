namespace NEAT
{
	/// <summary>
	/// Saveable gene.
	/// </summary>
	[System.Serializable]
	public class PackedGene
	{
		public float weight;
		public bool isEnabled;

		public PackedNeuron inNeuron;
		public PackedNeuron outNeuron;

		public PackedGene(Gene gene)
		{
			weight = gene.Weight;
			isEnabled = gene.IsEnabled;
			inNeuron = new PackedNeuron(gene.StartNode);
			outNeuron = new PackedNeuron(gene.EndNode);
		}
	}
}
