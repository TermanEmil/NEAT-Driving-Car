#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour {
	static ConnectionManager _instance;
	public static ConnectionManager instance {
		get {
			if (!_instance) {
				//first try to find one in the scene
				_instance = FindObjectOfType<ConnectionManager>();

				if (!_instance) {
					//if that fails, make a new one
					GameObject go = new GameObject("_ConnectionManager");
					_instance = go.AddComponent<ConnectionManager>();

					if (!_instance) {
						//if that still fails, we have a big problem;
						Debug.LogError("Fatal Error: could not create ConnectionManager");
					}
				}
			}

			return _instance;
		}
	}

	[SerializeField] Connection connectionPrefab = null;
	[SerializeField] List<Connection> connections = new List<Connection>();

	public static Connection FindConnection(RectTransform t1, RectTransform t2) {
		if (!instance) return null;

		foreach (Connection connection in instance.connections) {
			if (connection == null) continue;

			if (connection.Match(t1, t2)) {
				return connection;
			}
		}

		return null;
	}

	public static List<Connection> FindConnections(RectTransform transform) {
		List<Connection> found = new List<Connection>();
		if (!instance) return found;

		foreach (Connection connection in instance.connections) {
			if (connection == null) continue;

			if (connection.Contains(transform)) {
				found.Add(connection);
			}
		}

		return found;
	}

	public static void AddConnection(Connection c) {
		if (c == null || !instance) return;

		if (!instance.connections.Contains(c)) {
			c.transform.SetParent(instance.transform);
			instance.connections.Add(c);
		}
	}

	public static void RemoveConnection(Connection c) {
		//don't use the property here. We don't want to create an instance when the scene loads
		if (c != null && _instance != null)
		{
			_instance.connections.Remove(c);
			if (Application.isPlaying)
				Destroy(c.gameObject);
			else
				DestroyImmediate(c.gameObject);
		}
	}

	public static void SortConnections() {
		if (!instance) return;

		instance.connections.Sort((Connection c1, Connection c2) => {return string.Compare(c1.name, c2.name);});
		for (int i = 0; i < instance.connections.Count; i++) {
			instance.connections[i].transform.SetSiblingIndex(i);
		}
	}

	public static void CleanConnections() {
		if (!instance) return;

		//fist clean any null entries
		instance.connections.RemoveAll((Connection c) => {return c == null;});

		foreach (Transform child in instance.transform)
			if (Application.isPlaying)
				Destroy(child.gameObject);
			else
				DestroyImmediate(child.gameObject);

		////copy list because OnDestroy messages will modify the original
		//List<Connection> copy = new List<Connection>(instance.connections);
		//foreach (Connection c in copy) {
		//	if (c && !c.isValid) {
		//		if (!Application.isPlaying)
		//			DestroyImmediate(c.gameObject);
		//		else
		//			Destroy(c.gameObject);
		//	}
		//}
	}

	public static Connection CreateConnection(RectTransform t1, RectTransform t2 = null) {
		if (!instance) return null;
		
		Connection conn;

		if (instance.connectionPrefab) {
			conn = Instantiate<Connection>(instance.connectionPrefab);
		} else {
			conn = new GameObject("new connection").AddComponent<Connection>();
		}

		conn.SetTargets(t1, t2);
		return conn;
	}
}
