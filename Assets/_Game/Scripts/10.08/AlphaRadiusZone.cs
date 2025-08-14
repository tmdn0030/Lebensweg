/*
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class AlphaRadiusZone : MonoBehaviour
{
    [Header("Neuer Fade Radius in Zone")]
    public float neuerFadeRadius = 25f; // ersetzt den Originalwert innerhalb der Zone

    private void OnTriggerEnter(Collider other)
    {
        CameraRadiusActivator activator = other.GetComponent<CameraRadiusActivator>();
        if (activator != null)
        {
            activator.SetTemporaryRadius(neuerFadeRadius);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CameraRadiusActivator activator = other.GetComponent<CameraRadiusActivator>();
        if (activator != null)
        {
            activator.ResetRadius();
        }
    }
}












using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class AlphaRadiusZone : MonoBehaviour
{
    [Header("Spline Reference")]
    public SplineContainer splineContainer;

    [Header("Kamera mit CameraRadiusActivator")]
    public CameraRadiusActivator activator;

    [Header("Zone Settings")]
    public float fullEffectLength = 2f;
    public float fadeInDistance = 2f;
    public float fadeOutDistance = 5f;
    public float centerOffset = 0f;

    // Runtime vars
    private bool isInside;

    // Calculated points
    private float fadeInStart;
    private float fullStart;
    private float fullEnd;
    private float fadeOutEnd;
    private Vector3 centerPoint;

    void Update()
    {
        if (splineContainer == null || activator == null) return;

        UpdateZonePoints();

        float camDist = GetCameraDistance();

        bool nowInside = camDist >= fadeInStart && camDist <= fadeOutEnd;

        if (nowInside && !isInside)
        {
            // Betreten der Zone → FadeIn starten
            foreach (var r in activator.Renderers)
            {
                activator.StartFade(r, true);
            }
            isInside = true;
        }
        else if (!nowInside && isInside)
        {
            // Verlassen der Zone → FadeOut starten
            foreach (var r in activator.Renderers)
            {
                activator.StartFade(r, false);
            }
            isInside = false;
        }
    }

    void UpdateZonePoints()
    {
        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);

        SplineUtility.GetNearestPoint(spline, transform.position, out float3 _, out float t);
        float centerDistance = Mathf.Clamp(totalLength * t + centerOffset, 0f, totalLength);
        centerPoint = DistToPoint(centerDistance);

        fullStart = centerDistance - fullEffectLength / 2f;
        fullEnd = centerDistance + fullEffectLength / 2f;
        fadeInStart = fullStart - fadeInDistance;
        fadeOutEnd = fullEnd + fadeOutDistance;
    }

    float GetCameraDistance()
    {
        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);

        SplineUtility.GetNearestPoint(spline, activator.transform.position, out float3 _, out float t);
        return totalLength * t;
    }

    Vector3 DistToPoint(float dist)
    {
        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);
        dist = Mathf.Clamp(dist, 0f, totalLength);
        float u = dist / totalLength;
        return (Vector3)SplineUtility.EvaluatePosition(spline, u);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UpdateZonePoints();
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

        if (activator != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(activator.transform.position, baseSize * 0.5f);
        }
    }
#endif
}













using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class AlphaRadiusZone : MonoBehaviour
{
    [Header("Spline Reference")]
    public SplineContainer splineContainer;

    [Header("Kamera mit CameraRadiusActivator")]
    public Transform cameraTransform;

    [Header("Zone Settings")]
    public float fullEffectLength = 2f;
    public float fadeInDistance = 2f;
    public float fadeOutDistance = 5f;
    public float centerOffset = 0f;
    public float targetFadeRadius = 25f;

    // Runtime vars
    private float originalRadius;
    private CameraRadiusActivator activator;
    private bool isInside;

    // Calculated points
    private float fadeInStart;
    private float fullStart;
    private float fullEnd;
    private float fadeOutEnd;
    private Vector3 centerPoint;

    void Start()
    {
        if (cameraTransform != null)
            activator = cameraTransform.GetComponent<CameraRadiusActivator>();

        if (activator != null)
            originalRadius = activator.fadeRadius;
    }

    void Update()
    {
        if (splineContainer == null || cameraTransform == null || activator == null) return;

        UpdateZonePoints();

        float camDist = GetCameraDistance();

        bool nowInside = camDist >= fadeInStart && camDist <= fadeOutEnd;

        if (nowInside && !isInside)
        {
            // Betreten
            activator.SetTemporaryRadius(targetFadeRadius);
            isInside = true;
        }
        else if (!nowInside && isInside)
        {
            // Verlassen
            activator.ResetRadius();
            isInside = false;
        }
    }

    void UpdateZonePoints()
    {
        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);

        SplineUtility.GetNearestPoint(spline, transform.position, out float3 _, out float t);
        float centerDistance = Mathf.Clamp(totalLength * t + centerOffset, 0f, totalLength);
        centerPoint = DistToPoint(centerDistance);

        fullStart = centerDistance - fullEffectLength / 2f;
        fullEnd = centerDistance + fullEffectLength / 2f;
        fadeInStart = fullStart - fadeInDistance;
        fadeOutEnd = fullEnd + fadeOutDistance;
    }

    float GetCameraDistance()
    {
        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);

        SplineUtility.GetNearestPoint(spline, cameraTransform.position, out float3 _, out float t);
        return totalLength * t;
    }

    Vector3 DistToPoint(float dist)
    {
        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);
        dist = Mathf.Clamp(dist, 0f, totalLength);
        float u = dist / totalLength;
        return (Vector3)SplineUtility.EvaluatePosition(spline, u);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UpdateZonePoints();
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

        if (cameraTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(cameraTransform.position, baseSize * 0.5f);
        }
    }
#endif
}














using UnityEngine;


[RequireComponent(typeof(Collider))]
public class AlphaRadiusZone : MonoBehaviour
{
    [Header("Neuer Fade Radius in Zone")]
    public float neuerFadeRadius = 25f; // ersetzt den Originalwert innerhalb der Zone

    private void OnTriggerEnter(Collider other)
    {
        CameraRadiusActivator activator = other.GetComponent<CameraRadiusActivator>();
        if (activator != null)
        {
            activator.SetTemporaryRadius(neuerFadeRadius);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CameraRadiusActivator activator = other.GetComponent<CameraRadiusActivator>();
        if (activator != null)
        {
            activator.ResetRadius();
        }
    }
}







using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AlphaRadiusZone : MonoBehaviour
{
    [Header("Zusatzradius relativ zum Original")]
    public float radiusErweiterung = 5f; // wird zum Original fadeRadius addiert

    private void OnTriggerEnter(Collider other)
    {
        CameraRadiusActivator activator = other.GetComponent<CameraRadiusActivator>();
        if (activator != null)
        {
            float newRadius = activator.fadeRadius + radiusErweiterung;
            activator.SetTemporaryRadius(newRadius);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CameraRadiusActivator activator = other.GetComponent<CameraRadiusActivator>();
        if (activator != null)
        {
            activator.ResetRadius();
        }
    }
}








using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AlphaRadiusZone : MonoBehaviour
{
    [Header("Zusatzradius relativ zum Original")]
    public float radiusErweiterung = 5f; // wird zum Original fadeRadius addiert

    private void OnTriggerEnter(Collider other)
    {
        CameraRadiusActivator activator = other.GetComponent<CameraRadiusActivator>();
        if (activator != null)
        {
            float newRadius = activator.fadeRadius + radiusErweiterung;
            activator.SetTemporaryRadius(newRadius);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CameraRadiusActivator activator = other.GetComponent<CameraRadiusActivator>();
        if (activator != null)
        {
            activator.ResetRadius();
        }
    }
}
*/