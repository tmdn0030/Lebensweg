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