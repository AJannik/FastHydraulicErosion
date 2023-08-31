using UnityEngine;

public class MeshGeneration : MonoBehaviour
{
    public Texture2D heightMap;
    public MeshFilter meshFilter;
    public float heightMultiplier = 1f;
    [SerializeField] private MeshRenderer meshRenderer;

    private MeshData meshData;
    
    private readonly int heightMapTextureProp = Shader.PropertyToID("_HeightMap");
    
    private void Start()
    {
        float[,] values = SampleHeightMap();
        meshData = new MeshData(values, heightMultiplier);
        meshFilter.mesh = meshData.CreateMesh();
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

    public void UpdateHeightMap(RenderTexture hMap)
    {
        meshRenderer.materials[0].SetTexture(heightMapTextureProp, hMap);
    }
}
