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

        // t: 0..1 abhängig davon, wie nah wir am Trigger sind
        float t = 1f - Mathf.Clamp01(distToTrigger / revealRadius);

        // Animation nur vorwärts abspielen
        if (scroller.virtualDistance > globalTrigger)
        {
            t = Mathf.Max(lastT, t);
        }

        lastT = t;

        // Animation auf Progress setzen
        animator.Play(animationStateName, 0, t);
    }
}
