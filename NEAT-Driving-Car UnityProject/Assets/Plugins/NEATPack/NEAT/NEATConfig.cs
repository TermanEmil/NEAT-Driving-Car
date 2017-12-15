using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.ComponentModel;

namespace NEAT
{
	[System.Serializable]
	public class NEATConfig
	{
		public int genomeCount = 50;
		public int inputCount = 1;
		public int outputCount = 1;

		public System.Func<float, float> activationFunction = Sigmoid;

		// The gene's weight is initialized with a value between
		// -this value and +this value.
		public float weightInitRandomValue = 1.0f;

		// Chance to disable gene if disabled in either parent.
		public float chanceToDisableGeneIfDisabledInEitherParent = 0.75f;

		[Header("Evolution stuff")]
		
		// Max number of generations allowed for a generation to not improve.
		public int maxStagnation = 20;

		// If a species has at least this much fitness, it won't be killed,
		// even if doesn't improve in maxStagnation generations.
		public float goodFitness = 9999999f;
		
		// The percentage of genomes to be remembered for the next
		// generation in a Species.
		public float genomesToRememberForNextGeneration = 0.25f;

		// The minimum number of genomes a species should keep
		// if possible.
		public int minNbOfGenomesToKeep = 2;

		// The gradient that decides how much priority have
		// those with big fitness.
		public float weightedRandomGradient = 1f;

		// The percentage of genomes to be soft copied for the next
		// generation. Soft, because they will be slightly mutated.
		public float partOfGenomesToCopyForNextGenerations = 0.2f;

		public StructMutation structMutation = new StructMutation();
		public WeightMutation weightMutation = new WeightMutation();
		public SpeciesCompatibility speciesCompatibility = new SpeciesCompatibility();

		#region Helpers
		/// <summary>
		/// A random weight with witch a gene starts.
		/// </summary>
		public float NewRandomWeight() => Random.Range(-weightInitRandomValue, weightInitRandomValue);

		/// <summary>
		///  The value with witch a gene weight is mutated.
		/// </summary>
		public float WeightMutationValue() => weightMutation.scale * weightInitRandomValue;

		public float GetAWeightMutation() => Random.Range(-WeightMutationValue(), WeightMutationValue());

		private static float Sigmoid(float x) => 1 / (1 + Mathf.Exp(-x));
		#endregion
	}

	[System.Serializable]
	public class StructMutation
	{
		// Probability to add a new node.
		public float nodeAddProb = 0.2f;

		// Probability to delete a node all its connections.
		public float nodeDelProb = 0.01f;

		// Probability to add a new gene, with a random weight.
		public float connectionAddProb = 0.7f;

		// Probability to delete a gene.
		public float connectionDelProb = 0.1f;
	}

	[System.Serializable]
	public class WeightMutation
	{
		// Chance that the weigt will suffer any changes (In genereal)
		public float prob = 0.8f;

		// Chance that the gene's weight will be modified.
		public float changeValProb = 0.9f;

		// Chance that Gene will recive an entire new weight.
		public float NewValProb => (1 - changeValProb);

		// Weight mutation scale.
		// A mutation value is calucated as: startingMutationValue * scale
		public float scale = 0.1f;
	}

	/// <summary>
	/// Configurations for population speciation.
	/// The calculated genetic distance (genetic compatibility)
	/// should be normalized.
	/// </summary>
	[System.Serializable]
	public class SpeciesCompatibility
	{
		public float excessImpact = 1.0f;
		public float disjointImpact = 1.0f;
		public float weightMeanDifCoef = 3f;

		// If the genetic distance between 2 genomes is
		// less than this treshold, then they are in the
		// same species.
		// Setting it to 1, will cause to have only one species.
		public float threshold = 0.2f;

		public float TotalImpactSum => excessImpact + disjointImpact + weightMeanDifCoef;
	};
}
