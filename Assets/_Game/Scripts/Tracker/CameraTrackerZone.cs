using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CameraTrackerZone : MonoBehaviour
{
    [Header("Spline Reference")]
    public SplineContainer splineContainer;

    [Header("Tracking Distances")]
    public float activeRadius = 2f;
    public float fadeRadius = 2f;

    [Header("LookAt Target")]
    public Transform lookAtTarget;  // <-- DAS FEHLT und ist nötig!

    [Header("Debug Info (read-only)")]
    public float distanceOnSpline;
    public Vector3 closestPoint;

    public float GetWeight(float cameraDistance)
    {
        float delta = Mathf.Abs(cameraDistance - distanceOnSpline);

        if (delta <= activeRadius) return 1f;
        if (delta <= activeRadius + fadeRadius) return Mathf.InverseLerp(activeRadius + fadeRadius, activeRadius, delta);
        return 0f;
    }

    void UpdateAnchor()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;

        SplineUtility.GetNearestPoint(splineContainer.Spline, transform.position, out float3 nearest, out float t);
        closestPoint = nearest;
        float totalLength = SplineUtility.CalculateLength(splineContainer.Spline, transform.localToWorldMatrix);
        distanceOnSpline = totalLength * t;
    }

#if UNITY_EDITOR
    void OnEnable() => UpdateAnchor();
    void OnValidate() => UpdateAnchor();
    void OnDrawGizmosSelected()
    {
        UpdateAnchor();
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, closestPoint);
        Gizmos.DrawSphere(closestPoint, 0.2f);
        Handles.color = new Color(0f, 1f, 0f, 0.2f);
        Handles.DrawSolidDisc(closestPoint, Vector3.up, activeRadius);
        Handles.color = new Color(1f, 1f, 0f, 0.2f);
        Handles.DrawSolidDisc(closestPoint, Vector3.up, activeRadius + fadeRadius);
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
public class CameraTrackerZone : MonoBehaviour
{
    [Header("Spline Reference")]
    public SplineContainer splineContainer;

    [Header("Tracking Distances")]
    public float activeRadius = 2f;
    public float fadeRadius = 2f;

    [Header("Debug Info (read-only)")]
    public float distanceOnSpline;
    public Vector3 closestPoint;

    public float GetWeight(float cameraDistance)
    {
        float delta = Mathf.Abs(cameraDistance - distanceOnSpline);

        if (delta <= activeRadius) return 1f;
        if (delta <= activeRadius + fadeRadius) return Mathf.InverseLerp(activeRadius + fadeRadius, activeRadius, delta);
        return 0f;
    }

    void UpdateAnchor()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;

        SplineUtility.GetNearestPoint(splineContainer.Spline, transform.position, out float3 nearest, out float t);
        closestPoint = nearest;
        float totalLength = SplineUtility.CalculateLength(splineContainer.Spline, transform.localToWorldMatrix);
        distanceOnSpline = totalLength * t;
    }

#if UNITY_EDITOR
    void OnEnable() => UpdateAnchor();
    void OnValidate() => UpdateAnchor();
    void OnDrawGizmosSelected()
    {
        UpdateAnchor();
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, closestPoint);
        Gizmos.DrawSphere(closestPoint, 0.2f);
        Handles.color = new Color(0f, 1f, 0f, 0.2f);
        Handles.DrawSolidDisc(closestPoint, Vector3.up, activeRadius);
        Handles.color = new Color(1f, 1f, 0f, 0.2f);
        Handles.DrawSolidDisc(closestPoint, Vector3.up, activeRadius + fadeRadius);
    }
#endif
}
*/