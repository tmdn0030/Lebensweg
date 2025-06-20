using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(ScrollController))]
public class SplineCameraDriver : MonoBehaviour
{
    [Header("Verknüpfungen")]
    public CinemachineCamera cineCamera;
    public ScrollController scrollSource;

    private CinemachineSplineDolly splineDolly;

    void Start()
    {
        if (cineCamera == null)
        {
            Debug.LogError("CinemachineCamera ist nicht gesetzt!");
            return;
        }

        splineDolly = cineCamera.GetComponent<CinemachineSplineDolly>();
        if (splineDolly == null)
        {
            Debug.LogError("Keine CinemachineSplineDolly-Komponente gefunden!");
            return;
        }

        if (scrollSource == null)
        {
            scrollSource = GetComponent<ScrollController>();
        }
    }

    void Update()
    {
        // Nur zur Beobachtung oder externen Verarbeitung
        float virtualDist = scrollSource.virtualDistance;
        float actualDist = splineDolly.CameraPosition;

        // Debug.Log($"Virtual: {virtualDist}, Actual: {actualDist}");
    }
}
