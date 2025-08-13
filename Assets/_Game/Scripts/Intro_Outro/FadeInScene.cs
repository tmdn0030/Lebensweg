using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // für Image
using System.Collections;

public class FadeInScene : MonoBehaviour
{
    [Header("Fade-Einstellungen")]
    [SerializeField] private float fadeDuration = 1f; // Dauer des Fade-In-Effekts

    [Header("Tag des weißen Image-Objekts")]
    [SerializeField] private string fadeImageTag = "FadeImage"; // Tag für das weiße Image-Objekt

    private Image fadeImage;  // Das weiße Image im Canvas

    private void Start()
    {
        // Versuchen, das fadeImage anhand des Tags zu finden
        fadeImage = GameObject.FindGameObjectWithTag(fadeImageTag)?.GetComponent<Image>();

        if (fadeImage != null)
        {
            // Sicherstellen, dass das fadeImage zu Beginn unsichtbar ist
            Color color = fadeImage.color;
            color.a = 1f; // Zu Beginn vollständig sichtbar (Alpha = 1)
            fadeImage.color = color;
        }
        else
        {
            Debug.LogError("Kein fadeImage mit dem Tag '" + fadeImageTag + "' gefunden!");
        }

        // Starte Fade-In Effekt
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);  // Von 1 auf 0
                fadeImage.color = color;
                yield return null;
            }

            // Sicherstellen, dass der Alpha-Wert am Ende auf 0 bleibt
            color.a = 0f;
            fadeImage.color = color;
        }
    }
}
