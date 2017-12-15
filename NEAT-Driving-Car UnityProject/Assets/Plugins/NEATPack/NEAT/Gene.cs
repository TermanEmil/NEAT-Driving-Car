using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NEAT
{
	[System.Serializable]
	public class Gene
	{
		#region Private data
		// Statically store all kinds of genes, to identify the innovation number
		// of a new gene.
		private static List<Gene> kindsOfGenes = new List<Gene>();
		#endregion

		#region Public Properties
		public float Weight { get; set; } = 0;
		public int InnovationNb { get; private set; } = -1;
		public bool IsEnabled { get; set; } = true;

		public Neuron StartNode { get; set; } = null;
		public Neuron EndNode { get; set; } = null;
		#endregion

		#region Constructors and Destructors
		public Gene(Neuron inNode, Neuron outNode, float weight = 0, bool isEnabled = true)
		{
			GeneInit(inNode, outNode, weight, isEnabled);
			InnovationNb = GetInnovationNumber(this);
		}

		public Gene(Gene other)
		{
			GeneInit(other.StartNode, other.EndNode, other.Weight, other.IsEnabled);
			InnovationNb = other.InnovationNb;
		}

		public Gene(PackedGene packedGene)
		{
			GeneInit(
				new Neuron(packedGene.inNeuron),
				new Neuron(packedGene.outNeuron),
				packedGene.weight,
				packedGene.isEnabled
			);
			InnovationNb = GetInnovationNumber(this);
		}

		private void GeneInit(Neuron inNode, Neuron outNode, float weight, bool isEnabled)
		{
			StartNode = inNode;
			EndNode = outNode;
			IsEnabled = isEnabled;
			Weight = weight;
		}
		#endregion

		#region public methods
		public override string ToString()
		{
			return  "InnovNb: " + InnovationNb + " " +
					"w: " + Weight.ToString() + " " +
					"In: {" + StartNode.ToString() + "} " +
					"Out: {" + EndNode.ToString() + "}";
		}

		/// <summary>
		/// There is a config.WeightMutateNewValProb chance to
		/// reinitialize the Weight.
		/// </summary>
		public void Mutate(NEATConfig config)
		{
			if (Random.Range(0, 1.0f) < config.weightMutation.NewValProb)
				Weight = config.NewRandomWeight();
			else
				Weight += config.GetAWeightMutation();
		}

		/// <summary>
		/// Disconnect from his neurons removing the references
		/// inside his neurons.
		/// </summary>
		public void Disconnect()
		{
			StartNode.OutGenes.Remove(this);
			EndNode.InGenes.Remove(this);
		}

		/// <summary>
		/// Connect to the given neurons: assign the references inside
		/// these neurons.
		/// </summary>
		public void ConnectToNeurons(Neuron startNode, Neuron endNode)
		{
			if (!startNode.OutGenes.Contains(this))
				startNode.OutGenes.Add(this);

			if (!endNode.InGenes.Contains(this))
				endNode.InGenes.Add(this);

			StartNode = startNode;
			EndNode = endNode;
		}
		#endregion

		#region private methods
		/// <summary>
		/// Get a unique innovation number based on the innovation numbers
		/// of the start and end node.
		/// This is used for historical marking.
		/// </summary>
		private static int GetInnovationNumber(Gene target)
		{
			Gene matchingGene = kindsOfGenes.FirstOrDefault(x =>
				(x.StartNode.InnovationNb == target.StartNode.InnovationNb) &&
				(x.EndNode.InnovationNb == target.EndNode.InnovationNb));

			if (matchingGene != null)
				return matchingGene.InnovationNb;

			kindsOfGenes.Add(target);
			return kindsOfGenes.Count - 1;
		}
		#endregion
	}

	class GeneComparer : EqualityComparer<Gene>
	{
		public override bool Equals(Gene g1, Gene g2) => g1.InnovationNb == g2.InnovationNb;
		public override int GetHashCode(Gene gene) => gene.InnovationNb.GetHashCode();
	}
};
