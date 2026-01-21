using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 1f;

    [SerializeField] private BuildingData buildingData1;
    [SerializeField] private BuildingData buildingData2;
    [SerializeField] private BuildingData buildingData3;
    [SerializeField] private BuildingData buildingData4;
    [SerializeField] private BuildingData buildingData5;

    [SerializeField] private BuildingPreview previewPrefab;
    [SerializeField] private Building buildingPrefab;
    [SerializeField] private BuildingGrid grid;

    [SerializeField] private bool useGrid = true;

    [Header("Stacking settings (free placement)")]
    [SerializeField] private float stackOffset = 0.05f;
    [SerializeField] private float maxStackSearchHeight = 10f;
    [SerializeField] private LayerMask buildingLayer;

    [Header("Merge System")]
    [SerializeField] private List<MergeRecipe> mergeRecipes; // Lista recept (przypisz w Inspectorze)
    [SerializeField] private MergeIndicator mergeIndicatorPrefab; // Prefab z literk?
    [SerializeField] private float mergeCheckRadius = 1.5f;   // Odleg?o?? ??czenia
    [SerializeField] private KeyCode mergeKey = KeyCode.M;    // Klawisz do ??czenia

    private BuildingPreview preview;
    private MergeIndicator currentIndicator;
    private bool isMovingBuilding = false;

    private BuildingData oldData;
    private float oldRotation;
    private Vector3 oldCenterPos;
    private List<Vector3> oldPositions;

    private BuildingEQ inventory;
    private List<Building> undoStack = new List<Building>();

    // Zmienne do obs?ugi aktualnego celu mergowania
    private Building potentialMergeTarget = null;
    private MergeRecipe activeRecipe = null;

    private void Start()
    {
        inventory = FindObjectOfType<BuildingEQ>();
        if (inventory != null)
        {
            inventory.Initialize(new List<BuildingData> { buildingData1, buildingData2, buildingData3, buildingData4, buildingData5 });
        }

        // Instancjonujemy wska?nik raz i go ukrywamy
        if (mergeIndicatorPrefab != null)
        {
            currentIndicator = Instantiate(mergeIndicatorPrefab);
            currentIndicator.Hide();
        }
    }

    private void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition();

        if (preview != null)
        {
            HandlePreview(mousePos);

            // --- LOGIKA MERGOWANIA ---
            CheckForMergePossibility();

            // 1. Obs?uga klawisza MERGE (np. "M")
            if (potentialMergeTarget != null && Input.GetKeyDown(mergeKey))
            {
                PerformMerge();
                return; // Przerywamy, aby nie postawi? budynku
            }
            // -------------------------

            if (Input.GetKeyDown(KeyCode.R))
                preview.AddRotation(90);

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                CancelCurrentPreview();
                return;
            }

            // 2. Obs?uga MYSZKI (Standardowe stawianie)
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
            // (Je?li to nie przenoszenie, tylko stawianie z EQ)
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

    // --- NOWA METODA: Sprawdza otoczenie ---
    private void CheckForMergePossibility()
    {
        potentialMergeTarget = null;
        activeRecipe = null;

        if (preview == null) return;

        // Szukamy budynków w pobli?u preview
        Collider[] hits = Physics.OverlapSphere(preview.transform.position, mergeCheckRadius, buildingLayer);

        foreach (var hit in hits)
        {
            Building nearbyBuilding = hit.GetComponentInParent<Building>();

            // Ignorujemy null i ewentualnie obiekt, który w?a?nie przenosimy (je?li mia?by collider w??czony)
            if (nearbyBuilding == null) continue;

            // Sprawdzamy wszystkie recepty
            foreach (var recipe in mergeRecipes)
            {
                // Sprawdzenie A+B lub B+A
                bool matchA = (recipe.InputA == preview.Data && recipe.InputB == nearbyBuilding.Data);
                bool matchB = (recipe.InputB == preview.Data && recipe.InputA == nearbyBuilding.Data);

                if (matchA || matchB)
                {
                    potentialMergeTarget = nearbyBuilding;
                    activeRecipe = recipe;
                    break;
                }
            }

            if (potentialMergeTarget != null) break; // Znaleziono, przerywamy szukanie
        }

        // Obs?uga wizualna
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

    // --- NOWA METODA: Wykonuje po??czenie ---
    private void PerformMerge()
    {
        if (potentialMergeTarget == null || activeRecipe == null) return;

        // Pozycja po?rodku
        Vector3 mergePosition = (potentialMergeTarget.transform.position + preview.transform.position) / 2f;

        // Usu? stary budynek ze sceny
        if (useGrid)
        {
            // Opcjonalnie: Czyszczenie grida dla starego budynku, je?li grid jest u?ywany
            // grid.ClearBuilding(potentialMergeTarget); 
        }
        Destroy(potentialMergeTarget.gameObject);

        // Usu? preview
        Destroy(preview.gameObject);
        preview = null;

        if (currentIndicator != null) currentIndicator.Hide();

        // Stwórz wynik
        Building newBuilding = Instantiate(buildingPrefab, mergePosition, Quaternion.identity);
        newBuilding.Setup(activeRecipe.Result, 0);

        undoStack.Add(newBuilding);
        if (undoStack.Count > 3) undoStack.RemoveAt(0);

        isMovingBuilding = false;

        Debug.Log($"Po??czono budynki w: {activeRecipe.Result.name}");
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

    public Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
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
            // Ta metoda wydaje si? duplikowa? HandlePreview, ale zostawiam dla kompatybilno?ci
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