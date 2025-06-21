using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using UnityEngine.Splines;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SplineEventManager : MonoBehaviour
{
    [Header("Verbindung")]
    public ScrollController scrollController;
    public SplineContainer splineContainer; // Optional für exakte Gizmo-Positionen

    [Header("Spline Events")]
    public List<SplineEvent> events = new();

    void Update()
    {
        if (scrollController == null || events.Count == 0)
            return;

        float currentDistance = scrollController.virtualDistance;

        foreach (var e in events)
        {
            float distToTrigger = Mathf.Abs(currentDistance - e.triggerDistance);

            if (distToTrigger <= e.triggerRadius)
            {
                if (!e.triggerOnce || !e.hasFired)
                {
                    // UnityEvent auslösen
                    e.onTriggered?.Invoke();

                    // Optional: AnimationClip abspielen
                    if (e.animationClip != null && e.targetAnimator != null)
                    {
                        e.targetAnimator.Play(e.animationClip.name, 0, 0f);
                    }

                    e.hasFired = true;
                }
            }
            else if (!e.triggerOnce)
            {
                e.hasFired = false;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (events == null || events.Count == 0)
            return;

        foreach (var e in events)
        {
            Vector3 pos = transform.position + Vector3.forward * e.triggerDistance;

            // Wenn Spline referenziert ist → echte Kurvenposition berechnen
            if (splineContainer != null && splineContainer.Spline != null)
            {
                float splineLength = SplineUtility.CalculateLength(splineContainer.Spline, splineContainer.transform.localToWorldMatrix);
                float t = Mathf.Clamp01(e.triggerDistance / splineLength);
                Vector3 splinePos = splineContainer.Spline.EvaluatePosition(t);
                pos = splineContainer.transform.TransformPoint(splinePos);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(pos, 0.3f);

#if UNITY_EDITOR
            Handles.Label(pos, $"Event: {e.eventName}\n@ {e.triggerDistance:F1}m");
#endif
        }
    }
}

[System.Serializable]
public class SplineEvent
{
    [Tooltip("Interner Name nur zur Übersicht")]
    public string eventName;

    [Tooltip("Triggerposition entlang der Spline (in Metern)")]
    public float triggerDistance;

    [Tooltip("Wie nah muss der Scroller sein, damit ausgelöst wird?")]
    public float triggerRadius = 1f;

    [Tooltip("Nur einmal auslösen?")]
    public bool triggerOnce = true;

    [Tooltip("Optional: UnityEvents, die beim Trigger ausgelöst werden")]
    public UnityEvent onTriggered;

    [Tooltip("Optional: AnimationClip, der abgespielt wird")]
    public AnimationClip animationClip;

    [Tooltip("Animator, auf dem der Clip abgespielt werden soll")]
    public Animator targetAnimator;

    [HideInInspector]
    public bool hasFired;
}
