using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingEQ : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Canvas mainCanvas;

    [Header("Visuals")]
    [SerializeField] private Sprite defaultIcon;

    private List<BuildingData> buildings = new();

    private void Awake()
    {
        if (buildingSystem == null) buildingSystem = FindObjectOfType<BuildingSystem>();
        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();
    }

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            TutorialManager tutorial = FindObjectOfType<TutorialManager>();
            if (tutorial != null && tutorial.IsDialogueActive) return;

            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = inventoryPanel.gameObject.activeSelf;
            inventoryPanel.gameObject.SetActive(!isActive);
        }
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.gameObject.SetActive(false);
        }
    }

    public void OpenInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.gameObject.SetActive(true);
        }
    }

    public void Initialize(List<BuildingData> availableBuildings)
    {
        buildings = availableBuildings;
        CreateSlots();
    }

    private void CreateSlots()
    {
        foreach (Transform child in inventoryPanel) Destroy(child.gameObject);

        for (int i = 0; i < buildings.Count; i++)
        {
            BuildingData data = buildings[i];
            GameObject slot = Instantiate(slotPrefab, inventoryPanel);

            Image iconImg = slot.transform.Find("Icon").GetComponent<Image>();
            if (iconImg != null)
            {
                iconImg.sprite = data.Icon != null ? data.Icon : defaultIcon;
                iconImg.preserveAspect = true;
            }

            var handler = slot.AddComponent<EQSlotDragHandler>();
            handler.Setup(data, this);

            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
            }
        }
    }

    public void SelectBuilding(BuildingData data)
    {
        TutorialManager tutorial = FindObjectOfType<TutorialManager>();

        if (tutorial != null)
        {
            if (tutorial.IsDialogueActive) return;
            if (tutorial.IsBuildingBlocked) return;
            if (tutorial.IsStatueBlocked && data.name.ToLower().Contains("statue")) return;

            if (tutorial.IsFirstTaskActive)
            {
                int objectIndex = buildings.IndexOf(data);
                if (objectIndex == 2) return;
            }
        }

        if (buildingSystem != null && buildingSystem.IsLimitReached(data)) return;

        ToggleInventory();
        Vector3 worldPos = buildingSystem.GetMouseWorldPosition();
        buildingSystem.CreatePreviewFromInventory(data, worldPos);
    }
}