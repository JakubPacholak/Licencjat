using UnityEngine;

public class RandomMap : MonoBehaviour
{
    [Header("Ground Settings Ranges")]
    [SerializeField] private Vector2Int sizeRange = new Vector2Int(10, 25);

    [SerializeField] private Material groundMaterial;

    [Header("References")]
    [SerializeField] private BuildingGrid buildingGrid;

    private int groundSize;

    private void Start()
    {
        if (buildingGrid == null)
        {
            buildingGrid = FindObjectOfType<BuildingGrid>();
        }
        groundSize = Random.Range(sizeRange.x, sizeRange.y + 1);

        GenerateFlatCube();
        if (buildingGrid != null)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                buildingGrid.SetGroundMeshRenderer(renderer);
            }
        }
    }

    private void GenerateFlatCube()
    {
        gameObject.AddComponent<BoxCollider>();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (groundMaterial != null)
        {
            meshRenderer.material = groundMaterial;
        }
        else
        {
            Material defaultMat = new Material(Shader.Find("Standard"));
            defaultMat.color = new Color(0.5f, 0f, 0.5f);
            meshRenderer.material = defaultMat;
        }
        Mesh cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        meshFilter.mesh = cubeMesh;
        transform.localScale = new Vector3(groundSize, 0.1f, groundSize);
        transform.position = new Vector3(0, -0.05f, 0);
    }
}