#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NEAT
{
	public delegate float GenomeInputFunction();

	/// <summary>
	/// A 'connection' class between the Genome logic
	/// and the actual problem.
	/// This class should calculate the fitness and make use
	/// of the neural network.
	/// </summary>
	public abstract class GenomeProxy : MonoBehaviour
	{
		#region Properties
		public int Id { get; protected set; }
		protected Population Popl { get; set; }

		// An array of input functions for the neural network.
		// Its length has to match NEATConfig.inputCount
		public GenomeInputFunction[] InputFunctions;

		// The Genome from the population it represents.
		public Genome GenomeProprety
		{
			get { return Popl.Genomes[Id]; }
			set { Popl.Genomes[Id] = value; }
		}

		// A general state: indicates if the this
		// genome is done, dead or anything like that.
		public bool IsDone { get; set; }
		#endregion

		#region Public methods
		public virtual void Init(int id, Population popl)
		{
			Id = id;
			Popl = popl;
		}

		/// <summary>
		/// Activate the nerual network, after which the
		/// output is processed.
		/// </summary>
		public void ActivateNeuralNet()
		{
			var netInputs = new float[InputFunctions.Length];
			for (int i = 0; i < InputFunctions.Length; i++)
			{
				netInputs[i] = InputFunctions[i]();
			}

			float[] netOutputs = GenomeProprety.FeedNeuralNetwork(Popl.Config, netInputs);
			ProcessNetworkOutput(netOutputs);
		}

		/// <summary>
		/// This function must be overriden in order to use the network's
		/// outputs.
		/// </summary>
		public virtual void ProcessNetworkOutput(float[] netOutputs)
		{
			Debug.Assert(netOutputs.Length == Popl.Config.outputCount);
		}
		#endregion
	}
}
