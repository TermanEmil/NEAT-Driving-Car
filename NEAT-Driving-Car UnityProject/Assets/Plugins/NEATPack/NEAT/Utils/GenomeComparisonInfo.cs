using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NEAT
{
	public class GenomeComparisonInfo
	{
		public Dictionary<Gene, Genome> Disjoint { get; set; } = null;
		public Dictionary<Gene, Genome> Excess { get; set; } = null;
		public List<Gene> Matching { get; set; } = null;

		public Genome Target1 { get; set; } = null;
		public Genome Target2 { get; set; } = null;

		public GenomeComparisonInfo(Genome target1, Genome target2)
		{
			Target1 = target1;
			Target2 = target2;

			var genes1 = target1.Genes.OrderBy(x => x.InnovationNb).ToArray();
			var genes2 = target2.Genes.OrderBy(x => x.InnovationNb).ToArray();

			Disjoint = new Dictionary<Gene, Genome>();
			Excess = new Dictionary<Gene, Genome>();
			Matching = new List<Gene>();

			var n = Mathf.Max(genes1.Length, genes2.Length);
			var comparer = new GeneComparer();

			for (int i = 0; i < n; i++)
			{
				if (i < genes1.Length)
				{
					var geneFromParent2 = target2.GetGene(genes1[i]);
					if (geneFromParent2 != null)
					{
						if (Random.Range(0, 1f) <= 0.5f)
							Matching.Add(genes1[i]);
						else
							Matching.Add(geneFromParent2);
					}
					else
					{
						if (genes2.Length != 0 && genes1[i].InnovationNb <= genes2.Last().InnovationNb)
							Disjoint.Add(genes1[i], target1);
						else
							Excess.Add(genes1[i], target1);
					}
				}

				if (i < genes2.Length && !genes1.Contains(genes2[i], comparer))
				{
					if (genes1.Length != 0 && genes2[i].InnovationNb <= genes1.Last().InnovationNb)
						Disjoint.Add(genes2[i], target2);
					else
						Excess.Add(genes2[i], target2);
				}
			}
		}
	}
}
