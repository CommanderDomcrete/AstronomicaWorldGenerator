using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorEditor : Editor
{
    public override void OnInspectorGUI() {
        // Reference to the target script
        PlanetGenerator planetGenerator = (PlanetGenerator)target;

        // Header Section
        EditorGUILayout.LabelField("Planet Settings", EditorStyles.boldLabel);

        // Exposed parameters
        planetGenerator.radius = EditorGUILayout.FloatField("Radius", planetGenerator.radius);
        planetGenerator.previewResolution = EditorGUILayout.IntField("Preview Resolution", planetGenerator.previewResolution);

        // Add some space
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("LOD Settings", EditorStyles.boldLabel);

        planetGenerator.maxResolution = EditorGUILayout.IntField("Max Resolution", planetGenerator.maxResolution);
        planetGenerator.faceSize = EditorGUILayout.FloatField("Face Size", planetGenerator.faceSize);

        // Debug visualization toggle
        EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
        planetGenerator.debugViewEnabled = EditorGUILayout.Toggle("Enable Debug View", planetGenerator.debugViewEnabled);

        EditorGUILayout.Space();

        // Generate Planet Button
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Planet")) {
            planetGenerator.Initialize();
            planetGenerator.GenerateGeodesicSphere(planetGenerator.GetComponent<MeshFilter>().sharedMesh);

        }
    }
}
