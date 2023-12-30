using UnityEngine;

public class WaterSourceHandler : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float strength;
    [SerializeField] private float timeToLive;
    [SerializeField, Range(1, 300)] private int radius;

    public WaterSourceStruct GetData()
    {
        return new WaterSourceStruct(strength, timeToLive, radius, new Vector2(transform.position.x, transform.position.z));
    }
}