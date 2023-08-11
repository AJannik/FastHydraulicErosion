using UnityEngine;
using UnityEngine.Rendering;

public struct MeshData
{
    private int[] triangles;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int xSize, zSize;
    private float heightScale;

    public MeshData(float[,] heightValues, float heightScale)
    {
        xSize = heightValues.GetLength(0);
        zSize = heightValues.GetLength(1);
        
        vertices = new Vector3[xSize * zSize];
        uvs = new Vector2[xSize * zSize];
        triangles = new int[(xSize - 1) * (zSize - 1) * 6];
        this.heightScale = heightScale;
        
        GenerateMeshData(heightValues);
    }
    
    private void AddTriangle(int index, int a, int b, int c)
    {
        triangles[index] = a;
        triangles[index + 1] = b;
        triangles[index + 2] = c;
    }
    
    private void GenerateMeshData(float[,] heightValues)
    {
        int vertexIndex = 0;
        int triangleIndex = 0;

        for (int y = 0; y < zSize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                vertices[vertexIndex] = new Vector3(x, heightValues[x, y] * heightScale, y);
                uvs[vertexIndex] = new Vector2(1f - x / (float)xSize, 1f - y / (float)zSize);
                
                if (x < xSize - 1 && y < zSize - 1)
                {
                    AddTriangle(triangleIndex, vertexIndex, vertexIndex + xSize, vertexIndex + xSize + 1);
                    triangleIndex += 3;
                    
                    AddTriangle(triangleIndex, vertexIndex, vertexIndex + xSize + 1, vertexIndex + 1);
                    triangleIndex += 3;
                }
                
                vertexIndex++;
            }
        }
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void UpdateHeights(float[,] heightValues, float newHeightScale)
    {
        if (heightValues.GetLength(0) != xSize || heightValues.GetLength(1) != zSize)
        {
            Debug.LogError("UpdateHeights(): New heights are not the same Mesh size as the current Terrain!");
            return;
        }
        
        heightScale = newHeightScale;
        int vertexIndex = 0;

        for (int y = 0; y < zSize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                vertices[vertexIndex] = new Vector3(x, heightValues[x, y] * heightScale, y);
                uvs[vertexIndex] = new Vector2(1f - x / (float)xSize, 1f - y / (float)zSize);
                
                vertexIndex++;
            }
        }
    }

    public Mesh CreateMesh()
    {
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
}