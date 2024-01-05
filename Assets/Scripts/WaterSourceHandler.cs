using System;
using UnityEngine;

public class WaterSourceHandler : MonoBehaviour
{
    [SerializeField] private EventChannel eventChannel;
    [SerializeField] private float timeToLive;
    [SerializeField, Range(1, 300)] private int radius;
    [SerializeField, Range(0f, 0.5f)] private float strength;

    private Vector2 oldPos;
    private float oldTtl;
    private float oldStrength;
    private int oldRadius;
    
    private void Awake()
    {
        ValueChanged();
    }

    private void Update()
    {
        if (oldPos != new Vector2(transform.position.x, transform.position.z) || Math.Abs(oldTtl - timeToLive) > 0.01f || oldRadius != radius || Math.Abs(oldStrength - strength) > 0.01f)
        {
            eventChannel.RaiseUpdateWaterSource();
            ValueChanged();
        }
    }

    public WaterSourceStruct GetData(float resolution)
    {
        return new WaterSourceStruct(strength, timeToLive, radius, new Vector2(transform.position.x / resolution, transform.position.z / resolution));
    }

    private void ValueChanged()
    {
        oldPos = new Vector2(transform.position.x, transform.position.z);
        oldRadius = radius;
        oldStrength = strength;
        oldTtl = timeToLive;
    }
}