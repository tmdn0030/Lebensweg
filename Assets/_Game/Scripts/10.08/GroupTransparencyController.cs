using UnityEngine;

public class GroupTransparencyController : MonoBehaviour
{
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
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;

                // Falls der Shader nicht auf transparent eingestellt ist,
                // muss das Rendering Mode angepasst werden (optional)
            }
        }
    }
}
