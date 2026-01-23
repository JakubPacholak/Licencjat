using UnityEngine;
using UnityEngine.EventSystems;

public class CreatureShop : MonoBehaviour
{
    private BuildingEQ buildingEQ;

    private void Start()
    {
        buildingEQ = FindObjectOfType<BuildingEQ>();
    }
    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (buildingEQ != null)
        {
            buildingEQ.ToggleInventory();
            Debug.Log("Otwieram/Zamykam EQ!");
        }
    }
}