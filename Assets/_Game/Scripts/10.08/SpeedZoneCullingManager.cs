using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ManagedObject
{
    public GameObject obj;
    public float activationRadius;
    public float deactivationRadius;
}

public class SpeedZoneCullingManager : MonoBehaviour
{
    [Header("Standardwerte")]
    public float defaultActivationRadius = 50f;
    public float defaultDeactivationRadius = 60f;

    public Transform playerTransform;
    public float deactivateDelay = 0.1f;

    [Header("Individuelle Objekte (optional)")]
    public List<ManagedObject> managedObjects = new List<ManagedObject>();

    private Dictionary<GameObject, float> offTimers = new Dictionary<GameObject, float>();
    private GameObject[] allSpeedZones;

    void Start()
    {
        if (playerTransform == null)
            playerTransform = Camera.main.transform;

        // Alle SpeedZone-Objekte automatisch finden
        allSpeedZones = GameObject.FindGameObjectsWithTag("SpeedZone");

        foreach (GameObject g in allSpeedZones)
        {
            if (managedObjects.Exists(x => x.obj == g)) continue;

            ManagedObject mo = new ManagedObject
            {
                obj = g,
                activationRadius = defaultActivationRadius,
                deactivationRadius = defaultDeactivationRadius
            };
            managedObjects.Add(mo);
        }
    }

    void Update()
    {
        Vector3 playerPos = playerTransform.position;

        foreach (ManagedObject mo in managedObjects)
        {
            if (mo.obj == null) continue;

            float dist = Vector3.Distance(playerPos, mo.obj.transform.position);

            // Aktivieren
            if (!mo.obj.activeSelf && dist <= mo.activationRadius)
            {
                mo.obj.SetActive(true);
                if (offTimers.ContainsKey(mo.obj))
                    offTimers.Remove(mo.obj);
            }
            // Deaktivieren mit Verzögerung
            else if (mo.obj.activeSelf && dist > mo.deactivationRadius)
            {
                if (!offTimers.ContainsKey(mo.obj))
                    offTimers[mo.obj] = Time.time + deactivateDelay;

                if (Time.time >= offTimers[mo.obj])
                {
                    mo.obj.SetActive(false);
                    offTimers.Remove(mo.obj);
                }
            }
            // Timer zurücksetzen, falls Spieler wieder näher kommt
            else if (mo.obj.activeSelf && dist <= mo.deactivationRadius)
            {
                if (offTimers.ContainsKey(mo.obj))
                    offTimers.Remove(mo.obj);
            }
        }
    }
}
