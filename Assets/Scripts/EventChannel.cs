using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EventChannel", menuName = "EventChannel", order = 0)]
public class EventChannel : ScriptableObject
{
    public UnityAction OnUpdateWaterSources;
    public UnityAction<bool, float, int> OnRainChanged;

    public void RaiseUpdateWaterSource()
    {
        OnUpdateWaterSources?.Invoke();
    }

    public void RaiseRainChanged(bool useRain, float strength, int radius)
    {
        OnRainChanged?.Invoke(useRain, strength, radius);
    }
}