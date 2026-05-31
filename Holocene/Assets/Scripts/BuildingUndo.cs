using UnityEngine;

[RequireComponent(typeof(BuildingSystem))]
public class BuildingUndo : MonoBehaviour
{
    [SerializeField] private KeyCode undoKey = KeyCode.Q;
    [SerializeField] private BuildingGrid grid;

    private BuildingSystem buildingSystem;
    private const int MaxUndo = 4;

    private void Awake()
    {
        buildingSystem = GetComponent<BuildingSystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(undoKey))
        {
            buildingSystem.UndoLastBuilding();
        }
    }
}