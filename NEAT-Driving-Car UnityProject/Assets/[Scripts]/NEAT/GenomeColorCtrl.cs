using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NEAT;
using System.Linq;

public class GenomeColorCtrl
{
	Dictionary<Species, Color> speciesColor = new Dictionary<Species, Color>();

	/// <summary>
	/// Remove nonexistent species from its dictionary
	/// </summary>
	public void UpdateSpeciesColor(SpeciesControl speciesControl)
	{
		var updatedSpeciesColor = new Dictionary<Species, Color>();

		foreach (var species in speciesControl.SpeciesList)
		{
			if (speciesColor.ContainsKey(species))
				updatedSpeciesColor.Add(species, speciesColor[species]);
			else
				updatedSpeciesColor.Add(species, RandomColor());
		}

		speciesColor = updatedSpeciesColor;
	}

	public Color GetSpeciesColor(Species species)
	{
		if (species == null)
			return Color.white;

		return speciesColor[species];
	}

	private Color RandomColor()
	{
		return new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
	}
}
