using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneLoaderOnTrigger : MonoBehaviour
{
    [Header("Name der Szene, die geladen werden soll")]
    public string sceneToLoad = "Outro";

    private void OnTriggerEnter(Collider other)
    {
        // Prüfen, dass z.B. die Kamera oder der Spieler das Trigger-Objekt betritt
        // Hier kannst du den Tag "Player" setzen oder direkt Transform vergleichen
        if (other.CompareTag("MainCamera"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
