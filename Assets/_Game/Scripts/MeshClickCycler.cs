using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MeshClickCycler : MonoBehaviour
{
    [Header("Meshes")]
    public GameObject[] meshStates; // Die verschiedenen Meshes
    private int currentIndex = 0;

    [Header("Text Management")]
    public TextMeshProUGUI sharedText;
    public string[] textPerMesh;
    public GameObject[] optionalTextObjects;

    [Header("Text Animation")]
    public Animator textAnimator;
    public string animationTrigger = "OnChange";

    // Diese Methode wird manuell von externem Click-Handler aufgerufen
    public void CycleToNextMesh()
    {
        // Aktuelles Mesh und Text deaktivieren
        if (meshStates.Length == 0) return;

        meshStates[currentIndex].SetActive(false);

        if (optionalTextObjects.Length > currentIndex && optionalTextObjects[currentIndex] != null)
            optionalTextObjects[currentIndex].SetActive(false);

        currentIndex = (currentIndex + 1) % meshStates.Length;

        UpdateState();
    }

    void Start()
    {
        UpdateState();
    }

    void UpdateState()
    {
        meshStates[currentIndex].SetActive(true);

        if (optionalTextObjects.Length > currentIndex && optionalTextObjects[currentIndex] != null)
        {
            optionalTextObjects[currentIndex].SetActive(true);
            if (sharedText != null) sharedText.gameObject.SetActive(false);
        }
        else
        {
            if (sharedText != null)
            {
                sharedText.gameObject.SetActive(true);
                if (textPerMesh.Length > currentIndex)
                    sharedText.text = textPerMesh[currentIndex];
            }
        }

        if (textAnimator != null && !string.IsNullOrEmpty(animationTrigger))
        {
            textAnimator.SetTrigger(animationTrigger);
        }
    }
}
