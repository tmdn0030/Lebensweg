using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SpeedZone : MonoBehaviour
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

    [Header("Speedzone Werte")]
    public float newScrollSpeed = 0.03f;
    public float easeDuration = 1.0f;
    public EaseType ease = EaseType.SmoothStep;

    [Header("Verhalten beim Verlassen")]
    public bool resetOnExit = false;
    public float defaultSpeed = 0.01f;
    public float resetEaseDuration = 1.0f;
    public EaseType resetEase = EaseType.SmoothStep;

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

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPos, 0.16f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPos, 0.16f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, endPos);
        }
    }
#endif
}
