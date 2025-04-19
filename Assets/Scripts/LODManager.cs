using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static Unity.VisualScripting.Metadata;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class LODManager : MonoBehaviour {
    public Camera playerCamera;    // Reference to the camera
    public PlanetGenerator planetGenerator;

    public float maxLODDistance = 100f;   // Distance for lowest LOD
    public float minLODDistance = 10f;  // Distance for highest LOD
    public int maxResolution = 6;        // Highest resolution level
    public int minResolution = 2;        // Lowest resolution level

    private float checkTimer = 0f; // Timer for checking LOD updates

    private Mesh planetMesh;
    
    public static Dictionary<TriangleCacheKey,QuadTreeNode> visibleNodes = new Dictionary<TriangleCacheKey, QuadTreeNode> ();

    public static Dictionary<int, float> detailLevelDistance = new Dictionary<int, float>()
    {
        {-1, Mathf.Infinity },
        {0, 500f },
        {1, 300f },
        {2, 100f },
        {3, 50f },
        {4, 30f },
        {5, 10f },
    };

    //max's debug tools
    public bool hasTriedToGenerate = false;

    private void Start() {
        if (planetGenerator == null || playerCamera == null) return;

        planetMesh = planetGenerator.gameObject.GetComponent<MeshFilter>().mesh;
        List<int> triangles = new List<int>(planetMesh.triangles);
        List<Vector3> vertexPositions = new List<Vector3>(planetMesh.vertices);


        if (triangles.Count == 0) return;

        // iterate the planet triangles in sets of 3, because trianlges array is a flat array of ints
        // each int represents a single vertex in a triangle, grouped into sets of three these make a triangle
        for (int i = 0; i < triangles.Count; i += 3) {

            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            Vector3 centroid = SubdivideFunctions.CalculateCentroid(vertexPositions[v1], vertexPositions[v2], vertexPositions[v3]);
            List<int> nodeTriangles = new List<int> { v1, v2, v3 };
            List<Vector3> nodeVertexPositions = new List<Vector3> { vertexPositions[v1], vertexPositions[v2], vertexPositions[v3] };
            ConstructTree(nodeTriangles, nodeVertexPositions, centroid);
        }
        foreach (KeyValuePair<TriangleCacheKey, QuadTreeNode> visibleNode in visibleNodes) {
            Debug.LogWarning($"QuadTreeNode at position: {visibleNode.Value.position}");
            foreach (var tri in visibleNode.Value.nodeTriangles) {
                Debug.Log(tri);
            }
            foreach (var vert in visibleNode.Value.nodeVertices) {
                Debug.Log(vert);
            }
        }
        foreach (var vertex in planetMesh.vertices) {
            Debug.Log(vertex);
        }
    }

    private void Update() {
        if (!hasTriedToGenerate){
            //hasTriedToGenerate = true;

        }
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0) { 

        StartCoroutine(UpdateQuadTree());
            checkTimer = 2f; // Reset the timer
        }

        // Colorize triangles based on proximity
        planetGenerator.ColorizeTriangles(playerCamera.transform.position, minLODDistance, maxLODDistance);

        if(Input.GetKeyUp(KeyCode.Space)) {
            Debug.Log("Space key pressed, updating mesh.");
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            UpdateMesh(vertices, triangles);
            planetMesh.Clear();
            planetMesh.vertices = vertices.ToArray();
            planetMesh.triangles = triangles.ToArray();
            planetMesh.RecalculateNormals();
        }
        //temp code
        //planetGenerator.gameObject.GetComponent<MeshFilter>().mesh.Clear();

    }

    private void ConstructTree(List<int> nodeTriangles, List<Vector3> nodeVertices, Vector3 centroid) {

        TriangleCacheKey key = new TriangleCacheKey(centroid, 0);
        if (!visibleNodes.TryGetValue(key, out var existingNode)) {
            QuadTreeNode parentNode = new QuadTreeNode(null, nodeTriangles, nodeVertices, centroid, 0);
            
            visibleNodes[key] = parentNode;
            parentNode.GenerateChildren(planetGenerator.radius, playerCamera.transform.position); // Don't think this is needed anymore
        } else {
            existingNode.GenerateChildren(planetGenerator.radius, playerCamera.transform.position); //Don't think this is needed anymore
        }
    }

    private void OnDrawGizmos() {
        if (planetGenerator == null || playerCamera == null || !planetGenerator.debugViewEnabled) return;

        Gizmos.color = Color.red; // High-resolution zone
        Gizmos.DrawWireSphere(playerCamera.transform.position, minLODDistance);

        Gizmos.color = Color.yellow; // Medium-resolution zone
        Gizmos.DrawWireSphere(playerCamera.transform.position, maxLODDistance);

    }

    IEnumerator UpdateQuadTree() {
        int nodeIndex = 1;
        // PUT THIS IN A COROUTINE
        
        foreach (var node in new List<KeyValuePair<TriangleCacheKey, QuadTreeNode>>(visibleNodes)) {
            //Debug.LogFormat($"Generating children for node: {node.Key} from the big dictionary ({nodeIndex}/{visibleNodes.Count})");
            node.Value.GenerateChildren(planetGenerator.radius, playerCamera.transform.position);
            // call entry.Value.GenerateChildren(...)
            nodeIndex++;
        }

        yield return new WaitForSeconds(5f);
        // compare dictionary of leaf nodes to dictionary of all loaded nodes
        // any new ones will be built

    }

    private void UpdateMesh(List<Vector3> vertices, List<int> triangles) {
        // Update the mesh with new triangles and vertices

        Dictionary<TriangleCacheKey, QuadTreeNode> leafNodes = new Dictionary<TriangleCacheKey, QuadTreeNode>();
        

        foreach (var node in visibleNodes) {

            var indexOffset = vertices.Count;
            Debug.Log($"Adding node {node.Key} to mesh with index offset {indexOffset}");
            if (node.Value.children != null) return;

            leafNodes.Add(node.Key, node.Value);
            vertices.AddRange(node.Value.nodeVertices);

            for(int i = 0; i < node.Value.nodeTriangles.Count; i++) {
                triangles.Add( i + /*node.Value.nodeTriangles[i]*/ + indexOffset );
                Debug.Log($"Adding triangle {triangles[i+indexOffset]}");
            }
            
        }
    }
}