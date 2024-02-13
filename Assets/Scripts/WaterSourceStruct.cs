
using UnityEngine;

public struct WaterSourceStruct
{
    public float strength;
    public float ttl;
    public int radius;
    public Vector2Int pos;

    public WaterSourceStruct(float strength, float ttl, int radius, Vector2 position)
    {
        this.strength = strength;
        this.ttl = ttl;
        this.radius = radius;
        pos = new Vector2Int((int)position.x, (int)position.y);
    }
}