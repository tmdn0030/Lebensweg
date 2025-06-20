using UnityEngine;

public class DistanceTriggerManager : MonoBehaviour
{
    [Tooltip("Referenz auf das ScrollController-Script, das die virtuelle Distanz verwaltet.")]
    public ScrollController scroller;

    private DistanceEventTrigger[] allTriggers;

    void Start()
    {
        if (scroller == null)
        {
            Debug.LogError("ScrollController-Referenz fehlt im DistanceTriggerManager!", this);
            return;
        }

        allTriggers = Object.FindObjectsByType<DistanceEventTrigger>(FindObjectsSortMode.None);
        scroller.OnDistanceChanged += EvaluateAllTriggers;
    }

    void OnDestroy()
    {
        if (scroller != null)
            scroller.OnDistanceChanged -= EvaluateAllTriggers;
    }

    void EvaluateAllTriggers(float virtualDistance)
    {
        foreach (var trigger in allTriggers)
        {
            trigger.Evaluate(virtualDistance);
        }
    }
}

