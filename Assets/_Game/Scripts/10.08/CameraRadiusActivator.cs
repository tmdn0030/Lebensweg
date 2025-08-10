using UnityEngine;
using System.Collections.Generic;

public class CameraRadiusActivator : MonoBehaviour
{
    public float activationRadius = 50f;
    public float deactivationRadius = 60f;
    private List<Transform> objects = new List<Transform>();

    void Start()
    {
        // Alle MeshRenderer-Objekte schnell und ohne Sortierung finden
        foreach (var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            objects.Add(renderer.transform);
        }
    }


    void Update()
    {
        Vector3 camPos = transform.position;
        float activationSqr = activationRadius * activationRadius;
        float deactivationSqr = deactivationRadius * deactivationRadius;

        foreach (Transform obj in objects)
        {
            float distSqr = (obj.position - camPos).sqrMagnitude;
            bool isActive = obj.gameObject.activeSelf;

            if (!isActive && distSqr < activationSqr)
                obj.gameObject.SetActive(true);
            else if (isActive && distSqr > deactivationSqr)
                obj.gameObject.SetActive(false);
        }
    }
}
