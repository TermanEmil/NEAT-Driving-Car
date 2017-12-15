using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NEAT;
using UnityEngine.UI;

public class GenomeCar : GenomeProxy
{
	#region Fields
	private CarController carController = null;

	[SerializeField] private float life = 100;
	[SerializeField] private Text fitnessText = null;
	public SpriteRenderer speciesColor;

	[Header("Raycast stuff")]
	[SerializeField] private LayerMask obstacleLayer = 0;
	[SerializeField] private Transform raycastOrigin = null;
	[SerializeField] private Transform[] raycastEndPoints = null;

	private int finishCross = 0;
	private Vector3 lastPositionMark;
	private PopulationCar populationCar;

	private float currentMaxFitness = 0;
	private float lastMaxFitnessUpdate = 0;

	private List<Checkpoint> checkpointPassed = new List<Checkpoint>();
	#endregion

	#region Monobehaviour
	private void Start()
	{
		carController = GetComponent<CarController>();
		populationCar = FindObjectOfType<PopulationCar>();
	}

	private void Update()
	{
		fitnessText.text = ((int)GenomeProprety.Fitness).ToString();

		life -= CarSettings.Instance.dieSpeed * Time.deltaTime;
		if (life <= 0)
			Die();

		DieIfNotImproving();
	}

	private void FixedUpdate()
	{
		ActivateNeuralNet();
		CalculateFitness();
	}

	private void OnCollisionEnter(Collision collision)
	{
		Die();
	}

	private void OnTriggerEnter(Collider other)
	{
		var checkpoint = other.GetComponent<Checkpoint>();

		if (checkpoint == null)
			return;

		if (checkpoint.isFinish)
			ProcessFinihCross();
		if (checkpoint.isFitnessPoint && !checkpointPassed.Contains(checkpoint))
			ProcessFitnessPointCross(checkpoint);
		if (checkpoint.isTeleport)
			ProcessTeleportCross(checkpoint);
	}
	#endregion

	#region Override
	public override void Init(int id, Population popl)
	{
		base.Init(id, popl);
		AssignGenomeInputFunctions();
	}

	public override void ProcessNetworkOutput(float[] netOutputs)
	{
		base.ProcessNetworkOutput(netOutputs);
		carController.Move(
			vertAxis: Mathf.Lerp(-1f, 1f, netOutputs[0]),
			horizontalAxis: Mathf.Lerp(-1f, 1f, netOutputs[1])
		);
	}
	#endregion

	#region Public Methods
	public void Reinit(Transform targetPositionRotation)
	{
		transform.SetPositionAndRotation(
			targetPositionRotation.position,
			targetPositionRotation.rotation
		);

		lastPositionMark = transform.position;
		gameObject.SetActive(true);

		IsDone = false;
		GenomeProprety.Fitness = 0;

		life = CarSettings.Instance.startingCarLife;
		finishCross = 0;
		checkpointPassed.Clear();

		currentMaxFitness = 0;
		lastMaxFitnessUpdate = Time.time;
	}

	public void Die()
	{
		IsDone = true;
		gameObject.SetActive(false);
	}
	#endregion

	#region Private methods
	private void AddFitness(float fitness)
	{
		GenomeProprety.Fitness += fitness;
		life += fitness;
	}

	/// <summary>
	/// Define the car's inputs.
	/// A for loop can't be used here, since lambda functions are defined.
	/// The last input is the speed.
	/// </summary>
	private void AssignGenomeInputFunctions()
	{
		InputFunctions = new GenomeInputFunction[Popl.Config.inputCount];

		InputFunctions[0] = () => GetNormRaycastHit(raycastEndPoints[0].position);
		InputFunctions[1] = () => GetNormRaycastHit(raycastEndPoints[1].position);
		InputFunctions[2] = () => GetNormRaycastHit(raycastEndPoints[2].position);
		InputFunctions[3] = () => GetNormRaycastHit(raycastEndPoints[3].position);
		InputFunctions[4] = () => GetNormRaycastHit(raycastEndPoints[4].position);
		InputFunctions[5] = () => GetNormRaycastHit(raycastEndPoints[5].position);
		InputFunctions[6] = () => GetNormRaycastHit(raycastEndPoints[6].position);
		InputFunctions[7] = () => GetNormRaycastHit(raycastEndPoints[7].position);
		InputFunctions[8] = () => GetNormRaycastHit(raycastEndPoints[8].position);

		// Speed.
		InputFunctions[9] = () =>
		{
			var forwardVel = transform.InverseTransformDirection(carController.MyRigidbody.velocity).z;
			return Mathf.InverseLerp(populationCar.maxNegativeVelocity, populationCar.maxVelocity, forwardVel);
		};
	}

	/// <summary>
	/// Make a raycast in the given direction and return the distance to the 
	/// first object it hit within the obstacleLayer mask. The distance
	/// is normalized between [0; 1]. In case it didn't hit anything, 1 is
	/// returned.
	/// </summary>
	/// <returns>Normalized dist to the first object detected.</returns>
	private float GetNormRaycastHit(Vector3 endPoint)
	{
		RaycastHit hit;
		bool itHitSomething = Physics.Linecast(raycastOrigin.position, endPoint, out hit, obstacleLayer);

		if (GizmosCtrl.Instance.enableDebugLines && itHitSomething)
			Debug.DrawLine(raycastOrigin.position, hit.point, Color.red);
		
		float dist = (raycastOrigin.position - endPoint).magnitude;
		return itHitSomething ? (hit.point - raycastOrigin.position).magnitude / dist : 1f;
	}

	private bool IsDrivingForward()
	{
		return Mathf.Sign(transform.InverseTransformDirection(carController.MyRigidbody.velocity).z) > 0;
	}

	/// <summary>
	/// The fitness is calculated by saving the last position
	/// and calculating the traveled distance from the last position.
	/// If the care is forward, the fitness is added, otherwise, it's
	/// substracted.
	/// </summary>
	private void CalculateFitness()
	{
		var localVel = transform.InverseTransformDirection(carController.MyRigidbody.velocity);

		float travelDir = Mathf.Sign(localVel.z);
		var fitness = (transform.position - lastPositionMark).magnitude;
		fitness *= CarSettings.Instance.fitnessPerUnit * Time.fixedDeltaTime * travelDir;
		AddFitness(fitness);

		lastPositionMark = transform.position;
	}

	private void DieIfNotImproving()
	{
		if (GenomeProprety.Fitness > currentMaxFitness)
		{
			currentMaxFitness = GenomeProprety.Fitness;
			lastMaxFitnessUpdate = Time.time;
		}

		if (Time.time > lastMaxFitnessUpdate + CarSettings.Instance.maxTimeWithNoFitnessImprovement)
			Die();
	}

	private void ProcessFinihCross()
	{
		if (!IsDrivingForward())
		{
			Die();
			return;
		}

		finishCross++;
		if (finishCross >= CarSettings.Instance.targetFinishCrossTimes)
		{
			var fitnessMult = CarSettings.Instance.finishFitnessMultiplier;

			if (populationCar.firstToCrossFinish == null)
			{
				populationCar.firstToCrossFinish = this;
				fitnessMult *= CarSettings.Instance.fitnessMultiplierForBeingFirst;
			}

			AddFitness(GenomeProprety.Fitness * fitnessMult);
			Die();
		}
	}

	private void ProcessFitnessPointCross(Checkpoint checkpoint)
	{
		checkpointPassed.Add(checkpoint);
		if (IsDrivingForward())
			AddFitness(checkpoint.fitnessWhenTouched);
	}

	private void ProcessTeleportCross(Checkpoint checkpoint)
	{
		if (!IsDrivingForward())
		{
			Die();
			return;
		}

		checkpoint.Teleport(transform);
	}
	#endregion
}
