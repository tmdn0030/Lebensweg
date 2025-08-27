using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ManagedLight
{
    public Light light;
    public float activationRadius;
    public float deactivationRadius;
}

public class LightCullingManager : MonoBehaviour
{
    [Header("Standardwerte")]
    public float defaultActivationRadius = 30f;
    public float defaultDeactivationRadius = 40f;

    public Transform playerTransform;
    public float deactivateDelay = 0.1f;

    [Header("Lichter mit individueller Distanz (optional)")]
    public List<ManagedLight> managedLights = new List<ManagedLight>();

    private Dictionary<Light, float> offTimers = new Dictionary<Light, float>();

#if UNITY_2023_1_OR_NEWER
    private Light[] allLights;
#else
    private Light[] allLights;
#endif

    void Start()
    {
        if (playerTransform == null)
            playerTransform = Camera.main.transform;

#if UNITY_2023_1_OR_NEWER
        allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
        allLights = FindObjectsOfType<Light>(true);
#endif

        // Alle Lights in managedLights aufnehmen, Standardwerte setzen
        foreach (Light l in allLights)
        {
            if (l.type != LightType.Point && l.type != LightType.Spot) continue;
            if (managedLights.Exists(x => x.light == l)) continue; // schon manuell hinzugefügt

            ManagedLight ml = new ManagedLight
            {
                light = l,
                activationRadius = defaultActivationRadius,
                deactivationRadius = defaultDeactivationRadius
            };
            managedLights.Add(ml);
        }
    }

    void Update()
    {
        Vector3 playerPos = playerTransform.position;

        foreach (ManagedLight ml in managedLights)
        {
            if (ml.light == null) continue;

            float dist = Vector3.Distance(playerPos, ml.light.transform.position);

            // Einschalten
            if (!ml.light.enabled && dist <= ml.activationRadius)
            {
                ml.light.enabled = true;
                if (offTimers.ContainsKey(ml.light))
                    offTimers.Remove(ml.light);
            }
            // Ausschalten mit Verzögerung
            else if (ml.light.enabled && dist > ml.deactivationRadius)
            {
                if (!offTimers.ContainsKey(ml.light))
                    offTimers[ml.light] = Time.time + deactivateDelay;

                if (Time.time >= offTimers[ml.light])
                {
                    ml.light.enabled = false;
                    offTimers.Remove(ml.light);
                }
            }
            // Timer zurücksetzen, falls Spieler wieder näher kommt
            else if (ml.light.enabled && dist <= ml.deactivationRadius)
            {
                if (offTimers.ContainsKey(ml.light))
                    offTimers.Remove(ml.light);
            }
        }
    }
}














/*
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ManagedLight
{
    public Light light;
    [Tooltip("Wenn 0, wird der Standardwert verwendet.")]
    public float activationRadius = 0f;
    [Tooltip("Wenn 0, wird der Standardwert verwendet.")]
    public float deactivationRadius = 0f;
}

public class LightCullingManager : MonoBehaviour
{
    [Header("Standardwerte")]
    public float defaultActivationRadius = 30f;
    public float defaultDeactivationRadius = 40f;

    public Transform playerTransform;
    public float deactivateDelay = 0.1f;

    [Header("Individuelle Einstellungen")]
    public List<ManagedLight> managedLights = new List<ManagedLight>();

    private Dictionary<Light, float> offTimers = new Dictionary<Light, float>();

    void Start()
    {
        if (playerTransform == null)
            playerTransform = Camera.main.transform;

#if UNITY_2023_1_OR_NEWER
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
        Light[] allLights = FindObjectsOfType<Light>(true);
#endif

        foreach (Light l in allLights)
        {
            if (l.type == LightType.Point || l.type == LightType.Spot)
            {
                // Prüfen, ob Licht schon in der Liste ist
                if (!managedLights.Exists(x => x.light == l))
                {
                    ManagedLight ml = new ManagedLight
                    {
                        light = l,
                        activationRadius = 0f,      // 0 = Standardwert nutzen
                        deactivationRadius = 0f
                    };
                    managedLights.Add(ml);
                }
            }
        }
    }

    void Update()
    {
        Vector3 playerPos = playerTransform.position;

        foreach (ManagedLight ml in managedLights)
        {
            if (ml.light == null) continue;

            float actRadius = ml.activationRadius > 0 ? ml.activationRadius : defaultActivationRadius;
            float deactRadius = ml.deactivationRadius > 0 ? ml.deactivationRadius : defaultDeactivationRadius;

            float dist = Vector3.Distance(playerPos, ml.light.transform.position);

            // Einschalten
            if (!ml.light.enabled && dist <= actRadius)
            {
                ml.light.enabled = true;
                if (offTimers.ContainsKey(ml.light))
                    offTimers.Remove(ml.light);
            }
            // Ausschalten mit Verzögerung
            else if (ml.light.enabled && dist > deactRadius)
            {
                if (!offTimers.ContainsKey(ml.light))
                    offTimers[ml.light] = Time.time + deactivateDelay;

                if (Time.time >= offTimers[ml.light])
                {
                    ml.light.enabled = false;
                    offTimers.Remove(ml.light);
                }
            }
            // Timer zurücksetzen, falls Spieler wieder näher kommt
            else if (ml.light.enabled && dist <= deactRadius)
            {
                if (offTimers.ContainsKey(ml.light))
                    offTimers.Remove(ml.light);
            }
        }
    }
}


















using UnityEngine;
using System.Collections.Generic;

public class LightCullingManager : MonoBehaviour
{
    [Header("Einstellungen")]
    public float activationRadius = 30f;      // Distanz, ab der Licht eingeschaltet wird
    public float deactivationRadius = 40f;    // Distanz, ab der Licht ausgeschaltet wird
    public Transform playerTransform;         // Spieler oder Kamera
    public float deactivateDelay = 0.1f;      // Verzögerung beim Ausschalten

    private List<Light> managedLights = new List<Light>();
    private Dictionary<Light, float> offTimers = new Dictionary<Light, float>();

    void Start()
    {
        if (playerTransform == null)
            playerTransform = Camera.main.transform;

#if UNITY_2023_1_OR_NEWER
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
        Light[] allLights = FindObjectsOfType<Light>(true);
#endif

        foreach (Light l in allLights)
        {
            if (l.type == LightType.Point || l.type == LightType.Spot)
                managedLights.Add(l);
        }

        // Optional: alle Lights sofort deaktivieren, außer sie stehen in der Nähe
        foreach (Light l in managedLights)
        {
            float dist = Vector3.Distance(playerTransform.position, l.transform.position);
            l.enabled = dist <= activationRadius;
        }
    }

    void Update()
    {
        Vector3 playerPos = playerTransform.position;

        foreach (Light l in managedLights)
        {
            if (l == null) continue;

            float dist = Vector3.Distance(playerPos, l.transform.position);

            // Einschalten sofort
            if (!l.enabled && dist <= activationRadius)
            {
                l.enabled = true;
                if (offTimers.ContainsKey(l))
                    offTimers.Remove(l);
            }
            // Ausschalten mit Verzögerung
            else if (l.enabled && dist > deactivationRadius)
            {
                if (!offTimers.ContainsKey(l))
                    offTimers[l] = Time.time + deactivateDelay;

                if (Time.time >= offTimers[l])
                {
                    l.enabled = false;
                    offTimers.Remove(l);
                }
            }
            // Falls Spieler wieder näher kommt, Timer löschen
            else if (l.enabled && dist <= deactivationRadius)
            {
                if (offTimers.ContainsKey(l))
                    offTimers.Remove(l);
            }
        }
    }
}
*/