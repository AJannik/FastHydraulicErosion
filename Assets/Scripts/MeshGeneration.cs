using UnityEngine;

public class MeshGeneration : MonoBehaviour
{
    public Texture2D heightMap;
    public MeshFilter meshFilter;
    public float heightMultiplier = 1f;
    public MeshRenderer meshRenderer;

    private MeshData meshData;
    
    void Start()
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

        meshData = new MeshData(values, heightMultiplier);
        meshFilter.mesh = meshData.CreateMesh();
    }

    void Update()
    {
        
    }

    private void UpdateHeights(float[,] heightValues)
    {
        int width = heightValues.GetLength(0);
        int height = heightValues.GetLength(1);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

            }
        }
    }
}
