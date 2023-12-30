using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EventChannel", menuName = "EventChannel", order = 0)]
public class EventChannel : ScriptableObject
{
    public UnityAction OnUpdateWaterSources;

    public void RaiseUpdateWaterSource()
    {
        OnUpdateWaterSources?.Invoke();
    }
}