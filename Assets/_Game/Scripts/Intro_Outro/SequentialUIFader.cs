using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SequentialFaderAndDissolve : MonoBehaviour
{
    [Header("Objekte (chronologisch)")]
    [Tooltip("Mische UI-GameObjects mit CanvasGroup und Dissolve-Objekte in gewünschter Reihenfolge")]
    public List<GameObject> objectsToProcess;

    [Header("Fade Einstellungen")]
    public float fadeDuration = 1f;

    [Header("Dissolve Einstellungen")]
    [Tooltip("Name des Float-Parameters im Shader")]
    public string dissolveProperty = "_DissolveAmount";
    public float dissolveDuration = 2f;

    private int currentIndex = 0;
    private bool isAnimating = false;

    private List<CanvasGroup> canvasGroups = new List<CanvasGroup>();

    private void Start()
    {
        canvasGroups.Clear();

        for (int i = 0; i < objectsToProcess.Count; i++)
        {
            var go = objectsToProcess[i];
            if (go == null)
            {
                canvasGroups.Add(null);
                continue;
            }

            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = go.AddComponent<CanvasGroup>();
            }

            canvasGroups.Add(cg);

            if (i == 0)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            else
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }

            // Für Dissolve-Objekte (ohne CanvasGroup) Material vorbereiten
            if (cg == null)
            {
                Renderer rend = go.GetComponent<Renderer>();
                if (rend != null && rend.material.HasProperty(dissolveProperty))
                {
                    Material matInstance = Instantiate(rend.material);
                    rend.material = matInstance;
                    matInstance.SetFloat(dissolveProperty, 1f);
                }
            }
        }
    }

    private void Update()
    {
        if (isAnimating) return;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TriggerNext();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TriggerNext();
        }
    }

    public void TriggerNext()
    {
        if (isAnimating) return;
        if (currentIndex >= objectsToProcess.Count) return;

        var currentGo = objectsToProcess[currentIndex];
        CanvasGroup cg = canvasGroups[currentIndex];

        Renderer rend = currentGo.GetComponent<Renderer>();
        bool hasDissolve = rend != null && rend.material.HasProperty(dissolveProperty);

        if (cg != null && !hasDissolve)
        {
            StartCoroutine(FadeOutCurrentFadeInNext());
        }
        else if (hasDissolve)
        {
            StartCoroutine(AnimateDissolve());
        }
        else
        {
            Debug.LogWarning($"Objekt {currentGo.name} ist kein UI und kein Dissolve-Objekt, wird übersprungen.");
            currentIndex++;
            TriggerNext();
        }
    }

    IEnumerator FadeOutCurrentFadeInNext()
    {
        isAnimating = true;

        CanvasGroup current = canvasGroups[currentIndex];
        CanvasGroup next = (currentIndex + 1 < canvasGroups.Count) ? canvasGroups[currentIndex + 1] : null;

        bool isLast = (currentIndex == objectsToProcess.Count - 1);

        if (isLast)
        {
            // Letztes Element -> sichtbar und interaktiv lassen
            if (current != null)
            {
                current.alpha = 1f;
                current.interactable = true;
                current.blocksRaycasts = true;
            }

            // Falls ein nächstes Element existiert (theoretisch nicht), dieses einblenden
            if (next != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / fadeDuration);
                    next.alpha = Mathf.Lerp(0f, 1f, t);
                    yield return null;
                }
                next.alpha = 1f;
                next.interactable = true;
                next.blocksRaycasts = true;
            }
        }
        else
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                if (current != null)
                    current.alpha = Mathf.Lerp(1f, 0f, t);

                if (next != null)
                    next.alpha = Mathf.Lerp(0f, 1f, t);

                yield return null;
            }

            if (current != null)
            {
                current.alpha = 0f;
                current.interactable = false;
                current.blocksRaycasts = false;
            }

            if (next != null)
            {
                next.alpha = 1f;
                next.interactable = true;
                next.blocksRaycasts = true;
            }
        }

        currentIndex++;
        isAnimating = false;
    }

    IEnumerator AnimateDissolve()
    {
        isAnimating = true;

        GameObject go = objectsToProcess[currentIndex];
        Renderer rend = go.GetComponent<Renderer>();

        if (rend == null)
        {
            Debug.LogError($"Objekt {go.name} hat keinen Renderer!");
            isAnimating = false;
            yield break;
        }

        Material mat = rend.material;

        if (!mat.HasProperty(dissolveProperty))
        {
            Debug.LogError($"Shader von Objekt {go.name} hat den Parameter '{dissolveProperty}' nicht!");
            isAnimating = false;
            yield break;
        }

        float elapsed = 0f;
        float startValue = 1f;
        float endValue = 0f;

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);

            float val = Mathf.Lerp(startValue, endValue, t);
            mat.SetFloat(dissolveProperty, val);

            yield return null;
        }

        mat.SetFloat(dissolveProperty, endValue);

        currentIndex++;
        isAnimating = false;
    }
}
