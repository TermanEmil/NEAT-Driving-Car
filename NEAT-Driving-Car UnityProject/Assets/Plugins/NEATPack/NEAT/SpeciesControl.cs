using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NEAT
{
    public class SpeciesControl
    {
		public List<Species> SpeciesList { get; set; } = new List<Species>();
		public float TotalAverageFitness => SpeciesList.Count == 0 ? 0 : SpeciesList.Sum(x => x.AverageFitness);
		public Species RandomSpecies => SpeciesList[UnityEngine.Random.Range(0, SpeciesList.Count)];

        public SpeciesControl()
        {
            SpeciesList = new List<Species>();
        }

		/// <summary>
		/// Groups all the genomes in Species, so that they can
		/// develop their own structures, without being 'distracted'
		/// by other local maximums.
		/// </summary>
		public void Speciate(NEATConfig config, List<Genome> genomes)
		{
			foreach (var genome in genomes)
			{
				var compatibleSpecies = GetGenomeSpecies(config, SpeciesList, genome);

				if (compatibleSpecies == null)
				{
					var newSpecies = new Species();
					newSpecies.NewGenomes.Add(genome);
					SpeciesList.Add(newSpecies);
				}
				else
					compatibleSpecies.NewGenomes.Add(genome);
			}

			foreach (var speciesI in SpeciesList)
			{
				speciesI.Genomes = speciesI.NewGenomes;
				speciesI.NewGenomes = new List<Genome>();
				speciesI.Generation++;
			}

			EliminateEmptySpecies();
			AssignSpeciesToGenomes(SpeciesList);
		}

		/// <summary>
		/// Determines the species in which the given genome is located.
		/// Two genomes are in the same species if their genetic distance
		/// is less than the given compatibility treshold.
		/// </summary>
		private Species GetGenomeSpecies(NEATConfig config, List<Species> species, Genome genome)
        {
            if (species == null)
                return null;
            
            foreach (var speciesI in species)
            {
				if (speciesI.Genomes.Count == 0)
					continue;
                var threshold = config.speciesCompatibility.threshold;
                var geneticDist = Genome.GeneticDistance(config, genome, speciesI.Representative);

                if (geneticDist <= threshold)
                    return speciesI;
            }

            return null;
        }

		/// <summary>
		/// Make every genome to know what species they are.
		/// </summary>
		/// <param name="species"></param>
        private void AssignSpeciesToGenomes(List<Species> species)
        {
            foreach (var speciesI in species)
                foreach (var genome in speciesI.Genomes)
                    genome.SpeciesId = speciesI;
        }

		private void EliminateEmptySpecies()
		{
			SpeciesList.RemoveAll(x => x.Genomes.Count == 0);
		}
    }
}
