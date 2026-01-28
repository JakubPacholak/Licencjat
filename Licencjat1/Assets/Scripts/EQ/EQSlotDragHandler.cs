using UnityEngine;
using UnityEngine.EventSystems;

public class EQSlotDragHandler : MonoBehaviour, IPointerClickHandler
{
    private BuildingData data;
    private BuildingEQ parent;

    public void Setup(BuildingData buildingData, BuildingEQ parentScript)
    {
        data = buildingData;
        parent = parentScript;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (parent != null)
        {
            parent.SelectBuilding(data);
        }
    }
}