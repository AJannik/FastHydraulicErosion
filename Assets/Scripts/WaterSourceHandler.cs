using UnityEngine;

public class WaterSourceHandler : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float strength;
    [SerializeField, Range(1, 300)] private int radius;

    public WaterSourceStruct GetData()
    {
        return new WaterSourceStruct(strength, radius, transform.position);
    }
}