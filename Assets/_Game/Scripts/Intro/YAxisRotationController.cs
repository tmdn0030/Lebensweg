using UnityEngine;
using UnityEngine.InputSystem;

public class YAxisRotationController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float damping = 0.95f;
    [SerializeField] private float minVelocity = 0.1f;
    [SerializeField] private bool invertDirection = true;
    
    [Header("Target Settings")]
    [SerializeField] private Transform targetToRotate;

    private bool isDragging = false;
    private float currentVelocity = 0f;
    private float lastInputX = 0f;
    private Camera cam;
    
    // Input System
    private Mouse mouse;
    private Touchscreen touchscreen;

    void Start()
    {
        cam = Camera.main ?? FindFirstObjectByType<Camera>();
        
        if (targetToRotate == null)
            targetToRotate = transform;
            
        // Input System Setup
        mouse = Mouse.current;
        touchscreen = Touchscreen.current;
    }

    void Update()
    {
        HandleInput();
        
        // Momentum mit Damping
        if (!isDragging && Mathf.Abs(currentVelocity) > minVelocity)
        {
            float direction = invertDirection ? -1f : 1f;
            targetToRotate.Rotate(0, currentVelocity * direction * Time.deltaTime, 0);
            currentVelocity *= damping;
        }
        else if (!isDragging)
        {
            currentVelocity = 0f;
        }
    }

    void HandleInput()
    {
        // Maus Input
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (IsMouseOverObject(mouse.position.ReadValue()))
                    StartDragging(mouse.position.ReadValue().x);
            }
            else if (mouse.leftButton.isPressed && isDragging)
            {
                UpdateDragging(mouse.position.ReadValue().x);
            }
            else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
            {
                StopDragging();
            }
        }

        // Touch Input
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            var touch = touchscreen.primaryTouch;
            
            if (touch.press.wasPressedThisFrame)
            {
                if (IsMouseOverObject(touch.position.ReadValue()))
                    StartDragging(touch.position.ReadValue().x);
            }
            else if (isDragging)
            {
                UpdateDragging(touch.position.ReadValue().x);
            }
        }
        else if (touchscreen != null && touchscreen.primaryTouch.press.wasReleasedThisFrame && isDragging)
        {
            StopDragging();
        }
    }

    bool IsMouseOverObject(Vector2 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out RaycastHit hit) && 
               hit.collider.transform.IsChildOf(transform) || hit.collider.transform == transform;
    }

    void StartDragging(float inputX)
    {
        isDragging = true;
        lastInputX = inputX;
        currentVelocity = 0f;
    }

    void UpdateDragging(float inputX)
    {
        float deltaX = inputX - lastInputX;
        float rotationAmount = deltaX * rotationSpeed;
        float direction = invertDirection ? -1f : 1f;
        
        targetToRotate.Rotate(0, rotationAmount * direction, 0);
        currentVelocity = rotationAmount / Time.deltaTime;
        
        lastInputX = inputX;
    }

    void StopDragging()
    {
        isDragging = false;
    }

    void OnValidate()
    {
        damping = Mathf.Clamp01(damping);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        minVelocity = Mathf.Max(0f, minVelocity);
    }
}