using UnityEngine;
using System.Collections.Generic;

public class LODManager : MonoBehaviour {
    public Camera playerCamera;    // Reference to the camera
    private Vector3 playerCameraPos;
    public PlanetGenerator planetGenerator;

    public float maxLODDistance = 100f;   // Distance for lowest LOD
    public float minLODDistance = 10f;  // Distance for highest LOD
    public int maxResolution = 6;        // Highest resolution level
    public int minResolution = 2;        // Lowest resolution level
    private Vector3 centroid;

    private Mesh planetMesh;
    private List<int> triangles;
    private List<Vector3> vertices;

    List<int> assignTriangles = new List<int>();
    List<Vector3> assignVertices = new List<Vector3>();
    
    public static Dictionary<Vector3,QuadTreeNode> visibleNodes = new Dictionary<Vector3, QuadTreeNode> ();

    public static Dictionary<int, float> detailLevelDistance = new Dictionary<int, float>()
    {
        {0, Mathf.Infinity },
        {1, 60f },
        {2, 25f },
        {3, 10f },
        {4, 4f }
    };

    private void Start() {
        playerCameraPos = playerCamera.transform.position;
        planetMesh = planetGenerator.gameObject.GetComponent<MeshFilter>().mesh;
        triangles = new List<int>(planetMesh.triangles);
        vertices = new List<Vector3>(planetMesh.vertices);

        if (planetGenerator == null || playerCamera == null) return;

        if (triangles.Count == 0) return;

        for (int i = 0; i < triangles.Count; i += 3) {



            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            assignTriangles.AddRange(new[] { triangles[i], triangles[i + 1], triangles[i + 2] }); // new list to assign each triangle to a node
            assignVertices.AddRange(new[] { vertices[v1], vertices[v2], vertices[v3] });

            centroid = SubdivideFunctions.CalculateCentroid(planetMesh.triangles, i, planetMesh.vertices);
            ConstructTree(assignTriangles, assignVertices);
            assignTriangles.Clear();
            assignVertices.Clear();

        }
        foreach (KeyValuePair<Vector3, QuadTreeNode> visibleNode in visibleNodes) { 
            Debug.Log(visibleNode.Key + " " + visibleNode.Value);
        }
    }
    private void Update() {

        UpdateQuadTree();

        // Colorize triangles based on proximity
        planetGenerator.ColorizeTriangles(playerCameraPos, minLODDistance, maxLODDistance);

        //temp code
        //planetGenerator.gameObject.GetComponent<MeshFilter>().mesh.Clear();

    }

    private void ConstructTree(List<int> nodeTriangles, List<Vector3> nodeVertices) {

        QuadTreeNode parentNode = new QuadTreeNode(null, nodeTriangles, nodeVertices, centroid, 0);

        parentNode.GenerateChildren(nodeTriangles, nodeVertices, planetGenerator.radius, playerCameraPos);
    }

    private void OnDrawGizmos() {
        if (planetGenerator == null || playerCamera == null || !planetGenerator.debugViewEnabled) return;

        Gizmos.color = Color.red; // High-resolution zone
        Gizmos.DrawWireSphere(playerCamera.transform.position, minLODDistance);

        Gizmos.color = Color.yellow; // Medium-resolution zone
        Gizmos.DrawWireSphere (playerCamera.transform.position, maxLODDistance);

    }

    private void UpdateQuadTree() {
        //compare dictionary of leaf nodes to dictionary of all loaded nodes
        // any new ones will be built
        
        
    }

}