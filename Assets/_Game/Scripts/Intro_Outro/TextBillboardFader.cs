using UnityEngine;

[ExecuteAlways]
public class TextBillboardFader : MonoBehaviour
{
    [Header("Angezeigter Text")]
    [SerializeField] private string text = "Hier bin ich!";
    
    [Header("Text Einstellungen")]
    [SerializeField] private float fontSize = 0.2f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.3f, 0); // Jetzt alle Achsen

    [Header("Sichtbarkeit je Entfernung")]
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float fadeSpeed = 5f;

    private TextMesh textMesh;
    private Camera mainCam;
    private float currentAlpha = 0f;

    void Start()
    {
        mainCam = Camera.main ?? FindFirstObjectByType<Camera>();
        SetupText();
    }

    void SetupText()
    {
        Transform existing = transform.Find("BillboardText");
        if (existing != null)
        {
            textMesh = existing.GetComponent<TextMesh>();
        }
        else
        {
            GameObject go = new GameObject("BillboardText");
            go.transform.SetParent(transform);
            textMesh = go.AddComponent<TextMesh>();
        }

        textMesh.text = text;
        textMesh.fontSize = 100;
        textMesh.characterSize = fontSize;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = textColor;
        textMesh.transform.localPosition = offset;
    }

    void Update()
    {
        if (mainCam == null) return;
        if (textMesh == null) SetupText();

        // Billboard zur Kamera, aufrecht
        Vector3 direction = mainCam.transform.position - textMesh.transform.position;
        textMesh.transform.rotation = Quaternion.LookRotation(-direction.normalized, Vector3.up);

        // Lokale Position anwenden
        textMesh.transform.localPosition = offset;

        // Abstand zur Kamera
        float distance = Vector3.Distance(mainCam.transform.position, textMesh.transform.position);
        float targetAlpha = Mathf.InverseLerp(maxDistance, minDistance, distance);
        targetAlpha = Mathf.Clamp01(targetAlpha);

        // Sanftes Ein-/Ausblenden
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Textfarbe mit Alpha setzen
        Color c = textColor;
        c.a = currentAlpha;
        textMesh.color = c;
    }

    void OnValidate()
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.characterSize = fontSize;
            textMesh.color = textColor;
            textMesh.transform.localPosition = offset;
        }
    }
}
