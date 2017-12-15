#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NEAT
{
	public enum ENeuronType
	{
		Bias, Input, Output, Hidden
	}

	public class Neuron
	{
		#region Fields
		// The node value.
		private float innerValue = Mathf.Infinity;
		#endregion

		#region Public Propreties
		public int InnovationNb { get; private set; } = -1;
		public ENeuronType Type { get; set; } = ENeuronType.Hidden;

		// The genes entering this node.
		public List<Gene> InGenes { get; private set; } = null;

		// The genes leaving this node.
		public List<Gene> OutGenes { get; private set; } = null;

		// The Neurons that this neuron leads.
		public IEnumerable<Neuron> OutNeurons => OutGenes.Select(x => x.EndNode);

		// The value of the neuron. It's alway 1 if it's bias.
		public float InnerValue
		{
			get { return (Type == ENeuronType.Bias) ? 1 : innerValue; }
			set { innerValue = value; }
		}
		#endregion

		#region Constructors and Destructors
		public Neuron(int inovationNb, ENeuronType type = ENeuronType.Hidden)
		{
			NeuronInit(inovationNb, type);
		}

		public Neuron(Neuron other)
		{
			NeuronInit(other.InnovationNb, other.Type);
			InnerValue = other.InnerValue;
		}

		public Neuron(PackedNeuron packedNeuron)
		{
			NeuronInit(packedNeuron.innovationNb, packedNeuron.type);
			InnerValue = 0;
		}

		private void NeuronInit(int inovationNb, ENeuronType type)
		{
			InnerValue = 0;
			InnovationNb = inovationNb;
			Type = type;

			InGenes = new List<Gene>();
			OutGenes = new List<Gene>();
		}
		#endregion

		#region public methods
		public override string ToString()
		{
			return "InnovNb: " + InnovationNb + " " +
				   "InnerVal: " + InnerValue + " " +
				   "Type: " + Type.ToString();
		}
		#endregion
	}
};
