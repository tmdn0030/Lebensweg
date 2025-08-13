using UnityEngine;
using UnityEngine.UI;  // für Image
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeOutNewScene : MonoBehaviour
{
    [Header("Fade-Einstellungen")]
    [SerializeField] private float fadeOutDuration = 1f; // Dauer des Fade-Out-Effekts
    [SerializeField] private float fadeOutDelay = 0.5f;  // Verzögerung, bevor das Fade-Out beginnt

    [Header("Tag des weißen Image-Objekts")]
    [SerializeField] private string fadeImageTag = "FadeImage"; // Tag für das weiße Image-Objekt

    private Image fadeImage;  // Das weiße Image im Canvas

    private void Start()
    {
        // Versuche, das fadeImage anhand des Tags zu finden
        fadeImage = GameObject.FindGameObjectWithTag(fadeImageTag)?.GetComponent<Image>();

        if (fadeImage == null)
        {
            Debug.LogError("Kein fadeImage mit dem Tag '" + fadeImageTag + "' gefunden!");
            return;
        }

        // Setze das fadeImage zu Beginn auf sichtbar (Alpha = 1)
        Color color = fadeImage.color;
        color.a = 1f; // Zu Beginn vollständig sichtbar
        fadeImage.color = color;

        // Starte Fade-Out Effekt nach der Verzögerung
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        // Warten, bis die Verzögerung abgelaufen ist
        yield return new WaitForSeconds(fadeOutDelay);

        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration); // Von 1 auf 0
                fadeImage.color = color;
                yield return null;
            }

            // Sicherstellen, dass das Bild vollständig unsichtbar wird
            color.a = 0f;
            fadeImage.color = color;
        }
    }
}
