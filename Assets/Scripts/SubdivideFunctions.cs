using UnityEngine;
using System.Collections.Generic;

public static class SubdivideFunctions
{
    public static void SubdivideTriangles(List<Vector3> vertices, List<int> triangles) { // we pass in the vertices and triangles we generated for the base mesh
        // A new list to store subdivided triangles
        List<int> newTriangles = new List<int>();   // This list will store the subdivided triangles separately from the original list.
        Dictionary<int, int> midpointCache = new Dictionary<int, int>(); // We store the midpoints in a dictionary

        for (int i = 0; i < triangles.Count; i += 3) {    // Here we are iterating through the triangles list to get each triangles edges
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            //  Here we find midpoints of the edges
            int a = GetMidpointIndex(v1, v2, vertices, midpointCache);
            int b = GetMidpointIndex(v2, v3, vertices, midpointCache);
            int c = GetMidpointIndex(v3, v1, vertices, midpointCache);

            // From the Midpoint indices we can create the new suibdivided triangles
            newTriangles.Add(v1);
            newTriangles.Add(a);
            newTriangles.Add(c);

            newTriangles.Add(v2);
            newTriangles.Add(b);
            newTriangles.Add(a);

            newTriangles.Add(v3);
            newTriangles.Add(c);
            newTriangles.Add(b);

            newTriangles.Add(a);
            newTriangles.Add(b);
            newTriangles.Add(c);
        }

        triangles.Clear();                  // Because we have subdivided the whole mesh, we can replace the whole list with the new subdivided one
        triangles.AddRange(newTriangles);  // here we merge the subdivided triangles back into the original list
    }                                     // We will need to find a way to merge our lod list together

    // Helper Method: Calculate or retrieve the midpoint index
    private static int GetMidpointIndex(int index1, int index2, List<Vector3> vertices, Dictionary<int, int> midpointCache) {
        // Create a unique key for the edge
        int smallerIndex = Mathf.Min(index1, index2);
        int largerIndex = Mathf.Max(index1, index2);
        int edgeKey = (smallerIndex << 16) | largerIndex;
        // Uses bit manipulation to combine smallerIndex and largerIndex into a single integer key. 
        // This avoids the need for complex data structures and ensures fast lookups.

        // Check if midpoint already exists
        if (midpointCache.TryGetValue(edgeKey, out int midpointIndex)) {
            return midpointIndex;
        }

        //Calculate midpoint by averaging the positions of the two vertices
        Vector3 midpoint = (vertices[index1] + vertices[index2]) / 2f;
        vertices.Add(midpoint.normalized); // Normalize to sphere surface and adds to vertices list

        // Cache the midpoint index
        // When a new vertex (the calculated midpoint) is added to the  list, it becomes the last element in that list.
        // List indices (plural for index) are 0 based so the newly added vertex is the number of stored vertices - 1, which gives the position of the last element in that list
        // When you add the new midpoint to the  list, it is appended to the end of the list.
        midpointIndex = vertices.Count - 1;
        midpointCache[edgeKey] = midpointIndex;

        return midpointIndex;
    }

    public static Vector3 CalculateCentroid(int[] triangles, int i, Vector3[] vertices) {
        int v1 = triangles[i];
        int v2 = triangles[i + 1];
        int v3 = triangles[i + 2];

        // Calculate the centroid of the triangle
        Vector3 centroid = (vertices[v1] + vertices[v2] + vertices[v3]) / 3f;
        return centroid;
    }
    // inputs are the 3 original vertices
    public static List<int> SubdivideTriangle(int v1, int v2, int v3, List<Vector3> vertices) {
        List<int> newTriangles = new List<int>();
        Dictionary<int, int> midpointCache = new Dictionary<int, int>();
        // Subdivide the triangle


        int a = GetMidpointIndex(v1, v2, vertices, midpointCache);
        int b = GetMidpointIndex(v2, v3, vertices, midpointCache);
        int c = GetMidpointIndex(v3, v1, vertices, midpointCache);

        newTriangles.AddRange(new[] { v1, a, c });
        newTriangles.AddRange(new[] { v2, b, a });
        newTriangles.AddRange(new[] { v3, c, b });
        newTriangles.AddRange(new[] { a, b, c });
        
        return newTriangles;

    }
}
