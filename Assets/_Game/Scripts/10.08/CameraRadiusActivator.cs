using System.Collections.Generic;
using UnityEngine;

public class CameraRadiusActivator : MonoBehaviour
{
    [Header("Settings")]
    public float fadeRadius = 20f;             // Sichtbarkeitsradius
    public float deactivateExtension = 2f;     // Extra Abstand, ab dem Objekt deaktiviert wird
    public float fadeSpeed = 3f;                // Fadingspeed
    public string alphaProperty = "_Alpha";    // Shader Property-Name
    public bool isColorProperty = false;        // True wenn Color-Property mit Alpha genutzt wird
    public string targetShaderName = "Shader Graphs/MyTransparentShader"; // Shadername

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

            // 1. Wenn außerhalb des deactivate Radius → Objekt deaktivieren
            if (dist > fadeRadius + deactivateExtension)
            {
                if (r.gameObject.activeSelf)
                    r.gameObject.SetActive(false);
                continue; // Nichts weiter tun wenn deaktiviert
            }
            else
            {
                // Objekt ggf. reaktivieren
                if (!r.gameObject.activeSelf)
                    r.gameObject.SetActive(true);
            }

            // 2. Innerhalb fadeRadius alpha auf 1, sonst auf 0 faden
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