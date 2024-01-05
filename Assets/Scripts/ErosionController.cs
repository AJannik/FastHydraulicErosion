using UnityEngine;

public class ErosionController : MonoBehaviour
{
    [SerializeField] private int size = 512;
    [SerializeField] private ComputeShader erosionShader;
    [SerializeField] private Texture2D heightMap;
    [SerializeField] private MeshGeneration meshGeneration;
    [SerializeField] private MeshRenderer debugMesh;
    [SerializeField] private WaterSourceManager waterSourceManager;
    [SerializeField] private EventChannel eventChannel;
    
    private int initKernel;
    private int waterIncKernel;
    private int updateFluxMapKernel;
    private int updateWaterHeightKernel;
    private int updateVelocityFieldKernel;
    private int erosionKernel;
    private int transportKernel;
    private int evaporationKernel;
    private RenderTexture computeDataMap1; // r = heightMap, g = suspended sediment, b = water level
    private RenderTexture computeFluxMap; // pipe model flux field
    private RenderTexture computeVelocityField; // velocity field
    private RenderTexture computeWaterDeltaMap; // r = delta1, g = delta2
    private RenderTexture sedimentDeltaMap; // r = delta1, g = delta2
    private ComputeBuffer waterSourcesBuffer;
    
    private readonly int dataMap1ShaderProp = Shader.PropertyToID("dataMap1");
    private readonly int fluxMapShaderProp = Shader.PropertyToID("fluxMap");
    private readonly int velocityFieldShaderProp = Shader.PropertyToID("velocityField");
    private readonly int waterDeltaMapShaderProp = Shader.PropertyToID("waterDeltaMap");
    private readonly int sedimentDeltaMapShaderProp = Shader.PropertyToID("sedimentDeltaMap");

    private void OnEnable()
    {
        eventChannel.OnUpdateWaterSources += SetWaterSources;
    }

    private void OnDisable()
    {
        eventChannel.OnUpdateWaterSources -= SetWaterSources;
    }

    private void Start()
    {
        initKernel = erosionShader.FindKernel("Init");
        waterIncKernel = erosionShader.FindKernel("WaterInc");
        updateFluxMapKernel = erosionShader.FindKernel("UpdateFluxMap");
        updateWaterHeightKernel = erosionShader.FindKernel("UpdateWaterHeight");
        updateVelocityFieldKernel = erosionShader.FindKernel("UpdateVelocityField");
        erosionKernel = erosionShader.FindKernel("Erosion");
        transportKernel = erosionShader.FindKernel("Transportation");
        evaporationKernel = erosionShader.FindKernel("Evaporation");
        
        meshGeneration.CreateMesh(size);
        
        computeDataMap1 = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeFluxMap = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeVelocityField = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        computeWaterDeltaMap = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        sedimentDeltaMap = new RenderTexture(size, size, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB) {enableRandomWrite = true};
        Graphics.Blit(heightMap, computeDataMap1);
        
        SetWaterSources();
        
        erosionShader.SetTexture(initKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(initKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(initKernel, velocityFieldShaderProp, computeVelocityField);
        erosionShader.SetTexture(initKernel, fluxMapShaderProp, computeFluxMap);
        
        erosionShader.SetTexture(waterIncKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(waterIncKernel, dataMap1ShaderProp, computeDataMap1);

        erosionShader.SetTexture(updateFluxMapKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(updateFluxMapKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(updateFluxMapKernel, fluxMapShaderProp, computeFluxMap);
        
        erosionShader.SetTexture(updateWaterHeightKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(updateWaterHeightKernel, fluxMapShaderProp, computeFluxMap);
        
        erosionShader.SetTexture(updateVelocityFieldKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(updateVelocityFieldKernel, fluxMapShaderProp, computeFluxMap);
        erosionShader.SetTexture(updateVelocityFieldKernel, velocityFieldShaderProp, computeVelocityField);
        
        erosionShader.SetTexture(erosionKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(erosionKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(erosionKernel, velocityFieldShaderProp, computeVelocityField);
        erosionShader.SetTexture(erosionKernel, sedimentDeltaMapShaderProp, sedimentDeltaMap);
        
        erosionShader.SetTexture(transportKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        erosionShader.SetTexture(transportKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(transportKernel, velocityFieldShaderProp, computeVelocityField);
        erosionShader.SetTexture(transportKernel, sedimentDeltaMapShaderProp, sedimentDeltaMap);
        
        erosionShader.SetTexture(evaporationKernel, dataMap1ShaderProp, computeDataMap1);
        erosionShader.SetTexture(evaporationKernel, waterDeltaMapShaderProp, computeWaterDeltaMap);
        
        erosionShader.SetFloat("pipeCrossSection", 5f);
        erosionShader.SetFloat("lengthPipe", 1f);
        erosionShader.SetFloat("gravity", 1f);
        erosionShader.SetFloat("minAlpha", 0.0f);
        erosionShader.SetFloat("sedimentTransportConst", 0.0004f);
        erosionShader.SetFloat("dissolvingConst", 0.00003f);
        erosionShader.SetFloat("depositionConst", 0.00003f);
        erosionShader.SetFloat("evaporationConst", 0.3f);
        erosionShader.SetInt("dimensionX", size);
        erosionShader.SetInt("dimensionY", size);

        meshGeneration.UpdateHeightMap(computeDataMap1);
        meshGeneration.SetPosition(new Vector2(size, size));

        if (debugMesh)
        {
            debugMesh.materials[0].mainTexture = computeWaterDeltaMap;
        }
        
        erosionShader.Dispatch(initKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
    }

    private void Update()
    {
        erosionShader.SetFloat("simulationTimeStep", Time.deltaTime);
        erosionShader.SetFloat("runTime", Time.time);
        erosionShader.Dispatch(waterIncKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(updateFluxMapKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(updateWaterHeightKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(updateVelocityFieldKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(erosionKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(transportKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        erosionShader.Dispatch(evaporationKernel, computeDataMap1.width / 8, computeDataMap1.height / 8, 1);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Texture2D tex = ToTexture2D(sedimentDeltaMap);
            Color color = SumTextureChannel(tex);
            Debug.Log("Dissolve: " + color.r);
            Debug.Log("Deposition: " + color.g);
            Debug.Log("In Water: " + color.b);
        }
    }

    private Color SumTextureChannel(Texture2D texture)
    {
        Color sum = Color.black;
        
        foreach (Color color in texture.GetPixels())
        {
            sum += color;
        }

        return sum;
    }
    
    private Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBAFloat, 1, true);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    private void SetWaterSources()
    {
        waterSourcesBuffer?.Release();
        WaterSourceStruct[] waterSources = new WaterSourceStruct[waterSourceManager.NumSources];
        for (var index = 0; index < waterSourceManager.WaterSourceHandlers.Count; index++)
        {
            waterSources[index] = waterSourceManager.WaterSourceHandlers[index].GetData(meshGeneration.Resolution);
        }

        waterSourcesBuffer = new ComputeBuffer(waterSources.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(WaterSourceStruct)));
        waterSourcesBuffer.SetData(waterSources);
        erosionShader.SetInt("numWaterSources", waterSources.Length);
        erosionShader.SetBuffer(waterIncKernel, "waterSources", waterSourcesBuffer);
    }

    private void OnDestroy()
    {
        computeDataMap1.Release();
        computeFluxMap.Release();
        computeVelocityField.Release();
        computeWaterDeltaMap.Release();
        waterSourcesBuffer?.Release();
    }
}