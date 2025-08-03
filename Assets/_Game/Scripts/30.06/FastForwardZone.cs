using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class FastForwardZone : MonoBehaviour
{
    [Header("Spline-Zone")]
    public SplineContainer spline;
    [Tooltip("Startpunkt auf der Spline in Metern")]
    public float startDistance = 0f;

    [Tooltip("Länge der Zone (in Metern)")]
    public float zoneLength = 2f;

    [Tooltip("Mit Objekt mitwandern")]
    public bool followTransform = false;

    [HideInInspector] public float offsetToSpline = 0f;
    private bool lastFollowTransform = false;

    [Tooltip("Endpunkt auf der Spline (berechnet, read-only)")]
    [SerializeField]
    public float endDistance;

    [Header("Fast Forward Zone Werte")]
    [Tooltip("Fahrgeschwindigkeit innerhalb der Fast Forward Zone")]
    public float fastForwardSpeed = 0.1f;

    [Tooltip("Dämpfung (Ease) am Anfang und Ende der Zone")]
    public EaseType easeType = EaseType.SmoothStep;

    [Header("Verhalten beim Verlassen")]
    public bool resetOnExit = false;
    public float defaultSpeed = 0.01f;

    [Header("Dämpfung am Start und Ende")]
    public float easingDuration = 2f;

    public enum EaseType
    {
        Linear,
        SmoothStep,
        EaseIn,
        EaseOut,
        EaseInOut,
        SineWave
    }

    void OnValidate() => UpdateZone();
    void Update() => UpdateZone();

    private void UpdateZone()
    {
        if (spline == null || spline.Spline == null) return;
        float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);

        // Wechsel von followTransform merken
        if (followTransform != lastFollowTransform)
        {
            if (followTransform) CacheOffsetToSpline();
            lastFollowTransform = followTransform;
        }

        if (followTransform)
            UpdateStartDistanceFromTransform();

        startDistance = Mathf.Clamp(startDistance, 0, totalLength - zoneLength);
        zoneLength = Mathf.Clamp(zoneLength, 0, totalLength - startDistance);
        endDistance = startDistance + zoneLength;
    }

    private void CacheOffsetToSpline()
    {
        if (spline == null || spline.Spline == null) return;
        float3 nearest;
        float t;
        SplineUtility.GetNearestPoint(spline.Spline, (float3)transform.position, out nearest, out t);
        float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);
        float posOnSpline = Mathf.Clamp(totalLength * t, 0, totalLength - zoneLength);
        offsetToSpline = startDistance - posOnSpline;
    }

    private void UpdateStartDistanceFromTransform()
    {
        if (spline == null || spline.Spline == null) return;
        float3 nearest;
        float t;
        SplineUtility.GetNearestPoint(spline.Spline, (float3)transform.position, out nearest, out t);
        float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);
        float posOnSpline = Mathf.Clamp(totalLength * t, 0, totalLength - zoneLength);
        startDistance = Mathf.Clamp(posOnSpline + offsetToSpline, 0, totalLength - zoneLength);
    }

    // Öffentliche Methode, um EaseProgress aus der anderen Klasse aufzurufen
    public float GetProgressBasedOnCurrentPosition(float currentPosition)
    {
        return GetEaseProgress(currentPosition);
    }

    // Berechnet eine "Ease" für die Geschwindigkeit basierend auf der Position in der Zone
    public float GetEaseProgress(float currentPosition)
    {
        float progress = Mathf.InverseLerp(startDistance, endDistance, currentPosition);

        switch (easeType)
        {
            case EaseType.Linear: return progress;
            case EaseType.SmoothStep: return Mathf.SmoothStep(0f, 1f, progress);
            case EaseType.EaseIn: return progress * progress;
            case EaseType.EaseOut: return progress * (2 - progress);
            case EaseType.EaseInOut: return progress < 0.5f ? 2 * progress * progress : -1 + (4 - 2 * progress) * progress;
            case EaseType.SineWave: return 0.5f - 0.5f * Mathf.Cos(Mathf.PI * progress);
            default: return progress;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (spline != null && spline.Spline != null)
        {
            float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);

            float startT = Mathf.Clamp01(startDistance / totalLength);
            float endT = Mathf.Clamp01(endDistance / totalLength);

            Vector3 startPos = (Vector3)SplineUtility.EvaluatePosition(spline.Spline, startT);
            Vector3 endPos = (Vector3)SplineUtility.EvaluatePosition(spline.Spline, endT);

            // Visualisierung der Zone
            Gizmos.color = Color.cyan;  // Farbe für Fast Forward Zone
            Gizmos.DrawWireSphere(startPos, 0.16f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(endPos, 0.16f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, endPos);
        }
    }
#endif
}
