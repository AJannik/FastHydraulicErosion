
using UnityEngine;

public struct WaterSourceStruct
{
    public float strength;
    public int radius;
    public Vector2Int pos;

    public WaterSourceStruct(float strength, int radius, Vector2 position)
    {
        this.strength = strength;
        this.radius = radius;
        pos = new Vector2Int((int)position.x, (int)position.y);
    }
}