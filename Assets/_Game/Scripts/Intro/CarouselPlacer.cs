using UnityEngine;

[ExecuteInEditMode]
public class CarouselPlacer : MonoBehaviour
{
    public Transform[] slots;
    public float radius = 2f;
    public bool lookOutward = true;

    public void PlaceSlots()
    {
        if (slots == null || slots.Length == 0)
            return;

        int count = slots.Length;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            Vector3 localPos = new Vector3(x, 0, z);
            slots[i].localPosition = localPos;

            if (lookOutward)
            {
                slots[i].LookAt(transform.position);
                slots[i].Rotate(0, 180f, 0f); // Nach außen drehen
            }
        }
    }
}
