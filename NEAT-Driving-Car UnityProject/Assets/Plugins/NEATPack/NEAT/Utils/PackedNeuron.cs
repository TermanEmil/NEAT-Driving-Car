namespace NEAT
{
	[System.Serializable]
	public class PackedNeuron
	{
		public int innovationNb;
		public ENeuronType type;

		public PackedNeuron(Neuron neuron)
		{
			innovationNb = neuron.InnovationNb;
			type = neuron.Type;
		}
	}
}
