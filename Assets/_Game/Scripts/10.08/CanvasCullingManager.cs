using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ManagedCanvas
{
    public Canvas canvas;
    public float activationRadius;
    public float deactivationRadius;
}

public class CanvasCullingManager : MonoBehaviour
{
    [Header("Standardwerte")]
    public float defaultActivationRadius = 50f;
    public float defaultDeactivationRadius = 60f;

    public Transform playerTransform;
    public float deactivateDelay = 0.1f;

    [Header("Canvases mit individueller Distanz (optional)")]
    public List<ManagedCanvas> managedCanvases = new List<ManagedCanvas>();

    private Dictionary<Canvas, float> offTimers = new Dictionary<Canvas, float>();
    private Canvas[] allCanvases;

    void Start()
    {
        if (playerTransform == null)
            playerTransform = Camera.main.transform;

#if UNITY_2023_1_OR_NEWER
        allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
#else
        allCanvases = FindObjectsOfType<Canvas>(true);
#endif

        foreach (Canvas c in allCanvases)
        {
            if (managedCanvases.Exists(x => x.canvas == c)) continue;

            ManagedCanvas mc = new ManagedCanvas
            {
                canvas = c,
                activationRadius = defaultActivationRadius,
                deactivationRadius = defaultDeactivationRadius
            };
            managedCanvases.Add(mc);
        }
    }

    void Update()
    {
        Vector3 playerPos = playerTransform.position;

        foreach (ManagedCanvas mc in managedCanvases)
        {
            if (mc.canvas == null) continue;

            float dist = Vector3.Distance(playerPos, mc.canvas.transform.position);

            // Aktivieren
            if (!mc.canvas.gameObject.activeSelf && dist <= mc.activationRadius)
            {
                mc.canvas.gameObject.SetActive(true);
                if (offTimers.ContainsKey(mc.canvas))
                    offTimers.Remove(mc.canvas);
            }
            // Deaktivieren mit Verzögerung
            else if (mc.canvas.gameObject.activeSelf && dist > mc.deactivationRadius)
            {
                if (!offTimers.ContainsKey(mc.canvas))
                    offTimers[mc.canvas] = Time.time + deactivateDelay;

                if (Time.time >= offTimers[mc.canvas])
                {
                    mc.canvas.gameObject.SetActive(false);
                    offTimers.Remove(mc.canvas);
                }
            }
            // Timer zurücksetzen, falls Spieler wieder näher kommt
            else if (mc.canvas.gameObject.activeSelf && dist <= mc.deactivationRadius)
            {
                if (offTimers.ContainsKey(mc.canvas))
                    offTimers.Remove(mc.canvas);
            }
        }
    }
}
