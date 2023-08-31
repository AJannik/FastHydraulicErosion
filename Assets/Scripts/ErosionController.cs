using UnityEngine;

public class ErosionController : MonoBehaviour
{
    [SerializeField] private ComputeShader erosionShader;
    [SerializeField] private Texture2D heightMap;
    [SerializeField] private MeshGeneration meshGeneration;
    
    private int erosionKernel;
    private RenderTexture computeHeightMap;
    
    private readonly int heightMapShaderProp = Shader.PropertyToID("heightMap");
    
    private void Start()
    {
        erosionKernel = erosionShader.FindKernel("CSMain");
        computeHeightMap = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        Graphics.Blit(heightMap, computeHeightMap);
        erosionShader.SetTexture(erosionKernel, heightMapShaderProp, computeHeightMap);
        meshGeneration.UpdateHeightMap(computeHeightMap);
    }

    private void FixedUpdate()
    {
        erosionShader.Dispatch(erosionKernel, computeHeightMap.width / 8, computeHeightMap.height / 8, 1);
    }

    private void OnDestroy()
    {
        computeHeightMap.Release();
    }
}