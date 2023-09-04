using UnityEngine;

public class ErosionController : MonoBehaviour
{
    [SerializeField] private ComputeShader erosionShader;
    [SerializeField] private Texture2D heightMap;
    [SerializeField] private MeshGeneration meshGeneration;
    
    private int erosionKernel;
    private RenderTexture computeDataMap1; // r = heightMap, g = suspended sediment, b = water level
    private RenderTexture computeDataMap2; // pipe model flux field
    private RenderTexture computeDataMap3; // velocity field
    
    private readonly int dataMap1ShaderProp = Shader.PropertyToID("dataMap1");
    private readonly int dataMap2ShaderProp = Shader.PropertyToID("fluxMap");
    private readonly int dataMap3ShaderProp = Shader.PropertyToID("velocityField");
    
    private void Start()
    {
        erosionKernel = erosionShader.FindKernel("CSMain"); // TODO
        computeDataMap1 = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeDataMap2 = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeDataMap3 = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        Graphics.Blit(heightMap, computeDataMap1);
        
        erosionShader.SetTexture(erosionKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(erosionKernel, dataMap2ShaderProp, computeDataMap2);
        erosionShader.SetTexture(erosionKernel, dataMap3ShaderProp, computeDataMap3);
        meshGeneration.UpdateHeightMap(computeDataMap1);
    }

    private void FixedUpdate()
    {
        erosionShader.Dispatch(erosionKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
    }

    private void OnDestroy()
    {
        computeDataMap1.Release();
        computeDataMap2.Release();
        computeDataMap3.Release();
    }
}