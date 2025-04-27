
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using System.Net;
using Unity.VisualScripting;
using Mono.Cecil;

public class QuadTreeNode {
    public Vector3 position;
    public int detailLevel;
    public QuadTreeNode[] children;
    public List<int> nodeTriangles;
    public GameObject triPoint;
    public QuadTreeNode parentNode;
    public bool outOfRange = false; // used to check if the node is out of range of the player position
    public QuadTreeNode(QuadTreeNode parentNode, QuadTreeNode[] children, List<int> nodeTriangles, Vector3 position, int detailLevel) {
    
        string childString = children != null ? children.Length.ToString() : "null";
        string nodeTriangleString = nodeTriangles != null ? string.Join(", ", nodeTriangles) : "null";
        //Debug.LogWarning($"Creating QuadTreeNode at position: {position}, detailLevel: {detailLevel} with children: {childString}, nodeTriangles: {nodeTriangleString}, nodeVertices: {nodeString}");
        this.nodeTriangles = nodeTriangles;
        this.detailLevel = detailLevel;
        this.position = position;
        this.children = children;
        this.parentNode = parentNode;

        //triPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //triPoint.transform.position = position;
        //triPoint.transform.localScale = new Vector3(5f, 5f, 5f);
        
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
                
                List<int> newTriangles = SubdivideFunctions.SubdivideTriangle(this.nodeTriangles[0], this.nodeTriangles[1], this.nodeTriangles[2], LODManager.vertexCache, radius); // ********NEED TO FIND CORRECT ORDER. SOME FACES WILL BE BACK TO FRONT!!!!!!!!!!!

                children = new QuadTreeNode[4];
                                                                                                                                        // Tried using nested loops but I couldn't get it to work.
                for (int i = 0; i < children.Length; i++) {                                                                             // For this to work we need to think about how the out puts pair up
                                                                                                                                        // 0 : 0, 1, 2
                    List<int> childTriangle = new List<int>();                                                                         // 1 : 3, 4, 5
                                                                                                                                        // 2 : 6, 7, 8
                    int k = 3 * i;                                                                                                      // 3 : 9, 10, 11
                    int v1 = newTriangles[k];                                                                                            // we can see this pattern can correlate to (3 * i), + 1, +2
                    int v2 = newTriangles[k + 1];
                    int v3 = newTriangles[k + 2];

                    childTriangle.AddRange(new[] { v1, v2, v3 }); // new list to assign each triangle to a node

                    Vector3 centroid = SubdivideFunctions.CalculateCentroid(LODManager.vertexCache[v1], LODManager.vertexCache[v2], LODManager.vertexCache[v3]);

                    TriangleCacheKey key = new TriangleCacheKey(centroid, detailLevel + 1);

                    if (LODManager.visibleNodes.TryGetValue(key, out QuadTreeNode existingNode)) {
                        //Debug.Log($"Using cached node at {centroid} for detail level {detailLevel + 1}");
                        children[i] = existingNode;
                    } else {
                        QuadTreeNode newNode = new QuadTreeNode(this, null, childTriangle, centroid, detailLevel + 1);
                        children[i] = newNode; // do we need this here? Yes we do because it assigns the new node to the children array
                        LODManager.visibleNodes[key] = newNode;
                    }   
                }

            } 
            else{
                //Debug.Log("Node at " + position + " is too far from player position " + playerPosition + " with detail level " + detailLevel);
            }
        } 
        else {
            //Debug.Log("Detail level too high or too low for node at " + position + " with detail level " + detailLevel);
        }
        //Debug.Log(playerPosition);
    }

    public void RemoveChildren(float radius, Vector3 playerPosition) {
        // Check if node is out of range
        // If yes, are all it's children out of range? If no then do nothing
        // If yes, retire all the nodes children
        
        if ((Vector3.Distance(position.normalized * radius, playerPosition) > LODManager.detailLevelDistance[detailLevel])) {

            outOfRange = true;

            if (children == null) return;
            //Debug.Log("Node at " + position + " is out of range for detail level " + detailLevel + "and they've got children");

            foreach (var child in children) {
                if (child != null) {
                    child.RemoveChildren(radius, playerPosition);
                    if (child.outOfRange) break;
                }
            }
            
            if (outOfRange) {
                //Debug.Log("Node at " + position + " is out of range for detail level " + detailLevel + " and all children are out of range");
                foreach (var child in children) {

                    if (child != null) {
                        //Debug.Log("Retiring child node at " + child.position + " with detail level " + child.detailLevel);
                        child.RetireNode();
                    }
                }
                children = null; // Clear the children array
                string childString = children != null ? children.Length.ToString() : "null";
                string nodeTriangleString = nodeTriangles != null ? string.Join(", ", nodeTriangles) : "null";
                //Debug.LogWarning($"Children removed at parent position: {position}, detailLevel: {detailLevel}, nodeTriangles: {nodeTriangleString}");
            }
        }
        
        else if ((Vector3.Distance(position.normalized * radius, playerPosition) > LODManager.detailLevelDistance[detailLevel]) && detailLevel == 0) {
            if (children == null) return; // No children to remove
            foreach (var child in children) {
                if (child != null) {
                    child.RetireNode();
                }
            }
            children = null; // Clear the children array
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

    // Retires node and all its children
    public void RetireNode() {
        //Debug.Log("Retiring node at " + position + " with detail level " + detailLevel);
        if (children != null) {
            foreach (var child in children) {
                if (child != null) {
                    child.RetireNode();
                }
            }
        }

        // Remove node from visibleNodes
        TriangleCacheKey key = new TriangleCacheKey(position, detailLevel);
        if(LODManager.visibleNodes.ContainsKey(key)) {
            LODManager.visibleNodes.Remove(key);
        }
        children = null;
        nodeTriangles = null;

        //LODManager.visibleNodes.Remove(new TriangleCacheKey(position, detailLevel));
        if (triPoint != null) {
            GameObject.Destroy(triPoint); // Destroy the GameObject associated with this node
        }

        if (parentNode != null) {
            parentNode.RemoveChildReference(this); // Remove this node from its parent's children
        }
    }

    //Removes a child reference from parent
    public void RemoveChildReference(QuadTreeNode child) {
        if (children == null) return;

        for (int i = 0; i < children.Length; i++) {
            if (children[i] == child) {
                children[i] = null; // Remove the child reference
                break; // Exit the loop once we find and remove the child
            }
        }
    }

}

