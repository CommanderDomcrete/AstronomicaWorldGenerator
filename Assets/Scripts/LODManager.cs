using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static Unity.VisualScripting.Metadata;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEditor.Experimental.GraphView;

public class LODManager : MonoBehaviour {
    public Camera playerCamera;    // Reference to the camera
    public PlanetGenerator planetGenerator;

    private float maxLODDistance = 600f;   // Distance for lowest LOD
    private float minLODDistance = 400f;  // Distance for highest LOD
    public int maxResolution = 6;        // Highest resolution level
    public int minResolution = 2;        // Lowest resolution level

    private float checkTimer = 0f; // Timer for checking LOD updates

    private Mesh planetMesh;

    public static Dictionary<VertexCacheKey, int> vertexCacheIndex = new Dictionary<VertexCacheKey, int>();
    public static List<Vector3> vertexCache = new List<Vector3>();

    public static Dictionary<TriangleCacheKey,QuadTreeNode> visibleNodes = new Dictionary<TriangleCacheKey, QuadTreeNode> ();
    public static Dictionary<TriangleCacheKey, QuadTreeNode> rootNodes = new Dictionary<TriangleCacheKey, QuadTreeNode>();
    public static Dictionary<int, float> detailLevelDistance = new Dictionary<int, float>()
    {
        {-1, Mathf.Infinity },
        {0, 600f },
        {1, 400f },
        {2, 300f },
        {3, 200f },
        {4, 100f },
        {5, 50f },
    };

    //max's debug tools
    public bool hasTriedToGenerate = false;

    private void Start() {
        if (planetGenerator == null || playerCamera == null) return;

        planetMesh = planetGenerator.gameObject.GetComponent<MeshFilter>().mesh;
        List<int> triangles = new List<int>(planetMesh.triangles);
        List<Vector3> vertexPositions = new List<Vector3>(planetMesh.vertices);

        foreach(var vertex in planetMesh.vertices) { //  Adding vertices to cache
            GetOrAddVertex(vertex);
        }

        if (triangles.Count == 0) return;

        // iterate the planet triangles in sets of 3, because triangles array is a flat array of ints
        // each int represents a single vertex in a triangle, grouped into sets of three these make a triangle
        for (int i = 0; i < triangles.Count; i += 3) {

            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            Vector3 centroid = SubdivideFunctions.CalculateCentroid(vertexPositions[v1], vertexPositions[v2], vertexPositions[v3]);
            List<int> nodeTriangles = new List<int> { v1, v2, v3 };
            ConstructTree(nodeTriangles, centroid);
            rootNodes = visibleNodes; // Store the root nodes in a separate dictionary  
        }
    }

    private void Update() {
        if (!hasTriedToGenerate){
            //hasTriedToGenerate = true;

        }
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0) { 

            StartCoroutine(UpdateQuadTree());
            planetMesh.Clear();
            StartCoroutine(UpdateMesh(vertexCache));
            checkTimer = 2f; // Reset the timer
        }
    }

    private void ConstructTree(List<int> nodeTriangles, Vector3 centroid) {

        TriangleCacheKey key = new TriangleCacheKey(centroid, 0);
        if (!visibleNodes.TryGetValue(key, out var existingNode)) {
            QuadTreeNode parentNode = new QuadTreeNode(null, null, nodeTriangles, centroid, 0);
            
            visibleNodes[key] = parentNode;
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
            node.Value.GenerateChildren(planetGenerator.geometrySettings.radius, playerCamera.transform.position);
            // call entry.Value.GenerateChildren(...)
            nodeIndex++;
        }
        foreach (var node in new List<KeyValuePair<TriangleCacheKey, QuadTreeNode>>(visibleNodes)) {
            //Debug.LogFormat($"Generating children for node: {node.Key} from the big dictionary ({nodeIndex}/{visibleNodes.Count})");
            // call entry.Value.GenerateChildren(...)
            node.Value.RemoveChildren(planetGenerator.geometrySettings.radius, playerCamera.transform.position);
        }
        yield return new WaitForSeconds(5f);
        // compare dictionary of leaf nodes to dictionary of all loaded nodes
        // any new ones will be built

    }

    IEnumerator UpdateMesh(List<Vector3> newVertices) {
        List<int> newTriangles = new List<int>();

        List<QuadTreeNode> leafNodes = new List<QuadTreeNode>();

        leafNodes = GatherLeafNodes();
        foreach (var node in leafNodes) {
            foreach(int triangleIndex in node.nodeTriangles) {
                newTriangles.Add(triangleIndex);
            }
        }

        planetMesh.vertices = newVertices.ToArray();
        planetMesh.triangles = newTriangles.ToArray();
        planetMesh.RecalculateNormals();
        yield return new WaitForSeconds(5f);
    }

    public int GetOrAddVertex(Vector3 vertex) {
        VertexCacheKey key = new VertexCacheKey(vertex);
        if (!vertexCacheIndex.TryGetValue(key, out int index)) {
            index = vertexCache.Count;
            vertexCache.Add(vertex);
            vertexCacheIndex[key] = index;
        }
        return index;
    }

    private List<QuadTreeNode> GatherLeafNodes() {
        List<QuadTreeNode> leafNodes = new List<QuadTreeNode>();
        foreach (var node in visibleNodes) {
            if (node.Value.children == null) {
                leafNodes.Add(node.Value);
            }
        }
        return leafNodes;
    }
}