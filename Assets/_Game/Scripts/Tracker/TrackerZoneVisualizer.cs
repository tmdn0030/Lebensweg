using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TrackerZoneVisualizer : MonoBehaviour
{
    [Header("Spline Reference")]
    public SplineContainer splineContainer;

    [Header("LookAt Target")]
    public Transform lookAtTarget;

    [Header("Effect Distances (in Metern)")]
    public float fullEffectLength = 2f;
    public float fadeInDistance = 2f;
    public float fadeOutDistance = 5f;

    [Header("Offset & Stärke")]
    [Tooltip("Offset entlang der Spline in Metern (verschiebt den Zentrumspunkt)")]
    public float centerOffset = 0f;

    [Range(0f, 1f)]
    [Tooltip("Effektstärke (0 = kein Effekt, 1 = voller Effekt)")]
    public float effectStrength = 1f;

    [Header("Output für LookAtZone (read-only)")]
    public float fadeInStart;
    public float fullStart;
    public float fullEnd;
    public float fadeOutEnd;

    public Vector3 centerPoint;

    void UpdatePoints()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;

        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);
        if (totalLength <= 0f) return;

        // Punkt entlang Spline finden
        SplineUtility.GetNearestPoint(spline, transform.position, out float3 _, out float t);
        float centerDistance = Mathf.Clamp(totalLength * t + centerOffset, 0f, totalLength);
        centerPoint = DistToPoint(centerDistance);

        // Berechne tatsächliche Distanzen
        fullStart = centerDistance - fullEffectLength / 2f;
        fullEnd = centerDistance + fullEffectLength / 2f;
        fadeInStart = fullStart - fadeInDistance;
        fadeOutEnd = fullEnd + fadeOutDistance;

        // Lokale Methode
        Vector3 DistToPoint(float dist)
        {
            dist = Mathf.Clamp(dist, 0f, totalLength);
            float u = dist / totalLength;
            return (Vector3)SplineUtility.EvaluatePosition(spline, u);
        }
    }

#if UNITY_EDITOR
    void OnEnable() => UpdatePoints();
    void OnValidate() => UpdatePoints();
    void Update() => UpdatePoints();

    void OnDrawGizmosSelected()
    {
        UpdatePoints();
        float baseSize = 0.2f;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(centerPoint, baseSize);

        Gizmos.color = Color.purple;
        Gizmos.DrawSphere(DistToPoint(fullStart), baseSize * 0.8f);
        Gizmos.DrawSphere(DistToPoint(fullEnd), baseSize * 0.8f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(DistToPoint(fadeInStart), baseSize * 0.6f);
        Gizmos.DrawSphere(DistToPoint(fadeOutEnd), baseSize * 0.6f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(DistToPoint(fadeInStart), DistToPoint(fullStart));
        Gizmos.DrawLine(DistToPoint(fullStart), DistToPoint(fullEnd));
        Gizmos.DrawLine(DistToPoint(fullEnd), DistToPoint(fadeOutEnd));

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, centerPoint);

        Vector3 DistToPoint(float dist)
        {
            var spline = splineContainer.Spline;
            float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);
            dist = Mathf.Clamp(dist, 0f, totalLength);
            float u = dist / totalLength;
            return (Vector3)SplineUtility.EvaluatePosition(spline, u);
        }
    }
#endif
}









/*
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TrackerZoneVisualizer : MonoBehaviour
{
    [Header("Spline Reference")]
    public SplineContainer splineContainer;

    [Header("LookAt Target")]
    public Transform lookAtTarget;

    [Header("Effect Distances (in Metern)")]
    public float fullEffectLength = 2f;
    public float fadeInDistance = 2f;
    public float fadeOutDistance = 5f;

    [Header("Offset & Stärke")]
    [Tooltip("Offset entlang der Spline in Metern (verschiebt den Zentrumspunkt)")]
    public float centerOffset = 0f;

    [Range(0f, 1f)]
    [Tooltip("Effektstärke (0 = kein Effekt, 1 = voller Effekt)")]
    public float effectStrength = 1f;

    [Header("Debug Info (read-only)")]
    public Vector3 centerPoint;
    public Vector3 fullStartPoint;
    public Vector3 fullEndPoint;
    public Vector3 fadeInStartPoint;
    public Vector3 fadeOutEndPoint;

    void UpdatePoints()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;

        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);
        if (totalLength <= 0f) return;

        // Find closest point on the spline
        SplineUtility.GetNearestPoint(spline, transform.position, out float3 nearest, out float t);
        float centerDistance = totalLength * t;

        // Apply offset
        centerDistance = Mathf.Clamp(centerDistance + centerOffset, 0f, totalLength);
        centerPoint = DistToPoint(centerDistance);

        float fullStartDist = centerDistance - fullEffectLength / 2f;
        float fullEndDist = centerDistance + fullEffectLength / 2f;

        float fadeInStartDist = fullStartDist - fadeInDistance;
        float fadeOutEndDist = fullEndDist + fadeOutDistance;

        fullStartPoint = DistToPoint(fullStartDist);
        fullEndPoint = DistToPoint(fullEndDist);
        fadeInStartPoint = DistToPoint(fadeInStartDist);
        fadeOutEndPoint = DistToPoint(fadeOutEndDist);

        // Lokale Funktion
        Vector3 DistToPoint(float dist)
        {
            dist = Mathf.Clamp(dist, 0f, totalLength);
            float u = dist / totalLength;
            return (Vector3)SplineUtility.EvaluatePosition(spline, u);
        }
    }

#if UNITY_EDITOR
    void OnEnable() => UpdatePoints();
    void OnValidate() => UpdatePoints();
    void Update() => UpdatePoints();

    void OnDrawGizmosSelected()
    {
        UpdatePoints();

        // Stärke beachten (optional für Gizmo-Größe)
        float baseSize = 0.1f * Mathf.Clamp01(effectStrength);

        // Center Point (grün)
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(centerPoint, baseSize);

        // Full Effect Start/End (blau)
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(fullStartPoint, baseSize * 2f);
        Gizmos.DrawSphere(fullEndPoint, baseSize * 2f);

        // Fade Start/End (gelb)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(fadeInStartPoint, baseSize * 2f);
        Gizmos.DrawSphere(fadeOutEndPoint, baseSize * 2f);

        // Linien
        Gizmos.color = Color.green;
        Gizmos.DrawLine(fadeInStartPoint, fullStartPoint);
        Gizmos.DrawLine(fullStartPoint, fullEndPoint);
        Gizmos.DrawLine(fullEndPoint, fadeOutEndPoint);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, centerPoint);
    }
#endif
}
*/