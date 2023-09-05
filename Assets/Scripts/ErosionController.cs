using UnityEngine;

public class ErosionController : MonoBehaviour
{
    [SerializeField] private ComputeShader erosionShader;
    [SerializeField] private Texture2D heightMap;
    [SerializeField] private MeshGeneration meshGeneration;
    [SerializeField] private MeshRenderer debugMesh;
    
    private int erosionKernel;
    private int initKernel;
    private RenderTexture computeDataMap1; // r = heightMap, g = suspended sediment, b = water level
    private RenderTexture computeFluxMap; // pipe model flux field
    private RenderTexture computeVelocityField; // velocity field
    
    private readonly int dataMap1ShaderProp = Shader.PropertyToID("dataMap1");
    private readonly int fluxMapShaderProp = Shader.PropertyToID("fluxMap");
    private readonly int velocityFieldShaderProp = Shader.PropertyToID("velocityField");
    
    private void Start()
    {
        erosionKernel = erosionShader.FindKernel("FlowSimulation");
        initKernel = erosionShader.FindKernel("Init");
        computeDataMap1 = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeFluxMap = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeVelocityField = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        Graphics.Blit(heightMap, computeDataMap1);
        
        erosionShader.SetTexture(initKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(erosionKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(erosionKernel, fluxMapShaderProp, computeFluxMap);
        erosionShader.SetTexture(erosionKernel, velocityFieldShaderProp, computeVelocityField);
        erosionShader.SetFloat("waterSourceStrength", 1f);
        erosionShader.SetFloat("pipeCrossSection", 1f);
        erosionShader.SetFloat("lengthPipe", 1f);
        erosionShader.SetFloat("gravity", 9.81f);
        erosionShader.SetFloat("evaporationConst", 0.1f);
        meshGeneration.UpdateHeightMap(computeDataMap1);
        debugMesh.materials[0].mainTexture = computeFluxMap;
        
        erosionShader.Dispatch(initKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
    }

    private void FixedUpdate()
    {
        erosionShader.SetFloat("simulationTimeStep", Time.fixedDeltaTime);
        erosionShader.Dispatch(erosionKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
    }

    private void OnDestroy()
    {
        computeDataMap1.Release();
        computeFluxMap.Release();
        computeVelocityField.Release();
    }
}