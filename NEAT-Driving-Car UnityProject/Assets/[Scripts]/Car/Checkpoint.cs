using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Checkpoint : MonoBehaviour
{
	public LayerMask layerMask;
	public bool isFinish = false;
	public bool isFitnessPoint = false;
	public float fitnessWhenTouched = 5000;

	public bool isTeleport = false;
	public Transform teleportPosition;

	public List<GenomeCar> crossedBy = new List<GenomeCar>();

	public void Teleport(Transform target)
	{
		target.transform.position = teleportPosition.position;
		target.transform.rotation = teleportPosition.rotation;
		target.GetComponent<Rigidbody>().velocity = Vector3.zero;
	}
}
