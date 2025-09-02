using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraAFKDetector : MonoBehaviour
{
    [SerializeField] private float afkTime = 180f;
    [SerializeField] private string sceneToLoad = "Intro";
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private bool showDebug = true; // Checkbox im Inspector

    private Vector3 lastPosition;
    private float idleTimer;

    void Start()
    {
        lastPosition = transform.position;
        idleTimer = 0f;
    }

    void Update()
    {
        float sqrDistance = (transform.position - lastPosition).sqrMagnitude;

        if (sqrDistance > movementThreshold * movementThreshold)
        {
            if (showDebug) Debug.Log("Bewegung erkannt → Timer zurückgesetzt");
            idleTimer = 0f;
            lastPosition = transform.position;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (showDebug) Debug.Log($"Keine Bewegung → Timer = {idleTimer:F2} Sekunden");

            if (idleTimer >= afkTime)
            {
                if (showDebug) Debug.Log("AFK erkannt → Szene wird geladen!");
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}










/*
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraAFKDetector : MonoBehaviour
{
    [SerializeField] private float afkTime = 180f;
    [SerializeField] private string sceneToLoad = "Intro";
    [SerializeField] private float movementThreshold = 0.1f;

    private Vector3 lastPosition;
    private float idleTimer;

    void Start()
    {
        lastPosition = transform.position;
        idleTimer = 0f;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);

        if (distance > movementThreshold)
        {
            Debug.Log("Bewegung erkannt → Timer zurückgesetzt");
            idleTimer = 0f;
            lastPosition = transform.position;
        }
        else
        {
            idleTimer += Time.deltaTime;
            Debug.Log($"Keine Bewegung → Timer = {idleTimer:F2} Sekunden");

            if (idleTimer >= afkTime)
            {
                Debug.Log("AFK erkannt → Szene wird geladen!");
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}
decimal 

*/