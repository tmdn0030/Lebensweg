/*
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class MovementController : MonoBehaviour
{
    [Header("Kamera & Bewegung")]
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;

    [Header("Yaw & Look")]
    public float lookSpeed = 0.1f;
    public float rotationDamping = 0.1f;

    [Header("Zoom (echte Kamera)")]
    public float zoomedFOV = 30f;
    public float zoomDuration = 1.5f;
    public float zoomSpeed = 5f;

    [Header("Zoom-Einstellungen")]
    public float doubleClickThreshold = 0.3f;

    [Header("Eingabeoptionen")]
    public bool invertScroll = false;

    [Header("Spline-Ende blockieren")]
    public float blockEndOffset = 0.5f;

    [Header("GUI-Debug")]
    public bool showDebugGUI = true;

    [Header("SpeedZones (werden automatisch gefunden, falls leer)")]
    public SpeedZone[] speedZones;

    [Header("AnimationZones (werden automatisch gefunden, falls leer)")]
    public AnimationZone[] animationZones;

    [Header("TextZones (werden automatisch gefunden, falls leer)")]
    public TextZone[] textZones;

    private CinemachineSplineDolly splineDolly;
    private float scrollVelocity;
    private float currentDistance;

    private YawOverrideExtension yawExtension;
    private float yawAmount;

    private float defaultFOV;
    private bool isZooming;
    private Coroutine zoomCoroutine;
    private Coroutine scrollSpeedCoroutine;

    private Vector2 previousScrollPos;
    private Vector2 previousLookPos;
    private bool isScrolling;
    private bool isLooking;

    private CinemachineBasicMultiChannelPerlin shakePerlin;

    private float lastMouseClickTime = 0f;
    private Camera mainCam;

    private bool isClamped = false;
    private SpeedZone activeZone;
    private float initialScrollSpeed;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        currentDistance = splineDolly.CameraPosition;

        yawExtension = cineCam.GetComponent<YawOverrideExtension>();
        if (yawExtension == null)
            yawExtension = cineCam.gameObject.AddComponent<YawOverrideExtension>();

        shakePerlin = cineCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

        mainCam = Camera.main;
        if (mainCam != null)
            defaultFOV = mainCam.fieldOfView;
        else
            Debug.LogWarning("Keine Kamera mit Tag MainCamera gefunden!");

        initialScrollSpeed = scrollSpeedMetersPerPixel;

        if (speedZones == null || speedZones.Length == 0)
            speedZones = Object.FindObjectsByType<SpeedZone>(FindObjectsSortMode.None);

        if (animationZones == null || animationZones.Length == 0)
            animationZones = Object.FindObjectsByType<AnimationZone>(FindObjectsSortMode.None);

        if (textZones == null || textZones.Length == 0)
            textZones = Object.FindObjectsByType<TextZone>(FindObjectsSortMode.None);
    }

    void Update()
    {
        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        float usableMaxDistance = Mathf.Max(0f, maxDistance - blockEndOffset);

        // ----- TextZones zuerst prüfen! -----
        if (HandleTextZones()) return;

        HandleInput();

        float effectiveVelocity = scrollVelocity;
        currentDistance = Mathf.Clamp(currentDistance + effectiveVelocity * Time.deltaTime, 0, usableMaxDistance);
        splineDolly.CameraPosition = currentDistance;

        // Clamp-Logik wie gehabt
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

        float damping = 0.9f;
        float dampingFactor = Mathf.Pow(damping, Time.deltaTime * 60f);
        scrollVelocity *= dampingFactor;
        if (Mathf.Abs(scrollVelocity) < 0.001f)
            scrollVelocity = 0f;

        if (!isLooking)
            yawAmount = Mathf.Lerp(yawAmount, 0f, Time.deltaTime * 5f);

        if (yawExtension != null)
        {
            yawAmount = Mathf.Clamp(yawAmount, yawExtension.minYawLimit, yawExtension.maxYawLimit);
            yawExtension.yawOverride = yawAmount;
        }

        if (shakePerlin != null)
            shakePerlin.AmplitudeGain = (isScrolling || isLooking) ? 0f : 1f;

        HandleSpeedZones(maxDistance);
        HandleAnimationZones();
    }

    // ---- TEXT ZONES: Drag-Y-Scrolling für Seitenwechsel ----
    private bool HandleTextZones()
    {
        bool anyTextActive = false;
        foreach (var zone in textZones)
        {
            if (zone == null || zone.spline != splineDolly.Spline) continue;
            bool isActive = zone.CheckAndTrigger(currentDistance, mainCam);

            // Drag-Y-Scroll-Input (wie Bewegung):
            if (isActive)
            {
                float deltaY = 0f;
                bool dragging = false;
                if (Mouse.current != null && Mouse.current.leftButton.isPressed)
                {
                    Vector2 currentPos = Mouse.current.position.ReadValue();
                    if (!zone.isDragging)
                    {
                        zone.lastY = currentPos.y;
                        zone.isDragging = true;
                    }
                    else
                    {
                        deltaY = currentPos.y - zone.lastY;
                        zone.lastY = currentPos.y;
                        if (Mathf.Abs(deltaY) > 1f)
                            dragging = true;
                    }
                }
                else
                {
                    zone.isDragging = false;
                }
                if (dragging)
                    zone.HandleDragScroll(deltaY, mainCam);

                scrollVelocity = 0f; // Movement deaktiviert während Dialog!
            }
            anyTextActive |= isActive;
        }
        return anyTextActive;
    }

    //---- SPEED ZONES -----
    private void HandleSpeedZones(float totalLength)
    {
        SpeedZone matchingZone = null;
        foreach (var zone in speedZones)
        {
            if (zone == null || zone.spline != splineDolly.Spline) continue;
            float start = Mathf.Clamp(zone.startDistance, 0, totalLength);
            float end = Mathf.Clamp(zone.endDistance, start, totalLength);

            if (currentDistance >= start && currentDistance <= end)
            {
                matchingZone = zone;
                break;
            }
        }

        if (matchingZone != null && matchingZone != activeZone)
        {
            if (scrollSpeedCoroutine != null) StopCoroutine(scrollSpeedCoroutine);
            scrollSpeedCoroutine = StartCoroutine(ChangeScrollSpeedSmoothly(
                matchingZone.newScrollSpeed, matchingZone.easeDuration, matchingZone.ease));
            activeZone = matchingZone;
        }
        else if (matchingZone == null && activeZone != null && activeZone.resetOnExit)
        {
            if (scrollSpeedCoroutine != null) StopCoroutine(scrollSpeedCoroutine);
            float resetTo = activeZone.defaultSpeed > 0 ? activeZone.defaultSpeed : initialScrollSpeed;
            scrollSpeedCoroutine = StartCoroutine(ChangeScrollSpeedSmoothly(
                resetTo, activeZone.resetEaseDuration, activeZone.resetEase));
            activeZone = null;
        }
    }

    // --- ANIMATIONZONEN (scrubben der Animationen nach Position) ---
    private void HandleAnimationZones()
    {
        if (animationZones == null) return;
        foreach (var zone in animationZones)
        {
            if (zone == null) continue;
            zone.ScrubAnimation(currentDistance);
        }
    }

    // NUR ScrollSpeed sanft ändern!
    private IEnumerator ChangeScrollSpeedSmoothly(float newSpeed, float duration, SpeedZone.EaseType ease)
    {
        float startSpeed = scrollSpeedMetersPerPixel;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            float easedT = Ease(t, ease);
            scrollSpeedMetersPerPixel = Mathf.Lerp(startSpeed, newSpeed, easedT);
            yield return null;
        }
        scrollSpeedMetersPerPixel = newSpeed;
    }

    private float Ease(float t, SpeedZone.EaseType ease)
    {
        switch (ease)
        {
            case SpeedZone.EaseType.Linear: return t;
            case SpeedZone.EaseType.SmoothStep: return Mathf.SmoothStep(0f, 1f, t);
            case SpeedZone.EaseType.EaseIn: return t * t;
            case SpeedZone.EaseType.EaseOut: return t * (2 - t);
            case SpeedZone.EaseType.EaseInOut: return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
            case SpeedZone.EaseType.SineWave: return 0.5f - 0.5f * Mathf.Cos(Mathf.PI * t);
            default: return t;
        }
    }

    // ---- USER INPUT (SCROLL & LOOK, wie gehabt) ----
    void HandleInput()
    {
        bool leftPressed = Mouse.current?.leftButton.isPressed ?? false;
        bool rightPressed = Mouse.current?.rightButton.isPressed ?? false;
        int touchCount = Touchscreen.current?.touches.Count ?? 0;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time - lastMouseClickTime < doubleClickThreshold)
            {
                TriggerZoom();
            }
            lastMouseClickTime = Time.time;
        }

    #if UNITY_EDITOR
        if (leftPressed && !rightPressed)
    #else
        if (touchCount == 5)
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
        if (rightPressed)
    #else
        if (touchCount == 1)
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
        if (isZooming || mainCam == null) return;

        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomRoutine());
    }

    IEnumerator ZoomRoutine()
    {
        isZooming = true;

        while (Mathf.Abs(mainCam.fieldOfView - zoomedFOV) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, zoomedFOV, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(zoomDuration);

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
        GUI.Label(new Rect(10, y + 1 * line, 1000, line), $"Scroll Speed: {scrollSpeedMetersPerPixel:F4}", style);
        GUI.Label(new Rect(10, y + 2 * line, 1000, line), $"Actual Distance: {currentDistance:F2}", style);
        GUI.Label(new Rect(10, y + 3 * line, 1000, line), $"Yaw: {yawExtension?.yawOverride:F2}", style);
        GUI.Label(new Rect(10, y + 4 * line, 1000, line), $"Zooming: {isZooming}", style);
        GUI.Label(new Rect(10, y + 5 * line, 1000, line), $"Touches: {(Touchscreen.current?.touches.Count ?? 0)}", style);
        GUI.Label(new Rect(10, y + 6 * line, 1000, line), $"Clamped: {isClamped}", style);
        GUI.Label(new Rect(10, y + 7 * line, 1000, line), $"SpeedZone: {(activeZone ? activeZone.name : "keine")}", style);
    #endif
    }
}
*/

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class MovementController : MonoBehaviour
{
    [Header("Kamera & Bewegung")]
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;

    [Header("Yaw & Look")]
    public float lookSpeed = 0.1f;
    public float rotationDamping = 0.1f;

    [Header("Eingabeoptionen")]
    public bool invertScroll = false;

    [Header("Spline-Ende blockieren")]
    public float blockEndOffset = 0.5f;

    [Header("GUI-Debug")]
    public bool showDebugGUI = true;

    [Header("SpeedZones (werden automatisch gefunden, falls leer)")]
    public SpeedZone[] speedZones;

    [Header("AnimationZones (werden automatisch gefunden, falls leer)")]
    public AnimationZone[] animationZones;

    [Header("Touch-Einstellungen")]
    [Range(1,5)]
    public int fingersToScroll = 2;

    private CinemachineSplineDolly splineDolly;
    private float scrollVelocity;
    private float currentDistance;

    private YawOverrideExtension yawExtension;
    private float yawAmount;

    private Coroutine scrollSpeedCoroutine;

    private Vector2 previousScrollPos;
    private Vector2 previousLookPos;
    private bool isScrolling;
    private bool isLooking;

    private CinemachineBasicMultiChannelPerlin shakePerlin;

    private bool isClamped = false;
    private SpeedZone activeZone;
    private float initialScrollSpeed;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        currentDistance = splineDolly.CameraPosition;

        yawExtension = cineCam.GetComponent<YawOverrideExtension>();
        if (yawExtension == null)
            yawExtension = cineCam.gameObject.AddComponent<YawOverrideExtension>();

        shakePerlin = cineCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

        initialScrollSpeed = scrollSpeedMetersPerPixel;

        if (speedZones == null || speedZones.Length == 0)
            speedZones = Object.FindObjectsByType<SpeedZone>(FindObjectsSortMode.None);

        if (animationZones == null || animationZones.Length == 0)
            animationZones = Object.FindObjectsByType<AnimationZone>(FindObjectsSortMode.None);

        fingersToScroll = Mathf.Clamp(fingersToScroll, 1, 5);
    }

    void Update()
    {
        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        float usableMaxDistance = Mathf.Max(0f, maxDistance - blockEndOffset);

        HandleInput();

        float effectiveVelocity = scrollVelocity;
        currentDistance = Mathf.Clamp(currentDistance + effectiveVelocity * Time.deltaTime, 0, usableMaxDistance);
        splineDolly.CameraPosition = currentDistance;

        // Clamp-Logik
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

        // Dämpfung
        float damping = 0.9f;
        float dampingFactor = Mathf.Pow(damping, Time.deltaTime * 60f);
        scrollVelocity *= dampingFactor;
        if (Mathf.Abs(scrollVelocity) < 0.001f)
            scrollVelocity = 0f;

        if (!isLooking)
            yawAmount = Mathf.Lerp(yawAmount, 0f, Time.deltaTime * 5f);

        if (yawExtension != null)
        {
            yawAmount = Mathf.Clamp(yawAmount, yawExtension.minYawLimit, yawExtension.maxYawLimit);
            yawExtension.yawOverride = yawAmount;
        }

        if (shakePerlin != null)
            shakePerlin.AmplitudeGain = (isScrolling || isLooking) ? 0f : 1f;

        HandleSpeedZones(maxDistance);
        HandleAnimationZones();
    }

    //---- SPEED ZONES -----
    private void HandleSpeedZones(float totalLength)
    {
        SpeedZone matchingZone = null;
        foreach (var zone in speedZones)
        {
            if (zone == null || zone.spline != splineDolly.Spline) continue;
            float start = Mathf.Clamp(zone.startDistance, 0, totalLength);
            float end = Mathf.Clamp(zone.endDistance, start, totalLength);

            if (currentDistance >= start && currentDistance <= end)
            {
                matchingZone = zone;
                break;
            }
        }

        if (matchingZone != null && matchingZone != activeZone)
        {
            if (scrollSpeedCoroutine != null) StopCoroutine(scrollSpeedCoroutine);
            scrollSpeedCoroutine = StartCoroutine(ChangeScrollSpeedSmoothly(
                matchingZone.newScrollSpeed, matchingZone.easeDuration, matchingZone.ease));
            activeZone = matchingZone;
        }
        else if (matchingZone == null && activeZone != null && activeZone.resetOnExit)
        {
            if (scrollSpeedCoroutine != null) StopCoroutine(scrollSpeedCoroutine);
            float resetTo = activeZone.defaultSpeed > 0 ? activeZone.defaultSpeed : initialScrollSpeed;
            scrollSpeedCoroutine = StartCoroutine(ChangeScrollSpeedSmoothly(
                resetTo, activeZone.resetEaseDuration, activeZone.resetEase));
            activeZone = null;
        }
    }

    // --- ANIMATIONZONEN (scrubben der Animationen nach Position) ---
    private void HandleAnimationZones()
    {
        if (animationZones == null) return;
        foreach (var zone in animationZones)
        {
            if (zone == null) continue;
            zone.ScrubAnimation(currentDistance);
        }
    }

    // NUR ScrollSpeed sanft ändern!
    private IEnumerator ChangeScrollSpeedSmoothly(float newSpeed, float duration, SpeedZone.EaseType ease)
    {
        float startSpeed = scrollSpeedMetersPerPixel;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            float easedT = Ease(t, ease);
            scrollSpeedMetersPerPixel = Mathf.Lerp(startSpeed, newSpeed, easedT);
            yield return null;
        }
        scrollSpeedMetersPerPixel = newSpeed;
    }

    private float Ease(float t, SpeedZone.EaseType ease)
    {
        switch (ease)
        {
            case SpeedZone.EaseType.Linear: return t;
            case SpeedZone.EaseType.SmoothStep: return Mathf.SmoothStep(0f, 1f, t);
            case SpeedZone.EaseType.EaseIn: return t * t;
            case SpeedZone.EaseType.EaseOut: return t * (2 - t);
            case SpeedZone.EaseType.EaseInOut: return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
            case SpeedZone.EaseType.SineWave: return 0.5f - 0.5f * Mathf.Cos(Mathf.PI * t);
            default: return t;
        }
    }

    // ---- USER INPUT (SCROLL & LOOK) ----
    void HandleInput()
    {
        bool leftPressed = Mouse.current?.leftButton.isPressed ?? false;
        bool rightPressed = Mouse.current?.rightButton.isPressed ?? false;
        int touchCount = Touchscreen.current?.touches.Count ?? 0;

        // --- SCROLL ---
#if UNITY_EDITOR
        if (leftPressed && !rightPressed)
#else
        if (touchCount == fingersToScroll)
#endif
        {
            Vector2 currentPos =
#if UNITY_EDITOR
                Mouse.current.position.ReadValue();
#else
                Touchscreen.current.primaryTouch.position.ReadValue();
#endif
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

        // --- LOOK (Yaw beim Gedrückthalten & Draggen) ---
#if UNITY_EDITOR
        if (rightPressed)
#else
        if (touchCount == 1 && fingersToScroll != 1)
#endif
        {
            Vector2 lookPos =
#if UNITY_EDITOR
                Mouse.current.position.ReadValue();
#else
                Touchscreen.current.primaryTouch.position.ReadValue();
#endif
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
        GUI.Label(new Rect(10, y + 1 * line, 1000, line), $"Scroll Speed: {scrollSpeedMetersPerPixel:F4}", style);
        GUI.Label(new Rect(10, y + 2 * line, 1000, line), $"Actual Distance: {currentDistance:F2}", style);
        GUI.Label(new Rect(10, y + 3 * line, 1000, line), $"Yaw: {yawExtension?.yawOverride:F2}", style);
        GUI.Label(new Rect(10, y + 4 * line, 1000, line), $"Touches: {(Touchscreen.current?.touches.Count ?? 0)}", style);
        GUI.Label(new Rect(10, y + 5 * line, 1000, line), $"Clamped: {isClamped}", style);
        GUI.Label(new Rect(10, y + 6 * line, 1000, line), $"SpeedZone: {(activeZone ? activeZone.name : "keine")}", style);
#endif
    }
}


