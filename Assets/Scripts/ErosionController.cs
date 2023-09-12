using UnityEngine;
using UnityEngine.Serialization;

public class ErosionController : MonoBehaviour
{
    [SerializeField] private ComputeShader erosionShader;
    [SerializeField] private Texture2D heightMap;
    [SerializeField] private MeshGeneration meshGeneration;
    [SerializeField] private MeshRenderer debugMesh;
    [FormerlySerializedAs("waterSource")] [SerializeField] private WaterSourceHandler waterSourceHandler;
    
    private int initKernel;
    private int waterIncKernel;
    private int flowSimulationKernel;
    private int evaporationKernel;
    private RenderTexture computeDataMap1; // r = heightMap, g = suspended sediment, b = water level
    private RenderTexture computeFluxMap; // pipe model flux field
    private RenderTexture computeVelocityField; // velocity field
    private RenderTexture computeWaterDeltaMap; // r = delta1, g = delta2
    private ComputeBuffer waterSourcesBuffer;
    
    private readonly int dataMap1ShaderProp = Shader.PropertyToID("dataMap1");
    private readonly int fluxMapShaderProp = Shader.PropertyToID("fluxMap");
    private readonly int velocityFieldShaderProp = Shader.PropertyToID("velocityField");
    private readonly int waterDeltaMapShaderProp = Shader.PropertyToID("waterDeltaMap");
    
    private void Start()
    {
        initKernel = erosionShader.FindKernel("Init");
        waterIncKernel = erosionShader.FindKernel("WaterInc");
        flowSimulationKernel = erosionShader.FindKernel("FlowSimulation");
        evaporationKernel = erosionShader.FindKernel("Evaporation");
        computeDataMap1 = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeFluxMap = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeVelocityField = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeWaterDeltaMap = new RenderTexture(heightMap.width, heightMap.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        Graphics.Blit(heightMap, computeDataMap1);
        
        WaterSourceStruct[] waterSources = new[] { waterSourceHandler.GetData() };
        waterSourcesBuffer = new ComputeBuffer(waterSources.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(WaterSourceStruct)));
        waterSourcesBuffer.SetData(waterSources);
        
        erosionShader.SetTexture(initKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(initKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        
        erosionShader.SetTexture(waterIncKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(waterIncKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetBuffer(waterIncKernel, "waterSources", waterSourcesBuffer);
        
        erosionShader.SetTexture(flowSimulationKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(flowSimulationKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(flowSimulationKernel, fluxMapShaderProp, computeFluxMap);
        erosionShader.SetTexture(flowSimulationKernel, velocityFieldShaderProp, computeVelocityField);
        
        erosionShader.SetTexture(evaporationKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(evaporationKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        
        erosionShader.SetInt("numWaterSources", waterSources.Length);
        erosionShader.SetFloat("pipeCrossSection", 0.5f);
        erosionShader.SetFloat("lengthPipe", 1f);
        erosionShader.SetFloat("gravity", 9.81f);
        erosionShader.SetFloat("evaporationConst", 0.001f);
        meshGeneration.UpdateHeightMap(computeDataMap1);

        if (debugMesh)
        {
            debugMesh.materials[0].mainTexture = computeFluxMap;
        }
        
        erosionShader.Dispatch(initKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
    }

    private void FixedUpdate()
    {
        erosionShader.SetFloat("simulationTimeStep", Time.fixedDeltaTime);
        erosionShader.Dispatch(waterIncKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(flowSimulationKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(evaporationKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
    }

    private void OnDestroy()
    {
        computeDataMap1.Release();
        computeFluxMap.Release();
        computeVelocityField.Release();
        computeWaterDeltaMap.Release();
        waterSourcesBuffer.Release();
    }
}