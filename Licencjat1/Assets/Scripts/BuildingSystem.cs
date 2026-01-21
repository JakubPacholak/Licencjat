using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 1f;

    [Header("Data References")]
    [SerializeField] private BuildingData buildingData1;
    [SerializeField] private BuildingData buildingData2;
    [SerializeField] private BuildingData buildingData3;
    [SerializeField] private BuildingData buildingData4;
    [SerializeField] private BuildingData buildingData5;

    [Header("System Prefabs")]
    [SerializeField] private BuildingPreview previewPrefab;
    [SerializeField] private Building buildingPrefab;
    [SerializeField] private BuildingGrid grid;

    [SerializeField] private bool useGrid = true;

    [Header("Stacking & Collision")]
    [SerializeField] private float stackOffset = 0.05f;
    [SerializeField] private float maxStackSearchHeight = 10f;
    [SerializeField] private LayerMask buildingLayer;

    [Header("Ground Restriction")]
    [Tooltip("Ustaw tutaj warstw? (Layer), któr? ma Twoja platforma/ziemia")]
    [SerializeField] private LayerMask placementLayer;

    [Header("Merge System")]
    [SerializeField] private List<MergeRecipe> mergeRecipes;
    [SerializeField] private MergeIndicator mergeIndicatorPrefab;
    [SerializeField] private float mergeCheckRadius = 1.5f;
    [SerializeField] private KeyCode mergeKey = KeyCode.M;

    private BuildingPreview preview;
    private MergeIndicator currentIndicator;
    private bool isMovingBuilding = false;

    private BuildingData oldData;
    private float oldRotation;
    private Vector3 oldCenterPos;
    private List<Vector3> oldPositions;

    private BuildingEQ inventory;
    private List<Building> undoStack = new List<Building>();
    private Dictionary<MergeRecipe, int> recipeUsageHistory = new Dictionary<MergeRecipe, int>();

    private Building potentialMergeTarget = null;
    private MergeRecipe activeRecipe = null;
    private bool hasMerged = false;

    private void Start()
    {
        inventory = FindObjectOfType<BuildingEQ>();
        if (inventory != null)
        {
            inventory.Initialize(new List<BuildingData> { buildingData1, buildingData2, buildingData3, buildingData4, buildingData5 });
        }

        if (mergeIndicatorPrefab != null)
        {
            currentIndicator = Instantiate(mergeIndicatorPrefab);
            currentIndicator.Hide();
        }
    }

    private void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition();

        bool isValidPosition = !mousePos.Equals(Vector3.negativeInfinity);

        if (preview != null)
        {
            if (!isValidPosition)
            {
                preview.gameObject.SetActive(false);
                if (currentIndicator != null) currentIndicator.Hide();
                return;
            }

            if (!preview.gameObject.activeSelf)
            {
                preview.gameObject.SetActive(true);
            }

            HandlePreview(mousePos);
            CheckForMergePossibility();

            if (potentialMergeTarget != null && Input.GetKeyDown(mergeKey))
            {
                PerformMerge();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
                preview.AddRotation(90);

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                CancelCurrentPreview();
                return;
            }

            if (isMovingBuilding)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (preview.State == BuildingPreview.BuildingPreviewState.POSITIVE)
                    {
                        List<Vector3> positions = preview.BuildingModels.GetRotatedShapeUnitOffsets()
                            .Select(o => preview.transform.position + o).ToList();
                        PlaceBuilding(positions);
                    }
                    else
                    {
                        CancelCurrentPreview();
                    }
                }
                return;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (preview.State == BuildingPreview.BuildingPreviewState.POSITIVE)
                {
                    List<Vector3> positions = preview.BuildingModels.GetRotatedShapeUnitOffsets()
                        .Select(o => preview.transform.position + o).ToList();
                    PlaceBuilding(positions);
                }
            }
        }
    }

    public Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, placementLayer))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < 45f)
            {
                return hit.point;
            }
        }

        return Vector3.negativeInfinity;
    }

    private void CheckForMergePossibility()
    {
        if (hasMerged)
        {
            potentialMergeTarget = null;
            if (currentIndicator != null) currentIndicator.Hide();
            return;
        }

        potentialMergeTarget = null;
        activeRecipe = null;

        if (preview == null) return;

        Collider[] hits = Physics.OverlapSphere(preview.transform.position, mergeCheckRadius, buildingLayer);

        foreach (var hit in hits)
        {
            Building nearbyBuilding = hit.GetComponentInParent<Building>();

            if (nearbyBuilding == null) continue;

            foreach (var recipe in mergeRecipes)
            {
                if (recipe.MaxUses > 0)
                {
                    if (recipeUsageHistory.ContainsKey(recipe) && recipeUsageHistory[recipe] >= recipe.MaxUses)
                    {
                        continue;
                    }
                }

                bool matchA = (recipe.InputA == preview.Data && recipe.InputB == nearbyBuilding.Data);
                bool matchB = (recipe.InputB == preview.Data && recipe.InputA == nearbyBuilding.Data);

                if (matchA || matchB)
                {
                    potentialMergeTarget = nearbyBuilding;
                    activeRecipe = recipe;
                    break;
                }
            }

            if (potentialMergeTarget != null) break;
        }

        if (currentIndicator != null)
        {
            if (potentialMergeTarget != null)
            {
                Vector3 centerPos = (preview.transform.position + potentialMergeTarget.transform.position) / 2f;
                currentIndicator.Show(centerPos, mergeKey.ToString());
            }
            else
            {
                currentIndicator.Hide();
            }
        }
    }

    private void PerformMerge()
    {
        if (potentialMergeTarget == null || activeRecipe == null) return;

        Vector3 mergePosition = (potentialMergeTarget.transform.position + preview.transform.position) / 2f;

        if (useGrid)
        {
        }
        Destroy(potentialMergeTarget.gameObject);

        Destroy(preview.gameObject);
        preview = null;

        if (currentIndicator != null) currentIndicator.Hide();

        Building newBuilding = Instantiate(buildingPrefab, mergePosition, Quaternion.identity);
        newBuilding.Setup(activeRecipe.Result, 0);

        undoStack.Add(newBuilding);
        if (undoStack.Count > 3) undoStack.RemoveAt(0);

        isMovingBuilding = false;

        Debug.Log($"Po??czono budynki w: {activeRecipe.Result.name}");

        if (recipeUsageHistory.ContainsKey(activeRecipe))
        {
            recipeUsageHistory[activeRecipe]++;
        }
        else
        {
            recipeUsageHistory.Add(activeRecipe, 1);
        }

        isMovingBuilding = false;
    }

    public void CancelCurrentPreview()
    {
        if (isMovingBuilding)
        {
            Building restoredBuilding = Instantiate(buildingPrefab, oldCenterPos, Quaternion.identity);
            restoredBuilding.Setup(oldData, oldRotation);
            if (useGrid)
            {
                grid.SetBuilding(restoredBuilding, oldPositions);
            }
            isMovingBuilding = false;
        }

        if (preview != null)
        {
            Destroy(preview.gameObject);
            preview = null;
        }

        if (currentIndicator != null) currentIndicator.Hide();
    }

    private void HandlePreview(Vector3 mouseWorldPosition)
    {
        List<Vector3> rotatedOffsets = preview.BuildingModels.GetRotatedShapeUnitOffsets();
        List<Vector3> worldPositionsBasedOnMouse = rotatedOffsets.Select(offset => mouseWorldPosition + offset).ToList();

        Vector3 targetPosition = mouseWorldPosition;

        if (!useGrid && preview.Data.AllowStacking)
        {
            targetPosition = GetStackTopPosition(mouseWorldPosition);
        }

        if (useGrid)
        {
            bool canBuild = grid.CanBuild(worldPositionsBasedOnMouse);
            if (canBuild)
            {
                Vector3 snappedCenterPosition = GetSnappedCenterPosition(worldPositionsBasedOnMouse);
                preview.transform.position = snappedCenterPosition;
                preview.ChangeState(BuildingPreview.BuildingPreviewState.POSITIVE);
            }
            else
            {
                preview.transform.position = mouseWorldPosition;
                preview.ChangeState(BuildingPreview.BuildingPreviewState.NEGATIVE);
            }
        }
        else
        {
            bool canBuild = CheckCollisionWithoutGrid(worldPositionsBasedOnMouse);
            preview.transform.position = targetPosition;
            preview.ChangeState(canBuild ? BuildingPreview.BuildingPreviewState.POSITIVE : BuildingPreview.BuildingPreviewState.NEGATIVE);
        }
    }

    private void PlaceBuilding(List<Vector3> buildingPositions)
    {
        Building building = Instantiate(buildingPrefab, preview.transform.position, Quaternion.identity);
        building.Setup(preview.Data, preview.BuildingModels.Rotation);

        if (useGrid)
        {
            grid.SetBuilding(building, buildingPositions);
        }

        undoStack.Add(building);
        if (undoStack.Count > 3)
        {
            undoStack.RemoveAt(0);
        }

        Destroy(preview.gameObject);
        preview = null;
        isMovingBuilding = false;

        if (currentIndicator != null) currentIndicator.Hide();
    }

    private Vector3 GetSnappedCenterPosition(List<Vector3> allBuildingPosition)
    {
        List<int> xs = allBuildingPosition.Select(p => Mathf.FloorToInt(p.x)).ToList();
        List<int> zs = allBuildingPosition.Select(p => Mathf.FloorToInt(p.z)).ToList();

        int minX = xs.Min();
        int maxX = xs.Max();
        float centerX = minX + (maxX - minX) / 2f + CellSize / 2f;

        int minZ = zs.Min();
        int maxZ = zs.Max();
        float centerZ = minZ + (maxZ - minZ) / 2f + CellSize / 2f;

        return new Vector3(centerX, grid.transform.position.y, centerZ);
    }

    private BuildingPreview CreatePreview(BuildingData data, Vector3 position)
    {
        BuildingPreview buildingPreview = Instantiate(previewPrefab, position, Quaternion.identity);
        buildingPreview.Setup(data);
        isMovingBuilding = false;
        return buildingPreview;
    }

    public bool HasActivePreview() => preview != null;

    public void CancelPreview()
    {
        CancelCurrentPreview();
    }

    public void StartMovingBuilding(Building buildingToMove, BuildingGrid grid)
    {
        if (preview != null) return;

        oldPositions = buildingToMove.Data.Model.GetAllBuldingPosition();
        oldRotation = buildingToMove.Rotation;
        oldCenterPos = buildingToMove.transform.position;
        oldData = buildingToMove.Data;

        if (useGrid)
        {
            List<BuildingGridCell> cellsToClear = new List<BuildingGridCell>();
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    var cell = grid.GetCell(x, y);
                    if (cell.GetBuilding() == buildingToMove)
                    {
                        cellsToClear.Add(cell);
                    }
                }
            }
            foreach (var cell in cellsToClear)
            {
                cell.Clear();
            }
        }

        Destroy(buildingToMove.gameObject);

        Vector3 mousePos = GetMouseWorldPosition();
        if (mousePos.Equals(Vector3.negativeInfinity))
        {
            mousePos = oldCenterPos;
        }

        preview = CreatePreview(oldData, mousePos);
        preview.SetRotation(oldRotation);

        isMovingBuilding = true;
    }

    public BuildingPreview CreatePreviewFromInventory(BuildingData data, Vector3 position)
    {
        if (preview != null) Destroy(preview.gameObject);
        preview = CreatePreview(data, position);
        return preview;
    }

    public void UpdatePreviewPosition(Vector3 worldPosition)
    {
        if (preview != null)
        {
            HandlePreview(worldPosition);
        }
    }

    public void PlaceCurrentPreview()
    {
        if (preview != null && preview.State == BuildingPreview.BuildingPreviewState.POSITIVE)
        {
            List<Vector3> offsets = preview.BuildingModels.GetRotatedShapeUnitOffsets();
            List<Vector3> positions = offsets.Select(o => preview.transform.position + o).ToList();
            PlaceBuilding(positions);
        }
    }

    private bool CheckCollisionWithoutGrid(List<Vector3> worldPositions)
    {
        if (preview != null && preview.Data.AllowStacking)
        {
            return true;
        }

        float halfCell = CellSize / 2f;
        foreach (var pos in worldPositions)
        {
            Collider[] hits = Physics.OverlapBox(pos, new Vector3(halfCell, halfCell, halfCell));
            foreach (var hit in hits)
            {
                if (hit.GetComponentInParent<Building>() != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private Vector3 GetStackTopPosition(Vector3 mousePos)
    {
        Vector3 rayOrigin = mousePos + Vector3.up * maxStackSearchHeight;
        Ray ray = new Ray(rayOrigin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, maxStackSearchHeight * 2f, buildingLayer))
        {
            Renderer rend = hit.collider.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                return new Vector3(mousePos.x, rend.bounds.max.y + stackOffset, mousePos.z);
            }
            return new Vector3(mousePos.x, hit.point.y + stackOffset, mousePos.z);
        }
        return mousePos;
    }

    public void UndoLastBuilding()
    {
        if (undoStack.Count > 0)
        {
            Building last = undoStack[undoStack.Count - 1];
            Destroy(last.gameObject);
            undoStack.RemoveAt(undoStack.Count - 1);
        }
    }
}