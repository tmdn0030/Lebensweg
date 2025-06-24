using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Custom/Tracker Rotation Extension")]
public class TrackerManager : CinemachineExtension
{
    [Header("Tracker Zones (drag here)")]
    public List<CameraTrackerZone> trackerZones = new();

    private CinemachineSplineDolly dolly;

    protected override void Awake()
    {
        base.Awake();
        dolly = GetComponent<CinemachineSplineDolly>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        dolly = GetComponent<CinemachineSplineDolly>();
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize || dolly == null || trackerZones == null || trackerZones.Count == 0)
            return;

        float camDistance = dolly.CameraPosition;

        float bestWeight = 0f;
        Transform bestTarget = null;

        foreach (var zone in trackerZones)
        {
            if (zone == null || zone.lookAtTarget == null)
                continue;

            float weight = zone.GetWeight(camDistance);
            if (weight > bestWeight)
            {
                bestWeight = weight;
                bestTarget = zone.lookAtTarget;
            }
        }

        if (bestTarget != null && bestWeight > 0f)
        {
            Vector3 toTarget = bestTarget.position - state.RawPosition;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                state.RawOrientation = Quaternion.Slerp(state.RawOrientation, targetRot, bestWeight);
            }
        }
    }
}






/*
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Custom/Tracker Manager")]
public class TrackerManager : CinemachineExtension
{
    [Header("Tracker Zones (drag here)")]
    public List<CameraTrackerZone> trackerZones = new();

    private CinemachineSplineDolly dolly;

    protected override void Awake()
    {
        base.Awake();
        dolly = GetComponent<CinemachineSplineDolly>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        dolly = GetComponent<CinemachineSplineDolly>();
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize || dolly == null)
            return;

        // Get the camera position along the spline
        float camDistance = dolly.CameraPosition;

        float bestWeight = 0f;
        Transform bestTarget = null;

        foreach (var zone in trackerZones)
        {
            float weight = zone.GetWeight(camDistance);
            if (weight > bestWeight && zone.transform != null)
            {
                bestWeight = weight;
                bestTarget = zone.transform;
            }
        }

        if (bestTarget != null && bestWeight > 0f)
        {
            Vector3 toTarget = bestTarget.position - state.RawPosition;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                state.RawOrientation = Quaternion.Slerp(state.RawOrientation, targetRot, bestWeight);
            }
        }
    }
}
*/