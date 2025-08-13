using UnityEngine;
using UnityEngine.InputSystem;

public class CarouselController : MonoBehaviour
{
    [Header("Input")]
    public InputAction clickAction;

    [Header("Rotation Settings")]
    public float rotationDuration = 2.0f;     // Dauer der Drehung
    public float easingPower = 3.0f;          // Für sanftes Stoppen

    private bool isRotating = false;
    private bool hasTriggeredScene = false;
    private float idleTimer = 0f;

    private float startY;
    private float targetY;
    private float elapsedTime;

    private void OnEnable()
    {
        if (clickAction != null)
        {
            clickAction.Enable();
            clickAction.performed += OnClick;
        }
    }

    private void OnDisable()
    {
        if (clickAction != null)
        {
            clickAction.performed -= OnClick;
            clickAction.Disable();
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        if (!isRotating)
        {
            hasTriggeredScene = false;
            idleTimer = 0f;

            startY = transform.eulerAngles.y;

            int slotIndex = Random.Range(0, 4);                // 0–3
            float slotOffset = slotIndex * 90f;

            int fullRotations = Random.Range(2, 6);            // 2–5 Umdrehungen
            float totalRotation = fullRotations * 360f + slotOffset;

            targetY = startY + totalRotation;
            elapsedTime = 0f;
            isRotating = true;
        }
    }

    private void Update()
    {
        if (isRotating)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotationDuration);

            // Cubic ease out
            float easedT = 1f - Mathf.Pow(1f - t, easingPower);
            float currentY = Mathf.Lerp(startY, targetY, easedT);

            transform.rotation = Quaternion.Euler(0f, currentY, 0f);

            if (t >= 1f)
            {
                isRotating = false;
                idleTimer = 0f;
            }
        }
        else if (!hasTriggeredScene)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= 2f)
            {
                float finalY = transform.eulerAngles.y % 360f;
                int selectedIndex = Mathf.RoundToInt(finalY / 90f) % 4;

                Transform selectedChild = transform.GetChild(selectedIndex);
                SceneLoader loader = selectedChild.GetComponent<SceneLoader>();
                if (loader != null)
                {
                    loader.LoadScene();
                    hasTriggeredScene = true;
                }
            }
        }
    }
}
