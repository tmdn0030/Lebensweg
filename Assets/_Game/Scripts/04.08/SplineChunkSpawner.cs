using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SplineChunkSpawner : MonoBehaviour
{
    [Header("Spline & Prefabs")]
    public SplineContainer spline;        
    public GameObject[] chunkPrefabs;   // Mehrere Prefabs

    [Header("Placement")]
    public float spacing = 0.5f;
    public float lateralOffset = 0.5f;
    public float baseYOffset = -1f;
    public float randomOffsetY = 0.2f;

    [Header("Scale Settings")]
    public Vector3 baseScale = Vector3.one;
    public Vector3 randomScale = Vector3.one;

    [Header("Noise Movement")]
    public float noiseAmplitude = 0.2f;
    public float noiseFrequency = 1f;

    [Header("Rotation")]
    public float baseRotationSpeed = 20f;
    public float randomRotationSpeed = 10f;

    [Header("Wave Popup Settings")]
    public Transform cameraTransform;
    public float activationRadius = 10f;
    public float farHeight = -1f;
    public float nearHeight = 0.5f;
    public float popSpeed = 2f;

    [Header("Wave Shape")]
    public AnimationCurve waveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Random Seed")]
    public int randomSeed = 12345;

    // intern
    private Transform[] chunks;
    private Vector3[] basePositions;
    private Vector3[] rotationAxes;
    private float[] rotationSpeeds;
    private float[] currentHeights;
    private float[] chunkSplineDistances;
    private float4x4 splineMatrix;
    private float splineLength;

    void Start()
    {
        if (spline == null || chunkPrefabs == null || chunkPrefabs.Length == 0) return;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        UnityEngine.Random.InitState(randomSeed);
        splineMatrix = spline.transform.localToWorldMatrix;

        splineLength = SplineUtility.CalculateLength(spline.Spline, splineMatrix);
        int count = Mathf.CeilToInt(splineLength / spacing);

        chunks = new Transform[count + 1];
        basePositions = new Vector3[count + 1];
        rotationAxes = new Vector3[count + 1];
        rotationSpeeds = new float[count + 1];
        currentHeights = new float[count + 1];
        chunkSplineDistances = new float[count + 1];

        for (int i = 0; i <= count; i++)
        {
            float t = (float)i / count;

            float3 localPos = spline.Spline.EvaluatePosition(t);
            float3 localTan = spline.Spline.EvaluateTangent(t);

            Vector3 position = math.transform(splineMatrix, localPos);
            Vector3 tangent = math.rotate(splineMatrix, localTan);
            Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);

            Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized;
            position += side * UnityEngine.Random.Range(-lateralOffset, lateralOffset);
            position += Vector3.up * (baseYOffset + UnityEngine.Random.Range(-randomOffsetY, randomOffsetY));

            rotation *= Quaternion.Euler(
                UnityEngine.Random.Range(-15f,15f),
                UnityEngine.Random.Range(-15f,15f),
                UnityEngine.Random.Range(-15f,15f)
            );

            Vector3 scale = baseScale + new Vector3(
                UnityEngine.Random.Range(-randomScale.x, randomScale.x),
                UnityEngine.Random.Range(-randomScale.y, randomScale.y),
                UnityEngine.Random.Range(-randomScale.z, randomScale.z)
            );
            scale.x = Mathf.Max(0.01f, scale.x);
            scale.y = Mathf.Max(0.01f, scale.y);
            scale.z = Mathf.Max(0.01f, scale.z);

            // Zufälliges Prefab auswählen
            GameObject prefab = chunkPrefabs[UnityEngine.Random.Range(0, chunkPrefabs.Length)];
            GameObject chunk = Instantiate(prefab, position, rotation, transform);
            chunk.transform.localScale = scale;

            chunks[i] = chunk.transform;
            basePositions[i] = position;
            rotationAxes[i] = new Vector3(
                UnityEngine.Random.Range(-1f,1f),
                UnityEngine.Random.Range(-1f,1f),
                UnityEngine.Random.Range(-1f,1f)
            ).normalized;
            rotationSpeeds[i] = baseRotationSpeed + UnityEngine.Random.Range(-randomRotationSpeed, randomRotationSpeed);
            currentHeights[i] = farHeight;

            chunkSplineDistances[i] = t * splineLength;
        }
    }

    void Update()
    {
        if (chunks == null) return;
        float time = Time.time;

        // Kamera-Position auf Spline ermitteln
        float3 nearest;
        float _dist = SplineUtility.GetNearestPoint(spline.Spline, (float3)cameraTransform.position, out nearest, out float normalizedT);
        float cameraSplineDistance = normalizedT * splineLength;

        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] == null) continue;

            float waveOffset = Mathf.Abs(chunkSplineDistances[i] - cameraSplineDistance);
            float normalizedOffset = Mathf.Clamp01(1f - (waveOffset / activationRadius));

            float waveFactor = waveCurve.Evaluate(normalizedOffset);

            float targetHeight = Mathf.Lerp(farHeight, nearHeight, waveFactor);
            currentHeights[i] = Mathf.Lerp(currentHeights[i], targetHeight, Time.deltaTime * popSpeed);

            Vector3 noiseOffset = new Vector3(
                (Mathf.PerlinNoise(time * noiseFrequency, i * 0.1f) - 0.5f) * 2f * noiseAmplitude,
                (Mathf.PerlinNoise(time * noiseFrequency + 100, i * 0.1f) - 0.5f) * 2f * noiseAmplitude,
                (Mathf.PerlinNoise(time * noiseFrequency + 200, i * 0.1f) - 0.5f) * 2f * noiseAmplitude
            );

            Vector3 pos = basePositions[i] + noiseOffset;
            pos.y += currentHeights[i];
            chunks[i].position = pos;

            chunks[i].Rotate(rotationAxes[i], rotationSpeeds[i] * Time.deltaTime, Space.World);
        }
    }
}
