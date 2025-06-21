using UnityEngine;
using System.Collections.Generic;

public class FollowCamYAlignManager : MonoBehaviour
{
    [Header("Ziel-Kamera")]
    [Tooltip("Welche Kamera soll verfolgt werden? (leer = Hauptkamera)")]
    [SerializeField] private Camera targetCamera;

    [Header("Rotationsverhalten")]
    [Tooltip("Wie schnell das Objekt der Kamera folgt (0 = sehr weich, 1 = sofort)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothFactor = 0.1f;

    [Tooltip("Wie oft sollen die Ziel-Richtungen neu berechnet werden (Sekunden)?")]
    [SerializeField] private float updateInterval = 0.2f;

    [Tooltip("Maximale Entfernung zur Kamera, damit Rotation aktiv ist (0 = immer)")]
    [SerializeField] private float maxDistance = 50f;

    private class FollowData
    {
        public Transform transform;
        public Quaternion targetRotation;
    }

    private List<FollowData> followObjects = new();
    private float updateTimer;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        GameObject[] tagged = GameObject.FindGameObjectsWithTag("FollowCam");
        foreach (var obj in tagged)
        {
            followObjects.Add(new FollowData
            {
                transform = obj.transform,
                targetRotation = obj.transform.rotation
            });
        }

        updateTimer = updateInterval;
    }

    void Update()
    {
        updateTimer -= Time.deltaTime;

        if (updateTimer <= 0f)
        {
            UpdateTargetRotations();
            updateTimer = updateInterval;
        }

        SmoothlyRotateObjects();
    }

    private void UpdateTargetRotations()
    {
        Vector3 camPos = targetCamera.transform.position;

        foreach (var obj in followObjects)
        {
            float distance = Vector3.Distance(obj.transform.position, camPos);

            if (maxDistance > 0f && distance > maxDistance)
                continue;

            Vector3 direction = camPos - obj.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                obj.targetRotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private void SmoothlyRotateObjects()
    {
        foreach (var obj in followObjects)
        {
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, obj.targetRotation, smoothFactor);
        }
    }
}
