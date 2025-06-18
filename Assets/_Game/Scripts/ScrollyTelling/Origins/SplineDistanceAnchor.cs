using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SplineDistanceAnchor : MonoBehaviour
{
    [Header("Spline Setup")]
    public SplineContainer splineContainer;

    [Header("Debug (read only)")]
    public float distanceOnSpline;
    public float tOnSpline;
    public Vector3 closestPoint;

    void OnEnable()
    {
        if (splineContainer != null)
            UpdateAnchor();
    }

    void OnValidate()
    {
        UpdateAnchor();
    }

    [ContextMenu("Update Anchor")]
    public void UpdateAnchor()
    {
        if (splineContainer == null || splineContainer.Spline == null)
        {
            Debug.LogWarning("SplineContainer fehlt!", this);
            return;
        }

        var spline = splineContainer.Spline;
        float3 pos = transform.position;

        SplineUtility.GetNearestPoint(spline, pos, out float3 nearest, out float t);

        tOnSpline = t;
        closestPoint = nearest;

        float totalLength = SplineUtility.CalculateLength(spline, transform.localToWorldMatrix);

        distanceOnSpline = totalLength * t;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Grüner Punkt und Linie zum Spline-Punkt
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, closestPoint);
        Gizmos.DrawSphere(closestPoint, 0.2f);

        // Linien zu allen VirtualDistanceBasedAnimator, die auf diesen Anchor zeigen
        VirtualDistanceBasedAnimator[] animators = Object.FindObjectsByType<VirtualDistanceBasedAnimator>(FindObjectsSortMode.None);

        Gizmos.color = Color.cyan;

        foreach (var anim in animators)
        {
            if (anim.originAnchor == this)
            {
                Gizmos.DrawLine(transform.position, anim.transform.position);
                Gizmos.DrawCube(anim.transform.position, Vector3.one * 0.2f);
            }
        }
    }
#endif
}

// Editor-Klasse außerhalb der SplineDistanceAnchor-Klasse
#if UNITY_EDITOR
[CustomEditor(typeof(SplineDistanceAnchor))]
public class SplineDistanceAnchorEditor : Editor
{
    private Vector3 lastPos;

    private void OnEnable()
    {
        lastPos = ((SplineDistanceAnchor)target).transform.position;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Update Anchor"))
        {
            ((SplineDistanceAnchor)target).UpdateAnchor();
            EditorUtility.SetDirty(target);
        }
    }

    private void OnSceneGUI()
    {
        SplineDistanceAnchor anchor = (SplineDistanceAnchor)target;

        if (anchor.transform.position != lastPos)
        {
            anchor.UpdateAnchor();
            lastPos = anchor.transform.position;
            EditorUtility.SetDirty(anchor);
        }
    }
}
#endif
