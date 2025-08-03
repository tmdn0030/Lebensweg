using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Playables;
using UnityEngine.Animations;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class AnimationZone : MonoBehaviour
{
    [Header("Spline-Zone")]
    public SplineContainer spline;
    public float startDistance = 0f;
    public float zoneLength = 2f;
    public bool followTransform = false;

    [Header("Animation")]
    public AnimationClip animationClip;
    public Animator animator;

    [Header("Debug")]
    [SerializeField] float previewNormalizedTime;

    private float offsetToSpline = 0f;
    private bool lastFollowTransform = false;
    private float lastPlayedTime = 0f;
    private bool isInsideZone = false;
    private bool hasFinished = false; // Merkt sich, ob die Zone einmal komplett durchlaufen wurde

    private PlayableGraph playableGraph;
    private AnimationClipPlayable playable;
    private RuntimeAnimatorController originalController;

    public float endDistance => startDistance + zoneLength;

    void Reset()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animationClip == null) return;

        // Original-Controller merken
        originalController = animator.runtimeAnimatorController;

        // PlayableGraph erstellen
        playableGraph = PlayableGraph.Create("AnimationZoneGraph");
        playable = AnimationClipPlayable.Create(playableGraph, animationClip);
        var output = AnimationPlayableOutput.Create(playableGraph, "AnimationZoneOutput", animator);
        output.SetSourcePlayable(playable);
        playableGraph.Play();

        playable.SetSpeed(0f);
        playable.Pause();
    }

    void OnValidate()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (zoneLength < 0.01f) zoneLength = 0.01f;

        if (followTransform && !lastFollowTransform)
            CacheOffsetToSpline();
        lastFollowTransform = followTransform;

        if (followTransform)
            UpdateStartDistanceFromTransform();
    }

    void Update()
    {
#if UNITY_EDITOR
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
    if (animationClip == null || !playableGraph.IsValid()) return;

    float s = startDistance;
    float e = endDistance;
    float totalLength = animationClip.length;

    double clipTime;

    if (splinePosition <= s)
    {
        // Vor der Zone: Anfangszustand
        clipTime = 0;
        isInsideZone = false;
    }
    else if (splinePosition >= e)
    {
        // Nach der Zone: Letzter Frame
        clipTime = totalLength - 0.0001f;
        isInsideZone = false;
    }
    else
    {
        // Innerhalb der Zone: Normal scrubbing
        float t = Mathf.InverseLerp(s, e, splinePosition);
        clipTime = Mathf.Clamp01(t) * totalLength;
        previewNormalizedTime = t;
        lastPlayedTime = t;
        isInsideZone = true;
    }

    // Zeit setzen
    playable.SetTime(clipTime);
    playable.SetSpeed(0f);

    // Umschalten auf Playable nur innerhalb oder am Ende der Zone
    if (isInsideZone || splinePosition >= e)
    {
        if (animator.runtimeAnimatorController != null)
            animator.runtimeAnimatorController = null;
    }
    else
    {
        // Vor der Zone → zurück zum Controller
        if (animator.runtimeAnimatorController == null)
            animator.runtimeAnimatorController = originalController;
    }
}


    void LateUpdate()
    {
        if (!isInsideZone && hasFinished)
        {
            // Nach Abschluss: Immer auf letztem Frame stehen bleiben
            playable.SetTime(animationClip.length - 0.0001f);
            playable.SetSpeed(0f);
            if (animator.runtimeAnimatorController != null)
                animator.runtimeAnimatorController = null;
        }
    }

    public void ResetZone()
    {
        // Ermöglicht Reset (z.B. per Button oder Event)
        hasFinished = false;
    }

    void OnDestroy()
    {
        if (playableGraph.IsValid())
            playableGraph.Destroy();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Selection.activeGameObject != gameObject) return;
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

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.magenta;
        Handles.Label(startPoint + Vector3.up * 0.2f, "Anim Start", style);
        Handles.Label(endPoint + Vector3.up * 0.2f, "Anim End", style);
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
    private float lastPlayedTime = 0f; // Variable zum Speichern der letzten Animationszeit

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

        // Speichern des letzten Frames, damit die Animation nicht zurückspringt
        lastPlayedTime = t;
    }

    private void KeepAnimationAtLastFrame()
    {
        if (animator == null) return;

        // Prüfen, ob das Ende der Zone erreicht wurde
        if (previewNormalizedTime >= 1f)
        {
            // Stoppen der Animation, damit sie nicht zurückspringt
            animator.Play(animationClip.name, 0, lastPlayedTime);
            animator.speed = 0f; // Animation einfrieren
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Visualisierung nur, wenn das Objekt in der Hierarchie ausgewählt ist
        if (Selection.activeGameObject != gameObject) return;

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

*/