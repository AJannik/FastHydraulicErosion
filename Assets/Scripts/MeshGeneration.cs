using UnityEngine;

public class MeshGeneration : MonoBehaviour
{
    public Texture2D heightMap;
    public MeshFilter meshFilter;
    public float heightMultiplier = 1f;
    public MeshRenderer meshRenderer;
    public Shader shader;
    
    private MeshData meshData;
    
    private void Start()
    {
        float[,] values = SampleHeightMap();
        meshData = new MeshData(values, heightMultiplier);
        meshFilter.mesh = meshData.CreateMesh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float[,] values = SampleHeightMap();
            meshFilter.mesh.SetVertices(meshData.UpdateHeights(values, heightMultiplier));
        }
    }

    private float[,] SampleHeightMap()
    {
        Color[] pixels = heightMap.GetPixels();
        int width = heightMap.width;
        int depth = heightMap.height;
        float[,] values = new float[width, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < depth; y++)
            {
                values[x, y] = pixels[y * width + x].r;
            }
        }

        return values;
    }
}
