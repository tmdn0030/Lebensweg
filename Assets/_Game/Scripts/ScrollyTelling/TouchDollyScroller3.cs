using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller3 : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;
    public float damping = 0.9f;
    public SpeedProfile speedProfile;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CinemachineSplineDolly splineDolly;
    private float currentDistance;
    private float scrollVelocity = 0f;
    public float virtualDistance = 0f;

    private Vector2 previousScrollPos;
    private bool isScrolling;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        currentDistance = splineDolly.CameraPosition;
        virtualDistance = currentDistance;
    }

    void Update()
    {
        HandleInput();

        float multiplier = GetSpeedMultiplier(currentDistance);
        float effectiveVelocity = scrollVelocity * multiplier;

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        float proposedDistance = currentDistance + effectiveVelocity * Time.deltaTime;
        currentDistance = Mathf.Clamp(proposedDistance, 0, maxDistance);
        splineDolly.CameraPosition = currentDistance;

        // ✅ Virtuelle Entfernung läuft unabhängig von realer Bewegung
        virtualDistance += scrollVelocity * Time.deltaTime;
        virtualDistance = Mathf.Clamp(virtualDistance, 0, maxDistance);
        virtualDistance = Mathf.Round(virtualDistance * 100f) / 100f;

        // Optional: harter Sync an den Endpunkten
        if (Mathf.Abs(currentDistance) < 0.01f)
            virtualDistance = 0f;
        else if (Mathf.Abs(currentDistance - maxDistance) < 0.01f)
            virtualDistance = maxDistance;

        float dampingFactor = Mathf.Pow(damping, Time.deltaTime * 60f);
        scrollVelocity *= dampingFactor;

        if (Mathf.Abs(scrollVelocity) < 0.001f)
            scrollVelocity = 0f;
    }

    void HandleInput()
    {
        bool touched = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        bool leftPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        Vector2 currentPos = leftPressed
            ? Mouse.current.position.ReadValue()
            : touched ? Touchscreen.current.primaryTouch.position.ReadValue() : Vector2.zero;

        if (leftPressed || touched)
        {
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
    }

    void ApplyScroll(float deltaY)
    {
        float deltaScroll = deltaY * scrollSpeedMetersPerPixel;
        scrollVelocity += deltaScroll;
    }

    float GetSpeedMultiplier(float distance)
    {
        if (speedProfile == null || speedProfile.speedSections.Count == 0)
            return 1f;

        var sections = speedProfile.speedSections;

        for (int i = 0; i < sections.Count - 1; i++)
        {
            var current = sections[i];
            var next = sections[i + 1];

            if (distance >= current.startDistance && distance <= next.startDistance)
            {
                float t = Mathf.InverseLerp(current.startDistance, next.startDistance, distance);
                AnimationCurve curve = speedProfile.GetEasingCurveForDistance(distance);
                t = curve.Evaluate(t);

                float lerped = Mathf.Lerp(current.speedMultiplier, next.speedMultiplier, t);
                return Mathf.Max(lerped, 0.01f);
            }
        }

        return Mathf.Max(sections[^1].speedMultiplier, 0.01f);
    }

    void OnGUI()
    {
        GUIStyle bigLabelStyle = new GUIStyle(GUI.skin.label);
        bigLabelStyle.fontSize = 48;
        bigLabelStyle.normal.textColor = Color.white;

        float lineHeight = 60f;
        float yOffset = 10f;

        GUI.Label(new Rect(10, yOffset + 0 * lineHeight, 1000, lineHeight), $"Speed Multiplier: {GetSpeedMultiplier(currentDistance):F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 1 * lineHeight, 1000, lineHeight), $"Scroll Velocity: {scrollVelocity:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 2 * lineHeight, 1000, lineHeight), $"Actual Distance: {currentDistance:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 3 * lineHeight, 1000, lineHeight), $"Virtual Distance: {virtualDistance:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 4 * lineHeight, 1000, lineHeight), $"Distance Delta: {(virtualDistance - currentDistance):F2}", bigLabelStyle);
    }
}










/*
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller3 : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;
    public float damping = 0.9f;
    public SpeedProfile speedProfile;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CinemachineSplineDolly splineDolly;
    private float currentDistance;
    private float scrollVelocity = 0f;
    public float virtualDistance = 0f;

    private Vector2 previousScrollPos;
    private bool isScrolling;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        currentDistance = splineDolly.CameraPosition;
        virtualDistance = currentDistance;
    }

    void Update()
    {
        HandleInput();

        float multiplier = GetSpeedMultiplier(currentDistance);
        float effectiveVelocity = scrollVelocity * multiplier;

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        float proposedDistance = currentDistance + effectiveVelocity * Time.deltaTime;
        currentDistance = Mathf.Clamp(proposedDistance, 0, maxDistance);
        splineDolly.CameraPosition = currentDistance;

        // ✅ Virtuelle Entfernung läuft unabhängig von realer Bewegung
        virtualDistance += scrollVelocity * Time.deltaTime;
        virtualDistance = Mathf.Clamp(virtualDistance, 0, maxDistance);
        virtualDistance = Mathf.Round(virtualDistance * 100f) / 100f;

        // Optional: harter Sync an den Endpunkten
        if (Mathf.Abs(currentDistance) < 0.01f)
            virtualDistance = 0f;
        else if (Mathf.Abs(currentDistance - maxDistance) < 0.01f)
            virtualDistance = maxDistance;

        float dampingFactor = Mathf.Pow(damping, Time.deltaTime * 60f);
        scrollVelocity *= dampingFactor;

        if (Mathf.Abs(scrollVelocity) < 0.001f)
            scrollVelocity = 0f;
    }

    void HandleInput()
    {
        bool touched = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        bool leftPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        Vector2 currentPos = leftPressed
            ? Mouse.current.position.ReadValue()
            : touched ? Touchscreen.current.primaryTouch.position.ReadValue() : Vector2.zero;

        if (leftPressed || touched)
        {
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
    }

    void ApplyScroll(float deltaY)
    {
        float deltaScroll = deltaY * scrollSpeedMetersPerPixel;
        scrollVelocity += deltaScroll;
    }

    float GetSpeedMultiplier(float distance)
    {
        if (speedProfile == null || speedProfile.speedSections.Count == 0)
            return 1f;

        var sections = speedProfile.speedSections;

        for (int i = 0; i < sections.Count - 1; i++)
        {
            var current = sections[i];
            var next = sections[i + 1];

            if (distance >= current.startDistance && distance <= next.startDistance)
            {
                float t = Mathf.InverseLerp(current.startDistance, next.startDistance, distance);
                AnimationCurve curve = speedProfile.GetEasingCurveForDistance(distance);
                t = curve.Evaluate(t);

                float lerped = Mathf.Lerp(current.speedMultiplier, next.speedMultiplier, t);
                return Mathf.Max(lerped, 0.01f);
            }
        }

        return Mathf.Max(sections[^1].speedMultiplier, 0.01f);
    }

    void OnGUI()
    {
        GUIStyle bigLabelStyle = new GUIStyle(GUI.skin.label);
        bigLabelStyle.fontSize = 48;
        bigLabelStyle.normal.textColor = Color.white;

        float lineHeight = 60f;
        float yOffset = 10f;

        GUI.Label(new Rect(10, yOffset + 0 * lineHeight, 1000, lineHeight), $"Speed Multiplier: {GetSpeedMultiplier(currentDistance):F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 1 * lineHeight, 1000, lineHeight), $"Scroll Velocity: {scrollVelocity:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 2 * lineHeight, 1000, lineHeight), $"Actual Distance: {currentDistance:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 3 * lineHeight, 1000, lineHeight), $"Virtual Distance: {virtualDistance:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 4 * lineHeight, 1000, lineHeight), $"Distance Delta: {(virtualDistance - currentDistance):F2}", bigLabelStyle);
    }
}









using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller3 : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;
    public float damping = 0.9f; // Dämpfungsfaktor (0.9 = 10% Verlust pro Frame)
    public SpeedProfile speedProfile;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Optional: Sanfte Übergänge im SpeedProfile

    private CinemachineSplineDolly splineDolly;
    private float currentDistance;
    private float previousDistance;
    private float scrollVelocity = 0f;
    public float virtualDistance = 0f;


    private Vector2 previousScrollPos;
    private bool isScrolling;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        currentDistance = splineDolly.CameraPosition;
    }

    void Update()
    {
        HandleInput();

        float multiplier = GetSpeedMultiplier(currentDistance);
        float effectiveVelocity = scrollVelocity * multiplier;

        previousDistance = currentDistance;
        currentDistance += effectiveVelocity * Time.deltaTime;

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        currentDistance = Mathf.Clamp(currentDistance, 0, maxDistance);
        splineDolly.CameraPosition = currentDistance;

        // Nur wenn sich currentDistance durch das Scrollen verändert hat, updaten wir virtualDistance
        if (!Mathf.Approximately(currentDistance, previousDistance))
        {
            virtualDistance += scrollVelocity * Time.deltaTime;

            // Optional: Clamp auch für virtualDistance
            virtualDistance = Mathf.Clamp(virtualDistance, 0, maxDistance);
        }

        float dampingFactor = Mathf.Pow(damping, Time.deltaTime * 60f);
        scrollVelocity *= dampingFactor;

        if (Mathf.Abs(scrollVelocity) < 0.001f)
            scrollVelocity = 0f;
    }



    

    void HandleInput()
    {
        bool touched = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        bool leftPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        Vector2 currentPos = leftPressed
            ? Mouse.current.position.ReadValue()
            : touched ? Touchscreen.current.primaryTouch.position.ReadValue() : Vector2.zero;

        if (leftPressed || touched)
        {
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
    }

    void ApplyScroll(float deltaY)
    {
        float deltaScroll = deltaY * scrollSpeedMetersPerPixel;
        scrollVelocity += deltaScroll;
    }





    float GetSpeedMultiplier(float distance)
    {
        if (speedProfile == null || speedProfile.speedSections.Count == 0)
            return 1f;

        var sections = speedProfile.speedSections;

        for (int i = 0; i < sections.Count - 1; i++)
        {
            var current = sections[i];
            var next = sections[i + 1];

            if (distance >= current.startDistance && distance <= next.startDistance)
            {
                float t = Mathf.InverseLerp(current.startDistance, next.startDistance, distance);

                AnimationCurve curve = speedProfile.GetEasingCurveForDistance(distance);
                t = curve.Evaluate(t);

                


                float lerped = Mathf.Lerp(current.speedMultiplier, next.speedMultiplier, t);
                return Mathf.Max(lerped, 0.01f); // Schutz vor Totstop
            }
        }

        return Mathf.Max(sections[^1].speedMultiplier, 0.01f);
    }



    void OnGUI()
    {
        GUIStyle bigLabelStyle = new GUIStyle(GUI.skin.label);
        bigLabelStyle.fontSize = 48; // 3x größer als Standard (ca. 16)
        bigLabelStyle.normal.textColor = Color.white;

        float lineHeight = 60f;
        float yOffset = 10f;

        GUI.Label(new Rect(10, yOffset + 0 * lineHeight, 1000, lineHeight), $"Speed Multiplier: {GetSpeedMultiplier(currentDistance):F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 1 * lineHeight, 1000, lineHeight), $"Scroll Velocity: {scrollVelocity:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 2 * lineHeight, 1000, lineHeight), $"Actual Distance: {currentDistance:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 3 * lineHeight, 1000, lineHeight), $"Virtual Distance: {virtualDistance:F2}", bigLabelStyle);
        GUI.Label(new Rect(10, yOffset + 4 * lineHeight, 1000, lineHeight), $"Distance Delta: {(virtualDistance - currentDistance):F2}", bigLabelStyle);
    }


}
*/







/*
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller3 : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;
    public float damping = 0.9f; // Dämpfungsfaktor (0.9 = 10% Verlust pro Frame)
    public SpeedProfile speedProfile;
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Optional: Sanfte Übergänge im SpeedProfile

    private CinemachineSplineDolly splineDolly;
    private float currentDistance;
    private float scrollVelocity = 0f;

    private Vector2 previousScrollPos;
    private bool isScrolling;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        currentDistance = splineDolly.CameraPosition;
    }

    void Update()
    {
        HandleInput();

        float multiplier = GetSpeedMultiplier(currentDistance);
        float effectiveVelocity = scrollVelocity * multiplier;

        currentDistance += effectiveVelocity * Time.deltaTime;

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        currentDistance = Mathf.Clamp(currentDistance, 0, maxDistance);
        splineDolly.CameraPosition = currentDistance;

        // Exponentielle Dämpfung – framerate-unabhängig
        float dampingFactor = Mathf.Pow(damping, Time.deltaTime * 60f);
        scrollVelocity *= dampingFactor;

        // Deadzone für sauberes Stoppen
        if (Mathf.Abs(scrollVelocity) < 0.001f)
            scrollVelocity = 0f;
    }

    void HandleInput()
    {
        bool touched = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        bool leftPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        Vector2 currentPos = leftPressed
            ? Mouse.current.position.ReadValue()
            : touched ? Touchscreen.current.primaryTouch.position.ReadValue() : Vector2.zero;

        if (leftPressed || touched)
        {
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
    }

    void ApplyScroll(float deltaY)
    {
        float deltaScroll = deltaY * scrollSpeedMetersPerPixel;
        scrollVelocity += deltaScroll;
    }

    


    
    float GetSpeedMultiplier(float distance)
    {
        if (speedProfile == null || speedProfile.speedSections.Count == 0)
            return 1f;

        var sections = speedProfile.speedSections;

        for (int i = 0; i < sections.Count - 1; i++)
        {
            var current = sections[i];
            var next = sections[i + 1];

            if (distance >= current.startDistance && distance <= next.startDistance)
            {
                float t = Mathf.InverseLerp(current.startDistance, next.startDistance, distance);
                t = easingCurve.Evaluate(t); // Kurve für weiche Übergänge
                float lerped = Mathf.Lerp(current.speedMultiplier, next.speedMultiplier, t);
                return Mathf.Max(lerped, 0.01f); // Schutz vor Totstop
            }
        }

        return Mathf.Max(sections[^1].speedMultiplier, 0.01f);
    }
    


    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 30), $"Speed Multiplier: {GetSpeedMultiplier(currentDistance):F2}");
        GUI.Label(new Rect(10, 30, 400, 30), $"Scroll Velocity: {scrollVelocity:F2}");
    }
}





// =========================
// 3. TouchDollyScroller mit SpeedProfile
// =========================

using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class TouchDollyScroller3 : MonoBehaviour
{
    public CinemachineCamera cineCam;
    public float scrollSpeedMetersPerPixel = 0.01f;
    public float smoothTime = 0.2f;
    public SpeedProfile speedProfile;

    private CinemachineSplineDolly splineDolly;
    private float targetDistance;
    private float currentDistance;
    private float velocity = 0f;

    private Vector2 previousScrollPos;
    private bool isScrolling;
    private float scrollInputPosition;

    void Start()
    {
        splineDolly = cineCam.GetComponent<CinemachineSplineDolly>();
        targetDistance = currentDistance = splineDolly.CameraPosition;
    }

    void Update()
    {
        HandleInput();

        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref velocity, smoothTime);
        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        currentDistance = Mathf.Clamp(currentDistance, 0, maxDistance);
        splineDolly.CameraPosition = currentDistance;
    }

    void HandleInput()
    {
        bool touched = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        bool leftPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        Vector2 currentPos = leftPressed
            ? Mouse.current.position.ReadValue()
            : touched ? Touchscreen.current.primaryTouch.position.ReadValue() : Vector2.zero;

        if ((leftPressed || touched))
        {
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
    }




    void ApplyScroll(float deltaY)
    {
        scrollInputPosition += deltaY * scrollSpeedMetersPerPixel;

        float inputBasedTarget = currentDistance + deltaY * scrollSpeedMetersPerPixel;
        float multiplier = GetSpeedMultiplier(inputBasedTarget);
        targetDistance += deltaY * scrollSpeedMetersPerPixel * multiplier;

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        targetDistance = Mathf.Clamp(targetDistance, 0, maxDistance);
    }

    float GetSpeedMultiplier(float distance)
    {
        if (speedProfile == null || speedProfile.speedSections.Count == 0)
            return 1f;

        var sections = speedProfile.speedSections;

        for (int i = 0; i < sections.Count - 1; i++)
        {
            var current = sections[i];
            var next = sections[i + 1];

            if (distance >= current.startDistance && distance <= next.startDistance)
            {
                float t = Mathf.InverseLerp(current.startDistance, next.startDistance, distance);
                float lerped = Mathf.Lerp(current.speedMultiplier, next.speedMultiplier, t);
                return Mathf.Max(lerped, 0.01f); // <- verhindert harte Stopps
            }
        }

        return Mathf.Max(sections[^1].speedMultiplier, 0.01f);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 30), $"Speed Multiplier: {GetSpeedMultiplier(currentDistance):F2}");
    }





    
    void ApplyScroll(float deltaY)
    {
        scrollInputPosition += deltaY * scrollSpeedMetersPerPixel;

        float multiplier = GetSpeedMultiplier(currentDistance);
        targetDistance += deltaY * scrollSpeedMetersPerPixel * multiplier;

        float maxDistance = splineDolly.Spline?.Spline?.GetLength() ?? 0f;
        targetDistance = Mathf.Clamp(targetDistance, 0, maxDistance);
    }

    float GetSpeedMultiplier(float distance)
    {
        if (speedProfile == null || speedProfile.speedSections.Count == 0)
            return 1f;

        var sections = speedProfile.speedSections;

        for (int i = 0; i < sections.Count - 1; i++)
        {
            var current = sections[i];
            var next = sections[i + 1];

            if (distance >= current.startDistance && distance <= next.startDistance)
            {
                float t = Mathf.InverseLerp(current.startDistance, next.startDistance, distance);
                return Mathf.Lerp(current.speedMultiplier, next.speedMultiplier, t);
            }
        }

        return sections[^1].speedMultiplier;
    }
    
}
*/