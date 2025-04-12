using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class QuadTreeNode {
    public Vector3 position;
    public int detailLevel;
    public QuadTreeNode[] children;
    public List<Vector3> nodeVertices;
    public List<int> nodeTriangles;

    public QuadTreeNode(QuadTreeNode[] children, List<int> nodeTriangles, List<Vector3> nodeVertices, Vector3 position, int detailLevel) {
        this.nodeVertices = nodeVertices;
        this.nodeTriangles = nodeTriangles;
        this.detailLevel = detailLevel;
        this.position = position;
        this.children = children;
        GameObject triPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        triPoint.transform.position = position;
        triPoint.transform.localScale = new Vector3(10f, 10f, 10f);
        LODManager.visibleNodes[position * detailLevel] = this;
    }
    public void GenerateChildren(List<int> parentTriangles, List<Vector3> parentVertices, float radius, Vector3 playerPosition) {
        // We sudivide using the parent verts and tris to get the NEW TRIANGLES
        // Then we can calculate the NEW VERTS and CENTROIDS of the children


        if (detailLevel <= 4 && detailLevel >= 0) {

            if (Vector3.Distance(position.normalized * radius, playerPosition) <= LODManager.detailLevelDistance[detailLevel]) {

                List<int> newTriangle = SubdivideFunctions.SubdivideTriangle(0, 1, 2, parentVertices); // ********NEED TO FIND CORRECT ORDER. SOME FACES WILL BE BACK TO FRONT!!!!!!!!!!!

                children = new QuadTreeNode[4];
                                                                                                                                        // Tried using nested loops but I couldn't get it to work.
                for (int i = 0; i < children.Length; i++) {                                                                             // For this to work we need to think about how the out puts pair up
                                                                                                                                        // 0 : 0, 1, 2
                    List<int> childTriangles = new List<int>();                                                                        // 1 : 3, 4, 5
                    List<Vector3> childVertices = new List<Vector3>();                                                                  // 2 : 6, 7, 8
                    int k = 3 * i;                                                                                                      // 3 : 9, 10, 11
                    int v1 = newTriangle[k];                                                                                            // we can see this pattern can correlate to (3 * i), + 1, +2
                    int v2 = newTriangle[k + 1];
                    int v3 = newTriangle[k + 2];

                    childTriangles.AddRange(new[] { 0, 1, 2 }); // new list to assign each triangle to a node
                    childVertices.AddRange(new[] { parentVertices[v1], parentVertices[v2], parentVertices[v3] });


                    for (int j = 0; j < childVertices.Count; j++) {
                        childVertices[j] = childVertices[j].normalized * radius;
                    }

                    Vector3 centroid = SubdivideFunctions.CalculateCentroid(childTriangles.ToArray(), 0, childVertices.ToArray());

                    children[i] = new QuadTreeNode(null, childTriangles, childVertices, centroid, detailLevel + 1);

                }

                // Create grandchildren
                foreach (QuadTreeNode child in children) {
                    child.GenerateChildren(child.nodeTriangles, child.nodeVertices, radius, playerPosition);

                }

            }
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

}

