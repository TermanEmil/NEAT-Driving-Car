#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NEAT
{
	[System.Serializable]
	public class Population
	{
		#region Properties
		public int GenomeCount { get; private set; } = -1;
		public int InputCount { get; private set; } = -1;
		public int OutputCount { get; private set; } = -1;

		public NEATConfig Config { get; private set; } = null;
		public int Generation { get; set; } = 0;

		public List<Genome> Genomes { get; private set; } = new List<Genome>();
		public SpeciesControl SpeciesCtrl { get; set; } = null;
		#endregion

		#region Constructors and Destructors
		public Population(int genomeCount, int inCount, int outCount, NEATConfig config)
		{
			SpeciesCtrl = new SpeciesControl();
			GenomeCount = genomeCount;
			InputCount = inCount;
			OutputCount = outCount;
			Config = config;

			Genomes = new List<Genome>(GenomeCount);
			Populate();
		}
		#endregion

		#region Public methods
		public void Evolve()
		{
			MakeGenomesHaveOnlyPositiveFitnesses();
			SpeciesCtrl.Speciate(Config, Genomes);
			DoSelection();

			if (Genomes.Count <= 0)
			{
				Populate();
				Debug.Log("Repopulate");
				return;
			}

			Genomes = CreateNextGeneration();
			Generation++;
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Create a population with random genomes.
		/// </summary>
		private void Populate()
		{
			for (int i = 0; i < GenomeCount; i++)
				Genomes.Add(new Genome(InputCount, OutputCount, Config.weightInitRandomValue));
		}

		/// <summary>
		/// Remove genomes with no enabled connections.
		/// Remove species if max stagnation is reached, unless
		/// it's the last.
		/// </summary>
		private void DoSelection()
		{
			RemoveBrokenGenomes();
			RemoveRetrospectiveSpecies();
		}

		/// <summary>
		/// Remove genomes with no enabled connections.
		/// </summary>
		private void RemoveBrokenGenomes()
		{
			for (int i = 0; i < Genomes.Count; i++)
				if (Genomes[i].NbOfActiveGenes == 0)
					RemoveGenome(Genomes[i--]);
		}

		/// <summary>
		/// Remove species if max stagnation is reached.
		/// If all species are to be eliminated, keep the best.
		/// A species is said to be retrospective, if its maximum
		/// average fitness didn't improve in the last maxStagnation
		/// generations.
		/// </summary>
		private void RemoveRetrospectiveSpecies()
		{
			Species speciesToKeepForLastResort = null;

			SpeciesCtrl.SpeciesList.ForEach(x => x.UpdateMaxFitness());
			var speciesToEliminate = SpeciesCtrl.SpeciesList.Where(x => (x.DidntImproveInLastGenerations(Config) &&
																		x.AverageFitness < Config.goodFitness) ||
																		x.Genomes.Count <= 0);

			if (speciesToEliminate.Count() == SpeciesCtrl.SpeciesList.Count)
				speciesToKeepForLastResort = speciesToEliminate.OrderByDescending(x => x.AverageFitness).First();

			SpeciesCtrl.SpeciesList = SpeciesCtrl.SpeciesList.Except(speciesToEliminate
															 .Where(x => x != speciesToKeepForLastResort))
															 .ToList();
		}

		/// <summary>
		/// From all species, depending on their average fitness, take Genomes for
		/// the next generation. If a species is found to recive 1 or less population
		/// units, it won't offer anything for the next generation, making this
		/// species to disapper.
		/// </summary>
		private List<Genome> CreateNextGeneration()
		{			
			var fitnessSum = SpeciesCtrl.TotalAverageFitness ;
			var newGeneration = new List<Genome>();
			int availableNumberOfGenomes = Config.genomeCount;
			var bestSpecies = SpeciesCtrl.SpeciesList.OrderByDescending(x => x.AverageFitness).First();

			if (fitnessSum > 0.1f)
				foreach (var species in SpeciesCtrl.SpeciesList.Where(x => x != bestSpecies))
				{
					// Get children or clones from each species.
					int n = (int)(species.AverageFitness / fitnessSum * Config.genomeCount - 1);
					if (n > Config.genomeCount || n < 0)
						break;

					// No species with 0 or less genomes.
					if (n <= 1)
					{
						species.Genomes.Clear();
						continue;
					}
					availableNumberOfGenomes -= n;
					newGeneration.AddRange(species.GetNGenomesForNextGeneration(Config, n));
				}

			if (availableNumberOfGenomes > Config.genomeCount)
				availableNumberOfGenomes = Config.genomeCount - newGeneration.Count;

			if (availableNumberOfGenomes > 0)
				newGeneration.AddRange(bestSpecies.GetNGenomesForNextGeneration(Config, availableNumberOfGenomes));

			if (Config.minNbOfGenomesToKeep <= 0)
				throw new System.Exception("A species has to remember at least one genome.");

			foreach (var species in SpeciesCtrl.SpeciesList)
			{
				int n = Mathf.Max(
					Config.minNbOfGenomesToKeep,
					Mathf.RoundToInt(species.Genomes.Count * Config.genomesToRememberForNextGeneration)
				);
				species.RememberTheBestNGenomes(n);
			}

			if (newGeneration.Count != Config.genomeCount)
				throw new System.Exception(string.Format(
					"Invalid number of genomes in population: {0} given, {1} expected",
                    newGeneration.Count, Config.genomeCount));

			return newGeneration;
		}

		/// <summary>
		/// Eliminate the species with its genomes.
		/// </summary>
		private void EliminateSpecies(Species speciesTarget)
		{
			Genomes.RemoveAll(x => speciesTarget.Genomes.Contains(x));
			SpeciesCtrl.SpeciesList.Remove(speciesTarget);
		}

		/// <summary>
		/// Remove a genome from this and SpeciesCtrl storages.
		/// </summary>
		/// <param name="targetGenome"></param>
		private void RemoveGenome(Genome targetGenome)
		{
			Genomes.Remove(targetGenome);
			SpeciesCtrl.SpeciesList.ForEach(x => x.Genomes.Remove(targetGenome));
		}

		/// <summary>
		/// If the minimum fitness is less than 0, the absolute value of
		/// it is added to every genome, so that they all have positive fitness.
		/// It may be useful in some computations and it doesn't affect anything.
		/// </summary>
		private void MakeGenomesHaveOnlyPositiveFitnesses()
		{
			var minFitness = Genomes.Min(x => x.Fitness);

			if (minFitness > 0)
				return;

			var absMinFitness = Mathf.Abs(minFitness);
			Genomes.ForEach(x => x.Fitness += absMinFitness);
		}
		#endregion
	}
}
