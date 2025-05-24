using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableMesh : MonoBehaviour, IPointerClickHandler
{
    public MeshClickCycler cycler;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cycler != null)
            cycler.CycleToNextMesh();
    }
}
