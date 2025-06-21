using UnityEngine;

[RequireComponent(typeof(Animator))]
public class VirtualDistanceBasedAnimator : MonoBehaviour
{
    public ScrollController scroller;           // NEU: Referenz auf ScrollController
    public SplineDistanceAnchor originAnchor;   // Spline-Startpunkt
    public float relativeTriggerDistance = 10f;
    public float revealRadius = 5f;
    public string animationStateName = "Reveal";

    private Animator animator;
    private float lastT = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f; // Manuelle Steuerung der Animation
    }

    void Update()
    {
        if (scroller == null || originAnchor == null) return;

        // Wo auf der Spline soll die Animation ausgelöst werden?
        float globalTrigger = originAnchor.distanceOnSpline + relativeTriggerDistance;

        // Wie weit ist die Kamera aktuell vom Trigger entfernt?
        float distToTrigger = Mathf.Abs(scroller.virtualDistance - globalTrigger);

        // Übergangswert (0 = weit weg, 1 = direkt am Trigger)
        float t = 1f - Mathf.Clamp01(distToTrigger / revealRadius);

        // Animation nur vorwärts abspielen
        if (scroller.virtualDistance > globalTrigger)
        {
            t = Mathf.Max(lastT, t);
        }

        lastT = t;

        // Spielt die Animation bei Fortschritt t (zwischen 0 und 1)
        animator.Play(animationStateName, 0, t);
    }
}

/*

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class VirtualDistanceBasedAnimator : MonoBehaviour
{
    public TouchDollyScroller3 scroller;       // Referenz auf dein Scroll-Script
    public SplineDistanceAnchor originAnchor;  // Origin als Ankerpunkt
    public float relativeTriggerDistance = 10f;
    public float revealRadius = 5f;
    public string animationStateName = "Reveal";

    private Animator animator;
    private float lastT = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f; // Animation nur manuell steuern
    }

    void Update()
    {
        if (scroller == null || originAnchor == null) return;

        // Absolute Trigger-Distanz auf Spline (Origin + relative Distanz)
        float globalTrigger = originAnchor.distanceOnSpline + relativeTriggerDistance;

        // Abstand des Scrollers zum Triggerpunkt
        float distToTrigger = Mathf.Abs(scroller.virtualDistance - globalTrigger);

        // t: 0..1 abh�ngig davon, wie nah wir am Trigger sind
        float t = 1f - Mathf.Clamp01(distToTrigger / revealRadius);

        // Animation nur vorw�rts abspielen
        if (scroller.virtualDistance > globalTrigger)
        {
            t = Mathf.Max(lastT, t);
        }

        lastT = t;

        // Animation auf Progress setzen
        animator.Play(animationStateName, 0, t);
    }
}

*/