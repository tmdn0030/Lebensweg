using System.Collections.Generic;
using UnityEngine;

public class CameraLightRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;              // Radius, in dem Licht voll leuchtet
    public float deactivateExtension = 2f;      // Extra-Abstand, ab dem Licht deaktiviert wird
    public float fadeTime = 1f;                 // Dauer für komplettes Ein-/Ausblenden in Sekunden
    public bool deactivateFarLights = true;     // Licht komplett deaktivieren, wenn weit genug weg

    private List<Light> lightsList = new List<Light>();
    private Dictionary<Light, float> originalIntensity = new Dictionary<Light, float>();
    private Dictionary<Light, float> currentIntensity = new Dictionary<Light, float>();

    void Start()
    {
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (Light l in allLights)
        {
            if (l.type != LightType.Spot) continue;

            lightsList.Add(l);
            originalIntensity[l] = l.intensity; // Original speichern
            currentIntensity[l] = 0f;           // Start bei 0
            l.intensity = 0f;                   // Aus
        }
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var light in lightsList)
        {
            if (light == null) continue;

            float dist = Vector3.Distance(camPos, light.transform.position);

            // 1️⃣ Deaktivieren wenn zu weit weg
            if (deactivateFarLights && dist > fadeRadius + deactivateExtension)
            {
                if (light.gameObject.activeSelf)
                    light.gameObject.SetActive(false);
                continue;
            }
            else
            {
                if (!light.gameObject.activeSelf)
                    light.gameObject.SetActive(true);
            }

            // 2️⃣ Zielintensität setzen
            float target = dist <= fadeRadius ? originalIntensity[light] : 0f;

            // 3️⃣ Lerp mit Sekundenangabe
            float step = (fadeTime > 0f) ? Time.deltaTime / fadeTime : 1f;
            currentIntensity[light] = Mathf.Lerp(currentIntensity[light], target, step);

            light.intensity = currentIntensity[light];
        }
    }
}









/*
using System.Collections.Generic;
using UnityEngine;

public class CameraLightRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;
    public float deactivateExtension = 2f;
    public float fadeTime = 1f;
    public bool deactivateFarLights = true;

    private float originalFadeRadius;
    private float originalDeactivateExtension;

    private List<Light> lightsList = new List<Light>();
    private Dictionary<Light, float> originalIntensity = new Dictionary<Light, float>();
    private Dictionary<Light, float> currentIntensity = new Dictionary<Light, float>();

    void Start()
    {
        originalFadeRadius = fadeRadius;
        originalDeactivateExtension = deactivateExtension;

        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (Light l in allLights)
        {
            if (l.type != LightType.Spot) continue;

            lightsList.Add(l);
            originalIntensity[l] = l.intensity;
            currentIntensity[l] = 0f;
            l.intensity = 0f;
        }
    }

    public void SetTemporaryFadeRadius(float newRadius)
    {
        float erweiterung = newRadius - originalFadeRadius;

        fadeRadius = newRadius;
        deactivateExtension = originalDeactivateExtension + erweiterung;
    }

    public void ResetFadeRadius()
    {
        fadeRadius = originalFadeRadius;
        deactivateExtension = originalDeactivateExtension;
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var light in lightsList)
        {
            if (light == null) continue;

            float dist = Vector3.Distance(camPos, light.transform.position);

            if (deactivateFarLights && dist > fadeRadius + deactivateExtension)
            {
                if (light.gameObject.activeSelf)
                    light.gameObject.SetActive(false);
                continue;
            }
            else
            {
                if (!light.gameObject.activeSelf)
                    light.gameObject.SetActive(true);
            }

            float target = dist <= fadeRadius ? originalIntensity[light] : 0f;

            float step = (fadeTime > 0f) ? Time.deltaTime / fadeTime : 1f;
            currentIntensity[light] = Mathf.Lerp(currentIntensity[light], target, step);

            light.intensity = currentIntensity[light];
        }
    }
}








using System.Collections.Generic;
using UnityEngine;

public class CameraLightRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;              // Radius, in dem Licht voll leuchtet
    public float deactivateExtension = 2f;      // Extra-Abstand, ab dem Licht deaktiviert wird
    public float fadeTime = 1f;                 // Dauer für komplettes Ein-/Ausblenden in Sekunden
    public bool deactivateFarLights = true;     // Licht komplett deaktivieren, wenn weit genug weg

    private List<Light> lightsList = new List<Light>();
    private Dictionary<Light, float> originalIntensity = new Dictionary<Light, float>();
    private Dictionary<Light, float> currentIntensity = new Dictionary<Light, float>();

    void Start()
    {
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        foreach (Light l in allLights)
        {
            if (l.type != LightType.Spot) continue;

            lightsList.Add(l);
            originalIntensity[l] = l.intensity; // Original speichern
            currentIntensity[l] = 0f;           // Start bei 0
            l.intensity = 0f;                   // Aus
        }
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var light in lightsList)
        {
            if (light == null) continue;

            float dist = Vector3.Distance(camPos, light.transform.position);

            // 1️⃣ Deaktivieren wenn zu weit weg
            if (deactivateFarLights && dist > fadeRadius + deactivateExtension)
            {
                if (light.gameObject.activeSelf)
                    light.gameObject.SetActive(false);
                continue;
            }
            else
            {
                if (!light.gameObject.activeSelf)
                    light.gameObject.SetActive(true);
            }

            // 2️⃣ Zielintensität setzen
            float target = dist <= fadeRadius ? originalIntensity[light] : 0f;

            // 3️⃣ Lerp mit Sekundenangabe
            float step = (fadeTime > 0f) ? Time.deltaTime / fadeTime : 1f;
            currentIntensity[light] = Mathf.Lerp(currentIntensity[light], target, step);

            light.intensity = currentIntensity[light];
        }
    }
}
*/