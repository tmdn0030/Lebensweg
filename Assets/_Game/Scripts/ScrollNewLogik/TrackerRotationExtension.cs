using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Custom/Tracker Rotation Extension")]
public class TrackerRotationExtension : CinemachineExtension
{
    public enum TrackerMode { Additive, Override }

    [Header("Tracker Settings")]
    [Tooltip("Globaler Erkennungsradius, falls Tracker keinen eigenen Radius hat")]
    public float detectionRadius = 5f;

    [Tooltip("Wie schnell soll die Rotation zur Zielrichtung erfolgen")]
    public float rotationBlendSpeed = 3f;

    [Tooltip("Debug-Kreise im Editor anzeigen")]
    public bool showDebug = true;

    [Tooltip("Wie soll die Tracker-Rotation angewendet werden")]
    public TrackerMode mode = TrackerMode.Additive;

    private Transform currentTarget;
    private Quaternion smoothedRotation = Quaternion.identity;
    private bool isInTrackerZone = false;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize)
            return;

        Vector3 camPos = state.RawPosition;

        Transform nearestTracker = FindNearestTracker(camPos);
        bool trackerFound = nearestTracker != null;

        if (trackerFound)
        {
            Vector3 toTarget = nearestTracker.position - camPos;
            toTarget.y = 0; // Nur Yaw, kein Pitch
            if (toTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toTarget);
                if (!isInTrackerZone || currentTarget != nearestTracker)
                {
                    // Neues Ziel
                    smoothedRotation = state.RawOrientation;
                    currentTarget = nearestTracker;
                    isInTrackerZone = true;
                }
                smoothedRotation = Quaternion.Slerp(smoothedRotation, targetRot, deltaTime * rotationBlendSpeed);

                if (mode == TrackerMode.Override)
                {
                    state.RawOrientation = smoothedRotation;
                }
                else if (mode == TrackerMode.Additive)
                {
                    state.RawOrientation *= Quaternion.Inverse(state.RawOrientation) * smoothedRotation;
                }
            }
        }
        else if (isInTrackerZone)
        {
            Quaternion defaultRot = state.RawOrientation;
            smoothedRotation = Quaternion.Slerp(smoothedRotation, defaultRot, deltaTime * rotationBlendSpeed);
            state.RawOrientation = smoothedRotation;

            if (Quaternion.Angle(smoothedRotation, defaultRot) < 1f)
            {
                isInTrackerZone = false;
                currentTarget = null;
            }
        }
    }

    private Transform FindNearestTracker(Vector3 position)
    {
        GameObject[] allTrackers = GameObject.FindGameObjectsWithTag("Tracker");
        float closestDist = float.MaxValue;
        Transform closest = null;

        foreach (var go in allTrackers)
        {
            float radius = detectionRadius;
            var trackerSettings = go.GetComponent<TrackerSettings>();
            if (trackerSettings != null)
            {
                radius = trackerSettings.detectionRadius;
            }

            float dist = Vector3.Distance(position, go.transform.position);
            if (dist <= radius && dist < closestDist)
            {
                closestDist = dist;
                closest = go.transform;
            }
        }

        return closest;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        Gizmos.color = Color.yellow;
        GameObject[] trackers = GameObject.FindGameObjectsWithTag("Tracker");
        foreach (var go in trackers)
        {
            float radius = detectionRadius;
            var trackerSettings = go.GetComponent<TrackerSettings>();
            if (trackerSettings != null)
            {
                radius = trackerSettings.detectionRadius;
            }
            Gizmos.DrawWireSphere(go.transform.position, radius);
        }
    }
#endif
} 
