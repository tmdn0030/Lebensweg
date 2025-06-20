using UnityEngine;

public class ScrollSpeedManager : MonoBehaviour
{
    public SpeedProfile speedProfile;
    [Range(0f, 1f)] public float damping = 0.9f;

 

    public float GetSpeedMultiplier(float distance)
    {
        if (speedProfile == null || speedProfile.speedSections.Count == 0)
            return 1f;

        var sections = speedProfile.speedSections;

        for (int i = 0; i < sections.Count - 1; i++)
        {
            var current = sections[i];
            var next = sections[i + 1];

            if (distance >= current.startDistance && distance <= next.startDistance)
            {
                float t = Mathf.InverseLerp(current.startDistance, next.startDistance, distance);
                AnimationCurve curve = speedProfile.GetEasingCurveForDistance(distance);
                t = curve.Evaluate(t);

                float lerped = Mathf.Lerp(current.speedMultiplier, next.speedMultiplier, t);
                return Mathf.Max(lerped, 0.01f);
            }
        }

        return Mathf.Max(sections[^1].speedMultiplier, 0.01f);
    }
}
