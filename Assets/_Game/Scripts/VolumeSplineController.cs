using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.Cinemachine;

[ExecuteAlways]
public class VolumeSplineController : MonoBehaviour
{
    [Header("References")]
    public Volume globalVolume;
    public CinemachineCamera cinemachineCamera;

    [Header("Volume Events")]
    public List<VolumeEvent> volumeEvents = new();

    private Fog fog;
    private HDRISky hdriSky;
    private float initialFogHeight;
    private float initialHDRILux;

    void Start()
    {
        InitializeVolume();
    }

    void Update()
    {
        if (cinemachineCamera == null) return;

        var dolly = cinemachineCamera.GetComponent<CinemachineSplineDolly>();
        if (dolly == null || !dolly.IsValid) return;

        float camDistance = dolly.CameraPosition;
        if (volumeEvents.Count == 0) return;

        volumeEvents.Sort((a, b) => a.triggerDistance.CompareTo(b.triggerDistance));

        VolumeEvent from = null;
        VolumeEvent to = null;

        for (int i = 0; i < volumeEvents.Count; i++)
        {
            float blendStart = volumeEvents[i].triggerDistance;
            float blendEnd = blendStart + volumeEvents[i].revealRadius;

            if (camDistance < blendStart)
            {
                to = volumeEvents[i];
                if (i > 0) from = volumeEvents[i - 1];
                break;
            }
            else if (camDistance >= blendStart && camDistance <= blendEnd)
            {
                from = i > 0 ? volumeEvents[i - 1] : null;
                to = volumeEvents[i];

                float t = Mathf.InverseLerp(blendStart, blendEnd, camDistance);
                ApplyVolumeBlend(from, to, t);
                return;
            }
        }

        if (to == null)
        {
            ApplyVolumeEvent(volumeEvents[volumeEvents.Count - 1]);
        }
    }

    void InitializeVolume()
    {
        if (globalVolume == null || globalVolume.profile == null) return;

        globalVolume.profile.TryGet(out fog);
        globalVolume.profile.TryGet(out hdriSky);

        if (fog != null)
            initialFogHeight = fog.baseHeight.value;

        if (hdriSky != null)
            initialHDRILux = hdriSky.desiredLuxValue.value;

        foreach (var ve in volumeEvents)
        {
            if (ve.directionalLight != null)
                ve.initialLightIntensity = ve.directionalLight.intensity;
        }
    }

    void ApplyVolumeEvent(VolumeEvent e)
    {
        if (fog != null)
            fog.baseHeight.value = e.targetFogBaseHeight;

        if (hdriSky != null)
            hdriSky.desiredLuxValue.value = e.targetHDRILuxValue;

        if (e.directionalLight != null && e.directionalLight.type == LightType.Directional)
            e.directionalLight.intensity = e.targetDirectionalIntensity;
    }

    void ApplyVolumeBlend(VolumeEvent from, VolumeEvent to, float t)
    {
        if (fog != null)
        {
            float fromFog = from != null ? from.targetFogBaseHeight : initialFogHeight;
            fog.baseHeight.value = Mathf.Lerp(fromFog, to.targetFogBaseHeight, t);
        }

        if (hdriSky != null)
        {
            float fromLux = from != null ? from.targetHDRILuxValue : initialHDRILux;
            hdriSky.desiredLuxValue.value = Mathf.Lerp(fromLux, to.targetHDRILuxValue, t);
        }

        if (to.directionalLight != null && to.directionalLight.type == LightType.Directional)
        {
            float fromIntensity = from != null && from.directionalLight != null
                ? from.targetDirectionalIntensity
                : to.initialLightIntensity;

            to.directionalLight.intensity = Mathf.Lerp(fromIntensity, to.targetDirectionalIntensity, t);
        }
    }

    [Serializable]
    public class VolumeEvent
    {
        [Tooltip("Spline distance where this event starts to apply.")]
        public float triggerDistance;

        [Tooltip("Smooth transition range in meters after triggerDistance.")]
        public float revealRadius = 5f;

        [Header("Fog Override")]
        public float targetFogBaseHeight;

        [Header("HDRI Sky Override")]
        public float targetHDRILuxValue;

        [Header("Directional Light Control")]
        public Light directionalLight;
        public float targetDirectionalIntensity = 1f;

        [HideInInspector]
        public float initialLightIntensity;
    }
}



/*
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.Cinemachine;

[ExecuteAlways]
public class VolumeSplineController : MonoBehaviour
{
    [Header("References")]
    public Volume globalVolume;
    public CinemachineCamera cinemachineCamera;

    [Header("Volume Events")]
    public List<VolumeEvent> volumeEvents = new();

    private Fog fog;
    private HDRISky hdriSky;
    private float initialFogHeight;
    private float initialHDRILux;

    void Start()
    {
        InitializeVolume();
    }

    void Update()
    {
        if (cinemachineCamera == null) return;

        var dolly = cinemachineCamera.GetComponent<CinemachineSplineDolly>();
        if (dolly == null || !dolly.IsValid) return;

        float camDistance = dolly.CameraPosition;

        foreach (var volumeEvent in volumeEvents)
        {
            float delta = Mathf.Abs(camDistance - volumeEvent.triggerDistance);
            if (delta <= volumeEvent.revealRadius)
            {
                float t = 1f - delta / volumeEvent.revealRadius;

                // Fog
                if (fog != null)
                    fog.baseHeight.value = Mathf.Lerp(initialFogHeight, volumeEvent.targetFogBaseHeight, t);

                // HDRI Sky
                if (hdriSky != null)
                    hdriSky.desiredLuxValue.value = Mathf.Lerp(initialHDRILux, volumeEvent.targetHDRILuxValue, t);

                // Directional Light Intensity
                if (volumeEvent.directionalLight != null && volumeEvent.directionalLight.type == LightType.Directional)
                {
                    float originalIntensity = volumeEvent.initialLightIntensity;
                    float targetIntensity = volumeEvent.targetDirectionalIntensity;

                    volumeEvent.directionalLight.intensity = Mathf.Lerp(originalIntensity, targetIntensity, t);
                }
            }
        }
    }

    void InitializeVolume()
    {
        if (globalVolume == null || globalVolume.profile == null) return;

        globalVolume.profile.TryGet(out fog);
        globalVolume.profile.TryGet(out hdriSky);

        if (fog != null)
            initialFogHeight = fog.baseHeight.value;

        if (hdriSky != null)
            initialHDRILux = hdriSky.desiredLuxValue.value;

        // Initialize light intensities
        foreach (var ve in volumeEvents)
        {
            if (ve.directionalLight != null)
                ve.initialLightIntensity = ve.directionalLight.intensity;
        }
    }

    [Serializable]
    public class VolumeEvent
    {
        [Tooltip("The spline distance where this event begins to trigger.")]
        public float triggerDistance;

        [Tooltip("The radius around the trigger point for a smooth reveal.")]
        public float revealRadius = 5f;

        [Tooltip("Target value for Fog Base Height.")]
        public float targetFogBaseHeight;

        [Tooltip("Target value for HDRI Sky Lux.")]
        public float targetHDRILuxValue;

        [Header("Directional Light Control")]
        public Light directionalLight;

        [Tooltip("Target intensity value for the assigned directional light.")]
        public float targetDirectionalIntensity = 1f;

        [HideInInspector]
        public float initialLightIntensity; // Internal use
    }
}











using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Unity.Cinemachine;

[ExecuteAlways]
public class VolumeSplineController : MonoBehaviour
{
    [Header("References")]
    public Volume globalVolume;
    public CinemachineCamera cinemachineCamera;

    [Header("Volume Events")]
    public List<VolumeEvent> volumeEvents = new();

    private Fog fog;
    private HDRISky hdriSky;
    private float initialFogHeight;
    private float initialHDRILux;

    void Start()
    {
        InitializeVolume();
    }

    void Update()
    {
        if (cinemachineCamera == null) return;

        var dolly = cinemachineCamera.GetComponent<CinemachineSplineDolly>();
        if (dolly == null || !dolly.IsValid) return;

        // ✅ Das ist der korrekte Zugriff auf die Property, die du im Inspector "Position (Distance)" siehst
        float camDistance = dolly.CameraPosition; // entspricht dem Inspector-Feld :contentReference[oaicite:0]{index=0}

        foreach (var volumeEvent in volumeEvents)
        {
            float delta = Mathf.Abs(camDistance - volumeEvent.triggerDistance);
            if (delta <= volumeEvent.revealRadius)
            {
                float t = 1f - delta / volumeEvent.revealRadius;

                if (fog != null)
                    fog.baseHeight.value = Mathf.Lerp(initialFogHeight, volumeEvent.targetFogBaseHeight, t);

                if (hdriSky != null)
                    hdriSky.desiredLuxValue.value = Mathf.Lerp(initialHDRILux, volumeEvent.targetHDRILuxValue, t);
            }
        }
    }

    void InitializeVolume()
    {
        if (globalVolume == null || globalVolume.profile == null) return;
        globalVolume.profile.TryGet(out fog);
        globalVolume.profile.TryGet(out hdriSky);

        if (fog != null)
            initialFogHeight = fog.baseHeight.value;
        if (hdriSky != null)
            initialHDRILux = hdriSky.desiredLuxValue.value;
    }

    [Serializable]
    public class VolumeEvent
    {
        [Tooltip("Spline-Distanz bei der das Event triggern soll.")]
        public float triggerDistance;
        [Tooltip("Radius um den Trigger-Punkt für smooth Blending.")]
        public float revealRadius = 5f;
        [Tooltip("Zielwert für Fog Base Height.")]
        public float targetFogBaseHeight;
        [Tooltip("Zielwert für HDRI Sky Lux.")]
        public float targetHDRILuxValue;
    }
}
*/