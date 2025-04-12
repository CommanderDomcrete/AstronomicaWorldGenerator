using UnityEngine;
using System.Collections.Generic;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetGenerator : MonoBehaviour
{
    [Header("Planet Settings")]
    [SerializeField] public float radius = 10f;
    [SerializeField] public int previewResolution = 3; // controls resolution of the sphere

    [Header("Debug Settings")]
    [SerializeField] public bool debugViewEnabled = true; // Toggle for Debug view

    [Header("LOD Settings")]
    [SerializeField] public int maxResolution = 6;   // Maximum resolution for highest LOD
    [SerializeField] public float faceSize = 5f;   // Size of each quad tree node face

    private MeshFilter meshFilter;
    private Mesh generatedMesh; // Store the mesh for shared access
    private Dictionary<int, int> subdivisionLevels = new Dictionary<int, int>(); // Tracks LOD per triangle
    

    public void Initialize() {  // Check if Mesh Filter component is attached
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) {
            Debug.Log("Planet is missing mesh filter component");
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
    }
    // Generate base sphere mesh
    public void GenerateGeodesicSphere(Mesh mesh) {

        generatedMesh = mesh; // Store the mesh for later use
        // Generate vertices and triangles for spherical geometry here.
        // Generate initial icosahedron (base shape for geodesic sphere).
        List<Vector3> vertices = GenerateIcosahedronPoints();
        List<int> triangles = new List<int>{
        0, 11, 5,
        0, 5, 1,
        0, 1, 7,
        0, 7, 10,
        0, 10, 11,

        1, 5, 9,
        5, 11, 4,
        11, 10, 2,
        10, 7, 6,
        7, 1, 8,

        3, 9, 4,
        3, 4, 2,
        3, 2, 6,
        3, 6, 8,
        3, 8, 9,

        4, 9, 5,
        2, 4, 11,
        6, 2, 10,
        8, 6, 7,
        9, 8, 1
        };


        // Subdivide triangles based on resolution.
        for (int i = 0; i < previewResolution; i++) { // Higher the resolution, the more subdivisions
            SubdivideFunctions.SubdivideTriangles(vertices, triangles);
        }

        // Normalize vertices to create spherical shape.
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i] = vertices[i].normalized * radius;
        }

        // Assign vertices and triangles to the mesh.
        mesh.Clear(); // Clear mesh so we do not have multiple meshes when we regenerate
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private List<Vector3> GenerateIcosahedronPoints() {

        List<Vector3> vertices = new List<Vector3>();
        // Golden ratio calculation for the points of the intersecting rectangles
        float t = (1 + Mathf.Sqrt(5)) / 2;

        vertices.AddRange(new Vector3[]
        {
            new Vector3(-1, t, 0).normalized,
            new Vector3(1, t, 0).normalized,
            new Vector3(-1, -t, 0).normalized,
            new Vector3(1, -t, 0).normalized,

            new Vector3(0, -1, t).normalized,
            new Vector3(0, 1, t).normalized,
            new Vector3(0, -1, -t).normalized,
            new Vector3(0, 1, -t).normalized,

            new Vector3(t, 0, -1).normalized,
            new Vector3(t, 0, 1).normalized,
            new Vector3(-t, 0, -1).normalized,
            new Vector3(-t, 0, 1).normalized

        });
        return vertices;
    }

    public void SetResolution(int newResolution) {
        if (newResolution != previewResolution) {
            previewResolution = newResolution;
            GenerateGeodesicSphere(GetComponent<MeshFilter>().sharedMesh); // Regenerate sphere with new resolution
        }
    }

    private void OnDrawGizmos() {
        if (!debugViewEnabled || generatedMesh == null) {
            GetComponent<MeshRenderer>().sharedMaterial.SetInt("_DebugLOD", 0);
            return;
        }
        GetComponent<MeshRenderer>().sharedMaterial.SetInt("_DebugLOD", 1);
    }

    public void ColorizeTriangles(Vector3 cameraPosition, float minLODDistance, float maxLODDistance) {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        if (mesh == null) return;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < triangles.Length; i += 3) {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            // Calculate the centroid of the triangle
            Vector3 centroid = (vertices[v1] + vertices[v2] + vertices[v3]) / 3f;
            float distanceToCamera = Vector3.Distance(centroid, cameraPosition);

            // Assign color based on distance
            Color triangleColor;
            if (distanceToCamera <= minLODDistance) {
                triangleColor = Color.red; // High LOD
            }
            else if (distanceToCamera <= maxLODDistance) {
                triangleColor = Color.yellow; // Medium LOD
            }
            else {
                triangleColor = Color.green; // Low LOD
            }

            // Apply the same color to all vertices of the triangle
            colors[v1] = triangleColor;
            colors[v2] = triangleColor;
            colors[v3] = triangleColor;
        }

        // Assign the color array to the mesh
        mesh.colors = colors;
    }

}
