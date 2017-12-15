using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosCtrl : MonoBehaviour
{
	private static GizmosCtrl instance;
	public static GizmosCtrl Instance { get { return instance ?? (instance = FindObjectOfType<GizmosCtrl>()); } }

	public bool enabledGizmos = false;
	public bool enableDebugLines = false;
}
