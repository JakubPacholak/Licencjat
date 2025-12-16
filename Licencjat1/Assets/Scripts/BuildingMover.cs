using UnityEngine;

[RequireComponent(typeof(BuildingSystem))]
public class BuildingMover : MonoBehaviour
{
    [SerializeField] private BuildingGrid grid;

    private BuildingSystem buildingSystem;
    private Camera mainCamera;

    private void Awake()
    {
        buildingSystem = GetComponent<BuildingSystem>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !buildingSystem.HasActivePreview())
        {
            SelectBuildingToMove();
        }
    }

    private void SelectBuildingToMove()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            Building building = hit.collider.GetComponentInParent<Building>();

            if (building != null)
            {
                buildingSystem.StartMovingBuilding(building, grid);
            }
        }
    }
}