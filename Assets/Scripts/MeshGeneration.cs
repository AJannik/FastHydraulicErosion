using UnityEngine;

public class MeshGeneration : MonoBehaviour
{
    public Texture2D heightMap;
    public MeshFilter meshFilter;
    [SerializeField] private bool useProportionalHeight = true;
    [SerializeField] private float heightMultiplier = 1f;
    [SerializeField, Range(0.1f, 2f)] private float resolution = 1f;
    [SerializeField] private MeshRenderer meshRenderer;

    private MeshData meshData;
    
    private readonly int heightMapTextureProp = Shader.PropertyToID("_HeightMap");
    private readonly int heightStrengthProp = Shader.PropertyToID("_Height_Strength");

    private void Start()
    {
        float[,] values = SampleHeightMap();
        meshData = new MeshData(values, heightMultiplier, resolution);
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
    
    public void UpdateHeightMap(Texture2D hMap)
    {
        meshRenderer.materials[0].SetTexture(heightMapTextureProp, hMap);
    }

    public void SetPosition(Vector2 size)
    {
        transform.position = new Vector3(size.x, 0, size.y) * resolution;
        if (useProportionalHeight)
        {
            meshRenderer.materials[0].SetFloat(heightStrengthProp, size.magnitude * resolution);
            return;
        }
        
        meshRenderer.materials[0].SetFloat(heightStrengthProp, heightMultiplier * resolution);
    }
}
