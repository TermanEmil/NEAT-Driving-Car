using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NEAT
{
	[CustomEditor(typeof(GenomeProxy), true)]
	public class GenomeProxyEditor : Editor
	{

		EditorGenomeData editorGenomeData = new EditorGenomeData();

		private GenomeProxy Target { get { return (GenomeProxy)target; } }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (Application.isPlaying && Target.gameObject.activeSelf)
			{
				RepresentMeOnGraph();
				if (GUILayout.Button("Kill"))
					Target.IsDone = true;
				EditorGUILayout.LabelField("Fitness: ", Target.GenomeProprety.Fitness.ToString());
				EditorGUILayout.LabelField("Hidden Nodes: ", Target.GenomeProprety.HiddenNeurons.Count().ToString());
				EditorGUILayout.LabelField("Genes: ", Target.GenomeProprety.Genes.Count.ToString());

				DisplayGenome(Target.GenomeProprety, editorGenomeData);
			}
		}

		private void RepresentMeOnGraph()
		{
			if (GUILayout.Button("Draw on graph"))
			{
				var graphDrawer = FindObjectOfType<NEATGraph>();
				graphDrawer.genomeProxyToDraw = Target;
				graphDrawer.DrawGivenGenomeProxy();
			}
		}

		public static void DisplayGenome(Genome genome, EditorGenomeData editoGenomeData)
		{
			editoGenomeData.isFoldedOut = EditorGUILayout.Foldout(editoGenomeData.isFoldedOut, "Genome ");
			if (!editoGenomeData.isFoldedOut)
				return;

			EditorGUI.indentLevel++;

			editoGenomeData.UpdateArraySize(genome.Neurons.Count);
			for (int i = 0; i < genome.Neurons.Count; i++)
				DisplayNode(genome.Neurons[i], editoGenomeData.tab[i]);
			EditorGUI.indentLevel--;
		}

		public static void DisplayNode(Neuron neuron, EditorNeuronData editorNeuronData)
		{
			editorNeuronData.isFoldedOut = EditorGUILayout.Foldout(editorNeuronData.isFoldedOut, "Neuron " + neuron.InnovationNb);
			if (!editorNeuronData.isFoldedOut)
				return;

			EditorGUI.indentLevel++;
			EditorGUILayout.EnumPopup("Type:", neuron.Type);
			EditorGUILayout.LabelField("InGenes:");

			editorNeuronData.UpdateArraySize(neuron.InGenes.Count + neuron.OutGenes.Count);

			int counter = 0;

			foreach (var gene in neuron.InGenes)
			{
				EditorGUI.indentLevel++;
				DisplayGene(gene, editorNeuronData.tab[counter]);
				EditorGUI.indentLevel--;
				counter++;
			}

			EditorGUILayout.LabelField("OutGenes:");

			foreach (var gene in neuron.OutGenes)
			{
				EditorGUI.indentLevel++;
				DisplayGene(gene, editorNeuronData.tab[counter]);
				EditorGUI.indentLevel--;
				counter++;
			}

			EditorGUI.indentLevel--;
		}

		public static void DisplayGene(Gene gene, EditorGeneData editorGeneData)
		{
			editorGeneData.isFoldedOut = EditorGUILayout.Foldout(editorGeneData.isFoldedOut, "Gene: " + gene.InnovationNb);
			if (!editorGeneData.isFoldedOut)
				return;

			EditorGUI.indentLevel++;

			EditorGUILayout.Toggle("Is enabled: ", gene.IsEnabled);
			EditorGUILayout.LabelField("InNode: " + gene.StartNode.InnovationNb);
			EditorGUILayout.LabelField("OutNode: " + gene.EndNode.InnovationNb);

			EditorGUI.indentLevel--;
		}
	}

	public class EditorGenomeData
	{
		public EditorNeuronData[] tab = new EditorNeuronData[0];
		public bool isFoldedOut = false;

		public void UpdateArraySize(int size)
		{
			if (tab.Length != size)
			{
				tab = new EditorNeuronData[size];
				for (int i = 0; i < size; i++)
					tab[i] = new EditorNeuronData();
			}
		}
	}

	public class EditorNeuronData
	{
		public EditorGeneData[] tab = new EditorGeneData[0];
		public bool isFoldedOut = false;

		public void UpdateArraySize(int size)
		{
			if (tab.Length != size)
			{
				tab = new EditorGeneData[size];
				for (int i = 0; i < size; i++)
					tab[i] = new EditorGeneData();
			}
		}
	}

	public class EditorGeneData
	{
		public bool isFoldedOut = false;
	}
}
