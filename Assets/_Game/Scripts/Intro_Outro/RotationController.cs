using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RotationController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField, Range(0f, 1f)] private float damping = 0.9f;
    [SerializeField] private float minVelocity = 0.05f;
    [SerializeField] private bool invertDirection = true;

    [Header("Target Settings")]
    [SerializeField] private Transform rotationTarget;
    [SerializeField] private Transform focusTarget;
    [SerializeField] private float lookAtSpeed = 2f;

    // Debug Info
    private string debugHit = "Kein Hit";
    private Vector2 debugInputPosition;
    private bool debugDragging;
    private Vector3 debugVelocity;
    private string debugLookAtStatus = "-";

    private bool isDragging = false;
    private Vector2 lastInputPosition;
    private Vector3 currentVelocity;
    private Camera mainCamera;

    private Coroutine lookAtCoroutine;

    private Mouse mouse;
    private Touchscreen touchscreen;

    void Start()
    {
        mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        if (rotationTarget == null)
            rotationTarget = transform;

        mouse = Mouse.current;
        touchscreen = Touchscreen.current;
    }

    void Update()
    {
        HandleInput();

        if (!isDragging && lookAtCoroutine == null)
        {
            if (currentVelocity.magnitude > minVelocity)
            {
                Vector3 rotationStep = currentVelocity * (invertDirection ? -1f : 1f) * Time.deltaTime;
                rotationTarget.Rotate(rotationStep, Space.World);
                currentVelocity *= damping;
            }
            else
            {
                currentVelocity = Vector3.zero;
            }
        }

        debugDragging = isDragging;
        debugVelocity = currentVelocity;
    }

    void HandleInput()
    {
        Vector2? inputPosition = null;

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            inputPosition = mouse.position.ReadValue();
        else if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            inputPosition = touchscreen.primaryTouch.position.ReadValue();

        if (inputPosition.HasValue)
        {
            debugInputPosition = inputPosition.Value;

            Ray ray = mainCamera.ScreenPointToRay(inputPosition.Value);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                debugHit = hit.collider.name;

                if (focusTarget != null && hit.transform == focusTarget)
                {
                    StartLookAtFocusTarget();
                    return;
                }

                if (!isDragging && (hit.transform == transform || hit.transform.IsChildOf(transform)))
                {
                    StartDragging(inputPosition.Value);
                    StopLookAt();
                }
            }
            else
            {
                debugHit = "Nichts getroffen";
            }
        }

        if (isDragging)
        {
            Vector2 currentPos = mouse != null ? mouse.position.ReadValue() : touchscreen.primaryTouch.position.ReadValue();
            UpdateDragging(currentPos);
        }

        if ((mouse != null && mouse.leftButton.wasReleasedThisFrame) ||
            (touchscreen != null && touchscreen.primaryTouch.press.wasReleasedThisFrame))
        {
            StopDragging();
        }
    }

    void StartDragging(Vector2 inputPosition)
    {
        isDragging = true;
        lastInputPosition = inputPosition;
        currentVelocity = Vector3.zero;
    }

    void UpdateDragging(Vector2 inputPosition)
    {
        Vector2 delta = inputPosition - lastInputPosition;
        lastInputPosition = inputPosition;

        if (delta == Vector2.zero)
            return;

        float direction = invertDirection ? -1f : 1f;

        Vector3 dragAxis = new Vector3(-delta.y, delta.x, 0f).normalized;
        float dragMagnitude = delta.magnitude * rotationSpeed;

        Quaternion rotation = Quaternion.AngleAxis(dragMagnitude * direction, mainCamera.transform.TransformDirection(dragAxis));
        rotationTarget.rotation = rotation * rotationTarget.rotation;

        currentVelocity = mainCamera.transform.TransformDirection(dragAxis * dragMagnitude / Time.deltaTime);
    }

    void StopDragging()
    {
        isDragging = false;
    }

    void StartLookAtFocusTarget()
    {
        if (lookAtCoroutine != null)
            StopCoroutine(lookAtCoroutine);

        lookAtCoroutine = StartCoroutine(SmoothLookAtTarget());
    }

    void StopLookAt()
    {
        if (lookAtCoroutine != null)
        {
            StopCoroutine(lookAtCoroutine);
            lookAtCoroutine = null;
        }
    }

    IEnumerator SmoothLookAtTarget()
    {
        isDragging = false;
        currentVelocity = Vector3.zero;

        Vector3 startDirection = focusTarget.position - rotationTarget.position;
        Vector3 targetDirection = mainCamera.transform.position - rotationTarget.position;

        Quaternion initialRotation = rotationTarget.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(startDirection, targetDirection) * rotationTarget.rotation;

        float t = 0f;
        debugLookAtStatus = "Drehe mit EaseOut...";

        while (t < 1f)
        {
            if (isDragging)
            {
                debugLookAtStatus = "Abgebrochen durch Drag";
                yield break;
            }

            t += Time.deltaTime * lookAtSpeed;

            // Cubic Ease-Out
            float easedT = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);

            rotationTarget.rotation = Quaternion.Slerp(initialRotation, targetRotation, easedT);
            yield return null;
        }

        rotationTarget.rotation = targetRotation;
        lookAtCoroutine = null;
        debugLookAtStatus = "Abgeschlossen";
    }

    void OnValidate()
    {
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        minVelocity = Mathf.Max(0f, minVelocity);
        lookAtSpeed = Mathf.Max(0.01f, lookAtSpeed);
    }

    
}
