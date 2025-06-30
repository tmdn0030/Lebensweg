using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class AnimationZone : MonoBehaviour
{
    [Header("Spline-Zone")]
    public SplineContainer spline;

    [Tooltip("Startpunkt der Zone (in Metern auf der Spline). Wird automatisch berechnet, wenn 'Mit Objekt mitwandern' aktiviert ist.")]
    public float startDistance = 0f;

    [Tooltip("Länge der Zone in Metern")]
    public float zoneLength = 2f;

    [Tooltip("Wenn aktiv, wandert der Startpunkt automatisch mit der Objektposition entlang der Spline.")]
    public bool followTransform = false;

    [HideInInspector] public float offsetToSpline = 0f;
    private bool lastFollowTransform = false;

    [Header("Animation")]
    public AnimationClip animationClip;
    public Animator animator; // Optional, falls nicht auf diesem Objekt

    [Header("Debug")]
    [SerializeField] float previewNormalizedTime;

    public float endDistance => startDistance + zoneLength;

    void Reset()
    {
        animator = GetComponent<Animator>();
    }

    void OnValidate()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (zoneLength < 0.01f) zoneLength = 0.01f;

        // Wechsel von followTransform merken
        if (followTransform && !lastFollowTransform)
            CacheOffsetToSpline();
        lastFollowTransform = followTransform;

        if (followTransform)
            UpdateStartDistanceFromTransform();
    }

    void Update()
    {
#if UNITY_EDITOR
        // Editor: Offset merken, wenn Option umgeschaltet wurde
        if (followTransform && !lastFollowTransform)
            CacheOffsetToSpline();
        lastFollowTransform = followTransform;

        if (followTransform && !Application.isPlaying)
            UpdateStartDistanceFromTransform();
#endif
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

    public void ScrubAnimation(float splinePosition)
    {
        if (animationClip == null || animator == null) return;
        float s = startDistance;
        float e = endDistance;
        if (e <= s) return;

        float t = Mathf.InverseLerp(s, e, splinePosition);
        t = Mathf.Clamp01(t);

        previewNormalizedTime = t;

        animator.Play(animationClip.name, 0, t);
        animator.speed = 0f;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (spline == null || spline.Spline == null) return;

        float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);
        if (totalLength <= 0f) return;

        if (followTransform) UpdateStartDistanceFromTransform();

        float startDist = Mathf.Clamp(startDistance, 0, totalLength);
        float endDist = Mathf.Clamp(endDistance, 0, totalLength);

        Vector3 startPoint = SplineUtility.EvaluatePosition(spline.Spline, startDist / totalLength);
        Vector3 endPoint = SplineUtility.EvaluatePosition(spline.Spline, endDist / totalLength);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(startPoint, 0.12f);
        Gizmos.DrawSphere(endPoint, 0.12f);

        Gizmos.color = new Color(1f, 0.3f, 0.7f, 0.4f);
        Gizmos.DrawLine(startPoint, endPoint);

        // Info-Label
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.magenta;
        Handles.Label(startPoint + Vector3.up * 0.2f, "Anim Start", style);
        Handles.Label(endPoint + Vector3.up * 0.2f, "Anim End", style);
    }
#endif
}
