using UnityEngine;
using System.Collections.Generic;

public class DynRotManager : MonoBehaviour
{
    [Header("Rotations-Einstellungen")]

    [Tooltip("Maximaler Y-Ausschlag pro Objekt in Grad.")]
    [SerializeField] private float maxYRotationAngle = 10f;

    [Tooltip("Wie schnell sich die Rotation verändert.")]
    [SerializeField] private float wobbleSpeed = 0.5f;

    [Tooltip("Wie stark die Rotation interpoliert wird (0 = weich, 1 = hart).")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothFactor = 0.1f;

    [Tooltip("Zufälliger Zeitversatz für jedes Objekt.")]
    [SerializeField] private bool useRandomOffset = true;

    private class RotData
    {
        public Transform transform;
        public Quaternion baseRotation;
        public float timeOffset;
    }

    private List<RotData> rotObjects = new();

    void Start()
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("DynRot");

        foreach (var obj in taggedObjects)
        {
            rotObjects.Add(new RotData
            {
                transform = obj.transform,
                baseRotation = obj.transform.rotation,
                timeOffset = useRandomOffset ? Random.Range(0f, 1000f) : 0f
            });
        }
    }

    void Update()
    {
        float time = Time.time;

        foreach (var obj in rotObjects)
        {
            // Perlin Noise für sanfte, pseudo-zufällige Bewegung
            float noise = Mathf.PerlinNoise(obj.timeOffset, time * wobbleSpeed);
            float angleY = Mathf.Lerp(-maxYRotationAngle, maxYRotationAngle, noise);

            Quaternion targetRotation = obj.baseRotation * Quaternion.Euler(0f, angleY, 0f);
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, smoothFactor);
        }
    }
}
