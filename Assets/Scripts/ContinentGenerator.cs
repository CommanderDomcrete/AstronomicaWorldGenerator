using UnityEngine;
using System.Collections.Generic;

public class ContinentGenerator : MonoBehaviour
{
    [Header("Continent Settings")]
    [SerializeField] public int continentCount = 5; // Number of continents to generate
    [SerializeField] public float baseScale = 0.1f; // Scale of the noise for distortion
    [SerializeField] public float persistence = 0.5f; // Strength of the noise distortion
    [SerializeField] public Vector3 noiseOffset = new Vector3(0, 0, 0); // Offset for the noise function
    [SerializeField] public int octaves = 4; // Number of octaves for the noise function
    [SerializeField] public float noiseStrength = 0.5f; // Strength of the noise distortion
    Texture2D texture;
    const int textureResolution = 512;
    NoiseFilter continentNoiseFilter;
    public ContinentMaskSettings continentMaskSettings; // Reference to the continent mask settings

    public void GenerateContinents(Mesh planetMesh) {
        //CreateCellCentres(planetMesh.vertices); // Generate cell centres for the continents
        //ColourContinents(planetMesh, AssignVerticesToCells((planetMesh.vertices), CreateCellCentres(planetMesh.vertices))); // Colour the continents based on the assigned vertices
        GenerateContinentsMask();
    }

    private List<Vector3> CreateCellCentres(Vector3[] meshVertices) {
        List<Vector3> cellCentres = new List<Vector3>();
        for (int i = 0; i < continentCount; i++) {
            // Generate a continent here
            cellCentres.Add(meshVertices[Random.Range(0, meshVertices.Length)]);
        }
        return cellCentres;
    }

    Dictionary<int, List<int>> AssignVerticesToCells(Vector3[] meshVertices, List<Vector3> cellCentres) {
        continentNoiseFilter = new NoiseFilter(); // Create a new noise filter for the continent generation
        Dictionary<int, List<int>> continentCells = new Dictionary<int, List<int>>(); // A dictionary which stores the continent index and the list of vertices that belong to that continent/cell
        for (int i = 0; i < cellCentres.Count; i++) {
            continentCells[i] = new List<int>();    // for each cell centre, create a new list of vertices
        }
        // Assign all vertices to the closest cell centre
        for (int v = 0; v < meshVertices.Length; v++) {
            float minDistance = float.MaxValue;
            int closestCellCentre = -1;

            for (int c = 0; c < cellCentres.Count; c ++) {
                // Calculate the distance between the vertex and the cell centre, find the closest one
                float distance = Vector3.Distance(meshVertices[v], cellCentres[c]); 

                // Add noise-based distortion
                distance += (continentNoiseFilter.Evaluate(meshVertices[v]) - noiseOffset.x) * noiseStrength; ;

                if (distance < minDistance) {
                    minDistance = distance;
                    closestCellCentre = c;
                }
            }

            continentCells[closestCellCentre].Add(v);   // the closest cell centre is the one that the vertex belongs to
        }
        return continentCells;
    }

    private void ColourContinents(Mesh planetMesh, Dictionary<int, List<int>> continentCells) {
        Color[] colors = new Color[planetMesh.vertexCount]; // Create an array of colors for each vertex in the mesh
        Color[] continentColors = GenerateRandomColors(continentCells.Count); // Create an array of colors for each continent
        // Assign the colors to the mesh
        foreach (var cell in continentCells) {
            int cellIndex = cell.Key;
            foreach (int vertexIndex in cell.Value) {
                colors[vertexIndex] = continentColors[cellIndex]; // Assign the color of the continent to the vertex
            }
        }
        planetMesh.colors = colors;
    }

    private Color[] GenerateRandomColors(int count) {
        Color[] colors = new Color[count];
        for(int i = 0; i < count; i++) {
            colors[i] = new Color(Random.value, Random.value, Random.value); // Generate a random color
        }
        return colors;
    }

    public void GenerateContinentsMask() {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (texture == null || texture.width != textureResolution || texture.height != textureResolution) {
            texture = new Texture2D(textureResolution, textureResolution);
        }

        // Generate Voronoi cells
        Color[] colors = new Color[textureResolution * textureResolution];
        Vector2[] cellCenters = GenerateVoronoiCellCenters(10); // Example: 10 cells
        int[] cellPriorities = GenerateCellPriorities(cellCenters.Length);

        for (int y = 0; y < textureResolution; y++) {
            for (int x = 0; x < textureResolution; x++) {
                Vector2 point = new Vector2((float)x / textureResolution, (float)y / textureResolution);
                int cellIndex = GetClosestCellIndex(point, cellCenters);
                float priority = cellPriorities[cellIndex] / (float)cellPriorities.Length;

                // Assign a color based on the cell index and priority
                colors[y * textureResolution + x] = Color.Lerp(Color.black, Color.white, priority);
            }
        }

        // Apply the colors to the texture
        texture.SetPixels(colors);
        texture.Apply();

        // Assign the texture to the material
        continentMaskSettings.worldMaterial.SetTexture("_continentsMask", texture);
    }

    private Vector2[] GenerateVoronoiCellCenters(int cellCount) {
        Vector2[] centers = new Vector2[cellCount];
        for (int i = 0; i < cellCount; i++) {
            centers[i] = new Vector2(Random.value, Random.value); // Randomly distribute cell centers
        }
        return centers;
    }

    private int[] GenerateCellPriorities(int cellCount) {
        int[] priorities = new int[cellCount];
        for (int i = 0; i < cellCount; i++) {
            priorities[i] = Random.Range(1, 100); // Assign random priorities
        }
        return priorities;
    }

    private int GetClosestCellIndex(Vector2 point, Vector2[] cellCenters) {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < cellCenters.Length; i++) {
            float distance = Vector2.Distance(point, cellCenters[i]);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

}
