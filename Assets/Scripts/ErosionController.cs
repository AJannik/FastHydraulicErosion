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
    private int updateFluxMapKernel;
    private int updateWaterHeightKernel;
    private int updateVelocityFieldKernel;
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
        int size = 128;
        initKernel = erosionShader.FindKernel("Init");
        waterIncKernel = erosionShader.FindKernel("WaterInc");
        updateFluxMapKernel = erosionShader.FindKernel("UpdateFluxMap");
        updateWaterHeightKernel = erosionShader.FindKernel("UpdateWaterHeight");
        updateVelocityFieldKernel = erosionShader.FindKernel("UpdateVelocityField");
        evaporationKernel = erosionShader.FindKernel("Evaporation");
        
        computeDataMap1 = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeFluxMap = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeVelocityField = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeWaterDeltaMap = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        Graphics.Blit(heightMap, computeDataMap1);
        
        WaterSourceStruct[] waterSources = new[] { waterSourceHandler.GetData() };
        waterSourcesBuffer = new ComputeBuffer(waterSources.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(WaterSourceStruct)));
        waterSourcesBuffer.SetData(waterSources);
        
        erosionShader.SetTexture(initKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(initKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        
        erosionShader.SetTexture(waterIncKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(waterIncKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetBuffer(waterIncKernel, "waterSources", waterSourcesBuffer);
        
        erosionShader.SetTexture(updateFluxMapKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(updateFluxMapKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(updateFluxMapKernel, fluxMapShaderProp, computeFluxMap);
        
        erosionShader.SetTexture(updateWaterHeightKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(updateWaterHeightKernel, fluxMapShaderProp, computeFluxMap);
        
        erosionShader.SetTexture(updateVelocityFieldKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(updateVelocityFieldKernel, fluxMapShaderProp, computeFluxMap);
        erosionShader.SetTexture(updateVelocityFieldKernel, velocityFieldShaderProp, computeVelocityField);
        
        erosionShader.SetTexture(evaporationKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(evaporationKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        
        erosionShader.SetInt("numWaterSources", waterSources.Length);
        erosionShader.SetFloat("pipeCrossSection", 1f);
        erosionShader.SetFloat("lengthPipe", 1f);
        erosionShader.SetFloat("gravity", 9.81f);
        erosionShader.SetFloat("evaporationConst", 0.0f);
        erosionShader.SetInt("dimensionX", size);
        erosionShader.SetInt("dimensionY", size);

        meshGeneration.UpdateHeightMap(computeDataMap1);
        meshGeneration.SetPosition(new Vector2(size, size));

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
        erosionShader.Dispatch(updateFluxMapKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(updateWaterHeightKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(updateVelocityFieldKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
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