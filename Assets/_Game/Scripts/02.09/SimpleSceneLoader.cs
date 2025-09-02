using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    [Header("Name der Szene, die geladen werden soll")]
    public string sceneToLoad = "Outro";

    // Diese Funktion kannst du direkt im Button OnClick() eintragen
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
