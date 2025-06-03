using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarouselPlacer))]
public class CarouselPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CarouselPlacer placer = (CarouselPlacer)target;
        if (GUILayout.Button("Place Slots in Circle"))
        {
            placer.PlaceSlots();
        }
    }
}

