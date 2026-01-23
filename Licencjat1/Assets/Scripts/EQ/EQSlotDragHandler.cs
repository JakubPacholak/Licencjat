using UnityEngine;
using UnityEngine.EventSystems;

public class EQSlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private BuildingData data;
    private BuildingEQ parent;

    public void Setup(BuildingData buildingData, BuildingEQ parentScript)
    {
        data = buildingData;
        parent = parentScript;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parent != null) parent.BeginDrag(data);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parent != null) parent.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parent != null) parent.EndDrag(eventData);
    }
}