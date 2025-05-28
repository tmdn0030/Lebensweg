using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTest : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Debug.Log("Rechte Maustaste wird gedrückt");
        }
    }
}