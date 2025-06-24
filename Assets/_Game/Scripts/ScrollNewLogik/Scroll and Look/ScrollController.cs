using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class ScrollController : MonoBehaviour
{
    [Header("Kamera & Bewegung")]
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;

    [Header("Yaw & Look")]
    public float lookSpeed = 0.1f;
    public float rotationDamping = 0.1f;

    [Header("Geschwindigkeitsprofil")]
    public ScrollSpeedManager speedManager;

    [Header("Zoom (echte Kamera)")]
    public float zoomedFOV = 30f;
    public float zoomDuration = 1.5f;
    public float zoomSpeed = 5f;

    [Header("Zoom-Einstellungen")]
    [Tooltip("Maximales Zeitintervall (Sekunden) zwischen zwei Klicks/Taps für Zoom")]
    public float doubleClickThreshold = 0.3f;

    [Header("Eingabeoptionen")]
    public bool invertScroll = false;

    [Header("Spline-Ende blockieren")]
    [Tooltip("Verhindert das Drehen am letzten Spline-Knoten.")]
    public float blockEndOffset = 0.5f;

    [Header("GUI-Debug")]
    public bool showDebugGUI = true;

    [Header("Steuerung aktivieren")]
    [Tooltip("Aktiviert die Steuerungselemente (Umschauen, Zoomen, Touch-Steuerung)")]
    public bool enableLookControl = true;
    public bool enableZoomControl = true;

    [Header("Touchsteuerung")]
    [Tooltip("Anzahl der Finger zum Scrollen auf dem Touchscreen")]
    public int touchScrollFingerCount = 5;
    [Tooltip("Anzahl der Finger zum Umschauen auf dem Touchscreen")]
    public int touchLookFingerCount = 1;

    [Tooltip("Anzahl der Taps für Zoom")]
    public int zoomTapCount = 2;

    public float virtualDistance { get; private set; }
    public event Action<float> OnDistanceChanged;

    private CinemachineSplineDolly splineDolly;
    private float scrollVelocity;
    private float currentDistance;

    private YawOverrideExtension yawExtension;
    private float yawAmount;

    private float defaultFOV;
    private bool isZooming;
    private Coroutine zoomCoroutine;

    private Vector2 previousScrollPos;
    private Vector2 previousLookPos;
    private bool isScrolling;
    private bool isLooking;

    private CinemachineBasicMultiChannelPerlin shakePerlin;

    private float lastMouseClickTime = 0f;
    private Camera mainCam;

    private float virtualScrollBuffer = 0f;
    private bool isClamped = false;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        currentDistance = splineDolly.CameraPosition;
        virtualDistance = currentDistance;

        yawExtension = cineCam.GetComponent<YawOverrideExtension>();
        if (yawExtension == null)
            yawExtension = cineCam.gameObject.AddComponent<YawOverrideExtension>();

        shakePerlin = cineCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

        mainCam = Camera.main;
        if (mainCam != null)
            defaultFOV = mainCam.fieldOfView;
        else
            Debug.LogWarning("Keine Kamera mit Tag MainCamera gefunden!");
    }

    void Update()
    {
        HandleInput();

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        float usableMaxDistance = Mathf.Max(0f, maxDistance - blockEndOffset);

        float speedMultiplier = speedManager != null ? speedManager.GetSpeedMultiplier(currentDistance) : 1f;
        float effectiveVelocity = scrollVelocity * speedMultiplier;

        currentDistance = Mathf.Clamp(currentDistance + effectiveVelocity * Time.deltaTime, 0, usableMaxDistance);
        splineDolly.CameraPosition = currentDistance;

        if (!isClamped)
            virtualScrollBuffer += effectiveVelocity * Time.deltaTime;

        virtualDistance = Mathf.Round((currentDistance + virtualScrollBuffer) * 100f) / 100f;

        // Nur clampen, wenn Richtung nach außen geht
        if ((currentDistance >= usableMaxDistance && scrollVelocity > 0f) ||
            (currentDistance <= 0f && scrollVelocity < 0f))
        {
            isClamped = true;
            scrollVelocity = 0f;
        }
        else
        {
            isClamped = false;
        }

        OnDistanceChanged?.Invoke(virtualDistance);

        float damping = speedManager != null ? speedManager.damping : 0.9f;
        float dampingFactor = Mathf.Pow(damping, Time.deltaTime * 60f);
        scrollVelocity *= dampingFactor;
        if (Mathf.Abs(scrollVelocity) < 0.001f)
            scrollVelocity = 0f;

        if (!isLooking)
            yawAmount = Mathf.Lerp(yawAmount, 0f, Time.deltaTime * 5f);

        yawAmount = Mathf.Clamp(yawAmount, yawExtension.minYawLimit, yawExtension.maxYawLimit);
        yawExtension.yawOverride = yawAmount;

        if (shakePerlin != null)
            shakePerlin.AmplitudeGain = (isScrolling || isLooking) ? 0f : 1f;
    }

    void HandleInput()
    {
        bool leftPressed = Mouse.current?.leftButton.isPressed ?? false;
        bool rightPressed = Mouse.current?.rightButton.isPressed ?? false;
        bool touched = Touchscreen.current?.primaryTouch.press.isPressed ?? false;
        int touchCount = Touchscreen.current?.touches.Count ?? 0;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time - lastMouseClickTime < doubleClickThreshold && enableZoomControl)
            {
                TriggerZoom();
            }
            lastMouseClickTime = Time.time;
        }

        if (Touchscreen.current != null && touchCount >= zoomTapCount && enableZoomControl)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.tapCount.ReadValue() >= zoomTapCount)
            {
                TriggerZoom();
            }
        }

#if UNITY_EDITOR
        if (leftPressed && !rightPressed && enableLookControl)
#else
        if (touchCount == touchLookFingerCount && enableLookControl)
#endif
        {
            Vector2 currentPos = leftPressed
                ? Mouse.current.position.ReadValue()
                : Touchscreen.current.primaryTouch.position.ReadValue();

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
        else isScrolling = false;

#if UNITY_EDITOR
        if (rightPressed && enableLookControl)
#else
        if (touchCount == touchLookFingerCount && enableLookControl)
#endif
        {
            Vector2 lookPos = rightPressed
                ? Mouse.current.position.ReadValue()
                : Touchscreen.current.primaryTouch.position.ReadValue();

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
        else isLooking = false;
    }

    void ApplyScroll(float deltaY)
    {
        float direction = invertScroll ? -1f : 1f;
        scrollVelocity += deltaY * scrollSpeedMetersPerPixel * direction;
    }

    void ApplyLook(Vector2 delta)
    {
        float rawYawDelta = delta.x * lookSpeed;
        float nonLinearDamping = Mathf.Pow(Mathf.Abs(yawAmount), 1.2f);
        float dampFactor = 1f / (1f + nonLinearDamping * rotationDamping);
        float adjustedYawDelta = rawYawDelta * dampFactor;
        yawAmount += adjustedYawDelta;
    }

    void TriggerZoom()
    {
        if (!enableZoomControl || isZooming || mainCam == null) return;

        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomRoutine());
    }

    IEnumerator ZoomRoutine()
    {
        isZooming = true;

        // Zoom hinein
        while (Mathf.Abs(mainCam.fieldOfView - zoomedFOV) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, zoomedFOV, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(zoomDuration);

        // Zoom heraus
        while (Mathf.Abs(mainCam.fieldOfView - defaultFOV) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        mainCam.fieldOfView = defaultFOV;
        isZooming = false;
    }

    void OnGUI()
    {
#if UNITY_EDITOR
        if (!showDebugGUI) return;

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 30,
            normal = { textColor = Color.white }
        };

        float y = 10f;
        float line = 35f;

        GUI.Label(new Rect(10, y + 0 * line, 1000, line), $"Scroll Velocity: {scrollVelocity:F2}", style);
        GUI.Label(new Rect(10, y + 1 * line, 1000, line), $"Actual Distance: {currentDistance:F2}", style);
        GUI.Label(new Rect(10, y + 2 * line, 1000, line), $"Virtual Distance: {virtualDistance:F2}", style);
        GUI.Label(new Rect(10, y + 3 * line, 1000, line), $"Yaw: {yawExtension?.yawOverride:F2}", style);
        GUI.Label(new Rect(10, y + 4 * line, 1000, line), $"Zooming: {isZooming}", style);
        GUI.Label(new Rect(10, y + 5 * line, 1000, line), $"Touches: {(Touchscreen.current?.touches.Count ?? 0)}", style);
        GUI.Label(new Rect(10, y + 6 * line, 1000, line), $"Clamped: {isClamped}", style);
        GUI.Label(new Rect(10, y + 7 * line, 1000, line), $"Diff: {(virtualDistance - currentDistance):F2}", style);
#endif
    }
}
