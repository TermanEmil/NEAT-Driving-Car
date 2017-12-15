using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMenuAnim : MonoBehaviour
{
	[SerializeField] private float timeToDeactivateAfterMouseExit = 1;
	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void OnMouseEnter()
	{
		StopAllCoroutines();
		animator.SetTrigger("activated");
	}

	public void OnMouseExit()
	{
		StopAllCoroutines();
		StartCoroutine(DeactivateMenu(timeToDeactivateAfterMouseExit));
	}

	IEnumerator DeactivateMenu(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		animator.SetTrigger("idle");
	}
}
