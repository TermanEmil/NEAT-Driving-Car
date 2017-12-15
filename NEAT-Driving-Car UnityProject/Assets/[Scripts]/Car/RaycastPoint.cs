using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastPoint : MonoBehaviour
{
	[SerializeField] private bool drawGizmos = true;
	[SerializeField] private float size = 3;
	[SerializeField] private Transform rayOrigin = null;

	private void OnDrawGizmos()
	{
		if (!drawGizmos || !GizmosCtrl.Instance.enabledGizmos)
			return;

		Gizmos.DrawWireSphere(transform.position, size);
		Gizmos.DrawWireCube(transform.position, Vector3.one * size);

		if (rayOrigin != null)
			Gizmos.DrawLine(transform.position, rayOrigin.position);
	}
}
