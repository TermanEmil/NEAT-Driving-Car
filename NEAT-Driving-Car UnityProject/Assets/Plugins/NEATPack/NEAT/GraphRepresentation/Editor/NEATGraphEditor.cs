using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NEATGraph))]
public class NEATGraphEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        NEATGraph myTarget = (NEATGraph)target;

        bool drawRandom;
        bool reset;
		bool drawGivenGenome;

        GUILayout.BeginHorizontal();
        drawRandom = GUILayout.Button("Draw random");
		drawGivenGenome = GUILayout.Button("Draw given");
		reset = GUILayout.Button("Reset");
        GUILayout.EndHorizontal();

		if (drawRandom)
			myTarget.DrawRandomFromParameters();
		else if (drawGivenGenome)
			myTarget.DrawGivenGenomeProxy();
		else if (reset)
			myTarget.RemoveAllNodes();
    }
}
