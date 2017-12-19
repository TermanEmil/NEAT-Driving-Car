#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NEAT;
using System.Linq;

public class NEATGraph : MonoBehaviour
{
	[SerializeField] private float weightSize = 1;
	[SerializeField] private float maxWeight = 20;
	[SerializeField] private float connectionWidth = 0.5f;
	[SerializeField] private Color positiveWeightColor = Color.green;
	[SerializeField] private Color negativeWeightColor = Color.red;
	[SerializeField] private Color disabledColor = Color.black;

	[SerializeField] GameObject nodePrefab = null;
	[SerializeField] Transform nodesParent = null;

	[SerializeField] private Transform inputNodesPos = null;
	[SerializeField] private Transform outputNodesPos = null;
	[SerializeField] private Transform bottom = null;

	[SerializeField] private float distBetweenNodes = 2;

	[Header("Live Draw Parameters")]
	public int newGenomeInputCount = 2;
    public int newGenomeOutputCout = 1;
    public int hiddenNodes = 10;
	public int connectionMutations = 5;
    public float newGenomeRandomWeight = 10;

	[Header("Draw from given Genome")]
	public GenomeProxy genomeProxyToDraw;

	private Dictionary<Neuron, RectTransform> nodes = new Dictionary<Neuron, RectTransform>();

	public void DrawGenome(Genome genome)
	{
		RemoveAllNodes();

		int count = 0;
		foreach (var neuron in genome.Neurons)
		{
			var newNode = Instantiate(nodePrefab, nodesParent).GetComponent<RectTransform>();
			newNode.name = "Node " + count;

			var nodeText = neuron.InnovationNb.ToString();
			if (neuron.Type == ENeuronType.Bias)
				nodeText = "bias" + nodeText;
			else if (neuron.Type == ENeuronType.Input)
				nodeText = "in" + nodeText;
			else if (neuron.Type == ENeuronType.Output)
				nodeText = "out" + nodeText;

			newNode.GetChild(0).GetComponent<Text>().text = nodeText;

			nodes.Add(neuron, newNode);
			count++;
		}

		var inputNeurons = genome.InputNeurons.ToArray();
		var outputNeurons = genome.OutputNeurons.ToArray();
		foreach (var node in nodes)
		{
			ConnectNode(node.Value, node.Key, nodes, genome);
			if (genome.InputNeurons.Contains(node.Key))
			{
				int i = System.Array.IndexOf(inputNeurons, node.Key) + 1;
				node.Value.localPosition = inputNodesPos.localPosition + Vector3.down * distBetweenNodes * i;
			}
			else if (genome.OutputNeurons.Contains(node.Key))
			{
				int i = System.Array.IndexOf(outputNeurons, node.Key);
				node.Value.localPosition = outputNodesPos.localPosition + Vector3.down * distBetweenNodes * i;
			}
			else if (genome.BiasNeuron == node.Key)
			{
				node.Value.localPosition = inputNodesPos.localPosition;
			}
			else
			{
				node.Value.localPosition = GetPosOfNewNode();
			}
		}
	}

	public void DrawRandomFromParameters()
	{
		DrawGenome(RandomGenome(
			newGenomeInputCount,
			newGenomeOutputCout,
			hiddenNodes,
			connectionMutations,
			newGenomeRandomWeight
		));
	}

	public void DrawGivenGenomeProxy()
	{
		if (genomeProxyToDraw == null)
		{
			Debug.LogError("No genome proxy was given");
			return;
		}
		RemoveAllNodes();
		DrawGenome(genomeProxyToDraw.GenomeProprety);
	}

	public void RemoveAllNodes()
	{
		var children = new List<GameObject>();
		foreach (Transform child in nodesParent)
			children.Add(child.gameObject);
		children.ForEach(child =>
		{ 
			if (Application.isEditor)
				GameObject.DestroyImmediate(child);
			else
				Destroy(child);
		});

		nodes.Clear();
		ConnectionManager.CleanConnections();
		nodes.Clear();
	}

	// Create a ne random genome from the given parameters.
	public Genome RandomGenome(
		int inputs = 2,
		int outputs = 2,
		int hiddenNodes = 5,
		int connectionMutations = 5,
		float weight = 10)
	{
		Genome result = new Genome(inputs, outputs, weight);

		for (int i = 0; i < hiddenNodes; i++)
		{
			var neuron = Mutator.MutateAddNode(result);
			if (neuron == null)
				break;
			neuron.InGenes[0].Weight = Random.Range(-weight, weight);
			neuron.OutGenes[0].Weight = Random.Range(-weight, weight);
		}

		var poplProxy = FindObjectOfType<PopulationProxy>();
		var config = poplProxy == null ? poplProxy.Config : new NEATConfig();

		for (int i = 0; i < connectionMutations; i++)
		{
			var gene = Mutator.MutateAddGene(config, result);
			if (gene == null)
				break;
			gene.Weight = Random.Range(-weight, weight);
		}

		return result;
	}

	private void ConnectNode(
		RectTransform target,
		Neuron targetNeuron,
		Dictionary<Neuron, RectTransform> nodes,
		Genome genome)
	{
		foreach (var neuron in targetNeuron.OutGenes.Select(x => x.EndNode))
		{
			if (neuron == targetNeuron)
				continue;

			var conn = ConnectionManager.CreateConnection(target, nodes[neuron]);
			
			conn.points[0].weight = 0.6f;
			conn.points[1].weight = 0.6f;
			
			conn.line.widthMultiplier = connectionWidth;

			conn.points[0].direction = ConnectionPoint.ConnectionDirection.East;
			conn.points[1].direction = ConnectionPoint.ConnectionDirection.West;
			
			Gene gene = targetNeuron.OutGenes.First(x => x.EndNode == neuron);
			Color color = GetColor(gene);
			conn.points[0].color = color;
			conn.points[1].color = color;

			if (!gene.IsEnabled)
				conn.line.widthMultiplier *= 0.3f;
		}
	}

	private Color GetColor(Gene gene)
	{
		Color result;

		if (gene.IsEnabled)
		{
			float gradient = (gene.Weight + maxWeight) / (2 * maxWeight);
			result = Color.Lerp(negativeWeightColor, positiveWeightColor, gradient);
			
			if (gene.Weight >= 0)
				result.a = gene.Weight / maxWeight;
			else
				result.a = -gene.Weight / maxWeight;
		}
		else
		{
			result = disabledColor;
		}

		return result;
	}

	private Vector3 GetPosOfNewNode(int tries = 5)
	{
		var result = Vector3.zero;

		do
		{
			tries--;
			result.x = Random.Range(inputNodesPos.localPosition.x, outputNodesPos.localPosition.x);
			result.y = Random.Range(inputNodesPos.localPosition.y, bottom.localPosition.y);
		} while (PosIsTooNearToAnything(result) && tries > 0);

		return result;
	}

	private bool PosIsTooNearToAnything(Vector3 pos)
	{
		foreach(var node in nodes)
		{
			if (Vector3.Distance(pos, node.Value.localPosition) <= distBetweenNodes)
				return true;
		}
		return false;
	}
}
