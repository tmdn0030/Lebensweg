using UnityEngine;
using UnityEngine.Splines;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TextZone : MonoBehaviour
{
    [Header("Spline-Zone")]
    public SplineContainer spline;
    [Tooltip("Triggerpunkt auf der Spline in Metern")]
    public float triggerDistance = 0f;

    [Tooltip("Mit Objekt mitwandern")]
    public bool followTransform = false;

    [HideInInspector] public float offsetToSpline = 0f;
    private bool lastFollowTransform = false;

    [Header("Text & Anzeige")]
    [Tooltip("Textabschnitte, die angezeigt werden")]
    [TextArea(2, 6)]
    public List<string> textPages = new List<string>() { "Willkommen in der Zone!", "Ziehe um fortzufahren...", "Fertig!" };

    public TMP_FontAsset font;
    public int fontSize = 48;
    public Color fontColor = Color.white;
    public float maxWidth = 600f;
    public float boxPadding = 32f;
    public Color boxColor = new Color(0, 0, 0, 0.8f);

    [Header("Dialogbox Optionen")]
    public Vector2 anchoredPosition = new Vector2(0, -200);
    public bool centerOnScreen = true;

    [Header("Kamera-Interaktion")]
    public bool lookAtText = true;
    public Transform lookAtTarget; // Optional
    public float lookAtLerpSpeed = 4f; // Wie smooth zum Ziel blicken

    // --- RUNTIME ---
    [HideInInspector] public bool isActive = false;
    [HideInInspector] public int currentPage = 0;

    private Canvas dialogCanvas;
    private GameObject panelObj;
    private TMP_Text textComp;

    // Für Drag-Input:
    [HideInInspector] public bool isDragging = false;
    [HideInInspector] public float lastY = 0f;
    private float accumulatedScroll = 0f;
    public float scrollStep = 80f; // Pixel für Seitenwechsel

    // Text Fade/Move
    private GameObject prevTextGO;
    private Coroutine fadeCoroutine;

    private Camera mainCam;
    private Quaternion originalRotation;
    private bool camIsOverriding = false;

    void OnValidate() => UpdateZone();
    void Update()
    {
        UpdateZone();

        // Während Text aktiv ist: Kamera smooth zum Ziel rotieren (NUR, wenn LookAtText aktiv!)
        if (isActive && lookAtText && lookAtTarget != null && mainCam != null)
        {
            Vector3 to = lookAtTarget.position - mainCam.transform.position;
            Quaternion lookRot = Quaternion.LookRotation(to.normalized, Vector3.up);
            mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, lookRot, Time.deltaTime * lookAtLerpSpeed);
            camIsOverriding = true;
        }
        else if (camIsOverriding && mainCam != null)
        {
            // Nach Dialog Kamera zurück (optional)
            camIsOverriding = false;
        }
    }

    private void UpdateZone()
    {
        if (spline == null || spline.Spline == null) return;
        float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);

        if (followTransform != lastFollowTransform)
        {
            if (followTransform) CacheOffsetToSpline();
            lastFollowTransform = followTransform;
        }
        if (followTransform)
            UpdateTriggerDistanceFromTransform();

        triggerDistance = Mathf.Clamp(triggerDistance, 0, totalLength);
    }

    private void CacheOffsetToSpline()
    {
        if (spline == null || spline.Spline == null) return;
        Unity.Mathematics.float3 nearest;
        float t;
        SplineUtility.GetNearestPoint(spline.Spline, (Unity.Mathematics.float3)transform.position, out nearest, out t);
        float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);
        float posOnSpline = Mathf.Clamp(totalLength * t, 0, totalLength);
        offsetToSpline = triggerDistance - posOnSpline;
    }

    private void UpdateTriggerDistanceFromTransform()
    {
        if (spline == null || spline.Spline == null) return;
        Unity.Mathematics.float3 nearest;
        float t;
        SplineUtility.GetNearestPoint(spline.Spline, (Unity.Mathematics.float3)transform.position, out nearest, out t);
        float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);
        float posOnSpline = Mathf.Clamp(totalLength * t, 0, totalLength);
        triggerDistance = Mathf.Clamp(posOnSpline + offsetToSpline, 0, totalLength);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (spline != null && spline.Spline != null)
        {
            float totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);
            float t = Mathf.Clamp01(triggerDistance / totalLength);
            Vector3 pos = (Vector3)SplineUtility.EvaluatePosition(spline.Spline, t);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pos, 0.18f);

            // Wenn LookAtTarget, Linie einzeichnen
            if (lookAtTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(pos, lookAtTarget.position);
                Gizmos.DrawWireSphere(lookAtTarget.position, 0.14f);
            }
        }
    }
#endif

    // Wird vom MovementController gecallt!
    public bool CheckAndTrigger(float currentDistance, Camera cam)
    {
        if (!isActive && Mathf.Abs(currentDistance - triggerDistance) < 0.2f)
        {
            ActivateTextBox(cam);
            return true;
        }
        if (isActive) return true;
        return false;
    }

    // Drag-Input aus MovementController
    public void HandleDragScroll(float deltaY, Camera cam = null)
    {
        if (!isActive) return;
        accumulatedScroll += deltaY;

        // Blättern vorwärts
        while (accumulatedScroll > scrollStep)
        {
            NextPage();
            accumulatedScroll -= scrollStep;
        }
        // Blättern rückwärts
        while (accumulatedScroll < -scrollStep)
        {
            PrevPage();
            accumulatedScroll += scrollStep;
        }
    }

    public void ActivateTextBox(Camera cam = null)
    {
        isActive = true;
        currentPage = 0;
        ShowTextBox();
        if (cam == null) cam = Camera.main;
        mainCam = cam;
        if (mainCam != null)
            originalRotation = mainCam.transform.rotation;
    }

    public void NextPage()
    {
        if (!isActive) return;
        if (currentPage < textPages.Count - 1)
        {
            FadeAndMoveOldText();
            currentPage++;
            ShowTextBox();
        }
        else
        {
            HideTextBox();
        }
    }
    public void PrevPage()
    {
        if (!isActive) return;
        if (currentPage > 0)
        {
            FadeAndMoveOldText();
            currentPage--;
            ShowTextBox();
        }
    }

    public void HideTextBox()
    {
        isActive = false;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = null;

        if (dialogCanvas != null) Destroy(dialogCanvas.gameObject);
        dialogCanvas = null;
        panelObj = null;
        textComp = null;

        // Kamera zurücksetzen (optional)
        if (mainCam != null)
            mainCam.transform.rotation = originalRotation;
    }

    private void ShowTextBox()
    {
        if (dialogCanvas != null) Destroy(dialogCanvas.gameObject);

        dialogCanvas = new GameObject("TextZoneCanvas").AddComponent<Canvas>();

        if (!centerOnScreen && followTransform)
        {
            dialogCanvas.renderMode = RenderMode.WorldSpace;
            dialogCanvas.transform.position = transform.position;
            dialogCanvas.transform.rotation = transform.rotation;
            dialogCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(maxWidth + boxPadding * 2, fontSize * 2 + boxPadding * 2);
            dialogCanvas.GetComponent<RectTransform>().localScale = Vector3.one * 0.01f;
        }
        else
        {
            dialogCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        dialogCanvas.sortingOrder = 5000;

        panelObj = new GameObject("DialogPanel");
        panelObj.transform.SetParent(dialogCanvas.transform, false);
        RectTransform rect = panelObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(maxWidth + boxPadding * 2, fontSize * 2 + boxPadding * 2);
        rect.anchoredPosition = anchoredPosition;

        Image bg = panelObj.AddComponent<Image>();
        bg.color = boxColor;

        GameObject textGO = new GameObject("DialogText");
        textGO.transform.SetParent(panelObj.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(maxWidth, fontSize * 2);
        textRect.anchoredPosition = Vector2.zero;

        textComp = textGO.AddComponent<TextMeshProUGUI>();
        textComp.text = textPages[currentPage];
        textComp.fontSize = fontSize;
        textComp.color = fontColor;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.textWrappingMode = TextWrappingModes.Normal;
        textComp.rectTransform.sizeDelta = new Vector2(maxWidth, fontSize * 2);

        if (font != null)
            textComp.font = font;
    }

    private void FadeAndMoveOldText()
    {
        if (textComp == null) return;
        GameObject oldTextGO = textComp.gameObject;
        prevTextGO = oldTextGO;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAndMoveCoroutine(oldTextGO));
    }

    private IEnumerator FadeAndMoveCoroutine(GameObject go)
    {
        if (go == null) yield break;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp == null) yield break;
        Color c = tmp.color;
        float startY = go.GetComponent<RectTransform>().anchoredPosition.y;
        float targetY = startY + 100f;
        float t = 0f;
        while (t < 1f)
        {
            // SCHUTZ: Prüfe, ob Objekt schon weg ist!
            if (go == null || tmp == null)
                yield break;

            t += Time.deltaTime * 1.5f;
            float alpha = Mathf.Lerp(1f, 0f, t);
            float posY = Mathf.Lerp(startY, targetY, t);
            tmp.color = new Color(c.r, c.g, c.b, alpha);
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, posY);
            yield return null;
        }
        if (go != null)
            Destroy(go);
    }
}
