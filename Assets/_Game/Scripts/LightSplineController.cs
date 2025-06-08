using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class LightSplineController : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera cinemachineCamera;
    public List<LightEvent> lightEvents = new();

    private float initialLightIntensity;

    void Start()
    {
        CacheInitialIntensity();
    }

    private Dictionary<Light, LightEvent> activeEvents = new();

    void Update()
    {
        if (cinemachineCamera == null || lightEvents.Count == 0) return;

        var dolly = cinemachineCamera.GetComponent<CinemachineSplineDolly>();
        if (dolly == null || !dolly.IsValid) return;

        float camDistance = dolly.CameraPosition;

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
                float start = events[i].triggerDistance;
                float end = start + events[i].revealRadius;

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
                    float fromVal = from != null ? from.targetIntensity : to.initialIntensity;
                    float targetVal = Mathf.Lerp(fromVal, to.targetIntensity, t);
                    light.intensity = targetVal;
                    goto NextLight;
                }
            }

            // Wenn kein aktives Event-Bereich gefunden wurde:
            if (camDistance < events[0].triggerDistance)
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

    void ApplyLightEvent(LightEvent e)
    {
        if (e.directionalLight != null && e.directionalLight.type == LightType.Directional)
            e.directionalLight.intensity = e.targetIntensity;
    }

    void ApplyLightBlend(LightEvent from, LightEvent to, float t)
    {
        if (to.directionalLight == null || to.directionalLight.type != LightType.Directional)
            return;

        float fromIntensity = from != null ? from.targetIntensity : to.initialIntensity;
        to.directionalLight.intensity = Mathf.Lerp(fromIntensity, to.targetIntensity, t);
    }

    [Serializable]
    public class LightEvent
    {
        [Tooltip("Spline distance where light starts transitioning.")]
        public float triggerDistance;

        [Tooltip("Blend distance in meters.")]
        public float revealRadius = 5f;

        public Light directionalLight;

        [Tooltip("Target intensity for the directional light.")]
        public float targetIntensity = 1f;

        [HideInInspector]
        public float initialIntensity;
    }
}
