using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller2 : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;
    public float smoothTime = 0.2f;
    public float lookSpeed = 0.1f;

    private CinemachineSplineDolly splineDolly;
    private float targetDistance;
    private float currentDistance;
    private float velocity = 0f;

    private Vector2 previousScrollPos;
    private Vector2 previousLookPos;

    private bool isScrolling;
    private bool isLooking;

    private Quaternion targetRotation;
    private Quaternion startRotation;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();

        targetDistance = currentDistance = splineDolly.CameraPosition;

        targetRotation = startRotation = cineCam.transform.rotation;
    }

    void Update()
    {
        HandleInput();

        // Smooth Dolly-Position
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref velocity, smoothTime);
        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        currentDistance = Mathf.Clamp(currentDistance, 0, maxDistance);
        splineDolly.CameraPosition = currentDistance;
    }

    void LateUpdate()
    {
        // Wende Rotation nach Cinemachine an
        cineCam.transform.rotation = Quaternion.Slerp(cineCam.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    void HandleInput()
    {
        bool leftPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool rightPressed = Mouse.current != null && Mouse.current.rightButton.isPressed;
        bool touched = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        int touchCount = Touchscreen.current != null ? Touchscreen.current.touches.Count : 0;

        // Scrollen
        if ((leftPressed || (touched && touchCount == 1)) && !rightPressed)
        {
            Vector2 currentPos = leftPressed ? Mouse.current.position.ReadValue() : Touchscreen.current.primaryTouch.position.ReadValue();

            if (!isScrolling)
            {
                previousScrollPos = currentPos;
                isScrolling = true;
            }
            else
            {
                float deltaY = currentPos.y - previousScrollPos.y;
                ApplyScroll(deltaY);
                previousScrollPos = currentPos;
            }
        }
        else
        {
            isScrolling = false;
        }

        // Umschauen
        if (rightPressed || (touched && touchCount >= 2))
        {
            Vector2 lookPos = rightPressed
                ? Mouse.current.position.ReadValue()
                : Touchscreen.current.touches[1].position.ReadValue();

            if (!isLooking)
            {
                previousLookPos = lookPos;
                isLooking = true;
            }
            else
            {
                Vector2 delta = lookPos - previousLookPos;
                ApplyLook(delta);
                previousLookPos = lookPos;
            }
        }
        else
        {
            if (isLooking)
            {
                targetRotation = startRotation;  // Sanft zurück zur Originalrotation
            }
            isLooking = false;
        }
    }

    void ApplyScroll(float deltaY)
    {
        if (Mathf.Abs(deltaY) < 0.01f) return;

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        targetDistance += deltaY * scrollSpeedMetersPerPixel;
        targetDistance = Mathf.Clamp(targetDistance, 0, maxDistance);
    }

    void ApplyLook(Vector2 delta)
    {
        float yaw = delta.x * lookSpeed;
        float pitch = -delta.y * lookSpeed;

        // Wende Rotation im Weltkoordinatensystem an
        Quaternion lookRot = Quaternion.Euler(pitch, yaw, 0);
        targetRotation = Quaternion.Euler(0, 0, 0); // Reset Roll
        targetRotation = targetRotation * lookRot * cineCam.transform.rotation;
    }
}
