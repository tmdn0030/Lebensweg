using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ManagedMesh
{
    public Renderer meshRenderer;
    public float activationRadius;
    public float deactivationRadius;
}

public class MeshCullingManager : MonoBehaviour
{
    [Header("Standardwerte")]
    public float defaultActivationRadius = 50f;
    public float defaultDeactivationRadius = 60f;

    public Transform playerTransform;
    public float deactivateDelay = 0.1f;

    [Header("Meshes mit individueller Distanz (optional)")]
    public List<ManagedMesh> managedMeshes = new List<ManagedMesh>();

    private Dictionary<Renderer, float> offTimers = new Dictionary<Renderer, float>();
    private Renderer[] allMeshes;

    void Start()
    {
        if (playerTransform == null)
            playerTransform = Camera.main.transform;

#if UNITY_2023_1_OR_NEWER
        allMeshes = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
#else
        allMeshes = FindObjectsOfType<Renderer>(true);
#endif

        // Alle Renderer automatisch aufnehmen, Standardradien setzen
        foreach (Renderer r in allMeshes)
        {
            if (managedMeshes.Exists(x => x.meshRenderer == r)) continue; // schon manuell hinzugefügt

            ManagedMesh mm = new ManagedMesh
            {
                meshRenderer = r,
                activationRadius = defaultActivationRadius,
                deactivationRadius = defaultDeactivationRadius
            };
            managedMeshes.Add(mm);
        }
    }

    void Update()
    {
        Vector3 playerPos = playerTransform.position;

        foreach (ManagedMesh mm in managedMeshes)
        {
            if (mm.meshRenderer == null) continue;

            float dist = Vector3.Distance(playerPos, mm.meshRenderer.transform.position);

            // Einschalten
            if (!mm.meshRenderer.enabled && dist <= mm.activationRadius)
            {
                mm.meshRenderer.enabled = true;
                if (offTimers.ContainsKey(mm.meshRenderer))
                    offTimers.Remove(mm.meshRenderer);
            }
            // Ausschalten mit Verzögerung
            else if (mm.meshRenderer.enabled && dist > mm.deactivationRadius)
            {
                if (!offTimers.ContainsKey(mm.meshRenderer))
                    offTimers[mm.meshRenderer] = Time.time + deactivateDelay;

                if (Time.time >= offTimers[mm.meshRenderer])
                {
                    mm.meshRenderer.enabled = false;
                    offTimers.Remove(mm.meshRenderer);
                }
            }
            // Timer zurücksetzen, falls Spieler wieder näher kommt
            else if (mm.meshRenderer.enabled && dist <= mm.deactivationRadius)
            {
                if (offTimers.ContainsKey(mm.meshRenderer))
                    offTimers.Remove(mm.meshRenderer);
            }
        }
    }
}
