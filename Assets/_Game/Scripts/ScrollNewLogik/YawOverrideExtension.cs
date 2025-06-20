using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("")]
public class YawOverrideExtension : CinemachineExtension
{
    [Tooltip("Yaw Override in Grad relativ zur Spline-Rotation")]
    public float yawOverride = 0f;

    [Tooltip("Minimaler Yaw-Winkel (relativ zur Spline)")]
    public float minYawLimit = -45f;

    [Tooltip("Maximaler Yaw-Winkel (relativ zur Spline)")]
    public float maxYawLimit = 45f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize)
        {
            float clampedYaw = Mathf.Clamp(yawOverride, minYawLimit, maxYawLimit);
            var rot = state.RawOrientation.eulerAngles;
            rot.y += clampedYaw;
            state.RawOrientation = Quaternion.Euler(rot);
        }
    }
}
