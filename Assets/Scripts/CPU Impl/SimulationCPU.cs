using UnityEngine;

namespace CPU_Impl
{
    public class SimulationCPU : MonoBehaviour
    {
        [SerializeField] private int size = 8;
        [SerializeField] private Texture2D heightMap;
        [SerializeField] private WaterSourceHandler waterSourceHandler;
        [SerializeField] private MeshGeneration meshGeneration;
        
        private float[,] heightValues;
        private float[,] waterHeight;
        private float[,] waterHeightDelta1;
        private float[,] waterHeightDelta2;
        private float[,] sediment;
        private float[,] sedimentDelta;
        private float[,] erosionSum;
        private float[,] depositionSum;
        private float[,] sedimentSum;
        private Vector4[,] fluxValues;
        private Vector2[,] velocityField;
        private WaterSourceStruct[] waterSources;
        private Texture2D tex;

        private float pipeCrossSection = 5f;
        private float gravity = 1f;
        private float lengthPipe = 1f;
        private float sedimentTransportConst = 0.0004f;
        private float dissolvingConst = 0.00003f;
        private float depositionConst = 0.00003f;
        private float evaporationConst = 0.03f;

        private void Start()
        {
            heightValues = new float[size, size];
            waterHeight = new float[size, size];
            waterHeightDelta1 = new float[size, size];
            waterHeightDelta2 = new float[size, size];
            sediment = new float[size, size];
            sedimentDelta = new float[size, size];
            erosionSum = new float[size, size];
            depositionSum = new float[size, size];
            sedimentSum = new float[size, size];
            fluxValues = new Vector4[size, size];
            velocityField = new Vector2[size, size];
            heightMap = Resize(heightMap, size, size);
            tex = new Texture2D(size, size, TextureFormat.RGBAFloat, 1, true);
            
            waterSources = new[] { waterSourceHandler.GetData(meshGeneration.Resolution) };
            
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    heightValues[i, j] = heightMap.GetPixel(i, j).r;
                    tex.SetPixel(i, j, heightMap.GetPixel(i, j));
                }
            }
            
            tex.Apply();
            meshGeneration.UpdateHeightMap(tex);
            meshGeneration.SetPosition(new Vector2(size, size));
        }

        private void Update()
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    WaterInc(i, j);
                }
            }
            
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    CalcFlux(i, j);
                }
            }
            
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    UpdateWaterHeight(i, j);
                }
            }
            
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    UpdateVelocityField(i, j);
                }
            }
            
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Erosion(i, j);
                }
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Transport(i, j);
                }
            }
            
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Evaporation(i, j);
                }
            }
            
            tex.Apply();
            PrintSums();
        }

        private void WaterInc(int x, int y)
        {
            float waterDelta1 = waterHeight[x, y];
                    
            for (int k = 0; k < waterSources.Length; k++)
            {
                if (CreateCircle(x, y, waterSources[k]) && Time.realtimeSinceStartup < 50.0)
                {
                    waterDelta1 += Time.deltaTime * waterSources[k].strength;
                }
            }

            waterHeightDelta1[x, y] = waterDelta1;
        }

        private void CalcFlux(int x, int y)
        {
            float fluxL = 0;
            float fluxR = 0;
            float fluxT = 0;
            float fluxB = 0;
    
            if (x > 0)
            {
                fluxL = Mathf.Max(0f, fluxValues[x, y].x + Time.deltaTime * pipeCrossSection * (gravity * DeltaHeight(x, y, -1, 0) / lengthPipe));
            }
    
            if (x < size - 1)
            {
                fluxR = Mathf.Max(0f, fluxValues[x, y].z + Time.deltaTime * pipeCrossSection * (gravity * DeltaHeight(x, y, 1, 0) / lengthPipe));
            }
    
            if (y < size - 1)
            {
                fluxT = Mathf.Max(0f, fluxValues[x, y].y + Time.deltaTime * pipeCrossSection * (gravity * DeltaHeight(x, y, 0, 1) / lengthPipe));
            }
     
            if (y > 0)
            {
                fluxB = Mathf.Max(0f, fluxValues[x, y].w + Time.deltaTime * pipeCrossSection * (gravity * DeltaHeight(x, y, 0, -1) / lengthPipe));
            }
    
            float k = Mathf.Min(1f, (waterHeightDelta1[x, y] * lengthPipe) / ((fluxL + fluxR + fluxB + fluxT) * Time.deltaTime));
            k = Mathf.Max(k, 0f);
            fluxValues[x, y] = k * new Vector4(fluxL, fluxT, fluxR, fluxB);
        }

        private void UpdateWaterHeight(int x, int y)
        {
            // inflow flux from neighbours to cell (x, y)
            float flowInR = 0;
            if (x < size - 1)
            {
                flowInR = fluxValues[x + 1, y].x;
            }

            float flowInL = 0;
            if (x > 0)
            {
                flowInL = fluxValues[x - 1, y].z;
            }

            float flowInT = 0;
            if (y < size - 1)
            {
                flowInT = fluxValues[x, y + 1].w;
            }

            float flowInB = 0;
            if (y > 0 )
            {
                flowInB = fluxValues[x, y - 1].y;
            }

            float flowIn = flowInR + flowInB + flowInL + flowInT;
            float flowOut = fluxValues[x, y].x + fluxValues[x, y].y + fluxValues[x, y].z + fluxValues[x, y].w;
    
            float deltaVolume = Time.deltaTime * (flowIn - flowOut);
    
            float waterDelta2 = waterHeightDelta1[x, y] + deltaVolume / lengthPipe;
    
            waterHeightDelta2[x, y] = waterDelta2;
        }

        private void UpdateVelocityField(int x, int y)
        {
            float flowInR = 0;
            if (x < size - 1)
            {
                flowInR = fluxValues[x + 1, y].x;
            }

            float flowInL = 0;
            if (x > 0)
            {
                flowInL = fluxValues[x - 1, y].z;
            }

            float flowInT = 0;
            if (y < size - 1)
            {
                flowInT = fluxValues[x, y].w;
            }

            float flowInB = 0;
            if (y > 0)
            {
                flowInB = fluxValues[x, y - 1].y;
            }
    
            // Velocity field
            float averageWaterDelta = (waterHeightDelta1[x, y] + waterHeightDelta2[x, y]) / 2f;
            float deltaFlowX = (flowInL - fluxValues[x, y].x + fluxValues[x, y].z - flowInR) / 2f;
            float deltaFlowY = (flowInT - fluxValues[x, y].y + fluxValues[x, y].w - flowInB) / 2f;

            if (averageWaterDelta == 0.0)
            {
                velocityField[x, y] = Vector2.zero;
            }
            else
            {
                velocityField[x, y] = new Vector2(deltaFlowX, deltaFlowY);
            }
        }

        private void Erosion(int x, int y)
        {
            float alpha = Mathf.Cos(GetHeightMapNormal(x, y).y);
            float velocity = velocityField[x, y].magnitude;
            //alpha = max(alpha, minAlpha);
    
            float capacity = sedimentTransportConst * Mathf.Sin(alpha) * velocity;

            float sedDelta = 0;
            float newHeight = heightValues[x, y];

            if (capacity > sediment[x, y])
            {
                // dissolve
                float dissolve = dissolvingConst * Time.deltaTime;
                dissolve = Mathf.Min(dissolve, capacity - sediment[x, y]);
        
                if (heightValues[x, y] - dissolve >= 0f)
                {
                    newHeight = heightValues[x, y] - dissolve;

                    sedDelta = sediment[x, y] + dissolve;
                    sedimentDelta[x, y] = sedDelta;
                    erosionSum[x, y] += dissolve;
                    sedimentSum[x, y] = sedDelta;
                }
            }
            else
            {
                // deposition
                float deposit = depositionConst * Time.deltaTime;
                deposit = Mathf.Min(deposit, sediment[x, y] - capacity);

                if (sediment[x, y] - deposit >= 0f)
                {
                    newHeight = heightValues[x, y] + deposit;

                    sedDelta = sediment[x, y] - deposit;
                    sedimentDelta[x, y] = sedDelta;
                    depositionSum[x, y] += deposit;
                    sedimentSum[x, y] = sedDelta;
                }
            }
            
            heightValues[x, y] = newHeight;
        }
        
        private void Transport(int x, int y)
        {
            float newX = x - velocityField[x, y].x * Time.deltaTime * size;
            float newY = y - velocityField[x, y].y * Time.deltaTime * size;

            newX = Mathf.Max(newX, 0);
            newX = Mathf.Min(newX, size - 1);
            newY = Mathf.Max(newY, 0);
            newY = Mathf.Min(newY, size - 1);

            int x1 = Mathf.FloorToInt(newX);
            int x2 = Mathf.CeilToInt(newX);
            int y1 = Mathf.FloorToInt(newY);
            int y2 = Mathf.CeilToInt(newY);

            float r1 = Mathf.Lerp(sedimentDelta[x1, y1], sedimentDelta[x2, y1], newX - x1);
            float r2 = Mathf.Lerp(sedimentDelta[x1, y2], sedimentDelta[x2, y2], newX - x1);

            sediment[x, y] = Mathf.Lerp(r1, r2, newY - y1);
        }

        private void Evaporation(int x, int y)
        {
            float water = waterHeightDelta2[x, y] * (1f - evaporationConst * Time.deltaTime);
            if (water < 0.0001f)
            {
                water = 0f;
            }

            waterHeight[x, y] = water;
            tex.SetPixel(x, y, new Color(heightValues[x, y], sediment[x, y], waterHeight[x, y]));
        }

        private Vector3 GetHeightMapNormal(int x, int y)
        {
            // TODO: fix border
            if (x > 0 && x < size - 1 && y > 0 && y < size - 1)
            {
                Vector3 normal = new Vector3(2f * (heightValues[x + 1, y] - heightValues[x - 1, y]), 2f * (heightValues[x, y - 1] - heightValues[x, y + 1]), -4f);
                return normal.normalized;
            }

            return Vector3.up;
        }
        
        float DeltaHeight(int x, int y, int xOffset, int yOffset)
        {
            return heightValues[x, y] + waterHeight[x, y] - heightValues[x + xOffset, y + yOffset] - heightValues[x + xOffset, y + yOffset];
        }
        
        private bool CreateCircle(int x, int y, WaterSourceStruct waterSource)
        {
            Vector2Int uv = new Vector2Int(x, y);
            int dist = (int) Vector2Int.Distance(uv, waterSource.pos);
            dist = Mathf.Abs(dist);
            return dist <= waterSource.radius;
        }
        
        private Texture2D Resize(Texture2D texture2D, int sizeX, int sizeY)
        {
            RenderTexture rt = new RenderTexture(sizeX, sizeY,24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            Texture2D result = new Texture2D(sizeX,sizeY);
            result.ReadPixels(new Rect(0,0,sizeX,sizeY),0,0);
            result.Apply();
            return result;
        }

        private float SumArray(float[,] arr)
        {
            float sum = 0;
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    sum += arr[i, j];
                }
            }

            return sum;
        }

        private void PrintSums()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Dissolve: " + SumArray(erosionSum));
                Debug.Log("Deposition: " + SumArray(depositionSum));
                Debug.Log("In Water: " + SumArray(sediment));
            }
        }
    }
}