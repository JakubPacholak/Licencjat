using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 1f;

    [Header("Data References")]
    [SerializeField] private BuildingData buildingData1;
    [SerializeField] private BuildingData buildingData2;
    [SerializeField] private BuildingData buildingData3;
    [SerializeField] private BuildingData buildingData4;

    [Header("System Prefabs")]
    [SerializeField] private BuildingPreview previewPrefab;
    [SerializeField] private Building buildingPrefab;
    [SerializeField] private BuildingGrid grid;

    [SerializeField] private bool useGrid = true;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;

    [Header("Stacking & Collision")]
    [SerializeField] private float stackOffset = 0.05f;
    [SerializeField] private float maxStackSearchHeight = 10f;
    [SerializeField] private LayerMask buildingLayer;

    [Header("Ground Restriction")]
    [SerializeField] private LayerMask placementLayer;

    [Header("Merge System")]
    [SerializeField] private List<MergeRecipe> mergeRecipes;
    [SerializeField] private MergeIndicator mergeIndicatorPrefab;
    [SerializeField] private float mergeCheckRadius = 1.5f;
    [SerializeField] private KeyCode mergeKey = KeyCode.M;
    [SerializeField] private LineRenderer rangeVisualizer;

    private BuildingPreview preview;
    private MergeIndicator currentIndicator;

    private bool isMovingBuilding = false;

    private BuildingData oldData;
    private float oldRotation;
    private Vector3 oldCenterPos;
    private List<Vector3> oldPositions;
    private Material oldVariant;

    private BuildingEQ inventory;
    private List<Building> undoStack = new List<Building>();
    private Dictionary<MergeRecipe, int> recipeUsageHistory = new Dictionary<MergeRecipe, int>();

    private Building potentialMergeTarget = null;
    private MergeRecipe activeRecipe = null;
    private bool hasMerged = false;

    public CreatureState creatureState;

    public int GetTotalMerges()
    {
        return recipeUsageHistory.Values.Sum();
    }

    private void Start()
    {
        inventory = FindObjectOfType<BuildingEQ>();
        if (inventory != null)
        {
            inventory.Initialize(new List<BuildingData> { buildingData1, buildingData2, buildingData3, buildingData4 });
        }

        if (mergeIndicatorPrefab != null)
        {
            currentIndicator = Instantiate(mergeIndicatorPrefab);
            currentIndicator.Hide();
        }

        if (rangeVisualizer != null)
        {
            rangeVisualizer.positionCount = 51;
            rangeVisualizer.useWorldSpace = true;
            rangeVisualizer.enabled = false;
        }
    }

    private void Update()
    {
        if (preview != null)
        {
            HandlePreviewLogic();
        }
        else
        {
            if (rangeVisualizer != null) rangeVisualizer.enabled = false;
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            if (preview != null) CancelCurrentPreview();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (preview != null)
            {
                TryPlaceBuilding();
            }
            else
            {
                TryPickUpBuilding();
            }
        }
    }

    private void HandlePreviewLogic()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        bool isValidPosition = !mousePos.Equals(Vector3.negativeInfinity);

        if (!isValidPosition)
        {
            preview.gameObject.SetActive(false);
            if (currentIndicator != null) currentIndicator.Hide();
            if (rangeVisualizer != null) rangeVisualizer.enabled = false;
            return;
        }

        if (!preview.gameObject.activeSelf) preview.gameObject.SetActive(true);

        HandlePreviewPosition(mousePos);
        HandleRotation();
        CheckForMergePossibility();
        DrawMergeRangeCircle();

        if (potentialMergeTarget != null)
        {
            preview.ChangeState(BuildingPreview.BuildingPreviewState.POSITIVE);
        }
    }

    private void DrawMergeRangeCircle()
    {
        if (rangeVisualizer == null || preview == null) return;

        if (potentialMergeTarget == null)
        {
            rangeVisualizer.enabled = false;
            return;
        }

        rangeVisualizer.enabled = true;
        float angle = 0f;
        float segmentAngle = 360f / 50f;

        for (int i = 0; i < 51; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * mergeCheckRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * mergeCheckRadius;

            Vector3 pos = preview.transform.position + new Vector3(x, 0.2f, z);
            rangeVisualizer.SetPosition(i, pos);

            angle += segmentAngle;
        }
    }

    private void TryPlaceBuilding()
    {
        if (potentialMergeTarget != null && activeRecipe != null)
        {
            PerformMerge();
            return;
        }

        if (preview.State == BuildingPreview.BuildingPreviewState.POSITIVE)
        {
            List<Vector3> positions = preview.BuildingModels.GetRotatedShapeUnitOffsets()
                .Select(o => preview.transform.position + o).ToList();
            PlaceBuilding(positions);
            if (creatureState != null) creatureState.ShowHappyEmoticon();
        }
        else if (isMovingBuilding)
        {
        }
    }

    private void TryPickUpBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, buildingLayer))
        {
            Building building = hit.collider.GetComponentInParent<Building>();

            if (building == null)
            {
                building = hit.collider.GetComponent<Building>();
            }

            if (building == null && hit.transform.parent != null)
            {
                building = hit.transform.parent.GetComponent<Building>();
            }

            if (building != null)
            {
                StartMovingBuilding(building);
            }
        }
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            preview.AddRotation(-rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            preview.AddRotation(rotationSpeed * Time.deltaTime);
        }
    }

    public void StartMovingBuilding(Building buildingToMove)
    {
        if (preview != null) return;

        oldPositions = buildingToMove.Data.Model.GetAllBuldingPosition();
        oldRotation = buildingToMove.Rotation;
        oldCenterPos = buildingToMove.transform.position;
        oldData = buildingToMove.Data;
        oldVariant = buildingToMove.CurrentVariant;

        if (useGrid)
        {
            List<BuildingGridCell> cellsToClear = new List<BuildingGridCell>();
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    var cell = grid.GetCell(x, y);
                    if (cell.GetBuilding() == buildingToMove) cellsToClear.Add(cell);
                }
            }
            foreach (var cell in cellsToClear) cell.Clear();
        }

        Destroy(buildingToMove.gameObject);

        Vector3 mousePos = GetMouseWorldPosition();
        if (mousePos.Equals(Vector3.negativeInfinity)) mousePos = oldCenterPos;

        preview = CreatePreview(oldData, mousePos, oldVariant);
        preview.SetRotation(oldRotation);

        isMovingBuilding = true;
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

        if (mergeRecipes == null) return;

        Collider[] hits = Physics.OverlapSphere(preview.transform.position, mergeCheckRadius, buildingLayer);

        foreach (var hit in hits)
        {
            Building nearbyBuilding = hit.GetComponentInParent<Building>();
            if (nearbyBuilding == null) continue;

            foreach (var recipe in mergeRecipes)
            {
                if (recipe == null) continue;

                if (recipe.MaxUses > 0)
                {
                    if (recipeUsageHistory.ContainsKey(recipe) && recipeUsageHistory[recipe] >= recipe.MaxUses) continue;
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
                currentIndicator.Show(centerPos, "!");
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
        Destroy(potentialMergeTarget.gameObject);
        Destroy(preview.gameObject);
        preview = null;
        if (currentIndicator != null) currentIndicator.Hide();
        if (rangeVisualizer != null) rangeVisualizer.enabled = false;

        Building newBuilding = Instantiate(buildingPrefab, mergePosition, Quaternion.identity);

        Material mergeVariant = null;
        if (activeRecipe.Result.ColorVariants != null && activeRecipe.Result.ColorVariants.Count > 0)
        {
            mergeVariant = activeRecipe.Result.ColorVariants[Random.Range(0, activeRecipe.Result.ColorVariants.Count)];
        }

        newBuilding.Setup(activeRecipe.Result, 0, mergeVariant);

        if (activeRecipe.Result.PlacementVFX != null)
        {
            Destroy(Instantiate(activeRecipe.Result.PlacementVFX, mergePosition, Quaternion.identity), 5f);
        }

        undoStack.Add(newBuilding);
        if (undoStack.Count > 3) undoStack.RemoveAt(0);

        if (recipeUsageHistory.ContainsKey(activeRecipe)) recipeUsageHistory[activeRecipe]++;
        else recipeUsageHistory.Add(activeRecipe, 1);

        isMovingBuilding = false;
        hasMerged = false;
    }

    public void CancelCurrentPreview()
    {
        if (isMovingBuilding)
        {
            Building restoredBuilding = Instantiate(buildingPrefab, oldCenterPos, Quaternion.identity);
            restoredBuilding.Setup(oldData, oldRotation, oldVariant);
            if (useGrid) grid.SetBuilding(restoredBuilding, oldPositions);
            isMovingBuilding = false;
        }

        if (preview != null)
        {
            Destroy(preview.gameObject);
            preview = null;
        }
        if (currentIndicator != null) currentIndicator.Hide();
        if (rangeVisualizer != null) rangeVisualizer.enabled = false;
    }

    private void HandlePreviewPosition(Vector3 mouseWorldPosition)
    {
        List<Vector3> rotatedOffsets = preview.BuildingModels.GetRotatedShapeUnitOffsets();
        List<Vector3> worldPositionsBasedOnMouse = rotatedOffsets.Select(offset => mouseWorldPosition + offset).ToList();
        Vector3 targetPosition = mouseWorldPosition;

        if (!useGrid && preview.Data.AllowStacking)
        {
            targetPosition = GetStackTopPosition(mouseWorldPosition);
        }

        bool canBuild = false;

        if (useGrid)
        {
            canBuild = grid.CanBuild(worldPositionsBasedOnMouse);
            if (canBuild)
            {
                Vector3 snappedCenterPosition = GetSnappedCenterPosition(worldPositionsBasedOnMouse);
                preview.transform.position = snappedCenterPosition;
            }
            else
            {
                preview.transform.position = mouseWorldPosition;
            }
        }
        else
        {
            canBuild = CheckCollisionWithoutGrid(worldPositionsBasedOnMouse);
            preview.transform.position = targetPosition;
        }

        if (canBuild && preview.Data.OnlyStackOnSameType)
        {
            if (!CheckIfStackingOnSameType(preview.transform.position))
            {
                canBuild = false;
            }
        }

        preview.ChangeState(canBuild ? BuildingPreview.BuildingPreviewState.POSITIVE : BuildingPreview.BuildingPreviewState.NEGATIVE);
    }

    private bool CheckIfStackingOnSameType(Vector3 currentPos)
    {
        Ray ray = new Ray(currentPos + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f, buildingLayer))
        {
            Building buildingBelow = hit.collider.GetComponentInParent<Building>();
            if (buildingBelow != null)
            {
                if (buildingBelow.Data == preview.Data) return true;
                return false;
            }
        }
        return true;
    }

    private void PlaceBuilding(List<Vector3> buildingPositions)
    {
        Building building = Instantiate(buildingPrefab, preview.transform.position, Quaternion.identity);
        building.Setup(preview.Data, preview.BuildingModels.Rotation, preview.ChosenVariant);

        if (useGrid) grid.SetBuilding(building, buildingPositions);

        if (preview.Data.PlacementVFX != null)
        {
            Destroy(Instantiate(preview.Data.PlacementVFX, preview.transform.position, Quaternion.identity), 5f);
        }

        undoStack.Add(building);
        if (undoStack.Count > 3) undoStack.RemoveAt(0);

        Destroy(preview.gameObject);
        preview = null;
        isMovingBuilding = false;

        if (currentIndicator != null) currentIndicator.Hide();
        if (rangeVisualizer != null) rangeVisualizer.enabled = false;
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

    private BuildingPreview CreatePreview(BuildingData data, Vector3 position, Material variant = null)
    {
        BuildingPreview buildingPreview = Instantiate(previewPrefab, position, Quaternion.identity);
        buildingPreview.Setup(data, variant);
        return buildingPreview;
    }

    public bool HasActivePreview() => preview != null;
    public void CancelPreview() => CancelCurrentPreview();

    public BuildingPreview CreatePreviewFromInventory(BuildingData data, Vector3 position)
    {
        if (preview != null) Destroy(preview.gameObject);

        if (position.Equals(Vector3.negativeInfinity))
        {
            position = Vector3.zero;
        }

        Material randomVariant = null;
        if (data.ColorVariants != null && data.ColorVariants.Count > 0)
        {
            randomVariant = data.ColorVariants[Random.Range(0, data.ColorVariants.Count)];
        }

        preview = CreatePreview(data, position, randomVariant);
        isMovingBuilding = false;
        return preview;
    }

    private bool CheckCollisionWithoutGrid(List<Vector3> worldPositions)
    {
        if (preview != null && preview.Data.IgnoreCollision) return true;

        if (preview != null && preview.Data.AllowStacking && preview.transform.position.y > 0.1f)
            return true;

        float halfCell = CellSize / 2f;
        foreach (var pos in worldPositions)
        {
            Collider[] hits = Physics.OverlapBox(pos, new Vector3(halfCell, halfCell, halfCell));
            foreach (var hit in hits)
            {
                if (hit.GetComponentInParent<Building>() != null) return false;
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
            Building buildingBelow = hit.collider.GetComponentInParent<Building>();

            if (buildingBelow != null)
            {
                if (!buildingBelow.Data.CanBeStackedOn)
                {
                    return mousePos;
                }
            }

            Renderer rend = hit.collider.GetComponentInChildren<Renderer>();
            if (rend != null) return new Vector3(mousePos.x, rend.bounds.max.y + stackOffset, mousePos.z);
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