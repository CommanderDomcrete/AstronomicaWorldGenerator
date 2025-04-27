using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorEditor : Editor {
    PlanetGenerator planetGenerator;
    Editor geometryEditor;
    Editor continentEditor;
    public override void OnInspectorGUI() {
        using (var check = new EditorGUI.ChangeCheckScope()) {
            base.OnInspectorGUI();
            if(check.changed) {
                planetGenerator.GeneratePlanet();
            }
        }
        DrawSettingsEditor(planetGenerator.geometrySettings, planetGenerator.OnGeometrySettingsUpdated, ref planetGenerator.geometrySettingsFoldout, ref geometryEditor);
        DrawSettingsEditor(planetGenerator.continentMaskSettings, planetGenerator.OnContinentMaskSettingsUpdated, ref planetGenerator.continentMaskSettingsFoldout, ref continentEditor);
        /*
        EditorGUILayout.Space();
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
        }
        */
        if (GUILayout.Button("Generate Planet")) {
            planetGenerator.GeneratePlanet();
            planetGenerator.GenerateContinents();
        }
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor) {

        if (settings != null) {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            using (var check = new EditorGUI.ChangeCheckScope()) {

                if (foldout) {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed) {
                        if (onSettingsUpdated != null) {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

    private void OnEnable() {
        planetGenerator = (PlanetGenerator)target;
    }
}
