using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class LightToggleZone : MonoBehaviour
{
    [Header("Ziel-Licht (optional)")]
    [Tooltip("Wenn leer, wird das erste Directional Light in der Szene verwendet.")]
    public Light targetLight;

    [Header("Verhalten")]
    [Tooltip("Nach Fade auf 0 das Light wirklich deaktivieren?")]
    public bool disableLightWhenZero = true;

    private float initialIntensity = 1f;

    // Toggle-Session
    private bool sessionActive = false;
    private bool turnOnThisPass = false;

    private Vector3 entryPosWorld;
    private float startIntensity;
    private float targetIntensity;

    private Collider zoneCol;

    void Awake()
    {
        zoneCol = GetComponent<Collider>();
        if (!zoneCol.isTrigger)
        {
            Debug.LogWarning($"[{name}] Collider ist nicht als Trigger markiert. Setze isTrigger = true.");
            zoneCol.isTrigger = true;
        }

        // Rigidbody sicherstellen (für stabile Trigger-Ereignisse)
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start()
    {
        if (targetLight == null)
        {
            foreach (var l in FindObjectsOfType<Light>())
            {
                if (l.type == LightType.Directional)
                {
                    targetLight = l;
                    break;
                }
            }
        }

        if (targetLight == null)
        {
            Debug.LogError($"[{name}] Kein Directional Light gefunden/zugewiesen.");
            enabled = false;
            return;
        }

        initialIntensity = Mathf.Max(0.0001f, targetLight.intensity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;
        if (targetLight == null) return;

        // Neue Session starten
        sessionActive = true;
        entryPosWorld = other.transform.position;

        // Toggle-Ziel anhand aktuellem Zustand bestimmen
        bool isCurrentlyOn = targetLight.enabled && targetLight.intensity > 0.0001f;
        turnOnThisPass = !isCurrentlyOn;

        startIntensity = targetLight.intensity;
        targetIntensity = turnOnThisPass ? initialIntensity : 0f;

        // Wenn wir hochfaden wollen, sicherstellen, dass das Light aktiviert ist
        if (turnOnThisPass && !targetLight.enabled)
            targetLight.enabled = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!sessionActive || !other.CompareTag("MainCamera")) return;
        if (targetLight == null) return;

        // Progress (0..1) anhand der zurückgelegten Strecke seit Eintritt,
        // normalisiert mit der geschätzten Collider-Länge (AABB).
        Vector3 current = other.transform.position;
        Vector3 travel = current - entryPosWorld;

        float traveled = travel.magnitude;
        float totalToExit = ComputeExitDistanceAABB(entryPosWorld, travel, zoneCol.bounds);

        if (totalToExit <= 0.0001f)
            return; // Schutz gegen Division durch 0

        float t = Mathf.Clamp01(traveled / totalToExit);

        // Smoothstep-Easing
        float k = t * t * (3f - 2f * t);

        targetLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, k);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("MainCamera")) return;
        if (targetLight == null) return;

        // Am Ende sicherstellen, dass das Ziel erreicht wird
        targetLight.intensity = targetIntensity;

        if (Mathf.Approximately(targetIntensity, 0f) && disableLightWhenZero)
            targetLight.enabled = false;

        sessionActive = false;
    }

    /// <summary>
    /// Berechnet die Distanz vom Eintrittspunkt bis zum AABB-Austritt in Bewegungsrichtung.
    /// travel ist der bisherige Bewegungsvektor seit Eintritt.
    /// </summary>
    private float ComputeExitDistanceAABB(Vector3 entry, Vector3 travel, Bounds bounds)
    {
        if (travel.sqrMagnitude < 0.0001f)
            return bounds.size.magnitude; // Fallback

        Vector3 dir = travel.normalized;

        // Raycast gegen die AABB-Grenzen
        float tMin = float.MaxValue;

        // X
        if (Mathf.Abs(dir.x) > 0.0001f)
        {
            float tx = (dir.x > 0 ? bounds.max.x - entry.x : bounds.min.x - entry.x) / dir.x;
            if (tx > 0) tMin = Mathf.Min(tMin, tx);
        }
        // Y
        if (Mathf.Abs(dir.y) > 0.0001f)
        {
            float ty = (dir.y > 0 ? bounds.max.y - entry.y : bounds.min.y - entry.y) / dir.y;
            if (ty > 0) tMin = Mathf.Min(tMin, ty);
        }
        // Z
        if (Mathf.Abs(dir.z) > 0.0001f)
        {
            float tz = (dir.z > 0 ? bounds.max.z - entry.z : bounds.min.z - entry.z) / dir.z;
            if (tz > 0) tMin = Mathf.Min(tMin, tz);
        }

        if (tMin == float.MaxValue) tMin = bounds.size.magnitude;
        return Mathf.Max(0.0001f, tMin);
    }
}
