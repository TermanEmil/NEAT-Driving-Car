using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSettings : MonoBehaviour
{
	private static CarSettings instance;
	public static CarSettings Instance { get { return instance ?? (instance = FindObjectOfType<CarSettings>()); } }

	[Header("Car life")]
	public float startingCarLife = 600;
	public float dieSpeed = 50;

	[Header("Finish")]
	public int targetFinishCrossTimes = 3;
	public float finishFitnessMultiplier = 2;
	public float fitnessMultiplierForBeingFirst = 2;

	[Header("Other")]
	public float fitnessPerUnit = 10;
	public float maxTimeWithNoFitnessImprovement = 5f;
}
