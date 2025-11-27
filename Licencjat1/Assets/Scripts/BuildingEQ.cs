using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    private GameObject draggingIcon;
    private BuildingData draggedData;
    private BuildingPreview currentPreview;

    private void Awake()
    {
        if (buildingSystem == null) buildingSystem = FindObjectOfType<BuildingSystem>();
        if (mainCanvas == null) mainCanvas = FindObjectOfType<Canvas>();
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

            Image iconImg = slot.GetComponentInChildren<Image>();
            if (iconImg != null)
                iconImg.sprite = data.Icon != null ? data.Icon : defaultIcon;

            var handler = slot.AddComponent<EQSlotDragHandler>();
            handler.Setup(data, this);

            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
            }
        }
    }

    public void BeginDrag(BuildingData data)
    {
        draggedData = data;

        draggingIcon = new GameObject("EQ_DraggingIcon");
        draggingIcon.transform.SetParent(mainCanvas.transform, false);
        Image img = draggingIcon.AddComponent<Image>();
        img.sprite = data.Icon != null ? data.Icon : defaultIcon;
        img.raycastTarget = false;
        draggingIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);

        Vector3 worldPos = buildingSystem.GetMouseWorldPosition();
        currentPreview = buildingSystem.CreatePreviewFromInventory(data, worldPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingIcon != null)
            draggingIcon.transform.position = eventData.position;

        if (currentPreview != null)
        {
            Vector3 worldPos = buildingSystem.GetMouseWorldPosition();
            buildingSystem.UpdatePreviewPosition(worldPos);
        }
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (draggingIcon != null)
            Destroy(draggingIcon);

        if (currentPreview != null && currentPreview.State == BuildingPreview.BuildingPreviewState.POSITIVE)
        {
            buildingSystem.PlaceCurrentPreview();
        }
        else
        {
            buildingSystem.CancelCurrentPreview();
        }

        currentPreview = null;
        draggedData = null;
    }

    private void StartPreview(BuildingData data)
    {
        Vector3 mousePos = buildingSystem.GetMouseWorldPosition();
        buildingSystem.CreatePreviewFromInventory(data, mousePos);
    }
}

public class EQSlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private BuildingData data;
    private BuildingEQ parent;

    public void Setup(BuildingData buildingData, BuildingEQ parentScript)
    {
        data = buildingData;
        parent = parentScript;
    }

    public void OnBeginDrag(PointerEventData eventData) => parent.BeginDrag(data);
    public void OnDrag(PointerEventData eventData) => parent.OnDrag(eventData);
    public void OnEndDrag(PointerEventData eventData) => parent.EndDrag(eventData);
}