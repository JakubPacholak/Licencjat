using UnityEngine;

public class CreatureShop : MonoBehaviour
{
    public CreatureState creatureState;
    private BuildingEQ buildingEQ;

    private void Start()
    {
        buildingEQ = FindObjectOfType<BuildingEQ>();
    }
}