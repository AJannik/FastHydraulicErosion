using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterSourceManager : MonoBehaviour
{
    [SerializeField] private EventChannel eventChannel;
    [SerializeField] private bool useRain = false;
    [SerializeField, Range(0f, 1f)] private float rainStrength;
    [SerializeField] private int rainRadius;
    private readonly List<WaterSourceHandler> waterSourceHandlers = new List<WaterSourceHandler>();

    public List<WaterSourceHandler> WaterSourceHandlers => waterSourceHandlers;
    public int NumSources => waterSourceHandlers.Count;

    private bool oldUseRain;
    private float oldStrength;
    private int oldRadius;

    private void Awake()
    {
        UpdateSources();
    }

    private void Start()
    {
        ValueChanged();
    }

    private void Update()
    {
        if (useRain != oldUseRain || oldRadius != rainRadius || Math.Abs(oldStrength - rainStrength) > 0.01f)
        {
            ValueChanged();
        }
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
    
    private void ValueChanged()
    {
        oldRadius = rainRadius;
        oldStrength = rainStrength;
        oldUseRain = useRain;
        
        eventChannel.RaiseRainChanged(useRain, rainStrength, rainRadius);
    }
}