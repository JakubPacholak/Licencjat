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
        Level2QuestManager lvl2Manager = FindObjectOfType<Level2QuestManager>();
        if (lvl2Manager != null && lvl2Manager.IsSatelliteBlocked)
        {
            if (transform.GetSiblingIndex() == 3)
            {
                Debug.Log("Czwarty przycisk (Satelita) jest zablokowany!");
                return;
            }
        }

        TutorialManager lvl1Manager = FindObjectOfType<TutorialManager>();
        if (lvl1Manager != null && lvl1Manager.IsStatueBlocked)
        {
            if (data != null && data.name == "Statue")
            {
                return;
            }
        }

        if (parent != null)
        {
            parent.SelectBuilding(data);
        }
    }
}