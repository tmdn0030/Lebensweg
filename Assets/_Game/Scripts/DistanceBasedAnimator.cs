using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using Unity.Cinemachine;

[ExecuteAlways]
[RequireComponent(typeof(Animator))]
public class DistanceBasedAnimator : MonoBehaviour
{
    [Header("Spline Referenz")]
    public SplineContainer splineContainer;
    public CinemachineSplineDolly dolly;

    [Header("Trigger Einstellungen")]
    [Tooltip("Offset entlang der Spline in Metern (+ oder - vom Objekt aus)")]
    public float triggerOffset = 0f;
    public float revealRadius = 5f;

    [Tooltip("Der Name des Animationsclips (genau wie im Animator)")]
    public string animationStateName = "Reveal";

    private Animator animator;
    private float lastT = 0f;
    private float triggerDistance = 0f;
    private Vector3 triggerPoint; // Für Gizmo-Anzeige

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;
        UpdateTriggerDistance();
    }

    void Update()
    {
        if (dolly == null || splineContainer == null) return;

        UpdateTriggerDistance();

        float cameraDistance = dolly.CameraPosition;
        float distToTrigger = Mathf.Abs(cameraDistance - triggerDistance);

        float t = 1f - Mathf.Clamp01(distToTrigger / revealRadius);

        if (cameraDistance > triggerDistance)
        {
            t = Mathf.Max(lastT, t);
        }

        lastT = t;

        animator.Play(animationStateName, 0, t);
    }

    private void UpdateTriggerDistance()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;

        var spline = splineContainer.Spline;
        float totalLength = SplineUtility.CalculateLength(spline, splineContainer.transform.localToWorldMatrix);
        if (totalLength <= 0f) return;

        // Nächsten Punkt auf der Spline finden
        SplineUtility.GetNearestPoint(spline, transform.position, out float3 _, out float t);
        float centerDistance = Mathf.Clamp(totalLength * t + triggerOffset, 0f, totalLength);

        triggerDistance = centerDistance;
        triggerPoint = DistToPoint(centerDistance);

        Vector3 DistToPoint(float dist)
        {
            dist = Mathf.Clamp(dist, 0f, totalLength);
            float u = dist / totalLength;
            return (Vector3)SplineUtility.EvaluatePosition(spline, u);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;

        UpdateTriggerDistance();

        float baseSize = 0.2f;

        // Triggerpunkt
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(triggerPoint, baseSize);

        // Radius-Anzeige
        Gizmos.color = Color.yellow;
        float totalLength = SplineUtility.CalculateLength(splineContainer.Spline, splineContainer.transform.localToWorldMatrix);

        Vector3 startRadius = DistToPoint(triggerDistance - revealRadius);
        Vector3 endRadius = DistToPoint(triggerDistance + revealRadius);
        Gizmos.DrawSphere(startRadius, baseSize * 0.6f);
        Gizmos.DrawSphere(endRadius, baseSize * 0.6f);
        Gizmos.DrawLine(startRadius, endRadius);

        // Verbindung zum Objekt
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, triggerPoint);

        Vector3 DistToPoint(float dist)
        {
            dist = Mathf.Clamp(dist, 0f, totalLength);
            float u = dist / totalLength;
            return (Vector3)SplineUtility.EvaluatePosition(splineContainer.Spline, u);
        }
    }
#endif
}









/*
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Animator))]
public class DistanceBasedAnimator : MonoBehaviour
{
    public CinemachineSplineDolly dolly;
    public float triggerDistance = 10f;
    public float revealRadius = 5f;

    [Tooltip("Der Name des Animationsclips (genau wie im Animator)")]
    public string animationStateName = "Reveal";

    private Animator animator;
    private float lastT = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f; // Manuelle Steuerung
    }

    void Update()
    {
        if (dolly == null) return;

        float cameraDistance = dolly.CameraPosition;
        float distToTrigger = Mathf.Abs(cameraDistance - triggerDistance);

        float t = 1f - Mathf.Clamp01(distToTrigger / revealRadius);

        // ⛔ Verhindere, dass t kleiner wird wenn wir weiter weg scrollen (nach vorne)
        if (cameraDistance > triggerDistance)
        {
            t = Mathf.Max(lastT, t);
        }

        lastT = t;

        animator.Play(animationStateName, 0, t);
    }
}












using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Animator))]
public class DistanceBasedAnimator : MonoBehaviour
{
    public CinemachineSplineDolly dolly;
    public float triggerDistance = 10f;
    public float revealRadius = 5f;

    [Tooltip("Der Name des Animationsclips (genau wie im Animator)")]
    public string animationStateName = "Reveal";

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Wichtig: Animator muss im "Always Animate"-Modus sein und kein Autoplay!
        animator.speed = 0f; // Wir steuern die Zeit manuell
    }

    void Update()
    {
        if (dolly == null) return;

        float cameraDistance = dolly.CameraPosition;
        float distToTrigger = Mathf.Abs(cameraDistance - triggerDistance);

        float t = 1f - Mathf.Clamp01(distToTrigger / revealRadius); // 0 = zu weit weg, 1 = perfekt nah

        // Spiele Animation und setze Zeit
        animator.Play(animationStateName, 0, t);
    }
}




using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Animator))]
public class DistanceBasedAnimator : MonoBehaviour
{
    public CinemachineSplineDolly dolly;
    public float triggerDistance = 10f;
    public float revealRadius = 5f;

    [Tooltip("Der Name des Animationsclips (genau wie im Animator)")]
    public string animationStateName = "Reveal";

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Wichtig: Animator muss im "Always Animate"-Modus sein und kein Autoplay!
        animator.speed = 0f; // Wir steuern die Zeit manuell
    }

    void Update()
    {
        if (dolly == null) return;

        float cameraDistance = dolly.CameraPosition;
        float distToTrigger = Mathf.Abs(cameraDistance - triggerDistance);

        float t = 1f - Mathf.Clamp01(distToTrigger / revealRadius); // 0 = zu weit weg, 1 = perfekt nah

        // Spiele Animation und setze Zeit
        animator.Play(animationStateName, 0, t);
    }
}
*/