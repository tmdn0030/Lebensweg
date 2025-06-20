using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSplineController : MonoBehaviour
{
    [System.Serializable]
    public class LightSettings
    {
        public Light light; // Die Lichtquelle
        public float triggerDistance = 10f; // Die Distanz auf der Spline, ab der das Licht aktiviert wird
        public float radius = 5f; // Der Radius, innerhalb dessen die Intensität verändert wird
        public float maxIntensity = 1f; // Maximale Intensität des Lichts
        [HideInInspector] public float currentIntensity = 0f; // Aktuelle Intensität des Lichts
    }

    [Header("Lichtquellen Einstellungen")]
    public List<LightSettings> lightSettingsList = new List<LightSettings>(); // Liste der Lichtquellen mit ihren Einstellungen

    [Header("Kamera und Bewegung")]
    public ScrollController scrollController; // Referenz zum ScrollController (oder eine andere Klasse, die die Entfernung verwaltet)

    void Start()
    {
        // Überprüfen, ob die Liste der Lichter leer ist
        if (lightSettingsList.Count == 0)
        {
            Debug.LogWarning("Keine Lichtquellen in der Liste gefunden.");
        }
    }

    void Update()
    {
        if (scrollController == null)
        {
            Debug.LogError("ScrollController wurde nicht gesetzt!");
            return;
        }

        // Die aktuelle Entfernung aus dem ScrollController holen (wir nutzen hier die virtuelle Entfernung)
        float currentDistance = scrollController.virtualDistance;

        // Für jede Lichtquelle prüfen, ob sie aktiviert werden soll und die Intensität anpassen
        foreach (var lightSetting in lightSettingsList)
        {
            AdjustLightIntensity(lightSetting, currentDistance);
        }
    }

    void AdjustLightIntensity(LightSettings lightSetting, float currentDistance)
    {
        // Berechne die Distanz zur Trigger-Schwelle
        float distanceToTrigger = Mathf.Abs(currentDistance - lightSetting.triggerDistance);

        // Wenn die aktuelle Distanz innerhalb des Radius ist, beeinflusse die Intensität
        if (distanceToTrigger <= lightSetting.radius)
        {
            // Berechne den Intensitätsfaktor, wobei die Mitte des Radius die höchste Intensität hat
            float intensityFactor = 1f - (distanceToTrigger / lightSetting.radius);
            
            // Sanfte Interpolation nach oben bis zur maximalen Intensität
            lightSetting.currentIntensity = Mathf.Lerp(lightSetting.currentIntensity, lightSetting.maxIntensity * intensityFactor, Time.deltaTime * 5f);
        }
        else
        {
            // Wenn die Distanz außerhalb des Radius ist, verringere die Intensität sanft
            lightSetting.currentIntensity = Mathf.Lerp(lightSetting.currentIntensity, 0f, Time.deltaTime * 5f);
        }

        // Setze die Intensität des Lichts auf den berechneten Wert
        lightSetting.light.intensity = lightSetting.currentIntensity;
    }
}
