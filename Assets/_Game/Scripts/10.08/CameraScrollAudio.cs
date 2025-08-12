using UnityEngine;

public class CameraScrollAudio : MonoBehaviour
{
    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Click Feedback")]
    public AudioSource clickSource;      // AudioSource mit Klicksound
    public float stepDistance = 0.5f;    // Klick alle X Einheiten
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    [Header("Pink Noise")]
    public AudioSource noiseSource;      // AudioSource mit Pink Noise
    public AudioLowPassFilter lowPass;   // Für Höhensteuerung
    public float maxNoiseVolume = 0.15f; // Max Lautstärke bei hoher Geschwindigkeit
    public float speedForMaxVolume = 5f; // Geschwindigkeit, bei der max Lautstärke erreicht wird
    public float minCutoff = 500f;       // LowPass Cutoff bei Stillstand
    public float maxCutoff = 5000f;      // LowPass Cutoff bei max Speed

    private Vector3 lastClickPos;
    private Vector3 lastFramePos;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastClickPos = cameraTransform.position;
        lastFramePos = cameraTransform.position;

        // Noise vorbereiten
        if (noiseSource != null)
        {
            noiseSource.loop = true;
            noiseSource.volume = 0f;
            noiseSource.Play();
        }

        if (lowPass != null)
            lowPass.cutoffFrequency = minCutoff;
    }

    void Update()
    {
        // --- Geschwindigkeit berechnen ---
        float speed = (cameraTransform.position - lastFramePos).magnitude / Time.deltaTime;
        lastFramePos = cameraTransform.position;

        // --- Klicks (Distanz-basiert) ---
        float distSinceLastClick = Vector3.Distance(cameraTransform.position, lastClickPos);
        if (distSinceLastClick >= stepDistance)
        {
            clickSource.pitch = Random.Range(pitchMin, pitchMax);
            clickSource.PlayOneShot(clickSource.clip);
            lastClickPos = cameraTransform.position;
        }

        // --- Pink Noise (Geschwindigkeits-basiert) ---
        float speedNormalized = Mathf.Clamp01(speed / speedForMaxVolume);

        // Lautstärke anpassen
        float targetVol = speedNormalized * maxNoiseVolume;
        noiseSource.volume = Mathf.Lerp(noiseSource.volume, targetVol, Time.deltaTime * 5f);

        // Filter Cutoff anpassen
        if (lowPass != null)
        {
            float targetCutoff = Mathf.Lerp(minCutoff, maxCutoff, speedNormalized);
            lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, targetCutoff, Time.deltaTime * 5f);
        }

        // Optional: Pitch minimal anpassen für Windgefühl
        noiseSource.pitch = Mathf.Lerp(1f, 1.05f, speedNormalized);
    }
}
d