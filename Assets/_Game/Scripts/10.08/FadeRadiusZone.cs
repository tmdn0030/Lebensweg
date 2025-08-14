

/*
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FadeRadiusZone : MonoBehaviour
{
    [Header("Zusatzradius relativ zum Original")]
    public float radiusErweiterung = 5f; // wird addiert

    private void OnTriggerEnter(Collider other)
    {
        CameraLightRadiusActivator activator = other.GetComponent<CameraLightRadiusActivator>();
        if (activator != null)
        {
            activator.SetTemporaryFadeRadius(activator.fadeRadius + radiusErweiterung);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CameraLightRadiusActivator activator = other.GetComponent<CameraLightRadiusActivator>();
        if (activator != null)
        {
            activator.ResetFadeRadius();
        }
    }
}
*/