using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIControl : MonoBehaviour
{
	[SerializeField] private Text generationsText = null;
	[SerializeField] private Text aliveText = null;
	[SerializeField] private Text speciesText = null;
	[SerializeField] private Toggle fullMapView = null;

	private PopulationCar populationCar;
	private List<NEAT.GenomeProxy> lastDrawnGenomes = new List<NEAT.GenomeProxy>();
	private SmoothFollow smoothFollow;
	private NEATGraph neatGraph;

	private void Awake()
	{
		populationCar = FindObjectOfType<PopulationCar>();
		if (populationCar == null)
			Destroy(gameObject);
	}

	private void Start()
	{
		smoothFollow = FindObjectOfType<SmoothFollow>();
		neatGraph = FindObjectOfType<NEATGraph>();
	}

	public void UpdateGenerationText()
	{
		generationsText.text = "Generations: " + (populationCar.Popl.Generation + 1).ToString();
	}

	public void UpdateSpeciesText()
	{
		speciesText.text = "Species: " + populationCar.Popl.SpeciesCtrl.SpeciesList.Count(x => x.Genomes.Count != 0).ToString();
	}

	public void UpdateAliveText()
	{
		aliveText.text = "Alive: " + populationCar.GenomeProxies.Count(x => !x.IsDone);
	}

	public void SaveBest()
	{
		var best = populationCar.Popl.Genomes.OrderByDescending(x => x.Fitness).ToArray()[0];
		var fileName = NEAT.GenomeSaver.GenerateSaveFilePath(
			NEAT.GenomeSaver.DefaultSaveDir,
			best.Fitness,
			populationCar.Popl.Generation
		);
		NEAT.GenomeSaver.SaveGenome(best, fileName);
		Debug.Log("Saved best genome to: " + fileName);
	}

	public void LoadGenome(InputField filePathInputField)
	{
		try
		{
			if (filePathInputField.text[0] == '"')
				filePathInputField.text = filePathInputField.text.Remove(0, 1);
			if (filePathInputField.text.Last() == '"')
				filePathInputField.text = filePathInputField.text.Remove(filePathInputField.text.Length - 1, 1);

			var firstAlive = populationCar.GenomeProxies.FirstOrDefault(x => !x.IsDone);
			if (firstAlive == null)
			{
				Debug.Log("Couldn't find any genome alive");
				return;
			}

			var newGenome = NEAT.GenomeSaver.LoadGenome(populationCar.Popl.Config, filePathInputField.text);
			newGenome.Fitness = firstAlive.GenomeProprety.Fitness;
			populationCar.Popl.Genomes[firstAlive.Id] = newGenome;

			Debug.Log("New genome has been loaded");
		}
		catch (System.Exception e)
		{
			Debug.Log("Failed to load genome: " + e);
		}
	}

	public void DrawNetwork()
	{
		if (neatGraph == null)
			return;

		if (smoothFollow != null)
		{
			smoothFollow.follow = false;
			smoothFollow.GotoAllMapView();
		}
		fullMapView.isOn = true;

		var genomeToDraw = populationCar.GenomeProxies.OrderByDescending(x => x.GenomeProprety.Fitness)
													  .FirstOrDefault(x => !lastDrawnGenomes.Contains(x));
		if (genomeToDraw == null)
			genomeToDraw = populationCar.GenomeProxies.OrderByDescending(x => x.GenomeProprety.Fitness).First();
		lastDrawnGenomes.Add(genomeToDraw);


		neatGraph.RemoveAllNodes();
		neatGraph.RemoveAllNodes();
		neatGraph.genomeProxyToDraw = genomeToDraw;
		neatGraph.DrawGivenGenomeProxy();
	}

	public void ResetNetworkDisplay()
	{
		if (neatGraph == null)
			return;

		lastDrawnGenomes.Clear();
		neatGraph.RemoveAllNodes();
		neatGraph.RemoveAllNodes();
	}

	public void OnToggleFullMapView()
	{
		if (smoothFollow == null)
			return;

		if (fullMapView.isOn)
		{
			smoothFollow.follow = false;
			smoothFollow.GotoAllMapView();
		}
		else
		{
			smoothFollow.follow = true;
			ResetNetworkDisplay();
		}
	}

	public void KillAll()
	{
		populationCar.KillAll();
	}
}
