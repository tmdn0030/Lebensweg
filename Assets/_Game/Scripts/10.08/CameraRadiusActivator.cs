
using System.Collections.Generic;
using UnityEngine;

public class CameraRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;        // Sichtbarkeitsradius
    public float fadeSpeed = 3f;          // Fadingspeed
    public string alphaProperty = "_Alpha"; // Shader Property-Name (z. B. "_Alpha" oder "_BaseColor")
    public bool isColorProperty = false;  // True, wenn Alpha in einer Color-Property steckt
    public string targetShaderName = "Shader Graphs/MyTransparentShader"; // exakter Shadername

    List<Renderer> renderers = new List<Renderer>();
    Dictionary<Renderer, float> currentAlpha = new Dictionary<Renderer, float>();

    void Start()
    {
        // Alle Renderer finden
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer r in allRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;

            // 1️⃣ Nur Objekte mit passendem Shader nehmen
            if (r.sharedMaterial.shader.name != targetShaderName) continue;

            // 2️⃣ Nur wenn die Property existiert
            if (!r.sharedMaterial.HasProperty(alphaProperty)) continue;

            renderers.Add(r);
            currentAlpha[r] = 0f; // Start Alpha = 0
            ApplyAlpha(r, 0f);
        }
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var r in renderers)
        {
            if (r == null) continue;

            float dist = Vector3.Distance(camPos, r.transform.position);
            float targetAlpha = dist <= fadeRadius ? 1f : 0f;

            float a = Mathf.MoveTowards(currentAlpha[r], targetAlpha, fadeSpeed * Time.deltaTime);
            currentAlpha[r] = a;

            ApplyAlpha(r, a);
        }
    }

    void ApplyAlpha(Renderer r, float alpha)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        if (isColorProperty)
        {
            Color c = mpb.GetColor(alphaProperty);
            if (c == default) c = Color.white;
            c.a = alpha;
            mpb.SetColor(alphaProperty, c);
        }
        else
        {
            mpb.SetFloat(alphaProperty, alpha);
        }

        r.SetPropertyBlock(mpb);
    }
}


















/*
using System.Collections.Generic;
using UnityEngine;

public class CameraRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;        // Sichtbarkeitsradius
    public float fadeSpeed = 3f;          // Fadingspeed
    public string alphaProperty = "_Alpha"; // Shader Property-Name (z. B. "_Alpha" oder "_BaseColor")
    public bool isColorProperty = false;  // True, wenn Alpha in einer Color-Property steckt
    public string targetShaderName = "Shader Graphs/MyTransparentShader"; // exakter Shadername

    List<Renderer> renderers = new List<Renderer>();
    Dictionary<Renderer, float> currentAlpha = new Dictionary<Renderer, float>();

    void Start()
    {
        // Alle Renderer finden
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer r in allRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;

            // 1️⃣ Nur Objekte mit passendem Shader nehmen
            if (r.sharedMaterial.shader.name != targetShaderName) continue;

            // 2️⃣ Nur wenn die Property existiert
            if (!r.sharedMaterial.HasProperty(alphaProperty)) continue;

            renderers.Add(r);
            currentAlpha[r] = 0f; // Start Alpha = 0
            ApplyAlpha(r, 0f);
        }
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var r in renderers)
        {
            if (r == null) continue;

            float dist = Vector3.Distance(camPos, r.transform.position);
            float targetAlpha = dist <= fadeRadius ? 1f : 0f;

            float a = Mathf.MoveTowards(currentAlpha[r], targetAlpha, fadeSpeed * Time.deltaTime);
            currentAlpha[r] = a;

            ApplyAlpha(r, a);
        }
    }

    void ApplyAlpha(Renderer r, float alpha)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        if (isColorProperty)
        {
            Color c = mpb.GetColor(alphaProperty);
            if (c == default) c = Color.white;
            c.a = alpha;
            mpb.SetColor(alphaProperty, c);
        }
        else
        {
            mpb.SetFloat(alphaProperty, alpha);
        }

        r.SetPropertyBlock(mpb);
    }
}


















using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;
    public float fadeSpeed = 3f;
    public string alphaProperty = "_Alpha";
    public bool isColorProperty = false;
    public string targetShaderName = "Shader Graphs/MyTransparentShader";
    public float stableTimeAfterFade = 1f;

    private List<Renderer> renderers = new List<Renderer>();
    private Dictionary<Renderer, Coroutine> activeFades = new Dictionary<Renderer, Coroutine>();

    void Start()
    {
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer r in allRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;
            if (r.sharedMaterial.shader.name != targetShaderName) continue;
            if (!r.sharedMaterial.HasProperty(alphaProperty)) continue;

            renderers.Add(r);
            ApplyAlpha(r, 0f);
        }
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var r in renderers)
        {
            float dist = Vector3.Distance(camPos, r.transform.position);
            float targetAlpha = dist <= fadeRadius ? 1f : 0f;

            // Nur starten, wenn nicht schon fade läuft
            if (activeFades.ContainsKey(r) && activeFades[r] != null) continue;

            if ((targetAlpha == 1f && GetAlpha(r) < 1f) || (targetAlpha == 0f && GetAlpha(r) > 0f))
            {
                activeFades[r] = StartCoroutine(FadeCoroutine(r, targetAlpha));
            }
        }
    }

    private IEnumerator FadeCoroutine(Renderer r, float targetAlpha)
    {
        float alpha = GetAlpha(r);

        while (!Mathf.Approximately(alpha, targetAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            ApplyAlpha(r, alpha);
            yield return null;
        }

        // Stabilitätspuffer
        yield return new WaitForSeconds(stableTimeAfterFade);

        activeFades[r] = null;
    }

    private void ApplyAlpha(Renderer r, float alpha)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        if (isColorProperty)
        {
            Color c = mpb.GetColor(alphaProperty);
            if (c == default) c = Color.white;
            c.a = alpha;
            mpb.SetColor(alphaProperty, c);
        }
        else
        {
            mpb.SetFloat(alphaProperty, alpha);
        }

        r.SetPropertyBlock(mpb);
    }

    private float GetAlpha(Renderer r)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        if (isColorProperty) return mpb.GetColor(alphaProperty).a;
        else return mpb.GetFloat(alphaProperty);
    }

    // ------------------- NEU: Für AlphaRadiusZone -------------------
    public List<Renderer> Renderers => renderers;

    public void StartFade(Renderer r, bool fadeIn)
    {
        if (!renderers.Contains(r)) return;

        float targetAlpha = fadeIn ? 1f : 0f;

        if (activeFades.ContainsKey(r) && activeFades[r] != null)
        {
            StopCoroutine(activeFades[r]);
        }

        activeFades[r] = StartCoroutine(FadeCoroutine(r, targetAlpha));
    }
}
















using System.Collections.Generic;
using UnityEngine;

public class CameraRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;
    public float fadeSpeed = 3f;
    public string alphaProperty = "_Alpha";
    public bool isColorProperty = false;
    public string targetShaderName = "Shader Graphs/MyTransparentShader";

    private float originalFadeRadius;

    List<Renderer> renderers = new List<Renderer>();
    Dictionary<Renderer, float> currentAlpha = new Dictionary<Renderer, float>();

    // Debug GUI Position
    private Rect debugRect = new Rect(10, Screen.height - 60, 300, 50);

    void Start()
    {
        originalFadeRadius = fadeRadius;

        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer r in allRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;
            if (r.sharedMaterial.shader.name != targetShaderName) continue;
            if (!r.sharedMaterial.HasProperty(alphaProperty)) continue;

            renderers.Add(r);
            currentAlpha[r] = 0f;
            ApplyAlpha(r, 0f);
        }
    }

    public void SetTemporaryRadius(float newRadius)
    {
        fadeRadius = newRadius;
    }

    public void ResetRadius()
    {
        fadeRadius = originalFadeRadius;
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var r in renderers)
        {
            if (r == null) continue;

            float dist = Vector3.Distance(camPos, r.transform.position);
            float targetAlpha = dist <= fadeRadius ? 1f : 0f;

            float a = Mathf.MoveTowards(currentAlpha[r], targetAlpha, fadeSpeed * Time.deltaTime);
            currentAlpha[r] = a;
            ApplyAlpha(r, a);
        }
    }

    void ApplyAlpha(Renderer r, float alpha)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        if (isColorProperty)
        {
            Color c = mpb.GetColor(alphaProperty);
            if (c == default) c = Color.white;
            c.a = alpha;
            mpb.SetColor(alphaProperty, c);
        }
        else
        {
            mpb.SetFloat(alphaProperty, alpha);
        }

        r.SetPropertyBlock(mpb);
    }

    void OnGUI()
    {
        // GUI-Style
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        GUI.Box(new Rect(10, Screen.height - 65, 250, 55), ""); // Hintergrundbox
        GUI.Label(new Rect(15, Screen.height - 60, 250, 20), $"Fade Radius: {fadeRadius:F2}", style);
        GUI.Label(new Rect(15, Screen.height - 40, 250, 20), $"Original Radius: {originalFadeRadius:F2}", style);
        GUI.Label(new Rect(15, Screen.height - 20, 250, 20), $"Renderers: {renderers.Count}", style);
    }
}
















using System.Collections.Generic;
using UnityEngine;

public class CameraRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;
    public float deactivateExtension = 2f;
    public float fadeSpeed = 3f;
    public string alphaProperty = "_Alpha";
    public bool isColorProperty = false;
    public string targetShaderName = "Shader Graphs/MyTransparentShader";

    private float originalFadeRadius;
    private float originalDeactivateExtension;

    List<Renderer> renderers = new List<Renderer>();
    Dictionary<Renderer, float> currentAlpha = new Dictionary<Renderer, float>();

    void Start()
    {
        originalFadeRadius = fadeRadius;
        originalDeactivateExtension = deactivateExtension;

        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer r in allRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;
            if (r.sharedMaterial.shader.name != targetShaderName) continue;
            if (!r.sharedMaterial.HasProperty(alphaProperty)) continue;

            renderers.Add(r);
            currentAlpha[r] = 0f;
            ApplyAlpha(r, 0f);
            r.gameObject.SetActive(true);
        }
    }


    public void SetTemporaryRadius(float newRadius)
    {
        fadeRadius = newRadius;
        deactivateExtension = originalDeactivateExtension; // Optional: oder eigenen Wert setzen
    }


    public void ResetRadius()
    {
        fadeRadius = originalFadeRadius;
        deactivateExtension = originalDeactivateExtension;
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var r in renderers)
        {
            if (r == null) continue;

            float dist = Vector3.Distance(camPos, r.transform.position);

            if (dist > fadeRadius + deactivateExtension)
            {
                if (r.gameObject.activeSelf)
                    r.gameObject.SetActive(false);
                continue;
            }
            else
            {
                if (!r.gameObject.activeSelf)
                    r.gameObject.SetActive(true);
            }

            float targetAlpha = dist <= fadeRadius ? 1f : 0f;
            float a = Mathf.MoveTowards(currentAlpha[r], targetAlpha, fadeSpeed * Time.deltaTime);
            currentAlpha[r] = a;
            ApplyAlpha(r, a);
        }
    }

    void ApplyAlpha(Renderer r, float alpha)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        if (isColorProperty)
        {
            Color c = mpb.GetColor(alphaProperty);
            if (c == default) c = Color.white;
            c.a = alpha;
            mpb.SetColor(alphaProperty, c);
        }
        else
        {
            mpb.SetFloat(alphaProperty, alpha);
        }

        r.SetPropertyBlock(mpb);
    }
}

*/






/*
public void SetTemporaryRadius(float newRadius)
{
    float erweiterung = newRadius - originalFadeRadius;
    fadeRadius = newRadius;
    deactivateExtension = originalDeactivateExtension + erweiterung;
}
*/


/*
using System.Collections.Generic;
using UnityEngine;

public class CameraRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;
    public float deactivateRadius = 50f; // Ein deutlich größerer Radius für die Deaktivierung
    public float fadeSpeed = 3f;
    public string alphaProperty = "_Alpha";
    public bool isColorProperty = false;
    public string targetShaderName = "Shader Graphs/MyTransparentShader";

    List<Renderer> renderers = new List<Renderer>();
    Dictionary<Renderer, float> currentAlpha = new Dictionary<Renderer, float>();

    void Start()
    {
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer r in allRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;
            if (r.sharedMaterial.shader.name != targetShaderName) continue;
            if (!r.sharedMaterial.HasProperty(alphaProperty)) continue;

            renderers.Add(r);
            currentAlpha[r] = 0f;
            ApplyAlpha(r, 0f);
            r.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var r in renderers)
        {
            if (r == null) continue;

            float dist = Vector3.Distance(camPos, r.transform.position);

            // Separate Logik für Deaktivierung
            if (dist > deactivateRadius)
            {
                if (r.gameObject.activeSelf)
                    r.gameObject.SetActive(false);
                continue;
            }
            else if (!r.gameObject.activeSelf)
            {
                r.gameObject.SetActive(true);
            }

            // Fading-Logik
            float targetAlpha = dist <= fadeRadius ? 1f : 0f;
            float a = Mathf.MoveTowards(currentAlpha[r], targetAlpha, fadeSpeed * Time.deltaTime);
            currentAlpha[r] = a;
            ApplyAlpha(r, a);
        }
    }

    void ApplyAlpha(Renderer r, float alpha)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        if (isColorProperty)
        {
            Color c = mpb.GetColor(alphaProperty);
            if (c == default) c = Color.white;
            c.a = alpha;
            mpb.SetColor(alphaProperty, c);
        }
        else
        {
            mpb.SetFloat(alphaProperty, alpha);
        }

        r.SetPropertyBlock(mpb);
    }
}
























using System.Collections.Generic;
using UnityEngine;

public class CameraRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;        // Sichtbarkeitsradius
    public float fadeSpeed = 3f;          // Fadingspeed
    public string alphaProperty = "_Alpha"; // Shader Property-Name (z. B. "_Alpha" oder "_BaseColor")
    public bool isColorProperty = false;  // True, wenn Alpha in einer Color-Property steckt
    public string targetShaderName = "Shader Graphs/MyTransparentShader"; // exakter Shadername

    List<Renderer> renderers = new List<Renderer>();
    Dictionary<Renderer, float> currentAlpha = new Dictionary<Renderer, float>();

    void Start()
    {
        // Alle Renderer finden
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer r in allRenderers)
        {
            if (r == null || r.sharedMaterial == null) continue;

            // 1️⃣ Nur Objekte mit passendem Shader nehmen
            if (r.sharedMaterial.shader.name != targetShaderName) continue;

            // 2️⃣ Nur wenn die Property existiert
            if (!r.sharedMaterial.HasProperty(alphaProperty)) continue;

            renderers.Add(r);
            currentAlpha[r] = 0f; // Start Alpha = 0
            ApplyAlpha(r, 0f);
        }
    }

    void Update()
    {
        Vector3 camPos = transform.position;

        foreach (var r in renderers)
        {
            if (r == null) continue;

            float dist = Vector3.Distance(camPos, r.transform.position);
            float targetAlpha = dist <= fadeRadius ? 1f : 0f;

            float a = Mathf.MoveTowards(currentAlpha[r], targetAlpha, fadeSpeed * Time.deltaTime);
            currentAlpha[r] = a;

            ApplyAlpha(r, a);
        }
    }

    void ApplyAlpha(Renderer r, float alpha)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        if (isColorProperty)
        {
            Color c = mpb.GetColor(alphaProperty);
            if (c == default) c = Color.white;
            c.a = alpha;
            mpb.SetColor(alphaProperty, c);
        }
        else
        {
            mpb.SetFloat(alphaProperty, alpha);
        }

        r.SetPropertyBlock(mpb);
    }
}

















using UnityEngine;
using System.Collections.Generic;

public class CameraRadiusActivator : MonoBehaviour
{
    public float activationRadius = 50f;
    public float deactivationRadius = 60f;
    private List<Transform> objects = new List<Transform>();

    void Start()
    {
        // Alle MeshRenderer-Objekte schnell und ohne Sortierung finden
        foreach (var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            objects.Add(renderer.transform);
        }
    }


    void Update()
    {
        Vector3 camPos = transform.position;
        float activationSqr = activationRadius * activationRadius;
        float deactivationSqr = deactivationRadius * deactivationRadius;

        foreach (Transform obj in objects)
        {
            float distSqr = (obj.position - camPos).sqrMagnitude;
            bool isActive = obj.gameObject.activeSelf;

            if (!isActive && distSqr < activationSqr)
                obj.gameObject.SetActive(true);
            else if (isActive && distSqr > deactivationSqr)
                obj.gameObject.SetActive(false);
        }
    }
}

*/