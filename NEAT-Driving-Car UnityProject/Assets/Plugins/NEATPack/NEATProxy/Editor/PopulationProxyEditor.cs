using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NEAT
{
	[CustomEditor(typeof(PopulationProxy), true)]
	public class PopulationProxyEditor : Editor
	{
		private EditorSpeciesCtrlData editorSpeciesCtrlData = new EditorSpeciesCtrlData();
		private PopulationProxy Target { get { return (PopulationProxy)target; } }


		public override void OnInspectorGUI()
		{
			if (Application.isPlaying && Target.gameObject.activeSelf)
			{
				EditorGUILayout.LabelField("Max fitness: ", Target.GenomeProxies.Max(x => x.GenomeProprety.Fitness).ToString());
				EditorGUILayout.LabelField("Avg fitness: ", Target.GenomeProxies.Average(x => x.GenomeProprety.Fitness).ToString());
				EditorGUILayout.LabelField("Generation: ", Target.Popl.Generation.ToString());
				DrawSpeciesCtrl(Target.Popl.SpeciesCtrl, editorSpeciesCtrlData);
			}
			base.OnInspectorGUI();
		}

		private void DrawSpeciesCtrl(SpeciesControl speciesControl, EditorSpeciesCtrlData editorData)
		{			
			editorData.isFoldedOut = EditorGUILayout.Foldout(editorData.isFoldedOut, "Species ctrl:");
			if (!editorData.isFoldedOut)
				return;

			EditorGUI.indentLevel++;

			if (speciesControl.SpeciesList == null)
				return;

			editorData.UpdateArraySize(speciesControl.SpeciesList.Count);

			int i = 0;
			foreach (var species in speciesControl.SpeciesList)
			{
				EditorGUI.indentLevel++;
				DrawSpecies(species, editorData.tab[i]);
				EditorGUI.indentLevel--;
				i++;
			}
			EditorGUI.indentLevel--;
		}

		private void DrawSpecies(Species species, EditorSpeciesData editorSpeciesData)
		{
			editorSpeciesData.isFoldedOut = EditorGUILayout.Foldout(editorSpeciesData.isFoldedOut, "Species count: " + species.Genomes.Count);
			if (!editorSpeciesData.isFoldedOut)
				return;

			EditorGUI.indentLevel++;

			EditorGUILayout.LabelField("AvgFitness: ", species.AverageFitness.ToString());

			editorSpeciesData.UpdateArraySize(species.Genomes.Count);

			for (int i = 0; i < species.Genomes.Count; i++)
				GenomeProxyEditor.DisplayGenome(species.Genomes[i], editorSpeciesData.tab[i]);

			EditorGUI.indentLevel--;
		}
	}

	public class EditorSpeciesCtrlData
	{
		public bool isFoldedOut = false;
		public EditorSpeciesData[] tab = new EditorSpeciesData[0];

		public void UpdateArraySize(int size)
		{
			if (tab.Length != size)
			{
				tab = new EditorSpeciesData[size];
				for (int i = 0; i < size; i++)
					tab[i] = new EditorSpeciesData();
			}
		}
	}

	public class EditorSpeciesData
	{
		public bool isFoldedOut = false;
		public EditorGenomeData[] tab = new EditorGenomeData[0];

		public void UpdateArraySize(int size)
		{
			if (tab.Length != size)
			{
				tab = new EditorGenomeData[size];
				for (int i = 0; i < size; i++)
					tab[i] = new EditorGenomeData();
			}
		}
	}
}
