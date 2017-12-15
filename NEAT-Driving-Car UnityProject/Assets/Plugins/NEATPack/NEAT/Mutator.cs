using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NEAT
{
	/// <summary>
	/// A helper class for Genomes.
	/// </summary>
	public class Mutator
	{
		/// <summary>
		/// Mutates the given genome.
		/// First, try to mutate the structure. Only one
		/// structural mutation can happen at a time.
		/// Then, mutate the genome's weights, each
		/// genome has a chance to be modified.
		/// </summary>
		/// <param name="gradient">The mutation chances are multiplied by this gradient.</param>
		/// <returns>The same genome, but mutated.</returns>
		public static Genome Mutate(NEATConfig config, Genome genome, float gradient = 1f)
		{
			float rand = Random.Range(0, 1f);

			// Mutate structure. It's a weighted random choice.
			if (rand <= config.structMutation.connectionAddProb * gradient)
				MutateAddGene(config, genome);
			else if ((rand -= config.structMutation.connectionAddProb) <= config.structMutation.connectionDelProb * gradient)
				MutateDelGene(genome);
			else if ((rand -= config.structMutation.connectionDelProb) <= config.structMutation.nodeAddProb * gradient)
				MutateAddNode(genome);
			else if ((rand -= config.structMutation.nodeAddProb) <= config.structMutation.nodeDelProb * gradient)
				MutateDelNode(genome);

			// Mutate weight values.
			foreach (var gene in genome.Genes)
				if (Random.Range(0, 1.0f) <= config.weightMutation.prob * gradient)
					gene.Mutate(config);

			return genome;
		}

		/// <summary>
		///	Chapter 3.1 in the NEAT docs.
		///	Add a new node by 'spliting' a gene in two parts. The first part
		///	recives a weight of 1 and the second part recives the previous weight so
		///	that the starting effect of the new node is minimized. The 'splited' gene
		///	is disabled.
		/// </summary>
		public static Neuron MutateAddNode(Genome genome)
		{
			Gene[] availableGenes = genome.Genes.Where(x => x.IsEnabled).ToArray();

			if (availableGenes == null || availableGenes.Length == 0)
				return null;

			var targetGene = availableGenes[Random.Range(0, availableGenes.Length)];
			targetGene.IsEnabled = false;

			var newNeuron = new Neuron(genome.Neurons.Count);
			var preGene = new Gene(targetGene.StartNode, newNeuron);
			var postGene = new Gene(newNeuron, targetGene.EndNode);

			// Reduce the starting effect of adding this new node.
			preGene.Weight = 1;
			postGene.Weight = targetGene.Weight;

			// Establish the connections between nodes and genes.
			preGene.ConnectToNeurons(targetGene.StartNode, newNeuron);
			postGene.ConnectToNeurons(newNeuron, targetGene.EndNode);

			genome.Genes.Add(preGene);
			genome.Genes.Add(postGene);

			genome.Neurons.Add(newNeuron);

			return newNeuron;
		}

		public static Neuron MutateDelNode(Genome genome)
		{
			var hiddenNeurons = genome.HiddenNeurons;
			if (hiddenNeurons.Count() == 0)
				return null;

			return genome.RemoveNode(hiddenNeurons.ElementAt(Random.Range(0, hiddenNeurons.Count())));
		}

		public static Gene MutateAddGene(NEATConfig config, Genome genome)
		{
			List<Neuron> possibleStartingNodes = new List<Neuron>(genome.Neurons);
			while (possibleStartingNodes.Count > 0)
			{
				Neuron startingNode = possibleStartingNodes[Random.Range(0, possibleStartingNodes.Count)];
				var alreadyConnectedNodes = startingNode.OutNeurons;

				Neuron[] possibleEndNodes = genome.Neurons.Where(x =>
					x.Type != ENeuronType.Input &&
					x.Type != ENeuronType.Bias &&
					!alreadyConnectedNodes.Contains(x)
				).ToArray();

				if (possibleEndNodes.Length != 0)
				{
					Neuron endNeuron = possibleEndNodes[Random.Range(0, possibleEndNodes.Length)];
					var newConnection = new Gene(
						startingNode,
						endNeuron,
						(config == null) ? 0 : config.NewRandomWeight());

					startingNode.OutGenes.Add(newConnection);
					endNeuron.InGenes.Add(newConnection);
					genome.Genes.Add(newConnection);
					return newConnection;
				}
				else
					possibleStartingNodes.Remove(startingNode);
			}
			return null;
		}

		public static void MutateDelGene(Genome genome)
		{
			if (genome.Genes == null || genome.Genes.Count == 0)
				return;

			genome.RemoveGene(genome.Genes[Random.Range(0, genome.Genes.Count)]);
		}

		public static void MutateConnection(NEATConfig config, Genome genome)
		{
			foreach (var gene in genome.Genes)
				gene.Mutate(config);
		}
	}
}
