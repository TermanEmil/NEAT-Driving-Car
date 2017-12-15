#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

namespace NEAT
{
	public class Genome
	{
		#region Fields
		// The species in which this genome is.
		public Species SpeciesId { get; set; } = null;

		public int InputCount => Neurons.Count(x => x.Type == ENeuronType.Input);
		public int OutputCount => Neurons.Count(x => x.Type == ENeuronType.Output);

		public List<Gene> Genes { get; private set; } = null;
		public int NbOfActiveGenes => Genes.Count(x => x.IsEnabled);

		public float Fitness { get; set; }

		// The first Neurons are input and output neurons.
		public List<Neuron> Neurons { get; private set; } = null;
		public IEnumerable<Neuron> InputNeurons { get; private set; } = null;
		public IEnumerable<Neuron> OutputNeurons { get; private set; } = null;
		public Neuron BiasNeuron => Neurons.FirstOrDefault(x => x.Type == ENeuronType.Bias);

		public IEnumerable<Neuron> HiddenNeurons => Neurons.Where(x => x.Type != ENeuronType.Input &&
																	   x.Type != ENeuronType.Output &&
																	   x.Type != ENeuronType.Bias);

		// Private internal.
		HashSet<Neuron> solvedNeurons = new HashSet<Neuron>();
		#endregion

		#region Constructors and Destructors
		public Genome(int inputCount, int outputCount, float geneWeightRandomVal = 0)
		{
			if (inputCount < 1 || outputCount < 1)
				throw new System.ArgumentException("Invalid initialization", "inputCount or outputCount");

			SpeciesId = null;
			Fitness = 0;

			InitStartingNeurons(inputCount, outputCount);
			InitStartingGenes(geneWeightRandomVal);

			Assert.AreEqual(InputNeurons.Count(), inputCount);
			Assert.AreEqual(OutputNeurons.Count(), outputCount);
			Assert.AreEqual(Neurons.Count, inputCount + outputCount + 1);
			Assert.AreEqual(Genes.Count, inputCount * outputCount);
		}

		public Genome(Genome target, List<Gene> otherGenes = null)
		{
			SpeciesId = target.SpeciesId;
			Fitness = target.Fitness;
			CreateNetwork(target.InputCount, target.OutputCount, otherGenes ?? target.Genes);

			Assert.AreEqual(InputNeurons.Count(), target.InputCount);
			Assert.AreEqual(OutputNeurons.Count(), target.OutputCount);
		}

		public Genome(NEATConfig config, PackedGenome packedGenome)
		{
			SpeciesId = null;
			Fitness = 0;
			CreateNetwork(config.inputCount,
				config.outputCount,
				packedGenome.genes.Select(x => new Gene(x))
			);
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Find the same kind of gene in its genes, if there is one.
		/// </summary>
		public Gene GetGene(int innovationNb)
		{
			if (Genes == null)
				return null;

			return Genes.FirstOrDefault(x => x.InnovationNb == innovationNb);
		}

		public Gene GetGene(Gene targetGene)
		{
			if (targetGene == null)
				throw new System.ArgumentException("Target Gene can't be null.");

			return GetGene(targetGene.InnovationNb);
		}

		/// <summary>
		///	Feed inputs into the neural network.
		///	Starting from the output Neurons, solve all necessary neurons,
		///	including recurent sequences.
		///	The input and bias neurons are considered solved.
		/// </summary>
		/// <param name="inputs"> The given inner values of the Input layer. </param>
		public float[] FeedNeuralNetwork(NEATConfig config, float[] inputs)
		{
			var inputCount = InputCount;

			if (inputs.Length != inputCount)
				throw new System.Exception(string.Format("Invalid number of inputs, {0} given {1} expected",
					inputs.Length, inputCount));

			int i = 0;
			foreach (var inputNeuron in InputNeurons)
				inputNeuron.InnerValue = inputs[i++];

			solvedNeurons.Clear();

			foreach (var outNeuron in OutputNeurons)
				FeedNeuralNetworkRecursively(config.activationFunction, outNeuron, solvedNeurons);

			return CollectNetworkOutputs();
		}

		public Genome Mutate(NEATConfig config, float gradient = 1f)
		{
			return Mutator.Mutate(config, this, gradient);
		}

		/// <summary>
		/// Remove gene from the network with all its dependencies.
		/// </summary>
		public Gene RemoveGene(Gene targetGene)
		{
			targetGene.Disconnect();
			Genes.Remove(targetGene);
			return targetGene;
		}

		/// <summary>
		/// Remove a neuron from the network, including its connections.
		/// </summary>
		public Neuron RemoveNode(Neuron targetNeuron)
		{
			var genesToRemove = Genes.Where(x => x.StartNode == targetNeuron || x.EndNode == targetNeuron);
			foreach (var gene in genesToRemove)
				gene.Disconnect();

			Genes = Genes.Except(genesToRemove).ToList();
			Neurons.Remove(targetNeuron);
			return targetNeuron;

		}
		#endregion

		#region Static methods
		/// <summary>
		/// Tell how different are 2 genomes. This is used in speciation.
		/// </summary>
		/// <returns>A normalized float.</returns>
		public static float GeneticDistance(NEATConfig config, Genome genome1, Genome genome2)
		{
			var distance = 0f;
			var comparisonInfo = new GenomeComparisonInfo(genome1, genome2);
			var genesCountInBiggest = Mathf.Max(genome1.Genes.Count, genome2.Genes.Count);

			var disjointDist = comparisonInfo.Disjoint.Count * config.speciesCompatibility.disjointImpact;
			var excessDist = comparisonInfo.Excess.Count * config.speciesCompatibility.excessImpact;

			distance += disjointDist / genesCountInBiggest;
			distance += excessDist / genesCountInBiggest;

			float totalWeightDif = 0;
			foreach (var gene in comparisonInfo.Matching)
			{
				var weight1 = Mathf.Abs(genome1.GetGene(gene).Weight);
				var weight2 = Mathf.Abs(genome2.GetGene(gene).Weight);
				var diff = Mathf.Abs(weight1 - weight2) / Mathf.Max(weight1, weight2);
				totalWeightDif += diff;
			}

			distance += totalWeightDif / comparisonInfo.Matching.Count * config.speciesCompatibility.weightMeanDifCoef;
			distance /= config.speciesCompatibility.TotalImpactSum;
			return distance;
		}

		/// <summary>
		/// Create a new offspring by combining the 2 parents' genes.
		/// The Matching genes are already randomly chosen from parent1 or p2.
		/// </summary>
		/// <returns>A fresh Genome (not mutated yet).</returns>
		public static Genome Crossover(NEATConfig config, Genome parent1, Genome parent2)
		{
			var fittestParent = (parent1.Fitness > parent2.Fitness) ? parent1 : parent2;
			var genesToInherit = new List<Gene>(fittestParent.Genes.Count);
			var comparisonInfo = new GenomeComparisonInfo(parent1, parent2);

			// The matching genes are chosen randomly from target1 or target2
			genesToInherit.AddRange(comparisonInfo.Matching.Select(x => new Gene(x)));

			// Add the disjoint genes from the fittests.
			genesToInherit.AddRange(comparisonInfo.Disjoint.Where(x => x.Value == fittestParent)
														   .Select(x => new Gene(x.Key)));

			// Add the excess genes from the fittests.
			genesToInherit.AddRange(comparisonInfo.Excess.Where(x => x.Value == fittestParent)
														 .Select(x => new Gene(x.Key)));

			return new Genome(fittestParent, genesToInherit);
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Initialize the network with input, output and a bias neurons.
		/// </summary>
		private void AddStartingNeurons(int inputCount, int outputCount)
		{
			int i;

			for (i = 0; i < inputCount; i++)
				Neurons.Add(new Neuron(i, ENeuronType.Input));

			for (i = inputCount; i < inputCount + outputCount; i++)
				Neurons.Add(new Neuron(i, ENeuronType.Output));

			Neurons.Add(new Neuron(i, ENeuronType.Bias));
			Debug.Assert(Neurons[i].Type == ENeuronType.Bias);
		}

		private void InitStartingNeurons(int inputCount, int outputCount)
		{
			int i;

			Neurons = new List<Neuron>(inputCount + outputCount + 1);
			AddStartingNeurons(inputCount, outputCount);

			InputNeurons = Neurons.Take(inputCount);
			OutputNeurons = Neurons.GetRange(inputCount, outputCount);
		}

		/// <summary>
		/// Initialize the network with the starting genes.
		/// All inputs, but the bias, will be connected to output neurons.
		/// </summary>
		private void InitStartingGenes(float geneWeightRandomVal)
		{
			Genes = new List<Gene>(InputCount * OutputCount);
			var inputNeurons = InputNeurons.ToArray();
			var outputNeurons = OutputNeurons.ToArray();
			for (int i = 0; i < InputCount; i++)
				for (int j = 0; j < OutputCount; j++)
				{
					Gene newGene = new Gene(
						inputNeurons[i],
						outputNeurons[j],
						Random.Range(-geneWeightRandomVal, geneWeightRandomVal)
					);

					Genes.Add(newGene);
					inputNeurons[i].OutGenes.Add(newGene);
					outputNeurons[j].InGenes.Add(newGene);
				}
		}

		/// <summary>
		///	Calculate recursively inner value for each Neuron which points to target.
		///	The target is immedediatly added to the solvedNeurons, so that
		///	it's possible to solve recurent sequences.
		/// </summary>
		private static void FeedNeuralNetworkRecursively(
			System.Func<float, float> activationF,
			Neuron target,
			HashSet<Neuron> solvedNeurons)
		{
			float sum = 0;

			solvedNeurons.Add(target);
			if (target.Type == ENeuronType.Input || target.Type == ENeuronType.Bias)
				return;

			foreach (var gene in target.InGenes)
			{
				Debug.Assert(gene.EndNode == target);

				if (!gene.IsEnabled)
					continue;

				if (!solvedNeurons.Contains(gene.StartNode))
					FeedNeuralNetworkRecursively(activationF, gene.StartNode, solvedNeurons);

				sum += gene.Weight * gene.StartNode.InnerValue;
				Debug.Assert(solvedNeurons.Contains(gene.StartNode));
			}
			target.InnerValue = activationF(sum);
		}

		/// <summary>
		/// Create a network from the given genes without any prerequesties.
		/// </summary>
		/// <param name="targetGenes"></param>
		private void CreateNetwork(int inputCount, int outputCount, IEnumerable<Gene> targetGenes)
		{
			Neurons = new List<Neuron>();
			Genes = new List<Gene>(targetGenes.Count());

			AddStartingNeurons(inputCount, outputCount);
			foreach (var gene in targetGenes)
				Genes.Add(ForceConnection(gene));

			InputNeurons = Neurons.Where(x => x.Type == ENeuronType.Input);
			OutputNeurons = Neurons.Where(x => x.Type == ENeuronType.Output);

			Debug.Assert(Genes.Count == targetGenes.Count());
			Debug.Assert(InputNeurons.Count() == inputCount);
			Debug.Assert(OutputNeurons.Count() == outputCount);
		}

		/// <summary>
		/// Forcefully create a connection creating the required neurons.
		/// </summary>
		private Gene ForceConnection(Gene targetGene)
		{
			var startNode = ForceGetNeuron(targetGene.StartNode);
			var endNode = ForceGetNeuron(targetGene.EndNode);
			var resultingGene = new Gene(startNode, endNode, targetGene.Weight, targetGene.IsEnabled);

			resultingGene.ConnectToNeurons(startNode, endNode);
			return resultingGene;
		}

		/// <summary>
		/// Forces the genome to get a neuron with the same innovation nb
		/// as the targetNeuron. If there isn't one, create.
		/// </summary>
		private Neuron ForceGetNeuron(Neuron targetNeuron)
		{
			var result = Neurons.FirstOrDefault(x => x.InnovationNb == targetNeuron.InnovationNb);

			if (result == null)
			{
				result = new Neuron(targetNeuron);
				Neurons.Add(result);
			}
			return result;
		}

		/// <summary>
		/// Arrange the network output into an array of floats.
		/// </summary>
		private float[] CollectNetworkOutputs()
		{
			float[] result = new float[OutputCount];

			int i = 0;
			foreach (var outNeuron in OutputNeurons)
				result[i++] = outNeuron.InnerValue;

			return result;
		}
		#endregion
	}
};
