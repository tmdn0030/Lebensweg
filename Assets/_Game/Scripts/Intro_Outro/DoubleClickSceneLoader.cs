using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI; // für Image
using System.Collections;

public class DoubleClickSceneLoader : MonoBehaviour
{
    [Header("Szenen-Einstellungen")]
    [SerializeField] private string sceneNameToLoad = "NextScene";
    [SerializeField] private float delayBeforeSceneLoad = 0.5f;

    [Header("Doppelklick / Tap-Erkennung")]
    [SerializeField] private float doubleClickThreshold = 0.3f;

    [Header("Übergangseffekte")]
    [SerializeField] private string fadeImageTag = "FadeImage";  // Der Tag des weißen Image-Objekts

    private float lastClickTime = -1f;
    private Camera mainCam;
    private bool isWaiting = false;
    private Image fadeImage;  // Das weiße Image im Canvas

    private void Start()
    {
        mainCam = Camera.main ?? FindFirstObjectByType<Camera>();

        // Versuchen, das fadeImage anhand des Tags zu finden
        fadeImage = GameObject.FindGameObjectWithTag(fadeImageTag)?.GetComponent<Image>();

        // Sicherstellen, dass das fadeImage unsichtbar ist zu Beginn
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f; // Sicherstellen, dass es zu Beginn unsichtbar ist
            fadeImage.color = color;
        }
        else
        {
            Debug.LogError($"Kein fadeImage mit Tag '{fadeImageTag}' gefunden!");
        }
    }

    void Update()
    {
        if (isWaiting) return;

        Vector2? inputPosition = null;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputPosition = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (inputPosition.HasValue)
        {
            float time = Time.time;
            if (time - lastClickTime < doubleClickThreshold)
            {
                lastClickTime = -1f; // Reset
                TryHitAndStartSceneLoad(inputPosition.Value);
            }
            else
            {
                lastClickTime = time;
            }
        }
    }

    void TryHitAndStartSceneLoad(Vector2 screenPosition)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                Debug.Log($"Double click/tap erkannt auf {hit.transform.name}. Szene wird in {delayBeforeSceneLoad} Sekunden geladen...");
                StartCoroutine(DelayedSceneLoad());
            }
        }
    }

    IEnumerator DelayedSceneLoad()
    {
        isWaiting = true;

        // Überblendung zu Weiß
        yield return StartCoroutine(FadeToWhite());

        // Szene laden
        SceneManager.LoadScene(sceneNameToLoad);

        // Stelle sicher, dass der Übergang zu Weiß in der neuen Szene ausgeführt wird
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneNameToLoad);  // Warten, bis die neue Szene geladen ist

        // Jetzt das fadeImage in der neuen Szene anhand des Tags suchen und den Fade-Out anwenden
        yield return StartCoroutine(FadeOutInNewScene());
    }

    IEnumerator FadeToWhite()
    {
        if (fadeImage != null)
        {
            float elapsedTime = 0f;
            Color color = fadeImage.color;

            while (elapsedTime < delayBeforeSceneLoad)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsedTime / delayBeforeSceneLoad);
                fadeImage.color = color;
                yield return null;
            }
        }
    }

    IEnumerator FadeOutInNewScene()
    {
        // Versuche, das fadeImage in der neuen Szene zu finden, wenn es noch nicht gefunden wurde
        if (fadeImage == null)
        {
            fadeImage = GameObject.FindGameObjectWithTag(fadeImageTag)?.GetComponent<Image>();
        }

        // Sicherstellen, dass das fadeImage gefunden wurde
        if (fadeImage != null)
        {
            // Setze den Alpha-Wert auf 1 zu Beginn der neuen Szene
            Color color = fadeImage.color;
            color.a = 1f; // Setzt das Bild zu Beginn sichtbar (Alpha = 1)
            fadeImage.color = color;

            // Führe den Fade-Out-Effekt durch
            float elapsedTime = 0f;

            while (elapsedTime < delayBeforeSceneLoad)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsedTime / delayBeforeSceneLoad);
                fadeImage.color = color;
                yield return null;
            }

            // Sicherstellen, dass das Bild vollständig unsichtbar wird
            color.a = 0f;
            fadeImage.color = color;
        }
        else
        {
            Debug.LogError($"Kein fadeImage mit Tag '{fadeImageTag}' in der neuen Szene gefunden!");
        }
    }
}
