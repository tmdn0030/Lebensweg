using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class DoubleClickSceneLoader : MonoBehaviour
{
    [Header("Szenen-Einstellungen")]
    [SerializeField] private string sceneNameToLoad = "NextScene";
    [SerializeField] private float delayBeforeSceneLoad = 0.5f;

    [Header("Doppelklick / Tap-Erkennung")]
    [SerializeField] private float doubleClickThreshold = 0.3f;

    private float lastClickTime = -1f;
    private Camera mainCam;
    private bool isWaiting = false;

    private void Start()
    {
        mainCam = Camera.main ?? FindFirstObjectByType<Camera>();
    }

    void Update()
    {
        if (isWaiting) return;

        Vector2? inputPosition = null;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputPosition = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (inputPosition.HasValue)
        {
            float time = Time.time;
            if (time - lastClickTime < doubleClickThreshold)
            {
                lastClickTime = -1f; // Reset
                TryHitAndStartSceneLoad(inputPosition.Value);
            }
            else
            {
                lastClickTime = time;
            }
        }
    }

    void TryHitAndStartSceneLoad(Vector2 screenPosition)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                Debug.Log($"Double click/tap erkannt auf {hit.transform.name}. Szene wird in {delayBeforeSceneLoad} Sekunden geladen...");
                StartCoroutine(DelayedSceneLoad());
            }
        }
    }

    IEnumerator DelayedSceneLoad()
    {
        isWaiting = true;
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        SceneManager.LoadScene(sceneNameToLoad);
    }
}
