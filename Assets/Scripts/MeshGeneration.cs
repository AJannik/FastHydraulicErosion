using UnityEngine;
using UnityEngine.Rendering;

public class MeshGeneration : MonoBehaviour
{
    public Texture2D heightMap;
    public MeshFilter meshFilter;
    public float heightMultiplier = 1f;
    public MeshRenderer meshRenderer;
    
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
        
        meshFilter.mesh = GenerateMesh(values);
    }

    void Update()
    {
        
    }

    private Mesh GenerateMesh(float[,] heightValues)
    {
        int width = heightValues.GetLength(0);
        int depth = heightValues.GetLength(1);

        Vector3[] vertices = new Vector3[width * depth];
        Vector2[] uvs = new Vector2[width * depth];
        int[] triangles = new int[(width - 1) * (depth - 1) * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int y = 0; y < depth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                vertices[vertexIndex] = new Vector3(x, heightValues[x, y] * heightMultiplier, y);
                uvs[vertexIndex] = new Vector2(1f - x / (float)width, 1f - y / (float)depth);
                
                if (x < width - 1 && y < depth - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + width;
                    triangles[triangleIndex + 2] = vertexIndex + width + 1;

                    triangleIndex += 3;
                    
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + width + 1;
                    triangles[triangleIndex + 2] = vertexIndex + 1;
                    
                    triangleIndex += 3;
                }
                
                vertexIndex++;
            }
        }

        Mesh mesh = new Mesh
        {
            indexFormat = IndexFormat.UInt32,
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };

        mesh.RecalculateNormals();
        return mesh;
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

    private void AddTriangle()
    {
        
    }
}
