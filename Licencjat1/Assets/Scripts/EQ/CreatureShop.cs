using UnityEngine;
using UnityEngine.EventSystems;

public class CreatureShop : MonoBehaviour
{

    public CreatureState creatureState;
    private BuildingEQ buildingEQ;

    private void Start()
    {
        buildingEQ = FindObjectOfType<BuildingEQ>();
    }
    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (creatureState.currentState == CreatureState.State.Thinking)
        {
            buildingEQ.OpenInventory();
        }


        /*if (buildingEQ != null)
        {
            buildingEQ.ToggleInventory();

            Debug.Log("Otwieram/Zamykam EQ!");
        }*/
    }


}