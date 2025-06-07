using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller2 : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;
    public float smoothTime = 0.2f;
    public float lookSpeed = 0.1f;

    [Tooltip("Je höher, desto stärker wird die Rotation gedämpft (Hyperbel-Dämpfung).")]
    public float rotationDamping = 0.1f;

    private CinemachineSplineDolly splineDolly;
    private float targetDistance;
    private float currentDistance;
    private float velocity = 0f;

    private Vector2 previousScrollPos;
    private Vector2 previousLookPos;

    private bool isScrolling;
    private bool isLooking;

    private YawOverrideExtension yawExtension;
    private float yawAmount = 0f;

    private CinemachineBasicMultiChannelPerlin shakePerlin;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        targetDistance = currentDistance = splineDolly.CameraPosition;

        yawExtension = cineCam.GetComponent<YawOverrideExtension>();
        if (yawExtension == null)
        {
            yawExtension = cineCam.gameObject.AddComponent<YawOverrideExtension>();
        }

        shakePerlin = cineCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
    }

 void Update()
{
    HandleInput();

    if (shakePerlin != null)
    {
        // Shake nur wenn keine Eingabe aktiv ist
        shakePerlin.AmplitudeGain = (isScrolling || isLooking) ? 0f : 1f;
    }

    currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref velocity, smoothTime);
    float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
    currentDistance = Mathf.Clamp(currentDistance, 0, maxDistance);
    splineDolly.CameraPosition = currentDistance;
}


    void HandleInput()
    {
        bool leftPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool rightPressed = Mouse.current != null && Mouse.current.rightButton.isPressed;
        bool touched = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        int touchCount = Touchscreen.current != null ? Touchscreen.current.touches.Count : 0;

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
            isLooking = false;
        }

        if (!isLooking)
        {
            yawAmount = Mathf.Lerp(yawAmount, 0f, Time.deltaTime * 5f);
        }

        if (yawExtension != null)
        {
            yawExtension.yawOverride = yawAmount;
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
        float rawYawDelta = delta.x * lookSpeed;

        // Hyperbelartige Dämpfung, jetzt mit einstellbarem Dämpfungsfaktor
        float dampFactor = 1f / (1f + Mathf.Abs(yawAmount) * rotationDamping);
        float adjustedYawDelta = rawYawDelta * dampFactor;

        yawAmount += adjustedYawDelta;

        if (yawExtension != null)
        {
            yawAmount = Mathf.Clamp(yawAmount, yawExtension.minYawLimit, yawExtension.maxYawLimit);
        }
    }
}
