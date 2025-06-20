using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorTrigger : DistanceEventTrigger
{
    [Header("Animator Trigger Settings")]
    public string animationStateName = "Reveal";

    private Animator animator;
    private float lastT = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f; // Animation wird manuell gesteuert
    }

    protected override void OnProgress(float t)
    {
        // Fortschritt nie zurückspulen, nur vorwärts
        t = Mathf.Max(lastT, t);
        lastT = t;

        // Animation auf t setzen
        animator.Play(animationStateName, 0, t);
    }

    protected override void OnTriggered()
    {
        // Hier kannst du weitere einmalige Effekte auslösen
        // z. B. Sound, Partikel, Events, usw.
    }
}
