// =========================
// 1. ScriptableObject: SpeedProfile
// =========================

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scrollytelling/SpeedProfile")]
public class SpeedProfile : ScriptableObject
{
    [HideInInspector]
    public List<SpeedSection> speedSections = new List<SpeedSection>();

    public List<HighLevelSpeedEvent> speedEvents = new List<HighLevelSpeedEvent>();

    private void OnValidate()
    {
        RegenerateSections();
    }

    public void RegenerateSections()
    {
        speedSections.Clear();

        foreach (var evt in speedEvents)
        {
            bool hasEaseIn = evt.easeIn > 0f;
            bool hasLength = evt.length > 0f;
            bool hasEaseOut = evt.easeOut > 0f;

            float p1 = evt.startDistance - evt.easeIn;
            float p2 = evt.startDistance;
            float p3 = evt.startDistance + evt.length;
            float p4 = p3 + evt.easeOut;

            // Punkt 1: Ease-In-Start (Multiplikator = 1)
            if (hasEaseIn)
            {
                speedSections.Add(new SpeedSection
                {
                    startDistance = p1,
                    speedMultiplier = 1f,
                    easingCurve = evt.easingCurve
                });
            }

            // Punkt 2: Peak-Start (Multiplikator = Speed)
            speedSections.Add(new SpeedSection
            {
                startDistance = p2,
                speedMultiplier = evt.speedMultiplier,
                easingCurve = evt.easingCurve
            });

            // Punkt 3: Peak-Ende
            if (hasLength)
            {
                speedSections.Add(new SpeedSection
                {
                    startDistance = p3,
                    speedMultiplier = evt.speedMultiplier,
                    easingCurve = evt.easingCurve
                });
            }

            // Punkt 4: Ease-Out-Ende (zurück zu 1)
            if (hasEaseOut)
            {
                speedSections.Add(new SpeedSection
                {
                    startDistance = p4,
                    speedMultiplier = 1f,
                    easingCurve = evt.easingCurve
                });
            }
        }

        // Sortieren nach StartDistance
        speedSections.Sort((a, b) => a.startDistance.CompareTo(b.startDistance));
    }

    public AnimationCurve GetEasingCurveForDistance(float distance)
    {
        foreach (var evt in speedEvents)
        {
            float start = evt.startDistance - evt.easeIn;
            float end = evt.startDistance + evt.length + evt.easeOut;

            if (distance >= start && distance <= end)
            {
                return evt.easingCurve;
            }
        }

        return AnimationCurve.Linear(0, 0, 1, 1); // Fallback
    }
}

[System.Serializable]
public class HighLevelSpeedEvent
{
    public string label = "New Event";
    public float startDistance = 0f;
    public float length = 0f;
    public float easeIn = 2f;
    public float easeOut = 2f;
    public float speedMultiplier = 1f;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Neu
}

[System.Serializable]
public class SpeedSection
{
    public float startDistance = 0f;
    public float speedMultiplier = 1f;
    public AnimationCurve easingCurve = AnimationCurve.Linear(0, 0, 1, 1); // Neu
}






/*
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scrollytelling/SpeedProfile")]
public class SpeedProfile : ScriptableObject
{
    // Nicht sichtbar im Editor – nur zur Laufzeit benutzt
    [HideInInspector]
    public List<SpeedSection> speedSections = new List<SpeedSection>();

    // High-Level Events, aus denen die eigentlichen Sections generiert werden
    public List<HighLevelSpeedEvent> speedEvents = new List<HighLevelSpeedEvent>();


    private void OnValidate()
    {
        RegenerateSections();
    }


    public void RegenerateSections()
    {
        speedSections.Clear();

        foreach (var evt in speedEvents)
        {
            bool hasEaseIn = evt.easeIn > 0f;
            bool hasLength = evt.length > 0f;
            bool hasEaseOut = evt.easeOut > 0f;

            float p1 = evt.startDistance - evt.easeIn;
            float p2 = evt.startDistance;
            float p3 = evt.startDistance + evt.length;
            float p4 = p3 + evt.easeOut;

            // Punkt 1: Ease-In-Start (Multiplikator = 1)
            if (hasEaseIn)
            {
                speedSections.Add(new SpeedSection
                {
                    startDistance = p1,
                    speedMultiplier = 1f
                });
            }

            // Punkt 2: Peak-Start (Multiplikator = Speed)
            speedSections.Add(new SpeedSection
            {
                startDistance = p2,
                speedMultiplier = evt.speedMultiplier
            });

            // Punkt 3: Peak-Ende
            if (hasLength)
            {
                speedSections.Add(new SpeedSection
                {
                    startDistance = p3,
                    speedMultiplier = evt.speedMultiplier
                });
            }

            // Punkt 4: Ease-Out-Ende (zurück zu 1)
            if (hasEaseOut)
            {
                speedSections.Add(new SpeedSection
                {
                    startDistance = p4,
                    speedMultiplier = 1f
                });
            }
        }

        // Optional sortieren
        speedSections.Sort((a, b) => a.startDistance.CompareTo(b.startDistance));
    }

}

[System.Serializable]
public class HighLevelSpeedEvent
{
    public string label = "New Event";
    public float startDistance = 0f;
    public float length = 0f;
    public float easeIn = 2f;
    public float easeOut = 2f;
    public float speedMultiplier = 1f;
}

[System.Serializable]
public class SpeedSection
{
    public float startDistance = 0f;
    public float speedMultiplier = 1f;
}










using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scrollytelling/SpeedProfile")]
public class SpeedProfile : ScriptableObject
{
    public List<SpeedSection> speedSections = new List<SpeedSection>();

    public List<FlatSpeedPoint> GetExpandedSpeedPoints()
    {
        var points = new List<FlatSpeedPoint>();

        foreach (var section in speedSections)
        {
            float startEaseIn = section.startDistance - section.easeIn;
            float peakStart = section.startDistance;
            float peakEnd = section.startDistance + section.length;
            float endEaseOut = peakEnd + section.easeOut;

            points.Add(new FlatSpeedPoint(startEaseIn, 1f));
            points.Add(new FlatSpeedPoint(peakStart, section.speedMultiplier));
            points.Add(new FlatSpeedPoint(peakEnd, section.speedMultiplier));
            points.Add(new FlatSpeedPoint(endEaseOut, 1f));
        }

        return points.OrderBy(p => p.distance).ToList();
    }
}

[System.Serializable]
public class SpeedSection
{
    public string label = "New Section";
    public float startDistance = 0f;
    public float length = 0f;
    public float easeIn = 2f;
    public float easeOut = 2f;
    public float speedMultiplier = 1f;
}

public class FlatSpeedPoint
{
    public float distance;
    public float speedMultiplier;

    public FlatSpeedPoint(float distance, float multiplier)
    {
        this.distance = distance;
        this.speedMultiplier = multiplier;
    }
}




using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scrollytelling/SpeedProfile")]
public class SpeedProfile : ScriptableObject
{
    public List<SpeedSection> speedSections = new List<SpeedSection>();
}

[System.Serializable]
public class SpeedSection
{
    public string label = "New Section";
    public float startDistance = 0f;
    public float length = 0f;
    public float easeIn = 2f;
    public float easeOut = 2f;
    public float speedMultiplier = 1f;
}


*/