using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BuildingGrid : MonoBehaviour
{
    [SerializeField] private Renderer groundMeshRenderer;
    [SerializeField] private Terrain groundTerrain;

    private int width;
    private int height;
    private Vector3 gridOrigin;

    private BuildingGridCell[,] grid;
    public BuildingGridCell[,] GetGrid() => grid;
    public int GetLength(int dimension) => grid.GetLength(dimension);
    public BuildingGridCell GetCell(int x, int y) => grid[x, y];

    public void SetGroundMeshRenderer(Renderer ren) { groundMeshRenderer = ren; }

    private void Start()
    {
        bool foundGround = false;
        float surfaceY = 0f;

        if (groundTerrain != null)
        {
            Vector3 terrainSize = groundTerrain.terrainData.size;
            width = Mathf.FloorToInt(terrainSize.x / BuildingSystem.CellSize);
            height = Mathf.FloorToInt(terrainSize.z / BuildingSystem.CellSize);
            gridOrigin = groundTerrain.transform.position;
            surfaceY = gridOrigin.y;
            foundGround = true;
        }
        else if (groundMeshRenderer != null)
        {
            var bounds = groundMeshRenderer.bounds;
            width = Mathf.FloorToInt(bounds.size.x / BuildingSystem.CellSize);
            height = Mathf.FloorToInt(bounds.size.z / BuildingSystem.CellSize);
            surfaceY = bounds.max.y;
            gridOrigin = new Vector3(bounds.min.x, surfaceY, bounds.min.z);
            foundGround = true;
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                Terrain ter = hit.collider.GetComponent<Terrain>();
                Renderer ren = hit.collider.GetComponent<Renderer>();

                if (ren == null)
                {
                    ren = hit.collider.GetComponentInParent<Renderer>();
                }

                if (ter != null)
                {
                    groundTerrain = ter;
                    Vector3 terrainSize = groundTerrain.terrainData.size;
                    width = Mathf.FloorToInt(terrainSize.x / BuildingSystem.CellSize);
                    height = Mathf.FloorToInt(terrainSize.z / BuildingSystem.CellSize);
                    gridOrigin = groundTerrain.transform.position;
                    surfaceY = hit.point.y;
                    foundGround = true;
                }
                else if (ren != null)
                {
                    groundMeshRenderer = ren;
                    var bounds = groundMeshRenderer.bounds;
                    width = Mathf.FloorToInt(bounds.size.x / BuildingSystem.CellSize);
                    height = Mathf.FloorToInt(bounds.size.z / BuildingSystem.CellSize);
                    surfaceY = hit.point.y;
                    gridOrigin = new Vector3(bounds.min.x, surfaceY, bounds.min.z);
                    foundGround = true;
                }
            }
        }

        if (!foundGround)
        {
            Debug.LogError("No ground (Terrain or MeshRenderer) detected or assigned!");
            return;
        }

        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Invalid grid dimensions calculated from ground!");
            return;
        }

        grid = new BuildingGridCell[width, height];

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = new BuildingGridCell();
            }
        }
    }

    public void SetBuilding(Building building, List<Vector3> allBuildingPosition)
    {
        foreach (var p in allBuildingPosition)
        {
            (int x, int y) = WorldToGridPosition(p);
            grid[x, y].SetBuilding(building);
        }
    }

    public bool CanBuild(List<Vector3> allBuildingPositions)
    {
        foreach (var p in allBuildingPositions)
        {
            (int x, int y) = WorldToGridPosition(p);

            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            if (!grid[x, y].IsEmpty())
                return false;
        }

        return true;
    }

    private (int x, int y) WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / BuildingSystem.CellSize);
        int y = Mathf.FloorToInt((worldPosition.z - gridOrigin.z) / BuildingSystem.CellSize);
        return (x, y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (BuildingSystem.CellSize <= 0)
            return;

        Vector3 origin = Vector3.zero;
        float gizWidth = 0f;
        float gizHeight = 0f;
        bool foundGround = false;

        if (groundTerrain != null)
        {
            var ts = groundTerrain.terrainData.size;
            gizWidth = ts.x;
            gizHeight = ts.z;
            origin = groundTerrain.transform.position;
            origin.y += 0.01f;
            foundGround = true;
        }
        else if (groundMeshRenderer != null)
        {
            var b = groundMeshRenderer.bounds;
            gizWidth = b.size.x;
            gizHeight = b.size.z;
            origin = new Vector3(b.min.x, b.max.y + 0.01f, b.min.z);
            foundGround = true;
        }

        if (!foundGround)
            return;

        int w = Mathf.FloorToInt(gizWidth / BuildingSystem.CellSize);
        int h = Mathf.FloorToInt(gizHeight / BuildingSystem.CellSize);

        if (w <= 0 || h <= 0)
            return;

        for (int y = 0; y <= h; y++)
        {
            Vector3 start = origin + new Vector3(0, 0, y * BuildingSystem.CellSize);
            Vector3 end = origin + new Vector3(w * BuildingSystem.CellSize, 0, y * BuildingSystem.CellSize);
            Gizmos.DrawLine(start, end);
        }

        for (int x = 0; x <= w; x++)
        {
            Vector3 start = origin + new Vector3(x * BuildingSystem.CellSize, 0, 0);
            Vector3 end = origin + new Vector3(x * BuildingSystem.CellSize, 0, h * BuildingSystem.CellSize);
            Gizmos.DrawLine(start, end);
        }
    }
}

public class BuildingGridCell
{
    private Building building;
    public Building GetBuilding() => building;
    public void Clear() => building = null;

    public void SetBuilding(Building building)
    {
        this.building = building;
    }

    public bool IsEmpty()
    {
        return building == null;
    }
}