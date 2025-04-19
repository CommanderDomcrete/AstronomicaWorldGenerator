
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class QuadTreeNode {
    public Vector3 position;
    public int detailLevel;
    public QuadTreeNode[] children;
    public List<Vector3> nodeVertices;
    public List<int> nodeTriangles;
    public GameObject triPoint;
    public QuadTreeNode(QuadTreeNode[] children, List<int> nodeTriangles, List<Vector3> nodeVertices, Vector3 position, int detailLevel) {
    
        string childString = children != null ? children.Length.ToString() : "null";
        string nodeTriangleString = nodeTriangles != null ? string.Join(", ", nodeTriangles) : "null";
        string nodeString = nodeVertices != null ? string.Join(", ", nodeVertices) : "null";
        //Debug.LogWarning($"Creating QuadTreeNode at position: {position}, detailLevel: {detailLevel} with children: {childString}, nodeTriangles: {nodeTriangleString}, nodeVertices: {nodeString}");
        this.nodeVertices = nodeVertices;
        this.nodeTriangles = nodeTriangles;
        this.detailLevel = detailLevel;
        this.position = position;
        this.children = children;

        triPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        triPoint.transform.position = position;
        triPoint.transform.localScale = new Vector3(5f, 5f, 5f);
        
    }

    public void GenerateChildren(float radius, Vector3 playerPosition) {
        // We sudivide using the parent verts and tris to get the NEW TRIANGLES
        // Then we can calculate the NEW VERTS and CENTROIDS of the children


        if (detailLevel <= 4 && detailLevel >= 0) {
			
            if (Vector3.Distance(position.normalized * radius, playerPosition) <= LODManager.detailLevelDistance[detailLevel]) {

                //need to check if the node is already generated
                if (children != null) {
                    //Debug.Log("Node at " + position + " already has children, skipping generation.");
                    return;
                }

                //Debug.Log("Generating children for node at " + position + " with detail level " + detailLevel);
                //Debug.Log("Using parent triangles: " + string.Join(", ", this.nodeTriangles));
                //Debug.Log("Using parent vertices: " + string.Join(", ", this.nodeVertices));
                
                List<int> newTriangle = SubdivideFunctions.SubdivideTriangle(0, 1, 2, this.nodeVertices); // ********NEED TO FIND CORRECT ORDER. SOME FACES WILL BE BACK TO FRONT!!!!!!!!!!!

                children = new QuadTreeNode[4];
                                                                                                                                        // Tried using nested loops but I couldn't get it to work.
                for (int i = 0; i < children.Length; i++) {                                                                             // For this to work we need to think about how the out puts pair up
                                                                                                                                        // 0 : 0, 1, 2
                    List<int> childTriangles = new List<int>();                                                                         // 1 : 3, 4, 5
                    List<Vector3> childVertices = new List<Vector3>();                                                                  // 2 : 6, 7, 8
                    int k = 3 * i;                                                                                                      // 3 : 9, 10, 11
                    int v1 = newTriangle[k];                                                                                            // we can see this pattern can correlate to (3 * i), + 1, +2
                    int v2 = newTriangle[k + 1];
                    int v3 = newTriangle[k + 2];

                    childTriangles.AddRange(new[] { 0, 1, 2 }); // new list to assign each triangle to a node
                    childVertices.AddRange(new[] { this.nodeVertices[v1], this.nodeVertices[v2], this.nodeVertices[v3] });

                    for (int j = 0; j < childVertices.Count; j++) {
                        childVertices[j] = childVertices[j].normalized * radius;
                    }

                    Vector3 centroid = SubdivideFunctions.CalculateCentroid(childVertices[0], childVertices[1], childVertices[2]);

                    TriangleCacheKey key = new TriangleCacheKey(centroid, detailLevel + 1);

                    if (LODManager.visibleNodes.TryGetValue(key, out QuadTreeNode existingNode)) {
                        Debug.Log($"Using cached node at {centroid} for detail level {detailLevel + 1}");
                        children[i] = existingNode;
                    } else {
                        QuadTreeNode newNode = new QuadTreeNode(null, childTriangles, childVertices, centroid, detailLevel + 1);
                        //children[i] = newNode; // do we need this here?
                        LODManager.visibleNodes[key] = newNode;
                    }

                }
                /*/ Create grandchildren
                foreach (QuadTreeNode child in children) {
                    child.GenerateChildren(radius, playerPosition);
                }*/
            } 
            else{
                //Debug.Log("Node at " + position + " is too far from player position " + playerPosition + " with detail level " + detailLevel);
            }

        } 
        else {
            //Debug.Log("Detail level too high or too low for node at " + position + " with detail level " + detailLevel);
        }
        //Debug.Log(playerPosition);

        if ((Vector3.Distance(position.normalized * radius, playerPosition) > LODManager.detailLevelDistance[detailLevel - 1]) && detailLevel != 0) {
            //Debug.Log("Node at " + position + " is too far from player position " + playerPosition + " with detail level " + detailLevel);
            RetireNode();
        }
        else if ((Vector3.Distance(position.normalized * radius, playerPosition) > LODManager.detailLevelDistance[detailLevel]) && detailLevel == 0) {
            children = null;
        }
    }

    public QuadTreeNode GetLeafChildren(QuadTreeNode node) {
        if(node.children.Length == 0) {
            return node;
        }
        foreach (var child in node.children) {
            child.GetLeafChildren(child);
        }
        return null;
    }

    public void RetireNode() {
        //Debug.Log("Retiring node at " + position + " with detail level " + detailLevel);

        children = null;
        nodeTriangles = null;
        nodeVertices = null;
        LODManager.visibleNodes.Remove(new TriangleCacheKey(position, detailLevel));
        GameObject.Destroy(triPoint); // Destroy the GameObject associated with this node
    }

}

