using System.Collections.Generic;
using UnityEngine;

public class WaterSourceManager : MonoBehaviour
{
    [SerializeField] private EventChannel eventChannel;
    private readonly List<WaterSourceHandler> waterSourceHandlers = new List<WaterSourceHandler>();

    public List<WaterSourceHandler> WaterSourceHandlers => waterSourceHandlers;
    public int NumSources => waterSourceHandlers.Count;

    private void Awake()
    {
        UpdateSources();
    }

    private void OnTransformChildrenChanged()
    {
        UpdateSources();
    }

    private void UpdateSources()
    {
        waterSourceHandlers.Clear();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            WaterSourceHandler handler = transform.GetChild(i).GetComponent<WaterSourceHandler>();
            if (handler)
            {
                waterSourceHandlers.Add(handler);
            }
        }
        
        eventChannel.RaiseUpdateWaterSource();
    }
}