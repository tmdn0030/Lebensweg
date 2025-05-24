using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;
    public float smoothTime = 0.2f;

    private CinemachineSplineDolly splineDolly;
    private float targetDistance;
    private float currentDistance;
    private float velocity = 0f;

    private Vector2 previousInputPos;
    private bool isInteracting;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();

        targetDistance = currentDistance = splineDolly.CameraPosition;
    }

    void Update()
    {
        HandleInput();

        // Smooth Bewegung
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref velocity, smoothTime);

        // Begrenzung auf Länge des Splines
        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        currentDistance = Mathf.Clamp(currentDistance, 0, maxDistance);

        splineDolly.CameraPosition = currentDistance;
    }

    void HandleInput()
    {
        bool touched = false;
        Vector2 currentInputPos = Vector2.zero;

        // TOUCH
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            currentInputPos = Touchscreen.current.primaryTouch.position.ReadValue();
            touched = true;
        }
        // MOUSE
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            currentInputPos = Mouse.current.position.ReadValue();
            touched = true;
        }

        if (touched)
        {
            if (!isInteracting)
            {
                previousInputPos = currentInputPos;
                isInteracting = true;
            }
            else
            {
                float deltaY = currentInputPos.y - previousInputPos.y;
                ApplyScroll(deltaY);
                previousInputPos = currentInputPos;
            }
        }
        else
        {
            isInteracting = false;
        }
    }

    void ApplyScroll(float deltaY)
    {
        if (Mathf.Abs(deltaY) < 0.01f) return;

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        targetDistance += deltaY * scrollSpeedMetersPerPixel;
        targetDistance = Mathf.Clamp(targetDistance, 0, maxDistance);
    }

}









/*
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeed = 0.001f;
    public float smoothTime = 0.2f; // Wie weich/träge die Bewegung ist

    private CinemachineSplineDolly splineDolly;
    private float targetPosition;
    private float currentPosition;
    private float velocity = 0f;

    private Vector2 previousInputPos;
    private bool isInteracting;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        targetPosition = currentPosition = splineDolly.CameraPosition;
    }

    void Update()
    {
        HandleInput();

        // SMOOTH DAMP
        currentPosition = Mathf.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);
        splineDolly.CameraPosition = currentPosition;
    }

    void HandleInput()
    {
        bool touched = false;
        Vector2 currentInputPos = Vector2.zero;

        // TOUCH
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            currentInputPos = Touchscreen.current.primaryTouch.position.ReadValue();
            touched = true;
        }
        // MOUSE
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            currentInputPos = Mouse.current.position.ReadValue();
            touched = true;
        }

        if (touched)
        {
            if (!isInteracting)
            {
                previousInputPos = currentInputPos;
                isInteracting = true;
            }
            else
            {
                float deltaY = currentInputPos.y - previousInputPos.y;
                ApplyScroll(deltaY);
                previousInputPos = currentInputPos;
            }
        }
        else
        {
            isInteracting = false;
        }
    }

    void ApplyScroll(float deltaY)
    {
        if (Mathf.Abs(deltaY) < 0.01f) return;

        targetPosition += deltaY * scrollSpeed;
        targetPosition = Mathf.Clamp01(targetPosition);
    }
}



*/