using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class LightSplineController2 : MonoBehaviour
{
    [Header("References")]
    public ScrollController scrollController; // Referenz zum ScrollController, der virtualDistance liefert
    public List<LightEvent> lightEvents = new();

    void OnEnable()
    {
        if (scrollController != null)
            scrollController.OnDistanceChanged += OnDistanceChanged;

        CacheInitialIntensity();
        UpdateLights(scrollController != null ? scrollController.virtualDistance : 0f);
    }

    void OnDisable()
    {
        if (scrollController != null)
            scrollController.OnDistanceChanged -= OnDistanceChanged;
    }

    void OnDistanceChanged(float virtualDistance)
    {
        UpdateLights(virtualDistance);
    }

    void CacheInitialIntensity()
    {
        foreach (var e in lightEvents)
        {
            if (e.directionalLight != null)
                e.initialIntensity = e.directionalLight.intensity;
        }
    }

    void UpdateLights(float virtualDistance)
    {
        if (lightEvents.Count == 0) return;

        // Gruppiere Events nach Light
        Dictionary<Light, List<LightEvent>> groupedEvents = new();

        foreach (var e in lightEvents)
        {
            if (e.directionalLight == null) continue;

            if (!groupedEvents.ContainsKey(e.directionalLight))
                groupedEvents[e.directionalLight] = new List<LightEvent>();

            groupedEvents[e.directionalLight].Add(e);
        }

        foreach (var kvp in groupedEvents)
        {
            Light light = kvp.Key;
            List<LightEvent> events = kvp.Value;
            events.Sort((a, b) => a.triggerDistance.CompareTo(b.triggerDistance));

            LightEvent from = null;
            LightEvent to = null;

            // Finde erstes Event mit triggerDistance >= virtualDistance
            for (int i = 0; i < events.Count; i++)
            {
                if (virtualDistance <= events[i].triggerDistance)
                {
                    to = events[i];
                    from = i > 0 ? events[i - 1] : null;
                    break;
                }
            }

            if (to == null)
            {
                // virtualDistance größer als alle triggerDistances → letztes Event dauerhaft
                light.intensity = events[events.Count - 1].targetIntensity;
            }
            else if (from == null)
            {
                // Vor dem ersten Event oder beim ersten Event
                if (virtualDistance < to.triggerDistance - to.revealRadius)
                {
                    // Vor Einflussbereich → initialIntensity
                    light.intensity = to.initialIntensity;
                }
                else
                {
                    // Im Überblendbereich → smooth zwischen initialIntensity und targetIntensity
                    float t = Mathf.InverseLerp(to.triggerDistance - to.revealRadius, to.triggerDistance, virtualDistance);
                    t = t * t * (3f - 2f * t); // Smoothstep
                    light.intensity = Mathf.Lerp(to.initialIntensity, to.targetIntensity, t);
                }
            }
            else
            {
                // Zwischen zwei Events
                float toStart = to.triggerDistance - to.revealRadius;

                if (virtualDistance <= toStart)
                {
                    // Zwischen Events → konstant letzter targetIntensity Wert
                    light.intensity = from.targetIntensity;
                }
                else
                {
                    // Im Überblendbereich → smooth blend vom letzten Zielwert zum neuen Zielwert
                    float t = Mathf.InverseLerp(toStart, to.triggerDistance, virtualDistance);
                    t = t * t * (3f - 2f * t); // Smoothstep
                    light.intensity = Mathf.Lerp(from.targetIntensity, to.targetIntensity, t);
                }
            }
        }
    }

    [Serializable]
    public class LightEvent
    {
        [Tooltip("Spline distance where light reaches 100% of effect.")]
        public float triggerDistance;

        [Tooltip("Blend distance (meters) before triggerDistance.")]
        public float revealRadius = 5f;

        public Light directionalLight;

        [Tooltip("Target intensity for the directional light.")]
        public float targetIntensity = 1f;

        [HideInInspector]
        public float initialIntensity;
    }
}





/*
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class LightSplineController2 : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera cinemachineCamera;
    public ScrollController scrollController;
    public List<LightEvent> lightEvents = new();

    private float initialLightIntensity;

    void Start()
    {
        CacheInitialIntensity();
    }

    private Dictionary<Light, LightEvent> activeEvents = new();

    void Update()
    {
        if (scrollController == null || lightEvents.Count == 0) return;

        float camDistance = scrollController.virtualDistance;

        // Gruppiere Events nach Light
        Dictionary<Light, List<LightEvent>> groupedEvents = new();

        foreach (var e in lightEvents)
        {
            if (e.directionalLight == null) continue;

            if (!groupedEvents.ContainsKey(e.directionalLight))
                groupedEvents[e.directionalLight] = new List<LightEvent>();

            groupedEvents[e.directionalLight].Add(e);
        }

        // Verarbeite jedes Light einzeln
        foreach (var kvp in groupedEvents)
        {
            Light light = kvp.Key;
            List<LightEvent> events = kvp.Value;
            events.Sort((a, b) => a.triggerDistance.CompareTo(b.triggerDistance));

            LightEvent from = null;
            LightEvent to = null;

            for (int i = 0; i < events.Count; i++)
            {
                float end = events[i].triggerDistance;
                float start = end - events[i].revealRadius;

                if (camDistance < start)
                {
                    to = events[i];
                    from = i > 0 ? events[i - 1] : null;
                    break;
                }
                else if (camDistance >= start && camDistance <= end)
                {
                    to = events[i];
                    from = i > 0 ? events[i - 1] : null;
                    float t = Mathf.InverseLerp(start, end, camDistance);
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);
                    float fromVal = from != null ? from.targetIntensity : to.initialIntensity;
                    float targetVal = Mathf.Lerp(fromVal, to.targetIntensity, smoothT);
                    light.intensity = targetVal;
                    goto NextLight;
                }
            }

            // Kein aktiver Bereich gefunden
            if (camDistance < events[0].triggerDistance - events[0].revealRadius)
            {
                light.intensity = events[0].initialIntensity;
            }
            else
            {
                // Nach letztem Event → Zielwert beibehalten
                light.intensity = events[events.Count - 1].targetIntensity;
            }

        NextLight: continue;
        }
    }

    void CacheInitialIntensity()
    {
        foreach (var e in lightEvents)
        {
            if (e.directionalLight != null)
                e.initialIntensity = e.directionalLight.intensity;
        }
    }

    [Serializable]
    public class LightEvent
    {
        [Tooltip("Spline-Distanz, an der das Licht 100% seiner Zielintensität erreicht.")]
        public float triggerDistance;

        [Tooltip("Übergangsbereich in Metern VOR dem Triggerpunkt.")]
        public float revealRadius = 5f;

        public Light directionalLight;

        [Tooltip("Zielintensität des Lichts bei voller Wirkung.")]
        public float targetIntensity = 1f;

        [HideInInspector]
        public float initialIntensity;
    }
}
*/