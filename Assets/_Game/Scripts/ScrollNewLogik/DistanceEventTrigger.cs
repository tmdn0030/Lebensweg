using UnityEngine;

public abstract class DistanceEventTrigger : MonoBehaviour
{
    public float triggerDistance = 10f;
    public float activationRadius = 5f;

    private bool hasActivated = false;
    private float lastProgress = 0f;

    public virtual void Evaluate(float virtualDistance)
    {
        float distanceToTrigger = Mathf.Abs(virtualDistance - triggerDistance);
        float t = 1f - Mathf.Clamp01(distanceToTrigger / activationRadius);

        OnProgress(t);

        if (!hasActivated && virtualDistance >= triggerDistance)
        {
            OnTriggered();
            hasActivated = true;
        }

        lastProgress = t;
    }

    protected abstract void OnProgress(float t);
    protected abstract void OnTriggered();
}
