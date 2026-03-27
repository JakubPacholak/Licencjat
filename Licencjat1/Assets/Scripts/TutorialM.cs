using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

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
            instructionText.text = "<color=#55FF55><b>Tutorial Completed!</b></color>\nHave fun creating your world!";
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

        string[] taskNames = new string[]
        {
            "Move camera (RMB)",
            "Zoom camera (Scroll)",
            "Open inventory",
            "Place an item",
            "Rotate an item (Q/E)",
            "Merge two items",
            "Fill happiness bar"
        };

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>TUTORIAL TASKS:</b>\n");

        for (int i = 0; i < taskNames.Length; i++)
        {
            if ((int)currentPhase > i)
            {
                sb.AppendLine($"<color=#888888><s>[X] {taskNames[i]}</s></color>");
            }
            else if ((int)currentPhase == i)
            {
                sb.AppendLine($"<b><color=#FFFFFF>[ ] {taskNames[i]}</color></b>");
            }
            else
            {
                sb.AppendLine($"<color=#AAAAAA>[ ] {taskNames[i]}</color>");
            }
        }

        sb.AppendLine("\n<b>TIP:</b>");
        switch (currentPhase)
        {
            case TutorialPhase.MoveCamera:
                sb.Append("<color=#DDDDDD><size=80%>Use your mouse (RMB) to move camera around.</size></color>");
                break;
            case TutorialPhase.ZoomCamera:
                sb.Append("<color=#DDDDDD><size=80%>Using mouse scroll will let you zoom in and out.</size></color>");
                break;
            case TutorialPhase.OpenInventory:
                sb.Append("<color=#DDDDDD><size=80%>Click on the creature to see what items it wants.</size></color>");
                break;
            case TutorialPhase.PlaceItem:
                sb.Append("<color=#DDDDDD><size=80%>Click on any item in the inventory and place it.</size></color>");
                break;
            case TutorialPhase.RotateItem:
                sb.Append("<color=#DDDDDD><size=80%>Pick an object, but before placing, press Q or E.</size></color>");
                break;
            case TutorialPhase.MergeItems:
                sb.Append("<color=#DDDDDD><size=80%>Some things will fuse together into new variants!</size></color>");
                break;
            case TutorialPhase.FillHappiness:
                sb.Append("<color=#DDDDDD><size=80%>Happiness bar lets you know how creature feels.</size></color>");
                break;
        }

        instructionText.text = sb.ToString();
    }

    private int CountBuildings()
    {
        return FindObjectsByType<Building>(FindObjectsSortMode.None).Length;
    }
}