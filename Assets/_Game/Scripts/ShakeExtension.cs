using UnityEngine;
using Unity.Cinemachine;

public class ShakeExtension : CinemachineExtension
{
    public float amplitude = 0f;  // maximale Winkelabweichung in Grad
    public float frequency = 1f;  // Geschwindigkeit des Shakes

    private float shakeTime = 0f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (!Application.isPlaying)
            return;

        if (stage == CinemachineCore.Stage.Body && amplitude > 0f)
        {
            shakeTime += deltaTime * frequency;

            // Generiere weiche Perlin-Noise Werte für X, Y und Z, jeweils zwischen -1 und 1
            float shakeX = (Mathf.PerlinNoise(shakeTime, 0f) * 2f - 1f) * amplitude;
            float shakeY = (Mathf.PerlinNoise(shakeTime, 1f) * 2f - 1f) * amplitude;
            float shakeZ = (Mathf.PerlinNoise(shakeTime, 2f) * 2f - 1f) * amplitude;

            Quaternion shakeRot = Quaternion.Euler(shakeX, shakeY, shakeZ);

            state.RawOrientation = state.RawOrientation * shakeRot;
        }
    }

    public void StartShake(float amp, float freq)
    {
        amplitude = amp;
        frequency = freq;
        shakeTime = 0f;
    }

    public void StopShake()
    {
        amplitude = 0f;
    }
}
