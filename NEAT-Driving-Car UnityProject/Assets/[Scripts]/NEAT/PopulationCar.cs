using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NEAT;
using UnityEngine.Events;

public class PopulationCar : PopulationProxy
{
	[SerializeField] private UnityEvent onGenerationChange = null;
	[SerializeField] private UnityEvent onGenomeStatusChange = null;
	[SerializeField] private Transform carSpawnPoint = null;
	public Checkpoint theRealFinish = null;

	public float maxVelocity = 55f;
	public float maxNegativeVelocity = -5f;

	private SmoothFollow cameraFollow = null;
	private List<GenomeCar> cars = null;
	private GenomeColorCtrl genomeColorCtrl = new GenomeColorCtrl();

	public bool EveryoneIsDead => cars.FirstOrDefault(x => !x.IsDone) == null;
	protected override void Awake()
	{
		base.Awake();

		if (carSpawnPoint == null)
		{
			Debug.LogError("Car spawn point can't be null");
			Debug.Break();
		}

		if (cameraFollow == null)
			cameraFollow = FindObjectOfType<SmoothFollow>();
	}

	private void FixedUpdate()
	{
		var cameraTarget = cars.OrderByDescending(x => x.GenomeProprety.Fitness).FirstOrDefault(x => x.gameObject.activeSelf);
		cameraFollow.target = cameraTarget == null ? null : cameraTarget.transform;

		if (EveryoneIsDead)
			Evolve();

		onGenomeStatusChange.Invoke();
	}

	public void KillAll()
	{
		foreach (var car in cars)
			car.Die();
	}

	public void ReinitCars()
	{
		foreach (var car in cars)
			car.Reinit(carSpawnPoint);

		if (theRealFinish != null)
			theRealFinish.crossedBy.Clear();
	}

	public override void Evolve()
	{
		base.Evolve();
		ReinitCars();

		genomeColorCtrl.UpdateSpeciesColor(Popl.SpeciesCtrl);
		foreach (var car in cars)
			car.speciesColor.color = genomeColorCtrl.GetSpeciesColor(car.GenomeProprety.SpeciesId);
		onGenerationChange.Invoke();
	}

	public override void InitPopl()
	{
		base.InitPopl();
		cars = new List<GenomeCar>(Config.genomeCount);
	}

	protected override void InitGenomeProxyObj(GameObject genomeProxy)
	{
		var genomeCar = genomeProxy.GetComponent<GenomeCar>();
		if (cars.FirstOrDefault(x => x.Id == genomeCar.Id) == null)
		{
			cars.Add(genomeCar);
			genomeCar.Reinit(carSpawnPoint);
		}
	}
}
