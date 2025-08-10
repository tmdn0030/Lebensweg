using UnityEngine;

public class GroupTransparencyController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float alpha = 1f; // 0 = unsichtbar, 1 = sichtbar

    private Renderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        UpdateAlpha();
    }

    public void SetAlpha(float newAlpha)
    {
        alpha = Mathf.Clamp01(newAlpha);
        UpdateAlpha();
    }

    void UpdateAlpha()
    {
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                // Hier gezielt die Shader-Property "Alpha" setzen
                mat.SetFloat("_Alpha", alpha);
            }
        }
    }
}
