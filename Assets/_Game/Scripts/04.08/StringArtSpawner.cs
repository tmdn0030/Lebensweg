using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class StringArtSpawner : MonoBehaviour
{
    [Header("Pins & Objects")]
    public GameObject pinObjectPrefab;       // Prefab, das auf jedem Pin sitzt
    public GameObject stringObjectPrefab;    // Prefab für den Faden (muss LineRenderer haben)

    [Header("String Settings")]
    public int stringSegments = 20;          // Wie viele Segmente pro Faden
    public float maxSagAmount = 1.0f;        // Durchhang wenn weit weg
    public float minSagAmount = 0.1f;        // Durchhang wenn nah
    public float sagLerpSpeed = 3f;          // Geschwindigkeit des Spannens

    [Header("Activation Settings")]
    public Transform cameraTransform;
    public float activationRadius = 10f;

    private List<Transform> pins = new List<Transform>();
    private List<GameObject> pinInstances = new List<GameObject>();
    private List<LineRenderer> strings = new List<LineRenderer>();
    private float[] currentSag; // aktueller Durchhang pro Segment

    void OnEnable()
    {
        Generate();
    }

    public void Generate()
    {
        // Alte Instanzen löschen
        foreach (var go in pinInstances) if (go != null) DestroyImmediate(go);
        foreach (var lr in strings) if (lr != null) DestroyImmediate(lr.gameObject);

        pinInstances.Clear();
        strings.Clear();
        pins.Clear();

        // Alle Pins sammeln (Kinder)
        foreach (Transform child in transform)
        {
            pins.Add(child);
        }

        if (pins.Count < 2) return;

        // Pins mit Objekten besetzen
        foreach (var pin in pins)
        {
            if (pinObjectPrefab != null)
            {
                var instance = Instantiate(pinObjectPrefab, pin.position, pin.rotation, pin);
                pinInstances.Add(instance);
            }
        }

        currentSag = new float[pins.Count - 1];

        // Fäden zwischen den Pins erzeugen
        for (int i = 0; i < pins.Count - 1; i++)
        {
            var start = pins[i];
            var end = pins[i + 1];

            if (stringObjectPrefab == null) continue;

            var stringObj = Instantiate(stringObjectPrefab, Vector3.zero, Quaternion.identity, transform);
            var lr = stringObj.GetComponent<LineRenderer>();
            if (lr == null) { Debug.LogError("stringObjectPrefab benötigt einen LineRenderer!"); continue; }

            lr.positionCount = stringSegments;

            strings.Add(lr);
            currentSag[i] = maxSagAmount; // Start mit maximalem Durchhang
        }
    }

    void Update()
    {
        if (pins.Count < 2 || strings.Count != pins.Count - 1) return;
        if (cameraTransform == null) cameraTransform = Camera.main?.transform;
        if (cameraTransform == null) return;

        for (int i = 0; i < strings.Count; i++)
        {
            var start = pins[i];
            var end = pins[i + 1];
            var lr = strings[i];

            // Distanz Kamera zum Segment (mittlerer Punkt)
            Vector3 mid = (start.position + end.position) * 0.5f;
            float dist = Vector3.Distance(cameraTransform.position, mid);

            // Ziel-Durchhang: nah → minSagAmount, weit → maxSagAmount
            float targetSag = (dist <= activationRadius) ? minSagAmount : maxSagAmount;
            currentSag[i] = Mathf.Lerp(currentSag[i], targetSag, Time.deltaTime * sagLerpSpeed);

            // Faden-Punkte berechnen
            for (int j = 0; j < stringSegments; j++)
            {
                float t = j / (float)(stringSegments - 1);
                Vector3 pos = Vector3.Lerp(start.position, end.position, t);

                // Durchhang: Parabel
                float sag = Mathf.Sin(t * Mathf.PI) * currentSag[i];
                pos.y -= sag;

                lr.SetPosition(j, pos);
            }
        }
    }
}
