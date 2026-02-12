using UnityEngine;
using TMPro;
using System.Collections;

public enum TutorialPhase
{
    MoveCamera,
    ZoomCamera,
    OpenInventory,
    PlaceItem,
    RotateItem,
    MergeItems,
    FillHappiness
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI instructionText;
    public GameObject tutorialPanel;

    [Header("References")]
    public GameObject uiInventoryPanel;
    public BuildingSystem buildingSystem;
    public CreatureState creatureState;
    public Transform cameraTransform;

    [Header("Settings")]
    public float moveThreshold = 2.0f;

    private TutorialPhase currentPhase = TutorialPhase.MoveCamera;
    private Vector3 startCameraPos;
    private int startBuildingCount;
    private int startMergeCount;

    private bool isSystemReady = false;
    private bool isFinished = false;
    private float phaseCooldown = 0f;

    private void Start()
    {
        StartCoroutine(InitializeTutorialRoutine());
    }

    private IEnumerator InitializeTutorialRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            startCameraPos = cameraTransform.position;
        }

        startBuildingCount = CountBuildings();
        if (buildingSystem != null)
        {
            startMergeCount = buildingSystem.GetTotalMerges();
        }

        UpdateInstructionText();
        isSystemReady = true;
    }

    private void Update()
    {
        if (!isSystemReady || isFinished) return;

        if (phaseCooldown > 0f)
        {
            phaseCooldown -= Time.deltaTime;
            return;
        }

        CheckCurrentObjective();
    }

    private void CheckCurrentObjective()
    {
        switch (currentPhase)
        {
            case TutorialPhase.MoveCamera:
                if (cameraTransform == null) return;
                float dist = Vector3.Distance(startCameraPos, cameraTransform.position);

                if (dist > moveThreshold)
                {
                    NextPhase();
                }
                break;

            case TutorialPhase.ZoomCamera:
                float scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scrollInput) > 0.01f)
                {
                    NextPhase();
                }
                break;

            case TutorialPhase.OpenInventory:
                if (uiInventoryPanel != null && uiInventoryPanel.activeSelf && Input.GetMouseButtonDown(0))
                {
                    NextPhase();
                }
                break;

            case TutorialPhase.PlaceItem:
                int currentBuildings = CountBuildings();
                if (currentBuildings > startBuildingCount)
                {
                    startBuildingCount = currentBuildings;
                    NextPhase();
                }
                break;

            case TutorialPhase.RotateItem:
                if (buildingSystem != null && buildingSystem.HasActivePreview())
                {
                    if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
                    {
                        NextPhase();
                    }
                }
                break;

            case TutorialPhase.MergeItems:
                if (buildingSystem != null)
                {
                    if (buildingSystem.GetTotalMerges() > startMergeCount)
                    {
                        NextPhase();
                    }
                }
                break;

            case TutorialPhase.FillHappiness:
                if (creatureState != null)
                {
                    if (creatureState.image.fillAmount >= 0.95f)
                    {
                        NextPhase();
                    }
                }
                break;
        }
    }

    private void NextPhase()
    {
        if (currentPhase == TutorialPhase.FillHappiness)
        {
            if (!isFinished)
            {
                isFinished = true;
                StartCoroutine(FinishTutorialSequence());
            }
            return;
        }

        currentPhase++;
        phaseCooldown = 1.5f;

        if (currentPhase == TutorialPhase.MergeItems && buildingSystem != null)
        {
            startMergeCount = buildingSystem.GetTotalMerges();
        }

        if (cameraTransform != null)
        {
            startCameraPos = cameraTransform.position;
        }

        UpdateInstructionText();
    }

    private IEnumerator FinishTutorialSequence()
    {
        if (instructionText != null)
        {
            instructionText.text = "Tutorial Completed! Have fun creating your world!";
        }

        yield return new WaitForSeconds(6.0f);

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        else if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;

        switch (currentPhase)
        {
            case TutorialPhase.MoveCamera:
                instructionText.text = "You can use your mouse (RMB) to move camera around.";
                break;

            case TutorialPhase.ZoomCamera:
                instructionText.text = "Using mouse scroll will let you to zoom in and out.";
                break;

            case TutorialPhase.OpenInventory:
                instructionText.text = "Click on the creature to see what items it wants in the environment.";
                break;

            case TutorialPhase.PlaceItem:
                instructionText.text = "Now you can click on any item in the cloud and place it on the ground.";
                break;

            case TutorialPhase.RotateItem:
                instructionText.text = "Pick another object, but before you place it, pressing Q or E will let you rotate objects.";
                break;

            case TutorialPhase.MergeItems:
                instructionText.text = "Some things will fuse together into new variants, try finding out which ones!";
                break;

            case TutorialPhase.FillHappiness:
                instructionText.text = "Happiness bar in the bottom left corner lets you know how creature feels. Fill it up to finish!";
                break;
        }
    }

    private int CountBuildings()
    {
        return FindObjectsByType<Building>(FindObjectsSortMode.None).Length;
    }
}